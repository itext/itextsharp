using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css.apply;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.css.apply {
    internal class FontFamilyTest {
        static FontFamilyTest() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
        }

        private ChunkCssApplier applier = new ChunkCssApplier();

        [Test]
        public void ResolveSingleQuotedFontFamily() {
            Tag t = new Tag(null);
            t.CSS["color"] = "#000000";
            t.CSS["font-family"] = "'Helvetica'";
            t.CSS["font-size"] = "12pt";
            Chunk c = new Chunk("default text for chunk creation");
            applier.Apply(c, t);
            Assert.AreEqual("Helvetica", c.Font.Familyname);
        }

        [Test]
        public void ResolveDoubleQuotedFontFamily() {
            Tag t = new Tag(null);
            t.CSS["color"] = "#000000";
            t.CSS["font-family"] = "\"Helvetica\"";
            t.CSS["font-size"] = "12pt";
            Chunk c = new Chunk("default text for chunk creation");
            applier.Apply(c, t);
            Assert.AreEqual("Helvetica", c.Font.Familyname);
        }
    }
}