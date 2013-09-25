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

    internal class HorAndVertScalingTest {
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
            TextReader bis = File.OpenText(RESOURCES + "/snippets/xfa-hor-vert_snippet.html");
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
            Assert.AreEqual(4, elementList.Count);
        }

        [Test]
        public void ResolveFontSize() {
            Assert.AreEqual(12, elementList[0].Chunks[0].Font.Size, 0);
            Assert.AreEqual(16, elementList[1].Chunks[0].Font.Size, 0);
            Assert.AreEqual(16*1.5, elementList[1].Chunks[2].Font.Size, 0);
            Assert.AreEqual(15, elementList[2].Chunks[0].Font.Size, 0);
            Assert.AreEqual(7.5, elementList[2].Chunks[2].Font.Size, 0);
            Assert.AreEqual(6.375, elementList[2].Chunks[4].Font.Size, 0);
        }

        [Test]
        public void ResolveScaling() {
            Assert.AreEqual(1, elementList[1].Chunks[0].HorizontalScaling, 0);
            Assert.AreEqual(1/1.5f, elementList[1].Chunks[2].HorizontalScaling, 1e-7);
            Assert.AreEqual(1, elementList[2].Chunks[0].HorizontalScaling, 0);
            Assert.AreEqual(1/0.5f, elementList[2].Chunks[2].HorizontalScaling, 0);
            Assert.AreEqual(1/0.5f, elementList[2].Chunks[4].HorizontalScaling, 0);
            Assert.AreEqual(1, elementList[3].Chunks[0].HorizontalScaling, 0);
            Assert.AreEqual(1.5, elementList[3].Chunks[2].HorizontalScaling, 0);
        }
    }
}