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
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.css.apply {
    internal class ParagraphCssApplierTest {
        static ParagraphCssApplierTest() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
        }

        private static FontSizeTranslator fst = FontSizeTranslator.GetInstance();
        private Tag parent;
        private Tag first;
        private Tag second;
        private Tag child;
        private Paragraph firstPara;
        private Paragraph secondPara;
        private ParagraphCssApplier applier = new ParagraphCssApplier(new CssAppliersImpl());
        private HtmlPipelineContext configuration;

        [SetUp]
        virtual public void SetUp() {
            parent = new Tag("body");
            parent.CSS[CSS.Property.FONT_FAMILY] = "Helvetica";
            parent.CSS[CSS.Property.FONT_SIZE] = "12pt";
            first = new Tag(null);
            first.CSS[CSS.Property.FONT_FAMILY] = "Helvetica";
            first.CSS[CSS.Property.FONT_SIZE] = "12pt";
            second = new Tag(null);
            second.CSS[CSS.Property.FONT_FAMILY] = "Helvetica";
            second.CSS[CSS.Property.FONT_SIZE] = "12pt";
            child = new Tag(null);
            child.CSS[CSS.Property.FONT_FAMILY] = "Helvetica";
            child.CSS[CSS.Property.FONT_SIZE] = "12pt";

            parent.AddChild(first);
            first.Parent = parent;
            second.Parent = parent;
            first.AddChild(child);
            second.AddChild(child);
            parent.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(parent) + "pt";
            first.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(first) + "pt";
            first.CSS[CSS.Property.TEXT_ALIGN] = CSS.Value.LEFT;
            second.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(second) + "pt";
            child.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(child) + "pt";
            firstPara = new Paragraph(new Chunk("default text for chunk creation"));
            secondPara = new Paragraph(new Chunk("default text for chunk creation"));
            configuration = new HtmlPipelineContext(null);
            applier.Apply(firstPara, first, configuration);
        }

        [Test]
        virtual public void ResolveAlignment() {
            Assert.AreEqual(Element.ALIGN_LEFT, firstPara.Alignment, 0);

            first.CSS["text-align"] = "right";
            applier.Apply(firstPara, first, configuration);
            Assert.AreEqual(Element.ALIGN_RIGHT, firstPara.Alignment, 0);

            first.CSS["text-align"] = "left";
            applier.Apply(firstPara, first, configuration);
            Assert.AreEqual(Element.ALIGN_LEFT, firstPara.Alignment, 0);

            first.CSS["text-align"] = "center";
            applier.Apply(firstPara, first, configuration);
            Assert.AreEqual(Element.ALIGN_CENTER, firstPara.Alignment, 0);
        }

        [Test]
        virtual public void ResolveFirstLineIndent() {
            Assert.AreEqual(0f, firstPara.FirstLineIndent, 0);

            first.CSS["text-indent"] = "16pt";
            applier.Apply(firstPara, first, configuration);
            Assert.AreEqual(16, firstPara.FirstLineIndent, 0);
        }

        [Test]
        virtual public void ResolveIndentationLeft() {
            Assert.AreEqual(0f, firstPara.IndentationLeft, 0);

            first.CSS["margin-left"] = "10pt";
            applier.Apply(firstPara, first, configuration);
            Assert.AreEqual(10, firstPara.IndentationLeft, 0);
        }

        [Test]
        virtual public void ResolveIndentationRight() {
            Assert.AreEqual(0f, firstPara.IndentationRight, 0);

            first.CSS["margin-right"] = "10pt";
            applier.Apply(firstPara, first, configuration);
            Assert.AreEqual(10, firstPara.IndentationRight, 0);
        }

        [Test]
        [Ignore("We need possibility to detect that line-height undefined;")]
        virtual public void ResolveLeading() {
            Assert.AreEqual(18f, firstPara.Leading, 0);

            first.CSS["line-height"] = "25pt";
            applier.Apply(firstPara, first, configuration);
            Assert.AreEqual(25, firstPara.Leading, 0);

            child.CSS["line-height"] = "19pt";
            applier.Apply(firstPara, first, configuration);
            Assert.AreEqual(25, firstPara.Leading, 0);

            child.CSS["line-height"] = "30pt";
            applier.Apply(firstPara, first, configuration);
            Assert.AreEqual(30, firstPara.Leading, 0);
        }

        [Test]
        virtual public void ResolveSpacingAfter() {
            Assert.AreEqual(0, firstPara.SpacingBefore, 0);
            second.CSS["margin-bottom"] = "25pt";

            applier.Apply(secondPara, second, configuration);
            Assert.AreEqual(25, secondPara.SpacingAfter, 0);
        }

        [Test]
        virtual public void ResolveSpacingBeforeIs10() {
            parent.AddChild(second);
            first.CSS["margin-bottom"] = "12pt";
            second.CSS["margin-top"] = "22pt";

            applier.Apply(firstPara, first, configuration);
            applier.Apply(secondPara, second, configuration);
            Assert.AreEqual(22 - 12, secondPara.SpacingBefore, 0);
        }

        [Test]
        virtual public void ResolveSpacingBeforeIs5() {
            parent.AddChild(second);
            first.CSS["margin-bottom"] = "25pt";
            second.CSS["margin-top"] = "30pt";

            applier.Apply(firstPara, first, configuration);
            applier.Apply(secondPara, second, configuration);
            Assert.AreEqual(30 - 25, secondPara.SpacingBefore, 0);
        }

        [Test]
        virtual public void ResolveSpacingBeforeIs0() {
            parent.AddChild(second);
            first.CSS["margin-bottom"] = "35pt";
            second.CSS["margin-top"] = "30pt";

            applier.Apply(firstPara, first, configuration);
            applier.Apply(secondPara, second, configuration);
            //30-35 is reverted to 0.
            Assert.AreEqual(0, secondPara.SpacingBefore, 0);
        }

        [Test]
        virtual public void ResolveSpacingBeforeIs6() {
            parent.AddChild(second);
            first.CSS["margin-bottom"] = "2em";
            second.CSS["margin-top"] = "30pt";

            applier.Apply(firstPara, first, configuration);
            applier.Apply(secondPara, second, configuration);
            Assert.AreEqual(30 - (2*12), secondPara.SpacingBefore, 0);
        }

        [Test]
        virtual public void ResolveSpacingBeforeIs24() {
            parent.AddChild(second);
            first.CSS["margin-bottom"] = "2em";
            first.CSS[CSS.Property.FONT_SIZE] = "18";
            first.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(first).ToString(CultureInfo.InvariantCulture) + "pt";
            second.CSS["margin-top"] = "60pt";

            applier.Apply(firstPara, first, configuration);
            applier.Apply(secondPara, second, configuration);
            // 60 - 2 * (18px = 13.5pt)
            Assert.AreEqual(60 - (2*13.5f), secondPara.SpacingBefore, 0);
        }

        [Test]
        virtual public void ResolveSpacingBeforeIs12() {
            parent.AddChild(second);
            first.CSS["margin-bottom"] = "2em";
            first.CSS[CSS.Property.FONT_SIZE] = "2em";
            first.CSS[CSS.Property.FONT_SIZE] = fst.TranslateFontSize(first) + "pt";
            second.CSS["margin-top"] = "60pt";

            applier.Apply(firstPara, first, configuration);
            applier.Apply(secondPara, second, configuration);
            Assert.AreEqual(60 - (2*12*2), secondPara.SpacingBefore, 0);
        }
    }
}
