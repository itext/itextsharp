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
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css.apply;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html.pdfelement;
using iTextSharp.tool.xml.pipeline.html;

namespace iTextSharp.tool.xml.html {

    /**
     * @author redlab_b
     *
     */
    public class Div : AbstractTagProcessor {

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.ITagProcessor#content(com.itextpdf.tool.xml.Tag, java.lang.String)
         */
        public override IList<IElement> Content(IWorkerContext ctx, Tag tag, String content) {
            List<Chunk> sanitizedChunks = HTMLUtils.Sanitize(content, false);
            List<IElement> l = new List<IElement>(1);
            NoNewLineParagraph sanitizedNoNewLineParagraph = new NoNewLineParagraph();
            try {
                HtmlPipelineContext htmlPipelineContext = GetHtmlPipelineContext(ctx);
                foreach (Chunk sanitized in sanitizedChunks) {
                    sanitizedNoNewLineParagraph.Add(GetCssAppliers().Apply(sanitized, tag, htmlPipelineContext));
                }
                if (sanitizedNoNewLineParagraph.Count > 0) {
                    l.Add(GetCssAppliers().Apply(sanitizedNoNewLineParagraph, tag, htmlPipelineContext));
                }
            }
            catch (NoCustomContextException e) {
                throw new RuntimeWorkerException(e);
            }
            return l;
        }

        /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.tool.xml.ITagProcessor#endElement(com.itextpdf.tool.xml.Tag,
         * java.util.List, com.itextpdf.text.Document)
         */
        public override IList<IElement> End(IWorkerContext ctx, Tag tag, IList<IElement> currentContent) {
            try {
                Paragraph p = null;
                PdfDiv div = (PdfDiv)GetCssAppliers().Apply(new PdfDiv(), tag, GetHtmlPipelineContext(ctx));
                int direction = GetRunDirection(tag);

                if (direction != PdfWriter.RUN_DIRECTION_NO_BIDI) {
                    div.RunDirection = direction;
                }

			    foreach (IElement e in currentContent) {
				    if (e is Paragraph || e is PdfPTable || e is PdfDiv) {
					    if (p != null) {
                            if (p.Trim()) {
						        div.AddElement(p);
                            }
						    p = null;
					    }
					    div.AddElement(e);
                    } else {
					    if (p == null) {
						    p = new Paragraph();
                            p.Alignment = div.TextAlignment;

                            if (direction == PdfWriter.RUN_DIRECTION_RTL) {
                                InvertTextAlignForParagraph(p);
                            }

                            p.MultipliedLeading = 1.2f;
					    }

					    p.Add(e);
				    }
			    }

			    if (p != null && p.Trim()) {
                    div.AddElement(p);
			    }

                if (direction == PdfWriter.RUN_DIRECTION_RTL) {
                    invertTextAlignForDiv(div);
                }

			    List<IElement> l = new List<IElement>(1);
                l.Add(div);
                return l;
            } catch (NoCustomContextException e) {
                throw new RuntimeWorkerException(LocaleMessages.GetInstance().GetMessage(LocaleMessages.NO_CUSTOM_CONTEXT), e);
            }
        }

        private void invertTextAlignForDiv(PdfDiv div)
        {
            switch (div.TextAlignment)
            {
                case Element.ALIGN_UNDEFINED:
                case Element.ALIGN_CENTER:
                case Element.ALIGN_JUSTIFIED:
                case Element.ALIGN_JUSTIFIED_ALL:
                    break;
                case Element.ALIGN_RIGHT:
                    div.TextAlignment = Element.ALIGN_LEFT;
                    break;
                case Element.ALIGN_LEFT:
                default:
                    div.TextAlignment = Element.ALIGN_RIGHT;
                    break;
            }
        }

        /* (non-Javadoc)
        * @see com.itextpdf.tool.xml.ITagProcessor#isStackOwner()
        */
        public override bool IsStackOwner() {
            return true;
        }
    }
}
