/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
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
        virtual public void SetUp() {
            Directory.CreateDirectory(TARGET);
        }

        private CssUtils utils = CssUtils.GetInstance();

        [Test]
        virtual public void ParseXfaOnlyXML() {
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
