using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    internal class BookmarksTest {
        private const String TITLE = "1.\u00a0Paragraph 1";

        private class CustomPdfPageEventHelper : PdfPageEventHelper {
            public override void OnParagraph(PdfWriter writer, Document document, float position) {
                PdfContentByte cb = writer.DirectContent;
                PdfDestination destination = new PdfDestination(PdfDestination.FITH, position);
                new PdfOutline(cb.RootOutline, destination, TITLE);
            }
        }

        [Test]
        virtual public void TestNoBreakSpace() {
            MemoryStream ms = new MemoryStream();
            Document document = new Document();

            PdfWriter writer = PdfWriter.GetInstance(document, ms);

            document.Open();
            writer.PageEvent = new CustomPdfPageEventHelper();
            document.Add(new Paragraph("Hello World"));
            document.Close();

            // read bookmark back
            PdfReader r = new PdfReader(ms.ToArray());

            IList<Dictionary<String, Object>> bookmarks = SimpleBookmark.GetBookmark(r);
            Assert.AreEqual(1, bookmarks.Count, "bookmark size");
            Dictionary<String, Object> b = bookmarks[0];
            String title = (String) b["Title"];
            Assert.AreEqual(TITLE, title, "bookmark title");
        }
    }
}
