using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline.ctx;
using NUnit.Framework;
using iTextSharp.tool.xml.pipeline;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.pipeline {
    internal class PipelineTest {
        private AbstractPipelineExtension abstractPipelineExtension;
        private AbstractPipeline ap;
        private IWorkerContext ctx;
        /**
	 *
	 */

        private sealed class AbstractPipelineExtension : AbstractPipeline {
            /**
		 * @param next
		 */

            public AbstractPipelineExtension(IPipeline next) : base(next) {
            }
        }

        private class CustomAbstractPipeline : AbstractPipeline {
            public CustomAbstractPipeline(IPipeline next)
                : base(next) {
            }
        }

        /** Init test. */

        [SetUp]
        public void SetUp() {
            ctx = new WorkerContextImpl();
            abstractPipelineExtension = new AbstractPipelineExtension(null);
            ap = new CustomAbstractPipeline(abstractPipelineExtension);
        }

        /**
	 * Expect a {@link PipelineException} on calling getNewNoCustomContext.
	 * @
	 */

        [Test]
        [ExpectedException(typeof (PipelineException))]
        public void ValidateNoCustomContextExceptionThrown() {
            AbstractPipeline ap = new CustomAbstractPipeline(null);
            ap.GetLocalContext(ctx);
        }

        /**
	 * Verify that getNext actually returns the next pipeline.
	 */

        [Test]
        public void ValidateNext() {
            Assert.AreEqual(abstractPipelineExtension, ap.GetNext());
        }

        /**
	 * Verify that close actually returns the next pipeline.
	 */

        [Test]
        public void ValidateNextClose() {
            Assert.AreEqual(abstractPipelineExtension, ap.Close(ctx, null, null));
        }

        /**
	 * Verify that open actually returns the next pipeline.
	 */

        [Test]
        public void ValidateNextOpen() {
            Assert.AreEqual(abstractPipelineExtension, ap.Open(ctx, null, null));
        }

        /**
	 * Verify that content actually returns the next pipeline.
	 */

        [Test]
        public void ValidateNextContent() {
            Assert.AreEqual(abstractPipelineExtension, ap.Content(ctx, null, null, null));
        }
    }
}