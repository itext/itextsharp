using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.pipeline.html;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.examples.css.div {
    public class PageOverflow01Test : SampleTest {
        protected override string GetTestName() {
            return "pageOverflow01";
        }

        protected override void MakePdf(string outPdf) {
            Document doc = new Document(PageSize.A4.Rotate());
            PdfWriter pdfWriter = PdfWriter.GetInstance(doc, new FileStream(outPdf, FileMode.Create));
            doc.Open();
            TransformHtml2Pdf(doc, pdfWriter, new SampleTestImageProvider(), new XMLWorkerFontProvider(RESOURCES + @"tool\xml\examples\fonts"),
                new FileStream(RESOURCES + @"tool\xml\examples\sampleTest.css", FileMode.Open, FileAccess.Read, FileShare.Read));
            doc.Close();
        }

        protected override void TransformHtml2Pdf(Document doc, PdfWriter pdfWriter, IImageProvider imageProvider,
            IFontProvider fontProvider, Stream cssFile) {
            ICSSResolver cssResolver = XMLWorkerHelper.GetInstance().GetDefaultCssResolver(true);

            HtmlPipelineContext hpc;

            if (fontProvider != null)
                hpc = new HtmlPipelineContext(new CssAppliersImpl(fontProvider));
            else
                hpc = new HtmlPipelineContext(null);

            hpc.SetImageProvider(imageProvider);
            hpc.SetAcceptUnknown(true).AutoBookmark(true).SetTagFactory(Tags.GetHtmlTagProcessorFactory());
            HtmlPipeline htmlPipeline = new HtmlPipeline(hpc, new PdfWriterPipeline(doc, pdfWriter));
            IPipeline pipeline = new CssResolverPipeline(cssResolver, htmlPipeline);
            XMLWorker worker = new XMLWorker(pipeline, true);
            XMLParser xmlParse = new XMLParser(true, worker, Encoding.UTF8);
            xmlParse.Parse(new FileStream(inputHtml, FileMode.Open), Encoding.UTF8);
        }
    }
}
