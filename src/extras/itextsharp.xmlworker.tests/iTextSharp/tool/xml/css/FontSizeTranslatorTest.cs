using System.Globalization;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.css.apply;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.css {
    /**
 * @author Emiel Ackermann
 */

    internal class FontSizeTranslatorTest {
        private FontSizeTranslator fst = FontSizeTranslator.GetInstance();
        private Tag p;
        private Tag span;

        [SetUp]
        public void SetUp() {
            p = new Tag("p");
            span = new Tag("span");

            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            p.CSS[CSS.Property.FONT_FAMILY] = "Helvetica";
            p.CSS[CSS.Property.FONT_SIZE] = "12pt";
            p.AddChild(span);
            span.Parent = p;
        }

        [Test]
        public void ResolveXXAndXSmall() {
            p.CSS[CSS.Property.FONT_SIZE] = CSS.Value.XX_SMALL;
            p.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(p).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c1 = new ChunkCssApplier().Apply(new Chunk("Text before span "), p);
            Assert.AreEqual(6.75f, c1.Font.Size, 0);

            span.CSS[CSS.Property.FONT_SIZE] = CSS.Value.X_SMALL;
            span.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(span).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c2 = new ChunkCssApplier().Apply(new Chunk("text in span "), span);
            Assert.AreEqual(7.5f, c2.Font.Size, 0);
        }

        [Test]
        public void ResolveSmallAndMedium() {
            p.CSS[CSS.Property.FONT_SIZE] = CSS.Value.SMALL;
            p.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(p).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c1 = new ChunkCssApplier().Apply(new Chunk("Text before span "), p);
            Assert.AreEqual(9.75f, c1.Font.Size, 0);

            span.CSS[CSS.Property.FONT_SIZE] = CSS.Value.MEDIUM;
            span.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(span).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c2 = new ChunkCssApplier().Apply(new Chunk("text in span "), span);
            Assert.AreEqual(12f, c2.Font.Size, 0);
        }

        [Test]
        public void ResolveLargeXLargeXXLarge() {
            p.CSS[CSS.Property.FONT_SIZE] = CSS.Value.LARGE;
            p.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(p).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c1 = new ChunkCssApplier().Apply(new Chunk("Text before span "), p);
            Assert.AreEqual(13.5f, c1.Font.Size, 0);

            span.CSS[CSS.Property.FONT_SIZE] = CSS.Value.X_LARGE;
            span.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(span).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c2 = new ChunkCssApplier().Apply(new Chunk("text in span "), span);
            Assert.AreEqual(18f, c2.Font.Size, 0);

            p.CSS[CSS.Property.FONT_SIZE] = CSS.Value.XX_LARGE;
            p.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(p) + "pt";
            Chunk c3 = new ChunkCssApplier().Apply(new Chunk("Text after span."), p);
            Assert.AreEqual(24f, c3.Font.Size, 0);
        }

        [Test]
        public void ResolveDefaultToSmaller() {
            p.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(p).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c1 = new ChunkCssApplier().Apply(new Chunk("Text before span "), p);
            Assert.AreEqual(12, c1.Font.Size, 0);

            span.CSS[CSS.Property.FONT_SIZE] = CSS.Value.SMALLER;
            span.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(span).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c2 = new ChunkCssApplier().Apply(new Chunk("text in span "), span);
            Assert.AreEqual(9.75f, c2.Font.Size, 0);
        }

        [Test]
        public void ResolveDefaultToLarger() {
            p.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(p).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c1 = new ChunkCssApplier().Apply(new Chunk("Text before span "), p);
            Assert.AreEqual(12, c1.Font.Size, 0);

            span.CSS[CSS.Property.FONT_SIZE] = CSS.Value.LARGER;
            span.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(span).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c2 = new ChunkCssApplier().Apply(new Chunk("text in span "), span);
            Assert.AreEqual(13.5f, c2.Font.Size, 0);
        }

        [Test]
        public void Resolve15ToSmaller() {
            p.CSS[CSS.Property.FONT_SIZE] = "15pt";
            p.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(p).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c1 = new ChunkCssApplier().Apply(new Chunk("Text before span "), p);
            Assert.AreEqual(15, c1.Font.Size, 0);

            span.CSS[CSS.Property.FONT_SIZE] = CSS.Value.SMALLER;
            span.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(span).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c2 = new ChunkCssApplier().Apply(new Chunk("text in span "), span);
            Assert.AreEqual(15*0.85f, c2.Font.Size, 1e-6);
        }

        [Test]
        public void Resolve15ToLarger() {
            p.CSS[CSS.Property.FONT_SIZE] = "15pt";
            p.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(p).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c1 = new ChunkCssApplier().Apply(new Chunk("Text before span "), p);
            Assert.AreEqual(15, c1.Font.Size, 0);

            span.CSS[CSS.Property.FONT_SIZE] = CSS.Value.LARGER;
            span.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(span).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c2 = new ChunkCssApplier().Apply(new Chunk("text in span "), span);
            Assert.AreEqual(15*1.5f, c2.Font.Size, 0);
        }

        [Test]
        public void Resolve34ToSmaller() {
            p.CSS[CSS.Property.FONT_SIZE] = "34pt";
            p.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(p).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c1 = new ChunkCssApplier().Apply(new Chunk("Text before span "), p);
            Assert.AreEqual(34, c1.Font.Size, 0);

            span.CSS[CSS.Property.FONT_SIZE] = CSS.Value.SMALLER;
            span.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(span).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c2 = new ChunkCssApplier().Apply(new Chunk("text in span "), span);
            Assert.AreEqual(34*2/3f, c2.Font.Size, 1e-5);
        }

        [Test]
        public void Resolve34ToLarger() {
            p.CSS[CSS.Property.FONT_SIZE] = "34pt";
            p.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(p) + "pt";
            Chunk c1 = new ChunkCssApplier().Apply(new Chunk("Text before span "), p);
            Assert.AreEqual(34, c1.Font.Size, 0);

            span.CSS[CSS.Property.FONT_SIZE] = CSS.Value.LARGER;
            span.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(span) + "pt";
            Chunk c2 = new ChunkCssApplier().Apply(new Chunk("text in span "), span);
            Assert.AreEqual(34*1.5f, c2.Font.Size, 0);
        }
    }
}