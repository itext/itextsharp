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
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.util;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using NUnit.Framework;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using X509Certificate = System.Security.Cryptography.X509Certificates.X509Certificate;

namespace itextsharp.tests.iTextSharp.text.pdf {
    class PdfEncryptionTest {
        public static string SOURCE_FOLDER = @"..\..\resources\text\pdf\PdfEncryptionTest\";
        public static string DEST_FOLDER = @"PdfEncryptionTest\";


        public static byte[] ownerPassword = System.Text.Encoding.UTF8.GetBytes("ownerPassword");

        [SetUp]
        public void SetUp() {
            Directory.CreateDirectory(DEST_FOLDER);
        }

        [Test]
        public void EncryptAES256() {
            String outPdf = DEST_FOLDER + "AES256Encrypted.pdf";
            String cmpPdf = SOURCE_FOLDER + "cmp_AES256Encrypted.pdf";
            Document doc = new Document();
            PdfWriter pdfWriter = PdfWriter.GetInstance(doc, new FileStream(outPdf, FileMode.Create));
//            byte[] userPassword = "userPassword".getBytes();
            byte[] userPassword = null;
            pdfWriter.SetEncryption(userPassword, ownerPassword, -1852, PdfWriter.ENCRYPTION_AES_256);
            doc.Open();
            doc.Add(new Paragraph("hello encrypted world"));
            doc.Close();
            pdfWriter.Close();

            Assert.IsNull(new CompareTool().CompareByContent(outPdf, cmpPdf, DEST_FOLDER, "diff_"));
        }

        [Test]
        public void StampAES256() {
            String outPdf = DEST_FOLDER + "stampAES256.pdf";
            String cmpPdf = SOURCE_FOLDER + "cmp_stampAES256.pdf";
            PdfReader reader = new PdfReader(SOURCE_FOLDER + "AES256EncryptedDocument.pdf", ownerPassword);
            PdfStamper pdfStamper = new PdfStamper(reader, new FileStream(outPdf, FileMode.Create));
            pdfStamper.Close();
            Assert.IsNull(new CompareTool().CompareByContent(outPdf, cmpPdf, DEST_FOLDER, "diff_"));
        }

        [Test]
        public void UnethicalStampAES256() {
            String outPdf = DEST_FOLDER + "unethicalStampAES256.pdf";
            String cmpPdf = SOURCE_FOLDER + "cmp_unethicalStampAES256.pdf";
            PdfReader reader = new PdfReader(SOURCE_FOLDER + "AES256EncryptedDocument.pdf");
            PdfReader.unethicalreading = true;
            PdfStamper pdfStamper = new PdfStamper(reader, new FileStream(outPdf, FileMode.Create));
            pdfStamper.Close();
            Assert.IsNull(new CompareTool().CompareByContent(outPdf, cmpPdf, DEST_FOLDER, "diff_"));
        }

        [Test]
        public void ComputeUserPasswordAES256() {
            String encryptedPdf = SOURCE_FOLDER + "cmp_AES256Encrypted.pdf";
            PdfReader reader = new PdfReader(encryptedPdf, ownerPassword);
            byte[] password = reader.ComputeUserPassword();
            reader.Close();

            Assert.IsNull(password);
        }

        [Test]
        public void EncryptWithCertificateAndSignTest() {
            String inPdf = SOURCE_FOLDER + "in.pdf";
            String outPdf = DEST_FOLDER + "encrypt_cert_signed.pdf";
            String tmpPdf = DEST_FOLDER + "encrypt_cert.pdf";

            EncryptPdfWithCertificate(inPdf, tmpPdf, SOURCE_FOLDER + "test.cer");

            X509Certificate cert = new X509Certificate();
            cert.Import(SOURCE_FOLDER + "test.cer");

            Pkcs12Store pkstore = new Pkcs12Store(new FileStream(SOURCE_FOLDER + "test.p12", FileMode.Open, FileAccess.Read), "kspass".ToCharArray());
            string pkalias = null;
            foreach (object a in pkstore.Aliases)
            {
                pkalias = ((string)a);
                if (pkstore.IsKeyEntry(pkalias))
                    break;
            }
            ICipherParameters certpk = pkstore.GetKey(pkalias).Key;

            X509Certificate2 signCert = new X509Certificate2(SOURCE_FOLDER + "test.p12", "kspass");
            CertSign(signCert, new X509CertificateParser(), outPdf, new PdfReader(tmpPdf, Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(cert), certpk), "reason", "location");
        }

        public static void EncryptPdfWithCertificate(string sourceDocument, string targetDocument, string certPath)
        {
            X509Certificate chain = new X509Certificate();
            chain.Import(certPath);
            Org.BouncyCastle.X509.X509Certificate cert = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(chain);
            Org.BouncyCastle.X509.X509Certificate[] certs = new Org.BouncyCastle.X509.X509Certificate[1] { cert };
            PdfReader reader = new PdfReader(sourceDocument);
            PdfStamper st = new PdfStamper(reader, new FileStream(targetDocument, FileMode.Create, FileAccess.Write), '\0', false);
            int[] x = new int[1];
            x[0] = PdfWriter.ALLOW_SCREENREADERS;
            st.SetEncryption(certs, x, PdfWriter.STANDARD_ENCRYPTION_40);
            st.Close();
        }

        private static void CertSign(X509Certificate2 cert, X509CertificateParser cp, string destinationPath, PdfReader reader, string reason, string location)
        {
            Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[]
            {
                cp.ReadCertificate(cert.RawData)
            };

            IExternalSignature externalSignature = new X509Certificate2Signature(cert, "SHA-1");

            using (FileStream fout = new FileStream(destinationPath, FileMode.Create, FileAccess.ReadWrite))
            {
                using (PdfStamper stamper = PdfStamper.CreateSignature(reader, fout, '\0', null, true))
                {
                    PdfSignatureAppearance appearance = stamper.SignatureAppearance;
                    appearance.Reason = reason;
                    appearance.Location = location;
                    MakeSignature.SignDetached(appearance, externalSignature, chain, null, null, null, 0,
                        CryptoStandard.CADES);
                    stamper.Close();
                }
            }
        }
    }
}
