using System;
using System.Collections.Generic;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.ctx;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.pipeline {
    internal class CssResolverPipelineTest {
        private IDictionary<String, String> css2;

        [SetUp]
        virtual public void SetUp() {
            StyleAttrCSSResolver css = new StyleAttrCSSResolver();
            css.AddCss("dummy { key1: value1; key2: value2 } .aklass { key3: value3;} #dumid { key4: value4}", true);
            CssResolverPipeline p = new CssResolverPipeline(css, null);
            Tag t = new Tag("dummy");
            t.Attributes["id"] = "dumid";
            t.Attributes["class"] = "aklass";
            WorkerContextImpl context = new WorkerContextImpl();
            p.Init(context);
            IPipeline open = p.Open(context, t, null);
            css2 = t.CSS;
        }

        /**
	 * Verify that pipeline resolves css on tag.
	 *
	 * @throws CssResolverException
	 * @throws PipelineException
	 */

        [Test]
        virtual public void VerifyCssResolvedTag() {
            Assert.AreEqual("value1", css2["key1"]);
        }

        /**
	 * Verify that pipeline resolves css on tag2.
	 *
	 * @throws CssResolverException
	 * @throws PipelineException
	 */

        [Test]
        virtual public void VerifyCssResolvedTag2() {
            Assert.AreEqual("value2", css2["key2"]);
        }

        /**
	 * Verify that pipeline resolves css class.
	 *
	 * @throws CssResolverException
	 * @throws PipelineException
	 */

        [Test]
        virtual public void VerifyCssResolvedClass() {
            Assert.AreEqual("value3", css2["key3"]);
        }

        /**
	 * Verify that pipeline resolves css id.
	 *
	 * @throws CssResolverException
	 * @throws PipelineException
	 */

        [Test]
        virtual public void VerifyCssResolvedId() {
            Assert.AreEqual("value4", css2["key4"]);
        }
    }
}
