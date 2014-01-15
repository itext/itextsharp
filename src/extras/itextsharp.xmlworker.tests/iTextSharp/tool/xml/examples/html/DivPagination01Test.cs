using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.pipeline.html;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.examples.html {
    internal class DivPagination01Test : SampleTest {
        protected override string GetTestName() {
            return "divPagination01";
        }

        protected override void MakePdf(string outPdf) {
            Document doc = new Document();
            PdfWriter pdfWriter = PdfWriter.GetInstance(doc, new FileStream(outPdf, FileMode.Create));
            doc.SetMargins(doc.LeftMargin - 10, doc.RightMargin - 10, doc.TopMargin, doc.BottomMargin);
            doc.Open();


            CssFilesImpl cssFiles = new CssFilesImpl();
            cssFiles.Add(XMLWorkerHelper.GetCSS(File.OpenRead(RESOURCES + @"\tool\xml\examples\" + "sampleTest.css")));
            StyleAttrCSSResolver cssResolver = new StyleAttrCSSResolver(cssFiles);
            HtmlPipelineContext hpc =
                new HtmlPipelineContext(
                    new CssAppliersImpl(new XMLWorkerFontProvider(RESOURCES + @"\tool\xml\examples\fonts")));
            hpc.SetAcceptUnknown(true).AutoBookmark(true).SetTagFactory(Tags.GetHtmlTagProcessorFactory());
            hpc.SetImageProvider(new SampleTestImageProvider());
            HtmlPipeline htmlPipeline = new HtmlPipeline(hpc, new PdfWriterPipeline(doc, pdfWriter));
            IPipeline pipeline = new CssResolverPipeline(cssResolver, htmlPipeline);
            XMLWorker worker = new XMLWorker(pipeline, true);
            XMLParser p = new XMLParser(true, worker, Encoding.GetEncoding("UTF-8"));
            p.Parse(File.OpenRead(inputHtml), Encoding.GetEncoding("UTF-8"));
            doc.Close();
        }
    }
}
