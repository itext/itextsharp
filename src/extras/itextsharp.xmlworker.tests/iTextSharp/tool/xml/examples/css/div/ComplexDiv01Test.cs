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
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.examples.css.div {
    [Ignore]
    internal class ComplexDiv01Test : SampleTest {
        protected override string GetTestName() {
            return "complexDiv01";
        }

        protected override void TransformHtml2Pdf() {
            Document doc = new Document(PageSize.A3.Rotate());

            PdfWriter pdfWriter = PdfWriter.GetInstance(doc, new FileStream(outPdf, FileMode.Create));
            pdfWriter.CreateXmpMetadata();

            doc.SetMargins(200, 200, 0, 0);
            doc.Open();


            CssFilesImpl cssFiles = new CssFilesImpl();
            cssFiles.Add(
                XMLWorkerHelper.GetCSS(
                    File.OpenRead(RESOURCES + Path.DirectorySeparatorChar + testPath + testName +
                                  Path.DirectorySeparatorChar + "complexDiv_files" + Path.DirectorySeparatorChar +
                                  "main.css")));
            cssFiles.Add(
                XMLWorkerHelper.GetCSS(
                    File.OpenRead(RESOURCES + Path.DirectorySeparatorChar + testPath + testName +
                                  Path.DirectorySeparatorChar + "complexDiv_files" + Path.DirectorySeparatorChar +
                                  "widget082.css")));
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
            //ICC_Profile icc = ICC_Profile.getInstance(ComplexDiv01Test.class.getResourceAsStream("sRGB Color Space Profile.icm"));
            //pdfWriter.setOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            doc.Close();
        }
    }
}