using System;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html {
    /**
 * @author Balder
 *
 */

    internal class SpecialCharInPDFTest {
        public static String RESOURCE_TEST_PATH = @"..\..\resources\";
        public static String SNIPPETS = "/snippets/";
        private string TARGET = @"SpecialCharInPDFTest\";

        private static String TEST = "index_";

        static SpecialCharInPDFTest() {
            //FontFactory.registerDirectories();
            Document.Compress = false;
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
        }

        private CssUtils utils = CssUtils.GetInstance();

        [SetUp]
        public void SetUp() {
            Directory.CreateDirectory(TARGET);
        }

        [Test]
        public void ParseXfaOnlyXML() {
            StreamReader bis = File.OpenText(RESOURCE_TEST_PATH + SNIPPETS + TEST + "snippet.html");
            Document doc = new Document(PageSize.A4);
            float margin = utils.ParseRelativeValue("10%", PageSize.A4.Width);
            doc.SetMargins(margin, margin, margin, margin);
            PdfWriter writer = null;
            try {
                writer = PdfWriter.GetInstance(doc, new FileStream(
                    TARGET + TEST + "_charset.pdf", FileMode.Create));
            }
            catch (DocumentException e) {
                Console.WriteLine(e);
            }
            CssFilesImpl cssFiles = new CssFilesImpl();
            cssFiles.Add(XMLWorkerHelper.GetInstance().GetDefaultCSS());
            StyleAttrCSSResolver cssResolver = new StyleAttrCSSResolver(cssFiles);
            HtmlPipelineContext hpc = new HtmlPipelineContext(null);
            hpc.SetAcceptUnknown(true)
                .AutoBookmark(true)
                .SetTagFactory(Tags.GetHtmlTagProcessorFactory())
                .CharSet(Encoding.GetEncoding("ISO-8859-1"));
            IPipeline pipeline = new CssResolverPipeline(cssResolver,
                new HtmlPipeline(hpc, new PdfWriterPipeline(doc, writer)));
            XMLWorker worker = new XMLWorker(pipeline, true);
            doc.Open();
            XMLParser p = new XMLParser(true, worker);
            p.Parse(bis);
            doc.Close();
        }
    }
}