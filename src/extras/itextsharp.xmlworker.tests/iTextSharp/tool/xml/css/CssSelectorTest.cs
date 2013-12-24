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

    internal class CssSelectorTest {
        private CssSelector css;
        private Tag root;
        private Tag rChild;
        private Tag idroot;

        [SetUp]
        virtual public void SetUp() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            css = new CssSelector();
            root = new Tag("root");
            rChild = new Tag("rChild");
            rChild.Parent = root;
            root.Children.Add(rChild);

            Dictionary<String, String> rootAttr = new Dictionary<String, String>();
            rootAttr["class"] = "rootClass";
            rootAttr["id"] = "rootId";
            idroot = new Tag("root", rootAttr);
            Dictionary<String, String> classAttr = new Dictionary<String, String>();
            classAttr["class"] = "childClass";
            classAttr["id"] = "childId";
        }

        [Test]
        virtual public void ValidateRootSelector() {
            IDictionary<String, object> rootSelectors = css.CreateTagSelectors(root);
            Assert.IsTrue(rootSelectors.ContainsKey("root"), "Not found root");
            Assert.AreEqual(1, rootSelectors.Count, "Too many entries");
        }

        [Test]
        virtual public void ValidateChildSelectors() {
            IDictionary<String, object> rootSelectors = css.CreateTagSelectors(root);
            IDictionary<String, object> childSelectors = css.CreateTagSelectors(rChild);
            Assert.IsTrue(rootSelectors.ContainsKey("root"), "Not found root");
            Assert.AreEqual(1, rootSelectors.Count, "Too many entries for root");
            Assert.AreEqual(6, childSelectors.Count, "Too many entries for child");
            Assert.IsTrue(childSelectors.ContainsKey("root>rChild"), "Not found root>rChild");
            Assert.IsTrue(childSelectors.ContainsKey("root+rChild"), "Not found root+rChild");
            Assert.IsTrue(childSelectors.ContainsKey("rChild"), "Not found rChild");
            Assert.IsTrue(childSelectors.ContainsKey("root + rChild"), "Not found root + rChild");
            Assert.IsTrue(childSelectors.ContainsKey("root > rChild"), "Not found root > rChild");
            Assert.IsTrue(childSelectors.ContainsKey("root rChild"), "Not found root rChild");
        }

        [Test]
        virtual public void ValidateIdRootSelector() {
            IDictionary<String, object> rootSelectors = css.CreateAllSelectors(idroot);
            Assert.IsTrue(rootSelectors.ContainsKey("root"), "Not found root");
            Assert.IsTrue(rootSelectors.ContainsKey("#rootId"), "Not found rootId");
            Assert.IsTrue(rootSelectors.ContainsKey(".rootClass"), "Not found rootClass");
            Assert.AreEqual(5, rootSelectors.Count, "Too many entries");
        }

        [Test]
        virtual public void CreateClassSelectorsMultipleCSSClasses() {
            Tag t = new Tag("dummy");
            t.Attributes["class"] = "klass1 klass2 klass3";
            IDictionary<String, object> set = css.CreateClassSelectors(t);
            Assert.AreEqual(6, set.Count, "should have found 6 selectors");
        }
    }
}
