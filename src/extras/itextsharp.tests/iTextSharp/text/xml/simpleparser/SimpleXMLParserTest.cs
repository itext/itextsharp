using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.text.xml.simpleparser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.xml.simpleparser {
    internal class SimpleXMLParserTest {
        /**
	     * Validate correct whitespace handling of SimpleXMLHandler.
	     * Carriage return received as text instead of space.
	     */

        private class SimpleXMLDocHandlerTest : ISimpleXMLDocHandler {
            public StringBuilder b = new StringBuilder();

            public void Text(String str) {
                b.Append(str);
            }

            public void StartElement(String tag, IDictionary<String, String> h) {
            }

            public void StartDocument() {
            }

            public void EndElement(String tag) {
            }

            public void EndDocument() {
            }
        }

        [Test]
        public void WhitespaceHtml() {
            String whitespace = "<p>sometext\r moretext</p>";
            String expected = "sometext moretext";

            SimpleXMLDocHandlerTest docHandler = new SimpleXMLDocHandlerTest();
            SimpleXMLParser.Parse(docHandler, null, new StringReader(whitespace), true);
            Assert.AreEqual(expected, docHandler.b.ToString());
        }
    }
}
