/*
 * $Id: $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: VVB, Bruno Lowagie, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */

using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml.pipeline.ctx;
using iTextSharp.tool.xml.pipeline;
using iTextSharp.tool.xml.svg.graphic;
using iTextSharp.tool.xml.svg.tags;
using iTextSharp.tool.xml.svg.utils;

namespace iTextSharp.tool.xml.svg {

/**
 * This pipeline writes to a Document.
 * @author redlab_b
 *
 */
    public class PdfTemplatePipeline : AbstractPipeline
    {

        private PdfTemplate template;

        /**
         * @param doc the document
         * @param writer the writer
         */
        public PdfTemplatePipeline(PdfContentByte cb) : base(null) {
            template = cb.CreateTemplate(0, 0);
        }

        /**
         * @param po
         * @throws PipelineException
         */

        //lijst met graphische elementen (en andere?)
        private void Write(IWorkerContext context, ProcessObject po, Tag t)
        {
            //MapContext mp = GetLocalContext(context);
            if (po.ContainsWritable()) {
                //Document doc = (Document) mp.Get(DOCUMENT);
                //bool continuousWrite = (Boolean) mp.Get(CONTINUOUS);
                IWritable writable = null;
                while (null != (writable = po.Poll())) {
                    if (writable is Graphic) {
                        ((Graphic)writable).Draw(template, t.CSS);
                        if (writable is Svg) {
                            Svg svg = (Svg)writable;
                            text.Rectangle viewBox = svg.ViewBox;
                            template.BoundingBox = new text.Rectangle(viewBox.Width, viewBox.Height);
                        }
                    }
                }
            }
        }

        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.pipeline.Pipeline#open(com.itextpdf.tool.
         * xml.Tag, com.itextpdf.tool.xml.pipeline.ProcessObject)
         */
        public override IPipeline Open(IWorkerContext context, Tag t, ProcessObject po)
        {
            IDictionary<String, String> attributes = t.Attributes;
            if (attributes != null) {
                String transform;
                if (attributes.TryGetValue("transform", out transform) && transform != null)
                {
                    Matrix matrix = TransformationMatrix.GetTransformationMatrix(transform);
                    if (matrix != null) {
                        template.ConcatCTM(matrix);
                    }
                }
            }
            Write(context, po, t);

            return GetNext();
        }

        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.pipeline.Pipeline#content(com.itextpdf.tool
         * .xml.Tag, java.lang.String, com.itextpdf.tool.xml.pipeline.ProcessObject)
         */
        public override IPipeline Content(IWorkerContext context, Tag currentTag, String text, ProcessObject po)
        {
            Write(context, po, currentTag);
            return GetNext();
        }

        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.pipeline.Pipeline#close(com.itextpdf.tool
         * .xml.Tag, com.itextpdf.tool.xml.pipeline.ProcessObject)
         */
        public override IPipeline Close(IWorkerContext context, Tag t, ProcessObject po)
        {
            Write(context, po, t);
            //writer.GetDirectContent().RestoreState();
            return GetNext();
        }

        public PdfTemplate GetTemplate()
        {
            return template;
        }
    }
}
