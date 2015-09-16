using System.Collections.Generic;
/*
 * $Id$
 * 
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

namespace iTextSharp.text.pdf.events {

    /**
    * If you want to add more than one page event to a PdfPTable,
    * you have to construct a PdfPTableEventForwarder, add the
    * different events to this object and add the forwarder to
    * the PdfWriter.
    */

    public class PdfPTableEventForwarder : IPdfPTableEventAfterSplit {

        /** ArrayList containing all the PageEvents that have to be executed. */
        protected List<IPdfPTableEvent> events = new List<IPdfPTableEvent>();
        
        /** 
        * Add a page event to the forwarder.
        * @param event an event that has to be added to the forwarder.
        */
        virtual public void AddTableEvent(IPdfPTableEvent eventa) {
            events.Add(eventa);
        }

        /**
        * @see com.lowagie.text.pdf.PdfPTableEvent#tableLayout(com.lowagie.text.pdf.PdfPTable, float[][], float[], int, int, com.lowagie.text.pdf.PdfContentByte[])
        */
        virtual public void TableLayout(PdfPTable table, float[][] widths, float[] heights, int headerRows, int rowStart, PdfContentByte[] canvases) {
            foreach (IPdfPTableEvent eventa in events) {
                eventa.TableLayout(table, widths, heights, headerRows, rowStart, canvases);
            }
        }
        virtual public void SplitTable(PdfPTable table) {
		    foreach (IPdfPTableEvent eventa in events) {
			    if (eventa is IPdfPTableEventSplit)
                    ((IPdfPTableEventSplit)eventa).SplitTable(table);
		    }
        }
        /**
         * @see com.itextpdf.text.pdf.PdfPTableEventAfterSplit#afterSplitTable(com.itextpdf.text.pdf.PdfPTable, com.itextpdf.text.pdf.PdfPRow, int)
         * @since iText 5.4.3
         */
        virtual public void AfterSplitTable(PdfPTable table, PdfPRow startRow, int startIdx) {
            foreach (IPdfPTableEvent evente in events) {
                if (evente is IPdfPTableEventAfterSplit)
                    ((IPdfPTableEventAfterSplit)evente).AfterSplitTable(table, startRow, startIdx);
            }
        }

    }
}
