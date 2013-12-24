using System;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.pipeline {
    internal class HtmlPipelineCloneTest {
        private HtmlPipelineContext clone;
        private HtmlPipelineContext ctx;

        private class CustomAbstractImageProvider : AbstractImageProvider {
            public override String GetImageRootPath() {
                return "42 is the answer";
            }
        }

        /**
	 * @throws java.lang.Exception
	 */

        [SetUp]
        virtual public void SetUp() {
            ctx = new HtmlPipelineContext(null);
            ctx.SetImageProvider(new CustomAbstractImageProvider());
            clone = (HtmlPipelineContext) ctx.Clone();
        }

        [Test]
        virtual public void VerifyNewImageProvider() {
            Assert.AreNotSame(ctx.GetImageProvider(), clone.GetImageProvider());
        }

        [Test]
        virtual public void VerifyNewRoottags() {
            Assert.AreNotSame(ctx.GetRootTags(), clone.GetRootTags());
        }

        [Test]
        virtual public void VerifyNewPageSize() {
            Assert.AreNotSame(ctx.PageSize, clone.PageSize);
        }

        [Test]
        virtual public void VerifyNewMemory() {
            Assert.AreNotSame(ctx.GetMemory(), clone.GetMemory());
        }

        [Test]
        virtual public void VerifySameLinkProvider() {
            Assert.AreEqual(ctx.GetLinkProvider(), clone.GetLinkProvider());
        }
    }
}
