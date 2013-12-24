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

            virtual public void Text(String str) {
                b.Append(str);
            }

            virtual public void StartElement(String tag, IDictionary<String, String> h) {
            }

            virtual public void StartDocument() {
            }

            virtual public void EndElement(String tag) {
            }

            virtual public void EndDocument() {
            }
        }

        [Test]
        virtual public void WhitespaceHtml() {
            String whitespace = "<p>sometext\r moretext</p>";
            String expected = "sometext moretext";

            SimpleXMLDocHandlerTest docHandler = new SimpleXMLDocHandlerTest();
            SimpleXMLParser.Parse(docHandler, null, new StringReader(whitespace), true);
            Assert.AreEqual(expected, docHandler.b.ToString());
        }
    }
}
