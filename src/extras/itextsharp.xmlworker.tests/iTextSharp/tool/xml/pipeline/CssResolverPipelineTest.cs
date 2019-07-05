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
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.ctx;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.pipeline {
    internal class CssResolverPipelineTest {
        private IDictionary<String, String> css2;

        [SetUp]
        virtual public void SetUp() {
            StyleAttrCSSResolver css = new StyleAttrCSSResolver();
            css.AddCss("dummy { key1: value1; key2: value2 } .aklass { key3: value3;} #dumid { key4: value4}", true);
            CssResolverPipeline p = new CssResolverPipeline(css, null);
            Tag t = new Tag("dummy");
            t.Attributes["id"] = "dumid";
            t.Attributes["class"] = "aklass";
            WorkerContextImpl context = new WorkerContextImpl();
            p.Init(context);
            IPipeline open = p.Open(context, t, null);
            css2 = t.CSS;
        }

        /**
	 * Verify that pipeline resolves css on tag.
	 *
	 * @throws CssResolverException
	 * @throws PipelineException
	 */

        [Test]
        virtual public void VerifyCssResolvedTag() {
            Assert.AreEqual("value1", css2["key1"]);
        }

        /**
	 * Verify that pipeline resolves css on tag2.
	 *
	 * @throws CssResolverException
	 * @throws PipelineException
	 */

        [Test]
        virtual public void VerifyCssResolvedTag2() {
            Assert.AreEqual("value2", css2["key2"]);
        }

        /**
	 * Verify that pipeline resolves css class.
	 *
	 * @throws CssResolverException
	 * @throws PipelineException
	 */

        [Test]
        virtual public void VerifyCssResolvedClass() {
            Assert.AreEqual("value3", css2["key3"]);
        }

        /**
	 * Verify that pipeline resolves css id.
	 *
	 * @throws CssResolverException
	 * @throws PipelineException
	 */

        [Test]
        virtual public void VerifyCssResolvedId() {
            Assert.AreEqual("value4", css2["key4"]);
        }
    }
}
