using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css.apply;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.css.apply {
    internal class ChunkCssApplierTest {
        private Tag t;
        private Chunk c;
        private ChunkCssApplier applier = new ChunkCssApplier();

        [SetUp]
        public void SetUp() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            t = new Tag(null);
            t.CSS["color"] = "#000000";
            t.CSS["font-family"] = "Helvetica";
            t.CSS["font-size"] = "12pt";
            c = new Chunk("default text for chunk creation");
            applier.Apply(c, t);
        }

        [Test]
        public void ResolveFontFamily() {
            Assert.AreEqual("Helvetica", c.Font.Familyname);
//		t.CSS.put("font-family", "Verdana");
//		c = new ChunkCssApplier().Apply(c, t);
//		Assert.AreEqual("Verdana", c.Font.getFamilyname()); Where can i retrieve the familyname of the font?
        }

        [Test]
        public void ResolveFontSize() {
            Assert.AreEqual(12, c.Font.Size, 0);
            t.CSS["font-size"] = "18pt";
            c = applier.Apply(c, t);
            Assert.AreEqual(18, c.Font.Size, 0);
        }

        [Test]
        public void ResolveCharacterSpacing() {
            Assert.AreEqual(0, c.GetCharacterSpacing(), 0);
            t.CSS["letter-spacing"] = "15pt";
            c = applier.Apply(c, t);
            Assert.AreEqual(15, c.GetCharacterSpacing(), 0);
        }

        [Test]
        public void ResolveHorizontalAndVerticalScaling() {
            t.CSS["xfa-font-vertical-scale"] = "75pt";
            Assert.AreEqual(1, c.HorizontalScaling, 0);
            t.CSS["xfa-font-horizontal-scale"] = "75%";
            c = applier.Apply(c, t);
            Assert.AreEqual(0.75f, c.HorizontalScaling, 0);
            t.CSS["xfa-font-vertical-scale"] = "75%";
            c = applier.Apply(c, t);
            Assert.AreEqual(9, c.Font.Size, 0);
            Assert.AreEqual(100f / 75f, c.HorizontalScaling, 1e-7);
        }

        [Test]
        public void ResolveColor() {
            Assert.AreEqual(BaseColor.BLACK, c.Font.Color);
            t.CSS["color"] = "#00f";
            c = applier.Apply(c, t);
            Assert.AreEqual(255, c.Font.Color.B, 0);
            t.CSS["color"] = "#00ff00";
            c = applier.Apply(c, t);
            Assert.AreEqual(255, c.Font.Color.G, 0);
            t.CSS["color"] = "rgb(255,0,0)";
            c = applier.Apply(c, t);
            Assert.AreEqual(255, c.Font.Color.R, 0);
        }

        [Test]
        public void ResolveVerticalAlign() {
            Assert.AreEqual(0, c.GetTextRise(), 0);
            t.CSS["vertical-align"] = "5pt";
            c = applier.Apply(c, t);
            Assert.AreEqual(5, c.GetTextRise(), 0);

            t.CSS["vertical-align"] = "sub";
            c = applier.Apply(c, t);
            Assert.AreEqual(12, c.Font.Size, 0);
            Assert.AreEqual(-c.Font.Size/2, c.GetTextRise(), 0);

            t.CSS["vertical-align"] = "super";
            c = applier.Apply(c, t);
            Assert.AreEqual(12, c.Font.Size, 0);
            Assert.AreEqual(c.Font.Size/2 + 0.5, c.GetTextRise(), 0);
        }
    }
}