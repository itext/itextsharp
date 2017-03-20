/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
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
        virtual public void SetUp() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            t = new Tag(null);
            t.CSS["color"] = "#000000";
            t.CSS["font-family"] = "Helvetica";
            t.CSS["font-size"] = "12pt";
            c = new Chunk("default text for chunk creation");
            applier.Apply(c, t);
        }

        [Test]
        virtual public void ResolveFontFamily() {
            Assert.AreEqual("Helvetica", c.Font.Familyname);
//		t.CSS.put("font-family", "Verdana");
//		c = new ChunkCssApplier().Apply(c, t);
//		Assert.AreEqual("Verdana", c.Font.getFamilyname()); Where can i retrieve the familyname of the font?
        }

        [Test]
        virtual public void ResolveFontSize() {
            Assert.AreEqual(12, c.Font.Size, 0);
            t.CSS["font-size"] = "18pt";
            c = applier.Apply(c, t);
            Assert.AreEqual(18, c.Font.Size, 0);
        }

        [Test]
        virtual public void ResolveCharacterSpacing() {
            Assert.AreEqual(0, c.GetCharacterSpacing(), 0);
            t.CSS["letter-spacing"] = "15pt";
            c = applier.Apply(c, t);
            Assert.AreEqual(15, c.GetCharacterSpacing(), 0);
        }

        [Test]
        virtual public void ResolveHorizontalAndVerticalScaling() {
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
        virtual public void ResolveColor() {
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
        virtual public void ResolveVerticalAlign() {
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
