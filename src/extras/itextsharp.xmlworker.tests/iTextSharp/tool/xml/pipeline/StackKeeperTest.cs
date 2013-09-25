using iTextSharp.text;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.pipeline {
    internal class StackKeeperTest {
        private Chunk a;
        private Chunk b;
        private Chunk c;
        private StackKeeper sk;
        private Tag t;

        [SetUp]
        public void SetUp() {
            t = new Tag("root");
            sk = new StackKeeper(t);
            a = new Chunk("a");
            sk.Add(a);
            b = new Chunk("b");
            sk.Add(b);
            c = new Chunk("c");
            sk.Add(c);
        }

        [Test]
        public void ValidateFirstAddedElementIsFirst() {
            Assert.AreEqual(a, sk.GetElements()[0]);
        }

        [Test]
        public void ValidateMiddleIsMiddle() {
            Assert.AreEqual(b, sk.GetElements()[1]);
        }

        [Test]
        public void ValidateLastIsLast() {
            Assert.AreEqual(c, sk.GetElements()[2]);
        }

        [Test]
        public void ValidateCount() {
            Assert.AreEqual(3, sk.GetElements().Count);
        }

        [Test]
        public void ValidateTag() {
            Assert.AreEqual(t, sk.GetTag());
        }
    }
}