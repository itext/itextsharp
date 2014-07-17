using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.net;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.css {
    /**
     * @author redlab_b
     *
     */
    public class CSSFileProcessorTest {
        private CssFileProcessor proc;
        private IFileRetrieve retriever;
        private const string RESOURCES = @"..\..\resources\";

        [SetUp]
        virtual public void SetUp() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            proc = new CssFileProcessor();
            retriever = new FileRetrieveImpl();
        }

        [Test]
        virtual public void ParseCSS() {
            retriever.ProcessFromStream(File.OpenRead(RESOURCES + "/css/test.css"), proc);
            ICssFile file = proc.GetCss();
            IList<CssRule> rules = file.Get(new Tag("body"));
            Assert.IsTrue(rules.Count == 1);
            Assert.IsTrue(rules[0].NormalDeclarations.ContainsKey("margin"), "margin not found.");
            Assert.AreEqual("20px", rules[0].NormalDeclarations["margin"], "Value for margin not correct.");
        }
    }
}
