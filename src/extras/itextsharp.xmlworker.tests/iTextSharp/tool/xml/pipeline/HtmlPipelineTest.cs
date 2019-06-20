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
using System.Text;
using iTextSharp.text;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.ctx;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.pipeline {
    internal class HtmlPipelineTest {
        private HtmlPipeline p;
        private WorkerContextImpl wc;
        private HtmlPipelineContext hpc;
        private static String b;
        /**
	 * @
	 *
	 */

        [SetUp]
        virtual public void SetUp() {
            hpc = new HtmlPipelineContext(null);
            p = new HtmlPipeline(hpc, null);
            wc = new WorkerContextImpl();
            p.Init(wc);
        }

        [TearDown]
        virtual public void TearDown() {
            b = null;
        }

        private class CustomTagProcessor : ITagProcessor {
            virtual public IList<IElement> StartElement(IWorkerContext ctx,
                Tag tag) {
                return new List<IElement>(0);
            }

            public virtual bool IsStackOwner() {
                return false;
            }

            public virtual IList<IElement> EndElement (IWorkerContext ctx, Tag tag, IList<IElement> currentContent) {
                return new List<IElement>(0);
            }

            public virtual IList<IElement> Content(IWorkerContext ctx, Tag tag, String content) {
                Assert.AreEqual(b, content);
                return new List<IElement>(0);
            }
        }

        private class CustomTagProcessorFactory : ITagProcessorFactory {
            virtual public void RemoveProcessor(String tag) {
            }

            public virtual ITagProcessor GetProcessor(String tag, String nameSpace) {
                if (tag.Equals("tag", StringComparison.OrdinalIgnoreCase)) ;
                return new CustomTagProcessor();
            }

            public virtual void AddProcessor(ITagProcessor processor, params String[] tags) {
            }
        }


        [Test]
        virtual public void Init() {
            Assert.NotNull(p.GetLocalContext(wc));
        }

        [Test]
        virtual public void Text() {
            b = Encoding.GetEncoding("ISO-8859-1").GetString(Encoding.Default.GetBytes("aeéèàçï"));
            ITagProcessorFactory tagFactory = new CustomTagProcessorFactory();


            ((HtmlPipelineContext) p.GetLocalContext(wc)).SetTagFactory(tagFactory)
                .CharSet(Encoding.GetEncoding("ISO-8859-1"));
            p.Content(wc, new Tag("tag"), b, new ProcessObject());
        }
    }
}
