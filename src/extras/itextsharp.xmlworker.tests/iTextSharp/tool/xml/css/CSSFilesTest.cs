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

    internal class CSSFilesTest {
        private CssFilesImpl files;
        private Tag t;
        public const String RESOURCES = @"..\..\resources\";

        [SetUp]
        virtual public void SetUp() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            files = new CssFilesImpl();
            StyleAttrCSSResolver resolver = new StyleAttrCSSResolver(files);
            string u = RESOURCES + "/css/style.css";
            resolver.AddCssFile(u.Replace("%20", " "), false); // fix url conversion of space (%20) for File
            Dictionary<String, String> attr = new Dictionary<String, String>();
            t = new Tag("body", attr);
        }

        [Test]
        virtual public void GetStyle() {
            IDictionary<String, String> css = files.GetCSS(t);
            Assert.IsTrue(css.ContainsKey("font-size"));
            Assert.IsTrue(css.ContainsKey("color"));
        }

        [Test]
        virtual public void Clear() {
            files.Clear();
            Assert.IsFalse(files.HasFiles(), "files detected");
        }

        [Test]
        virtual public void ClearWithPersistent() {
            CssFileImpl css = new CssFileImpl();
            css.IsPersistent(true);
            files.Add(css);
            files.Clear();
            Assert.IsTrue(files.HasFiles(), "no files detected");
        }
    }
}
