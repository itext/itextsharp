using System;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;

namespace itextsharp.tests.resources.text.signature
{
    [TestFixture]
    public class XmlDSigRsaTest : XmlDSigTest {

        public const String KeyStore = @"..\..\resources\text\pdf\signature\ds-rsa\key";
        public const String Src = @"..\..\resources\text\pdf\signature\xfa.pdf";
        public const String CmpDir = @"..\..\resources\text\pdf\signature\ds-rsa\";
        public const String DestDir = @"signatures\ds-rsa\";

        RSA publicKey;
        AsymmetricKeyParameter privateKey;

        [SetUp]
        virtual public void LoadKey() {
            string import = "";
            using(StreamReader streamReader = new StreamReader(KeyStore))
                import = streamReader.ReadToEnd();

            publicKey = new RSACryptoServiceProvider();
            publicKey.FromXmlString(import);

            AsymmetricCipherKeyPair cipherKeyPair = DotNetUtilities.GetRsaKeyPair(publicKey.ExportParameters(true));
            privateKey = cipherKeyPair.Private;
            publicKey.ImportParameters(publicKey.ExportParameters(false));
            
            Directory.CreateDirectory(DestDir);
        }


        [Test]
        virtual public void XmlDSigRSAWithPublicKey() {

            String filename = "xfa.signed.pk.pdf";
            String output = DestDir + filename;
            
            SignWithPublicKey(Src, output, privateKey, publicKey, DigestAlgorithms.SHA1);

            String cmp = SaveXmlFromResult(output);

            Assert.IsTrue(VerifySignature(cmp), "XmlDSig Verification");

            Assert.IsTrue(CompareXmls(cmp, CmpDir + filename.Replace(".pdf", ".xml")));
        }

        [Test]
        virtual public void XmlDSigRSAWithKeyInfo() {

            String filename = "xfa.signed.ki.pdf";
            String output = DestDir + filename;

            SignWithKeyInfo(Src, output, privateKey, publicKey, DigestAlgorithms.SHA1);

            String cmp = SaveXmlFromResult(output);

            Assert.IsTrue(VerifySignature(cmp), "XmlDSig Verification");

            Assert.IsTrue(CompareXmls(cmp, CmpDir + filename.Replace(".pdf", ".xml")));
        }

        [Test]
        virtual public void XmlDSigRSAWithPublicKeyPackage() {

            String filename = "xfa.signed.pk.package.pdf";
            String output = DestDir + filename;
            SignPackageWithPublicKey(Src, output, XfaXpathConstructor.XdpPackage.Template, privateKey,
                                     publicKey, DigestAlgorithms.SHA1);

            String cmp = SaveXmlFromResult(output);

            Assert.IsTrue(VerifyPackageSignature(cmp), "XmlDSig Verification");

            Assert.IsTrue(CompareXmls(cmp, CmpDir + filename.Replace(".pdf", ".xml")));
        }

        [Test]
        virtual public void XmlDSigRSAWithKeyInfoPackage() {

            String filename = "xfa.signed.ki.package.pdf";
            String output = DestDir + filename;

            SignPackageWithKeyInfo(Src, output, XfaXpathConstructor.XdpPackage.Template, privateKey,
                                   publicKey, DigestAlgorithms.SHA1);

            String cmp = SaveXmlFromResult(output);

            Assert.IsTrue(VerifyPackageSignature(cmp), "XmlDSig Verification");

            Assert.IsTrue(CompareXmls(cmp, CmpDir + filename.Replace(".pdf", ".xml")));
        }
    }
}
