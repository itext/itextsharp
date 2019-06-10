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
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.pipeline;
using iTextSharp.tool.xml.pipeline.ctx;
namespace iTextSharp.tool.xml.pipeline.end {

    /**
     * This pipeline writes to a Document.
     * @author redlab_b
     *
     */
    public class PdfWriterPipeline : AbstractPipeline {

        private static ILogger LOG = LoggerFactory.GetLogger(typeof(PdfWriterPipeline));
        private Document doc;
        private PdfWriter writer;

        /**
         */
        public PdfWriterPipeline() : base(null) {
        }

        /**
         * @param next the next pipeline if any.
         */
        public PdfWriterPipeline(IPipeline next) : base(next) {
        }

        /**
         * @param doc the document
         * @param writer the writer
         */
        public PdfWriterPipeline(Document doc, PdfWriter writer) : base(null) {
            this.doc = doc;
            this.writer = writer;
            continiously = true;
        }

        /**
         * The key for the {@link Document} in the {@link MapContext} used as {@link CustomContext}.
         */
        public const String DOCUMENT = "DOCUMENT";
        /**
         * The key for the {@link PdfWriter} in the {@link MapContext} used as {@link CustomContext}.
         */
        public const String WRITER = "WRITER";
        /**
         * The key for the a bool in the {@link MapContext} used as {@link CustomContext}. Setting to true enables swallowing of DocumentExceptions
         */
        public const String CONTINUOUS = "CONTINUOUS";
        private bool continiously;

        public override IPipeline Init(IWorkerContext context) {
            MapContext mc = new MapContext();
            continiously = true;
            mc[CONTINUOUS] = continiously;
            if (null != doc) {
                mc[DOCUMENT] = doc;
            }
            if (null != writer) {
                mc[WRITER] = writer;
            }
            context.Put(GetContextKey(), mc);
            return base.Init(context);
        }

        /**
         * @param po
         * @throws PipelineException
         */
        private void Write(IWorkerContext context, ProcessObject po) {
            MapContext mp = (MapContext)GetLocalContext(context);
            if (po.ContainsWritable()) {
                Document doc = (Document) mp[DOCUMENT];
                bool continuousWrite = (bool) mp[CONTINUOUS];
                IWritable writable = null;
                while (null != (writable = po.Poll())) {
                    if (writable is WritableElement) {
                        foreach (IElement e in ((WritableElement) writable).Elements()) {
                            try {
                                if (!doc.Add(e) && LOG.IsLogging(Level.TRACE)) {
                                    LOG.Trace(String.Format(
                                            LocaleMessages.GetInstance().GetMessage(LocaleMessages.ELEMENT_NOT_ADDED),
                                            e.ToString()));
                                }
                            } catch (DocumentException e1) {
                                if (!continuousWrite) {
                                    throw new PipelineException(e1);
                                } else {
                                    LOG.Error(
                                            LocaleMessages.GetInstance().GetMessage(LocaleMessages.ELEMENT_NOT_ADDED_EXC),
                                            e1);
                                }
                            }
                        }
                    }
                }
            }
        }

        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.pipeline.IPipeline#open(com.itextpdf.tool.
         * xml.Tag, com.itextpdf.tool.xml.pipeline.ProcessObject)
         */
        public override IPipeline Open(IWorkerContext context, Tag t, ProcessObject po) {
            Write(context, po);
            return GetNext();
        }

        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.pipeline.IPipeline#content(com.itextpdf.tool
         * .xml.Tag, java.lang.String, com.itextpdf.tool.xml.pipeline.ProcessObject)
         */
        public override IPipeline Content(IWorkerContext context, Tag t, string text, ProcessObject po) {
            Write(context, po);
            return GetNext();
        }

        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.pipeline.IPipeline#close(com.itextpdf.tool
         * .xml.Tag, com.itextpdf.tool.xml.pipeline.ProcessObject)
         */
        public override IPipeline Close(IWorkerContext context, Tag t, ProcessObject po) {
            Write(context, po);
            return GetNext();
        }

        /**
         * The document to write to.
         * @param document the Document
         */
        virtual public void SetDocument(Document document) {
            this.doc = document;
        }

        /**
         * The writer used to write to the document.
         * @param writer the writer.
         */
        virtual public void SetWriter(PdfWriter writer) {
            this.writer = writer;
        }
    }
}
