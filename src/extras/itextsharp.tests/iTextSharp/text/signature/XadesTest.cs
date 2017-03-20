/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
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
