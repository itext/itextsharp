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
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.html.pdfelement;
using iTextSharp.tool.xml.html.table;
using iTextSharp.tool.xml.pipeline.ctx;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html.table {
    internal class TableTest {
        private List<IElement> cells1 = new List<IElement>();
        private List<IElement> cells2 = new List<IElement>();
        private List<IElement> rows = new List<IElement>();
        private Tag tag = new Tag(null, new Dictionary<String, String>());
        private NoNewLineParagraph basicPara = new NoNewLineParagraph();
        private NoNewLineParagraph extraPara = new NoNewLineParagraph();
        private Chunk basic = new Chunk("content");
        private Chunk extra = new Chunk("extra content");

        private TableRowElement row1;
        private HtmlCell cell1Row1 = new HtmlCell();
        private HtmlCell cell2Row1 = new HtmlCell();
        private HtmlCell cell3Row1 = new HtmlCell();
        private HtmlCell cell4Row1 = new HtmlCell();
        private TableRowElement row2;
        private HtmlCell cell1Row2 = new HtmlCell();
        private HtmlCell cell2Row2 = new HtmlCell();
        private HtmlCell cell3Row2 = new HtmlCell();

        [SetUp]
        virtual public void SetUp() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            tag.Parent = new Tag("defaultRoot");
            basicPara.Add(basic);
            extraPara.Add(extra);
            cell1Row1.AddElement(basicPara);
            cell2Row1.AddElement(extraPara);
            cell3Row1.AddElement(basicPara);
            cell4Row1.AddElement(extraPara);
            cell4Row1.Rowspan = 2;
            cells1.Add(cell1Row1);
            cells1.Add(cell2Row1);
            cells1.Add(cell3Row1);
            cells1.Add(cell4Row1);
            row1 = new TableRowElement(cells1, TableRowElement.Place.BODY);

            cell1Row2.AddElement(extraPara);
            cell2Row2.AddElement(basicPara);
//		Tag t = new Tag(null, new HashMap<String, String>());
//		t.getAttributes().put("col-span", "2");
            cell2Row2.Colspan = 2;
            cell3Row2.AddElement(extraPara);
            cells2.Add(cell1Row2);
            cells2.Add(cell2Row2);
            //cells2.Add(cell3Row2);
            row2 = new TableRowElement(cells2, TableRowElement.Place.BODY);

            rows.Add(row1);
            rows.Add(row2);
        }

        [Test]
        virtual public void ResolveBuild() {
            AbstractTagProcessor table2 = new Table();
            table2.SetCssAppliers(new CssAppliersImpl());
            WorkerContextImpl context = new WorkerContextImpl();
            context.Put(typeof (HtmlPipeline).FullName, new HtmlPipelineContext(null));
            PdfPTable table = (PdfPTable) (table2.End(context, tag, rows)[0]);
            Assert.AreEqual(4, table.Rows[0].GetCells().Length);
            Assert.AreEqual(4, table.NumberOfColumns);
        }

        [Test]
        virtual public void ResolveNumberOfCells() {
            Assert.AreEqual(4, ((TableRowElement) rows[0]).Content.Count);
            Assert.AreEqual(2, ((TableRowElement) rows[1]).Content.Count);
        }

        [Test]
        virtual public void ResolveColspan() {
            Assert.AreEqual(2, (((TableRowElement) rows[1]).Content[1]).Colspan);
        }

        [Test]
        virtual public void ResolveRowspan() {
            Assert.AreEqual(2, (((TableRowElement) rows[0]).Content[3]).Rowspan);
        }
    }
}
