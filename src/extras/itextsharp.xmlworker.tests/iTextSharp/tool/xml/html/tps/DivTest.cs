using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.html.pdfelement;
using iTextSharp.tool.xml.pipeline.ctx;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html.tps {
    /**
 * @author itextpdf.com
 *
 */

    internal class DivTest {
        private Div d = new Div();
        private List<IElement> currentContent = new List<IElement>();
        private WorkerContextImpl workerContextImpl;

        [SetUp]
        virtual public void SetUp() {
            workerContextImpl = new WorkerContextImpl();
            workerContextImpl.Put(typeof (HtmlPipeline).FullName, new HtmlPipelineContext(null));
            currentContent.Add(new Paragraph("titel paragraph"));
            currentContent.Add(Chunk.NEWLINE);
            currentContent.Add(new NoNewLineParagraph("first content text"));
            currentContent.Add(new Paragraph("footer text"));
            d.SetCssAppliers(new CssAppliersImpl());
        }

        /**
	 * Verifies that the call to content of {@link Div} returns a NoNewLineParagraph.
	 */

        [Test]
        virtual public void VerifyContent() {
            IList<IElement> content = d.Content(workerContextImpl, new Tag("div"), "text inside a div tag");
            Assert.IsTrue(content[0] is NoNewLineParagraph);
        }

        /**
	 * Verifies that the numbers of paragraphs returned by {@link Div#end}.
	 */

        [Test]
        virtual public void VerifyNumberOfParagraphs() {
            IList<IElement> endContent = d.End(workerContextImpl, new Tag("div"), currentContent);
            Assert.AreEqual(1, endContent.Count);
        }

        /**
	 * Verifies that the class of the elements returned by {@link Div#end}.
	 */

        [Test]
        virtual public void VerifyIfPdfDiv() {
            IList<IElement> endContent = d.End(workerContextImpl, new Tag("div"), currentContent);
            Assert.IsTrue(endContent[0] is PdfDiv);
        }

        /**
	 * Verifies if {@link Div} is a stack owner. Should be true.
	 */

        [Test]
        virtual public void VerifyIfStackOwner() {
            Assert.IsTrue(d.IsStackOwner());
        }
    }
}
