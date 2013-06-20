using System;
using System.Collections.Generic;
using System.IO;
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
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: Pavel Alay, Bruno Lowagie, et al.
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
namespace iTextSharp.text.pdf.security
{
    /**
     * Class that signs your XML.
     */
    public class MakeXmlSignature
    {
        private const string XML_DSIG_URI = "http://www.w3.org/2000/09/xmldsig#";
        private const string XML_DSIG_URI_C14N = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
        private const string XML_DSIG_URI_RSA_SHA1 = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
        private const string XML_DSIG_URI_DSA_SHA1 = "http://www.w3.org/2000/09/xmldsig#dsa-sha1";
        private const string XML_DSIG_URI_ENVELOPED = "http://www.w3.org/2000/09/xmldsig#enveloped-signature";
        private const string XML_DSIG_URI_XPATH_FILTER2 = "http://www.w3.org/2002/06/xmldsig-filter2";
        private const string XML_DSIG_URI_SHA1 = "http://www.w3.org/2000/09/xmldsig#sha1";

        private static void NormalizeNamespaces(XPathNavigator src, XPathNavigator dest)
        {
            IDictionary<string, string> dictLocal = src.GetNamespacesInScope(XmlNamespaceScope.Local);
            IDictionary<string, string> dictExclude = src.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

            foreach (KeyValuePair<string, string> pair in dictExclude)
                if (!dictLocal.ContainsKey(pair.Key))
                    dest.CreateAttribute("xmlns", pair.Key, "http://www.w3.org/2000/xmlns/", pair.Value);
        }

        private static byte[] CalculateC14nByteRange(XmlDocument doc) {
            XmlDsigC14NTransform c14nTransform = new XmlDsigC14NTransform();
            c14nTransform.LoadInput(doc);
            return ((MemoryStream)c14nTransform.GetOutput()).ToArray();
        }


        private static byte[] CalculateC14nDigest(XmlDocument doc, HashAlgorithm hash) {
            return hash.ComputeHash(CalculateC14nByteRange(doc));
        }

        /**
         * Signs the xml using the enveloped mode, with optional xpath transform (see XmlSignatureAppearance).
         * @param sap the XmlSignatureAppearance
         * @param externalSignature  the interface providing the actual signing
         * @param keyInfo KeyInfo for verification
         * @throws GeneralSecurityException
         * @throws IOException
         * @throws DocumentException
         */
        public static void SignXmlDSig(XmlSignatureAppearance sap,
            IExternalSignature externalSignature, KeyInfoClause keyInfo) {
            IXmlLocator xmlLocator = sap.GetXmlLocator();
            if (xmlLocator == null)
                throw new DocumentException(MessageLocalization.GetComposedMessage("xmllocator.cannot.be.null"));
            if (!externalSignature.GetHashAlgorithm().Equals("SHA1"))
                throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("support.only.sha1.hash.algorithm"));

            XmlDocument originalDoc = xmlLocator.GetDocument();
            byte[] md = CalculateC14nDigest(originalDoc, new SHA1Managed());
            IXpathConstructor xpathConstructor = sap.GetXpathConstructor();


            XmlElement signature = originalDoc.CreateElement("Signature", XML_DSIG_URI);
            originalDoc.DocumentElement.AppendChild(signature);

            XmlElement signedInfo = originalDoc.CreateElement("SignedInfo", XML_DSIG_URI);
            signature.AppendChild(signedInfo);

            XmlElement canonicalizationMethod = originalDoc.CreateElement("CanonicalizationMethod", XML_DSIG_URI);
            canonicalizationMethod.SetAttribute("Algorithm", XML_DSIG_URI_C14N);
            signedInfo.AppendChild(canonicalizationMethod);

            XmlElement signatureMethod = originalDoc.CreateElement("SignatureMethod", XML_DSIG_URI);
            if(externalSignature.GetEncryptionAlgorithm().Equals("RSA"))
                signatureMethod.SetAttribute("Algorithm", XML_DSIG_URI_RSA_SHA1);
            else if(externalSignature.GetEncryptionAlgorithm().Equals("DSA"))
                signatureMethod.SetAttribute("Algorithm", XML_DSIG_URI_DSA_SHA1);
            else
                throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("support.only.rsa.and.dsa.algorithms"));
            signedInfo.AppendChild(signatureMethod);

            XmlElement reference = originalDoc.CreateElement("Reference", XML_DSIG_URI);
            reference.SetAttribute("URI", "");

            XmlElement transforms = originalDoc.CreateElement("Transforms", XML_DSIG_URI);

            XmlElement transform = originalDoc.CreateElement("Transform", XML_DSIG_URI);
            transform.SetAttribute("Algorithm", XML_DSIG_URI_ENVELOPED);
            transforms.AppendChild(transform);

            if (xpathConstructor != null && xpathConstructor.GetXpathExpression().Length > 0) {
                XmlNodeList nodelist = originalDoc.SelectNodes(xpathConstructor.GetXpathExpression(), xpathConstructor.GetNamespaceManager());
                if (nodelist.Count == 1 && nodelist[0].NodeType == XmlNodeType.Element) {
                    XmlElement xpathSelect = (XmlElement) nodelist[0].CloneNode(true);
                    NormalizeNamespaces(nodelist[0].CreateNavigator(), xpathSelect.CreateNavigator());

                    XmlDocument digestDoc = new XmlDocument(originalDoc.NameTable);
                    digestDoc.LoadXml(xpathSelect.OuterXml);

                    md = CalculateC14nDigest(digestDoc, new SHA1Managed());

                    transform = originalDoc.CreateElement("Transform", XML_DSIG_URI);
                    transform.SetAttribute("Algorithm", XML_DSIG_URI_XPATH_FILTER2);

                    XmlElement xpath = originalDoc.CreateElement("XPath", XML_DSIG_URI);
                    xpath.SetAttribute("xmlns", XML_DSIG_URI_XPATH_FILTER2);
                    xpath.SetAttribute("Filter", "intersect");

                    XmlNode xpathNode = originalDoc.CreateTextNode(xpathConstructor.GetXpathExpression());
                    
                    xpath.AppendChild(xpathNode);
                    transform.AppendChild(xpath);
                    transforms.AppendChild(transform);
                }
            }
            
            reference.AppendChild(transforms);
            
            XmlElement digestMethod = originalDoc.CreateElement("DigestMethod", XML_DSIG_URI);
            digestMethod.SetAttribute("Algorithm", XML_DSIG_URI_SHA1);
            reference.AppendChild(digestMethod);

            XmlElement digestValue = originalDoc.CreateElement("DigestValue", XML_DSIG_URI);
            
            digestValue.AppendChild(originalDoc.CreateTextNode(Convert.ToBase64String(md)));
            
            reference.AppendChild(digestValue);
            signedInfo.AppendChild(reference);

            XmlElement signedInfoDigest = (XmlElement)signedInfo.CloneNode(true);
            NormalizeNamespaces(signedInfo.CreateNavigator(), signedInfoDigest.CreateNavigator());
            XmlDocument signedInfoDoc = new XmlDocument(originalDoc.NameTable);
            signedInfoDoc.LoadXml(signedInfoDigest.OuterXml);
            byte[] byteRange = CalculateC14nByteRange(signedInfoDoc);

            //Sign with ExternalSignature
            String valueBase64 = Convert.ToBase64String(externalSignature.Sign(byteRange));

            XmlElement signatureValue = originalDoc.CreateElement("SignatureValue", XML_DSIG_URI);
            signatureValue.AppendChild(originalDoc.CreateTextNode(valueBase64));

            signature.AppendChild(signatureValue);

            if (keyInfo != null) {
                XmlElement keyInfoElement = originalDoc.CreateElement("KeyInfo", XML_DSIG_URI);
                keyInfoElement.AppendChild(originalDoc.ImportNode(keyInfo.GetXml(), true));
                signature.AppendChild(keyInfoElement);
            }
                
            xmlLocator.SetDocument(originalDoc);
            sap.Close();    
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
            if (publicKey is DSA)
                SignXmlDSig(sap, externalSignature, new DSAKeyValue((DSA)publicKey));
            else if(publicKey is RSA)
                SignXmlDSig(sap, externalSignature, new RSAKeyValue((RSA)publicKey));
            else
                throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("support.only.rsa.and.dsa.algorithms"));
        }
    }
}