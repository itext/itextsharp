using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.examples.html
{
    class LetteredListTest : SampleTest {

        protected override string GetTestName() {
            return "letteredList";
        }

        protected override void MakePdf(string outPdf) {

            Document doc = new Document(PageSize.A4);
            PdfWriter pdfWriter = PdfWriter.GetInstance(doc, new FileStream(outPdf, FileMode.Create));
            doc.Open();
            FileStream cssFileStream = new FileStream(RESOURCES + @"\tool\xml\examples\html\letteredList\" + "sampleTest.css", FileMode.Open, FileAccess.Read, FileShare.Read);
            TransformHtml2Pdf(doc, pdfWriter, new SampleTestImageProvider(), new XMLWorkerFontProvider(RESOURCES + @"\tool\xml\examples\fonts"), cssFileStream);
            doc.Close();

        }
    }
}
