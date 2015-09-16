using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.XPath;
using Org.BouncyCastle.X509;
using iTextSharp.text.error_messages;
using iTextSharp.text.exceptions;

/*
 * $Id: MakeXmlSignature.java 5830 2013-05-31 09:29:15Z blowagie $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Pavel Alay, Bruno Lowagie, et al.
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
namespace iTextSharp.text.pdf.security
{
    /**
     * Class that signs your XML.
     */
    public class MakeXmlSignature
    {
        /**
         * Signs the xml using the enveloped mode, with optional xpath transform (see XmlSignatureAppearance).
         * @param sap the XmlSignatureAppearance
         * @param externalSignature  the interface providing the actual signing
         * @param keyInfo KeyInfo for verification
         * @throws GeneralSecurityException
         * @throws IOException
         * @throws DocumentException
         */
        public static void SignXmlDSig(XmlSignatureAppearance sap, IExternalSignature externalSignature, KeyInfoClause keyInfo) {

            VerifyArguments(sap, externalSignature);
            List<XmlElement> references = new List<XmlElement>(1);
            references.Add(GenerateContentReference(sap.GetXmlLocator().GetDocument(), sap, null));
                
            XmlElement signature = GenerateSignatureElement(sap.GetXmlLocator(), null, false);
            Sign(signature, sap.GetXmlLocator(), externalSignature, references, null, keyInfo);
            sap.Close();    
        }

        /**
         * Signs the xml with XAdES BES using the enveloped mode, with optional xpath transform (see XmlSignatureAppearance).
         * @param sap the XmlSignatureAppearance
         * @param externalSignature  the interface providing the actual signing
         * @param chain the certificate chain
         * @param includeSignaturePolicy if true SignaturePolicyIdentifier will be included (XAdES-EPES)
         * @throws GeneralSecurityException
         * @throws IOException
         * @throws DocumentException
         */
        public static void SignXades(XmlSignatureAppearance sap, IExternalSignature externalSignature, X509Certificate[] chain,
            bool includeSignaturePolicy) {

            VerifyArguments(sap, externalSignature);
            String contentReferenceId = SecurityConstants.Reference_ + GetRandomId();
            String signedPropertiesId = SecurityConstants.SignedProperties_ + GetRandomId();
            String signatureId = SecurityConstants.Signature_ + GetRandomId();

            XmlDocument doc = sap.GetXmlLocator().GetDocument();
            KeyInfoClause keyInfo = GenerateKeyInfo(chain, sap);
            List<XmlElement> references = new List<XmlElement>(2);
            
            XmlElement signature = GenerateSignatureElement(sap.GetXmlLocator(), signatureId, true);
            String[] signaturePolicy = null;
            if(includeSignaturePolicy) {
                signaturePolicy = new String[2];
                if(externalSignature.GetEncryptionAlgorithm().Equals(SecurityConstants.RSA)) {
                    signaturePolicy[0] = SecurityConstants.OID_RSA_SHA1;
                    signaturePolicy[1] = SecurityConstants.OID_RSA_SHA1_DESC;
                }
                else {
                    signaturePolicy[0] = SecurityConstants.OID_DSA_SHA1;
                    signaturePolicy[1] = SecurityConstants.OID_DSA_SHA1_DESC;
                }
            }

            XmlElement signedProperty;
            XmlElement dsObject = GenerateXadesObject(sap, signatureId, contentReferenceId, signedPropertiesId, signaturePolicy, out signedProperty);
            
            references.Add(GenerateCustomReference(doc, signedProperty, "#" + signedPropertiesId, SecurityConstants.SignedProperties_Type, null));
            references.Add(GenerateContentReference(doc, sap, contentReferenceId));

            Sign(signature, sap.GetXmlLocator(), externalSignature, references, dsObject, keyInfo);

            sap.Close();
        }

        /**
         * Signs the xml with XAdES BES using the enveloped mode, with optional xpath transform (see XmlSignatureAppearance).
         * @param sap the XmlSignatureAppearance
         * @param externalSignature  the interface providing the actual signing
         * @param chain the certificate chain
         * @throws GeneralSecurityException
         * @throws IOException
         * @throws DocumentException
         */
        public static void SignXadesBes(XmlSignatureAppearance sap, IExternalSignature externalSignature, X509Certificate[] chain) {
            SignXades(sap, externalSignature, chain, false);
        }

        /**
         * Signs the xml with XAdES BES using the enveloped mode, with optional xpath transform (see XmlSignatureAppearance).
         * @param sap the XmlSignatureAppearance
         * @param externalSignature  the interface providing the actual signing
         * @param chain the certificate chain
         * @throws GeneralSecurityException
         * @throws IOException
         * @throws DocumentException
         */
        public static void SignXadesEpes(XmlSignatureAppearance sap, IExternalSignature externalSignature, X509Certificate[] chain) {
            SignXades(sap, externalSignature, chain, true);
        }

        /**
         * Signs the xml using the enveloped mode, with optional xpath transform (see XmlSignatureAppearance).
         * @param sap the XmlSignatureAppearance
         * @param externalSignature  the interface providing the actual signing
         * @param chain the certificate chain
         * @throws GeneralSecurityException
         * @throws IOException
         * @throws DocumentException
         */
        public static void SignXmlDSig(XmlSignatureAppearance sap,
            IExternalSignature externalSignature, X509Certificate[] chain) {
            SignXmlDSig(sap, externalSignature, new KeyInfoX509Data(chain[0].GetEncoded()));
        }

        /**
         * Signs the xml using the enveloped mode, with optional xpath transform (see XmlSignatureAppearance).
         * @param sap the XmlSignatureAppearance
         * @param externalSignature  the interface providing the actual signing
         * @param publicKey PublicKey for verification
         * @throws GeneralSecurityException
         * @throws IOException
         * @throws DocumentException
         */
        public static void SignXmlDSig(XmlSignatureAppearance sap,
            IExternalSignature externalSignature, AsymmetricAlgorithm publicKey) {
            SignXmlDSig(sap, externalSignature, GenerateKeyInfo(publicKey));
        }

        private static void VerifyArguments(XmlSignatureAppearance sap, IExternalSignature externalSignature) {
            if (sap.GetXmlLocator() == null)
                throw new DocumentException(MessageLocalization.GetComposedMessage("xmllocator.cannot.be.null"));
            if (!externalSignature.GetHashAlgorithm().Equals(SecurityConstants.SHA1))
                throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("support.only.sha1.hash.algorithm"));

            if (!externalSignature.GetEncryptionAlgorithm().Equals(SecurityConstants.RSA)
                && !externalSignature.GetEncryptionAlgorithm().Equals(SecurityConstants.DSA))
                throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("support.only.rsa.and.dsa.algorithms"));
        }

        private static void NormalizeNamespaces(XPathNavigator src, XPathNavigator dest) {
            IDictionary<string, string> dictLocal = src.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);
            IDictionary<string, string> dictExclude = dest.GetNamespacesInScope(XmlNamespaceScope.Local);

            foreach(KeyValuePair<string, string> pair in dictLocal)
                if(!dictExclude.ContainsKey(pair.Key))
                    dest.CreateAttribute("xmlns", pair.Key, "http://www.w3.org/2000/xmlns/", pair.Value);
        }

        private static byte[] CalculateC14nByteRange(XmlDocument doc) {
            XmlDsigC14NTransform c14nTransform = new XmlDsigC14NTransform();
            c14nTransform.LoadInput(doc);
            return ((System.IO.MemoryStream)c14nTransform.GetOutput()).ToArray();
        }

        private static byte[] CalculateC14nDigest(XmlDocument doc, HashAlgorithm hash) {
            return hash.ComputeHash(CalculateC14nByteRange(doc));
        }

        private static XmlElement GenerateSignatureElement(IXmlLocator locator, String signatureId, bool isXades) {
            XmlElement signature = locator.GetDocument().CreateElement("Signature", SecurityConstants.XMLDSIG_URI);
            if(signatureId != null)
                signature.SetAttribute("Id", signatureId);
            if (isXades)
                signature.CreateNavigator().CreateAttribute("xmlns", "xades", SecurityConstants.XMLNS_URI, SecurityConstants.XADES_132_URI);
            return signature;
        }

        private static KeyInfoClause GenerateKeyInfo(X509Certificate[] chain, XmlSignatureAppearance sap) {
            X509Certificate certificate = chain[0];
            sap.SetCertificate(certificate);
            // Create a KeyInfo and add the KeyValue to it
            return new KeyInfoX509Data(chain[0].GetEncoded());
        }

        private static KeyInfoClause GenerateKeyInfo(AsymmetricAlgorithm publicKey) {
            if (publicKey is DSA)
                return new DSAKeyValue((DSA)publicKey);
            else if(publicKey is RSA)
                return new RSAKeyValue((RSA)publicKey);
            else
                throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("support.only.rsa.and.dsa.algorithms"));
        }

        private static String GetRandomId() {
            return Guid.NewGuid().ToString().Substring(24);
        }

        private static XmlElement GenerateXadesObject(XmlSignatureAppearance sap, String signatureId,String contentReferenceId, String signedPropertiesId,
            String[] signaturePolicy, out XmlElement signedProperty) {

            HashAlgorithm md = new SHA1Managed();
            X509Certificate cert = sap.GetCertificate();

            XmlDocument doc = sap.GetXmlLocator().GetDocument();

            XmlElement dsObject = doc.CreateElement("Object", SecurityConstants.XMLDSIG_URI);

            XmlElement QualifyingProperties = doc.CreateElement(SecurityConstants.XADES_QualifyingProperties, SecurityConstants.XADES_132_URI);
            QualifyingProperties.SetAttribute("Target", "#"+signatureId);
            XmlElement SignedProperties = doc.CreateElement(SecurityConstants.XADES_SignedProperties, SecurityConstants.XADES_132_URI);
                SignedProperties.SetAttribute("Id", signedPropertiesId);
                    XmlElement SignedSignatureProperties = doc.CreateElement(SecurityConstants.XADES_SignedSignatureProperties, SecurityConstants.XADES_132_URI);
                        XmlElement SigningTime = doc.CreateElement(SecurityConstants.XADES_SigningTime, SecurityConstants.XADES_132_URI);
                            String result = sap.GetSignDate().ToString(SecurityConstants.SigningTimeFormat);
                        SigningTime.AppendChild(doc.CreateTextNode(result));
                    SignedSignatureProperties.AppendChild(SigningTime);
                        XmlElement SigningCertificate = doc.CreateElement(SecurityConstants.XADES_SigningCertificate, SecurityConstants.XADES_132_URI);
                            XmlElement Cert = doc.CreateElement(SecurityConstants.XADES_Cert, SecurityConstants.XADES_132_URI);
                                XmlElement CertDigest = doc.CreateElement(SecurityConstants.XADES_CertDigest, SecurityConstants.XADES_132_URI);
                                    XmlElement DigestMethod = doc.CreateElement(SecurityConstants.DigestMethod, SecurityConstants.XMLDSIG_URI);
                                    DigestMethod.SetAttribute(SecurityConstants.Algorithm, SecurityConstants.XMLDSIG_URI_SHA1);
                                CertDigest.AppendChild(DigestMethod);
                                    XmlElement DigestValue = doc.CreateElement(SecurityConstants.DigestValue, SecurityConstants.XMLDSIG_URI);
                                    DigestValue.AppendChild(doc.CreateTextNode(Convert.ToBase64String(md.ComputeHash(cert.GetEncoded()))));
                                CertDigest.AppendChild(DigestValue);
                            Cert.AppendChild(CertDigest);
                            XmlElement IssueSerial = doc.CreateElement(SecurityConstants.XADES_IssuerSerial, SecurityConstants.XADES_132_URI);
                                    XmlElement X509IssuerName = doc.CreateElement(SecurityConstants.X509IssuerName, SecurityConstants.XMLDSIG_URI);
                                    X509IssuerName.AppendChild(doc.CreateTextNode(GetX509IssuerName(cert)));
                                IssueSerial.AppendChild(X509IssuerName);
                                    XmlElement X509SerialNumber = doc.CreateElement(SecurityConstants.X509SerialNumber, SecurityConstants.XMLDSIG_URI);
                                    X509SerialNumber.AppendChild(doc.CreateTextNode(GetX509SerialNumber(cert)));
                                IssueSerial.AppendChild(X509SerialNumber);
                            Cert.AppendChild(IssueSerial);
                        SigningCertificate.AppendChild(Cert);
                    SignedSignatureProperties.AppendChild(SigningCertificate);
                    if(signaturePolicy != null) {
                        XmlElement SignaturePolicyIdentifier = doc.CreateElement(SecurityConstants.XADES_SignaturePolicyIdentifier, SecurityConstants.XADES_132_URI);
                            XmlElement SignaturePolicyId = doc.CreateElement(SecurityConstants.XADES_SignaturePolicyId, SecurityConstants.XADES_132_URI);
                                XmlElement SigPolicyId = doc.CreateElement(SecurityConstants.XADES_SigPolicyId, SecurityConstants.XADES_132_URI);
                                    XmlElement Identifier = doc.CreateElement(SecurityConstants.XADES_Identifier, SecurityConstants.XADES_132_URI);
                                    Identifier.AppendChild(doc.CreateTextNode(signaturePolicy[0]));
                                    Identifier.SetAttribute(SecurityConstants.Qualifier, SecurityConstants.OIDAsURN);
                                SigPolicyId.AppendChild(Identifier);
                                //ANSI X9.57 DSA signature generated with SHA-1 hash (DSA x9.30)
                                    XmlElement Description = doc.CreateElement(SecurityConstants.XADES_Description, SecurityConstants.XADES_132_URI);
                                    Description.AppendChild(doc.CreateTextNode(signaturePolicy[1]));
                                SigPolicyId.AppendChild(Description);
                            SignaturePolicyId.AppendChild(SigPolicyId);
                                XmlElement SigPolicyHash = doc.CreateElement(SecurityConstants.XADES_SigPolicyHash, SecurityConstants.XADES_132_URI);
                                    DigestMethod = doc.CreateElement(SecurityConstants.DigestMethod, SecurityConstants.XMLDSIG_URI);
                                    DigestMethod.SetAttribute(SecurityConstants.Algorithm, SecurityConstants.XMLDSIG_URI_SHA1);
                                SigPolicyHash.AppendChild(DigestMethod);
                                    DigestValue = doc.CreateElement(SecurityConstants.DigestValue, SecurityConstants.XMLDSIG_URI);
                                    byte[] policyIdContent = System.Text.Encoding.UTF8.GetBytes(SigPolicyId.OuterXml);
                                    DigestValue.AppendChild(doc.CreateTextNode(Convert.ToBase64String((md.ComputeHash(policyIdContent)))));
                                SigPolicyHash.AppendChild(DigestValue);
                            SignaturePolicyId.AppendChild(SigPolicyHash);
                        SignaturePolicyIdentifier.AppendChild(SignaturePolicyId);
                    SignedSignatureProperties.AppendChild(SignaturePolicyIdentifier);
                    }

                    SignedProperties.AppendChild(SignedSignatureProperties);
                    XmlElement SignedDataObjectProperties = doc.CreateElement(SecurityConstants.XADES_SignedDataObjectProperties, SecurityConstants.XADES_132_URI);
                    XmlElement DataObjectFormat = doc.CreateElement(SecurityConstants.XADES_DataObjectFormat, SecurityConstants.XADES_132_URI);
                        DataObjectFormat.SetAttribute(SecurityConstants.ObjectReference, "#" + contentReferenceId);
                            String descr = sap.GetDescription();
                        if (descr != null) {
                            XmlElement Description = doc.CreateElement(SecurityConstants.XADES_Description, SecurityConstants.XADES_132_URI);
                            Description.AppendChild(doc.CreateTextNode(descr));
                        DataObjectFormat.AppendChild(Description);
                        }
                        XmlElement MimeType = doc.CreateElement(SecurityConstants.XADES_MimeType, SecurityConstants.XADES_132_URI);
                            MimeType.AppendChild(doc.CreateTextNode(sap.GetMimeType()));
                        DataObjectFormat.AppendChild(MimeType);
                            String enc = sap.GetXmlLocator().GetEncoding();
                        if (enc != null) {
                            XmlElement Encoding = doc.CreateElement(SecurityConstants.XADES_Encoding, SecurityConstants.XADES_132_URI);
                            Encoding.AppendChild(doc.CreateTextNode(enc));
                        DataObjectFormat.AppendChild(Encoding);
                        }
                    SignedDataObjectProperties.AppendChild(DataObjectFormat);
                SignedProperties.AppendChild(SignedDataObjectProperties);
            QualifyingProperties.AppendChild(SignedProperties);
            dsObject.AppendChild(QualifyingProperties);

            signedProperty = SignedProperties;
            return dsObject;
        }

        private static String GetX509IssuerName(X509Certificate cert) {
            return cert.IssuerDN.ToString();
        }

        private static String GetX509SerialNumber(X509Certificate cert) {
            return cert.SerialNumber.ToString();
        }

        private static XmlElement GenerateContentReference(XmlDocument doc, XmlSignatureAppearance sap, String referenceId) {
            
            IXpathConstructor xpathConstructor = sap.GetXpathConstructor();

            XmlElement reference = doc.CreateElement("Reference", SecurityConstants.XMLDSIG_URI);
            reference.SetAttribute("URI", "");
            if (referenceId != null)
                reference.SetAttribute("Id", referenceId);

            XmlElement transforms = doc.CreateElement("Transforms", SecurityConstants.XMLDSIG_URI);

            XmlElement transform = doc.CreateElement("Transform", SecurityConstants.XMLDSIG_URI);
            transform.SetAttribute("Algorithm", SecurityConstants.XMLDSIG_URI_ENVELOPED);
            transforms.AppendChild(transform);

            byte[] md = null;
            if (xpathConstructor != null && xpathConstructor.GetXpathExpression().Length > 0)
            {
                XmlNodeList nodelist = doc.SelectNodes(xpathConstructor.GetXpathExpression(),
                                                               xpathConstructor.GetNamespaceManager());
                if (nodelist.Count == 1 && nodelist[0].NodeType == XmlNodeType.Element)
                {
                    XmlElement xpathSelect = (XmlElement) nodelist[0].CloneNode(true);
                    NormalizeNamespaces(nodelist[0].CreateNavigator(), xpathSelect.CreateNavigator());

                    XmlDocument digestDoc = new XmlDocument(doc.NameTable);
                    digestDoc.LoadXml(xpathSelect.OuterXml);

                    md = CalculateC14nDigest(digestDoc, new SHA1Managed());

                    transform = doc.CreateElement("Transform", SecurityConstants.XMLDSIG_URI);
                    transform.SetAttribute("Algorithm", SecurityConstants.XMLDSIG_URI_XPATH_FILTER2);

                    XmlElement xpath = doc.CreateElement("XPath", SecurityConstants.XMLDSIG_URI);
                    xpath.SetAttribute("xmlns", SecurityConstants.XMLDSIG_URI_XPATH_FILTER2);
                    xpath.SetAttribute("Filter", "intersect");

                    XmlNode xpathNode = doc.CreateTextNode(xpathConstructor.GetXpathExpression());

                    xpath.AppendChild(xpathNode);
                    transform.AppendChild(xpath);
                    transforms.AppendChild(transform);
                }
            }
            
            if (md == null)
                md = CalculateC14nDigest(doc, new SHA1Managed());

            reference.AppendChild(transforms);

            XmlElement digestMethod = doc.CreateElement("DigestMethod", SecurityConstants.XMLDSIG_URI);
            digestMethod.SetAttribute("Algorithm", SecurityConstants.XMLDSIG_URI_SHA1);
            reference.AppendChild(digestMethod);

            XmlElement digestValue = doc.CreateElement("DigestValue", SecurityConstants.XMLDSIG_URI);

            digestValue.AppendChild(doc.CreateTextNode(Convert.ToBase64String(md)));

            reference.AppendChild(digestValue);
            return reference;
        }

        private static XmlElement GenerateCustomReference(XmlDocument doc, XmlElement signedElement, String uri, String type, String id) {

            XmlElement reference = doc.CreateElement("Reference", SecurityConstants.XMLDSIG_URI);
            if (uri == null)
                uri = string.Empty;
            reference.SetAttribute("URI", uri);
            if(type != null)
                reference.SetAttribute("Type", type);
            if(id != null)
                reference.SetAttribute("Id", id);
            XmlElement xpathSelect = (XmlElement)signedElement.CloneNode(true);
            XPathNavigator xpathSelectNavigator = xpathSelect.CreateNavigator();
            NormalizeNamespaces(doc.DocumentElement.CreateNavigator(), xpathSelectNavigator);
            xpathSelectNavigator.CreateAttribute("xmlns", "xades", SecurityConstants.XMLNS_URI, SecurityConstants.XADES_132_URI);
            xpathSelectNavigator.CreateAttribute("xmlns", "", SecurityConstants.XMLNS_URI, SecurityConstants.XMLDSIG_URI);
            
            XmlDocument digestDoc = new XmlDocument(doc.NameTable);
            digestDoc.LoadXml(xpathSelect.OuterXml);

            byte[] md = CalculateC14nDigest(digestDoc, new SHA1Managed());

            XmlElement digestMethod = doc.CreateElement("DigestMethod", SecurityConstants.XMLDSIG_URI);
            digestMethod.SetAttribute("Algorithm", SecurityConstants.XMLDSIG_URI_SHA1);
            reference.AppendChild(digestMethod);

            XmlElement digestValue = doc.CreateElement("DigestValue", SecurityConstants.XMLDSIG_URI);

            digestValue.AppendChild(doc.CreateTextNode(Convert.ToBase64String(md)));

            reference.AppendChild(digestValue);
            return reference;
        }

        private static void Sign(XmlElement signature, IXmlLocator xmlLocator, IExternalSignature externalSignature, 
                                 List<XmlElement> references, XmlElement dsObject, KeyInfoClause keyInfo) {

            XmlDocument originalDoc = xmlLocator.GetDocument();

            if (signature == null)
                throw new InvalidOperationException();

            XmlElement signedInfo = originalDoc.CreateElement("SignedInfo", SecurityConstants.XMLDSIG_URI);
            signature.AppendChild(signedInfo);

            XmlElement canonicalizationMethod = originalDoc.CreateElement("CanonicalizationMethod", SecurityConstants.XMLDSIG_URI);
            canonicalizationMethod.SetAttribute("Algorithm", SecurityConstants.XMLDSIG_URI_C14N);
            signedInfo.AppendChild(canonicalizationMethod);

            XmlElement signatureMethod = originalDoc.CreateElement("SignatureMethod", SecurityConstants.XMLDSIG_URI);
            if(externalSignature.GetEncryptionAlgorithm().Equals(SecurityConstants.RSA))
                signatureMethod.SetAttribute("Algorithm", SecurityConstants.XMLDSIG_URI_RSA_SHA1);
            else if(externalSignature.GetEncryptionAlgorithm().Equals(SecurityConstants.DSA))
                signatureMethod.SetAttribute("Algorithm", SecurityConstants.XMLDSIG_URI_DSA_SHA1);
            signedInfo.AppendChild(signatureMethod);

            foreach (XmlElement reference in references)
                signedInfo.AppendChild(reference);    
            
            //if append Signature to original document upper - reference digest will be incorrect.
            originalDoc.DocumentElement.AppendChild(signature);

            XmlElement signedInfoDigest = (XmlElement)signedInfo.CloneNode(true);
            NormalizeNamespaces(signedInfo.CreateNavigator(), signedInfoDigest.CreateNavigator());
            XmlDocument signedInfoDoc = new XmlDocument(originalDoc.NameTable);
            signedInfoDoc.LoadXml(signedInfoDigest.OuterXml);
            byte[] byteRange = CalculateC14nByteRange(signedInfoDoc);

            //Sign with ExternalSignature
            String valueBase64 = Convert.ToBase64String(externalSignature.Sign(byteRange));

            XmlElement signatureValue = originalDoc.CreateElement("SignatureValue", SecurityConstants.XMLDSIG_URI);
            signatureValue.AppendChild(originalDoc.CreateTextNode(valueBase64));

            signature.AppendChild(signatureValue);

            if(keyInfo != null) {
                XmlElement keyInfoElement = originalDoc.CreateElement("KeyInfo", SecurityConstants.XMLDSIG_URI);
                keyInfoElement.AppendChild(originalDoc.ImportNode(keyInfo.GetXml(), true));
                signature.AppendChild(keyInfoElement);
            }

            if (dsObject != null)
                signature.AppendChild(dsObject);

            xmlLocator.SetDocument(originalDoc);
        }
    }
}
