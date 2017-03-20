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
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using iTextSharp.tool.xml.css.apply;
using iTextSharp.tool.xml.pipeline.html;
using iTextSharp.tool.xml.html.pdfelement;
using System.Collections.Generic;

namespace iTextSharp.tool.xml.html {

/**
 * Applies CSS to an Element using the appliers from the <code>com.itextpdf.tool.xml.css.apply</code>.
 *
 * @author redlab_b
 *
 *
 */

    public class CssAppliersImpl : CssAppliers {

        /*
         * private static CssAppliersImpl myself = new CssAppliersImpl();
         *
         * public static CssAppliersImpl GetInstance() { return myself; }
         */
        private IDictionary<Type, ICssApplier> map;
        /**
         *
         */

        public CssAppliersImpl() {
            map = new Dictionary<Type, ICssApplier>();
            map[typeof (Chunk)] = new ChunkCssApplier(null);
            map[typeof(Paragraph)] = new ParagraphCssApplier(this);
            map[typeof (NoNewLineParagraph)] = new NoNewLineParagraphCssApplier();
            map[typeof (HtmlCell)] = new HtmlCellCssApplier();
            map[typeof (List)] = new ListStyleTypeCssApplier();
            map[typeof (LineSeparator)] = new LineSeparatorCssApplier();
            map[typeof (text.Image)] = new ImageCssApplier();
            map[typeof (PdfDiv)] = new DivCssApplier();
        }
        public CssAppliersImpl(IFontProvider fontProvider)
            : this() {
            ((ChunkCssApplier)map[typeof(Chunk)]).FontProvider = fontProvider;
        }

        public void PutCssApplier(Type t, ICssApplier c) {
            map[t] = c;
        }

        public ICssApplier GetCssApplier(Type t) {
            ICssApplier c;
            map.TryGetValue(t, out c);
            return c;
        }

        

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.html.CssAppliers#apply(com.itextpdf.text.Element, com.itextpdf.tool.xml.Tag, com.itextpdf.tool.xml.css.apply.MarginMemory, com.itextpdf.tool.xml.css.apply.PageSizeContainable, com.itextpdf.tool.xml.pipeline.html.ImageProvider)
         */

        public virtual IElement Apply(IElement e, Tag t, IMarginMemory mm, IPageSizeContainable psc, HtmlPipelineContext ctx) {
            ICssApplier c = null;
            foreach (KeyValuePair<Type, ICssApplier> entry in map) {
                if (entry.Key.IsInstanceOfType(e)) {
                    c = entry.Value;
                    break;
                }
            }
            if (c == null) {
                throw new Exception();
            }
            e = c.Apply(e, t, mm, psc, ctx);
            return e;
        }

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.html.CssAppliers#apply(com.itextpdf.text.Element, com.itextpdf.tool.xml.Tag, com.itextpdf.tool.xml.pipeline.html.HtmlPipelineContext)
         */

        public virtual IElement Apply(IElement e, Tag t, HtmlPipelineContext ctx) {
            return this.Apply(e, t, ctx, ctx, ctx);
        }

        public virtual ChunkCssApplier GetChunkCssAplier() {
            return (ChunkCssApplier) map[typeof (Chunk)];
        }

        public virtual ChunkCssApplier ChunkCssAplier {
            get { return (ChunkCssApplier) map[typeof (Chunk)]; }
            set { map[typeof (Chunk)] = value; }
        }

        public virtual CssAppliers Clone() {
            CssAppliersImpl clone = GetClonedObject();
            clone.map = new Dictionary<Type, ICssApplier>(map);
            return clone;
        }

        protected virtual CssAppliersImpl GetClonedObject() {
            return new CssAppliersImpl();
        }
    }
}
