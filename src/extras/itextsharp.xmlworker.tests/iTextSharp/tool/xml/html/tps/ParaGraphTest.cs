using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.ctx;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html.tps {
    /**
 * @author itextpdf.com
 *
 */

    internal class ParaGraphTest {
        private ParaGraph p = new ParaGraph();
        private List<IElement> currentContent = new List<IElement>();
        private WorkerContextImpl workerContextImpl;

        [SetUp]
        public void SetUp() {
            workerContextImpl = new WorkerContextImpl();
            p.SetCssAppliers(new CssAppliersImpl());
            workerContextImpl.Put(typeof (HtmlPipeline).FullName, new HtmlPipelineContext(null));
            currentContent.AddRange(p.Content(workerContextImpl, new Tag("p"), "some paragraph text"));
        }

        /**
	 * Verifies that the call to content of {@link ParaGraph} returns a Chunk.
	 */

        [Test]
        public void VerifyContent() {
            Assert.IsTrue(currentContent[0] is Chunk);
        }

        /**
	 * Verifies if the class of the elements returned by {@link ParaGraph#end} is a Paragraph.
	 */

        [Test]
        public void VerifyEnd() {
            IList<IElement> endContent = p.End(workerContextImpl, new Tag("p"), currentContent);
            Assert.IsTrue(endContent[0] is Paragraph);
        }

        /**
	 * Verifies if {@link ParaGraph} is a stack owner. Should be true.
	 */

        [Test]
        public void VerifyIfStackOwner() {
            Assert.IsTrue(p.IsStackOwner());
        }
    }
}