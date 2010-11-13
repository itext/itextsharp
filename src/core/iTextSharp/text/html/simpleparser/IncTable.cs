using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
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
    public class IncTable : IElement {
        private Dictionary<string,string> props = new Dictionary<string,string>();
        private List<List<PdfPCell>> rows = new List<List<PdfPCell>>();
        private List<PdfPCell> cols;
        private float[] colWidths;

        /** Creates a new instance of IncTable */
        public IncTable(Dictionary<string,string> props) {
            foreach (KeyValuePair<string,string> dc in props)
                this.props[dc.Key] = dc.Value;
        }
        
        public void AddCol(PdfPCell cell) {
            if (cols == null)
                cols = new List<PdfPCell>();
            cols.Add(cell);
        }
        
        public void AddCols(List<PdfPCell> ncols) {
            if (cols == null)
                cols = new List<PdfPCell>(ncols);
            else
                cols.AddRange(ncols);
        }
        
        public void EndRow() {
            if (cols != null) {
                cols.Reverse();
                rows.Add(cols);
                cols = null;
            }
        }
        
        public List<List<PdfPCell>> Rows {
            get {
                return rows;
            }
        }
        
        public float[] ColWidths {
            get { return colWidths; }
            set { colWidths = value; }
        }

        public PdfPTable BuildTable() {
            if (rows.Count == 0)
                return new PdfPTable(1);
            int ncol = 0;
            foreach (PdfPCell pc in rows[0]) {
                ncol += pc.Colspan;
            }
            PdfPTable table = new PdfPTable(ncol);
            String width;
            if (!props.TryGetValue("width", out width))
                table.WidthPercentage = 100;
            else {
                if (width.EndsWith("%"))
                    table.WidthPercentage = float.Parse(width.Substring(0, width.Length - 1), System.Globalization.NumberFormatInfo.InvariantInfo);
                else {
                    table.TotalWidth = float.Parse(width, System.Globalization.NumberFormatInfo.InvariantInfo);
                    table.LockedWidth = true;
                }
            }
        
            // Support for horizontal alignment of tables via HTML conversion
            string alignment;
            props.TryGetValue("align", out alignment);
            int align = Element.ALIGN_LEFT;
            if (alignment != null) {
                align = ElementTags.AlignmentValue(alignment);
            }
            table.HorizontalAlignment = align;
            
            foreach (List<PdfPCell> col in rows) {
                foreach (PdfPCell pc in col) {
                    table.AddCell(pc);
                }
            }
		    try {
			    if (colWidths != null)
				    table.SetWidths(colWidths);
		    } catch {
			    // fail silently
		    }
            return table;
        }

        public bool Process(IElementListener listener) {
            return false;
        }

        public int Type {
            get { 
                return 0;
            }
        }

        public bool IsContent() {
            return false;
        }

        public bool IsNestable() {
            return false;
        }

        public List<Chunk> Chunks {
            get {
                return null;    
            }
        }
    }
}
