using iTextSharp.tool.xml;
using iTextSharp.tool.xml.exceptions;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml {
    internal class TagUtilsTest {
        private Tag sibling1;
        private Tag sibling2;
        private Tag parent;
        private Tag sibling3;

        [SetUp]
        public void SetUp() {
            sibling1 = new Tag("sibling1");
            sibling2 = new Tag("sibling2");
            sibling3 = new Tag("sibling3");
            parent = new Tag("parent");
        }

        /**
	 * Validates that the first next sibling is found
	 * @
	 */

        [Test]
        public void TestSiblingAvailable1() {
            parent.AddChild(sibling1);
            parent.AddChild(sibling2);
            parent.AddChild(sibling3);
            Assert.AreEqual(sibling3, TagUtils.GetInstance().GetSibling(sibling2, 1));
        }

        /**
	 * Validates that the second next sibling is found
	 * @
	 */

        [Test]
        public void TestSiblingAvailable2() {
            parent.AddChild(sibling1);
            parent.AddChild(sibling2);
            parent.AddChild(sibling3);
            Assert.AreEqual(sibling3, TagUtils.GetInstance().GetSibling(sibling1, 2));
        }

        /**
	 * Validates that the previous sibling is found
	 * @
	 */

        [Test]
        public void TestSiblingAvailableMinus1() {
            parent.AddChild(sibling1);
            parent.AddChild(sibling2);
            parent.AddChild(sibling3);
            Assert.AreEqual(sibling1, TagUtils.GetInstance().GetSibling(sibling2, -1));
        }

        /**
	 * Validates that NoSiblingException is thrown when none is found.
	 * @
	 */

        [Test, ExpectedException(typeof (NoSiblingException))]
        public void TestNoSiblingAvailable() {
            parent.AddChild(sibling2);
            parent.AddChild(sibling3);
            TagUtils.GetInstance().GetSibling(sibling2, -1);
        }
    }
}