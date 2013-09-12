using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class MetaDataTest
    {
        [Test]
        public void TestProducer()
        {
            MemoryStream ms = new MemoryStream();
            Document document = new Document();

            PdfWriter.GetInstance(document, ms);
            document.Open();
            document.Add(new Paragraph("Hello World"));
            document.Close();

            PdfReader r = new PdfReader(ms.ToArray());

            Assert.AreEqual(r.Info["Producer"], Version.GetInstance().GetVersion, "Producer");
        }
    }
}
