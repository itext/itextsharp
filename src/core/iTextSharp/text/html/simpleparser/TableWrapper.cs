using System;
using System.Collections.Generic;
using System.Globalization;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
/*
 * $Id: IncTable.java 4632 2010-11-24 14:44:42Z blowagie $
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

namespace iTextSharp.text.html.simpleparser {

    /**
     * We use a TableWrapper because PdfPTable is rather complex
     * to put on the HTMLWorker stack.
     * @author  psoares
     * @since 5.0.6 (renamed)
     * @deprecated since 5.5.2
     */
    [Obsolete]
    public class TableWrapper : IElement {
        /**
         * The styles that need to be applied to the table
         * @since 5.0.6 renamed from props
         */
        private IDictionary<String, String> styles = new Dictionary<String, String>();
        /**
         * Nested list containing the PdfPCell elements that are part of this table.
         */
        private IList<IList<PdfPCell>> rows = new List<IList<PdfPCell>>();
        
        /**
         * Array containing the widths of the columns.
         * @since iText 5.0.6
         */
        private float[] colWidths;

        /**
         * Creates a new instance of IncTable.
         * @param   attrs   a Map containing attributes
         */
        public TableWrapper(IDictionary<String, String> attrs) {
            foreach (KeyValuePair<string,string> kv in attrs) {
                styles[kv.Key] = kv.Value;
            }
        }

        /**
         * Adds a new row to the table.
         * @param row a list of PdfPCell elements
         */
        virtual public void AddRow(IList<PdfPCell> row) {
            if (row != null) {
                List<PdfPCell> t = new List<PdfPCell>(row);
                t.Reverse();
                rows.Add(t);
            }
        }

        /**
         * Setter for the column widths
         * @since iText 5.0.6
         */
        virtual public float[] ColWidths {
            set { colWidths = value; }
        }

        /**
         * Creates a new PdfPTable based on the info assembled
         * in the table stub.
         * @return  a PdfPTable
         */
        virtual public PdfPTable CreateTable() {
            // no rows = simplest table possible
            if (rows.Count == 0)
                return new PdfPTable(1);
            // how many columns?
            int ncol = 0;
            foreach (PdfPCell pc in rows[0]) {
                ncol += pc.Colspan;
            }
            PdfPTable table = new PdfPTable(ncol);
            // table width
            String width;
            styles.TryGetValue(HtmlTags.WIDTH, out width);
            if (width == null)
                table.WidthPercentage = 100;
            else {
                if (width.EndsWith("%"))
                    table.WidthPercentage = float.Parse(width.Substring(0, width.Length - 1), CultureInfo.InvariantCulture);
                else {
                    table.TotalWidth = float.Parse(width, CultureInfo.InvariantCulture);
                    table.LockedWidth = true;
                }
            }
            // horizontal alignment
            String alignment;
            styles.TryGetValue(HtmlTags.ALIGN, out alignment);
            int align = Element.ALIGN_LEFT;
            if (alignment != null) {
                align = HtmlUtilities.AlignmentValue(alignment);
            }
            table.HorizontalAlignment = align;
            // column widths
            try {
                if (colWidths != null)
                    table.SetWidths(colWidths);
            } catch {
                // fail silently
            }
            // add the cells
            foreach (IList<PdfPCell> col in rows) {
                foreach (PdfPCell pc in col) {
                    table.AddCell(pc);
                }
            }
            return table;
        }

        // these Element methods are irrelevant for a table stub.
        
        virtual public bool Process(IElementListener listener) {
            return false;
        }

        virtual public int Type {
            get { 
                return 0;
            }
        }

        virtual public bool IsContent() {
            return false;
        }

        virtual public bool IsNestable() {
            return false;
        }

        virtual public IList<Chunk> Chunks {
            get {
                return null;    
            }
        }
    }
}
