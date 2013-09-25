using iTextSharp.tool.xml;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml {
    internal class TagTest {
        /**
	 * See that the attribute map is initialized
	 */

        [Test]
        public void ValidateAttributesNotNull() {
            Tag t = new Tag("dummy");
            Assert.NotNull(t.Attributes);
        }

        /**
	 * See that the css map is initialized
	 */

        [Test]
        public void ValidateCssNotNull() {
            Tag t = new Tag("dummy");
            Assert.NotNull(t.CSS);
        }

        /**
	 * See that the children is list
	 */

        [Test]
        public void ValidateChildrenNotNull() {
            Tag t = new Tag("dummy");
            Assert.NotNull(t.Children);
        }

        /**
	 * Validates that the tags parent is set when adding it as a child
	 */

        [Test]
        public void ValidateParentSetOnAdd() {
            Tag t = new Tag("pappie");
            Tag t2 = new Tag("baby");
            t.AddChild(t2);
            Assert.AreEqual(t, t2.Parent);
        }

        /**
	 * Validates that the parent tag does not have the tag set as child when set parent is
	 * called as parent.
	 */

        [Test]
        public void ValidateChildNotSetOnSetParent() {
            Tag t = new Tag("pappie");
            Tag t2 = new Tag("baby");
            t2.Parent = t;
            Assert.AreEqual(0, t.Children.Count);
        }

        /**
	 * Compare equal tag names.
	 */

        [Test]
        public void CompareTrue() {
            Assert.IsTrue(new Tag("pappie").CompareTag(new Tag("pappie")));
        }

        /**
	 * Compare notEqual tag names
	 */

        [Test]
        public void CompareFalse() {
            Assert.IsFalse(new Tag("pappie").CompareTag(new Tag("lappie")));
        }

        /**
	 * Compare equal namespace (and tagname)
	 */

        [Test]
        public void CompareTrueNS() {
            Assert.IsTrue(new Tag("pappie", "ns").CompareTag(new Tag("pappie", "ns")));
        }

        /**
	 * Compare different namespace (and equal tagname).
	 */

        [Test]
        public void CompareFalseNS() {
            Assert.IsFalse(new Tag("pappie", "ns").CompareTag(new Tag("pappie", "xs")));
        }

        /**
	 * Compare different namespace and different tagname.
	 */

        [Test]
        public void CompareFalseTagAndNS() {
            Assert.IsFalse(new Tag("pappie", "ns").CompareTag(new Tag("mammie", "xs")));
        }
    }
}