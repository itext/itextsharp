using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html {
    /**
 * Added to visually check all snippets. (see target/text-classes/*.pdf )
 */

    internal class SamplesTest {
        public static String RESOURCE_TEST_PATH = @"..\..\resources\";
        private string TARGET = @"SamplesTest\";
        private List<String> list = new List<String>();

        static SamplesTest() {
            //	FontFactory.registerDirectories();
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
        }

        [SetUp]
        public void SetUp() {
            Directory.CreateDirectory(TARGET);

            list.Add("widthTable_");
            list.Add("test-table-a_");
            list.Add("test-table-b_");
            list.Add("test-table-c_");
            list.Add("test-table-d_");
            list.Add("xfa-support_");
            list.Add("position_");
            list.Add("b-p_");
            list.Add("br-sub-sup_");
            list.Add("div_");
            list.Add("font_color_");
            list.Add("lineheight_");
//		list.Add("index_");
            list.Add("img_");
            list.Add("h_");
            list.Add("fontSizes_");
            list.Add("line-height_letter-spacing_");
            list.Add("longtext_");
            list.Add("margin-align_");
            list.Add("xfa-hor-vert_");
            list.Add("text-indent_text-decoration_");
            list.Add("comment-double-print_");
            list.Add("tab_");
            list.Add("table_");
            list.Add("tableInTable_");
            list.Add("lists_");
            list.Add("headers_");
        }

        [Test]
        public void CreateAllSamples() {
            bool success = true;
            foreach (String str in list) {
                try {
                    Console.WriteLine(str);
                    Document doc = new Document();
                    PdfWriter writer = null;
                    try {
                        writer = PdfWriter.GetInstance(doc,
                            new FileStream(TARGET + "/" + str + "Test.pdf", FileMode.Create));
                    }
                    catch (DocumentException e) {
                        Console.WriteLine(e);
                    }
                    doc.Open();
                    StreamReader bis = File.OpenText(RESOURCE_TEST_PATH + "/snippets/" + str + "snippet.html");
                    XMLWorkerHelper helper = XMLWorkerHelper.GetInstance();
                    helper.ParseXHtml(writer, doc, bis);
                    doc.Close();
                }
                catch (Exception e) {
                    Console.Write(e);
                    success = false;
                }
            }
            if (!success) {
                Assert.Fail();
            }
        }
    }
}