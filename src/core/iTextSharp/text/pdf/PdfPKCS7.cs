using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Globalization;
using System.IO;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Utilities;
using iTextSharp.text.error_messages;

/*
 * $Id$
 *
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
    * This class does all the processing related to signing and verifying a PKCS#7
    * signature.
    * <p>
    * It's based in code found at org.bouncycastle.
    */
    public class PdfPKCS7 {

        private byte[] sigAttr;
        private byte[] digestAttr;
        private int version, signerversion;
        private Dictionary<string,object> digestalgos;
        private List<X509Certificate> certs;
        private List<X509Crl> crls;
        private List<X509Certificate> signCerts;
        private X509Certificate signCert;
        private byte[] digest;
        private IDigest messageDigest;
        private IDigest encContDigest; // Stefan Santesson
        private String digestAlgorithm, digestEncryptionAlgorithm;
        private ISigner sig;
        private ICipherParameters privKey;
        private byte[] RSAdata;
        private bool verified;
        private bool verifyResult;
        private byte[] externalDigest;
        private byte[] externalRSAdata;
        
        private const String ID_PKCS7_DATA = "1.2.840.113549.1.7.1";
        private const String ID_PKCS7_SIGNED_DATA = "1.2.840.113549.1.7.2";
        private const String ID_RSA = "1.2.840.113549.1.1.1";
        private const String ID_DSA = "1.2.840.10040.4.1";
        private const String ID_CONTENT_TYPE = "1.2.840.113549.1.9.3";
        private const String ID_MESSAGE_DIGEST = "1.2.840.113549.1.9.4";
        private const String ID_SIGNING_TIME = "1.2.840.113549.1.9.5";
        private const String ID_ADBE_REVOCATION = "1.2.840.113583.1.1.8";
        /**
        * Holds value of property reason.
        */
        private String reason;
        
        /**
        * Holds value of property location.
        */
        private String location;
        
        /**
        * Holds value of property signDate.
        */
        private DateTime signDate;
        
        /**
        * Holds value of property signName.
        */
        private String signName;
        
        private TimeStampToken timeStampToken;
        
        private static readonly Dictionary<string,string> digestNames = new Dictionary<string,string>();
        private static readonly Dictionary<string,string> algorithmNames = new Dictionary<string,string>();
        private static readonly Dictionary<string,string> allowedDigests = new Dictionary<string,string>();

        static PdfPKCS7() {
            digestNames["1.2.840.113549.2.5"] = "MD5";
            digestNames["1.2.840.113549.2.2"] = "MD2";
            digestNames["1.3.14.3.2.26"] = "SHA1";
            digestNames["2.16.840.1.101.3.4.2.4"] = "SHA224";
            digestNames["2.16.840.1.101.3.4.2.1"] = "SHA256";
            digestNames["2.16.840.1.101.3.4.2.2"] = "SHA384";
            digestNames["2.16.840.1.101.3.4.2.3"] = "SHA512";
            digestNames["1.3.36.3.2.2"] = "RIPEMD128";
            digestNames["1.3.36.3.2.1"] = "RIPEMD160";
            digestNames["1.3.36.3.2.3"] = "RIPEMD256";
            digestNames["1.2.840.113549.1.1.4"] = "MD5";
            digestNames["1.2.840.113549.1.1.2"] = "MD2";
            digestNames["1.2.840.113549.1.1.5"] = "SHA1";
            digestNames["1.2.840.113549.1.1.14"] = "SHA224";
            digestNames["1.2.840.113549.1.1.11"] = "SHA256";
            digestNames["1.2.840.113549.1.1.12"] = "SHA384";
            digestNames["1.2.840.113549.1.1.13"] = "SHA512";
            digestNames["1.2.840.113549.2.5"] = "MD5";
            digestNames["1.2.840.113549.2.2"] = "MD2";
            digestNames["1.2.840.10040.4.3"] = "SHA1";
            digestNames["2.16.840.1.101.3.4.3.1"] = "SHA224";
            digestNames["2.16.840.1.101.3.4.3.2"] = "SHA256";
            digestNames["2.16.840.1.101.3.4.3.3"] = "SHA384";
            digestNames["2.16.840.1.101.3.4.3.4"] = "SHA512";
            digestNames["1.3.36.3.3.1.3"] = "RIPEMD128";
            digestNames["1.3.36.3.3.1.2"] = "RIPEMD160";
            digestNames["1.3.36.3.3.1.4"] = "RIPEMD256";
            
            algorithmNames["1.2.840.113549.1.1.1"] = "RSA";
            algorithmNames["1.2.840.10040.4.1"] = "DSA";
            algorithmNames["1.2.840.113549.1.1.2"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.4"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.5"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.14"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.11"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.12"] = "RSA";
            algorithmNames["1.2.840.113549.1.1.13"] = "RSA";
            algorithmNames["1.2.840.10040.4.3"] = "DSA";
            algorithmNames["2.16.840.1.101.3.4.3.1"] = "DSA";
            algorithmNames["2.16.840.1.101.3.4.3.2"] = "DSA";
            algorithmNames["1.3.36.3.3.1.3"] = "RSA";
            algorithmNames["1.3.36.3.3.1.2"] = "RSA";
            algorithmNames["1.3.36.3.3.1.4"] = "RSA";
            
            allowedDigests["MD5"] = "1.2.840.113549.2.5";
            allowedDigests["MD2"] = "1.2.840.113549.2.2";
            allowedDigests["SHA1"] = "1.3.14.3.2.26";
            allowedDigests["SHA224"] = "2.16.840.1.101.3.4.2.4";
            allowedDigests["SHA256"] = "2.16.840.1.101.3.4.2.1";
            allowedDigests["SHA384"] = "2.16.840.1.101.3.4.2.2";
            allowedDigests["SHA512"] = "2.16.840.1.101.3.4.2.3";
            allowedDigests["MD-5"] = "1.2.840.113549.2.5";
            allowedDigests["MD-2"] = "1.2.840.113549.2.2";
            allowedDigests["SHA-1"] = "1.3.14.3.2.26";
            allowedDigests["SHA-224"] = "2.16.840.1.101.3.4.2.4";
            allowedDigests["SHA-256"] = "2.16.840.1.101.3.4.2.1";
            allowedDigests["SHA-384"] = "2.16.840.1.101.3.4.2.2";
            allowedDigests["SHA-512"] = "2.16.840.1.101.3.4.2.3";
            allowedDigests["RIPEMD128"] = "1.3.36.3.2.2";
            allowedDigests["RIPEMD-128"] = "1.3.36.3.2.2";
            allowedDigests["RIPEMD160"] = "1.3.36.3.2.1";
            allowedDigests["RIPEMD-160"] = "1.3.36.3.2.1";
            allowedDigests["RIPEMD256"] = "1.3.36.3.2.3";
            allowedDigests["RIPEMD-256"] = "1.3.36.3.2.3";
        }
        
        /**
        * Gets the digest name for a certain id
        * @param oid    an id (for instance "1.2.840.113549.2.5")
        * @return   a digest name (for instance "MD5")
        * @since    2.1.6
        */
        public static String GetDigest(String oid) {
            String ret;
            if (digestNames.TryGetValue(oid, out ret))
                return ret;
            else
                return oid;
        }
        
        /**
        * Gets the algorithm name for a certain id.
        * @param oid    an id (for instance "1.2.840.113549.1.1.1")
        * @return   an algorithm name (for instance "RSA")
        * @since    2.1.6
        */
        public static String GetAlgorithm(String oid) {
            String ret;
            if (algorithmNames.TryGetValue(oid, out ret))
                return ret;
            else
                return oid;
        }
        
        public static String GetAllowedDigests(String name) {
            string ret;
            allowedDigests.TryGetValue(name.ToUpperInvariant(), out ret);
            return ret;
        }

        /**
        * Gets the timestamp token if there is one.
        * @return the timestamp token or null
        * @since    2.1.6
        */
        public TimeStampToken TimeStampToken {
            get {
                return timeStampToken;
            }
        }
        
        /**
        * Gets the timestamp date
        * @return   a date
        * @since    2.1.6
        */
        public DateTime TimeStampDate {
            get {
                if (timeStampToken == null)
                    return DateTime.MaxValue;
                return timeStampToken.TimeStampInfo.GenTime;
            }
        }
        
        /**
        * Verifies a signature using the sub-filter adbe.x509.rsa_sha1.
        * @param contentsKey the /Contents key
        * @param certsKey the /Cert key
        * @param provider the provider or <code>null</code> for the default provider
        */    
        public PdfPKCS7(byte[] contentsKey, byte[] certsKey) {

            X509CertificateParser cf = new X509CertificateParser();
            certs = new List<X509Certificate>();
            foreach (X509Certificate cc in cf.ReadCertificates(certsKey)) {
                certs.Add(cc);
            }
            signCerts = certs;
            signCert = certs[0];
            crls = new List<X509Crl>();
            Asn1InputStream inp = new Asn1InputStream(new MemoryStream(contentsKey));
            digest = ((DerOctetString)inp.ReadObject()).GetOctets();
            sig = SignerUtilities.GetSigner("SHA1withRSA");
            sig.Init(false, signCert.GetPublicKey());
        }
        
        private BasicOcspResp basicResp;
        
        /**
        * Gets the OCSP basic response if there is one.
        * @return the OCSP basic response or null
        * @since    2.1.6
        */
        public BasicOcspResp Ocsp {
            get {
                return basicResp;
            }
        }
        
        private void FindCRL(Asn1Sequence seq) {
            crls = new List<X509Crl>();
            for (int k = 0; k < seq.Count; ++k) {
                X509CrlParser pp = new X509CrlParser();
                X509Crl crl = pp.ReadCrl(seq[k].GetDerEncoded());
                crls.Add(crl);
            }
        }

        private void FindOcsp(Asn1Sequence seq) {
            basicResp = null;
            bool ret = false;
            while (true) {
                if ((seq[0] is DerObjectIdentifier) 
                    && ((DerObjectIdentifier)seq[0]).Id.Equals(OcspObjectIdentifiers.PkixOcspBasic.Id)) {
                    break;
                }
                ret = true;
                for (int k = 0; k < seq.Count; ++k) {
                    if (seq[k] is Asn1Sequence) {
                        seq = (Asn1Sequence)seq[0];
                        ret = false;
                        break;
                    }
                    if (seq[k] is Asn1TaggedObject) {
                        Asn1TaggedObject tag = (Asn1TaggedObject)seq[k];
                        if (tag.GetObject() is Asn1Sequence) {
                            seq = (Asn1Sequence)tag.GetObject();
                            ret = false;
                            break;
                        }
                        else
                            return;
                    }
                }
                if (ret)
                    return;
            }
            DerOctetString os = (DerOctetString)seq[1];
            Asn1InputStream inp = new Asn1InputStream(os.GetOctets());
            BasicOcspResponse resp = BasicOcspResponse.GetInstance(inp.ReadObject());
            basicResp = new BasicOcspResp(resp);
        }
        
        /**
        * Verifies a signature using the sub-filter adbe.pkcs7.detached or
        * adbe.pkcs7.sha1.
        * @param contentsKey the /Contents key
        * @param provider the provider or <code>null</code> for the default provider
        * @throws SecurityException on error
        * @throws CRLException on error
        * @throws InvalidKeyException on error
        * @throws CertificateException on error
        * @throws NoSuchProviderException on error
        * @throws NoSuchAlgorithmException on error
        */    
        public PdfPKCS7(byte[] contentsKey) {
            Asn1InputStream din = new Asn1InputStream(new MemoryStream(contentsKey));
            
            //
            // Basic checks to make sure it's a PKCS#7 SignedData Object
            //
            Asn1Object pkcs;
            
            try {
                pkcs = din.ReadObject();
            }
            catch  {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("can.t.decode.pkcs7signeddata.object"));
            }
            if (!(pkcs is Asn1Sequence)) {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("not.a.valid.pkcs.7.object.not.a.sequence"));
            }
            Asn1Sequence signedData = (Asn1Sequence)pkcs;
            DerObjectIdentifier objId = (DerObjectIdentifier)signedData[0];
            if (!objId.Id.Equals(ID_PKCS7_SIGNED_DATA))
                throw new ArgumentException(MessageLocalization.GetComposedMessage("not.a.valid.pkcs.7.object.not.signed.data"));
            Asn1Sequence content = (Asn1Sequence)((DerTaggedObject)signedData[1]).GetObject();
            // the positions that we care are:
            //     0 - version
            //     1 - digestAlgorithms
            //     2 - possible ID_PKCS7_DATA
            //     (the certificates and crls are taken out by other means)
            //     last - signerInfos
            
            // the version
            version = ((DerInteger)content[0]).Value.IntValue;
            
            // the digestAlgorithms
            digestalgos = new Dictionary<string,object>();
            IEnumerator e = ((Asn1Set)content[1]).GetEnumerator();
            while (e.MoveNext())
            {
                Asn1Sequence s = (Asn1Sequence)e.Current;
                DerObjectIdentifier o = (DerObjectIdentifier)s[0];
                digestalgos[o.Id] = null;
            }
            
            // the certificates and crls
            X509CertificateParser cf = new X509CertificateParser();
            certs = new List<X509Certificate>();
            foreach (X509Certificate cc in cf.ReadCertificates(contentsKey)) {
                certs.Add(cc);
            }
            crls = new List<X509Crl>();
            
            // the possible ID_PKCS7_DATA
            Asn1Sequence rsaData = (Asn1Sequence)content[2];
            if (rsaData.Count > 1) {
                DerOctetString rsaDataContent = (DerOctetString)((DerTaggedObject)rsaData[1]).GetObject();
                RSAdata = rsaDataContent.GetOctets();
            }
            
            // the signerInfos
            int next = 3;
            while (content[next] is DerTaggedObject)
                ++next;
            Asn1Set signerInfos = (Asn1Set)content[next];
            if (signerInfos.Count != 1)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("this.pkcs.7.object.has.multiple.signerinfos.only.one.is.supported.at.this.time"));
            Asn1Sequence signerInfo = (Asn1Sequence)signerInfos[0];
            // the positions that we care are
            //     0 - version
            //     1 - the signing certificate issuer and serial number
            //     2 - the digest algorithm
            //     3 or 4 - digestEncryptionAlgorithm
            //     4 or 5 - encryptedDigest
            signerversion = ((DerInteger)signerInfo[0]).Value.IntValue;
            // Get the signing certificate
            Asn1Sequence issuerAndSerialNumber = (Asn1Sequence)signerInfo[1];
            Org.BouncyCastle.Asn1.X509.X509Name issuer = Org.BouncyCastle.Asn1.X509.X509Name.GetInstance(issuerAndSerialNumber[0]);
            BigInteger serialNumber = ((DerInteger)issuerAndSerialNumber[1]).Value;
            foreach (X509Certificate cert in certs) {                                                            
                if (issuer.Equivalent(cert.IssuerDN) && serialNumber.Equals(cert.SerialNumber)) {
                    signCert = cert;                                                                             
                    break;                                                                                            
                }                                                                                                
            }
            if (signCert == null) {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("can.t.find.signing.certificate.with.serial.1", 
                    issuer.ToString() + " / " + serialNumber.ToString(16)));
            }
            CalcSignCertificateChain();
            digestAlgorithm = ((DerObjectIdentifier)((Asn1Sequence)signerInfo[2])[0]).Id;
            next = 3;
            if (signerInfo[next] is Asn1TaggedObject) {
                Asn1TaggedObject tagsig = (Asn1TaggedObject)signerInfo[next];
                Asn1Set sseq = Asn1Set.GetInstance(tagsig, false);
                sigAttr = sseq.GetEncoded(Asn1Encodable.Der);
                
                for (int k = 0; k < sseq.Count; ++k) {
                    Asn1Sequence seq2 = (Asn1Sequence)sseq[k];
                    if (((DerObjectIdentifier)seq2[0]).Id.Equals(ID_MESSAGE_DIGEST)) {
                        Asn1Set sset = (Asn1Set)seq2[1];
                        digestAttr = ((DerOctetString)sset[0]).GetOctets();
                    }
                    else if (((DerObjectIdentifier)seq2[0]).Id.Equals(ID_ADBE_REVOCATION)) {
                        Asn1Set setout = (Asn1Set)seq2[1];
                        Asn1Sequence seqout = (Asn1Sequence)setout[0];
                        for (int j = 0; j < seqout.Count; ++j) {
                            Asn1TaggedObject tg = (Asn1TaggedObject)seqout[j];
                            if (tg.TagNo == 1) {
                                Asn1Sequence seqin = (Asn1Sequence)tg.GetObject();
                                FindOcsp(seqin);
                            }
                            if (tg.TagNo == 0) {
                                Asn1Sequence seqin = (Asn1Sequence)tg.GetObject();
                                FindCRL(seqin);
                            }
                        }
                    }
                }
                if (digestAttr == null)
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("authenticated.attribute.is.missing.the.digest"));
                ++next;
            }
            digestEncryptionAlgorithm = ((DerObjectIdentifier)((Asn1Sequence)signerInfo[next++])[0]).Id;
            digest = ((DerOctetString)signerInfo[next++]).GetOctets();
            if (next < signerInfo.Count && (signerInfo[next] is DerTaggedObject)) {
                DerTaggedObject taggedObject = (DerTaggedObject) signerInfo[next];
                Asn1Set unat = Asn1Set.GetInstance(taggedObject, false);
                Org.BouncyCastle.Asn1.Cms.AttributeTable attble = new Org.BouncyCastle.Asn1.Cms.AttributeTable(unat);
                Org.BouncyCastle.Asn1.Cms.Attribute ts = attble[PkcsObjectIdentifiers.IdAASignatureTimeStampToken];
                if (ts != null && ts.AttrValues.Count > 0) {
                    Asn1Set attributeValues = ts.AttrValues;
                    Asn1Sequence tokenSequence = Asn1Sequence.GetInstance(attributeValues[0]);
                    Org.BouncyCastle.Asn1.Cms.ContentInfo contentInfo = Org.BouncyCastle.Asn1.Cms.ContentInfo.GetInstance(tokenSequence);
                    this.timeStampToken = new TimeStampToken(contentInfo);
                }
            }
            if (RSAdata != null || digestAttr != null) {
                messageDigest = GetHashClass();
                encContDigest = GetHashClass();
            }
            sig = SignerUtilities.GetSigner(GetDigestAlgorithm());
            sig.Init(false, signCert.GetPublicKey());
        }

        /**
        * Generates a signature.
        * @param privKey the private key
        * @param certChain the certificate chain
        * @param crlList the certificate revocation list
        * @param hashAlgorithm the hash algorithm
        * @param provider the provider or <code>null</code> for the default provider
        * @param hasRSAdata <CODE>true</CODE> if the sub-filter is adbe.pkcs7.sha1
        * @throws SecurityException on error
        * @throws InvalidKeyException on error
        * @throws NoSuchProviderException on error
        * @throws NoSuchAlgorithmException on error
        */    
        public PdfPKCS7(ICipherParameters privKey, X509Certificate[] certChain, object[] crlList,
                        String hashAlgorithm, bool hasRSAdata) {
            this.privKey = privKey;
            
            digestAlgorithm = GetAllowedDigests(hashAlgorithm);
            if (digestAlgorithm == null)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("unknown.hash.algorithm.1", hashAlgorithm));
            
            version = signerversion = 1;
            certs = new List<X509Certificate>();
            crls = new List<X509Crl>();
            digestalgos = new Dictionary<string,object>();
            digestalgos[digestAlgorithm] = null;
            
            //
            // Copy in the certificates and crls used to sign the private key.
            //
            signCert = certChain[0];
            for (int i = 0;i < certChain.Length;i++) {
                certs.Add(certChain[i]);
            }
            
//            if (crlList != null) {
//                for (int i = 0;i < crlList.length;i++) {
//                    crls.Add(crlList[i]);
//                }
//            }
            
            if (privKey != null) {
                //
                // Now we have private key, find out what the digestEncryptionAlgorithm is.
                //
                if (privKey is RsaKeyParameters)
                    digestEncryptionAlgorithm = ID_RSA;
                else if (privKey is DsaKeyParameters)
                    digestEncryptionAlgorithm = ID_DSA;
                else
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("unknown.key.algorithm.1", privKey.ToString()));

            }
            if (hasRSAdata) {
                RSAdata = new byte[0];
                messageDigest = GetHashClass();
            }

            if (privKey != null) {
                sig = SignerUtilities.GetSigner(GetDigestAlgorithm());
                sig.Init(true, privKey);
            }
        }

        /**
        * Update the digest with the specified bytes. This method is used both for signing and verifying
        * @param buf the data buffer
        * @param off the offset in the data buffer
        * @param len the data length
        * @throws SignatureException on error
        */
        public void Update(byte[] buf, int off, int len) {
            if (RSAdata != null || digestAttr != null)
                messageDigest.BlockUpdate(buf, off, len);
            else
                sig.BlockUpdate(buf, off, len);
        }
        
        /**
        * Verify the digest.
        * @throws SignatureException on error
        * @return <CODE>true</CODE> if the signature checks out, <CODE>false</CODE> otherwise
        */
        public bool Verify() {
            if (verified)
                return verifyResult;
            if (sigAttr != null) {
                byte[] msgDigestBytes = new byte[messageDigest.GetDigestSize()];
                messageDigest.DoFinal(msgDigestBytes, 0);
                bool verifyRSAdata = true;
                sig.BlockUpdate(sigAttr, 0, sigAttr.Length);
                // Stefan Santesson fixed a bug, keeping the code backward compatible
                bool encContDigestCompare = false;
                if (RSAdata != null) {
                    verifyRSAdata = Arrays.AreEqual(msgDigestBytes, RSAdata);
                    encContDigest.BlockUpdate(RSAdata, 0, RSAdata.Length);
                    byte[] encContDigestBytes = new byte[encContDigest.GetDigestSize()];
                    encContDigest.DoFinal(encContDigestBytes, 0);
                    encContDigestCompare = Arrays.AreEqual(encContDigestBytes, digestAttr);
                }
                bool absentEncContDigestCompare = Arrays.AreEqual(msgDigestBytes, digestAttr);
                bool concludingDigestCompare = absentEncContDigestCompare || encContDigestCompare;
                bool sigVerify = sig.VerifySignature(digest);
                verifyResult = concludingDigestCompare && sigVerify && verifyRSAdata;
                //verifyResult = Arrays.AreEqual(msgDigestBytes, digestAttr) && sig.VerifySignature(digest) && verifyRSAdata;
            }
            else {
                if (RSAdata != null){
                    byte[] msd = new byte[messageDigest.GetDigestSize()];
                    messageDigest.DoFinal(msd, 0);
                    sig.BlockUpdate(msd, 0, msd.Length);
                }
                verifyResult = sig.VerifySignature(digest);
            }
            verified = true;
            return verifyResult;
        }
        
        /**
        * Checks if the timestamp refers to this document.
        * @throws java.security.NoSuchAlgorithmException on error
        * @return true if it checks false otherwise
        * @since    2.1.6
        */
        public bool VerifyTimestampImprint() {
            if (timeStampToken == null)
                return false;
            TimeStampTokenInfo info = timeStampToken.TimeStampInfo;
            MessageImprint imprint = info.TstInfo.MessageImprint;
            String algOID = info.MessageImprintAlgOid;
            byte[] md = PdfEncryption.DigestComputeHash(GetDigest(algOID), digest);
            byte[] imphashed = imprint.GetHashedMessage();
            bool res = Arrays.AreEqual(md, imphashed);
            return res;
        }
        
        /**
        * Get all the X.509 certificates associated with this PKCS#7 object in no particular order.
        * Other certificates, from OCSP for example, will also be included.
        * @return the X.509 certificates associated with this PKCS#7 object
        */
        public X509Certificate[] Certificates {
            get {
                X509Certificate[] c = new X509Certificate[certs.Count];
                certs.CopyTo(c);
                return c;
            }
        }
            
        /**
        * Get the X.509 sign certificate chain associated with this PKCS#7 object.
        * Only the certificates used for the main signature will be returned, with
        * the signing certificate first.
        * @return the X.509 certificates associated with this PKCS#7 object
        * @since    2.1.6
        */
        public X509Certificate[] SignCertificateChain {
            get {
                X509Certificate[] ret = new X509Certificate[signCerts.Count];
                signCerts.CopyTo(ret);
                return ret;
            }
        }
        
        private void CalcSignCertificateChain() {
            List<X509Certificate> cc = new List<X509Certificate>();
            cc.Add(signCert);
            List<X509Certificate> oc = new List<X509Certificate>(certs);
            for (int k = 0; k < oc.Count; ++k) {
                if (signCert.Equals(oc[k])) {
                    oc.RemoveAt(k);
                    --k;
                    continue;
                }
            }
            bool found = true;
            while (found) {
                X509Certificate v = cc[cc.Count - 1];
                found = false;
                for (int k = 0; k < oc.Count; ++k) {
                    try {
                        v.Verify(oc[k].GetPublicKey());
                        found = true;
                        cc.Add(oc[k]);
                        oc.RemoveAt(k);
                        break;
                    }
                    catch {
                    }
                }
            }
            signCerts = cc;
        }
        
        /**
        * Get the X.509 certificate revocation lists associated with this PKCS#7 object
        * @return the X.509 certificate revocation lists associated with this PKCS#7 object in the form of X509Crl
        */
        public List<X509Crl> CRLs {
            get {
                return crls;
            }
        }
        
        /**
        * Get the X.509 certificate actually used to sign the digest.
        * @return the X.509 certificate actually used to sign the digest
        */
        public X509Certificate SigningCertificate {
            get {
                return signCert;
            }
        }
        
        /**
        * Get the version of the PKCS#7 object. Always 1
        * @return the version of the PKCS#7 object. Always 1
        */
        public int Version {
            get {
                return version;
            }
        }
        
        /**
        * Get the version of the PKCS#7 "SignerInfo" object. Always 1
        * @return the version of the PKCS#7 "SignerInfo" object. Always 1
        */
        public int SigningInfoVersion {
            get {
                return signerversion;
            }
        }

        /**
         * Getter for the digest encryption algorithm
         */
        public String DigestEncryptionAlgorithmOid {
            get {
                return digestEncryptionAlgorithm;
            }
        }

        /**
         * Getter for the digest algorithm
         */
        public String DigestAlgorithmOid {
            get {
                return digestAlgorithm;
            }
        }
        
        /**
        * Get the algorithm used to calculate the message digest
        * @return the algorithm used to calculate the message digest
        */
        public String GetDigestAlgorithm() {
            String dea = GetAlgorithm(digestEncryptionAlgorithm);
            if (dea == null)
                dea = digestEncryptionAlgorithm;
            
            return GetHashAlgorithm() + "with" + dea;
        }

        /**
        * Returns the algorithm.
        * @return the digest algorithm
        */
        public String GetHashAlgorithm() {
            return GetDigest(digestAlgorithm);
        }

        internal IDigest GetHashClass() {
            return DigestUtilities.GetDigest(GetHashAlgorithm());
        }

        /**
        * Loads the default root certificates at &lt;java.home&gt;/lib/security/cacerts
        * with the default provider.
        * @return a <CODE>KeyStore</CODE>
        */    
//        public static KeyStore LoadCacertsKeyStore() {
//            return LoadCacertsKeyStore(null);
//        }

        /**
        * Loads the default root certificates at &lt;java.home&gt;/lib/security/cacerts.
        * @param provider the provider or <code>null</code> for the default provider
        * @return a <CODE>KeyStore</CODE>
        */    
//        public static KeyStore LoadCacertsKeyStore(String provider) {
//            File file = new File(System.GetProperty("java.home"), "lib");
//            file = new File(file, "security");
//            file = new File(file, "cacerts");
//            FileInputStream fin = null;
//            try {
//                fin = new FileInputStream(file);
//                KeyStore k;
//                if (provider == null)
//                    k = KeyStore.GetInstance("JKS");
//                else
//                    k = KeyStore.GetInstance("JKS", provider);
//                k.Load(fin, null);
//                return k;
//            }
//            catch (Exception e) {
//                throw new ExceptionConverter(e);
//            }
//            finally {
//                try{fin.Close();}catch(Exception ex){}
//            }
//        }
        
        /**
        * Verifies a single certificate.
        * @param cert the certificate to verify
        * @param crls the certificate revocation list or <CODE>null</CODE>
        * @param calendar the date or <CODE>null</CODE> for the current date
        * @return a <CODE>String</CODE> with the error description or <CODE>null</CODE>
        * if no error
        */    
        public static String VerifyCertificate(X509Certificate cert, X509Crl[] crls, DateTime calendar) {
            try {
                if (!cert.IsValid(calendar))
                    return "The certificate has expired or is not yet valid";
            }
            catch (Exception e) {
                return e.ToString();
            }
            return null;
        }
        
        /**
        * Verifies a certificate chain against a KeyStore.
        * @param certs the certificate chain
        * @param keystore the <CODE>KeyStore</CODE>
        * @param crls the certificate revocation list or <CODE>null</CODE>
        * @param calendar the date or <CODE>null</CODE> for the current date
        * @return <CODE>null</CODE> if the certificate chain could be validade or a
        * <CODE>Object[]{cert,error}</CODE> where <CODE>cert</CODE> is the
        * failed certificate and <CODE>error</CODE> is the error message
        */    
        public static Object[] VerifyCertificates(X509Certificate[] certs, List<X509Certificate> keystore, X509Crl[] crls, DateTime calendar) {
            for (int k = 0; k < certs.Length; ++k) {
                X509Certificate cert = certs[k];
                String err = VerifyCertificate(cert, crls, calendar);
                if (err != null)
                    return new Object[]{cert, err};
                foreach (X509Certificate certStoreX509 in keystore) {
                    try {
                        if (VerifyCertificate(certStoreX509, crls, calendar) != null)
                            continue;
                        try {
                            cert.Verify(certStoreX509.GetPublicKey());
                            return null;
                        }
                        catch {
                            continue;
                        }
                    }
                    catch {
                    }
                }
                int j;
                for (j = 0; j < certs.Length; ++j) {
                    if (j == k)
                        continue;
                    X509Certificate certNext = certs[j];
                    try {
                        cert.Verify(certNext.GetPublicKey());
                        break;
                    }
                    catch {
                    }
                }
                if (j == certs.Length)
                    return new Object[]{cert, "Cannot be verified against the KeyStore or the certificate chain"};
            }
            return new Object[]{null, "Invalid state. Possible circular certificate chain"};
        }

        /**
        * Verifies an OCSP response against a KeyStore.
        * @param ocsp the OCSP response
        * @param keystore the <CODE>KeyStore</CODE>
        * @param provider the provider or <CODE>null</CODE> to use the BouncyCastle provider
        * @return <CODE>true</CODE> is a certificate was found
        * @since    2.1.6
        */    
        public static bool VerifyOcspCertificates(BasicOcspResp ocsp, List<X509Certificate> keystore) {
            try {
                foreach (X509Certificate certStoreX509 in keystore) {
                    try {
                        if (ocsp.Verify(certStoreX509.GetPublicKey()))
                            return true;
                    }
                    catch {
                    }
                }
            }
            catch {
            }
            return false;
        }
        
        /**
        * Verifies a timestamp against a KeyStore.
        * @param ts the timestamp
        * @param keystore the <CODE>KeyStore</CODE>
        * @param provider the provider or <CODE>null</CODE> to use the BouncyCastle provider
        * @return <CODE>true</CODE> is a certificate was found
        * @since    2.1.6
        */    
        public static bool VerifyTimestampCertificates(TimeStampToken ts, List<X509Certificate> keystore) {
            try {
                foreach (X509Certificate certStoreX509 in keystore) {
                    try {
                        ts.Validate(certStoreX509);
                        return true;
                    }
                    catch {
                    }
                }
            }
            catch {
            }
            return false;
        }
        
        /**
        * Retrieves the OCSP URL from the given certificate.
        * @param certificate the certificate
        * @return the URL or null
        * @throws CertificateParsingException on error
        * @since    2.1.6
        */
        public static String GetOCSPURL(X509Certificate certificate) {
            try {
                Asn1Object obj = GetExtensionValue(certificate, X509Extensions.AuthorityInfoAccess.Id);
                if (obj == null) {
                    return null;
                }
                
                Asn1Sequence AccessDescriptions = (Asn1Sequence) obj;
                for (int i = 0; i < AccessDescriptions.Count; i++) {
                    Asn1Sequence AccessDescription = (Asn1Sequence) AccessDescriptions[i];
                    if ( AccessDescription.Count != 2 ) {
                        continue;
                    } else {
                        if ((AccessDescription[0] is DerObjectIdentifier) && ((DerObjectIdentifier)AccessDescription[0]).Id.Equals("1.3.6.1.5.5.7.48.1")) {
                            String AccessLocation =  GetStringFromGeneralName((Asn1Object)AccessDescription[1]);
                            if ( AccessLocation == null ) {
                                return "" ;
                            } else {
                                return AccessLocation ;
                            }
                        }
                    }
                }
            } catch {
            }
            return null;
        }
        
        public static String GetCrlUrl(X509Certificate certificate)  {
            try {
                Asn1Object obj = GetExtensionValue(certificate, X509Extensions.CrlDistributionPoints.Id);
                if (obj == null) {
                    return null;
                }
                CrlDistPoint dist = CrlDistPoint.GetInstance(obj);
                DistributionPoint[] dists = dist.GetDistributionPoints();
                foreach (DistributionPoint p in dists) {
                    DistributionPointName distributionPointName = p.DistributionPointName;
                    if (DistributionPointName.FullName != distributionPointName.PointType) {
                        continue;
                    }
                    GeneralNames generalNames = (GeneralNames)distributionPointName.Name;
                    GeneralName[] names = generalNames.GetNames();
                    foreach (GeneralName name in names) {
                        if (name.TagNo != GeneralName.UniformResourceIdentifier) {
                            continue;
                        }
                        DerIA5String derStr = DerIA5String.GetInstance(name.Name);
                        return derStr.GetString();
                    }
                }
            } catch {
            }
            return null;
        }

        /**
        * Checks if OCSP revocation refers to the document signing certificate.
        * @return true if it checks false otherwise
        * @since    2.1.6
        */
        public bool IsRevocationValid() {
            if (basicResp == null)
                return false;
            if (signCerts.Count < 2)
                return false;
            try {
                X509Certificate[] cs = SignCertificateChain;
                SingleResp sr = basicResp.Responses[0];
                CertificateID cid = sr.GetCertID();
                X509Certificate sigcer = SigningCertificate;
                X509Certificate isscer = cs[1];
                CertificateID tis = new CertificateID(CertificateID.HashSha1, isscer, sigcer.SerialNumber);
                return tis.Equals(cid);
            }
            catch {
            }
            return false;
        }
        
        private static Asn1Object GetExtensionValue(X509Certificate cert, String oid) {
            byte[] bytes = cert.GetExtensionValue(new DerObjectIdentifier(oid)).GetDerEncoded();
            if (bytes == null) {
                return null;
            }
            Asn1InputStream aIn = new Asn1InputStream(new MemoryStream(bytes));
            Asn1OctetString octs = (Asn1OctetString) aIn.ReadObject();
            aIn = new Asn1InputStream(new MemoryStream(octs.GetOctets()));
            return aIn.ReadObject();
        }
        
        private static String GetStringFromGeneralName(Asn1Object names) {
            DerTaggedObject taggedObject = (DerTaggedObject) names ;
            return Encoding.GetEncoding(1252).GetString(Asn1OctetString.GetInstance(taggedObject, false).GetOctets());
        }

        /**
        * Get the "issuer" from the TBSCertificate bytes that are passed in
        * @param enc a TBSCertificate in a byte array
        * @return a DERObject
        */
        private static Asn1Object GetIssuer(byte[] enc) {
            Asn1InputStream inp = new Asn1InputStream(new MemoryStream(enc));
            Asn1Sequence seq = (Asn1Sequence)inp.ReadObject();
            return (Asn1Object)seq[seq[0] is DerTaggedObject ? 3 : 2];
        }

        /**
        * Get the "subject" from the TBSCertificate bytes that are passed in
        * @param enc A TBSCertificate in a byte array
        * @return a DERObject
        */
        private static Asn1Object GetSubject(byte[] enc) {
            Asn1InputStream inp = new Asn1InputStream(new MemoryStream(enc));
            Asn1Sequence seq = (Asn1Sequence)inp.ReadObject();
            return (Asn1Object)seq[seq[0] is DerTaggedObject ? 5 : 4];
        }

        /**
        * Get the issuer fields from an X509 Certificate
        * @param cert an X509Certificate
        * @return an X509Name
        */
        public static X509Name GetIssuerFields(X509Certificate cert) {
            return new X509Name((Asn1Sequence)GetIssuer(cert.GetTbsCertificate()));
        }

        /**
        * Get the subject fields from an X509 Certificate
        * @param cert an X509Certificate
        * @return an X509Name
        */
        public static X509Name GetSubjectFields(X509Certificate cert) {
            return new X509Name((Asn1Sequence)GetSubject(cert.GetTbsCertificate()));
        }
        
        /**
        * Gets the bytes for the PKCS#1 object.
        * @return a byte array
        */
        public byte[] GetEncodedPKCS1() {
            if (externalDigest != null)
                digest = externalDigest;
            else
                digest = sig.GenerateSignature();
            MemoryStream bOut = new MemoryStream();
            
            Asn1OutputStream dout = new Asn1OutputStream(bOut);
            dout.WriteObject(new DerOctetString(digest));
            dout.Close();
            
            return bOut.ToArray();
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
            if (digestEncryptionAlgorithm != null) {
                if (digestEncryptionAlgorithm.Equals("RSA")) {
                    this.digestEncryptionAlgorithm = ID_RSA;
                }
                else if (digestEncryptionAlgorithm.Equals("DSA")) {
                    this.digestEncryptionAlgorithm = ID_DSA;
                }
                else
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("unknown.key.algorithm.1", digestEncryptionAlgorithm));
            }
        }
        
        /**
        * Gets the bytes for the PKCS7SignedData object.
        * @return the bytes for the PKCS7SignedData object
        */
        public byte[] GetEncodedPKCS7() {
            return GetEncodedPKCS7(null, DateTime.Now, null, null);
        }
        
        /**
        * Gets the bytes for the PKCS7SignedData object. Optionally the authenticatedAttributes
        * in the signerInfo can also be set. If either of the parameters is <CODE>null</CODE>, none will be used.
        * @param secondDigest the digest in the authenticatedAttributes
        * @param signingTime the signing time in the authenticatedAttributes
        * @return the bytes for the PKCS7SignedData object
        */
        public byte[] GetEncodedPKCS7(byte[] secondDigest, DateTime signingTime) {
            return GetEncodedPKCS7(secondDigest, signingTime, null, null);
        }

        /**
        * Gets the bytes for the PKCS7SignedData object. Optionally the authenticatedAttributes
        * in the signerInfo can also be set, OR a time-stamp-authority client
        * may be provided.
        * @param secondDigest the digest in the authenticatedAttributes
        * @param signingTime the signing time in the authenticatedAttributes
        * @param tsaClient TSAClient - null or an optional time stamp authority client
        * @return byte[] the bytes for the PKCS7SignedData object
        * @since   2.1.6
        */
        public byte[] GetEncodedPKCS7(byte[] secondDigest, DateTime signingTime, ITSAClient tsaClient, byte[] ocsp) {
            if (externalDigest != null) {
                digest = externalDigest;
                if (RSAdata != null)
                    RSAdata = externalRSAdata;
            }
            else if (externalRSAdata != null && RSAdata != null) {
                RSAdata = externalRSAdata;
                sig.BlockUpdate(RSAdata, 0, RSAdata.Length);
                digest = sig.GenerateSignature();
            }
            else {
                if (RSAdata != null) {
                    RSAdata = new byte[messageDigest.GetDigestSize()];
                    messageDigest.DoFinal(RSAdata, 0);
                    sig.BlockUpdate(RSAdata, 0, RSAdata.Length);
                }
                digest = sig.GenerateSignature();
            }
            
            // Create the set of Hash algorithms
            Asn1EncodableVector digestAlgorithms = new Asn1EncodableVector();
            foreach (string dal in digestalgos.Keys) {
                Asn1EncodableVector algos = new Asn1EncodableVector();
                algos.Add(new DerObjectIdentifier(dal));
                algos.Add(DerNull.Instance);
                digestAlgorithms.Add(new DerSequence(algos));
            }
            
            // Create the contentInfo.
            Asn1EncodableVector v = new Asn1EncodableVector();
            v.Add(new DerObjectIdentifier(ID_PKCS7_DATA));
            if (RSAdata != null)
                v.Add(new DerTaggedObject(0, new DerOctetString(RSAdata)));
            DerSequence contentinfo = new DerSequence(v);
            
            // Get all the certificates
            //
            v = new Asn1EncodableVector();
            foreach (X509Certificate xcert in certs) {
                Asn1InputStream tempstream = new Asn1InputStream(new MemoryStream(xcert.GetEncoded()));
                v.Add(tempstream.ReadObject());
            }
            
            DerSet dercertificates = new DerSet(v);
            
            // Create signerinfo structure.
            //
            Asn1EncodableVector signerinfo = new Asn1EncodableVector();
            
            // Add the signerInfo version
            //
            signerinfo.Add(new DerInteger(signerversion));
            
            v = new Asn1EncodableVector();
            v.Add(GetIssuer(signCert.GetTbsCertificate()));
            v.Add(new DerInteger(signCert.SerialNumber));
            signerinfo.Add(new DerSequence(v));
            
            // Add the digestAlgorithm
            v = new Asn1EncodableVector();
            v.Add(new DerObjectIdentifier(digestAlgorithm));
            v.Add(DerNull.Instance);
            signerinfo.Add(new DerSequence(v));
            
            // add the authenticated attribute if present
            if (secondDigest != null /*&& signingTime != null*/) {
                signerinfo.Add(new DerTaggedObject(false, 0, GetAuthenticatedAttributeSet(secondDigest, signingTime, ocsp)));
            }
            // Add the digestEncryptionAlgorithm
            v = new Asn1EncodableVector();
            v.Add(new DerObjectIdentifier(digestEncryptionAlgorithm));
            v.Add(DerNull.Instance);
            signerinfo.Add(new DerSequence(v));
            
            // Add the digest
            signerinfo.Add(new DerOctetString(digest));
            
            // When requested, go get and add the timestamp. May throw an exception.
            // Added by Martin Brunecky, 07/12/2007 folowing Aiken Sam, 2006-11-15
            // Sam found Adobe expects time-stamped SHA1-1 of the encrypted digest
            if (tsaClient != null) {
                byte[] tsImprint = PdfEncryption.DigestComputeHash(tsaClient.GetDigestAlgorithm(), digest);
                byte[] tsToken = tsaClient.GetTimeStampToken(tsImprint);
                if (tsToken != null) {
                    Asn1EncodableVector unauthAttributes = BuildUnauthenticatedAttributes(tsToken);
                    if (unauthAttributes != null) {
                        signerinfo.Add(new DerTaggedObject(false, 1, new DerSet(unauthAttributes)));
                    }
                }
            }
            
            // Finally build the body out of all the components above
            Asn1EncodableVector body = new Asn1EncodableVector();
            body.Add(new DerInteger(version));
            body.Add(new DerSet(digestAlgorithms));
            body.Add(contentinfo);
            body.Add(new DerTaggedObject(false, 0, dercertificates));
            
//                if (crls.Count > 0) {
//                    v = new Asn1EncodableVector();
//                    for (Iterator i = crls.Iterator();i.HasNext();) {
//                        Asn1InputStream t = new Asn1InputStream(new ByteArrayInputStream((((X509CRL)i.Next()).GetEncoded())));
//                        v.Add(t.ReadObject());
//                    }
//                    DERSet dercrls = new DERSet(v);
//                    body.Add(new DERTaggedObject(false, 1, dercrls));
//                }
            
            // Only allow one signerInfo
            body.Add(new DerSet(new DerSequence(signerinfo)));
            
            // Now we have the body, wrap it in it's PKCS7Signed shell
            // and return it
            //
            Asn1EncodableVector whole = new Asn1EncodableVector();
            whole.Add(new DerObjectIdentifier(ID_PKCS7_SIGNED_DATA));
            whole.Add(new DerTaggedObject(0, new DerSequence(body)));
            
            MemoryStream bOut = new MemoryStream();
            
            Asn1OutputStream dout = new Asn1OutputStream(bOut);
            dout.WriteObject(new DerSequence(whole));
            dout.Close();
            
            return bOut.ToArray();
        }
        
        /**
        * Added by Aiken Sam, 2006-11-15, modifed by Martin Brunecky 07/12/2007
        * to start with the timeStampToken (signedData 1.2.840.113549.1.7.2).
        * Token is the TSA response without response status, which is usually
        * handled by the (vendor supplied) TSA request/response interface).
        * @param timeStampToken byte[] - time stamp token, DER encoded signedData
        * @return ASN1EncodableVector
        * @throws IOException
        */
        private Asn1EncodableVector BuildUnauthenticatedAttributes(byte[] timeStampToken) {
            if (timeStampToken == null)
                return null;

            // @todo: move this together with the rest of the defintions
            String ID_TIME_STAMP_TOKEN = "1.2.840.113549.1.9.16.2.14"; // RFC 3161 id-aa-timeStampToken

            Asn1InputStream tempstream = new Asn1InputStream(new MemoryStream(timeStampToken));
            Asn1EncodableVector unauthAttributes = new Asn1EncodableVector();

            Asn1EncodableVector v = new Asn1EncodableVector();
            v.Add(new DerObjectIdentifier(ID_TIME_STAMP_TOKEN)); // id-aa-timeStampToken
            Asn1Sequence seq = (Asn1Sequence) tempstream.ReadObject();
            v.Add(new DerSet(seq));

            unauthAttributes.Add(new DerSequence(v));
            return unauthAttributes;
        }
        
        /**
        * When using authenticatedAttributes the authentication process is different.
        * The document digest is generated and put inside the attribute. The signing is done over the DER encoded
        * authenticatedAttributes. This method provides that encoding and the parameters must be
        * exactly the same as in {@link #getEncodedPKCS7(byte[],Calendar)}.
        * <p>
        * A simple example:
        * <p>
        * <pre>
        * Calendar cal = Calendar.GetInstance();
        * PdfPKCS7 pk7 = new PdfPKCS7(key, chain, null, "SHA1", null, false);
        * MessageDigest messageDigest = MessageDigest.GetInstance("SHA1");
        * byte buf[] = new byte[8192];
        * int n;
        * Stream inp = sap.GetRangeStream();
        * while ((n = inp.Read(buf)) &gt; 0) {
        *    messageDigest.Update(buf, 0, n);
        * }
        * byte hash[] = messageDigest.Digest();
        * byte sh[] = pk7.GetAuthenticatedAttributeBytes(hash, cal);
        * pk7.Update(sh, 0, sh.length);
        * byte sg[] = pk7.GetEncodedPKCS7(hash, cal);
        * </pre>
        * @param secondDigest the content digest
        * @param signingTime the signing time
        * @return the byte array representation of the authenticatedAttributes ready to be signed
        */    
        public byte[] GetAuthenticatedAttributeBytes(byte[] secondDigest, DateTime signingTime, byte[] ocsp) {
            return GetAuthenticatedAttributeSet(secondDigest, signingTime, ocsp).GetEncoded(Asn1Encodable.Der);
        }
        
        private DerSet GetAuthenticatedAttributeSet(byte[] secondDigest, DateTime signingTime, byte[] ocsp) {
            Asn1EncodableVector attribute = new Asn1EncodableVector();
            Asn1EncodableVector v = new Asn1EncodableVector();
            v.Add(new DerObjectIdentifier(ID_CONTENT_TYPE));
            v.Add(new DerSet(new DerObjectIdentifier(ID_PKCS7_DATA)));
            attribute.Add(new DerSequence(v));
            v = new Asn1EncodableVector();
            v.Add(new DerObjectIdentifier(ID_SIGNING_TIME));
            v.Add(new DerSet(new DerUtcTime(signingTime)));
            attribute.Add(new DerSequence(v));
            v = new Asn1EncodableVector();
            v.Add(new DerObjectIdentifier(ID_MESSAGE_DIGEST));
            v.Add(new DerSet(new DerOctetString(secondDigest)));
            attribute.Add(new DerSequence(v));
            if (ocsp != null) {
                v = new Asn1EncodableVector();
                v.Add(new DerObjectIdentifier(ID_ADBE_REVOCATION));
                DerOctetString doctet = new DerOctetString(ocsp);
                Asn1EncodableVector vo1 = new Asn1EncodableVector();
                Asn1EncodableVector v2 = new Asn1EncodableVector();
                v2.Add(OcspObjectIdentifiers.PkixOcspBasic);
                v2.Add(doctet);
                DerEnumerated den = new DerEnumerated(0);
                Asn1EncodableVector v3 = new Asn1EncodableVector();
                v3.Add(den);
                v3.Add(new DerTaggedObject(true, 0, new DerSequence(v2)));
                vo1.Add(new DerSequence(v3));
                v.Add(new DerSet(new DerSequence(new DerTaggedObject(true, 1, new DerSequence(vo1)))));
                attribute.Add(new DerSequence(v));
            }
            return new DerSet(attribute);
        }

        
        public string Reason {
            get {
                return reason;
            }
            set {
                reason = value;
            }
        }

        
        public string Location {
            get {
                return location;
            }
            set {
                location = value;
            }
        }

        
        public DateTime SignDate {
            get {
                return signDate;
            }
            set {
                signDate = value;
            }
        }

        
        public string SignName {
            get {
                return signName;
            }
            set {
                signName = value;
            }
        }

        /**
        * a class that holds an X509 name
        */
        public class X509Name {
            /**
            * country code - StringType(SIZE(2))
            */
            public static DerObjectIdentifier C = new DerObjectIdentifier("2.5.4.6");

            /**
            * organization - StringType(SIZE(1..64))
            */
            public static DerObjectIdentifier O = new DerObjectIdentifier("2.5.4.10");

            /**
            * organizational unit name - StringType(SIZE(1..64))
            */
            public static DerObjectIdentifier OU = new DerObjectIdentifier("2.5.4.11");

            /**
            * Title
            */
            public static DerObjectIdentifier T = new DerObjectIdentifier("2.5.4.12");

            /**
            * common name - StringType(SIZE(1..64))
            */
            public static DerObjectIdentifier CN = new DerObjectIdentifier("2.5.4.3");

            /**
            * device serial number name - StringType(SIZE(1..64))
            */
            public static DerObjectIdentifier SN = new DerObjectIdentifier("2.5.4.5");

            /**
            * locality name - StringType(SIZE(1..64))
            */
            public static DerObjectIdentifier L = new DerObjectIdentifier("2.5.4.7");

            /**
            * state, or province name - StringType(SIZE(1..64))
            */
            public static DerObjectIdentifier ST = new DerObjectIdentifier("2.5.4.8");

            /** Naming attribute of type X520name */
            public static DerObjectIdentifier SURNAME = new DerObjectIdentifier("2.5.4.4");
            /** Naming attribute of type X520name */
            public static DerObjectIdentifier GIVENNAME = new DerObjectIdentifier("2.5.4.42");
            /** Naming attribute of type X520name */
            public static DerObjectIdentifier INITIALS = new DerObjectIdentifier("2.5.4.43");
            /** Naming attribute of type X520name */
            public static DerObjectIdentifier GENERATION = new DerObjectIdentifier("2.5.4.44");
            /** Naming attribute of type X520name */
            public static DerObjectIdentifier UNIQUE_IDENTIFIER = new DerObjectIdentifier("2.5.4.45");

            /**
            * Email address (RSA PKCS#9 extension) - IA5String.
            * <p>Note: if you're trying to be ultra orthodox, don't use this! It shouldn't be in here.
            */
            public static DerObjectIdentifier EmailAddress = new DerObjectIdentifier("1.2.840.113549.1.9.1");

            /**
            * email address in Verisign certificates
            */
            public static DerObjectIdentifier E = EmailAddress;

            /** object identifier */
            public static DerObjectIdentifier DC = new DerObjectIdentifier("0.9.2342.19200300.100.1.25");

            /** LDAP User id. */
            public static DerObjectIdentifier UID = new DerObjectIdentifier("0.9.2342.19200300.100.1.1");

            /** A Hashtable with default symbols */
            public static Dictionary<DerObjectIdentifier,string> DefaultSymbols = new Dictionary<DerObjectIdentifier,string>();
            
            static X509Name(){
                DefaultSymbols[C] = "C";
                DefaultSymbols[O] = "O";
                DefaultSymbols[T] = "T";
                DefaultSymbols[OU] = "OU";
                DefaultSymbols[CN] = "CN";
                DefaultSymbols[L] = "L";
                DefaultSymbols[ST] = "ST";
                DefaultSymbols[SN] = "SN";
                DefaultSymbols[EmailAddress] = "E";
                DefaultSymbols[DC] = "DC";
                DefaultSymbols[UID] = "UID";
                DefaultSymbols[SURNAME] = "SURNAME";
                DefaultSymbols[GIVENNAME] = "GIVENNAME";
                DefaultSymbols[INITIALS] = "INITIALS";
                DefaultSymbols[GENERATION] = "GENERATION";
            }
            /** A Hashtable with values */
            public Dictionary<string,List<string>> values = new Dictionary<string,List<string>>();

            /**
            * Constructs an X509 name
            * @param seq an Asn1 Sequence
            */
            public X509Name(Asn1Sequence seq) {
                IEnumerator e = seq.GetEnumerator();
                
                while (e.MoveNext()) {
                    Asn1Set sett = (Asn1Set)e.Current;
                    
                    for (int i = 0; i < sett.Count; i++) {
                        Asn1Sequence s = (Asn1Sequence)sett[i];
                        String id;
                        if (!(s[0] is DerObjectIdentifier) || !DefaultSymbols.TryGetValue((DerObjectIdentifier)s[0], out id))
                            continue;
                        List<string> vs;
                        if (!values.TryGetValue(id, out vs)) {
                            vs = new List<string>();
                            values[id] = vs;
                        }
                        vs.Add(((DerStringBase)s[1]).GetString());
                    }
                }
            }
            /**
            * Constructs an X509 name
            * @param dirName a directory name
            */
            public X509Name(String dirName) {
                X509NameTokenizer   nTok = new X509NameTokenizer(dirName);
                
                while (nTok.HasMoreTokens()) {
                    String  token = nTok.NextToken();
                    int index = token.IndexOf('=');
                    
                    if (index == -1) {
                        throw new ArgumentException(MessageLocalization.GetComposedMessage("badly.formated.directory.string"));
                    }
                    
                    String id = token.Substring(0, index).ToUpper(System.Globalization.CultureInfo.InvariantCulture);
                    String value = token.Substring(index + 1);
                    List<string> vs;
                    if (!values.TryGetValue(id, out vs)) {
                        vs = new List<string>();
                        values[id] = vs;
                    }
                    vs.Add(value);
                }                
            }
            
            public String GetField(String name) {
                List<string> vs;
                if (values.TryGetValue(name, out vs))
                    return vs.Count == 0 ? null : vs[0];
                else
                    return null;
            }

            /**
            * gets a field array from the values Hashmap
            * @param name
            * @return an ArrayList
            */
            public List<string> GetFieldArray(String name) {
                List<string> vs;
                if (values.TryGetValue(name, out vs))
                    return vs;
                else
                    return null;
            }
            
            /**
            * getter for values
            * @return a Hashtable with the fields of the X509 name
            */
            public Dictionary<string,List<string>> GetFields() {
                return values;
            }
            
            /**
            * @see java.lang.Object#toString()
            */
            public override String ToString() {
                return values.ToString();
            }
        }
        
        /**
        * class for breaking up an X500 Name into it's component tokens, ala
        * java.util.StringTokenizer. We need this class as some of the
        * lightweight Java environment don't support classes like
        * StringTokenizer.
        */
        public class X509NameTokenizer {
            private String          oid;
            private int             index;
            private StringBuilder    buf = new StringBuilder();
            
            public X509NameTokenizer(
            String oid) {
                this.oid = oid;
                this.index = -1;
            }
            
            public bool HasMoreTokens() {
                return (index != oid.Length);
            }
            
            public String NextToken() {
                if (index == oid.Length) {
                    return null;
                }
                
                int     end = index + 1;
                bool quoted = false;
                bool escaped = false;
                
                buf.Length = 0;
                
                while (end != oid.Length) {
                    char    c = oid[end];
                    
                    if (c == '"') {
                        if (!escaped) {
                            quoted = !quoted;
                        }
                        else {
                            buf.Append(c);
                        }
                        escaped = false;
                    }
                    else {
                        if (escaped || quoted) {
                            buf.Append(c);
                            escaped = false;
                        }
                        else if (c == '\\') {
                            escaped = true;
                        }
                        else if (c == ',') {
                            break;
                        }
                        else {
                            buf.Append(c);
                        }
                    }
                    end++;
                }
                
                index = end;
                return buf.ToString().Trim();
            }
        }
    }
}

