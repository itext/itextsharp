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
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html.pdfelement;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.pipeline.ctx;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
namespace iTextSharp.tool.xml.html {

    /**
     * @author redlab_b
     *
     */
    public class Body : AbstractTagProcessor {

        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.ITagProcessor#content(com.itextpdf.tool.xml.Tag, java.util.List,
         * com.itextpdf.text.Document, java.lang.String)
         */
        public override IList<IElement> Content(IWorkerContext ctx, Tag tag, String content) {
            List<Chunk> sanitizedChunks = HTMLUtils.Sanitize(content, false);
		    List<IElement> l = new List<IElement>(1);
            NoNewLineParagraph sanitizedNoNewLineParagraph = new NoNewLineParagraph();
            foreach (Chunk sanitized in sanitizedChunks) {
                Chunk c = GetCssAppliers().ChunkCssAplier.Apply(sanitized, tag);
                sanitizedNoNewLineParagraph.Add(c);
            }
            if (sanitizedNoNewLineParagraph.Count > 0) {
                try {
                    l.Add(GetCssAppliers().Apply(sanitizedNoNewLineParagraph, tag, GetHtmlPipelineContext(ctx)));
                } catch (NoCustomContextException e) {
                    throw new RuntimeWorkerException(LocaleMessages.GetInstance().GetMessage(LocaleMessages.NO_CUSTOM_CONTEXT), e);
                }
            }
		    return l;
        }

        public override IList<IElement> Start(IWorkerContext ctx, Tag tag)
        {
            List<IElement> l = new List<IElement>(1);
            try
            {
                IDictionary<String, String> css = tag.CSS;
                if (css.ContainsKey(CSS.Property.BACKGROUND_COLOR))
                {
                    Type type = typeof(PdfWriterPipeline);
                    MapContext pipeline = (MapContext)ctx.Get(type.FullName);
                    if (pipeline != null)
                    {
                        Document document = (Document)pipeline[PdfWriterPipeline.DOCUMENT];
                        if (document != null)
                        {
                            Rectangle rectangle = new Rectangle(document.Left, document.Bottom, document.Right, document.Top, document.PageSize.Rotation);
                            rectangle.BackgroundColor = HtmlUtilities.DecodeColor(css[CSS.Property.BACKGROUND_COLOR]);
                            PdfBody body = new PdfBody(rectangle);
                            l.Add(body);
                        }
                    }
                }
            }
            catch (NoCustomContextException e)
            {}
            
            return l;
        }
    }
}
