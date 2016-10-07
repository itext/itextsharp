using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

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
    }
}