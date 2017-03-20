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
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline.ctx;
using NUnit.Framework;
using iTextSharp.tool.xml.pipeline;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.pipeline {
    internal class PipelineTest {
        private AbstractPipelineExtension abstractPipelineExtension;
        private AbstractPipeline ap;
        private IWorkerContext ctx;
        /**
	 *
	 */

        private sealed class AbstractPipelineExtension : AbstractPipeline {
            /**
		 * @param next
		 */

            public AbstractPipelineExtension(IPipeline next) : base(next) {
            }
        }

        private class CustomAbstractPipeline : AbstractPipeline {
            public CustomAbstractPipeline(IPipeline next)
                : base(next) {
            }
        }

        /** Init test. */

        [SetUp]
        virtual public void SetUp() {
            ctx = new WorkerContextImpl();
            abstractPipelineExtension = new AbstractPipelineExtension(null);
            ap = new CustomAbstractPipeline(abstractPipelineExtension);
        }

        /**
	 * Expect a {@link PipelineException} on calling getNewNoCustomContext.
	 * @
	 */

        [Test]
        virtual public void ValidateNoCustomContextExceptionThrown() {
            AbstractPipeline ap = new CustomAbstractPipeline(null);
            Assert.Throws(typeof (PipelineException), delegate { ap.GetLocalContext(ctx); });
        }

        /**
	 * Verify that getNext actually returns the next pipeline.
	 */

        [Test]
        virtual public void ValidateNext() {
            Assert.AreEqual(abstractPipelineExtension, ap.GetNext());
        }

        /**
	 * Verify that close actually returns the next pipeline.
	 */

        [Test]
        virtual public void ValidateNextClose() {
            Assert.AreEqual(abstractPipelineExtension, ap.Close(ctx, null, null));
        }

        /**
	 * Verify that open actually returns the next pipeline.
	 */

        [Test]
        virtual public void ValidateNextOpen() {
            Assert.AreEqual(abstractPipelineExtension, ap.Open(ctx, null, null));
        }

        /**
	 * Verify that content actually returns the next pipeline.
	 */

        [Test]
        virtual public void ValidateNextContent() {
            Assert.AreEqual(abstractPipelineExtension, ap.Content(ctx, null, null, null));
        }
    }
}
