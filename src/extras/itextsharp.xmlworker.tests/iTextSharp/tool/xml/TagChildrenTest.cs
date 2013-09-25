using System;
using iTextSharp.tool.xml;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml {
    internal class TagChildrenTest {
        /**
	 *
	 */
        private static String CHILDS_CHILD = "childsChild";
        /**
	 *
	 */
        private static String CHILD2 = "child2";
        /**
	 *
	 */
        private static String CHILD1 = "child1";
        /**
	 *
	 */
        private static String ROOTSTR = "root";
        private Tag root;
        private Tag child2WithChild;
        private Tag child1NoChildren;
        private Tag childsChild;

        /**
	 * Init.
	 */

        [SetUp]
        public void SetUp() {
            root = new Tag(ROOTSTR);
            child1NoChildren = new Tag(CHILD1);
            root.AddChild(child1NoChildren);
            child2WithChild = new Tag(CHILD2);
            childsChild = new Tag(CHILDS_CHILD);
            child2WithChild.AddChild(childsChild);
            root.AddChild(child2WithChild);
        }

        /**
	 * Test {@link Tag#GetChild(String, String)}.
	 */

        [Test]
        public void GetChild() {
            Assert.AreEqual(child1NoChildren, root.GetChild(CHILD1, ""));
        }

        /**
	 * Test {@link Tag#GetChild(String, String, boolean)}.
	 */

        [Test]
        public void GetChildRecursive() {
            Assert.AreEqual(childsChild, root.GetChild(CHILDS_CHILD, "", true));
        }

        /**
	 * Test {@link Tag#HasChild(String, String)}.
	 */

        [Test]
        public void HasChild() {
            Assert.IsTrue(root.HasChild(CHILD1, ""));
        }

        /**
	 * Test {@link Tag#HasChild(String, String, boolean)}.
	 */

        [Test]
        public void HasChildRecursive() {
            Assert.IsTrue(root.HasChild(CHILDS_CHILD, "", true));
        }

        /**
	 * Test {@link Tag#hasParent()}.
	 */

        [Test]
        public void HasParent() {
            Assert.IsTrue(child1NoChildren.HasParent());
        }

        /**
	 * Test {@link Tag#HasChildren()}.
	 */

        [Test]
        public void HasChildren() {
            Assert.IsTrue(child2WithChild.HasChildren());
        }
    }
}