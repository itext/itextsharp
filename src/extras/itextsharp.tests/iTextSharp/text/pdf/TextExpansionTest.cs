using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class TextExpansionTest
    {
        private const string SOURCE_FOLDER = @"..\..\resources\text\pdf\TextExpansionTest\";
        private const string DEST_FOLDER = @"TextExpansionTest\";

        [SetUp]
        public virtual void SetUp()
        {
            TestResourceUtils.PurgeTempFiles();
            Directory.CreateDirectory(DEST_FOLDER);
        }

        [Test]
        public void ImageTaggingExpansionTest() {
            String filename = "TextExpansionTest.pdf";
            Document doc = new Document(PageSize.LETTER, 72, 72, 72, 72);
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(DEST_FOLDER + filename, FileMode.Create));
            writer.SetTagged();
            doc.Open();

            Chunk c1 = new Chunk("ABC");
            c1.SetTextExpansion("the alphabet");
            Paragraph p1 = new Paragraph();
            p1.Add(c1);
            doc.Add(p1);

            PdfTemplate t = writer.DirectContent.CreateTemplate(6, 6);
            t.SetLineWidth(1f);
            t.Circle(3f, 3f, 1.5f);
            t.SetGrayFill(0);
            t.FillStroke();
            Image i = Image.GetInstance(t);
            Chunk c2 = new Chunk(i, 100, -100);
            doc.Add(c2);

            Chunk c3 = new Chunk("foobar");
            c3.SetTextExpansion("bar bar bar");
            Paragraph p3 = new Paragraph();
            p3.Add(c3);
            doc.Add(p3);

            doc.Close();


            CompareTool compareTool = new CompareTool();
            String error = compareTool.CompareByContent(DEST_FOLDER + filename, SOURCE_FOLDER + "cmp_" + filename, DEST_FOLDER, "diff_");
            if (error != null) {
                Assert.Fail(error);
            }
        }
    }
}
