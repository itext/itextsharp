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
        virtual public void SetUp() {
            Directory.CreateDirectory(TARGET);
        }

        [Test]
        virtual public void ParseXfaOnlyXML() {
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
