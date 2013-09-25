using System;
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

    internal class SpanTest {
        private Span s = new Span();
        private IList<IElement> content = null;
        private WorkerContextImpl workerContextImpl;

        [SetUp]
        public void SetUp() {
            workerContextImpl = new WorkerContextImpl();
            workerContextImpl.Put(typeof (HtmlPipeline).FullName, new HtmlPipelineContext(null));
            s.SetCssAppliers(new CssAppliersImpl());
            content = s.Content(workerContextImpl, new Tag("span"), "	text snippet " +
                                                                    "return it sanitized!!       ");
        }

        /**
	 * Verifies if the call to content of {@link Span} returns a Chunk with all white spaces removed.
	 */

        [Test]
        public void VerifyContent() {
            Assert.IsTrue(content[1] is Chunk);
            String unsanitized = content[1].ToString();
            Assert.AreEqual("text snippet return it sanitized!!", unsanitized);
        }

        /**
	 * Verifies if the call to end of {@link Span} returns a NoNewLineParagraph.
	 */

        [Test]
        public void VerifyEnd() {
            Assert.IsTrue(s.End(workerContextImpl, new Tag("span"), content)[0] is NoNewLineParagraph);
        }

        /**
	 * Verifies if {@link Span} is a stack owner. Should be true.
	 */

        [Test]
        public void VerifyIfStackOwner() {
            Assert.IsTrue(s.IsStackOwner());
        }
    }
}