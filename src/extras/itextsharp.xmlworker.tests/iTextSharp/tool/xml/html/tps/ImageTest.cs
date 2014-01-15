using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.ctx;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;
using Image = iTextSharp.tool.xml.html.Image;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html.tps {
    /**
     * @author itextpdf.com
     *
     */
    internal class ImageTest {
        static ImageTest() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(5));
        }

        private static Tag I = new Tag("i");
        private Image i = new Image();
        private IWorkerContext workerContextImpl;
        private const String RESOURCES = @"..\..\resources\";

        [SetUp]
        virtual public void SetUp() {
            i.SetCssAppliers(new CssAppliersImpl());
            I.Attributes[HTML.Attribute.SRC] = RESOURCES + "images.jpg";
            workerContextImpl = new WorkerContextImpl();
            workerContextImpl.Put(typeof (HtmlPipeline).FullName, new HtmlPipelineContext(null));
        }

        /**
	     * Verifies that the call to end of {@link Image} returns a List<IElement> containing a Chunk with a Image in it.
	     */
        [Test]
        virtual public void VerifyEnd() {
            IList<IElement> content = i.End(workerContextImpl, I, null);
            Assert.IsTrue(content[0] is Chunk);
            Dictionary<String, Object> attributes = ((Chunk) content[0]).Attributes;
            Assert.IsTrue(attributes.ContainsKey("IMAGE"));
        }

        /**
	     * Verifies if {@link Image} is a stack owner. Should be false.
	     */
        [Test]
        virtual public void VerifyIfStackOwner() {
            Assert.IsFalse(i.IsStackOwner());
        }
    }
}
