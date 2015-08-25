using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto.Parameters;
using iTextSharp.text.error_messages;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Tsp;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Ess;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Utilities;
/*
 * $Id: PdfPKCS7.java 5195 2012-06-18 14:25:30Z blowagie $
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

    /**
     * This class does all the processing related to signing
     * and verifying a PKCS#7 signature.
     */
    public class PdfPKCS7 {

        // Constructors for creating new signatures

        /**
         * Assembles all the elements needed to create a signature, except for the data.
         * @param privKey the private key
         * @param certChain the certificate chain
         * @param interfaceDigest the interface digest
         * @param hashAlgorithm the hash algorithm
         * @param provider the provider or <code>null</code> for the default provider
         * @param hasRSAdata <CODE>true</CODE> if the sub-filter is adbe.pkcs7.sha1
         * @throws InvalidKeyException on error
         * @throws NoSuchProviderException on error
         * @throws NoSuchAlgorithmException on error
         */
        public PdfPKCS7(ICipherParameters privKey, ICollection<X509Certificate> certChain, 
                        String hashAlgorithm, bool hasRSAdata) {
            
            digestAlgorithmOid = DigestAlgorithms.GetAllowedDigests(hashAlgorithm);
            if (digestAlgorithmOid == null)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("unknown.hash.algorithm.1", hashAlgorithm));
            
            version = signerversion = 1;
            certs = new List<X509Certificate>(certChain);
            crls = new List<X509Crl>();
            digestalgos = new Dictionary<string,object>();
            digestalgos[digestAlgorithmOid] = null;
            
            //
            // Copy in the certificates and crls used to sign the private key.
            //
            signCert = certs[0];
                        
            if (privKey != null) {
                //
                // Now we have private key, find out what the digestEncryptionAlgorithm is.
                //
                if (privKey is RsaKeyParameters)
                    digestEncryptionAlgorithmOid = SecurityIDs.ID_RSA;
                else if (privKey is DsaKeyParameters)
                    digestEncryptionAlgorithmOid = SecurityIDs.ID_DSA;
                else
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("unknown.key.algorithm.1", privKey.ToString()));

            }
            if (hasRSAdata) {
                RSAdata = new byte[0];
                messageDigest = GetHashClass();
            }

            if (privKey != null) {
                sig = InitSignature(privKey);
            }
        }

        internal IDigest GetHashClass() {
            return DigestUtilities.GetDigest(GetHashAlgorithm());
        }

        // Constructors for validating existing signatures

        /**
         * Use this constructor if you want to verify a signature using the sub-filter adbe.x509.rsa_sha1.
         * @param contentsKey the /Contents key
         * @param certsKey the /Cert key
         */
        public PdfPKCS7(byte[] contentsKey, byte[] certsKey) {
            X509CertificateParser cf = new X509CertificateParser();
            certs = new List<X509Certificate>();

            foreach (X509Certificate cc in cf.ReadCertificates(certsKey)) {
                if (signCert != null)
                    signCert = cc;
                certs.Add(cc);
            }

            signCerts = certs;
            crls = new List<X509Crl>();

            Asn1InputStream inp = new Asn1InputStream(new MemoryStream(contentsKey));
            digest = ((Asn1OctetString)inp.ReadObject()).GetOctets();

            sig = SignerUtilities.GetSigner("SHA1withRSA");
            sig.Init(false, signCert.GetPublicKey());

            // setting the oid to SHA1withRSA
            digestAlgorithmOid = "1.2.840.10040.4.3";
            digestEncryptionAlgorithmOid = "1.3.36.3.3.1.2";
        }

        /**
         * Use this constructor if you want to verify a signature.
         * @param contentsKey the /Contents key
         * @param filterSubtype the filtersubtype
         * @param provider the provider or <code>null</code> for the default provider
         */
        public PdfPKCS7(byte[] contentsKey, PdfName filterSubtype) {
            this.filterSubtype = filterSubtype;
            isTsp = PdfName.ETSI_RFC3161.Equals(filterSubtype);
            isCades = PdfName.ETSI_CADES_DETACHED.Equals(filterSubtype);
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
            if (!objId.Id.Equals(SecurityIDs.ID_PKCS7_SIGNED_DATA))
                throw new ArgumentException(MessageLocalization.GetComposedMessage("not.a.valid.pkcs.7.object.not.signed.data"));
            Asn1Sequence content = (Asn1Sequence)((Asn1TaggedObject)signedData[1]).GetObject();
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
                Asn1OctetString rsaDataContent = (Asn1OctetString)((Asn1TaggedObject)rsaData[1]).GetObject();
                RSAdata = rsaDataContent.GetOctets();
            }
            
            // the signerInfos
            int next = 3;
            while (content[next] is Asn1TaggedObject)
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
            digestAlgorithmOid = ((DerObjectIdentifier)((Asn1Sequence)signerInfo[2])[0]).Id;
            next = 3;
            if (signerInfo[next] is Asn1TaggedObject) {
                Asn1TaggedObject tagsig = (Asn1TaggedObject)signerInfo[next];
                Asn1Set sseq = Asn1Set.GetInstance(tagsig, false);
                sigAttr = sseq.GetEncoded();
                // maybe not necessary, but we use the following line as fallback:
                sigAttrDer = sseq.GetEncoded(Asn1Encodable.Der);
                
                for (int k = 0; k < sseq.Count; ++k) {
                    Asn1Sequence seq2 = (Asn1Sequence)sseq[k];
                    String idSeq2 = ((DerObjectIdentifier)seq2[0]).Id;
                    if (idSeq2.Equals(SecurityIDs.ID_MESSAGE_DIGEST)) {
                        Asn1Set sset = (Asn1Set)seq2[1];
                        digestAttr = ((DerOctetString)sset[0]).GetOctets();
                    }
                    else if (idSeq2.Equals(SecurityIDs.ID_ADBE_REVOCATION)) {
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
                    else if (isCades && idSeq2.Equals(SecurityIDs.ID_AA_SIGNING_CERTIFICATE_V1)) {
                        Asn1Set setout = (Asn1Set)seq2[1];
                        Asn1Sequence seqout = (Asn1Sequence)setout[0];
                        SigningCertificate sv2 = Org.BouncyCastle.Asn1.Ess.SigningCertificate.GetInstance(seqout);
                        EssCertID[] cerv2m = sv2.GetCerts();
                        EssCertID cerv2 = cerv2m[0];
                        byte[] enc2 = signCert.GetEncoded();
                        IDigest m2 = DigestUtilities.GetDigest("SHA-1");
                        byte[] signCertHash = DigestAlgorithms.Digest(m2, enc2);
                        byte[] hs2 = cerv2.GetCertHash();
                        if (!Arrays.AreEqual(signCertHash, hs2))
                            throw new ArgumentException("Signing certificate doesn't match the ESS information.");
                    }
                    else if (isCades && idSeq2.Equals(SecurityIDs.ID_AA_SIGNING_CERTIFICATE_V2)) {
                        Asn1Set setout = (Asn1Set)seq2[1];
                        Asn1Sequence seqout = (Asn1Sequence)setout[0];
                        SigningCertificateV2 sv2 = SigningCertificateV2.GetInstance(seqout);
                        EssCertIDv2[] cerv2m = sv2.GetCerts();
                        EssCertIDv2 cerv2 = cerv2m[0];
                        AlgorithmIdentifier ai2 = cerv2.HashAlgorithm;
                        byte[] enc2 = signCert.GetEncoded();
                        IDigest m2 = DigestUtilities.GetDigest(ai2.ObjectID.Id);
                        byte[] signCertHash = DigestAlgorithms.Digest(m2, enc2);
                        byte[] hs2 = cerv2.GetCertHash();
                        if (!Arrays.AreEqual(signCertHash, hs2))
                            throw new ArgumentException("Signing certificate doesn't match the ESS information.");
                    }
                }
                if (digestAttr == null)
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("authenticated.attribute.is.missing.the.digest"));
                ++next;
            }
            digestEncryptionAlgorithmOid = ((DerObjectIdentifier)((Asn1Sequence)signerInfo[next++])[0]).Id;
            digest = ((Asn1OctetString)signerInfo[next++]).GetOctets();
            if (next < signerInfo.Count && (signerInfo[next] is DerTaggedObject)) {
                Asn1TaggedObject taggedObject = (Asn1TaggedObject) signerInfo[next];
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
            if (isTsp) {
                Org.BouncyCastle.Asn1.Cms.ContentInfo contentInfoTsp = Org.BouncyCastle.Asn1.Cms.ContentInfo.GetInstance(signedData);
                this.timeStampToken = new TimeStampToken(contentInfoTsp);
                TimeStampTokenInfo info = timeStampToken.TimeStampInfo;
                String algOID = info.MessageImprintAlgOid;
                messageDigest = DigestUtilities.GetDigest(algOID);
            }
            else {
                if (RSAdata != null || digestAttr != null) {
                    if (PdfName.ADBE_PKCS7_SHA1.Equals(GetFilterSubtype())) {
                        messageDigest = DigestUtilities.GetDigest("SHA1");
                    }
                    else {
                        messageDigest = GetHashClass();
                    }
                    encContDigest = GetHashClass();
                }
                sig = InitSignature(signCert.GetPublicKey());
            }
        }
        
        
        // Signature info

        /** Holds value of property signName. */
        private String signName;

        /** Holds value of property reason. */
        private String reason;

        /** Holds value of property location. */
        private String location;

        /** Holds value of property signDate. */
        private DateTime signDate;

        /**
         * Getter/setter for property sigName.
         * @return Value of property sigName.
         */
        virtual public string SignName {
            get {
                return signName;
            }
            set {
                signName = value;
            }
        }

        /**
         * Getter for property reason.
         * @return Value of property reason.
         */
        virtual public string Reason {
            get {
                return reason;
            }
            set {
                reason = value;
            }
        }

        /**
         * Getter for property location.
         * @return Value of property location.
         */
        virtual public string Location {
            get {
                return location;
            }
            set {
                location = value;
            }
        }

        /**
         * Getter for property signDate.
         * @return Value of property signDate.
         */
        virtual public DateTime SignDate {
            get {
                return signDate;
            }
            set {
                signDate = value;
            }
        }

        // version info
        
        /** Version of the PKCS#7 object */
        private int version = 1;
        
        /** Version of the PKCS#7 "SignerInfo" object. */
        private int signerversion = 1;
        
        /**
         * Get the version of the PKCS#7 object.
         * @return the version of the PKCS#7 object.
         */
        virtual public int Version {
            get {
                return version;
            }
        }

        /**
         * Get the version of the PKCS#7 "SignerInfo" object.
         * @return the version of the PKCS#7 "SignerInfo" object.
         */
        virtual public int SigningInfoVersion {
            get {
                return signerversion;
            }
        }
        
        // Message digest algorithm

        /** The ID of the digest algorithm, e.g. "2.16.840.1.101.3.4.2.1". */
        private String digestAlgorithmOid;
        
        /** The object that will create the digest */
        private IDigest messageDigest;
        
        /** The digest algorithms */
        private Dictionary<string,object> digestalgos;

        /** The digest attributes */
        private byte[] digestAttr;

        private PdfName filterSubtype;

        /**
         * Getter for the ID of the digest algorithm, e.g. "2.16.840.1.101.3.4.2.1"
         */
        virtual public String DigestAlgorithmOid {
            get {
                return digestAlgorithmOid;
            }
        }

        /**
         * Returns the name of the digest algorithm, e.g. "SHA256".
         * @return the digest algorithm name, e.g. "SHA256"
         */
        virtual public String GetHashAlgorithm() {
            return DigestAlgorithms.GetDigest(digestAlgorithmOid);
        }
        
        // Encryption algorithm
        
        /** The encryption algorithm. */
        private String digestEncryptionAlgorithmOid;

        /**
         * Getter for the digest encryption algorithm
         */
        virtual public String DigestEncryptionAlgorithmOid {
            get {
                return digestEncryptionAlgorithmOid;
            }
        }
       
        /**
         * Get the algorithm used to calculate the message digest, e.g. "SHA1withRSA".
         * @return the algorithm used to calculate the message digest
         */
        virtual public String GetDigestAlgorithm() {
            return GetHashAlgorithm() + "with" + GetEncryptionAlgorithm();
        }

        /*
         *  DIGITAL SIGNATURE CREATION
         */

        // The signature is created externally
        
        /** The signed digest if created outside this class */   
        private byte[] externalDigest;
        
        /** External RSA data */
        private byte[] externalRSAdata;
        
        /**
         * Sets the digest/signature to an external calculated value.
         * @param digest the digest. This is the actual signature
         * @param RSAdata the extra data that goes into the data tag in PKCS#7
         * @param digestEncryptionAlgorithm the encryption algorithm. It may must be <CODE>null</CODE> if the <CODE>digest</CODE>
         * is also <CODE>null</CODE>. If the <CODE>digest</CODE> is not <CODE>null</CODE>
         * then it may be "RSA" or "DSA"
         */
        virtual public void SetExternalDigest(byte[] digest, byte[] RSAdata, String digestEncryptionAlgorithm) {
            externalDigest = digest;
            externalRSAdata = RSAdata;
            if (digestEncryptionAlgorithm != null) {
                if (digestEncryptionAlgorithm.Equals("RSA")) {
                    this.digestEncryptionAlgorithmOid = SecurityIDs.ID_RSA;
                }
                else if (digestEncryptionAlgorithm.Equals("DSA")) {
                    this.digestEncryptionAlgorithmOid = SecurityIDs.ID_DSA;
                }
                else if (digestEncryptionAlgorithm.Equals("ECDSA"))
                {
                    this.digestEncryptionAlgorithmOid = SecurityIDs.ID_ECDSA;
                }
                else
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("unknown.key.algorithm.1", digestEncryptionAlgorithm));
            }
        }
        
        // The signature is created internally
        
        /** Class from the Java SDK that provides the functionality of a digital signature algorithm. */
        private ISigner sig;
        
        /** The signed digest as calculated by this class (or extracted from an existing PDF) */
        private byte[] digest;
        
        /** The RSA data */
        private byte[] RSAdata;

        // Signing functionality.
        
        private ISigner InitSignature(ICipherParameters key) {
            ISigner signature = SignerUtilities.GetSigner(GetDigestAlgorithm());
            signature.Init(true, key);
            return signature;
        }
    
        private ISigner InitSignature(AsymmetricKeyParameter key) {
            String digestAlgorithm = GetDigestAlgorithm();
            if (PdfName.ADBE_X509_RSA_SHA1.Equals(GetFilterSubtype()))
                digestAlgorithm = "SHA1withRSA";
            ISigner signature = SignerUtilities.GetSigner(digestAlgorithm);
            signature.Init(false, signCert.GetPublicKey());
            return signature;
        }
    
        /**
         * Update the digest with the specified bytes.
         * This method is used both for signing and verifying
         * @param buf the data buffer
         * @param off the offset in the data buffer
         * @param len the data length
         * @throws SignatureException on error
         */
        virtual public void Update(byte[] buf, int off, int len) {
            if (RSAdata != null || digestAttr != null || isTsp)
                messageDigest.BlockUpdate(buf, off, len);
            else
                sig.BlockUpdate(buf, off, len);
        }

        // adbe.x509.rsa_sha1 (PKCS#1)
        
        /**
         * Gets the bytes for the PKCS#1 object.
         * @return a byte array
         */
        virtual public byte[] GetEncodedPKCS1() {
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

        // other subfilters (PKCS#7)
        
        /**
         * Gets the bytes for the PKCS7SignedData object.
         * @return the bytes for the PKCS7SignedData object
         */
        virtual public byte[] GetEncodedPKCS7() {
            return GetEncodedPKCS7(null, null, null, null, CryptoStandard.CMS);
        }

        /**
         * Gets the bytes for the PKCS7SignedData object. Optionally the authenticatedAttributes
         * in the signerInfo can also be set. If either of the parameters is <CODE>null</CODE>, none will be used.
         * @param secondDigest the digest in the authenticatedAttributes
         * @return the bytes for the PKCS7SignedData object
         */
        virtual public byte[] GetEncodedPKCS7(byte[] secondDigest) {
            return GetEncodedPKCS7(secondDigest, null, null, null, CryptoStandard.CMS);
        }

        /**
         * Gets the bytes for the PKCS7SignedData object. Optionally the authenticatedAttributes
         * in the signerInfo can also be set, OR a time-stamp-authority client
         * may be provided.
         * @param secondDigest the digest in the authenticatedAttributes
         * @param tsaClient TSAClient - null or an optional time stamp authority client
         * @return byte[] the bytes for the PKCS7SignedData object
         * @since   2.1.6
         */
        virtual public byte[] GetEncodedPKCS7(byte[] secondDigest, ITSAClient tsaClient, byte[] ocsp, ICollection<byte[]> crlBytes, CryptoStandard sigtype) {
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
            v.Add(new DerObjectIdentifier(SecurityIDs.ID_PKCS7_DATA));
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
            v.Add(CertificateInfo.GetIssuer(signCert.GetTbsCertificate()));
            v.Add(new DerInteger(signCert.SerialNumber));
            signerinfo.Add(new DerSequence(v));
            
            // Add the digestAlgorithm
            v = new Asn1EncodableVector();
            v.Add(new DerObjectIdentifier(digestAlgorithmOid));
            v.Add(DerNull.Instance);
            signerinfo.Add(new DerSequence(v));
            
            // add the authenticated attribute if present
            if (secondDigest != null) {
                signerinfo.Add(new DerTaggedObject(false, 0, GetAuthenticatedAttributeSet(secondDigest, ocsp, crlBytes, sigtype)));
            }
            // Add the digestEncryptionAlgorithm
            v = new Asn1EncodableVector();
            v.Add(new DerObjectIdentifier(digestEncryptionAlgorithmOid));
            v.Add(DerNull.Instance);
            signerinfo.Add(new DerSequence(v));
            
            // Add the digest
            signerinfo.Add(new DerOctetString(digest));
            
            // When requested, go get and add the timestamp. May throw an exception.
            // Added by Martin Brunecky, 07/12/2007 folowing Aiken Sam, 2006-11-15
            // Sam found Adobe expects time-stamped SHA1-1 of the encrypted digest
            if (tsaClient != null) {
                byte[] tsImprint = DigestAlgorithms.Digest(tsaClient.GetMessageDigest(), digest);
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
                        
            // Only allow one signerInfo
            body.Add(new DerSet(new DerSequence(signerinfo)));
            
            // Now we have the body, wrap it in it's PKCS7Signed shell
            // and return it
            //
            Asn1EncodableVector whole = new Asn1EncodableVector();
            whole.Add(new DerObjectIdentifier(SecurityIDs.ID_PKCS7_SIGNED_DATA));
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

        // Authenticated attributes
        
        /**
         * When using authenticatedAttributes the authentication process is different.
         * The document digest is generated and put inside the attribute. The signing is done over the DER encoded
         * authenticatedAttributes. This method provides that encoding and the parameters must be
         * exactly the same as in {@link #getEncodedPKCS7(byte[],Calendar)}.
         * <p>
         * A simple example:
         * <p>
         * <pre>
         * Calendar cal = Calendar.getInstance();
         * PdfPKCS7 pk7 = new PdfPKCS7(key, chain, null, "SHA1", null, false);
         * MessageDigest messageDigest = MessageDigest.getInstance("SHA1");
         * byte buf[] = new byte[8192];
         * int n;
         * InputStream inp = sap.getRangeStream();
         * while ((n = inp.read(buf)) &gt; 0) {
         *    messageDigest.update(buf, 0, n);
         * }
         * byte hash[] = messageDigest.digest();
         * byte sh[] = pk7.getAuthenticatedAttributeBytes(hash, cal);
         * pk7.update(sh, 0, sh.length);
         * byte sg[] = pk7.getEncodedPKCS7(hash, cal);
         * </pre>
         * @param secondDigest the content digest
         * @return the byte array representation of the authenticatedAttributes ready to be signed
         */
        virtual public byte[] getAuthenticatedAttributeBytes(byte[] secondDigest, byte[] ocsp, ICollection<byte[]> crlBytes, CryptoStandard sigtype) {
            return GetAuthenticatedAttributeSet(secondDigest, ocsp, crlBytes, sigtype).GetEncoded(Asn1Encodable.Der);
        }

        /**
         * This method provides that encoding and the parameters must be
         * exactly the same as in {@link #getEncodedPKCS7(byte[],Calendar)}.
         * 
         * @param secondDigest the content digest
         * @return the byte array representation of the authenticatedAttributes ready to be signed
         */
        private DerSet GetAuthenticatedAttributeSet(byte[] secondDigest, byte[] ocsp, ICollection<byte[]> crlBytes, CryptoStandard sigtype) {
            Asn1EncodableVector attribute = new Asn1EncodableVector();
            Asn1EncodableVector v = new Asn1EncodableVector();
            v.Add(new DerObjectIdentifier(SecurityIDs.ID_CONTENT_TYPE));
            v.Add(new DerSet(new DerObjectIdentifier(SecurityIDs.ID_PKCS7_DATA)));
            attribute.Add(new DerSequence(v));
            v = new Asn1EncodableVector();
            v.Add(new DerObjectIdentifier(SecurityIDs.ID_MESSAGE_DIGEST));
            v.Add(new DerSet(new DerOctetString(secondDigest)));
            attribute.Add(new DerSequence(v));

            bool haveCrl = false;
            if (crlBytes != null) {
                foreach (byte[] bCrl in crlBytes) {
                    if (bCrl != null) {
                        haveCrl = true;
                        break;
                    }
                }
            }
            if (ocsp != null || haveCrl) {
                v = new Asn1EncodableVector();
                v.Add(new DerObjectIdentifier(SecurityIDs.ID_ADBE_REVOCATION));

                Asn1EncodableVector revocationV = new Asn1EncodableVector();

                if (haveCrl) {
                    Asn1EncodableVector v2 = new Asn1EncodableVector();
                    foreach (byte[] bCrl in crlBytes) {
                        if (bCrl == null)
                            continue;
                        Asn1InputStream t = new Asn1InputStream(bCrl);
                        v2.Add(t.ReadObject());
                    }
                    revocationV.Add(new DerTaggedObject(true, 0, new DerSequence(v2)));
                }

                if (ocsp != null) {
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
                    revocationV.Add(new DerTaggedObject(true, 1, new DerSequence(vo1)));
                }

                v.Add(new DerSet(new DerSequence(revocationV)));
                attribute.Add(new DerSequence(v));
            }
            if (sigtype == CryptoStandard.CADES) {
                v = new Asn1EncodableVector();
                v.Add(new DerObjectIdentifier(SecurityIDs.ID_AA_SIGNING_CERTIFICATE_V2));

                Asn1EncodableVector aaV2 = new Asn1EncodableVector();
                String sha256Oid = DigestAlgorithms.GetAllowedDigests(DigestAlgorithms.SHA256);

                // If we look into X.690-0207, clause 11.5, we can see that using DER all the components of a sequence having
                // default values shall not be included. According to RFC 5035, 5.4.1.1, definition of ESSCertIDv2, default
                // AlgorithmIdentifier is sha256.
                if (!sha256Oid.Equals(digestAlgorithmOid)) {
                    AlgorithmIdentifier algoId = new AlgorithmIdentifier(new DerObjectIdentifier(digestAlgorithmOid));
                    aaV2.Add(algoId);
                }

                byte[] dig = DigestAlgorithms.Digest(GetHashAlgorithm(), signCert.GetEncoded()); 
                aaV2.Add(new DerOctetString(dig));
                
                v.Add(new DerSet(new DerSequence(new DerSequence(new DerSequence(aaV2)))));
                attribute.Add(new DerSequence(v));
            }

            return new DerSet(attribute);
        }
        
        /*
         *  DIGITAL SIGNATURE VERIFICATION
         */
        
        /** Signature attributes */
        private byte[] sigAttr;
        /** Signature attributes (maybe not necessary, but we use it as fallback) */
        private byte[] sigAttrDer;
        
        /** encrypted digest */
        private IDigest encContDigest; // Stefan Santesson
        
        /** Indicates if a signature has already been verified */
        private bool verified;
        
        /** The result of the verification */
        private bool verifyResult;

        
        // verification
        
        /**
         * Verify the digest.
         * @throws SignatureException on error
         * @return <CODE>true</CODE> if the signature checks out, <CODE>false</CODE> otherwise
         */
        virtual public bool Verify() {
            if (verified)
                return verifyResult;
            if (isTsp) {
                TimeStampTokenInfo info = timeStampToken.TimeStampInfo;
                MessageImprint imprint = info.TstInfo.MessageImprint;
                byte[] md = new byte[messageDigest.GetDigestSize()];
                messageDigest.DoFinal(md, 0);
                byte[] imphashed = imprint.GetHashedMessage();
                verifyResult = Arrays.AreEqual(md, imphashed);
            }
            else {
                if (sigAttr != null || sigAttrDer != null) {
                    byte[] msgDigestBytes = new byte[messageDigest.GetDigestSize()];
                    messageDigest.DoFinal(msgDigestBytes, 0);
                    bool verifyRSAdata = true;
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
                    bool sigVerify = VerifySigAttributes(sigAttr) || VerifySigAttributes(sigAttrDer);
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
            }
            verified = true;
            return verifyResult;
        }

        private bool VerifySigAttributes(byte[] attr) {
            ISigner signature = InitSignature(signCert.GetPublicKey());
            signature.BlockUpdate(attr, 0, attr.Length);
            return signature.VerifySignature(digest);
        }

        /**
         * Checks if the timestamp refers to this document.
         * @throws java.security.NoSuchAlgorithmException on error
         * @return true if it checks false otherwise
         * @since   2.1.6
         */
        virtual public bool VerifyTimestampImprint() {
            if (timeStampToken == null)
                return false;
            TimeStampTokenInfo info = timeStampToken.TimeStampInfo;
            MessageImprint imprint = info.TstInfo.MessageImprint;
            String algOID = info.MessageImprintAlgOid;
            byte[] md = DigestAlgorithms.Digest(algOID, digest);
            byte[] imphashed = imprint.GetHashedMessage();
            bool res = Arrays.AreEqual(md, imphashed);
            return res;
        }

        // Certificates
        
        /** All the X.509 certificates in no particular order. */
        private List<X509Certificate> certs;
        
        /** All the X.509 certificates used for the main signature. */
        private ICollection<X509Certificate> signCerts;

        /** The X.509 certificate that is used to sign the digest. */
        private X509Certificate signCert;
        
        /**
         * Get all the X.509 certificates associated with this PKCS#7 object in no particular order.
         * Other certificates, from OCSP for example, will also be included.
         * @return the X.509 certificates associated with this PKCS#7 object
         */
        virtual public X509Certificate[] Certificates {
            get {
                X509Certificate[] c = new X509Certificate[certs.Count];
                certs.CopyTo(c, 0);
                return c;
            }
        }

        /**
         * Get the X.509 sign certificate chain associated with this PKCS#7 object.
         * Only the certificates used for the main signature will be returned, with
         * the signing certificate first.
         * @return the X.509 certificates associated with this PKCS#7 object
         * @since   2.1.6
         */
        virtual public X509Certificate[] SignCertificateChain {
            get {
                X509Certificate[] ret = new X509Certificate[signCerts.Count];
                signCerts.CopyTo(ret, 0);
                return ret;
            }
        }
                
        /**
         * Get the X.509 certificate actually used to sign the digest.
         * @return the X.509 certificate actually used to sign the digest
         */
        virtual public X509Certificate SigningCertificate {
            get {
                return signCert;
            }
        }

        /**
         * Helper method that creates the collection of certificates
         * used for the main signature based on the complete list
         * of certificates and the sign certificate.
         */
        private void CalcSignCertificateChain() {
            List<X509Certificate> cc = new List<X509Certificate>();
            cc.Add(signCert);
            List<X509Certificate> oc = new List<X509Certificate>(certs);
            for (int k = 0; k < oc.Count; ++k) {
                if (signCert.Equals(oc[k])) {
                    oc.RemoveAt(k);
                    --k;
                }
            }
            bool found = true;
            while (found) {
                X509Certificate v = cc[cc.Count - 1];
                found = false;
                for (int k = 0; k < oc.Count; ++k) {
                    X509Certificate issuer = (X509Certificate)oc[k];
                    try {
                        v.Verify(issuer.GetPublicKey());
                        found = true;
                        cc.Add(issuer);
                        oc.RemoveAt(k);
                        break;
                    }
                    catch {
                    }
                }
            }
            signCerts = cc;
        }
        
        // Certificate Revocation Lists

        private ICollection<X509Crl> crls;

        /**
         * Get the X.509 certificate revocation lists associated with this PKCS#7 object
         * @return the X.509 certificate revocation lists associated with this PKCS#7 object
         */
        virtual public ICollection<X509Crl> CRLs {
            get {
                return crls;
            }
        }

        /**
         * Helper method that tries to construct the CRLs.
         */
        private void FindCRL(Asn1Sequence seq) {
            crls = new List<X509Crl>();
            for (int k = 0; k < seq.Count; ++k) {
                X509CrlParser pp = new X509CrlParser();
                X509Crl crl = pp.ReadCrl(seq[k].GetDerEncoded());
                crls.Add(crl);
            }
        }
        
        // Online Certificate Status Protocol

        /** BouncyCastle BasicOCSPResp */
        private BasicOcspResp basicResp;

        /**
         * Gets the OCSP basic response if there is one.
         * @return the OCSP basic response or null
         * @since   2.1.6
         */
        virtual public BasicOcspResp Ocsp {
            get {
                return basicResp;
            }
        }

        /**
         * Checks if OCSP revocation refers to the document signing certificate.
         * @return true if it checks, false otherwise
         * @since   2.1.6
         */
        virtual public bool IsRevocationValid() {
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
                CertificateID tis = new CertificateID(cid.HashAlgOid, isscer, sigcer.SerialNumber);
                return tis.Equals(cid);
            }
            catch {
            }
            return false;
        }

        /**
         * Helper method that creates the BasicOCSPResp object.
         * @param seq
         * @throws IOException
         */
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
            Asn1OctetString os = (Asn1OctetString)seq[1];
            Asn1InputStream inp = new Asn1InputStream(os.GetOctets());
            BasicOcspResponse resp = BasicOcspResponse.GetInstance(inp.ReadObject());
            basicResp = new BasicOcspResp(resp);
        }
        
        // Time Stamps

        /** True if there's a PAdES LTV time stamp. */
        private bool isTsp;
        
        private bool isCades;

        /** BouncyCastle TimeStampToken. */
        private TimeStampToken timeStampToken;

        /**
         * Check if it's a PAdES-LTV time stamp.
         * @return true if it's a PAdES-LTV time stamp, false otherwise
         */
        virtual public bool IsTsp {
            get {
               return isTsp;
            }
        }

        /**
         * Gets the timestamp token if there is one.
         * @return the timestamp token or null
         * @since   2.1.6
         */
        virtual public TimeStampToken TimeStampToken {
            get {
                return timeStampToken;
            }
        }

        /**
         * Gets the timestamp date
         * @return  a date
         * @since   2.1.6
         */
        virtual public DateTime TimeStampDate {
            get {
                if (timeStampToken == null)
                    return DateTime.MaxValue;
                return timeStampToken.TimeStampInfo.GenTime;
            }
        }


        /**
         * Returns the filter subtype.
         */
        virtual public PdfName GetFilterSubtype()
        {
            return filterSubtype;
        }

        /**
         * Returns the encryption algorithm
         * @return	the name of an encryption algorithm
         */
        virtual public String GetEncryptionAlgorithm()
        {
            String encryptAlgo = EncryptionAlgorithms.GetAlgorithm(digestEncryptionAlgorithmOid);
            if (encryptAlgo == null)
                encryptAlgo = digestEncryptionAlgorithmOid;
            return encryptAlgo;
        }
    }
}
