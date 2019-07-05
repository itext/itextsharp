/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
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
        virtual public void SetUp() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
        }

        [TearDown]
        virtual public void TearDown() {
            list = null;
            writer = null;
        }

        /**
	 * Validate comment whitespace handling .
	 *
	 * @
	 */

        [Test]
        virtual public void StickyComment() {
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
        virtual public void SpecialTag() {
            String html = "<p><?formServer acrobat8.1dynamic defaultPDFRenderFormat?>ohoh</p>";
            XMLParser p = new XMLParser(false, Encoding.GetEncoding("UTF-8"));
            StringBuilder b = Init(html, p);
            p.Parse(new MemoryStream(Encoding.Default.GetBytes(html)));
            String str = b.ToString();
            Assert.IsTrue(str.Contains("acrobat8.1dynamic") && str.Contains("defaultPDFRenderFormat"));
        }

        private class CustomXMLParserListener : IXMLParserListener {
            virtual public void UnknownText(String text) {
            }


            virtual public void StartElement(String tag, IDictionary<String, String> attributes, String ns) {
            }

            virtual public void Init() {
            }

            virtual public void EndElement(String tag, String ns) {
            }

            virtual public void Comment(String comment) {
            }

            virtual public void Close() {
            }

            virtual public void Text(String text) {
                list.Add(text);
            }
        }

        [Test]
        virtual public void SpecialChars() {
            list = new List<String>();
            XMLParser p = new XMLParser(false, new CustomXMLParserListener());
            p.Parse(File.OpenRead(RESOURCES + "parser.xml"), Encoding.GetEncoding("UTF-8"));
//		org.junit.Assert.assertEquals(new String("e\u00e9\u00e8\u00e7\u00e0\u00f5".getBytes("UTF-8"), "UTF-8"), list.get(0));
//		org.junit.Assert.assertEquals(new String("e\u00e9\u00e8\u00e7\u00e0\u00f5".getBytes("UTF-8"), "UTF-8"), list.get(1));
            Assert.AreEqual("e\u00e9\u00e8\u00e7\u00e0\u00f5", list[0]);
            Assert.AreEqual("e\u00e9\u00e8\u00e7\u00e0\u00f5", list[1]);
        }

        virtual public void ReadBare() {
            StreamReader inpuStreamReader = new StreamReader(RESOURCES + "parser.xml", Encoding.GetEncoding("UTF-8"));
        }

        private class CustomAppender : IAppender {
            virtual public IAppender Append(char c) {
                writer.Append(c);
                return this;
            }

            virtual public IAppender Append(String str) {
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
