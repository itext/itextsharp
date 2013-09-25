using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.text.log;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.parser.io;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.parser {
    /**
 * @author redlab_b
 *
 */

    internal class ParserTest {
        private static List<String> list;
        private static StringBuilder writer;

        private const String RESOURCES = @"..\..\resources\com\itextpdf\tool\xml\parser\";
        private const String TARGET = @"ParserTest\";

        [SetUp]
        public void SetUp() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
        }

        [TearDown]
        public void TearDown() {
            list = null;
            writer = null;
        }

        /**
	 * Validate comment whitespace handling .
	 *
	 * @
	 */

        [Test]
        public void StickyComment() {
            String html = "<p><!--stickycomment-->sometext  moretext</p>";
            String expected = "<p><!--stickycomment-->sometext  moretext</p>";
            XMLParser p = new XMLParser(false, Encoding.Default);
            StringBuilder b = Init(html, p);
            p.Parse(new MemoryStream(Encoding.Default.GetBytes(html)));
            Assert.AreEqual(expected, b.ToString());
        }

        /**
	 * Validate attribute handling of &lt;?abc defg hijklm ?&gt;.
	 *
	 * @
	 */

        [Test]
        public void SpecialTag() {
            String html = "<p><?formServer acrobat8.1dynamic defaultPDFRenderFormat?>ohoh</p>";
            XMLParser p = new XMLParser(false, Encoding.GetEncoding("UTF-8"));
            StringBuilder b = Init(html, p);
            p.Parse(new MemoryStream(Encoding.Default.GetBytes(html)));
            String str = b.ToString();
            Assert.IsTrue(str.Contains("acrobat8.1dynamic") && str.Contains("defaultPDFRenderFormat"));
        }

        private class CustomXMLParserListener : IXMLParserListener {
            public void UnknownText(String text) {
            }


            public void StartElement(String tag, IDictionary<String, String> attributes, String ns) {
            }

            public void Init() {
            }

            public void EndElement(String tag, String ns) {
            }

            public void Comment(String comment) {
            }

            public void Close() {
            }

            public void Text(String text) {
                list.Add(text);
            }
        }

        [Test]
        public void SpecialChars() {
            list = new List<String>();
            XMLParser p = new XMLParser(false, new CustomXMLParserListener());
            p.Parse(File.OpenRead(RESOURCES + "parser.xml"), Encoding.GetEncoding("UTF-8"));
//		org.junit.Assert.assertEquals(new String("e\u00e9\u00e8\u00e7\u00e0\u00f5".getBytes("UTF-8"), "UTF-8"), list.get(0));
//		org.junit.Assert.assertEquals(new String("e\u00e9\u00e8\u00e7\u00e0\u00f5".getBytes("UTF-8"), "UTF-8"), list.get(1));
            Assert.AreEqual("e\u00e9\u00e8\u00e7\u00e0\u00f5", list[0]);
            Assert.AreEqual("e\u00e9\u00e8\u00e7\u00e0\u00f5", list[1]);
        }

        public void ReadBare() {
            StreamReader inpuStreamReader = new StreamReader(RESOURCES + "parser.xml", Encoding.GetEncoding("UTF-8"));
        }

        private class CustomAppender : IAppender {
            public IAppender Append(char c) {
                writer.Append(c);
                return this;
            }

            public IAppender Append(String str) {
                writer.Append(str);
                return this;
            }
        }

        /**
	 * @param html
	 * @param p
	 * @return
	 */

        private StringBuilder Init(String html, XMLParser p) {
            writer = new StringBuilder(html.Length);
            p.AddListener(new ParserListenerWriter(new CustomAppender(), false));
            return writer;
        }
    }
}