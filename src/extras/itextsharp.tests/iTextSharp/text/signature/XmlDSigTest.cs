/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.XPath;
using Microsoft.XmlDiffPatch;
using NUnit.Framework;
using Org.BouncyCastle.Crypto;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace itextsharp.tests.resources.text.signature
{
    public class XmlDSigTest
    {
        virtual protected void SignWithCertificate(String src, String dest, ICipherParameters pk,
            X509Certificate[] chain, String digestAlgorithm) {
            
            // Creating the reader and the stamper
            PdfReader reader = new PdfReader(src);
            FileStream os = new FileStream(dest, FileMode.Create);
            PdfStamper stamper = PdfStamper.createXmlSignature(reader, os);
            // Creating the appearance
            XmlSignatureAppearance appearance = stamper.XmlSignatureAppearance;
            appearance.SetXmlLocator(new XfaXmlLocator(stamper));
            // Creating the signature

            IExternalSignature pks = new PrivateKeySignature(pk, digestAlgorithm);
            MakeXmlSignature.SignXmlDSig(appearance, pks, chain);
        }

        virtual protected void SignXades(String src, String dest, ICipherParameters pk,
            X509Certificate[] chain, String digestAlgorithm, bool includeSignaturePolicy) {

            // Creating the reader and the stamper
            PdfReader reader = new PdfReader(src);
            FileStream os = new FileStream(dest, FileMode.Create);
            PdfStamper stamper = PdfStamper.createXmlSignature(reader, os);
            // Creating the appearance
            XmlSignatureAppearance appearance = stamper.XmlSignatureAppearance;
            appearance.SetXmlLocator(new XfaXmlLocator(stamper));
            appearance.SetDescription("Simple xfa form");
            // Creating the signature

            IExternalSignature pks = new PrivateKeySignature(pk, digestAlgorithm);
            MakeXmlSignature.SignXades(appearance, pks, chain, includeSignaturePolicy);
        }
        
        virtual protected void SignWithKeyInfo(String src, String dest, ICipherParameters pk,
            AsymmetricAlgorithm publicKey, String digestAlgorithm) {
            
            // Creating the reader and the stamper
            PdfReader reader = new PdfReader(src);
            FileStream os = new FileStream(dest, FileMode.Create);
            PdfStamper stamper = PdfStamper.createXmlSignature(reader, os);
            // Creating the appearance
            XmlSignatureAppearance appearance = stamper.XmlSignatureAppearance;
            //Set XfaXmlLocator to control getting and setting Document
            appearance.SetXmlLocator(new XfaXmlLocator(stamper));
            // Creating the signature
            IExternalSignature pks = new PrivateKeySignature(pk, digestAlgorithm);

            KeyInfoClause keyInfo;
            if(publicKey is DSA)
                keyInfo = new DSAKeyValue((DSA)publicKey);
            else if(publicKey is RSA)
                keyInfo = new RSAKeyValue((RSA)publicKey);
            else
                throw new ArgumentException("Invalid public key algorithm", "publicKey");

            MakeXmlSignature.SignXmlDSig(appearance, pks, keyInfo);
        }

        virtual protected void SignWithPublicKey(String src, String dest, ICipherParameters pk,
            AsymmetricAlgorithm publicKey, String digestAlgorithm) {

            // Creating the reader and the stamper
            PdfReader reader = new PdfReader(src);
            FileStream os = new FileStream(dest, FileMode.Create);
            PdfStamper stamper = PdfStamper.createXmlSignature(reader, os);
            // Creating the appearance
            XmlSignatureAppearance appearance = stamper.XmlSignatureAppearance;
            //Set XfaXmlLocator to control getting and setting Document
            appearance.SetXmlLocator(new XfaXmlLocator(stamper));
            // Creating the signature
            IExternalSignature pks = new PrivateKeySignature(pk, digestAlgorithm);

            MakeXmlSignature.SignXmlDSig(appearance, pks, publicKey);
        }

        virtual protected void SignPackageWithCertificate(String src, String dest, XfaXpathConstructor.XdpPackage xdpPackage,
            ICipherParameters pk, X509Certificate[] chain, String digestAlgorithm) {

            // Creating the reader and the stamper
            PdfReader reader = new PdfReader(src);
            FileStream os = new FileStream(dest, FileMode.Create);
            PdfStamper stamper = PdfStamper.createXmlSignature(reader, os);
            // Creating the appearance
            XmlSignatureAppearance appearance = stamper.XmlSignatureAppearance;
            //Set XfaXmlLocator to control getting and setting Document
            appearance.SetXmlLocator(new XfaXmlLocator(stamper));
            // Set XpathConstructor, to construct xpath expression for signing an xdp package
            appearance.SetXpathConstructor(new XfaXpathConstructor(xdpPackage));
            // Creating the signature
            IExternalSignature pks = new PrivateKeySignature(pk, digestAlgorithm);

            MakeXmlSignature.SignXmlDSig(appearance, pks, chain);
        }

        virtual protected void SignXadesPackage(String src, String dest, XfaXpathConstructor.XdpPackage xdpPackage,
    ICipherParameters pk, X509Certificate[] chain, String digestAlgorithm, bool includeSignaturePolicy) {

            // Creating the reader and the stamper
            PdfReader reader = new PdfReader(src);
            FileStream os = new FileStream(dest, FileMode.Create);
            PdfStamper stamper = PdfStamper.createXmlSignature(reader, os);
            // Creating the appearance
            XmlSignatureAppearance appearance = stamper.XmlSignatureAppearance;
            //Set XfaXmlLocator to control getting and setting Document
            appearance.SetXmlLocator(new XfaXmlLocator(stamper));
            appearance.SetDescription("Simple xfa form");
            // Set XpathConstructor, to construct xpath expression for signing an xdp package
            appearance.SetXpathConstructor(new XfaXpathConstructor(xdpPackage));
            // Creating the signature
            IExternalSignature pks = new PrivateKeySignature(pk, digestAlgorithm);

            MakeXmlSignature.SignXades(appearance, pks, chain, includeSignaturePolicy);
        }

        virtual protected void SignPackageWithKeyInfo(String src, String dest, XfaXpathConstructor.XdpPackage xdpPackage,
            ICipherParameters pk, AsymmetricAlgorithm publicKey, String digestAlgorithm) {

            // Creating the reader and the stamper
            PdfReader reader = new PdfReader(src);
            FileStream os = new FileStream(dest, FileMode.Create);
            PdfStamper stamper = PdfStamper.createXmlSignature(reader, os);
            // Creating the appearance
            XmlSignatureAppearance appearance = stamper.XmlSignatureAppearance;
            //Set XfaXmlLocator to control getting and setting Document
            appearance.SetXmlLocator(new XfaXmlLocator(stamper));
            // Set XpathConstructor, to construct xpath expression for signing an xdp package
            appearance.SetXpathConstructor(new XfaXpathConstructor(xdpPackage));
            // Creating the signature
            IExternalSignature pks = new PrivateKeySignature(pk, digestAlgorithm);
        
            KeyInfoClause keyInfo;
            if(publicKey is DSA)
                keyInfo = new DSAKeyValue((DSA)publicKey);
            else if(publicKey is RSA)
                keyInfo = new RSAKeyValue((RSA)publicKey);
            else
                throw new ArgumentException("Invalid public key algorithm", "publicKey");

            MakeXmlSignature.SignXmlDSig(appearance, pks, keyInfo);
        }

        virtual protected void SignPackageWithPublicKey(String src, String dest, XfaXpathConstructor.XdpPackage xdpPackage,
            ICipherParameters pk, AsymmetricAlgorithm publicKey, String digestAlgorithm){

            // Creating the reader and the stamper
            PdfReader reader = new PdfReader(src);
            FileStream os = new FileStream(dest, FileMode.Create);
            PdfStamper stamper = PdfStamper.createXmlSignature(reader, os);
            // Creating the appearance
            XmlSignatureAppearance appearance = stamper.XmlSignatureAppearance;
            //Set XfaXmlLocator to control getting and setting Document
            appearance.SetXmlLocator(new XfaXmlLocator(stamper));
            // Set XpathConstructor, to construct xpath expression for signing an xdp package
            appearance.SetXpathConstructor(new XfaXpathConstructor(xdpPackage));
            // Creating the signature
            IExternalSignature pks = new PrivateKeySignature(pk, digestAlgorithm);

            MakeXmlSignature.SignXmlDSig(appearance, pks, publicKey);
        }

        virtual protected String SaveXmlFromResult(String input) {
            
            PdfReader reader = new PdfReader(input);
            XfaForm form = new XfaForm(reader);
            String output = input.Replace(".pdf", ".xml");

            using(StreamWriter file = new StreamWriter(output))
                file.Write(form.DomDocument.OuterXml);
                        reader.Close();
            return output;
        }

        virtual protected bool CompareXmls(String xml1, String xml2) {
            XmlDiff xmldiff = new XmlDiff(XmlDiffOptions.None);
            return xmldiff.Compare(xml1, xml2, false);
        }

        public static bool VerifySignature(String xml) {
            if (!VerifyPackageSignature(xml))
                Assert.Fail("Internal verification fails");
            
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.Load(new XmlTextReader(xml));
            XmlElement signatureElement = (XmlElement)xmlDocument.GetElementsByTagName("Signature")[0];
            SignedXml signedXml = new SignedXml(xmlDocument);
            signedXml.LoadXml(signatureElement);
            return signedXml.CheckSignature();
        }

        public static bool VerifyPackageSignature(String xml) {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.Load(new XmlTextReader(xml));

            XmlElement signatureElement = (XmlElement)xmlDocument.GetElementsByTagName("Signature")[0];
            XmlElement signedInfo = (XmlElement)signatureElement.GetElementsByTagName("SignedInfo")[0];
            
            byte[] signedInfoByteRange = CalculateC14nByteRange(signedInfo, xmlDocument);
            
            XmlNodeList references = signatureElement.GetElementsByTagName("Reference");
            

            foreach (XmlElement reference in references)
            {
                String uri = reference.GetAttribute("URI");
                if (String.IsNullOrEmpty(uri)) // main reference
                {
                    signatureElement.ParentNode.RemoveChild(signatureElement);
                    XmlNodeList transforms = signatureElement.GetElementsByTagName("Transform");
                    if (transforms.Count > 1) // second transform is Xpath Filter 2.0
                    {
                        XmlElement xpathFilterTransform = (XmlElement) transforms[1];
                        XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
                        namespaceManager.AddNamespace("xdp", "http://ns.adobe.com/xdp/");
                        XmlNodeList nodelist = xmlDocument.SelectNodes(xpathFilterTransform.InnerText, namespaceManager);
                        if(!VerifyElement((XmlElement)nodelist[0], xmlDocument, reference))
                            return false;
                        //for correct work with SignedXml class
                        xpathFilterTransform.ParentNode.RemoveChild(xpathFilterTransform);
                    }
                    else
                    {
                        if(!VerifyElement(null, xmlDocument, reference))
                            return false;
                    }

                    // revert
                    xmlDocument.DocumentElement.AppendChild(signatureElement);
                }
                else // SignedProperties reference
                {
                    XmlElement signedProperties = (XmlElement)signatureElement.GetElementsByTagName("xades:SignedProperties")[0];
                    if(!VerifyElement(signedProperties, xmlDocument, reference))
                        return false;
                }
            }
            
            SignedXml signedXml = new SignedXml(xmlDocument);
            signedXml.LoadXml(signatureElement);

            IEnumerator keyInfoClauses = signedXml.KeyInfo.GetEnumerator(); keyInfoClauses.MoveNext();
            KeyInfoClause keyInfo = (KeyInfoClause)keyInfoClauses.Current;

            bool result = false;
            if (keyInfo is RSAKeyValue)
                result = ((RSACryptoServiceProvider)((RSAKeyValue)keyInfo).Key).VerifyData(signedInfoByteRange, "SHA1", signedXml.SignatureValue);
            else if (keyInfo is KeyInfoX509Data)
            {
                AsymmetricAlgorithm rsa = ((X509Certificate2)(((KeyInfoX509Data)keyInfo).Certificates[0])).PublicKey.Key;
                result = ((RSACryptoServiceProvider) rsa).VerifyData(signedInfoByteRange, "SHA1", signedXml.SignatureValue);
            }

            return result;
        }

        private static bool VerifyElement(XmlElement element, XmlDocument doc, XmlElement reference) {
            byte[] byteRange = null;
            if (element != null)
                byteRange = CalculateC14nByteRange(element, doc);
            else
                byteRange = CalculateC14nByteRange(doc);

            XmlElement digestValue = (XmlElement)reference.GetElementsByTagName("DigestValue")[0];

            String base64Hash = Convert.ToBase64String(new SHA1Managed().ComputeHash(byteRange));
            if(base64Hash != digestValue.InnerText)
                return false;
            return true;
        }

        private static byte[] CalculateC14nByteRange(XmlElement element, XmlDocument doc) {
            XmlElement cloneElement = (XmlElement)element.CloneNode(true);
            NormalizeNamespaces(element.CreateNavigator(), cloneElement.CreateNavigator());
            XmlDocument elememntDoc = new XmlDocument(doc.NameTable);
            elememntDoc.LoadXml(cloneElement.OuterXml);
            XmlDsigC14NTransform c14nTransform = new XmlDsigC14NTransform();
            c14nTransform.LoadInput(elememntDoc);
            return ((MemoryStream)c14nTransform.GetOutput()).ToArray();
        }

        private static byte[] CalculateC14nByteRange(XmlDocument doc) {
            XmlDsigC14NTransform c14nTransform = new XmlDsigC14NTransform();
            c14nTransform.LoadInput(doc);
            return ((MemoryStream)c14nTransform.GetOutput()).ToArray();
        }

        private static void NormalizeNamespaces(XPathNavigator src, XPathNavigator dest) {
            IDictionary<string, string> dictLocal = src.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);
            IDictionary<string, string> dictExclude = dest.GetNamespacesInScope(XmlNamespaceScope.Local);

            foreach(KeyValuePair<string, string> pair in dictLocal)
                if(!dictExclude.ContainsKey(pair.Key))
                    dest.CreateAttribute("xmlns", pair.Key, "http://www.w3.org/2000/xmlns/", pair.Value);
        }
    }
}
