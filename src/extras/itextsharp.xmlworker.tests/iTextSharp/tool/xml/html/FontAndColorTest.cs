using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline;
using NUnit.Framework;
using iTextSharp.tool.xml.css.apply;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html {
    /**
 * @author Balder
 */

    internal class FontAndColorTest {
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

            TextReader bis = File.OpenText(RESOURCES + "/snippets/font_color_snippet.html");
            XMLWorkerHelper helper = XMLWorkerHelper.GetInstance();
            elementList = new List<IElement>();
            helper.ParseXHtml(new CustomElementHandler(), bis);
        }

        [Test]
        virtual public void ResolveFontSize() {
            Tag t = new Tag("t");
            t.CSS["font-size"] = "12pt";
            Chunk c = new Chunk("default text with no styles attached.");
            c = new ChunkCssApplier().Apply(c, t);
            Assert.AreEqual(12, c.Font.Size, 0);
            t.CSS["font-size"] = "18pt";
            c = new ChunkCssApplier().Apply(c, t);
            Assert.AreEqual(18, c.Font.Size, 0);
        }

        [Test]
        virtual public void ResolveColor() {
            Assert.AreEqual(BaseColor.BLACK, elementList[0].Chunks[0].Font.Color);
            Assert.AreEqual(255, elementList[3].Chunks[0].Font.Color.B);
            Assert.AreEqual(255, elementList[3].Chunks[2].Font.Color.G);
            Assert.AreEqual(255, elementList[3].Chunks[10].Font.Color.R);
        }

        [TearDown]
        virtual public void TearDown() {
            elementList = null;
        }
    }
}
