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
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.net;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html {
    /**
 * @author redlab_b
 *
 */

    internal class LoadCssThroughLinkStyleTagTest {
        private static String HTML1 =
            "<html><head><link type='text/css' rel='stylesheet' href='style.css'/></head><body><p>Import css files test</p></body></html>";

        private static String HTML2 =
            "<html><head><link type='text/css' rel='stylesheet' href='style.css'/><link type='text/css' rel='stylesheet' href='test.css'/></head><body><p>Import css files test</p></body></html>";

        private static String HTML3 =
            "<html><head><link type='text/css' rel='stylesheet' href='style.css'/><link type='text/css' rel='stylesheet' href='test.css'/><style type='text/css'>body {padding: 5px;}</style></head><body><p>Import css files test</p></body></html>";

        private XMLParser p;
        private CssFilesImpl cssFiles;

        private const string RESOURCES = @"..\..\resources\";

        [SetUp]
        virtual public void SetUp() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            cssFiles = new CssFilesImpl();
            String path = RESOURCES + @"\css\test.css";
            path = path.Substring(0, path.LastIndexOf("test.css"));
            FileRetrieveImpl r = new FileRetrieveImpl(new String[] {path});
            StyleAttrCSSResolver cssResolver = new StyleAttrCSSResolver(cssFiles, r);
            HtmlPipelineContext hpc = new HtmlPipelineContext(null);
            hpc.SetAcceptUnknown(false).AutoBookmark(true).SetTagFactory(Tags.GetHtmlTagProcessorFactory());
            IPipeline pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(hpc, null));
            XMLWorker worker = new XMLWorker(pipeline, true);
            p = new XMLParser(worker);
        }

        [Test]
        virtual public void Parse1CssFileAndValidate() {
            p.Parse(new StringReader(HTML1));
            Dictionary<String, String> props = new Dictionary<String, String>();
            cssFiles.PopulateCss(new Tag("body"), props);
            Assert.IsTrue(props.ContainsKey("font-size"));
            Assert.IsTrue(props.ContainsKey("color"));
        }

        [Test]
        virtual public void Parse2CsszFileAndValidate() {
            p.Parse(new StringReader(HTML2));
            Dictionary<String, String> props = new Dictionary<String, String>();
            cssFiles.PopulateCss(new Tag("body"), props);
            Assert.IsTrue(props.ContainsKey("font-size"));
            Assert.IsTrue(props.ContainsKey("color"));
            Assert.IsTrue(props.ContainsKey("margin-left"));
            Assert.IsTrue(props.ContainsKey("margin-right"));
            Assert.IsTrue(props.ContainsKey("margin-top"));
            Assert.IsTrue(props.ContainsKey("margin-bottom"));
        }

        [Test]
        virtual public void Parse2CsszFilePluseStyleTagAndValidate() {
            p.Parse(new StringReader(HTML3));
            Dictionary<String, String> props = new Dictionary<String, String>();
            cssFiles.PopulateCss(new Tag("body"), props);
            Assert.IsTrue(props.ContainsKey("font-size"));
            Assert.IsTrue(props.ContainsKey("color"));
            Assert.IsTrue(props.ContainsKey("margin-left"));
            Assert.IsTrue(props.ContainsKey("margin-right"));
            Assert.IsTrue(props.ContainsKey("margin-top"));
            Assert.IsTrue(props.ContainsKey("margin-bottom"));
            Assert.IsTrue(props.ContainsKey("padding-left"));
            Assert.IsTrue(props.ContainsKey("padding-right"));
            Assert.IsTrue(props.ContainsKey("padding-top"));
            Assert.IsTrue(props.ContainsKey("padding-bottom"));
        }
    }
}
