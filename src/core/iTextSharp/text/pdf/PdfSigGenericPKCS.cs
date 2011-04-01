using System;
using System.IO;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
/*
 * This file is part of the iText project.
 * Copyright (c) 1998-2009 1T3XT BVBA
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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
namespace iTextSharp.text.pdf {

    /**
    * A signature dictionary representation for the standard filters.
    */
    public abstract class PdfSigGenericPKCS : PdfSignature {
        /**
        * The hash algorith, for example "SHA1"
        */    
        protected String hashAlgorithm;
        /**
        * The class instance that calculates the PKCS#1 and PKCS#7
        */    
        protected PdfPKCS7 pkcs;
        /**
        * The subject name in the signing certificate (the element "CN")
        */    
        protected String   name;

        private byte[] externalDigest;
        private byte[] externalRSAdata;
        private String digestEncryptionAlgorithm;

        /**
        * Creates a generic standard filter.
        * @param filter the filter name
        * @param subFilter the sub-filter name
        */    
        public PdfSigGenericPKCS(PdfName filter, PdfName subFilter) : base(filter, subFilter) {
        }

        /**
        * Sets the crypto information to sign.
        * @param privKey the private key
        * @param certChain the certificate chain
        * @param crlList the certificate revocation list. It can be <CODE>null</CODE>
        */    
        public void SetSignInfo(ICipherParameters privKey, X509Certificate[] certChain, object[] crlList) {
            pkcs = new PdfPKCS7(privKey, certChain, crlList, hashAlgorithm, PdfName.ADBE_PKCS7_SHA1.Equals(Get(PdfName.SUBFILTER)));
            pkcs.SetExternalDigest(externalDigest, externalRSAdata, digestEncryptionAlgorithm);
            if (PdfName.ADBE_X509_RSA_SHA1.Equals(Get(PdfName.SUBFILTER))) {
                if (certChain.length > 1) {
                    PdfArray arr = new PdfArray();
                    for (int ii = 0; ii < certChain.Length; ii++) {
                        arr.add(new PdfString(certChain[ii].GetEncoded()));
                    }
                    Put(PdfName.CERT, arr);
                }
                else {
                    MemoryStream bout = new MemoryStream();
                    for (int k = 0; k < certChain.Length; ++k) {
                        byte[] tmp = certChain[k].GetEncoded();
                        bout.Write(tmp, 0, tmp.Length);
                    }
                    bout.Close();
                    Cert = bout.ToArray();
                }
                Contents = pkcs.GetEncodedPKCS1();
            }
            else
                Contents = pkcs.GetEncodedPKCS7();
            name = PdfPKCS7.GetSubjectFields(pkcs.SigningCertificate).GetField("CN");
            if (name != null)
                Put(PdfName.NAME, new PdfString(name, PdfObject.TEXT_UNICODE));
            pkcs = new PdfPKCS7(privKey, certChain, crlList, hashAlgorithm, PdfName.ADBE_PKCS7_SHA1.Equals(Get(PdfName.SUBFILTER)));
            pkcs.SetExternalDigest(externalDigest, externalRSAdata, digestEncryptionAlgorithm);
        }

        /**
        * Sets the digest/signature to an external calculated value.
        * @param digest the digest. This is the actual signature
        * @param RSAdata the extra data that goes into the data tag in PKCS#7
        * @param digestEncryptionAlgorithm the encryption algorithm. It may must be <CODE>null</CODE> if the <CODE>digest</CODE>
        * is also <CODE>null</CODE>. If the <CODE>digest</CODE> is not <CODE>null</CODE>
        * then it may be "RSA" or "DSA"
        */    
        public void SetExternalDigest(byte[] digest, byte[] RSAdata, String digestEncryptionAlgorithm) {
            externalDigest = digest;
            externalRSAdata = RSAdata;
            this.digestEncryptionAlgorithm = digestEncryptionAlgorithm;
        }

        /**
        * Gets the subject name in the signing certificate (the element "CN")
        * @return the subject name in the signing certificate (the element "CN")
        */    
        public new String Name {
            get {
                return name;
            }
        }

        /**
        * Gets the class instance that does the actual signing.
        * @return the class instance that does the actual signing
        */    
        public PdfPKCS7 Signer {
            get {
                return pkcs;
            }
        }

        /**
        * Gets the signature content. This can be a PKCS#1 or a PKCS#7. It corresponds to
        * the /Contents key.
        * @return the signature content
        */    
        public byte[] SignerContents {
            get {
                if (PdfName.ADBE_X509_RSA_SHA1.Equals(Get(PdfName.SUBFILTER)))
                    return pkcs.GetEncodedPKCS1();
                else
                    return pkcs.GetEncodedPKCS7();
            }
        }

        /**
        * Creates a standard filter of the type VeriSign.
        */    
        public class VeriSign : PdfSigGenericPKCS {
            /**
            * The constructor for the default provider.
            */        
            public VeriSign() : base(PdfName.VERISIGN_PPKVS, PdfName.ADBE_PKCS7_DETACHED) {
                hashAlgorithm = "MD5";
                Put(PdfName.R, new PdfNumber(65537));
            }
        }

        /**
        * Creates a standard filter of the type self signed.
        */    
        public class PPKLite : PdfSigGenericPKCS {
            /**
            * The constructor for the default provider.
            */        
            public PPKLite() : base(PdfName.ADOBE_PPKLITE, PdfName.ADBE_X509_RSA_SHA1) {
                hashAlgorithm = "SHA1";
                Put(PdfName.R, new PdfNumber(65541));
            }
        }

        /**
        * Creates a standard filter of the type Windows Certificate.
        */    
        public class PPKMS : PdfSigGenericPKCS {
            /**
            * The constructor for the default provider.
            */        
            public PPKMS() : base(PdfName.ADOBE_PPKMS, PdfName.ADBE_PKCS7_SHA1) {
                hashAlgorithm = "SHA1";
            }
        }
    }
}
