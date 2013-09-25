using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text.log;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.net;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.css {
    /**
 * @author redlab_b
 *
 */

    internal class CSSFileProcessorTest {
        private CssFileProcessor proc;
        private IFileRetrieve retriever;
        private const string RESOURCES = @"..\..\resources\";

        [SetUp]
        public void SetUp() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            proc = new CssFileProcessor();
            retriever = new FileRetrieveImpl();
        }

        [Test]
        public void ParseCSS() {
            retriever.ProcessFromStream(File.OpenRead(RESOURCES + "/css/test.css"), proc);
            ICssFile file = proc.GetCss();
            IDictionary<String, String> map = file.Get("body");
            Assert.IsTrue(map.ContainsKey("margin"), "margin not found.");
            Assert.AreEqual("20px", map["margin"], "Value for margin not correct.");
        }
    }
}