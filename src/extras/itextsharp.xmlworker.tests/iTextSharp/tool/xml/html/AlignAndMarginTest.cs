using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.pipeline;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html {
    /**
 * @author Balder
 *
 */

    internal class AlignAndMarginTest {
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
            StreamReader bis = File.OpenText(RESOURCES + "/snippets/margin-align_snippet.html");
            XMLWorkerHelper helper = XMLWorkerHelper.GetInstance();
            elementList = new List<IElement>();
            helper.ParseXHtml(new CustomElementHandler(), bis);
        }

        /*
	[Test]
	public void resolveNumberOfElements()  {
	Assert.AreEqual(5, elementList.size());
	}

	[Test]
	public void resolveAlignment()  {
	Assert.AreEqual(Element.ALIGN_CENTER,((Paragraph)elementList[0]).getAlignment());
	Assert.AreEqual(Element.ALIGN_LEFT, ((Paragraph)elementList[1]).getAlignment());
	Assert.AreEqual(Element.ALIGN_RIGHT, ((Paragraph)elementList[2]).getAlignment());
	Assert.AreEqual(Element.ALIGN_LEFT, ((Paragraph)elementList[3]).getAlignment());
	}
	*/

        [Test]
        public void ResolveIndentations() {
            CssUtils cssUtils = CssUtils.GetInstance();
            Assert.AreEqual(cssUtils.ParseRelativeValue("1em", 12), ((Paragraph) elementList[0]).SpacingBefore, 0);
            Assert.AreEqual(cssUtils.ParsePxInCmMmPcToPt("1.5in"), ((Paragraph) elementList[1]).IndentationLeft, 0);
            Assert.AreEqual(cssUtils.ParsePxInCmMmPcToPt("3cm"), ((Paragraph) elementList[2]).IndentationRight, 0);
            Assert.AreEqual(cssUtils.ParsePxInCmMmPcToPt("5pc") - 12, ((Paragraph) elementList[3]).SpacingBefore, 0);
            Assert.AreEqual(cssUtils.ParsePxInCmMmPcToPt("4pc"), ((Paragraph) elementList[3]).SpacingAfter, 0);
            Assert.AreEqual(cssUtils.ParsePxInCmMmPcToPt("80px") - cssUtils.ParsePxInCmMmPcToPt("4pc"),
                ((Paragraph) elementList[4]).SpacingBefore, 0);
            Assert.AreEqual(cssUtils.ParsePxInCmMmPcToPt("80px"), ((Paragraph) elementList[4]).SpacingAfter, 0);
        }

        [TearDown]
        public void TearDown() {
            elementList = null;
        }
    }
}