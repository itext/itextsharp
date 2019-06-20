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
        virtual public void SetUp() {
            p = new Tag("p");
            span = new Tag("span");

            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            p.CSS[CSS.Property.FONT_FAMILY] = "Helvetica";
            p.CSS[CSS.Property.FONT_SIZE] = "12pt";
            p.AddChild(span);
            span.Parent = p;
        }

        [Test]
        virtual public void ResolveXXAndXSmall() {
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
        virtual public void ResolveSmallAndMedium() {
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
        virtual public void ResolveLargeXLargeXXLarge() {
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
        virtual public void ResolveDefaultToSmaller() {
            p.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(p).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c1 = new ChunkCssApplier().Apply(new Chunk("Text before span "), p);
            Assert.AreEqual(12, c1.Font.Size, 0);

            span.CSS[CSS.Property.FONT_SIZE] = CSS.Value.SMALLER;
            span.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(span).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c2 = new ChunkCssApplier().Apply(new Chunk("text in span "), span);
            Assert.AreEqual(9.75f, c2.Font.Size, 0);
        }

        [Test]
        virtual public void ResolveDefaultToLarger() {
            p.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(p).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c1 = new ChunkCssApplier().Apply(new Chunk("Text before span "), p);
            Assert.AreEqual(12, c1.Font.Size, 0);

            span.CSS[CSS.Property.FONT_SIZE] = CSS.Value.LARGER;
            span.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(span).ToString(CultureInfo.InvariantCulture) + "pt";
            Chunk c2 = new ChunkCssApplier().Apply(new Chunk("text in span "), span);
            Assert.AreEqual(13.5f, c2.Font.Size, 0);
        }

        [Test]
        virtual public void Resolve15ToSmaller() {
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
        virtual public void Resolve15ToLarger() {
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
        virtual public void Resolve34ToSmaller() {
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
        virtual public void Resolve34ToLarger() {
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
