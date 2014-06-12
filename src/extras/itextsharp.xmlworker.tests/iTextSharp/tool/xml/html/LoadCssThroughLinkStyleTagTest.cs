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
