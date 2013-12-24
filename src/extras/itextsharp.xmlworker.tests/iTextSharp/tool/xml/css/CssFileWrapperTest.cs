using System;
using iTextSharp.tool.xml.css;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.css {
    /**
 * @author Balder Van Camp
 *
 */

    internal class CssFileWrapperTest {
        private CSSFileWrapper w;

        [SetUp]
        virtual public void SetUp() {
            ICssFile css = new CssFileImpl();
            w = new CSSFileWrapper(css, true);
        }


        [Test]
        [ExpectedException(typeof (NotSupportedException))]
        virtual public void Testadd() {
            w.Add("", null);
        }

        [Test]
        [ExpectedException(typeof (NotSupportedException))]
        virtual public void TestisPersistent() {
            w.IsPersistent(false);
        }
    }
}
