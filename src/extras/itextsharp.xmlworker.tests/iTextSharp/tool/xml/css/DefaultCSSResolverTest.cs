using System;
using System.Collections.Generic;
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.css {
    /**
 * @author redlab_b
 *
 */

    internal class DefaultCSSResolverTest {
        private StyleAttrCSSResolver css;
        private Tag parent;
        private Tag child;

        /**
	 * Setup.
	 */

        [SetUp]
        public void SetUp() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            css = new StyleAttrCSSResolver();
            Dictionary<String, String> pAttr = new Dictionary<String, String>();
            pAttr["style"] = "fontk: Verdana; color: blue; test: a";
            parent = new Tag("parent", pAttr);
            Dictionary<String, String> cAttr = new Dictionary<String, String>();
            cAttr["style"] = "fontk: Arial; font-size: large; test: b";
            child = new Tag("child", cAttr);
            child.Parent = parent;
        }

        /**
	 * Resolves the style attribute to css for 1 tag.
	 */

        [Test]
        public void ResolveTagCss() {
            css.ResolveStyles(parent);
            IDictionary<String, String> css2 = parent.CSS;
            Assert.IsTrue(css2.ContainsKey("fontk"), "font not found");
            Assert.IsTrue(css2.ContainsKey("color"), "color not found");
            Assert.AreEqual("Verdana", css2["fontk"]);
            Assert.AreEqual("blue", css2["color"]);
        }

        /**
	 * Resolves the style attribute to css for parent and child.
	 */

        [Test]
        public void ResolveChildTagCss() {
            css.ResolveStyles(parent);
            css.ResolveStyles(child);
            IDictionary<String, String> css2 = child.CSS;
            Assert.IsTrue(css2.ContainsKey("fontk"), "font not found");
            Assert.IsTrue(css2.ContainsKey("color"), "color not found");
            Assert.IsTrue(css2.ContainsKey("font-size"), "font-size not found");
            Assert.AreEqual("Arial", css2["fontk"]);
            Assert.AreEqual("blue", css2["color"]);
            Assert.AreEqual("large", css2["font-size"]);
        }

        private class CustomCssInheritanceRulesOne : ICssInheritanceRules {
            public bool InheritCssTag(String tag) {
                return !"child".Equals(tag);
            }

            public bool InheritCssSelector(Tag tag, String key) {
                return true;
            }
        }

        /**
	 * checks CSSInheritance between parent and child on tag level
	 */

        [Test]
        public void CssTagLevelInheritance() {
            css.SetCssInheritance(new CustomCssInheritanceRulesOne());
            css.ResolveStyles(parent);
            css.ResolveStyles(child);
            IDictionary<String, String> css2 = child.CSS;
            Assert.IsTrue(css2.ContainsKey("fontk"), "font not found");
            Assert.IsTrue(css2.ContainsKey("font-size"), "font-size not found");
            Assert.IsFalse(css2.ContainsKey("color"), "color found while not expected");
            Assert.AreEqual("Arial", css2["fontk"]);
            Assert.AreEqual("large", css2["font-size"]);
        }

        private class CustomCssInheritanceRulesTwo : ICssInheritanceRules {
            public bool InheritCssTag(String tag) {
                return true;
            }

            public bool InheritCssSelector(Tag tag, String key) {
                return "child".Equals(tag.Name) && "color".Equals(key);
            }
        }

        /**
	 * checks CSSInheritance between parent and child on css element level
	 */

        [Test]
        public void CssTagStyleLevelInheritance() {
            css.SetCssInheritance(new CustomCssInheritanceRulesTwo());
            css.ResolveStyles(parent);
            css.ResolveStyles(child);
            IDictionary<String, String> css2 = child.CSS;
            Assert.IsTrue(css2.ContainsKey("fontk"), "font not found");
            Assert.IsTrue(css2.ContainsKey("font-size"), "font-size not found");
            Assert.IsTrue(css2.ContainsKey("color"), "color not found");
            Assert.AreEqual("Arial", css2["fontk"]);
            Assert.AreEqual("large", css2["font-size"]);
            Assert.AreEqual("blue", css2["color"]);
        }
    }
}