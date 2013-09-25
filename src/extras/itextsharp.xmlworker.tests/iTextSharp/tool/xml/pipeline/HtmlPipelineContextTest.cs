using iTextSharp.text;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.pipeline {
    internal class HtmlPipelineContextTest {
        private HtmlPipelineContext ctx;

        [SetUp]
        public void SetUp() {
            ctx = new HtmlPipelineContext(null);
        }

        /**
	 * Verifies the default page size A4.
	 */

        [Test]
        public void VerifyDefaultPageSize() {
            Assert.AreEqual(PageSize.A4, ctx.PageSize);
        }

        /**
	 *  Verifies the default of bookmarks enabled = true.
	 */

        [Test]
        public void VerifyDefaultBookmarksEnabled() {
            Assert.AreEqual(true, ctx.AutoBookmark());
        }

        /**
	 *   Verifies that the default of accepting unknown tags is true
	 */

        [Test]
        public void VerifyDefaultAcceptUnknown() {
            Assert.AreEqual(true, ctx.AcceptUnknown());
        }

        /**
	 *   Verifies that the default roottags
	 */

        [Test]
        public void VerifyDefaultRoottags() {
            Assert.AreEqual(true, ctx.GetRootTags().Contains("div") && ctx.GetRootTags().Contains("body"));
        }

        /**
	 *   Verifies that memory is not null
	 */

        [Test]
        public void VerifyMemory() {
            Assert.NotNull(ctx.GetMemory());
        }

        /**
	 * Verifies that NoImageProviderException is thrown
	 * @throws NoImageProviderException
	 */

        [Test]
        [ExpectedException(typeof (NoImageProviderException))]
        public void VerifyNoImageProvider() {
            ctx.GetImageProvider();
        }
    }
}