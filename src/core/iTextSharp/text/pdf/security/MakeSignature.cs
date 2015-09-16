using System;
using Org.BouncyCastle.X509;
using System.Collections.Generic;
using System.IO;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using iTextSharp.text.log;
using iTextSharp.text.io;
/*
 * $Id: MakeSignature.java 5199 2012-06-18 20:14:38Z psoares33 $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */
namespace iTextSharp.text.pdf.security {

    public enum CryptoStandard {
        CMS, CADES
    }

    /**
     * Class that signs your PDF.
     * @author Paulo Soares
     */
    public static class MakeSignature {
        /** The Logger instance. */
        private static readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof(MakeSignature));
        
        /**
         * Signs the document using the detached mode, CMS or CAdES equivalent.
         * @param sap the PdfSignatureAppearance
         * @param externalSignature the interface providing the actual signing
         * @param chain the certificate chain
         * @param crlList the CRL list
         * @param ocspClient the OCSP client
         * @param tsaClient the Timestamp client
         * @param provider the provider or null
         * @param estimatedSize the reserved size for the signature. It will be estimated if 0
         * @param cades true to sign CAdES equivalent PAdES-BES, false to sign CMS
         * @throws DocumentException 
         * @throws IOException 
         * @throws GeneralSecurityException 
         * @throws NoSuchAlgorithmException 
         * @throws Exception 
         */
        public static void SignDetached(PdfSignatureAppearance sap, IExternalSignature externalSignature, ICollection<X509Certificate> chain, ICollection<ICrlClient> crlList, IOcspClient ocspClient,
                ITSAClient tsaClient, int estimatedSize, CryptoStandard sigtype) {
            List<X509Certificate> certa = new List<X509Certificate>(chain);
            ICollection<byte[]> crlBytes = null;
            int i = 0;
            while (crlBytes == null && i < certa.Count)
        	    crlBytes = ProcessCrl(certa[i++], crlList);
            if (estimatedSize == 0) {
                estimatedSize = 8192;
                if (crlBytes != null) {
                    foreach (byte[] element in crlBytes) {
                        estimatedSize += element.Length + 10;
                    }
                }
                if (ocspClient != null)
                    estimatedSize += 4192;
                if (tsaClient != null)
                    estimatedSize += 4192;
            }
            sap.Certificate = certa[0];
            if(sigtype == CryptoStandard.CADES)
                sap.AddDeveloperExtension(PdfDeveloperExtension.ESIC_1_7_EXTENSIONLEVEL2);
            PdfSignature dic = new PdfSignature(PdfName.ADOBE_PPKLITE, sigtype == CryptoStandard.CADES ? PdfName.ETSI_CADES_DETACHED : PdfName.ADBE_PKCS7_DETACHED);
            dic.Reason = sap.Reason;
            dic.Location = sap.Location;
            dic.SignatureCreator = sap.SignatureCreator;
            dic.Contact = sap.Contact;
            dic.Date = new PdfDate(sap.SignDate); // time-stamp will over-rule this
            sap.CryptoDictionary = dic;

            Dictionary<PdfName, int> exc = new Dictionary<PdfName, int>();
            exc[PdfName.CONTENTS] = estimatedSize * 2 + 2;
            sap.PreClose(exc);

            String hashAlgorithm = externalSignature.GetHashAlgorithm();
            PdfPKCS7 sgn = new PdfPKCS7(null, chain, hashAlgorithm, false);
            IDigest messageDigest = DigestUtilities.GetDigest(hashAlgorithm);
            Stream data = sap.GetRangeStream();
            byte[] hash = DigestAlgorithms.Digest(data, hashAlgorithm);
            byte[] ocsp = null;
            if (chain.Count >= 2 && ocspClient != null) {
                ocsp = ocspClient.GetEncoded(certa[0], certa[1], null);
            }
            byte[] sh = sgn.getAuthenticatedAttributeBytes(hash, ocsp, crlBytes, sigtype);
            byte[] extSignature = externalSignature.Sign(sh);
            sgn.SetExternalDigest(extSignature, null, externalSignature.GetEncryptionAlgorithm());

            byte[] encodedSig = sgn.GetEncodedPKCS7(hash, tsaClient, ocsp, crlBytes, sigtype);

            if (estimatedSize < encodedSig.Length)
                throw new IOException("Not enough space");

            byte[] paddedSig = new byte[estimatedSize];
            System.Array.Copy(encodedSig, 0, paddedSig, 0, encodedSig.Length);

            PdfDictionary dic2 = new PdfDictionary();
            dic2.Put(PdfName.CONTENTS, new PdfString(paddedSig).SetHexWriting(true));
            sap.Close(dic2);
        }
        
        /**
         * Processes a CRL list.
         * @param cert	a Certificate if one of the CrlList implementations needs to retrieve the CRL URL from it.
         * @param crlList	a list of CrlClient implementations
         * @return	a collection of CRL bytes that can be embedded in a PDF.
         */
        public static ICollection<byte[]> ProcessCrl(X509Certificate cert, ICollection<ICrlClient> crlList) {
            if (crlList == null)
                return null;
            List<byte[]> crlBytes = new List<byte[]>();
            foreach (ICrlClient cc in crlList) {
                if (cc == null)
                    continue;
                ICollection<byte[]> b = cc.GetEncoded(cert, null);
                if (b == null)
                    continue;
                LOGGER.Info("Processing " + cc.GetType().Name);
                crlBytes.AddRange(b);
            }
            if (crlBytes.Count == 0)
                return null;
            else
                return crlBytes;
        }

        /**
         * Sign the document using an external container, usually a PKCS7. The signature is fully composed
         * externally, iText will just put the container inside the document.
         * @param sap the PdfSignatureAppearance
         * @param externalSignatureContainer the interface providing the actual signing
         * @param estimatedSize the reserved size for the signature
         * @throws GeneralSecurityException
         * @throws IOException
         * @throws DocumentException 
         */
        public static void SignExternalContainer(PdfSignatureAppearance sap, IExternalSignatureContainer externalSignatureContainer, int estimatedSize) {
            PdfSignature dic = new PdfSignature(null, null);
            dic.Reason = sap.Reason;
            dic.Location = sap.Location;
            dic.SignatureCreator = sap.SignatureCreator;
            dic.Contact = sap.Contact;
            dic.Date = new PdfDate(sap.SignDate); // time-stamp will over-rule this
            externalSignatureContainer.ModifySigningDictionary(dic);
            sap.CryptoDictionary = dic;

            Dictionary<PdfName, int> exc = new Dictionary<PdfName, int>();
            exc[PdfName.CONTENTS] = estimatedSize * 2 + 2;
            sap.PreClose(exc);

            Stream data = sap.GetRangeStream();
            byte[] encodedSig = externalSignatureContainer.Sign(data);

            if (estimatedSize < encodedSig.Length)
                throw new IOException("Not enough space");

            byte[] paddedSig = new byte[estimatedSize];
            System.Array.Copy(encodedSig, 0, paddedSig, 0, encodedSig.Length);

            PdfDictionary dic2 = new PdfDictionary();
            dic2.Put(PdfName.CONTENTS, new PdfString(paddedSig).SetHexWriting(true));
            sap.Close(dic2);
        }
        
        /**
         * Signs a PDF where space was already reserved.
         * @param reader the original PDF
         * @param fieldName the field to sign. It must be the last field
         * @param outs the output PDF
         * @param externalSignatureContainer the signature container doing the actual signing. Only the 
         * method ExternalSignatureContainer.sign is used
         * @throws DocumentException
         * @throws IOException
         * @throws GeneralSecurityException 
         */
        public static void SignDeferred(PdfReader reader, String fieldName, Stream outs, IExternalSignatureContainer externalSignatureContainer) {
            AcroFields af = reader.AcroFields;
            PdfDictionary v = af.GetSignatureDictionary(fieldName);
            if (v == null)
                throw new DocumentException("No field");
            if (!af.SignatureCoversWholeDocument(fieldName))
                throw new DocumentException("Not the last signature");
            PdfArray b = v.GetAsArray(PdfName.BYTERANGE);
            long[] gaps = b.AsLongArray();
            if (b.Size != 4 || gaps[0] != 0)
                throw new DocumentException("Single exclusion space supported");
            IRandomAccessSource readerSource = reader.SafeFile.CreateSourceView();
            Stream rg = new RASInputStream(new RandomAccessSourceFactory().CreateRanged(readerSource, gaps));
            byte[] signedContent = externalSignatureContainer.Sign(rg);
            int spaceAvailable = (int)(gaps[2] - gaps[1]) - 2;
            if ((spaceAvailable & 1) != 0)
                throw new DocumentException("Gap is not a multiple of 2");
            spaceAvailable /= 2;
            if (spaceAvailable < signedContent.Length)
                throw new DocumentException("Not enough space");
            StreamUtil.CopyBytes(readerSource, 0, gaps[1] + 1, outs);
            ByteBuffer bb = new ByteBuffer(spaceAvailable * 2);
            foreach (byte bi in signedContent) {
                bb.AppendHex(bi);
            }
            int remain = (spaceAvailable - signedContent.Length) * 2;
            for (int k = 0; k < remain; ++k) {
                bb.Append((byte)48);
            }
            bb.WriteTo(outs);
            StreamUtil.CopyBytes(readerSource, gaps[2] - 1, gaps[3] + 1, outs);
        }
    }
}
