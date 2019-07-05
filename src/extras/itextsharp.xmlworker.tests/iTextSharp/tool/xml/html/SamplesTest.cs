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
        virtual public void SetUp() {
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
        virtual public void CreateAllSamples() {
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
