using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html {
    /**
 * @author Balder
 *
 */

    internal class PandBTest {
        private static List<IElement> elementList;
        private const string RESOURCES = @"..\..\resources\";

        private class CustomElementHandler : IElementHandler {
            public void Add(IWritable w) {
                elementList.AddRange(((WritableElement) w).Elements());
            }
        }

        [SetUp]
        public void SetUp() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            StreamReader bis = File.OpenText(RESOURCES + "/snippets/b-p_snippet.html");
            XMLWorkerHelper helper = XMLWorkerHelper.GetInstance();
            elementList = new List<IElement>();
            helper.ParseXHtml(new CustomElementHandler(), bis);
        }

        [TearDown]
        public void TearDown() {
            elementList = null;
        }

        [Test]
        public void ResolveNumberOfElements() {
            Assert.AreEqual(7, elementList.Count);
        }
    }
}