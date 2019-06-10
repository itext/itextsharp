/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
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
        virtual public void SetUp() {
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
        virtual public void ResolveTagCss() {
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
        virtual public void ResolveChildTagCss() {
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
            virtual public bool InheritCssTag(String tag) {
                return !"child".Equals(tag);
            }

            virtual public bool InheritCssSelector(Tag tag, String key) {
                return true;
            }
        }

        /**
	 * checks CSSInheritance between parent and child on tag level
	 */

        [Test]
        virtual public void CssTagLevelInheritance() {
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
            virtual public bool InheritCssTag(String tag) {
                return true;
            }

            virtual public bool InheritCssSelector(Tag tag, String key) {
                return "child".Equals(tag.Name) && "color".Equals(key);
            }
        }

        /**
	 * checks CSSInheritance between parent and child on css element level
	 */

        [Test]
        virtual public void CssTagStyleLevelInheritance() {
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
