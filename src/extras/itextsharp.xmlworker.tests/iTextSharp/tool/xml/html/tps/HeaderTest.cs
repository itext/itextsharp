using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.ctx;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;
using Header = iTextSharp.tool.xml.html.Header;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html.tps {
    /**
 * @author itextpdf.com
 *
 */

    internal class HeaderTest {
        private static Tag H2 = new Tag("h2");
        private Header h = new Header();
        private IList<IElement> content = null;
        private WorkerContextImpl workerContextImpl;

        [SetUp]
        public void init() {
            h.SetCssAppliers(new CssAppliersImpl());
            workerContextImpl = new WorkerContextImpl();
            workerContextImpl.Put(typeof (HtmlPipeline).FullName, new HtmlPipelineContext(null).AutoBookmark(true));
            content = h.Content(workerContextImpl, H2, "text inside a header tag");
        }

        /**
	 * Verifies that the call to content of {@link Header} returns a Chunk.
	 */

        [Test]
        public void VerifyContent() {
            Assert.IsTrue(content[0] is Chunk);
        }

        /**
	 * Verifies if {@link Header#end} returns both a WritableDirectElement and a Paragraph.
	 */

        [Test]
        public void VerifyEnd() {
            IList<IElement> end = h.End(workerContextImpl, H2, content);
            Assert.IsTrue(end[0] is WritableDirectElement);
            Assert.IsTrue(end[1] is Paragraph);
        }

        /**
	 * Verifies if {@link Header} is a stack owner. Should be true.
	 */

        [Test]
        public void VerifyIfStackOwner() {
            Assert.IsTrue(h.IsStackOwner());
        }
    }
}