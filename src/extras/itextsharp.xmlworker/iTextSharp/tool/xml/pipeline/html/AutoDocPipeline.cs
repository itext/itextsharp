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
using System.IO;
using System.util;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.pipeline;
using iTextSharp.tool.xml.pipeline.ctx;
using iTextSharp.tool.xml.pipeline.end;
namespace iTextSharp.tool.xml.pipeline.html {

    /**
     * This pipeline can automagically create documents. Allowing you to parse
     * continuously, without needing to renew the configuration. This class does
     * expect {@link PdfWriterPipeline} to be the last pipe of the line. If a
     * {@link HtmlPipeline} is available it's context will also be reset.
     *
     * @author redlab_b
     *
     */
    public class AutoDocPipeline : AbstractPipeline {

        private IFileMaker fm;
        private String tag;
        private String opentag;
        private Rectangle pagesize;

        /**
         * Constructor
         * @param fm a FileMaker to provide a stream for every new document
         * @param tag the tag on with to create a new document and close it
         * @param opentag the tag on which to open the document ( {@link Document#open()}
         * @param pagesize the pagesize for the documents
         *
         */
        public AutoDocPipeline(IFileMaker fm, String tag, String opentag, Rectangle pagesize) : base(null) {
            this.fm = fm;
            this.tag = tag;
            this.opentag = opentag;
            this.pagesize = pagesize;
        }

        /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.tool.xml.pipeline.AbstractPipeline#open(com.itextpdf.tool
         * .xml.Tag, com.itextpdf.tool.xml.pipeline.ProcessObject)
         */
        public override IPipeline Open(IWorkerContext context, Tag t, ProcessObject po) {
            try {
                String tagName = t.Name;
                if (tag.Equals(tagName)) {
                    MapContext cc;
                    cc = (MapContext) context.Get(typeof(PdfWriterPipeline).FullName);
                    Document d = new Document(pagesize);
                    try {
                        Stream os = fm.GetStream();
                        cc[PdfWriterPipeline.DOCUMENT] = d;
                        cc[PdfWriterPipeline.WRITER] = PdfWriter.GetInstance(d, os);
                    } catch (IOException e) {
                        throw new PipelineException(e);
                    } catch (DocumentException e) {
                        throw new PipelineException(e);
                    }

                }
                if (Util.EqualsIgnoreCase(t.Name, opentag)) {
                    MapContext cc;
                    cc = (MapContext) context.Get(typeof(PdfWriterPipeline).FullName);
                    Document d = (Document) cc[PdfWriterPipeline.DOCUMENT];
                    CssUtils cssUtils = CssUtils.GetInstance();
                    float pageWidth = d.PageSize.Width;
                    float marginLeft = 0;
                    float marginRight = 0;
                    float marginTop = 0;
                    float marginBottom = 0;
                    IDictionary<String, String> css = t.CSS;
                    foreach (KeyValuePair<String, String> entry in css) {
                        String key = entry.Key;
                        String value = entry.Value;
                        if (Util.EqualsIgnoreCase(key, CSS.Property.MARGIN_LEFT)) {
                            marginLeft = cssUtils.ParseValueToPt(value, pageWidth);
                        } else if (Util.EqualsIgnoreCase(key, CSS.Property.MARGIN_RIGHT)) {
                            marginRight = cssUtils.ParseValueToPt(value, pageWidth);
                        } else if (Util.EqualsIgnoreCase(key, CSS.Property.MARGIN_TOP)) {
                            marginTop = cssUtils.ParseValueToPt(value, pageWidth);
                        } else if (Util.EqualsIgnoreCase(key, CSS.Property.MARGIN_BOTTOM)) {
                            marginBottom = cssUtils.ParseValueToPt(value, pageWidth);
                        }
                    }
                    d.SetMargins(marginLeft, marginRight, marginTop, marginBottom);
                    d.Open();

                }
            } catch (NoCustomContextException e) {
                throw new PipelineException(LocaleMessages.GetInstance().GetMessage(LocaleMessages.PIPELINE_AUTODOC), e);
            }

            return GetNext();
        }

        /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.tool.xml.pipeline.AbstractPipeline#close(com.itextpdf.tool
         * .xml.Tag, com.itextpdf.tool.xml.pipeline.ProcessObject)
         */
        public override IPipeline Close(IWorkerContext context, Tag t, ProcessObject po) {
            String tagName = t.Name;
            if (tag.Equals(tagName)) {
                MapContext cc;
                try {
                    cc = (MapContext) context.Get(typeof(PdfWriterPipeline).FullName);
                    Document d = (Document) cc[PdfWriterPipeline.DOCUMENT];
                    d.Close();
                } catch (NoCustomContextException e) {
                    throw new PipelineException("AutoDocPipeline depends on PdfWriterPipeline.", e);
                }
                try {
                    HtmlPipelineContext hpc = (HtmlPipelineContext) context.Get(typeof(HtmlPipeline).FullName);
                    HtmlPipelineContext clone = (HtmlPipelineContext)hpc.Clone();
                    clone.SetPageSize(pagesize);
                    ((WorkerContextImpl)context).Put(typeof(HtmlPipeline).FullName, clone);
                } catch (NoCustomContextException) {
                }
            }
            return GetNext();
        }
    }
}
