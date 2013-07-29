using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using Microsoft.XmlDiffPatch;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;

namespace itextsharp.tests.resources.text.signature
{
    public class XmlDSigTest
    {
        protected void SignWithCertificate(String src, String dest, ICipherParameters pk,
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

        protected void SignBes(String src, String dest, ICipherParameters pk,
            X509Certificate[] chain, String digestAlgorithm) {

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
            MakeXmlSignature.SignXadesBes(appearance, pks, chain);
        }
        
        protected void SignWithKeyInfo(String src, String dest, ICipherParameters pk,
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

        protected void SignWithPublicKey(String src, String dest, ICipherParameters pk,
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

        protected void SignPackageWithCertificate(String src, String dest, XfaXpathConstructor.XdpPackage xdpPackage,
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

        protected void SignBesPackage(String src, String dest, XfaXpathConstructor.XdpPackage xdpPackage,
    ICipherParameters pk, X509Certificate[] chain, String digestAlgorithm) {

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

            MakeXmlSignature.SignXadesBes(appearance, pks, chain);
        }

        protected void SignPackageWithKeyInfo(String src, String dest, XfaXpathConstructor.XdpPackage xdpPackage,
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

        protected void SignPackageWithPublicKey(String src, String dest, XfaXpathConstructor.XdpPackage xdpPackage,
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

        protected String SaveXmlFromResult(String input) {
            
            PdfReader reader = new PdfReader(input);
            XfaForm form = new XfaForm(reader);
            String output = input.Replace(".pdf", ".xml");

            using(StreamWriter file = new StreamWriter(output))
                file.Write(form.DomDocument.OuterXml);
                        reader.Close();
            return output;
        }

        protected bool CompareXmls(String xml1, String xml2) {
            XmlDiff xmldiff = new XmlDiff(XmlDiffOptions.None);
            return xmldiff.Compare(xml1, xml2, false);
        }
    }
}