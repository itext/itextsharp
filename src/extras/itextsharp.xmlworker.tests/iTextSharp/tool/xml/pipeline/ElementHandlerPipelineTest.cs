using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline;
using iTextSharp.tool.xml.pipeline.ctx;
using iTextSharp.tool.xml.pipeline.end;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.pipeline {
    internal class ElementHandlerPipelineTest {
        private static List<IWritable> lst;
        private ProcessObject po;
        private ElementHandlerPipeline p;
        private WritableElement writable;
        private WorkerContextImpl context;

        private class CustomElementHandler : IElementHandler {
            public void Add(IWritable w) {
                lst.Add(w);
            }
        }

        [SetUp]
        public void SetUp() {
            lst = new List<IWritable>();
            IElementHandler elemH = new CustomElementHandler();

            p = new ElementHandlerPipeline(elemH, null);
            po = new ProcessObject();
            writable = new WritableElement(new Chunk("aaaaa"));
            po.Add(writable);
            context = new WorkerContextImpl();
            p.Init(context);
        }

        /**
	 * Verifies that the content of the ProcessObject is processed on open.
	 * @
	 */

        [Test]
        public void RunOpen() {
            p.Open(context, null, po);
            Assert.AreEqual(writable, lst[0]);
        }

        /**
	 * Verifies that the content of the ProcessObject is processed on content.
	 * @
	 */

        [Test]
        public void RunContent() {
            p.Content(context, null, null, po);
            Assert.AreEqual(writable, lst[0]);
        }

        /**
	 * Verifies that the content of the ProcessObject is processed on close.
	 * @
	 */

        [Test]
        public void RunClose() {
            p.Close(context, null, po);
            Assert.AreEqual(writable, lst[0]);
        }
    }
}