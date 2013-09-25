using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html.tps {
    /**
 * @author itextpdf.com
 *
 */

    internal class AbstractTagprocessorTest {
        private class CustomTagProcessor : AbstractTagProcessor {
        }

        [Test]
        public void VerifyEnd() {
            AbstractTagProcessor a = new CustomTagProcessor();
            Tag tag = new Tag("dummy");
            tag.CSS["page-break-after"] = "always";
            IList<IElement> end = a.EndElement(null, tag, new List<IElement>());
            Assert.AreEqual(Chunk.NEXTPAGE, end[0]);
        }

        [Test]
        public void VerifyStart() {
            AbstractTagProcessor a = new CustomTagProcessor();
            Tag tag = new Tag("dummy");
            tag.CSS["page-break-before"] = "always";
            IList<IElement> end = a.StartElement(null, tag);
            Assert.AreEqual(Chunk.NEXTPAGE, end[0]);
        }

        [Test]
        public void VerifyFontsizeTranslation() {
            AbstractTagProcessor a = new CustomTagProcessor();
            Tag tag = new Tag("dummy");
            tag.CSS["font-size"] = "16px";
            a.StartElement(null, tag);
            Assert.AreEqual("12pt", tag.CSS["font-size"]);
        }

        [Test]
        public void VerifyIfStackowner() {
            Assert.IsFalse((new CustomTagProcessor()).IsStackOwner());
        }
    }
}