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

    internal class TPBreakTest {
        /**
	 * Verifies that the call to end of {@link Break} returns a Chunk.NEWLINE.
	 */

        [Test]
        virtual public void VerifyBreakChunk() {
            Break br = new Break();
            WorkerContextImpl workerContextImpl = new WorkerContextImpl();
            CssAppliersImpl cssAppliers = new CssAppliersImpl();
            workerContextImpl.Put(typeof (HtmlPipeline).FullName, new HtmlPipelineContext(cssAppliers));
            br.SetCssAppliers(cssAppliers);
            IList<IElement> end = br.End(workerContextImpl, new Tag("span"), null);
            Assert.AreEqual(Chunk.NEWLINE.Content, end[0].Chunks[0].Content);
        }

        /**
	 * Verifies if {@link Break} is a stack owner. Should be false.
	 */

        [Test]
        virtual public void VerifyIfStackOwner() {
            Assert.IsFalse(new Break().IsStackOwner());
        }
    }
}
