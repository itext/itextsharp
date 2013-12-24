using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css.apply;
using iTextSharp.tool.xml.html.pdfelement;
using iTextSharp.tool.xml.html.table;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.css.apply {
    internal class HtmlCellCssApplierTest {
        private List<Element> cells;
        private Tag tag;
        private NoNewLineParagraph basicPara;
        private Chunk basic;

        private TableRowElement row1;
        private HtmlCell cell;
        private HtmlCellCssApplier applier;
        private HtmlPipelineContext config;

        [SetUp]
        virtual public void SetUp() {
            cells = new List<Element>();
            tag = new Tag("td", new Dictionary<String, String>());
            basicPara = new NoNewLineParagraph();
            basic = new Chunk("content");

            cell = new HtmlCell();
            applier = new HtmlCellCssApplier();


            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            Tag parent = new Tag("tr");
            parent.Parent = new Tag("table");
            tag.Parent = parent;
            basicPara.Add(basic);
            cell.AddElement(basicPara);
            cells.Add(cell);
            config = new HtmlPipelineContext(null);
        }

        /*Disabled as long as the default borders are enabled*/

        virtual public void ResolveNoBorder() {
            applier.Apply(cell, tag, config, config);
            Assert.AreEqual(Rectangle.NO_BORDER, cell.Border);
        }

        [Test]
        virtual public void ResolveColspan() {
            Assert.AreEqual(1, cell.Colspan, 0);
            tag.Attributes["colspan"] = "2";
            applier.Apply(cell, tag, config, config);
            Assert.AreEqual(2, cell.Colspan);
        }

        [Test]
        virtual public void ResolveRowspan() {
            Assert.AreEqual(1, cell.Rowspan, 0);
            tag.Attributes["rowspan"] = "3";
            applier.Apply(cell, tag, config, config);
            Assert.AreEqual(3, cell.Rowspan);
        }

        [Test]
        virtual public void ResolveFixedWidth() {
            HtmlCell fixedWidthCell = new HtmlCell();
            tag.Attributes["width"] = "90pt";
            fixedWidthCell = applier.Apply(fixedWidthCell, tag, config, config);
            Assert.AreEqual(90, (fixedWidthCell).FixedWidth, 0);
        }

        [Test]
        virtual public void ResolveBorderWidth() {
            Assert.AreEqual(0.5, cell.BorderWidthTop, 0);
            tag.CSS["border-width-top"] = "5pt";
            tag.CSS["border-width-left"] = "6pt";
            tag.CSS["border-width-right"] = "7pt";
            tag.CSS["border-width-bottom"] = "8pt";
            applier.Apply(cell, tag, config, config);
            Assert.AreEqual(5, cell.CellValues.BorderWidthTop, 0);
            Assert.AreEqual(6, cell.CellValues.BorderWidthLeft, 0);
            Assert.AreEqual(7, cell.CellValues.BorderWidthRight, 0);
            Assert.AreEqual(8, cell.CellValues.BorderWidthBottom, 0);
        }

        [Test]
        virtual public void ResolveBorderColor() {
            Assert.AreEqual(null, cell.BorderColorTop);
            tag.CSS["border-color-top"] = "red";
            tag.CSS["border-color-left"] = "#0f0";
            tag.CSS["border-color-right"] = "#0000ff";
            tag.CSS["border-color-bottom"] = "rgb(000,111,222)";
            applier.Apply(cell, tag, config, config);
            Assert.AreEqual(BaseColor.RED, cell.CellValues.BorderColorTop);
            Assert.AreEqual(BaseColor.GREEN, cell.CellValues.BorderColorLeft);
            Assert.AreEqual(BaseColor.BLUE, cell.CellValues.BorderColorRight);
            Assert.AreEqual(new BaseColor(000, 111, 222), cell.CellValues.BorderColorBottom);
        }
    }
}
