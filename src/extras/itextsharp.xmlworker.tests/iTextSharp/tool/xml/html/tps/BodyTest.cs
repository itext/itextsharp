using System.Collections.Generic;
using iTextSharp.text;
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

    internal class BodyTest {
        private Body b = new Body();
        private WorkerContextImpl workerContextImpl;

        [SetUp]
        public void Init() {
            b.SetCssAppliers(new CssAppliersImpl());
            workerContextImpl = new WorkerContextImpl();
            workerContextImpl.Put(typeof (HtmlPipeline).FullName, new HtmlPipelineContext(null));
        }

        /**
	 * Verifies that the call to content of {@link Body} returns a NoNewLineParagraph.
	 */

        [Test]
        public void VerifyContent() {
            IList<IElement> content = b.Content(workerContextImpl, new Tag("body"), "text inside a body tag");
            Assert.IsTrue(content[0] is NoNewLineParagraph);
        }

        /**
	 * Verifies if {@link Body} is a stack owner. Should be false.
	 */

        [Test]
        public void VerifyIfStackOwner() {
            Assert.IsFalse(b.IsStackOwner());
        }
    }
}