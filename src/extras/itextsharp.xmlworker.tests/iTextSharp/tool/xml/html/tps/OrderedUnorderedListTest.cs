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
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css.apply;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.ctx;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;
using ListItextSharp = iTextSharp.text.List;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html.tps {
    /**
 * @author Emiel Ackermann
 *
 */

    internal class OrderedUnorderedListTest {
        private Tag root;
        private Tag p;
        private Tag ul;
        private Tag first;
        private Tag last;
        private List<IElement> listWithOne;
        private List<IElement> listWithTwo;
        private ListItem single;
        private ListItem start;
        private ListItem end;
        private OrderedUnorderedList orderedUnorderedList;
        private WorkerContextImpl workerContextImpl;

        [SetUp]
        virtual public void SetUp() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            root = new Tag("body");
            p = new Tag("p");
            ul = new Tag("ul");
            first = new Tag("li");
            last = new Tag("li");

            single = new ListItem("Single");
            start = new ListItem("Start");
            end = new ListItem("End");

            listWithOne = new List<IElement>();
            listWithTwo = new List<IElement>();
            orderedUnorderedList = new OrderedUnorderedList();
            CssAppliersImpl cssAppliers = new CssAppliersImpl();
            orderedUnorderedList.SetCssAppliers(cssAppliers);
            workerContextImpl = new WorkerContextImpl();
            HtmlPipelineContext context2 = new HtmlPipelineContext(cssAppliers);
            workerContextImpl.Put(typeof (HtmlPipeline).FullName, context2);
            root.AddChild(p);
            root.AddChild(ul);
            ul.AddChild(first);
            ul.AddChild(last);
            p.CSS["font-size"] = "12pt";
            p.CSS["margin-top"] = "12pt";
            p.CSS["margin-bottom"] = "12pt";
            new ParagraphCssApplier(cssAppliers).Apply(new Paragraph("paragraph"), p, context2);
            first.CSS["margin-top"] = "50pt";
            first.CSS["padding-top"] = "25pt";
            first.CSS["margin-bottom"] = "50pt";
            first.CSS["padding-bottom"] = "25pt";
            last.CSS["margin-bottom"] = "50pt";
            last.CSS["padding-bottom"] = "25pt";
            listWithOne.Add(single);
            listWithTwo.Add(start);
            listWithTwo.Add(end);
        }

        [Test]
        virtual public void ListWithOneNoListMarginAndPadding() {
            ListItextSharp endList = (ListItextSharp)
                orderedUnorderedList.End(workerContextImpl, ul, listWithOne)[0];
            Assert.AreEqual(50f + 25f - 12f, ((ListItem) endList.Items[0]).SpacingBefore, 0f);
            Assert.AreEqual(50f + 25f, ((ListItem) endList.Items[0]).SpacingAfter, 0f);
        }

        [Test]
        virtual public void ListWithOneNoListPaddingTop() {
            ul.CSS["margin-top"] = "100pt";
            ListItextSharp endList = (ListItextSharp) orderedUnorderedList.End(workerContextImpl, ul, listWithOne)[0];
            Assert.AreEqual(100f + 25f - 12f, ((ListItem) endList.Items[0]).SpacingBefore, 0f);
        }

        [Test]
        virtual public void ListWithOneNoListPaddingTop2() {
            ul.CSS["margin-top"] = "100pt";
            ul.CSS["padding-top"] = "0pt";
            ListItextSharp endList = (ListItextSharp) orderedUnorderedList.End(workerContextImpl, ul, listWithOne)[0];
            Assert.AreEqual(100f + 25f - 12f, ((ListItem) endList.Items[0]).SpacingBefore, 0f);
        }

        [Test]
        virtual public void ListWithOneWithListPaddingTop() {
            ul.CSS["margin-top"] = "100pt";
            ul.CSS["padding-top"] = "25pt";
            ListItextSharp endList = (ListItextSharp) orderedUnorderedList.End(workerContextImpl, ul, listWithOne)[0];
            Assert.AreEqual(100f + 25f + 50f + 25f - 12f, ((ListItem) endList.Items[0]).SpacingBefore, 0f);
        }

        /**
	 * Verifies if the largest of the margin-bottom's of the ul and it's only li is selected and added to the padding-bottom of li, because padding-bottom of ul == null.
	 */

        [Test]
        virtual public void ListWithOneNoListPaddingBottom() {
            ul.CSS["margin-bottom"] = "25pt";
            ListItextSharp endList = (ListItextSharp) orderedUnorderedList.End(workerContextImpl, ul, listWithOne)[0];
            Assert.AreEqual(50f + 25f, ((ListItem) endList.Items[0]).SpacingAfter, 0f);
        }

        /**
	 * Verifies if the largest of the margin-bottom's of the ul and it's only li is selected and added to the padding-bottom of li, because padding-bottom of ul == 0.
	 */

        [Test]
        virtual public void ListWithOneNoListPaddingBottom2() {
            ul.CSS["margin-bottom"] = "100pt";
            ul.CSS["padding-bottom"] = "0pt";
            ListItextSharp endList = (ListItextSharp) orderedUnorderedList.End(workerContextImpl, ul, listWithOne)[0];
            Assert.AreEqual(100f + 25f, ((ListItem) endList.Items[0]).SpacingAfter, 0f);
        }

        /**
	 * Verifies if margin-bottom and padding-bottom of both the ul and it's only li are added up, because padding-bottom of ul != 0.
	 */

        [Test]
        virtual public void ListWithOneWithListPaddingBottom() {
            ul.CSS["margin-bottom"] = "100pt";
            ul.CSS["padding-bottom"] = "25pt";
            ListItextSharp endList = (ListItextSharp) orderedUnorderedList.End(workerContextImpl, ul, listWithOne)[0];
            Assert.AreEqual(100f + 25f + 50f + 25f, ((ListItem) endList.Items[0]).SpacingAfter, 0f);
        }

        /**
	 * In this test the ul tag does not contain any margin or padding settings.
	 * Verifies if the margin- and padding-top of the first li and the margin- and padding-bottom of the last are set.
	 */

        [Test]
        virtual public void ListWithTwoNoListMarginAndPadding() {
            ListItextSharp endList = (ListItextSharp) orderedUnorderedList.End(workerContextImpl, ul, listWithTwo)[0];
            Assert.AreEqual(50f + 25f - 12f, ((ListItem) endList.Items[0]).SpacingBefore, 0f);
            Assert.AreEqual(50f + 25f, ((ListItem) endList.Items[1]).SpacingAfter, 0f);
        }

        /**
	 * Verifies if the largest of the margin-top's of the ul and it's first li is selected and added to the padding-top of li, because padding-top of ul == null.
	 */

        [Test]
        virtual public void ListWithTwoNoListPaddingTop() {
            ul.CSS["margin-top"] = "100pt";
            ListItextSharp endList = (ListItextSharp) orderedUnorderedList.End(workerContextImpl, ul, listWithTwo)[0];
            Assert.AreEqual(100f + 25f - 12f, ((ListItem) endList.Items[0]).SpacingBefore, 0f);
        }

        /**
	 * Verifies if the largest of the margin-top's of the ul and it's first li is selected and added to the padding-top of li, because padding-top of ul == 0.
	 */

        [Test]
        virtual public void ListWithTwoNoListPaddingTop2() {
            ul.CSS["margin-top"] = "100pt";
            ul.CSS["padding-top"] = "0pt";
            ListItextSharp endList = (ListItextSharp) orderedUnorderedList.End(workerContextImpl, ul, listWithTwo)[0];
            Assert.AreEqual(100f + 25f - 12f, ((ListItem) endList.Items[0]).SpacingBefore, 0f);
        }

        /**
	 * Verifies if margin-top and padding-top of both the ul and it's first li are added up, because padding-top of ul != 0.
	 */

        [Test]
        virtual public void ListWithTwoWithListPaddingTop() {
            ul.CSS["margin-top"] = "100pt";
            ul.CSS["padding-top"] = "25pt";
            ListItextSharp endList = (ListItextSharp) orderedUnorderedList.End(workerContextImpl, ul, listWithTwo)[0];
            Assert.AreEqual(100f + 25f + 50f + 25f - 12f, ((ListItem) endList.Items[0]).SpacingBefore, 0f);
        }

        /**
	 * Verifies if the largest of the margin-bottom's of the ul and it's last li is selected and added to the padding-bottom of li, because padding-bottom of ul == null.
	 */

        [Test]
        virtual public void ListWithTwoNoListPaddingBottom() {
            ul.CSS["margin-bottom"] = "100pt";
            ListItextSharp endList = (ListItextSharp) orderedUnorderedList.End(workerContextImpl, ul, listWithTwo)[0];
            Assert.AreEqual(100f + 25f, ((ListItem) endList.Items[1]).SpacingAfter, 0f);
        }

        /**
	 * Verifies if the largest of the margin-bottom's of the ul and it's last li is selected and added to the padding-bottom of li, because padding-bottom of ul == 0.
	 */

        [Test]
        virtual public void ListWithTwoNoListPaddingBottom2() {
            ul.CSS["margin-bottom"] = "100pt";
            ul.CSS["padding-bottom"] = "0pt";
            ListItextSharp endList = (ListItextSharp) orderedUnorderedList.End(workerContextImpl, ul, listWithTwo)[0];
            Assert.AreEqual(100f + 25f, ((ListItem) endList.Items[1]).SpacingAfter, 0f);
        }

        /**
	 * Verifies if margin-bottom and padding-bottom of both the ul and it's last li are added up, because padding-bottom of ul != 0.
	 */

        [Test]
        virtual public void ListWithTwoWithListPaddingBottom() {
            ul.CSS["margin-bottom"] = "100pt";
            ul.CSS["padding-bottom"] = "25pt";
            ListItextSharp endList = (ListItextSharp) orderedUnorderedList.End(workerContextImpl, ul, listWithTwo)[0];
            Assert.AreEqual(100f + 25f + 50f + 25f, ((ListItem) endList.Items[1]).SpacingAfter, 0f);
        }

        /**
	 * Verifies if {@link OrderedUnorderedList} is a stack owner. Should be true.
	 */

        [Test]
        virtual public void VerifyIfStackOwner() {
            Assert.IsTrue(orderedUnorderedList.IsStackOwner());
        }
    }
}
