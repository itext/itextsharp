using System;
using System.Collections.Generic;
using iTextSharp.tool.xml;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml {
    internal class XMLWorkerTest {
        private XMLWorker worker;
        private static bool called;

        private class CustomPipeline : IPipeline {
            virtual public IPipeline Open(IWorkerContext context, Tag t, ProcessObject po) {
                called = true;
                return null;
            }

            virtual public IPipeline Init(IWorkerContext context) {
                called = true;
                return null;
            }

            virtual public IPipeline Content(IWorkerContext context, Tag t, String content, ProcessObject po) {
                called = true;
                return null;
            }

            virtual public IPipeline Close(IWorkerContext context, Tag t, ProcessObject po) {
                called = true;
                return null;
            }

            virtual public IPipeline GetNext() {
                return null;
            }
        }

        [SetUp]
        virtual public void SetUp() {
            worker = new XMLWorker(new CustomPipeline(), false);

            called = false;
        }

        [Test]
        virtual public void VerifyPipelineInitCalled() {
            worker.Init();
            Assert.IsTrue(called);
        }

        [Test]
        virtual public void VerifyPipelineOpenCalled() {
            worker.StartElement("test", new Dictionary<String, String>(), "ns");
            Assert.IsTrue(called);
        }

        [Test]
        virtual public void VerifyPipelineContentCalled() {
            worker.StartElement("test", new Dictionary<String, String>(), "ns");
            worker.Text("test");
            Assert.IsTrue(called);
        }

        [Test]
        virtual public void VerifyPipelineContentNotCalledOnNoTag() {
            worker.Text("test");
            Assert.IsFalse(called);
        }

        [Test]
        virtual public void VerifyPipelineCloseCalled() {
            worker.EndElement("test", "ns");
            Assert.IsTrue(called);
        }

        [Test]
        virtual public void VerifyNoCurrentTag() {
            worker.Init();
            Assert.IsNull(worker.GetCurrentTag());
        }

        [Test]
        virtual public void VerifyCurrentTag() {
            worker.StartElement("test", new Dictionary<String, String>(), "ns");
            Assert.NotNull(worker.GetCurrentTag());
        }

        [TearDown]
        virtual public void Clean() {
            worker.Close();
        }
    }
}
