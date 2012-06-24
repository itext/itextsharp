using System;
using Org.BouncyCastle.X509;
using System.Collections.Generic;
using System.IO;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
/*
 * $Id: MakeSignature.java 5199 2012-06-18 20:14:38Z psoares33 $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
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

    /**
     * Class that signs your PDF.
     * @author Paulo Soares
     */
    public static class MakeSignature {
	    /** Parameter to indicate that you want to sign using the Cryptographic Message Syntax. */
        public const bool CMS = false;
	    /** Parameter to indicate that you want to sign using CMS Advanced Electronic Signatures. */
        public const bool CADES = true;
        
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
                ITSAClient tsaClient, int estimatedSize, bool cades) {
            List<X509Certificate> certa = new List<X509Certificate>(chain);
            ICollection<byte[]> crlBytes = ProcessCrl(certa[0], crlList);
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
            PdfSignature dic = new PdfSignature(PdfName.ADOBE_PPKLITE, cades ? PdfName.ETSI_CADES_DETACHED : PdfName.ADBE_PKCS7_DETACHED);
            dic.Reason = sap.Reason;
            dic.Location = sap.Location;
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
            byte[] buf = new byte[8192];
            int n;
            while ((n = data.Read(buf, 0, buf.Length)) > 0) {
                messageDigest.BlockUpdate(buf, 0, n);
            }
            byte[] hash = new byte[messageDigest.GetDigestSize()];
            messageDigest.DoFinal(hash, 0);
            DateTime cal = DateTime.Now;
            byte[] ocsp = null;
            if (chain.Count >= 2 && ocspClient != null) {
                ocsp = ocspClient.GetEncoded(certa[0], certa[1], null);
            }
            byte[] sh = sgn.getAuthenticatedAttributeBytes(hash, cal, ocsp, crlBytes, cades);
            byte[] extSignature = externalSignature.Sign(sh);
            sgn.SetExternalDigest(extSignature, null, externalSignature.GetEncryptionAlgorithm());

            byte[] encodedSig = sgn.GetEncodedPKCS7(hash, cal, tsaClient, ocsp, crlBytes, cades);

            if (estimatedSize + 2 < encodedSig.Length)
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
                crlBytes.AddRange(b);
            }
            if (crlBytes.Count == 0)
                return null;
            else
                return crlBytes;
        }
    }
}
