using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.tool.xml.parser;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.parser {
    /**
 * @author itextpdf.com
 *
 */

    internal class HTMLWhiteSpacesTest {
        private String str;
        private static StringBuilder b;

        [SetUp]
        public void SetUp() {
            str = "<body><b>&euro;<b> 124</body>";
        }

        private class CustomXMLParserListener : IXMLParserListener {
            public void UnknownText(String text) {
            }

            public void Text(String text) {
                b.Append(text);
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
        }

        /**
	 * See that a space is not removed after a special char.
	 * @throws IOException
	 */

        [Test]
        public void CheckIfSpaceIsStillThere() {
            b = new StringBuilder();
            XMLParser p = new XMLParser(true, new CustomXMLParserListener());
            p.Parse(new StringReader(str));
            Assert.AreEqual("\u20ac 124", b.ToString());
        }

        [TearDown]
        public void TearDown() {
            b = null;
        }
    }
}