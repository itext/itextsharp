using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;

namespace itextsharp.tests.resources.text.signature {
    [TestFixture]
    public class XadesTest : XmlDSigTest {
        public const String KEYSTORE = @"..\..\resources\text\pdf\signature\ds-ks\pkcs12";
        public const string PASSWORD = "password";
        public const String Src = @"..\..\resources\text\pdf\signature\xfa.pdf";
        public const String DestDirBes = @"signatures\xades-bes\";
        public const String DestDirEpes = @"signatures\xades-epes\";


        [Test]
        virtual public void XadesBesRsa() {
            Directory.CreateDirectory(DestDirBes);

            String filename = "xfa.signed.bes.pdf";
            String output = DestDirBes + filename;

            MemoryStream ks = new MemoryStream();
            using (FileStream reader = new FileStream(KEYSTORE, FileMode.Open))
            {
                byte[] buffer = new byte[reader.Length];
                reader.Read(buffer, 0, (int)reader.Length);
                ks.Write(buffer, 0, buffer.Length);
                ks.Position = 0;
            }
            Pkcs12Store store = new Pkcs12Store(ks, PASSWORD.ToCharArray());
            String alias = "";
            List<X509Certificate> chain = new List<X509Certificate>();
            // searching for private key

            foreach(string al in store.Aliases) {
                if(store.IsKeyEntry(al) && store.GetKey(al).Key.IsPrivate) {
                    alias = al;
                    break;
                }
            }

            AsymmetricKeyEntry pk = store.GetKey(alias);
            foreach(X509CertificateEntry c in store.GetCertificateChain(alias))
                chain.Add(c.Certificate);

            RsaPrivateCrtKeyParameters parameters = pk.Key as RsaPrivateCrtKeyParameters;
            SignXades(Src, output, parameters, chain.ToArray(), DigestAlgorithms.SHA1, false);
            
            String cmp = SaveXmlFromResult(output);

            Assert.IsTrue(VerifySignature(cmp), "XmlDSig Verification");
            
        }

        [Test]
        virtual public void XadesBesRsaPackage() {

            Directory.CreateDirectory(DestDirBes);

            String filename = "xfa.signed.bes.package.pdf";
            String output = DestDirBes + filename;

            MemoryStream ks = new MemoryStream();
            using(FileStream reader = new FileStream(KEYSTORE, FileMode.Open)) {
                byte[] buffer = new byte[reader.Length];
                reader.Read(buffer, 0, (int)reader.Length);
                ks.Write(buffer, 0, buffer.Length);
                ks.Position = 0;
            }
            Pkcs12Store store = new Pkcs12Store(ks, PASSWORD.ToCharArray());
            String alias = "";
            List<X509Certificate> chain = new List<X509Certificate>();
            // searching for private key

            foreach(string al in store.Aliases) {
                if(store.IsKeyEntry(al) && store.GetKey(al).Key.IsPrivate) {
                    alias = al;
                    break;
                }
            }

            AsymmetricKeyEntry pk = store.GetKey(alias);
            foreach(X509CertificateEntry c in store.GetCertificateChain(alias)) {
                chain.Add(c.Certificate);
            }

            RsaPrivateCrtKeyParameters parameters = pk.Key as RsaPrivateCrtKeyParameters;

            SignXadesPackage(Src, output, XfaXpathConstructor.XdpPackage.Template, parameters, chain.ToArray(), DigestAlgorithms.SHA1, false);

            String cmp = SaveXmlFromResult(output);

            Assert.IsTrue(VerifyPackageSignature(cmp), "XmlDSig Verification");
        }

        [Test]
        virtual public void XadesEpesRsa() {
            Directory.CreateDirectory(DestDirEpes);

            String filename = "xfa.signed.epes.pdf";
            String output = DestDirEpes + filename;

            MemoryStream ks = new MemoryStream();
            using(FileStream reader = new FileStream(KEYSTORE, FileMode.Open)) {
                byte[] buffer = new byte[reader.Length];
                reader.Read(buffer, 0, (int)reader.Length);
                ks.Write(buffer, 0, buffer.Length);
                ks.Position = 0;
            }
            Pkcs12Store store = new Pkcs12Store(ks, PASSWORD.ToCharArray());
            String alias = "";
            List<X509Certificate> chain = new List<X509Certificate>();
            // searching for private key

            foreach(string al in store.Aliases) {
                if(store.IsKeyEntry(al) && store.GetKey(al).Key.IsPrivate) {
                    alias = al;
                    break;
                }
            }

            AsymmetricKeyEntry pk = store.GetKey(alias);
            foreach(X509CertificateEntry c in store.GetCertificateChain(alias))
                chain.Add(c.Certificate);

            RsaPrivateCrtKeyParameters parameters = pk.Key as RsaPrivateCrtKeyParameters;
            SignXades(Src, output, parameters, chain.ToArray(), DigestAlgorithms.SHA1, true);

            String cmp = SaveXmlFromResult(output);

            Assert.IsTrue(VerifySignature(cmp), "XmlDSig Verification");
        }

        [Test]
        virtual public void XadesEpesRsaPackage() {

            Directory.CreateDirectory(DestDirEpes);

            String filename = "xfa.signed.epes.package.pdf";
            String output = DestDirEpes + filename;

            MemoryStream ks = new MemoryStream();
            using(FileStream reader = new FileStream(KEYSTORE, FileMode.Open)) {
                byte[] buffer = new byte[reader.Length];
                reader.Read(buffer, 0, (int)reader.Length);
                ks.Write(buffer, 0, buffer.Length);
                ks.Position = 0;
            }
            Pkcs12Store store = new Pkcs12Store(ks, PASSWORD.ToCharArray());
            String alias = "";
            List<X509Certificate> chain = new List<X509Certificate>();
            // searching for private key

            foreach(string al in store.Aliases) {
                if(store.IsKeyEntry(al) && store.GetKey(al).Key.IsPrivate) {
                    alias = al;
                    break;
                }
            }

            AsymmetricKeyEntry pk = store.GetKey(alias);
            foreach(X509CertificateEntry c in store.GetCertificateChain(alias)) {
                chain.Add(c.Certificate);
            }

            RsaPrivateCrtKeyParameters parameters = pk.Key as RsaPrivateCrtKeyParameters;

            SignXadesPackage(Src, output, XfaXpathConstructor.XdpPackage.Template, parameters, chain.ToArray(), DigestAlgorithms.SHA1, true);

            String cmp = SaveXmlFromResult(output);

            Assert.IsTrue(VerifyPackageSignature(cmp), "XmlDSig Verification");
        }
    }
}
