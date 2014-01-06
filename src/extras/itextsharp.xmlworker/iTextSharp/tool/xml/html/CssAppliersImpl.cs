/*
 * $Id: CssAppliersImpl.java 265 2012-01-17 12:38:18Z eugenemark $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2014 iText Group NV
 * Authors: Balder Van Camp, Emiel Ackermann, et al.
 *
 * This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General
 * Public License version 3 as published by the Free Software Foundation with the addition of the following permission
 * added to Section 15 as permitted in Section 7(a): FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP, ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more
 * details. You should have received a copy of the GNU Affero General Public License along with this program; if not,
 * see http://www.gnu.org/licenses or write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL: http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions of this program must display Appropriate
 * Legal Notices, as required under Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License, a covered work must retain the producer
 * line in every PDF that is created or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing a commercial license. Buying such a license is
 * mandatory as soon as you develop commercial activities involving the iText software without disclosing the source
 * code of your own applications. These activities include: offering paid services to customers as an ASP, serving PDFs
 * on the fly in a web application, shipping iText with a closed source product.
 *
 * For more information, please contact iText Software Corp. at this address: sales@itextpdf.com
 */
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using iTextSharp.tool.xml.css.apply;
using iTextSharp.tool.xml.pipeline.html;
using iTextSharp.tool.xml.html.pdfelement;

namespace iTextSharp.tool.xml.html {

/**
 * Applies CSS to an Element using the appliers from the <code>com.itextpdf.tool.xml.css.apply</code>.
 *
 * @author redlab_b
 *
 *
 */

    public class CssAppliersImpl : CssAppliers
    {

        /*
         * private static CssAppliersImpl myself = new CssAppliersImpl();
         *
         * public static CssAppliersImpl GetInstance() { return myself; }
         */
        protected ChunkCssApplier chunk;
        protected ParagraphCssApplier paragraph;
        private NoNewLineParagraphCssApplier nonewlineparagraph;
        private HtmlCellCssApplier htmlcell;
        private ListStyleTypeCssApplier list;
        private LineSeparatorCssApplier lineseparator;
        private ImageCssApplier image;
        private DivCssApplier div;
        /**
         *
         */

        public CssAppliersImpl()
        {
            chunk = new ChunkCssApplier(null);
            paragraph = new ParagraphCssApplier(this);
            nonewlineparagraph = new NoNewLineParagraphCssApplier();
            htmlcell = new HtmlCellCssApplier();
            list = new ListStyleTypeCssApplier();
            lineseparator = new LineSeparatorCssApplier();
            image = new ImageCssApplier();
            div = new DivCssApplier();
        }

        public CssAppliersImpl(IFontProvider fontProvider)
            : this()
        {
            chunk.FontProvider = fontProvider;
        }

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.html.CssAppliers#apply(com.itextpdf.text.Element, com.itextpdf.tool.xml.Tag, com.itextpdf.tool.xml.css.apply.MarginMemory, com.itextpdf.tool.xml.css.apply.PageSizeContainable, com.itextpdf.tool.xml.pipeline.html.ImageProvider)
         */

        virtual public IElement Apply(IElement e, Tag t, IMarginMemory mm, IPageSizeContainable psc, IImageProvider ip)
        {
            // warning, mapping is done by instance of, make sure to add things in the right order when adding more.
            if (e is Chunk)
            {
                // covers TabbedChunk & Chunk
                e = chunk.Apply((Chunk) e, t);
            }
            else if (e is Paragraph)
            {
                e = paragraph.Apply((Paragraph) e, t, mm);
            }
            else if (e is NoNewLineParagraph)
            {
                e = nonewlineparagraph.Apply((NoNewLineParagraph) e, t, mm);
            }
            else if (e is HtmlCell)
            {
                e = htmlcell.Apply((HtmlCell) e, t, mm, psc);
            }
            else if (e is List)
            {
                e = list.Apply((List) e, t, ip);
            }
            else if (e is LineSeparator)
            {
                e = lineseparator.Apply((LineSeparator) e, t, psc);
            }
            else if (e is text.Image)
            {
                e = image.Apply((text.Image) e, t);
            } else if (e is PdfDiv) {
                e = div.apply((PdfDiv)e, t, mm, psc);
            }
            return e;

        }

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.html.CssAppliers#apply(com.itextpdf.text.Element, com.itextpdf.tool.xml.Tag, com.itextpdf.tool.xml.pipeline.html.HtmlPipelineContext)
         */

        virtual public IElement Apply(IElement e, Tag t, HtmlPipelineContext ctx)
        {
            try
            {
                return this.Apply(e, t, ctx, ctx, ctx.GetImageProvider());
            }
            catch (NoImageProviderException)
            {
                return this.Apply(e, t, ctx, ctx, null);
            }
        }

        virtual public ChunkCssApplier GetChunkCssAplier()
        {
            return chunk;
        }

        virtual public ChunkCssApplier ChunkCssAplier {
            get { return this.chunk; }
            set { this.chunk = value; }
        }

        virtual public CssAppliers Clone()
        {
            CssAppliersImpl clone = new CssAppliersImpl();
            clone.chunk = chunk;

            clone.paragraph = paragraph;
            clone.nonewlineparagraph = nonewlineparagraph;
            clone.htmlcell = htmlcell;
            clone.list = list;
            clone.lineseparator = lineseparator;
            clone.image = image;
            clone.div = div;

            return clone;
        }
    }
}
