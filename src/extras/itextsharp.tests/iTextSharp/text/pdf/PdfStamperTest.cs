using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.testutils;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    class PdfStamperTest {
        static  string DestFolder = "com/itextpdf/test/pdf/PdfStamperTest/";
        private string TestResourcesPath = @"..\..\resources\text\pdf\PdfStamperTest\";

        [SetUp]
        public void SetUp() {
            Directory.CreateDirectory(DestFolder);
        }

        [Test]
        public void SetPageContentTest01()  {
            String outPdf = DestFolder + "out1.pdf";
            PdfReader reader =
                new PdfReader(TestResourceUtils.GetResourceAsStream(TestResourcesPath, "in.pdf"));
            PdfStamper stamper = new PdfStamper(reader, new FileStream(outPdf, FileMode.Create));
            reader.EliminateSharedStreams();
            int total = reader.NumberOfPages + 1;
            for (int i = 1; i < total; i++) {
                byte[] bb = reader.GetPageContent(i);
                reader.SetPageContent(i, bb);
            }
            stamper.Close();

            new CompareTool().CompareByContent(outPdf, TestResourceUtils.GetResourceAsTempFile(TestResourcesPath, "cmp_out1.pdf"), DestFolder, "diff_");
        }

        [Test]
        public void SetPageContentTest02()  {
            String outPdf = DestFolder + "out2.pdf";
            PdfReader reader = new PdfReader(TestResourceUtils.GetResourceAsStream(TestResourcesPath, "in.pdf"));
            PdfStamper stamper = new PdfStamper(reader, new FileStream(outPdf, FileMode.Create));
            int total = reader.NumberOfPages + 1;
            for (int i = 1; i < total; i++) {
                byte[] bb = reader.GetPageContent(i);
                reader.SetPageContent(i, bb);
            }
            reader.RemoveUnusedObjects();
            stamper.Close();

            String s = new CompareTool().CompareByContent(outPdf, TestResourceUtils.GetResourceAsTempFile(TestResourcesPath, "cmp_out2.pdf"), DestFolder, "diff_");
        }


    }
}
