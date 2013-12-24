using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html {
    /**
 * @author Balder
 *
 */

    internal class LineHeightLetterSpacingTest {
        private ITagProcessorFactory factory;
        private XMLWorker worker;
        private MemoryStream baos;
        private XMLWorkerHelper workerFactory;
        private static List<IElement> elementList;

        private const string RESOURCES = @"..\..\resources\";

        private class CustomElementHandler : IElementHandler {
            virtual public void Add(IWritable w) {
                elementList.AddRange(((WritableElement) w).Elements());
            }
        }

        [SetUp]
        virtual public void SetUp() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            StreamReader bis = File.OpenText(RESOURCES + "/snippets/line-height_letter-spacing_snippet.html");
            XMLWorkerHelper helper = XMLWorkerHelper.GetInstance();
            elementList = new List<IElement>();
            helper.ParseXHtml(new CustomElementHandler(), bis);
        }

        [TearDown]
        virtual public void TearDown() {
            elementList = null;
        }

        [Test]
        virtual public void ResolveNumberOfElements() {
            Assert.AreEqual(7, elementList.Count);
        }

        [Test]
        virtual public void ResolveFontSize() {
            Assert.AreEqual(16, elementList[2].Chunks[0].Font.Size, 0);
            Assert.AreEqual(15, elementList[4].Chunks[0].Font.Size, 0);
        }

        [Test]
        virtual public void ResolveLeading() {
            Assert.IsTrue(Math.Abs(1.2f - ((Paragraph) elementList[0]).MultipliedLeading) < 0.0001f);
            Assert.AreEqual(8, ((Paragraph) elementList[1]).Leading, 0);
            // leading laten bepalen door inner line-height setting?
            Assert.AreEqual(160, ((Paragraph) elementList[2]).Leading, 0);
            Assert.AreEqual(21, ((Paragraph) elementList[3]).Leading, 0); //1.75em
            Assert.AreEqual(45, ((Paragraph) elementList[4]).Leading, 0);
        }

        [Test]
        virtual public void ResolveCharSpacing() {
            Assert.AreEqual(CssUtils.GetInstance().ParsePxInCmMmPcToPt("1.6pc"),
                elementList[5].Chunks[0].GetCharacterSpacing(), 0);
            Assert.AreEqual(CssUtils.GetInstance().ParseRelativeValue("0.83em", elementList[6].Chunks[2].Font.Size),
                elementList[6].Chunks[2].GetCharacterSpacing(), 0);
        }
    }
}
