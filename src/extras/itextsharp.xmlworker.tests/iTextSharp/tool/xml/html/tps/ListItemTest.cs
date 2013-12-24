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

    internal class ListItemTest {
        private OrderedUnorderedListItem li = new OrderedUnorderedListItem();
        private List<IElement> currentContent = new List<IElement>();
        private WorkerContextImpl workerContextImpl;

        [SetUp]
        virtual public void init() {
            li.SetCssAppliers(new CssAppliersImpl());
            workerContextImpl = new WorkerContextImpl();
            workerContextImpl.Put(typeof (HtmlPipeline).FullName, new HtmlPipelineContext(null));
            currentContent.AddRange(li.Content(workerContextImpl, new Tag("li"), "list item"));
        }

        /**
	 * Verifies that the call to content of {@link OrderedUnorderedListItem} returns a Chunk.
	 */

        [Test]
        virtual public void VerifyContent() {
            Assert.IsTrue(currentContent[0] is Chunk);
        }

        /**
	 * Verifies if the class of the elements returned by {@link OrderedUnorderedListItem#end} is a ListItem.
	 */

        [Test]
        virtual public void VerifyEnd() {
            IList<IElement> endContent = li.End(workerContextImpl, new Tag("li"), currentContent);
            Assert.IsTrue(endContent[0] is ListItem);
        }

        /**
	 * Verifies if {@link OrderedUnorderedListItem} is a stack owner. Should be true.
	 */

        [Test]
        virtual public void VerifyIfStackOwner() {
            Assert.IsTrue(li.IsStackOwner());
        }
    }
}
