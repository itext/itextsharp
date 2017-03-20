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
using System;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline;
namespace iTextSharp.tool.xml.pipeline.end {

    /**
     * As the {@link PdfWriterPipeline} but this one just passes everything on to an {@link ElementHandler}.
     * Allowing you to get all {@link Writable}s at the end of the pipeline. (or in between)
     * @author redlab_b
     *
     */
    public class ElementHandlerPipeline : AbstractPipeline {

        private IElementHandler handler;

        /**
         * Does not use a context.
         * @param handler the ElementHandler
         * @param next the next pipeline in line. (or <code>null</code> if none )
         */
        public ElementHandlerPipeline(IElementHandler handler, IPipeline next) : base(next) {
            this.handler = handler;
        }

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.pipeline.AbstractPipeline#open(com.itextpdf.tool.xml.Tag, com.itextpdf.tool.xml.pipeline.ProcessObject)
         */
        public override IPipeline Open(IWorkerContext context, Tag t, ProcessObject po) {
            Consume(context, po);
            return GetNext();
        }

        /**
         * Called in <code>open</code>, <code>content</code> and <code>close</code> to pass the {@link Writable}s to the handler
         * @param po
         */
        private void Consume(IWorkerContext context, ProcessObject po) {
            if (po.ContainsWritable()) {
                IWritable w = null;
                while ( null != (w =po.Poll())) {
                    handler.Add(w);
                }
            }
        }

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.pipeline.AbstractPipeline#content(com.itextpdf.tool.xml.Tag, java.lang.String, com.itextpdf.tool.xml.pipeline.ProcessObject)
         */
        public override IPipeline Content(IWorkerContext context, Tag t, string text, ProcessObject po) {
            Consume(context, po);
            return GetNext();
        }

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.pipeline.AbstractPipeline#close(com.itextpdf.tool.xml.Tag, com.itextpdf.tool.xml.pipeline.ProcessObject)
         */
        public override IPipeline Close(IWorkerContext context, Tag t, ProcessObject po) {
            Consume(context, po);
            return GetNext();
        }

    }
}
