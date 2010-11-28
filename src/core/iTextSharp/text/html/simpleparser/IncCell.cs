using System;
using System.Collections.Generic;
using System.util;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
/*
 * This file is part of the iText project.
 * Copyright (c) 1998-2009 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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

namespace iTextSharp.text.html.simpleparser {
    /**
    *
    * @author  psoares
    */
    public class IncCell : ITextElementArray {
        
        private List<Chunk> chunks = new List<Chunk>();
        private PdfPCell cell;
        /**
         * The width of the cell.
         * @since iText 5.0.6
         */
        private float width;
        /**
         * Indicates if the width is a percentage.
         * @since iText 5.0.6
         */
        private bool percentage;
        
        /** Creates a new instance of IncCell */
        public IncCell(String tag, ChainedProperties props) {
            cell = new PdfPCell();
            String value = props["colspan"];
            if (value != null)
                cell.Colspan = int.Parse(value);
            value = props["rowspan"];
            if (value != null)
                cell.Rowspan = int.Parse(value);
            value = props["align"];
            if (tag.Equals("th"))
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
            if (value != null) {
                if (Util.EqualsIgnoreCase(value, "center"))
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                else if (Util.EqualsIgnoreCase(value, "right"))
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                else if (Util.EqualsIgnoreCase(value, "left"))
                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                else if (Util.EqualsIgnoreCase(value, "justify"))
                    cell.HorizontalAlignment = Element.ALIGN_JUSTIFIED;
            }
            value = props["valign"];
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            if (value != null) {
                if (Util.EqualsIgnoreCase(value, "top"))
                    cell.VerticalAlignment = Element.ALIGN_TOP;
                else if (Util.EqualsIgnoreCase(value, "bottom"))
                    cell.VerticalAlignment = Element.ALIGN_BOTTOM;
            }
            value = props["border"];
            float border = 0;
            if (value != null)
                border = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            cell.BorderWidth = border;
            value = props["cellpadding"];
            if (value != null)
                cell.Padding = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            cell.UseDescender = true;
            value = props["bgcolor"];
            cell.BackgroundColor = Markup.DecodeColor(value);
            value = props["width"];
            if (value != null) {
                value = value.Trim();
                if (value.EndsWith("%")) {
                    percentage = true;
                    value = value.Substring(0, value.Length - 1);
                }
                width = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            }
        }
        
        public bool Add(IElement o) {
            cell.AddElement(o);
            return true;
        }
        
        public List<Chunk> Chunks {
            get {
                return chunks;
            }
        }
        
        public bool Process(IElementListener listener) {
            return true;
        }
        
        public int Type {
            get {
                return Element.RECTANGLE;
            }
        }
        
        public PdfPCell Cell {
            get {
                return cell;
            }
        }    

        /**
        * @see com.lowagie.text.Element#isContent()
        * @since   iText 2.0.8
        */
        public bool IsContent() {
            return true;
        }

        /**
        * @see com.lowagie.text.Element#isNestable()
        * @since   iText 2.0.8
        */
        public bool IsNestable() {
            return true;
        }
  

        public float Width {
            get { return width; }
        }

        public bool IsPercentage {
            get {
                return percentage;
            }
        }

        public override string ToString() {
            return base.ToString();
        }
    }
}
