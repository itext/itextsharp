using System;
using System.Collections;
using iTextSharp.text.error_messages;

using iTextSharp.text;
using iTextSharp.text.pdf.events;

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

namespace iTextSharp.text.pdf {
    /** This is a table that can be put at an absolute position but can also
    * be added to the document as the class <CODE>Table</CODE>.
    * In the last case when crossing pages the table always break at full rows; if a
    * row is bigger than the page it is dropped silently to avoid infinite loops.
    * <P>
    * A PdfPTableEvent can be associated to the table to do custom drawing
    * when the table is rendered.
    * @author Paulo Soares
    */

    public class PdfPTable : ILargeElement{
        
        /** The index of the original <CODE>PdfcontentByte</CODE>.
        */    
        public const int BASECANVAS = 0;
        /** The index of the duplicate <CODE>PdfContentByte</CODE> where the background will be drawn.
        */    
        public const int BACKGROUNDCANVAS = 1;
        /** The index of the duplicate <CODE>PdfContentByte</CODE> where the border lines will be drawn.
        */    
        public const int LINECANVAS = 2;
        /** The index of the duplicate <CODE>PdfContentByte</CODE> where the text will be drawn.
        */    
        public const int TEXTCANVAS = 3;
        
        protected ArrayList rows = new ArrayList();
        protected float totalHeight = 0;
        protected PdfPCell[] currentRow;
        protected int currentRowIdx = 0;
        protected PdfPCell defaultCell = new PdfPCell((Phrase)null);
        protected float totalWidth = 0;
        protected float[] relativeWidths;
        protected float[] absoluteWidths;
        protected IPdfPTableEvent tableEvent;
        
    /** Holds value of property headerRows. */
        protected int headerRows;
        
    /** Holds value of property widthPercentage. */
        protected float widthPercentage = 80;
        
    /** Holds value of property horizontalAlignment. */
        private int horizontalAlignment = Element.ALIGN_CENTER;
        
    /** Holds value of property skipFirstHeader. */
        private bool skipFirstHeader = false;

        /**
        * Holds value of property skipLastFooter.
        * @since    2.1.6
        */
        private bool skipLastFooter = false;

        protected bool isColspan = false;
        
        protected int runDirection = PdfWriter.RUN_DIRECTION_DEFAULT;

        /**
        * Holds value of property lockedWidth.
        */
        private bool lockedWidth = false;
        
        /**
        * Holds value of property splitRows.
        */
        private bool splitRows = true;
        
    /** The spacing before the table. */
        protected float spacingBefore;
        
    /** The spacing after the table. */
        protected float spacingAfter;
        
        /**
        * Holds value of property extendLastRow.
        */
        private bool[] extendLastRow = {false, false};
        
        /**
        * Holds value of property headersInEvent.
        */
        private bool headersInEvent;
        
        /**
        * Holds value of property splitLate.
        */
        private bool splitLate = true;
        
        /**
        * Defines if the table should be kept
        * on one page if possible
        */
        private bool keepTogether;

        /**
        * Indicates if the PdfPTable is complete once added to the document.
        * @since	iText 2.0.8
        */
        protected bool complete = true;
        
		private int footerRows;

        /**
        * Keeps track of the completeness of the current row.
        * @since    2.1.6
        */
        protected bool rowCompleted = true;

        protected PdfPTable() {
        }
        
        /** Constructs a <CODE>PdfPTable</CODE> with the relative column widths.
        * @param relativeWidths the relative column widths
        */    
        public PdfPTable(float[] relativeWidths) {
            if (relativeWidths == null)
                throw new ArgumentNullException(MessageLocalization.GetComposedMessage("the.widths.array.in.pdfptable.constructor.can.not.be.null"));
            if (relativeWidths.Length == 0)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.widths.array.in.pdfptable.constructor.can.not.have.zero.length"));
            this.relativeWidths = new float[relativeWidths.Length];
            Array.Copy(relativeWidths, 0, this.relativeWidths, 0, relativeWidths.Length);
            absoluteWidths = new float[relativeWidths.Length];
            CalculateWidths();
            currentRow = new PdfPCell[absoluteWidths.Length];
            keepTogether = false;
        }
        
        /** Constructs a <CODE>PdfPTable</CODE> with <CODE>numColumns</CODE> columns.
        * @param numColumns the number of columns
        */    
        public PdfPTable(int numColumns) {
            if (numColumns <= 0)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.number.of.columns.in.pdfptable.constructor.must.be.greater.than.zero"));
            relativeWidths = new float[numColumns];
            for (int k = 0; k < numColumns; ++k)
                relativeWidths[k] = 1;
            absoluteWidths = new float[relativeWidths.Length];
            CalculateWidths();
            currentRow = new PdfPCell[absoluteWidths.Length];
            keepTogether = false;
        }
        
        /** Constructs a copy of a <CODE>PdfPTable</CODE>.
        * @param table the <CODE>PdfPTable</CODE> to be copied
        */    
        public PdfPTable(PdfPTable table) {
            CopyFormat(table);
            for (int k = 0; k < currentRow.Length; ++k) {
                if (table.currentRow[k] == null)
                    break;
                currentRow[k] = new PdfPCell(table.currentRow[k]);
            }
            for (int k = 0; k < table.rows.Count; ++k) {
                PdfPRow row = (PdfPRow)(table.rows[k]);
                if (row != null)
                    row = new PdfPRow(row);
                rows.Add(row);
            }
        }
        
        /**
        * Makes a shallow copy of a table (format without content).
        * @param table
        * @return a shallow copy of the table
        */
        public static PdfPTable ShallowCopy(PdfPTable table) {
            PdfPTable nt = new PdfPTable();
            nt.CopyFormat(table);
            return nt;
        }

        /**
        * Copies the format of the sourceTable without copying the content. 
        * @param sourceTable
        */
        protected internal void CopyFormat(PdfPTable sourceTable) {
            relativeWidths = new float[sourceTable.NumberOfColumns];
            absoluteWidths = new float[sourceTable.NumberOfColumns];
            System.Array.Copy(sourceTable.relativeWidths, 0, relativeWidths, 0, NumberOfColumns);
            System.Array.Copy(sourceTable.absoluteWidths, 0, absoluteWidths, 0, NumberOfColumns);
            totalWidth = sourceTable.totalWidth;
            totalHeight = sourceTable.totalHeight;
            currentRowIdx = 0;
            tableEvent = sourceTable.tableEvent;
            runDirection = sourceTable.runDirection;
            defaultCell = new PdfPCell(sourceTable.defaultCell);
            currentRow = new PdfPCell[sourceTable.currentRow.Length];
            isColspan = sourceTable.isColspan;
            splitRows = sourceTable.splitRows;
            spacingAfter = sourceTable.spacingAfter;
            spacingBefore = sourceTable.spacingBefore;
            headerRows = sourceTable.headerRows;
			footerRows = sourceTable.footerRows;
            lockedWidth = sourceTable.lockedWidth;
            extendLastRow = sourceTable.extendLastRow;
            headersInEvent = sourceTable.headersInEvent;
            widthPercentage = sourceTable.widthPercentage;
            splitLate = sourceTable.splitLate;
            skipFirstHeader = sourceTable.skipFirstHeader;
            skipLastFooter = sourceTable.skipLastFooter;
            horizontalAlignment = sourceTable.horizontalAlignment;
            keepTogether = sourceTable.keepTogether;
            complete = sourceTable.complete;
        }

        /** Sets the relative widths of the table.
        * @param relativeWidths the relative widths of the table.
        * @throws DocumentException if the number of widths is different than the number
        * of columns
        */    
        public void SetWidths(float[] relativeWidths) {
            if (relativeWidths.Length != NumberOfColumns)
                throw new DocumentException(MessageLocalization.GetComposedMessage("wrong.number.of.columns"));
            this.relativeWidths = new float[relativeWidths.Length];
            Array.Copy(relativeWidths, 0, this.relativeWidths, 0, relativeWidths.Length);
            absoluteWidths = new float[relativeWidths.Length];
            totalHeight = 0;
            CalculateWidths();
            CalculateHeights(true);
        }

        /** Sets the relative widths of the table.
        * @param relativeWidths the relative widths of the table.
        * @throws DocumentException if the number of widths is different than the number
        * of columns
        */    
        public void SetWidths(int[] relativeWidths) {
            float[] tb = new float[relativeWidths.Length];
            for (int k = 0; k < relativeWidths.Length; ++k)
                tb[k] = relativeWidths[k];
            SetWidths(tb);
        }

        protected internal void CalculateWidths() {
            if (totalWidth <= 0)
                return;
            float total = 0;
            int numCols = NumberOfColumns;
            for (int k = 0; k < numCols; ++k)
                total += relativeWidths[k];
            for (int k = 0; k < numCols; ++k)
                absoluteWidths[k] = totalWidth * relativeWidths[k] / total;
        }
        
        /** Sets the full width of the table from the absolute column width.
        * @param columnWidth the absolute width of each column
        * @throws DocumentException if the number of widths is different than the number
        * of columns
        */    
        public void SetTotalWidth(float[] columnWidth) {
            if (columnWidth.Length != NumberOfColumns)
                throw new DocumentException(MessageLocalization.GetComposedMessage("wrong.number.of.columns"));
            totalWidth = 0;
            for (int k = 0; k < columnWidth.Length; ++k)
                totalWidth += columnWidth[k];
            SetWidths(columnWidth);
        }

        /** Sets the percentage width of the table from the absolute column width.
        * @param columnWidth the absolute width of each column
        * @param pageSize the page size
        * @throws DocumentException
        */    
        public void SetWidthPercentage(float[] columnWidth, Rectangle pageSize) {
            if (columnWidth.Length != NumberOfColumns)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("wrong.number.of.columns"));
            float totalWidth = 0;
            for (int k = 0; k < columnWidth.Length; ++k)
                totalWidth += columnWidth[k];
            widthPercentage = totalWidth / (pageSize.Right - pageSize.Left) * 100f;
            SetWidths(columnWidth);
        }

        /** Gets the full width of the table.
        * @return the full width of the table
        */    
        public float TotalWidth {
            get {
                return totalWidth;
            }
            set {
                if (this.totalWidth == value)
                    return;
                this.totalWidth = value;
                totalHeight = 0;
                CalculateWidths();
                CalculateHeights(true);
            }
        }

        /**
        * Calculates the heights of the table.
        * 
        * @param    firsttime   if true, the heights of the rows will be recalculated.
        * This takes time; normally the heights of the rows are already calcultated,
        * so in most cases, it's save to use false as parameter.
        * @return   the total height of the table. Note that it will be 0 if you didn't
        * specify the width of the table with SetTotalWidth().
        * @since    2.1.5   added a parameter and a return type to an existing method,
        * and made it public
        */
        public float CalculateHeights(bool firsttime) { 
            if (totalWidth <= 0)
                return 0;
            totalHeight = 0;
            for (int k = 0; k < rows.Count; ++k) {
                totalHeight += GetRowHeight(k, firsttime);
            }
            return totalHeight;
        }
        
        /**
        * Calculates the heights of the table.
        */
        public void CalculateHeightsFast() {
            CalculateHeights(false);
        }
        
        /** Gets the default <CODE>PdfPCell</CODE> that will be used as
        * reference for all the <CODE>addCell</CODE> methods except
        * <CODE>addCell(PdfPCell)</CODE>.
        * @return default <CODE>PdfPCell</CODE>
        */    
        public PdfPCell DefaultCell {
            get {
                return defaultCell;
            }
        }
        
        /**
        * Adds a cell element.
        * 
        * @param cell the cell element
        */    
        public void AddCell(PdfPCell cell) {
            rowCompleted = false;
            PdfPCell ncell = new PdfPCell(cell);
            
            int colspan = ncell.Colspan;
            colspan = Math.Max(colspan, 1);
            colspan = Math.Min(colspan, currentRow.Length - currentRowIdx);
            ncell.Colspan = colspan;

            if (colspan != 1)
                isColspan = true;
            int rdir = ncell.RunDirection;
            if (rdir == PdfWriter.RUN_DIRECTION_DEFAULT)
                ncell.RunDirection = runDirection;
            
            SkipColsWithRowspanAbove();
            
            bool cellAdded = false;
            if (currentRowIdx < currentRow.Length) {  
                currentRow[currentRowIdx] = ncell;
                currentRowIdx += colspan;
                cellAdded = true;
            }

            SkipColsWithRowspanAbove();
            
            while (currentRowIdx >= currentRow.Length) {
                int numCols = NumberOfColumns;
                if (runDirection == PdfWriter.RUN_DIRECTION_RTL) {
                    PdfPCell[] rtlRow = new PdfPCell[numCols];
                    int rev = currentRow.Length;
                    for (int k = 0; k < currentRow.Length; ++k) {
                        PdfPCell rcell = currentRow[k];
                        int cspan = rcell.Colspan;
                        rev -= cspan;
                        rtlRow[rev] = rcell;
                        k += cspan - 1;
                    }
                    currentRow = rtlRow;
                }
                PdfPRow row = new PdfPRow(currentRow);
                if (totalWidth > 0) {
                    row.SetWidths(absoluteWidths);
                    totalHeight += row.MaxHeights;
                }
                rows.Add(row);
                currentRow = new PdfPCell[numCols];
                currentRowIdx = 0;
                SkipColsWithRowspanAbove();
                rowCompleted = true;
            }
            
            if (!cellAdded) {
                currentRow[currentRowIdx] = ncell;
                currentRowIdx += colspan;
            }
        }
        
        /**
        * When updating the row index, cells with rowspan should be taken into account.
        * This is what happens in this method.
        * @since    2.1.6
        */
        private void SkipColsWithRowspanAbove() {
            int direction = 1;
            if (runDirection == PdfWriter.RUN_DIRECTION_RTL)
                direction = -1;
            while (RowSpanAbove(rows.Count, currentRowIdx))
                currentRowIdx += direction;
        }
        
        /**
        * Checks if there are rows above belonging to a rowspan.
        * @param    currRow the current row to check
        * @param    currCol the current column to check
        * @return   true if there's a cell above that belongs to a rowspan
        * @since    2.1.6
        */
        internal bool RowSpanAbove(int currRow, int currCol) {
            
            if ((currCol >= NumberOfColumns) 
                    || (currCol < 0) 
                    || (currRow == 0))
                return false;
            
            int row = currRow - 1;
            PdfPRow aboveRow = (PdfPRow)rows[row];
            if (aboveRow == null)
                return false;
            PdfPCell aboveCell = (PdfPCell)aboveRow.GetCells()[currCol];
            while ((aboveCell == null) && (row > 0)) {
                aboveRow  = (PdfPRow)rows[--row];
                if (aboveRow == null)
                    return false;
                aboveCell = (PdfPCell)aboveRow.GetCells()[currCol];
            }
            
            int distance = currRow - row;

            if (aboveCell == null) {
                int col = currCol - 1;
                aboveCell = (PdfPCell)aboveRow.GetCells()[col];
                while ((aboveCell == null) && (row > 0))
                    aboveCell = (PdfPCell)aboveRow.GetCells()[--col];
                return aboveCell != null && aboveCell.Rowspan > distance;
            }
            
            if ((aboveCell.Rowspan == 1) && (distance > 1)) {
                int col = currCol - 1;
                aboveRow = (PdfPRow)rows[row + 1];
                distance--;
                aboveCell = (PdfPCell)aboveRow.GetCells()[col];
                while ((aboveCell == null) && (col > 0))
                    aboveCell = (PdfPCell)aboveRow.GetCells()[--col];
            }
            
            return aboveCell != null && aboveCell.Rowspan > distance;
        }
        
        /** Adds a cell element.
        * @param text the text for the cell
        */    
        public void AddCell(String text) {
            AddCell(new Phrase(text));
        }
        
        /**
        * Adds a nested table.
        * @param table the table to be added to the cell
        */    
        public void AddCell(PdfPTable table) {
            defaultCell.Table = table;
            AddCell(defaultCell);
            defaultCell.Table = null;
        }
        
        /**
        * Adds an Image as Cell.
        * @param image the <CODE>Image</CODE> to add to the table. This image will fit in the cell
        */    
        public void AddCell(Image image) {
            defaultCell.Image = image;
            AddCell(defaultCell);
            defaultCell.Image = null;
        }
        
        /**
        * Adds a cell element.
        * @param phrase the <CODE>Phrase</CODE> to be added to the cell
        */    
        public void AddCell(Phrase phrase) {
            defaultCell.Phrase = phrase;
            AddCell(defaultCell);
            defaultCell.Phrase = null;
        }
        
        /**
        * Writes the selected rows to the document.
        * <P>
        * <CODE>canvases</CODE> is obtained from <CODE>beginWritingRows()</CODE>.
        * @param rowStart the first row to be written, zero index
        * @param rowEnd the last row to be written + 1. If it is -1 all the
        * rows to the end are written
        * @param xPos the x write coodinate
        * @param yPos the y write coodinate
        * @param canvases an array of 4 <CODE>PdfContentByte</CODE> obtained from
        * <CODE>beginWrittingRows()</CODE>
        * @return the y coordinate position of the bottom of the last row
        * @see #beginWritingRows(com.lowagie.text.pdf.PdfContentByte)
        */    
        public float WriteSelectedRows(int rowStart, int rowEnd, float xPos, float yPos, PdfContentByte[] canvases) {
            return WriteSelectedRows(0, -1, rowStart, rowEnd, xPos, yPos, canvases);
        }
        
        /** Writes the selected rows and columns to the document.
        * This method does not clip the columns; this is only important
        * if there are columns with colspan at boundaries.
        * <P>
        * <CODE>canvases</CODE> is obtained from <CODE>beginWritingRows()</CODE>.
        * <P>
        * The table event is only fired for complete rows.
        * @param colStart the first column to be written, zero index
        * @param colEnd the last column to be written + 1. If it is -1 all the
        * columns to the end are written
        * @param rowStart the first row to be written, zero index
        * @param rowEnd the last row to be written + 1. If it is -1 all the
        * rows to the end are written
        * @param xPos the x write coodinate
        * @param yPos the y write coodinate
        * @param canvases an array of 4 <CODE>PdfContentByte</CODE> obtained from
        * <CODE>beginWrittingRows()</CODE>
        * @return the y coordinate position of the bottom of the last row
        * @see #beginWritingRows(com.lowagie.text.pdf.PdfContentByte)
        */    
        public float WriteSelectedRows(int colStart, int colEnd, int rowStart, int rowEnd, float xPos, float yPos, PdfContentByte[] canvases) {
            if (totalWidth <= 0)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.table.width.must.be.greater.than.zero"));
            int totalRows = rows.Count;
            if (rowStart < 0)
                rowStart = 0;
            if (rowEnd < 0)
                rowEnd = totalRows;
            else
                rowEnd = Math.Min(rowEnd, totalRows);
            if (rowStart >= rowEnd)
                return yPos;
            
            int totalCols = NumberOfColumns;
            if (colStart < 0)
                colStart = 0;
            else
                colStart = Math.Min(colStart, totalCols);
            if (colEnd < 0)
                colEnd = totalCols;
            else
                colEnd = Math.Min(colEnd, totalCols);
            float yPosStart = yPos;
            for (int k = rowStart; k < rowEnd; ++k) {
                PdfPRow row = (PdfPRow)rows[k];
                if (row != null) {
                    row.WriteCells(colStart, colEnd, xPos, yPos, canvases);
                    yPos -= row.MaxHeights;
                }
            }
            if (tableEvent != null && colStart == 0 && colEnd == totalCols) {
                float[] heights = new float[rowEnd - rowStart + 1];
                heights[0] = yPosStart;
                for (int k = rowStart; k < rowEnd; ++k) {
                    PdfPRow row = (PdfPRow)rows[k];
                    float hr = 0;
                    if (row != null)
                        hr = row.MaxHeights;
                    heights[k - rowStart + 1] = heights[k - rowStart] - hr;
                }
                tableEvent.TableLayout(this, GetEventWidths(xPos, rowStart, rowEnd, headersInEvent), heights, headersInEvent ? headerRows : 0, rowStart, canvases);
            }
            return yPos;
        }
        
        /**
        * Writes the selected rows to the document.
        * 
        * @param rowStart the first row to be written, zero index
        * @param rowEnd the last row to be written + 1. If it is -1 all the
        * rows to the end are written
        * @param xPos the x write coodinate
        * @param yPos the y write coodinate
        * @param canvas the <CODE>PdfContentByte</CODE> where the rows will
        * be written to
        * @return the y coordinate position of the bottom of the last row
        */    
        public float WriteSelectedRows(int rowStart, int rowEnd, float xPos, float yPos, PdfContentByte canvas) {
            return WriteSelectedRows(0, -1, rowStart, rowEnd, xPos, yPos, canvas);
        }
        
        /**
        * Writes the selected rows to the document.
        * This method clips the columns; this is only important
        * if there are columns with colspan at boundaries.
        * <P>
        * The table event is only fired for complete rows.
        * 
        * @param colStart the first column to be written, zero index
        * @param colEnd the last column to be written + 1. If it is -1 all the
        * @param rowStart the first row to be written, zero index
        * @param rowEnd the last row to be written + 1. If it is -1 all the
        * rows to the end are written
        * @param xPos the x write coodinate
        * @param yPos the y write coodinate
        * @param canvas the <CODE>PdfContentByte</CODE> where the rows will
        * be written to
        * @return the y coordinate position of the bottom of the last row
        */    
        public float WriteSelectedRows(int colStart, int colEnd, int rowStart, int rowEnd, float xPos, float yPos, PdfContentByte canvas) {
            int totalCols = NumberOfColumns;
            if (colStart < 0)
                colStart = 0;
            else
        	    colStart = Math.Min(colStart, totalCols);
            
    	    if (colEnd < 0)
                colEnd = totalCols;
    	    else
    		    colEnd = Math.Min(colEnd, totalCols);
        	
    	    bool clip = (colStart != 0 || colEnd != totalCols);
        	
            if (clip) {
                float w = 0;
                for (int k = colStart; k < colEnd; ++k)
                    w += absoluteWidths[k];
                canvas.SaveState();
                float lx = (colStart == 0) ? 10000 : 0;
                float rx = (colEnd == totalCols) ? 10000 : 0;
                canvas.Rectangle(xPos - lx, -10000, w + lx + rx, PdfPRow.RIGHT_LIMIT);
                canvas.Clip();
                canvas.NewPath();
            }
            
            PdfContentByte[] canvases = BeginWritingRows(canvas);
            float y = WriteSelectedRows(colStart, colEnd, rowStart, rowEnd, xPos, yPos, canvases);
            EndWritingRows(canvases);
            
            if (clip)
                canvas.RestoreState();
            
            return y;
        }
        
        /** Gets and initializes the 4 layers where the table is written to. The text or graphics are added to
        * one of the 4 <CODE>PdfContentByte</CODE> returned with the following order:<p>
        * <ul>
        * <li><CODE>PdfPtable.BASECANVAS</CODE> - the original <CODE>PdfContentByte</CODE>. Anything placed here
        * will be under the table.
        * <li><CODE>PdfPtable.BACKGROUNDCANVAS</CODE> - the layer where the background goes to.
        * <li><CODE>PdfPtable.LINECANVAS</CODE> - the layer where the lines go to.
        * <li><CODE>PdfPtable.TEXTCANVAS</CODE> - the layer where the text go to. Anything placed here
        * will be over the table.
        * </ul><p>
        * The layers are placed in sequence on top of each other.
        * @param canvas the <CODE>PdfContentByte</CODE> where the rows will
        * be written to
        * @return an array of 4 <CODE>PdfContentByte</CODE>
        * @see #writeSelectedRows(int, int, float, float, PdfContentByte[])
        */    
        public static PdfContentByte[] BeginWritingRows(PdfContentByte canvas) {
            return new PdfContentByte[]{
                canvas,
                canvas.Duplicate,
                canvas.Duplicate,
                canvas.Duplicate,
            };
        }
        
        /** Finishes writing the table.
        * @param canvases the array returned by <CODE>beginWritingRows()</CODE>
        */    
        public static void EndWritingRows(PdfContentByte[] canvases) {
            PdfContentByte canvas = canvases[BASECANVAS];
            canvas.SaveState();
            canvas.Add(canvases[BACKGROUNDCANVAS]);
            canvas.RestoreState();
            canvas.SaveState();
            canvas.SetLineCap(2);
            canvas.ResetRGBColorStroke();
            canvas.Add(canvases[LINECANVAS]);
            canvas.RestoreState();
            canvas.Add(canvases[TEXTCANVAS]);
        }
        
        /** Gets the number of rows in this table.
        * @return the number of rows in this table
        */    
        public int Size {
            get {
                return rows.Count;
            }
        }
        
        /** Gets the total height of the table.
        * @return the total height of the table
        */    
        public float TotalHeight {
            get {
                return totalHeight;
            }
        }
        
        /** Gets the height of a particular row.
        * @param idx the row index (starts at 0)
        * @return the height of a particular row
        */    
        public float GetRowHeight(int idx) {
            return GetRowHeight(idx, false);
        }
        /**
        * Gets the height of a particular row.
        * 
        * @param idx the row index (starts at 0)
        * @param firsttime  is this the first time the row heigh is calculated?
        * @return the height of a particular row
        * @since    3.0.0
        */    
        public float GetRowHeight(int idx, bool firsttime) {
            if (totalWidth <= 0 || idx < 0 || idx >= rows.Count)
                return 0;
            PdfPRow row = (PdfPRow)rows[idx];
            if (row == null)
                return 0;
            if (firsttime)
                row.SetWidths(absoluteWidths);
            float height = row.MaxHeights;
            PdfPCell cell;
            PdfPRow tmprow;
            for (int i = 0; i < relativeWidths.Length; i++) {
                if(!RowSpanAbove(idx, i))
                    continue;
                int rs = 1;
                while (RowSpanAbove(idx - rs, i)) {
                    rs++;
                }
                tmprow = (PdfPRow)rows[idx - rs];
                cell = tmprow.GetCells()[i];
                float tmp = 0;
                if (cell != null && cell.Rowspan == rs + 1) {
                    tmp = cell.GetMaxHeight();
                    while (rs > 0) {
                        tmp -= GetRowHeight(idx - rs);
                        rs--;
                    }
                }
                if (tmp > height)
                    height = tmp;
            }
            row.MaxHeights = height;
            return height;
        }
        
        /**
        * Gets the maximum height of a cell in a particular row (will only be different
        * from getRowHeight is one of the cells in the row has a rowspan > 1).
        * 
        * @param    rowIndex    the row index
        * @param    cellIndex   the cell index
        * @return the height of a particular row including rowspan
        * @since    2.1.6
        */    
        public float GetRowspanHeight(int rowIndex, int cellIndex) {
            if (totalWidth <= 0 || rowIndex < 0 || rowIndex >= rows.Count)
                return 0;
            PdfPRow row = (PdfPRow)rows[rowIndex];
            if (row == null || cellIndex >= row.GetCells().Length)
                return 0;
            PdfPCell cell = row.GetCells()[cellIndex];
            if (cell == null)
                return 0;
            float rowspanHeight = 0;
            for (int j = 0; j < cell.Rowspan; j++) {
                rowspanHeight += GetRowHeight(rowIndex + j);
            }
            return rowspanHeight;
        }

        /** Gets the height of the rows that constitute the header as defined by
        * <CODE>setHeaderRows()</CODE>.
        * @return the height of the rows that constitute the header and footer
        */    
        public float HeaderHeight {
            get {
                float total = 0;
                int size = Math.Min(rows.Count, headerRows);
                for (int k = 0; k < size; ++k) {
                    PdfPRow row = (PdfPRow)rows[k];
                    if (row != null)
                        total += row.MaxHeights;
                }
                return total;
            }
        }
        
        /** Gets the height of the rows that constitute the header as defined by
        * <CODE>setFooterRows()</CODE>.
        * @return the height of the rows that constitute the footer
        * @since 2.1.1
        */    
        public float FooterHeight {
            get {
                float total = 0;
                int start = Math.Max(0, headerRows - footerRows);
                int size = Math.Min(rows.Count, headerRows);
                for (int k = start; k < size; ++k) {
                    PdfPRow row = (PdfPRow)rows[k];
                    if (row != null)
                        total += row.MaxHeights;
                }
                return total;
            }
        }
        
        /** Deletes a row from the table.
        * @param rowNumber the row to be deleted
        * @return <CODE>true</CODE> if the row was deleted
        */    
        public bool DeleteRow(int rowNumber) {
            if (rowNumber < 0 || rowNumber >= rows.Count) {
                return false;
            }
            if (totalWidth > 0) {
                PdfPRow row = (PdfPRow)rows[rowNumber];
                if (row != null)
                    totalHeight -= row.MaxHeights;
            }
            rows.RemoveAt(rowNumber);
            if (rowNumber < headerRows) {
                --headerRows;
                if (rowNumber >= (headerRows - footerRows))
                    --footerRows;
            }
            return true;
        }
        
        /** Deletes the last row in the table.
        * @return <CODE>true</CODE> if the last row was deleted
        */    
        public bool DeleteLastRow() {
            return DeleteRow(rows.Count - 1);
        }
        
        /**
        * Removes all of the rows except headers
        */
        public void DeleteBodyRows() {
            ArrayList rows2 = new ArrayList();
            for (int k = 0; k < headerRows; ++k)
                rows2.Add(rows[k]);
            rows = rows2;
            totalHeight = 0;
            if (totalWidth > 0)
                totalHeight = HeaderHeight;
        }

        /** Returns the number of columns.
        * @return  the number of columns.
        * @since   2.1.1
        */
        public int NumberOfColumns {
            get {
                return relativeWidths.Length;
            }
        }
        public int HeaderRows {
            get {
                return headerRows;
            }
            set {
                headerRows = value;
                if (headerRows < 0)
                    headerRows = 0;
            }
        }
        
        public int FooterRows {
            get {
                return footerRows;
            }
            set {
                footerRows = value;
                if (footerRows < 0)
                    footerRows = 0;
            }
        }
        
		/**
        * Gets all the chunks in this element.
        *
        * @return    an <CODE>ArrayList</CODE>
        */
        public ArrayList Chunks {
            get {
                return new ArrayList();
            }
        }
        
        /**
        * Gets the type of the text element.
        *
        * @return    a type
        */
        public int Type {
            get {
                return Element.PTABLE;
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
  
        /**
        * Processes the element by adding it (or the different parts) to an
        * <CODE>ElementListener</CODE>.
        *
        * @param    listener    an <CODE>ElementListener</CODE>
        * @return    <CODE>true</CODE> if the element was processed successfully
        */
        public bool Process(IElementListener listener) {
            try {
                return listener.Add(this);
            }
            catch (DocumentException) {
                return false;
            }
        }
                        
        public float WidthPercentage {
            get {
                return widthPercentage;
            }
            set {
                widthPercentage = value;
            }
        }

        public int HorizontalAlignment {
            get {
                return horizontalAlignment;
            }
            set {
                horizontalAlignment = value;
            }
        }

        /**
        * Gets a row with a given index
        * (added by Jin-Hsia Yang).
        * @param idx
        * @return the row at position idx
        */
        public PdfPRow GetRow(int idx) {
            return (PdfPRow)rows[idx];
        }

        /**
        * Gets an arraylist with all the rows in the table.
        * @return an arraylist
        */
        public ArrayList Rows {
            get {
                return rows;
            }
        }

        /**
        * Gets an arraylist with a selection of rows.
        * @param    start   the first row in the selection
        * @param    end     the first row that isn't part of the selection
        * @return   a selection of rows
        * @since    2.1.6
        */
        public ArrayList GetRows(int start, int end) {
            ArrayList list = new ArrayList();
            if (start < 0 || end > Size) {
                return list;
            }
            PdfPRow firstRow = AdjustCellsInRow(start, end);
            int colIndex = 0;
            PdfPCell cell;
            while (colIndex < NumberOfColumns) {
                int rowIndex = start;
                while (RowSpanAbove(rowIndex--, colIndex)) {
                    PdfPRow row = GetRow(rowIndex);
                    if (row != null) {
                        PdfPCell replaceCell = row.GetCells()[colIndex];
                        if (replaceCell != null) {
                            firstRow.GetCells()[colIndex] = new PdfPCell(replaceCell);
                            float extra = 0;
                            int stop = Math.Min(rowIndex + replaceCell.Rowspan, end);
                            for (int j = start + 1; j < stop; j++) {
                                extra += GetRowHeight(j);
                            }
                            firstRow.SetExtraHeight(colIndex, extra);
                            float diff = GetRowspanHeight(rowIndex, colIndex)
                                - GetRowHeight(start) - extra;
                            firstRow.GetCells()[colIndex].ConsumeHeight(diff);
                        }
                    }
                }
                cell = firstRow.GetCells()[colIndex];
                if (cell == null)
                    colIndex++;
                else
                    colIndex += cell.Colspan;
            }
            list.Add(firstRow);
            for (int i = start + 1; i < end; i++) {
                list.Add(AdjustCellsInRow(i, end));
            }
            return list;
        }
        
        /**
        * Calculates the extra height needed in a row because of rowspans.
        * @param    start   the index of the start row (the one to adjust)
        * @param    end     the index of the end row on the page
        * @since    2.1.6
        */
        protected PdfPRow AdjustCellsInRow(int start, int end) {
            PdfPRow row = new PdfPRow(GetRow(start));
            row.InitExtraHeights();
            PdfPCell cell;
            PdfPCell[] cells = row.GetCells();
            for (int i = 0; i < cells.Length; i++) {
                cell = cells[i];
                if (cell == null || cell.Rowspan == 1)
                    continue;
                int stop = Math.Min(end, start + cell.Rowspan);
                float extra = 0;
                for (int k = start + 1; k < stop; k++) {
                    extra += GetRowHeight(k);
                }
                row.SetExtraHeight(i, extra);
            }
            return row;
        }

        public IPdfPTableEvent TableEvent {
            get {
                return tableEvent;
            }
            set {
                if (value == null) this.tableEvent = null;
                else if (this.tableEvent == null) this.tableEvent = value;
                else if (this.tableEvent is PdfPTableEventForwarder) ((PdfPTableEventForwarder)this.tableEvent).AddTableEvent(value);
                else {
                    PdfPTableEventForwarder forward = new PdfPTableEventForwarder();
                    forward.AddTableEvent(this.tableEvent);
                    forward.AddTableEvent(value);
                    this.tableEvent = forward;
                }
            }
        }

        /** Gets the absolute sizes of each column width.
        * @return he absolute sizes of each column width
        */    
        public float[] AbsoluteWidths {
            get {
                return absoluteWidths;
            }
        }
        
        internal float [][] GetEventWidths(float xPos, int firstRow, int lastRow, bool includeHeaders) {
            if (includeHeaders) {
                firstRow = Math.Max(firstRow, headerRows);
                lastRow = Math.Max(lastRow, headerRows);
            }
            float[][] widths = new float[(includeHeaders ? headerRows : 0) + lastRow - firstRow][];
            if (isColspan) {
                int n = 0;
                if (includeHeaders) {
                    for (int k = 0; k < headerRows; ++k) {
                        PdfPRow row = (PdfPRow)rows[k];
                        if (row == null)
                            ++n;
                        else
                            widths[n++] = row.GetEventWidth(xPos);
                    }
                }
                for (; firstRow < lastRow; ++firstRow) {
                        PdfPRow row = (PdfPRow)rows[firstRow];
                        if (row == null)
                            ++n;
                        else
                            widths[n++] = row.GetEventWidth(xPos);
                }
            }
            else {
                int numCols = NumberOfColumns;
                float[] width = new float[numCols + 1];
                width[0] = xPos;
                for (int k = 0; k < numCols; ++k)
                    width[k + 1] = width[k] + absoluteWidths[k];
                for (int k = 0; k < widths.Length; ++k)
                    widths[k] = width;
            }
            return widths;
        }

        public bool SkipFirstHeader {
            get {
                return skipFirstHeader;
            }
            set {
                skipFirstHeader = value;
            }
        }

        /**
        * Tells you if the last footer needs to be skipped
        * (for instance if the footer says "continued on the next page")
        * 
        * @return Value of property skipLastFooter.
        * @since   2.1.6
        */
        public bool SkipLastFooter {
            get {
                return skipLastFooter;
            }
            set {
                skipLastFooter = value;
            }
        }
        
        public int RunDirection {
            get {
                return runDirection;
            }
            set {
                switch (value) {
                    case PdfWriter.RUN_DIRECTION_DEFAULT:
                    case PdfWriter.RUN_DIRECTION_NO_BIDI:
                    case PdfWriter.RUN_DIRECTION_LTR:
                    case PdfWriter.RUN_DIRECTION_RTL:
                        this.runDirection = value;
                        break;
                    default:
                        throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.run.direction.1", runDirection));
                }
            }
        }
        
        public bool LockedWidth {
            get {
                return lockedWidth;
            }
            set {
                lockedWidth = value;
            }
        }
                
        public bool SplitRows {
            get {
                return splitRows;
            }
            set {
                splitRows = value;
            }
        }

        public float SpacingBefore {
            get {
                return spacingBefore;
            }
            set {
                spacingBefore = value;
            }
        }
        
        public float SpacingAfter {
            get {
                return spacingAfter;
            }
            set {
                spacingAfter = value;
            }
        }

        public bool ExtendLastRow {
            get {
                return extendLastRow[0];
            }
            set {
                extendLastRow[0] = value;
                extendLastRow[1] = value;
            }
        }
        
        /**
        * When set the last row on every page will be extended to fill
        * all the remaining space to the bottom boundary; except maybe the
        * final row.
        * 
        * @param extendLastRows true to extend the last row on each page; false otherwise
        * @param extendFinalRow false if you don't want to extend the final row of the complete table
        * @since iText 5.0.0
        */
        public void SetExtendLastRow(bool extendLastRows, bool extendFinalRow) {
            extendLastRow[0] = extendLastRows;
            extendLastRow[1] = extendFinalRow;
        }
        
        /**
        * Gets the value of the last row extension, taking into account
        * if the final row is reached or not.
        * 
        * @return true if the last row will extend; false otherwise
        * @since iText 5.0.0
        */
        public bool IsExtendLastRow(bool newPageFollows) {
            if (newPageFollows) {
                return extendLastRow[0];    
            }
            return extendLastRow[1];
        }

        public bool HeadersInEvent {
            get {
                return headersInEvent;
            }
            set {
                headersInEvent = value;
            }
        }
        public bool SplitLate {
            get {
                return splitLate;
            }
            set {
                splitLate = value;
            }
        }

        /**
        * If true the table will be kept on one page if it fits, by forcing a 
        * new page if it doesn't fit on the current page. The default is to
        * split the table over multiple pages.
        *
        * @param p_KeepTogether whether to try to keep the table on one page
        */
        public bool KeepTogether {
            set {
                keepTogether = value;
            }
            get {
                return keepTogether;
            }
        }

        /**
        * Completes the current row with the default cell. An incomplete row will be dropped
        * but calling this method will make sure that it will be present in the table.
        */
        public void CompleteRow() {
            while (!rowCompleted) {
                AddCell(defaultCell);
            }
        }

        /**
        * @since   iText 2.0.8
        * @see com.lowagie.text.LargeElement#flushContent()
        */
        public void FlushContent() {
            DeleteBodyRows();
            SkipFirstHeader = true;
        }

        /**
        * @since   iText 2.0.8
        * @see com.lowagie.text.LargeElement#isComplete()
        */
        public bool ElementComplete {
            get {
                return complete;
            }
            set {
                complete = value;
            }
        }
    }
}
