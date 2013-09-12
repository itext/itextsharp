using System.IO;
using iTextSharp.text;
using iTextSharp.text.exceptions;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class UnbalancedOperatorsTest
    {
        private Document document;
        private PdfWriter writer;

        private void InitializeDocument()
        {
            MemoryStream baos = new MemoryStream();
            document = new Document();
            writer = PdfWriter.GetInstance(document, baos);
            document.Open();
        }

        [Test]
        public void TestBasicDocument()
        {
            InitializeDocument();
            writer.DirectContent.SaveState();
            document.Add(new Paragraph("Hello World"));
            writer.DirectContent.RestoreState();
            document.Close();
        }

        [Test]
        public void TestNewPage()
        {
            InitializeDocument();
            writer.DirectContent.SaveState();
            document.Add(new Paragraph("Hello World"));
            writer.DirectContent.RestoreState();
            document.NewPage();
            document.Add(new Paragraph("Hello World"));
            document.Close();
        }

        [Test]
        [ExpectedException(typeof(IllegalPdfSyntaxException))]
        public void TestUnbalancedRestoreState()
        {
            InitializeDocument();
            document.Add(new Paragraph("Hello World"));
            writer.DirectContent.RestoreState();
            document.Close();
        }

        [Test]
        [ExpectedException(typeof(IllegalPdfSyntaxException))]
        public void TestUnbalancedSaveStateOnClose()
        {
            InitializeDocument();
            writer.DirectContent.SaveState();
            document.Add(new Paragraph("Hello World"));
            document.Close();
        }

        [Test]
        [ExpectedException(typeof(IllegalPdfSyntaxException))]
        public void TestUnbalancedSaveStateOnNewPage()
        {
            InitializeDocument();
            writer.DirectContent.SaveState();
            document.Add(new Paragraph("Hello World"));
            document.NewPage();
            document.Add(new Paragraph("Hello World"));
            document.Close();
        }
    }
}
