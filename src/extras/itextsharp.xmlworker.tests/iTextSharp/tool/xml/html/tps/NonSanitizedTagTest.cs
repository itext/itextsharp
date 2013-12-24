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

    internal class NonSanitizedTagTest {
        private NonSanitizedTag t = new NonSanitizedTag();
        private IList<IElement> content = null;
        private WorkerContextImpl workerContextImpl;

        [SetUp]
        virtual public void init() {
            workerContextImpl = new WorkerContextImpl();
            workerContextImpl.Put(typeof (HtmlPipeline).FullName, new HtmlPipelineContext(null));
            t.SetCssAppliers(new CssAppliersImpl());
            content = t.Content(workerContextImpl, new Tag("pre"), "   code snippet {" +
                                                                   "return it all!!}        ");
        }

        /**
	 * Verifies if the call to content of {@link NonSanitizedTag} returns a Chunk with all white spaces in it.
	 */

        [Test]
        virtual public void VerifyContent() {
            Assert.IsTrue(content[0] is Chunk);
            String unsanitized = content[0].ToString();
            Assert.AreEqual("   code snippet {" +
                            "return it all!!}        ", unsanitized);
        }

        /**
	 * Verifies if the call to end of {@link NonSanitizedTag} returns a NoNewLineParagraph.
	 */

        [Test]
        virtual public void VerifyEnd() {
            Assert.IsTrue(t.End(workerContextImpl, new Tag("pre"), content)[0] is NoNewLineParagraph);
        }

        /**
	 * Verifies if {@link NonSanitizedTag} is a stack owner. Should be false.
	 */

        [Test]
        virtual public void VerifyIfStackOwner() {
            Assert.IsFalse(t.IsStackOwner());
        }
    }
}
