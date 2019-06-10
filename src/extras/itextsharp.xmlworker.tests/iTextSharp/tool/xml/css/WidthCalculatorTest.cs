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
