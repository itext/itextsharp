using System;
using System.Collections.Generic;
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.css {
    /**
 * @author Emiel Ackermann
 *
 */

    internal class WidthCalculatorTest {
        private Tag body;
        private Tag table;
        private Tag row;
        private Tag cell;
        private HtmlPipelineContext config;
        private WidthCalculator calc;

        [SetUp]
        virtual public void SetUp() {
            body = new Tag("body", new Dictionary<String, String>());
            table = new Tag("table", new Dictionary<String, String>());
            row = new Tag("tr", new Dictionary<String, String>());
            cell = new Tag("td", new Dictionary<String, String>());
            config = new HtmlPipelineContext(null);
            calc = new WidthCalculator();

            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            body.AddChild(table);
            table.Parent = body;
            table.AddChild(row);
            row.Parent = table;
            row.AddChild(cell);
            cell.Parent = row;
        }

        [Test]
        virtual public void ResolveBodyWidth80() {
            body.Attributes[HTML.Attribute.WIDTH] = "80%";
            Assert.AreEqual(0.8*config.PageSize.Width, calc.GetWidth(body, config.GetRootTags(), config.PageSize.Width),
                0);
        }

        [Test]
        virtual public void ResolveTableWidth80() {
            table.Attributes[HTML.Attribute.WIDTH] = "80%";
            Assert.AreEqual(0.8*config.PageSize.Width, calc.GetWidth(table, config.GetRootTags(), config.PageSize.Width),
                0);
        }

        [Test]
        virtual public void ResolveCellWidth20of100() {
            cell.Attributes[HTML.Attribute.WIDTH] = "20%";
            Assert.AreEqual(config.PageSize.Width*0.2f, calc.GetWidth(cell, config.GetRootTags(), config.PageSize.Width),
                0.01f);
        }

        [Test]
        virtual public void ResolveCellWidth20of80() {
            table.Attributes[HTML.Attribute.WIDTH] = "80%";
            cell.Attributes[HTML.Attribute.WIDTH] = "20%";
            Assert.AreEqual(0.8f*config.PageSize.Width*0.2f,
                calc.GetWidth(cell, config.GetRootTags(), config.PageSize.Width), 0.01f);
        }
    }
}
