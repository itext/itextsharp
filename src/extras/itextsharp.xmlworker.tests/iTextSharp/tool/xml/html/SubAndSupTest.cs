using System;
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

    internal class SubAndSupTest {
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
            StreamReader bis = File.OpenText(RESOURCES + "/snippets/br-sub-sup_snippet.html");
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
            Assert.AreEqual(8, elementList.Count); // Br's count for one element(Chunk.NEWLINE).
        }

        [Test]
        public void ResolveNewLines() {
            Assert.AreEqual(Chunk.NEWLINE.Content, elementList[1].Chunks[0].Content);
            Assert.AreEqual(8f, elementList[1].Chunks[0].Font.Size, 0);
            Assert.AreEqual(Chunk.NEWLINE.Content, elementList[4].Chunks[1].Content);
        }

        [Test]
        public void ResolveFontSize() {
            Assert.AreEqual(12, elementList[5].Chunks[0].Font.Size, 0);
            Assert.AreEqual(9.75f, elementList[5].Chunks[2].Font.Size, 0);
            Assert.AreEqual(24, elementList[6].Chunks[0].Font.Size, 0);
            Assert.AreEqual(18f, elementList[6].Chunks[2].Font.Size, 0);
        }

        [Test]
        public void ResolveTextRise() {
            Assert.AreEqual(-9.75f/2, elementList[5].Chunks[2].GetTextRise(), 0);
            Assert.AreEqual(-9.75f/2, elementList[5].Chunks[4].GetTextRise(), 0);
            Assert.AreEqual(18/2 + 0.5, elementList[6].Chunks[2].GetTextRise(), 0);
            Assert.AreEqual(-18/2, elementList[6].Chunks[6].GetTextRise(), 0);
            Assert.AreEqual(0, elementList[7].Chunks[0].GetTextRise(), 0);
            Assert.AreEqual(-3, elementList[7].Chunks[2].GetTextRise(), 0);
            Assert.AreEqual(4, elementList[7].Chunks[14].GetTextRise(), 0);
            Assert.AreEqual(3, elementList[7].Chunks[22].GetTextRise(), 0);
        }

        [Test]
        public void ResolvePhraseLeading() {
            Assert.IsTrue(Math.Abs(1.2f - ((Paragraph) elementList[5]).MultipliedLeading) < 0.0001);
            Assert.IsTrue(Math.Abs(1.2f - ((Paragraph) elementList[6]).MultipliedLeading) < 0.0001);
        }
    }
}