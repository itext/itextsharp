using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.examples.css.at_rule {
    public class AtMedia_Rule_IgnoringTest : SampleTest {

        protected override String GetTestName() {
            return "atmedia_rule_ignoring";
        }

        protected override void MakePdf(String outPdf) {
            Document doc = new Document(PageSize.A4);
            PdfWriter pdfWriter = PdfWriter.GetInstance(doc, File.Create(outPdf));
            doc.Open();
            FileStream cssFileStream = new FileStream(RESOURCES + @"tool/xml/examples/css/at_rule/atmedia_rule_ignoring/media-print.css", FileMode.Open, FileAccess.Read, FileShare.Read);
            TransformHtml2Pdf(doc, pdfWriter, new SampleTestImageProvider(), new XMLWorkerFontProvider(RESOURCES + @"tool\xml\examples\fonts\"), cssFileStream);
            doc.Close();
        }
    }
}