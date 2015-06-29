using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;

/*
 * 
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
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

namespace iTextSharp.text.pdf.draw {

    /**
    * Helper class implementing the DrawInterface. Can be used to add
    * horizontal or vertical separators. Won't draw anything unless
    * you implement the draw method.
    * @since   2.1.2
    */

    public class VerticalPositionMark : IDrawInterface, IElement {

        /** Another implementation of the DrawInterface; its draw method will overrule LineSeparator.Draw(). */
        protected IDrawInterface drawInterface = null;

        /** The offset for the line. */
        protected float offset = 0;
        
        /**
        * Creates a vertical position mark that won't draw anything unless
        * you define a DrawInterface.
        */
        public VerticalPositionMark() { 
        }

        /**
        * Creates a vertical position mark that won't draw anything unless
        * you define a DrawInterface.
        * @param   drawInterface   the drawInterface for this vertical position mark.
        * @param   offset          the offset for this vertical position mark.
        */
        public VerticalPositionMark(IDrawInterface drawInterface, float offset) {
            this.drawInterface = drawInterface;
            this.offset = offset;
        }
        
        /**
        * @see com.lowagie.text.pdf.draw.DrawInterface#draw(com.lowagie.text.pdf.PdfContentByte, float, float, float, float, float)
        */
        public virtual void Draw(PdfContentByte canvas, float llx, float lly, float urx, float ury, float y) {
            if (drawInterface != null) {
                drawInterface.Draw(canvas, llx, lly, urx, ury, y + offset);
            }
        }
        
        /**
        * @see com.lowagie.text.Element#process(com.lowagie.text.ElementListener)
        */
        virtual public bool Process(IElementListener listener) {
            try {
                return listener.Add(this);
            } catch (DocumentException) {
                return false;
            }
        }

        /**
        * @see com.lowagie.text.Element#type()
        */
        virtual public int Type {
            get {
                return Element.YMARK;
            }
        }

        /**
        * @see com.lowagie.text.Element#isContent()
        */
        virtual public bool IsContent() {
            return true;
        }

        /**
        * @see com.lowagie.text.Element#isNestable()
        */
        virtual public bool IsNestable() {
            return false;
        }

        /**
        * @see com.lowagie.text.Element#getChunks()
        */
        virtual public IList<Chunk> Chunks {
            get {
                List<Chunk> list = new List<Chunk>();
                list.Add(new Chunk(this, true));
                return list;
            }
        }

        /**
        * Setter for the interface with the overruling Draw() method.
        * @param drawInterface a DrawInterface implementation
        */
        public virtual IDrawInterface DrawInterface {
            set {
                drawInterface = value;
            }
            get {
                return drawInterface;
            }
        }

        /**
        * Setter for the offset. The offset is relative to the current
        * Y position. If you want to underline something, you have to
        * choose a negative offset.
        * @param offset    an offset
        */
        public virtual float Offset {
            set {
                offset = value;
            }
            get {
                return offset;
            }
        }
    }
}
