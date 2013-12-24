using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.ctx;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;
using Anchor = iTextSharp.tool.xml.html.Anchor;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html.tps {
    /**
 * @author itextpdf.com
 *
 */

    internal class AnchorTest {
        /**
	 * Validates that the content of an &lt;a&gt; is transformed to a Chunk.
	 */

        [Test]
        virtual public void TestContentToChunk() {
            Anchor a = new Anchor();
            Tag t = new Tag("dummy");
            String content2 = "some content";
            WorkerContextImpl ctx = new WorkerContextImpl();
            HtmlPipelineContext htmlPipelineContext = new HtmlPipelineContext(null);
            ctx.Put(typeof (HtmlPipeline).FullName, htmlPipelineContext);
            a.SetCssAppliers(new CssAppliersImpl());
            IList<IElement> ct = a.Content(ctx, t, content2);
            Assert.AreEqual(content2, ct[0].Chunks[0].Content);
        }

        /**
	 * Verifies if {@link Anchor} is a stack owner. Should be true.
	 */

        [Test]
        virtual public void VerifyIfStackOwner() {
            Assert.IsTrue(new Anchor().IsStackOwner());
        }
    }
}
