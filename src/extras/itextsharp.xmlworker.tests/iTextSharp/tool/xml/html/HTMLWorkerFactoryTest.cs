using System;
using System.IO;
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

    internal class HTMLWorkerFactoryTest {
        public static String RESOURCE_TEST_PATH = @"..\..\resources\";
        private string TARGET = @"HTMLWorkerFactoryTest\";
//	public static String SNIPPETS = "/snippets/";
        public static String SNIPPETS = "/bugs/";

//	private static String TEST = "doc_";
//    private static String TEST = "xfa-support_";
//    private static String TEST = "Atkins_";
//    private static String TEST = "b-p_";
//    private static String TEST = "br-sub-sup_";
//    private static String TEST = "font_color_";
//    private static String TEST = "fontSizes_";
//    private static String TEST = "line-height_letter-spacing_";
//    private static String TEST = "longtext_";
//    private static String TEST = "error_message_test_";
//    private static String TEST = "xfa-support_";
//    private static String TEST = "margin-align_";
//    private static String TEST = "xfa-hor-vert_";
//    private static String TEST = "text-indent_text-decoration_";
//    private static String TEST = "comment-double-print_";
//    private static String TEST = "tab_";
//	  private static String TEST = "table_";
//	  private static String TEST = "tableInTable_";
//	  private static String TEST = "table_incomplete_";
//	  private static String TEST = "lists_";
//	  private static String TEST = "img_";
//	  private static String TEST = "position_";
//	  private static String TEST = "h_";
//	  private static String TEST = "booksales_";
//	  private static String TEST = "index_";
//	  private static String TEST = "headers_";
//	  private static String TEST = "headers_noroottag_";
//	  private static String TEST = "index_anchor_";
//	  private static String TEST = "lineheight_";
//	  private static String TEST = "table_exception_";
//	  private static String TEST = "widthTable_";
//	  private static String TEST ="test-table-a_";
//	  private static String TEST ="test-table-b_";
//	  private static String TEST ="test-table-c_";
//	  private static String TEST ="test-table-d_";
//	  private static String TEST = "pagebreaks_";

        // Bug snippets

        private static String TEST = "colored_lists_";

        static HTMLWorkerFactoryTest() {
            //FontFactory.registerDirectories();
            Document.Compress = false;
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
        }

        [SetUp]
        public void SetUp() {
            Directory.CreateDirectory(TARGET);
        }

        private CssUtils utils = CssUtils.GetInstance();

        [Test]
        public void ParseXfaOnlyXML() {
            TextReader bis = File.OpenText(RESOURCE_TEST_PATH + SNIPPETS + TEST + "snippet.html");
            Document doc = new Document(PageSize.A4);
            float margin = utils.ParseRelativeValue("10%", PageSize.A4.Width);
            doc.SetMargins(margin, margin, margin, margin);
            PdfWriter writer = null;
            try {
                writer = PdfWriter.GetInstance(doc, new FileStream(
                    TARGET + TEST + "Test.pdf", FileMode.Create));
            }
            catch (DocumentException e) {
                Console.WriteLine(e);
            }
            CssFilesImpl cssFiles = new CssFilesImpl();
            cssFiles.Add(XMLWorkerHelper.GetInstance().GetDefaultCSS());
            StyleAttrCSSResolver cssResolver = new StyleAttrCSSResolver(cssFiles);
            HtmlPipelineContext hpc = new HtmlPipelineContext(null);
            hpc.SetAcceptUnknown(true).AutoBookmark(true).SetTagFactory(Tags.GetHtmlTagProcessorFactory());
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