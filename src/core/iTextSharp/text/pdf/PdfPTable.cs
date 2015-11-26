using System;
using System.Collections.Generic;
using iTextSharp.text.error_messages;
using iTextSharp.text.api;
using iTextSharp.text.log;
using iTextSharp.text.pdf.events;
using iTextSharp.text.pdf.interfaces;

/*
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

namespace iTextSharp.text.pdf {
    /** This is a table that can be put at an absolute position but can also
    * be added to the document as the class <CODE>Table</CODE>.
    * In the last case when crossing pages the table always break at full rows; if a
    * row is bigger than the page it is dropped silently to avoid infinite loops.
    * <p/>
    * A PdfPTableEvent can be associated to the table to do custom drawing
    * when the table is rendered.
    * @author Paulo Soares
    */

    public class PdfPTable : ILargeElement, ISpaceable, IAccessibleElement
    {

        private readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof (PdfPTable));

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

        protected List<PdfPRow> rows = new List<PdfPRow>();
        protected float totalHeight = 0;
        protected PdfPCell[] currentRow;
        /**
         * The current column index.
         * 
         * @since 5.1.0 renamed from currentColIdx
         */
        protected int currentColIdx = 0;
        protected PdfPCell defaultCell = new PdfPCell((Phrase) null);
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
        * 
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

        protected float paddingTop;

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
        * @since iText 2.0.8
        */
        protected bool complete = true;

        virtual public bool Complete {
            get { return complete; }
            set { complete = value; }
        }

        private int footerRows;

        /**
        * Keeps track of the completeness of the current row.
        * 
        * @since    2.1.6
        */
        protected bool rowCompleted = true;

        protected bool loopCheck = true;
        protected bool rowsNotChecked = true;

        protected PdfName role = PdfName.TABLE;
        protected Dictionary<PdfName, PdfObject> accessibleAttributes = null;
        protected AccessibleElementId id = new AccessibleElementId();
        private PdfPTableHeader header = null;
        private PdfPTableBody body = null;
        private PdfPTableFooter footer = null;

        private int numberOfWrittenRows;

        protected PdfPTable()
        {
        }

        /** Constructs a <CODE>PdfPTable</CODE> with the relative column widths.
        * @param relativeWidths the relative column widths
        */

        public PdfPTable(float[] relativeWidths)
        {
            if (relativeWidths == null)
                throw new ArgumentNullException(
                    MessageLocalization.GetComposedMessage("the.widths.array.in.pdfptable.constructor.can.not.be.null"));
            if (relativeWidths.Length == 0)
                throw new ArgumentException(
                    MessageLocalization.GetComposedMessage(
                        "the.widths.array.in.pdfptable.constructor.can.not.have.zero.length"));
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

        public PdfPTable(int numColumns)
        {
            if (numColumns <= 0)
                throw new ArgumentException(
                    MessageLocalization.GetComposedMessage(
                        "the.number.of.columns.in.pdfptable.constructor.must.be.greater.than.zero"));
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

        public PdfPTable(PdfPTable table)
        {
            CopyFormat(table);
            for (int k = 0; k < currentRow.Length; ++k)
            {
                if (table.currentRow[k] == null)
                    break;
                currentRow[k] = new PdfPCell(table.currentRow[k]);
            }
            for (int k = 0; k < table.rows.Count; ++k) {
                PdfPRow row = table.rows[k];

                if (row != null) {
                    row = new PdfPRow(row);
                }

                rows.Add(row);
            }
        }

        public virtual void Init() {
            LOGGER.Info("Initialize row and cell heights");

            foreach (PdfPRow row in Rows) {
                if (row == null) {
                    continue;
                }

                row.calculated = false;

                foreach (PdfPCell cell in row.GetCells()) {
                    if (cell == null) {
                        continue;
                    }

                    cell.CalculatedHeight = 0;
                }
            }
        }

        /**
        * Makes a shallow copy of a table (format without content).
        * @param table
        * @return a shallow copy of the table
        */

        public static PdfPTable ShallowCopy(PdfPTable table)
        {
            PdfPTable nt = new PdfPTable();
            nt.CopyFormat(table);
            return nt;
        }

        /**
        * Copies the format of the sourceTable without copying the content. 
        * @param sourceTable
        * @since 2.1.6 private is now protected
        */
        protected internal virtual void CopyFormat(PdfPTable sourceTable) {
            rowsNotChecked = sourceTable.rowsNotChecked;
            relativeWidths = new float[sourceTable.NumberOfColumns];
            absoluteWidths = new float[sourceTable.NumberOfColumns];
            System.Array.Copy(sourceTable.relativeWidths, 0, relativeWidths, 0, NumberOfColumns);
            System.Array.Copy(sourceTable.absoluteWidths, 0, absoluteWidths, 0, NumberOfColumns);
            totalWidth = sourceTable.totalWidth;
            totalHeight = sourceTable.totalHeight;
            currentColIdx = 0;
            tableEvent = sourceTable.tableEvent;
            runDirection = sourceTable.runDirection;
            if (sourceTable.defaultCell is PdfPHeaderCell)
                defaultCell = new PdfPHeaderCell((PdfPHeaderCell) sourceTable.defaultCell);
            else
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
            loopCheck = sourceTable.loopCheck;
            id = sourceTable.ID;
            role = sourceTable.Role;
            if (sourceTable.accessibleAttributes != null)
                accessibleAttributes = new Dictionary<PdfName, PdfObject>(sourceTable.accessibleAttributes);
            header = sourceTable.GetHeader();
            body = sourceTable.GetBody();
            footer = sourceTable.GetFooter();
        }

        /** Sets the relative widths of the table.
        * @param relativeWidths the relative widths of the table.
        * @throws DocumentException if the number of widths is different than the number
        *                           of columns
        */

        virtual public void SetWidths(float[] relativeWidths)
        {
            if (relativeWidths.Length != NumberOfColumns)
                throw new DocumentException(MessageLocalization.GetComposedMessage("wrong.number.of.columns"));
            this.relativeWidths = new float[relativeWidths.Length];
            Array.Copy(relativeWidths, 0, this.relativeWidths, 0, relativeWidths.Length);
            absoluteWidths = new float[relativeWidths.Length];
            totalHeight = 0;
            CalculateWidths();
            CalculateHeights();
        }

        /** Sets the relative widths of the table.
        * @param relativeWidths the relative widths of the table.
        * @throws DocumentException if the number of widths is different than the number
        *                           of columns
        */

        virtual public void SetWidths(int[] relativeWidths)
        {
            float[] tb = new float[relativeWidths.Length];
            for (int k = 0; k < relativeWidths.Length; ++k)
                tb[k] = relativeWidths[k];
            SetWidths(tb);
        }

        /**
        * @since 2.1.6 private is now protected
        */

        virtual protected internal void CalculateWidths()
        {
            if (totalWidth <= 0)
                return;
            float total = 0;
            int numCols = NumberOfColumns;
            for (int k = 0; k < numCols; ++k)
                total += relativeWidths[k];
            for (int k = 0; k < numCols; ++k)
                absoluteWidths[k] = totalWidth*relativeWidths[k]/total;
        }

        /** Sets the full width of the table from the absolute column width.
        * @param columnWidth the absolute width of each column
        * @throws DocumentException if the number of widths is different than the number
        *                           of columns
        */

        virtual public void SetTotalWidth(float[] columnWidth)
        {
            if (columnWidth.Length != NumberOfColumns)
                throw new DocumentException(MessageLocalization.GetComposedMessage("wrong.number.of.columns"));
            totalWidth = 0;
            for (int k = 0; k < columnWidth.Length; ++k)
                totalWidth += columnWidth[k];
            SetWidths(columnWidth);
        }

        /** Sets the percentage width of the table from the absolute column width.  Warning: Don't use this with setLockedWidth(true). These two settings don't mix.
        * @param columnWidth the absolute width of each column
        * @param pageSize    the page size
        * @throws DocumentException
        */

        virtual public void SetWidthPercentage(float[] columnWidth, Rectangle pageSize)
        {
            if (columnWidth.Length != NumberOfColumns)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("wrong.number.of.columns"));
            SetTotalWidth(columnWidth);
            widthPercentage = totalWidth/(pageSize.Right - pageSize.Left)*100f;
        }

        /** Gets the full width of the table.
        * @return the full width of the table
        */

        virtual public float TotalWidth
        {
            get { return totalWidth; }
            set
            {
                if (this.totalWidth == value)
                    return;
                this.totalWidth = value;
                totalHeight = 0;
                CalculateWidths();
                CalculateHeights();
            }
        }

        /**
         * Calculates the heights of the table.
         *
         * @return the total height of the table. Note that it will be 0 if you didn't
         * specify the width of the table with SetTotalWidth().
         * and made it public
         */

        virtual public float CalculateHeights()
        {
            if (totalWidth <= 0)
                return 0;
            totalHeight = 0;
            for (int k = 0; k < rows.Count; ++k)
            {
                totalHeight += GetRowHeight(k, true);
            }
            return totalHeight;
        }

        /**
         * Changes the number of columns. Any existing rows will be deleted.
         * 
         * @param the new number of columns
         */

        virtual public void ResetColumnCount(int newColCount)
        {
            if (newColCount <= 0)
                throw new ArgumentException(
                    MessageLocalization.GetComposedMessage(
                        "the.number.of.columns.in.pdfptable.constructor.must.be.greater.than.zero"));
            relativeWidths = new float[newColCount];
            for (int k = 0; k < newColCount; ++k)
                relativeWidths[k] = 1;
            absoluteWidths = new float[relativeWidths.Length];
            CalculateWidths();
            currentRow = new PdfPCell[absoluteWidths.Length];
            totalHeight = 0;
        }

        /** Gets the default <CODE>PdfPCell</CODE> that will be used as
        * reference for all the <CODE>addCell</CODE> methods except
        * <CODE>addCell(PdfPCell)</CODE>.
        * @return default <CODE>PdfPCell</CODE>
        */

        virtual public PdfPCell DefaultCell
        {
            get { return defaultCell; }
        }

        /**
        * Adds a cell element.
        * 
        * @param cell the cell element
        */

        virtual public PdfPCell AddCell(PdfPCell cell)
        {
            rowCompleted = false;
            PdfPCell ncell;
            if (cell is PdfPHeaderCell)
                ncell = new PdfPHeaderCell((PdfPHeaderCell)cell);
            else
                ncell = new PdfPCell(cell);

            int colspan = ncell.Colspan;
            colspan = Math.Max(colspan, 1);
            colspan = Math.Min(colspan, currentRow.Length - currentColIdx);
            ncell.Colspan = colspan;

            if (colspan != 1)
                isColspan = true;
            int rdir = ncell.RunDirection;
            if (rdir == PdfWriter.RUN_DIRECTION_DEFAULT)
                ncell.RunDirection = runDirection;

            SkipColsWithRowspanAbove();

            bool cellAdded = false;
            if (currentColIdx < currentRow.Length)
            {
                currentRow[currentColIdx] = ncell;
                currentColIdx += colspan;
                cellAdded = true;
            }

            SkipColsWithRowspanAbove();

            while (currentColIdx >= currentRow.Length)
            {
                int numCols = NumberOfColumns;
                if (runDirection == PdfWriter.RUN_DIRECTION_RTL)
                {
                    PdfPCell[] rtlRow = new PdfPCell[numCols];
                    int rev = currentRow.Length;
                    for (int k = 0; k < currentRow.Length; ++k)
                    {
                        PdfPCell rcell = currentRow[k];
                        int cspan = rcell.Colspan;
                        rev -= cspan;
                        rtlRow[rev] = rcell;
                        k += cspan - 1;
                    }
                    currentRow = rtlRow;
                }
                PdfPRow row = new PdfPRow(currentRow);
                if (totalWidth > 0)
                {
                    row.SetWidths(absoluteWidths);
                    totalHeight += row.MaxHeights;
                }
                rows.Add(row);
                currentRow = new PdfPCell[numCols];
                currentColIdx = 0;
                SkipColsWithRowspanAbove();
                rowCompleted = true;
            }

            if (!cellAdded)
            {
                currentRow[currentColIdx] = ncell;
                currentColIdx += colspan;
            }
            return ncell;
        }

        /**
        * When updating the row index, cells with rowspan should be taken into account.
        * This is what happens in this method.
        * 
        * @since    2.1.6
        */

        private void SkipColsWithRowspanAbove()
        {
            int direction = 1;
            if (runDirection == PdfWriter.RUN_DIRECTION_RTL)
                direction = -1;
            while (RowSpanAbove(rows.Count, currentColIdx))
                currentColIdx += direction;
        }

        /**
        * Added by timmo3.  This will return the correct cell taking it's cellspan into account
        *
        * @param row the row index
        * @param col the column index
        * @return PdfPCell at the given row and position or null otherwise
        */

        internal PdfPCell CellAt(int row, int col)
        {
            PdfPCell[] cells = rows[row].GetCells();
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] != null)
                {
                    if (col >= i && col < (i + cells[i].Colspan))
                    {
                        return cells[i];
                    }
                }
            }
            return null;
        }

        /**
        * Checks if there are rows above belonging to a rowspan.
        * @param    currRow the current row to check
        * @param    currCol the current column to check
        * @return   true if there's a cell above that belongs to a rowspan
        * @since    2.1.6
        */

        internal bool RowSpanAbove(int currRow, int currCol)
        {

            if ((currCol >= NumberOfColumns)
                || (currCol < 0)
                || (currRow < 1))
                return false;

            int row = currRow - 1;
            PdfPRow aboveRow = rows[row];
            if (aboveRow == null)
                return false;
            PdfPCell aboveCell = CellAt(row, currCol);
            while (aboveCell == null && row > 0)
            {
                aboveRow = rows[--row];
                if (aboveRow == null)
                    return false;
                aboveCell = CellAt(row, currCol);
            }

            int distance = currRow - row;

            if ((aboveCell.Rowspan == 1) && (distance > 1))
            {
                int col = currCol - 1;
                aboveRow = rows[row + 1];
                distance--;
                aboveCell = aboveRow.GetCells()[col];
                while (aboveCell == null && col > 0)
                    aboveCell = aboveRow.GetCells()[--col];
            }

            return aboveCell != null && aboveCell.Rowspan > distance;
        }

        /** Adds a cell element.
        * @param text the text for the cell
        */

        virtual public void AddCell(String text)
        {
            AddCell(new Phrase(text));
        }

        /**
        * Adds a nested table.
        * @param table the table to be added to the cell
        */

        virtual public void AddCell(PdfPTable table)
        {
            defaultCell.Table = table;
            PdfPCell newCell = AddCell(defaultCell);
            newCell.id = new AccessibleElementId();
            defaultCell.Table = null;
        }

        /**
        * Adds an Image as Cell.
        * @param image the <CODE>Image</CODE> to add to the table.
        *              This image will fit in the cell
        */

        virtual public void AddCell(Image image)
        {
            defaultCell.Image = image;
            PdfPCell newCell = AddCell(defaultCell);
            newCell.id = new AccessibleElementId();
            defaultCell.Image = null;
        }

        /**
        * Adds a cell element.
        * @param phrase the <CODE>Phrase</CODE> to be added to the cell
        */

        virtual public void AddCell(Phrase phrase)
        {
            defaultCell.Phrase = phrase;
            PdfPCell newCell = AddCell(defaultCell);
            newCell.id = new AccessibleElementId();
            defaultCell.Phrase = null;
        }

        /**
        * Writes the selected rows to the document.
        * <P>
        * <CODE>canvases</CODE> is obtained from <CODE>beginWritingRows()</CODE>.
        * @param rowStart   the first row to be written, zero index
        * @param rowEnd     the last row to be written + 1. If it is -1 all the
        *                   rows to the end are written
        * @param xPos       the x write coodinate
        * @param yPos       the y write coodinate
        * @param canvases   an array of 4 <CODE>PdfContentByte</CODE> obtained from
        *                   <CODE>beginWrittingRows()</CODE>
        * @return the y coordinate position of the bottom of the last row
        * @see #beginWritingRows(com.lowagie.text.pdf.PdfContentByte)
        */

        virtual public float WriteSelectedRows(int rowStart, int rowEnd, float xPos, float yPos, PdfContentByte[] canvases)
        {
            return WriteSelectedRows(0, -1, rowStart, rowEnd, xPos, yPos, canvases);
        }

        /** Writes the selected rows and columns to the document.
        * This method does not clip the columns; this is only important
        * if there are columns with colspan at boundaries.
        * <P>
        * <CODE>canvases</CODE> is obtained from <CODE>beginWritingRows()</CODE>.
        * <P>
        * The table event is only fired for complete rows.
        * @param colStart   the first column to be written, zero index
        * @param colEnd     the last column to be written + 1. If it is -1 all the
        *                   columns to the end are written
        * @param rowStart   the first row to be written, zero index
        * @param rowEnd     the last row to be written + 1. If it is -1 all the
        *                   rows to the end are written
        * @param xPos       the x write coodinate
        * @param yPos       the y write coodinate
        * @param canvases   an array of 4 <CODE>PdfContentByte</CODE> obtained from
        *                   <CODE>beginWrittingRows()</CODE>
        * @return the y coordinate position of the bottom of the last row
        * @see #beginWritingRows(com.lowagie.text.pdf.PdfContentByte)
        */

        virtual public float WriteSelectedRows(int colStart, int colEnd, int rowStart, int rowEnd, float xPos, float yPos,
                                       PdfContentByte[] canvases)
        {
            return WriteSelectedRows(colStart, colEnd, rowStart, rowEnd, xPos, yPos, canvases, true);
        }

        /**
         * Writes the selected rows and columns to the document.
         * This method does not clip the columns; this is only important
         * if there are columns with colspan at boundaries.
         * <CODE>canvases</CODE> is obtained from <CODE>beginWritingRows()</CODE>.
         * The table event is only fired for complete rows.
         *
         * @param colStart  the first column to be written, zero index
         * @param colEnd    the last column to be written + 1. If it is -1 all the
         *                  columns to the end are written
         * @param rowStart  the first row to be written, zero index
         * @param rowEnd    the last row to be written + 1. If it is -1 all the
         *                  rows to the end are written
         * @param xPos      the x write coordinate
         * @param yPos      the y write coordinate
         * @param canvases  an array of 4 <CODE>PdfContentByte</CODE> obtained from
         *                  <CODE>beginWritingRows()</CODE>
         * @param   reusable if set to false, the content in the cells is "consumed";
         * if true, you can reuse the cells, the row, the parent table as many times you want.
         * @return the y coordinate position of the bottom of the last row
         * @see #beginWritingRows(com.itextpdf.text.pdf.PdfContentByte)
         * @since 5.1.0 added the reusable parameter
         */

        virtual public float WriteSelectedRows(int colStart, int colEnd, int rowStart, int rowEnd, float xPos, float yPos,
                                       PdfContentByte[] canvases, bool reusable)
        {
            if (totalWidth <= 0)
                throw new ArgumentException(
                    MessageLocalization.GetComposedMessage("the.table.width.must.be.greater.than.zero"));
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

            LOGGER.Info(String.Format("Writing row {0} to {1}; column {2} to {3}", rowStart, rowEnd, colStart, colEnd));

            float yPosStart = yPos;

            PdfPTableBody currentBlock = null;
            if (rowsNotChecked)
                GetFittingRows(float.MaxValue, rowStart);
            List<PdfPRow> __rows = GetRows(rowStart, rowEnd);
            int k = rowStart;
            foreach (PdfPRow row in __rows) {
                if (GetHeader().rows != null && GetHeader().rows.Contains(row) && currentBlock == null) {
                    currentBlock = OpenTableBlock(GetHeader(), canvases[TEXTCANVAS]);
                } else if (GetBody().rows != null && GetBody().rows.Contains(row) && currentBlock == null) {
                    currentBlock = OpenTableBlock(GetBody(), canvases[TEXTCANVAS]);
                } else if (GetFooter().rows != null && GetFooter().rows.Contains(row) && currentBlock == null) {
                    currentBlock = OpenTableBlock(GetFooter(), canvases[TEXTCANVAS]);
                }
                if (row != null)
                {
                    row.WriteCells(colStart, colEnd, xPos, yPos, canvases, reusable);
                    yPos -= row.MaxHeights;
                }
                if (GetHeader().rows != null && GetHeader().rows.Contains(row) && (k == rowEnd - 1 || !GetHeader().rows.Contains(__rows[k + 1]))) {
                    currentBlock = CloseTableBlock(GetHeader(), canvases[TEXTCANVAS]);
                } else if (GetBody().rows != null && GetBody().rows.Contains(row) && (k == rowEnd - 1 || !GetBody().rows.Contains(__rows[k + 1]))) {
                    currentBlock = CloseTableBlock(GetBody(), canvases[TEXTCANVAS]);
                } else if (GetFooter().rows != null && GetFooter().rows.Contains(row) && (k == rowEnd - 1 || !GetFooter().rows.Contains(__rows[k + 1]))) {
                    currentBlock = CloseTableBlock(GetFooter(), canvases[TEXTCANVAS]);
                }
                k++;
            }
            if (tableEvent != null && colStart == 0 && colEnd == totalCols)
            {
                float[] heights = new float[rowEnd - rowStart + 1];
                heights[0] = yPosStart;
                for (k = rowStart; k < rowEnd; ++k)
                {
                    PdfPRow row = __rows[k];
                    float hr = 0;
                    if (row != null)
                        hr = row.MaxHeights;
                    heights[k - rowStart + 1] = heights[k - rowStart] - hr;
                }
                tableEvent.TableLayout(this, GetEventWidths(xPos, rowStart, rowEnd, headersInEvent), heights,
                                       headersInEvent ? headerRows : 0, rowStart, canvases);
            }
            return yPos;
        }

        private PdfPTableBody OpenTableBlock(PdfPTableBody block, PdfContentByte canvas) {
            if (canvas.writer.GetStandardStructElems().Contains(block.Role)) {
                canvas.OpenMCBlock(block);
                return block;
            }
            return null;
        }

        private PdfPTableBody CloseTableBlock(PdfPTableBody block, PdfContentByte canvas) {
            if (canvas.writer.GetStandardStructElems().Contains(block.Role))
                canvas.CloseMCBlock(block);
            return null;
        }

        /**
        * Writes the selected rows to the document.
        * 
        * @param rowStart   the first row to be written, zero index
        * @param rowEnd     the last row to be written + 1. If it is -1 all the
        *                   rows to the end are written
        * @param xPos       the x write coodinate
        * @param yPos       the y write coodinate
        * @param canvas     the <CODE>PdfContentByte</CODE> where the rows will
        *                   be written to
        * @return the y coordinate position of the bottom of the last row
        */

        virtual public float WriteSelectedRows(int rowStart, int rowEnd, float xPos, float yPos, PdfContentByte canvas)
        {
            return WriteSelectedRows(0, -1, rowStart, rowEnd, xPos, yPos, canvas);
        }

        /**
        * Writes the selected rows to the document.
        * This method clips the columns; this is only important
        * if there are columns with colspan at boundaries.
        * <P>
        * The table event is only fired for complete rows.
        * 
        * @param colStart   the first column to be written, zero index
        * @param colEnd     the last column to be written + 1. If it is -1 all the
        * @param rowStart   the first row to be written, zero index
        * @param rowEnd     the last row to be written + 1. If it is -1 all the
        *                   rows to the end are written
        * @param xPos       the x write coodinate
        * @param yPos       the y write coodinate
        * @param canvas     the <CODE>PdfContentByte</CODE> where the rows will
        *                   be written to
        * @return the y coordinate position of the bottom of the last row
        */

        virtual public float WriteSelectedRows(int colStart, int colEnd, int rowStart, int rowEnd, float xPos, float yPos,
                                       PdfContentByte canvas)
        {
            return WriteSelectedRows(colStart, colEnd, rowStart, rowEnd, xPos, yPos, canvas, true);
        }


        /**
         * Writes the selected rows and columns to the document.
         * This method clips the columns; this is only important
         * if there are columns with colspan at boundaries.
         * The table event is only fired for complete rows.
         *
         * @param colStart  the first column to be written, zero index
         * @param colEnd    the last column to be written + 1. If it is -1 all the
         *                  columns to the end are written
         * @param rowStart  the first row to be written, zero index
         * @param rowEnd    the last row to be written + 1. If it is -1 all the
         *                  rows to the end are written
         * @param xPos      the x write coordinate
         * @param yPos      the y write coordinate
         * @param canvas    the <CODE>PdfContentByte</CODE> where the rows will
         *                  be written to     
         * @return the y coordinate position of the bottom of the last row
         * @param   reusable if set to false, the content in the cells is "consumed";
         * if true, you can reuse the cells, the row, the parent table as many times you want.
         * @since 5.1.0 added the reusable parameter
         */

        virtual public float WriteSelectedRows(int colStart, int colEnd, int rowStart, int rowEnd, float xPos, float yPos,
                                       PdfContentByte canvas, bool reusable)
        {
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

            if (clip)
            {
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
            float y = WriteSelectedRows(colStart, colEnd, rowStart, rowEnd, xPos, yPos, canvases, reusable);
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
        * @param    canvas the <CODE>PdfContentByte</CODE> where the rows will
        *           be written to
        * @return an array of 4 <CODE>PdfContentByte</CODE>
        * @see #writeSelectedRows(int, int, float, float, PdfContentByte[])
        */

        public static PdfContentByte[] BeginWritingRows(PdfContentByte canvas)
        {
            return new PdfContentByte[]
                       {
                           canvas,
                           canvas.Duplicate,
                           canvas.Duplicate,
                           canvas.Duplicate
                       };
        }

        /** Finishes writing the table.
        * @param canvases the array returned by <CODE>beginWritingRows()</CODE>
        */

        public static void EndWritingRows(PdfContentByte[] canvases)
        {
            PdfContentByte canvas = canvases[BASECANVAS];
            PdfArtifact artifact = new PdfArtifact();
            canvas.OpenMCBlock(artifact);
            canvas.SaveState();
            canvas.Add(canvases[BACKGROUNDCANVAS]);
            canvas.RestoreState();
            canvas.SaveState();
            canvas.SetLineCap(2);
            canvas.ResetRGBColorStroke();
            canvas.Add(canvases[LINECANVAS]);
            canvas.RestoreState();
            canvas.CloseMCBlock(artifact);
            canvas.Add(canvases[TEXTCANVAS]);
        }

        /** Gets the number of rows in this table.
        * @return the number of rows in this table
        */

        virtual public int Size
        {
            get { return rows.Count; }
        }

        /** Gets the total height of the table.
        * @return the total height of the table
        */

        virtual public float TotalHeight
        {
            get { return totalHeight; }
        }

        /** Gets the height of a particular row.
        * @param idx the row index (starts at 0)
        * @return the height of a particular row
        */

        virtual public float GetRowHeight(int idx)
        {
            return GetRowHeight(idx, false);
        }

        /**
        * Gets the height of a particular row.
        * 
        * @param idx        the row index (starts at 0)
        * @param firsttime  is this the first time the row heigh is calculated?
        * @return the height of a particular row
        * @since 5.0.0
        */

        virtual protected internal float GetRowHeight(int idx, bool firsttime)
        {
            if (totalWidth <= 0 || idx < 0 || idx >= rows.Count)
                return 0;
            PdfPRow row = rows[idx];
            if (row == null)
                return 0;
            if (firsttime)
                row.SetWidths(absoluteWidths);
            float height = row.MaxHeights;
            PdfPCell cell;
            PdfPRow tmprow;
            for (int i = 0; i < relativeWidths.Length; i++)
            {
                if (!RowSpanAbove(idx, i))
                    continue;
                int rs = 1;
                while (RowSpanAbove(idx - rs, i))
                {
                    rs++;
                }
                tmprow = rows[idx - rs];
                cell = tmprow.GetCells()[i];
                float tmp = 0;
                if (cell != null && cell.Rowspan == rs + 1)
                {
                    tmp = cell.GetMaxHeight();
                    while (rs > 0)
                    {
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
        * @param rowIndex    the row index
        * @param cellIndex   the cell index
        * @return the height of a particular row including rowspan
        * @since    2.1.6
        */

        virtual public float GetRowspanHeight(int rowIndex, int cellIndex)
        {
            if (totalWidth <= 0 || rowIndex < 0 || rowIndex >= rows.Count)
                return 0;
            PdfPRow row = rows[rowIndex];
            if (row == null || cellIndex >= row.GetCells().Length)
                return 0;
            PdfPCell cell = row.GetCells()[cellIndex];
            if (cell == null)
                return 0;
            float rowspanHeight = 0;
            for (int j = 0; j < cell.Rowspan; j++)
            {
                rowspanHeight += GetRowHeight(rowIndex + j);
            }
            return rowspanHeight;
        }

        /**
         * Checks if a cell in a row has a rowspan greater than 1.
         * 
         * @since 5.1.0
         */

        virtual public bool HasRowspan(int rowIdx)
        {
            if (rowIdx < rows.Count && GetRow(rowIdx).HasRowspan())
            {
                return true;
            }
            PdfPRow previousRow = rowIdx > 0 ? GetRow(rowIdx - 1) : null;
    	    if (previousRow != null && previousRow.HasRowspan()) {
                return true;
            }
            for (int i = 0; i < NumberOfColumns; i++)
            {
                if (RowSpanAbove(rowIdx - 1, i))
                    return true;
            }
            return false;
        }

        /**
         * Makes sure the footers value is lower than the headers value.
         * 
         * @since 5.0.1
         */

        virtual public void NormalizeHeadersFooters()
        {
            if (footerRows > headerRows)
                footerRows = headerRows;
        }

        /** Gets the height of the rows that constitute the header as defined by
        * <CODE>setHeaderRows()</CODE>.
        * @return the height of the rows that constitute the header and footer
        */

        virtual public float HeaderHeight
        {
            get
            {
                float total = 0;
                int size = Math.Min(rows.Count, headerRows);
                for (int k = 0; k < size; ++k)
                {
                    PdfPRow row = rows[k];
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

        virtual public float FooterHeight
        {
            get
            {
                float total = 0;
                int start = Math.Max(0, headerRows - footerRows);
                int size = Math.Min(rows.Count, headerRows);
                for (int k = start; k < size; ++k)
                {
                    PdfPRow row = rows[k];
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

        virtual public bool DeleteRow(int rowNumber)
        {
            if (rowNumber < 0 || rowNumber >= rows.Count)
            {
                return false;
            }
            if (totalWidth > 0)
            {
                PdfPRow row = rows[rowNumber];
                if (row != null)
                    totalHeight -= row.MaxHeights;
            }
            rows.RemoveAt(rowNumber);
            if (rowNumber < headerRows)
            {
                --headerRows;
                if (rowNumber >= (headerRows - footerRows))
                    --footerRows;
            }
            return true;
        }

        /** Deletes the last row in the table.
        * @return <CODE>true</CODE> if the last row was deleted
        */

        virtual public bool DeleteLastRow()
        {
            return DeleteRow(rows.Count - 1);
        }

        /**
        * Removes all of the rows except headers
        */

        virtual public void DeleteBodyRows()
        {
            List<PdfPRow> rows2 = new List<PdfPRow>();
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

        virtual public int NumberOfColumns
        {
            get { return relativeWidths.Length; }
        }

        virtual public int HeaderRows
        {
            get { return headerRows; }
            set
            {
                headerRows = value;
                if (headerRows < 0)
                    headerRows = 0;
            }
        }

        virtual public int FooterRows
        {
            get { return footerRows; }
            set
            {
                footerRows = value;
                if (footerRows < 0)
                    footerRows = 0;
            }
        }

        /**
        * Gets all the chunks in this element.
        *
        * @return    an <CODE>List</CODE>
        */

        virtual public IList<Chunk> Chunks
        {
            get { return new List<Chunk>(); }
        }

        /**
        * Gets the type of the text element.
        *
        * @return    a type
        */

        virtual public int Type
        {
            get { return Element.PTABLE; }
        }

        /**
        * @since   iText 2.0.8
        * @see com.lowagie.text.Element#isContent()
        */

        virtual public bool IsContent()
        {
            return true;
        }

        /**
        * @since   iText 2.0.8
        * @see com.lowagie.text.Element#isNestable()
        */

        virtual public bool IsNestable()
        {
            return true;
        }

        /**
        * Processes the element by adding it (or the different parts) to an
        * <CODE>ElementListener</CODE>.
        *
        * @param    listener    an <CODE>ElementListener</CODE>
        * @return    <CODE>true</CODE> if the element was processed successfully
        */

        virtual public bool Process(IElementListener listener)
        {
            try
            {
                return listener.Add(this);
            }
            catch (DocumentException)
            {
                return false;
            }
        }

        virtual public float WidthPercentage
        {
            get { return widthPercentage; }
            set { widthPercentage = value; }
        }

        virtual public int HorizontalAlignment
        {
            get { return horizontalAlignment; }
            set { horizontalAlignment = value; }
        }

        /**
         * Gets a row with a given index.
         *
         * @param idx
         * @return the row at position idx
         */

        virtual public PdfPRow GetRow(int idx)
        {
            return rows[idx];
        }

        /**
         * Returns the index of the last completed row.
         *
         * @return the index of a row
         */

        virtual public int getLastCompletedRowIndex()
        {
            return rows.Count - 1;
        }

        /**
         * Defines where the table may be broken (if necessary).
         *
         * @param breakPoints int[]
         * @throws System.IndexOutOfRangeException if a row index is passed that is out of bounds
         */

        virtual public void SetBreakPoints(int[] breakPoints)
        {
            KeepRowsTogether(0, rows.Count); // sets all rows as unbreakable

            for (int i = 0; i < breakPoints.Length; i++)
            {
                GetRow(breakPoints[i]).MayNotBreak = false;
            }
        }

        /**
         * Defines which rows should not allow a page break (if possible).
         *
         * @param rows int[]
         * @throws System.IndexOutOfRangeException if a row index is passed that is out of bounds
         */

        virtual public void KeepRowsTogether(int[] rows)
        {
            for (int i = 0; i < rows.Length; i++)
            {
                GetRow(rows[i]).MayNotBreak = true;
            }
        }

        /**
         * Defines a range of rows that should not allow a page break (if possible).
         *
         * @param start int
         * @param end int
         * @throws System.IndexOutOfRangeException if a row index is passed that is out of bounds
         */

        virtual public void KeepRowsTogether(int start, int end)
        {
            if (start < end)
            {
                while (start < end)
                {
                    GetRow(start).MayNotBreak = true;
                    start++;
                }
            }
        }

        /**
         * Defines a range of rows (from the parameter to the last row) that should not allow a page break (if possible).
         * The equivalent of calling {@link #keepRowsTogether(int,int) keepRowsTogether(start, rows.size()}.
         *
         * @param start int
         * @throws System.IndexOutOfRangeException if a row index is passed that is out of bounds
         */

        virtual public void KeepRowsTogether(int start)
        {
            KeepRowsTogether(start, rows.Count);
        }

        /**
        * Gets an arraylist with all the rows in the table.
        * @return an arraylist
        */

        virtual public List<PdfPRow> Rows
        {
            get { return rows; }
        }

        /**
        * Gets an arraylist with a selection of rows.
        * @param    start   the first row in the selection
        * @param    end     the first row that isn't part of the selection
        * @return a selection of rows
        * @since    2.1.6
        */

        virtual public List<PdfPRow> GetRows(int start, int end)
        {
            List<PdfPRow> list = new List<PdfPRow>();
            if (start < 0 || end > Size)
            {
                return list;
            }
            for (int i = start; i < end; i++)
            {
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

        virtual protected PdfPRow AdjustCellsInRow(int start, int end)
        {
            PdfPRow row = GetRow(start);
            if (row.Adjusted) return row;
            row = new PdfPRow(row);
            PdfPCell cell;
            PdfPCell[] cells = row.GetCells();
            for (int i = 0; i < cells.Length; i++)
            {
                cell = cells[i];
                if (cell == null || cell.Rowspan == 1)
                    continue;
                int stop = Math.Min(end, start + cell.Rowspan);
                float extra = 0;
                for (int k = start + 1; k < stop; k++)
                {
                    extra += GetRow(k).MaxHeights;
                }
                row.SetExtraHeight(i, extra);
            }
            row.Adjusted = true;
            return row;
        }

        /**
         * Sets the table event for this table.
         *
         * @param event the table event for this table
         */

        virtual public IPdfPTableEvent TableEvent
        {
            get { return tableEvent; }
            set
            {
                if (value == null) this.tableEvent = null;
                else if (this.tableEvent == null) this.tableEvent = value;
                else if (this.tableEvent is PdfPTableEventForwarder)
                    ((PdfPTableEventForwarder) this.tableEvent).AddTableEvent(value);
                else
                {
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

        virtual public float[] AbsoluteWidths
        {
            get { return absoluteWidths; }
        }

        internal float[][] GetEventWidths(float xPos, int firstRow, int lastRow, bool includeHeaders)
        {
            if (includeHeaders)
            {
                firstRow = Math.Max(firstRow, headerRows);
                lastRow = Math.Max(lastRow, headerRows);
            }
            float[][] widths = new float[(includeHeaders ? headerRows : 0) + lastRow - firstRow][];
            if (isColspan)
            {
                int n = 0;
                if (includeHeaders)
                {
                    for (int k = 0; k < headerRows; ++k)
                    {
                        PdfPRow row = rows[k];
                        if (row == null)
                            ++n;
                        else
                            widths[n++] = row.GetEventWidth(xPos, absoluteWidths);
                    }
                }
                for (; firstRow < lastRow; ++firstRow)
                {
                    PdfPRow row = rows[firstRow];
                    if (row == null)
                        ++n;
                    else
                        widths[n++] = row.GetEventWidth(xPos, absoluteWidths);
                }
            }
            else
            {
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

        virtual public bool SkipFirstHeader
        {
            get { return skipFirstHeader; }
            set { skipFirstHeader = value; }
        }

        /**
        * Tells you if the last footer needs to be skipped
        * (for instance if the footer says "continued on the next page")
        * 
        * @return Value of property skipLastFooter.
        * @since 2.1.6
        */

        virtual public bool SkipLastFooter
        {
            get { return skipLastFooter; }
            set { skipLastFooter = value; }
        }

        virtual public int RunDirection
        {
            get { return runDirection; }
            set
            {
                switch (value)
                {
                    case PdfWriter.RUN_DIRECTION_DEFAULT:
                    case PdfWriter.RUN_DIRECTION_NO_BIDI:
                    case PdfWriter.RUN_DIRECTION_LTR:
                    case PdfWriter.RUN_DIRECTION_RTL:
                        this.runDirection = value;
                        break;
                    default:
                        throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.run.direction.1",
                                                                                           runDirection));
                }
            }
        }

        virtual public bool LockedWidth
        {
            get { return lockedWidth; }
            set { lockedWidth = value; }
        }

        virtual public bool SplitRows
        {
            get { return splitRows; }
            set { splitRows = value; }
        }

        virtual public float SpacingBefore
        {
            get { return spacingBefore; }
            set { spacingBefore = value; }
        }

        virtual public float SpacingAfter
        {
            get { return spacingAfter; }
            set { spacingAfter = value; }
        }

        virtual public float PaddingTop
        {
            get { return paddingTop; }
            set { paddingTop = value; }
        }

        virtual public String Summary 
        {
            get { return GetAccessibleAttribute(PdfName.SUMMARY).ToString(); }
            set { SetAccessibleAttribute(PdfName.SUMMARY, new PdfString(value)); }
        }

        virtual public bool ExtendLastRow
        {
            get { return extendLastRow[0]; }
            set
            {
                extendLastRow[0] = value;
                extendLastRow[1] = value;
            }
        }

        /**
        * When set the last row on every page will be extended to fill
        * all the remaining space to the bottom boundary; except maybe the
        * row.
        * 
        * @param extendLastRows true to extend the last row on each page; false otherwise
        * @param extendFinalRow false if you don't want to extend the row of the complete table
        * @since iText 5.0.0
        */

        virtual public void SetExtendLastRow(bool extendLastRows, bool extendFinalRow)
        {
            extendLastRow[0] = extendLastRows;
            extendLastRow[1] = extendFinalRow;
        }

        /**
        * Gets the value of the last row extension, taking into account
        * if the row is reached or not.
        * 
        * @return true if the last row will extend; 
         *        false otherwise
        * @since iText 5.0.0
        */

        virtual public bool IsExtendLastRow(bool newPageFollows)
        {
            if (newPageFollows)
            {
                return extendLastRow[0];
            }
            return extendLastRow[1];
        }

        virtual public bool HeadersInEvent
        {
            get { return headersInEvent; }
            set { headersInEvent = value; }
        }

        virtual public bool SplitLate
        {
            get { return splitLate; }
            set { splitLate = value; }
        }

        /**
        * If true the table will be kept on one page if it fits, by forcing a 
        * new page if it doesn't fit on the current page. The default is to
        * split the table over multiple pages.
        *
        * @param p_KeepTogether whether to try to keep the table on one page
        */

        virtual public bool KeepTogether
        {
            set { keepTogether = value; }
            get { return keepTogether; }
        }

        /**
        * Completes the current row with the default cell. An incomplete row will be dropped
        * but calling this method will make sure that it will be present in the table.
        */

        virtual public void CompleteRow()
        {
            while (!rowCompleted)
            {
                AddCell(defaultCell);
            }
        }

        /**
        * @since   iText 2.0.8
        * @see com.lowagie.text.LargeElement#flushContent()
        */

        virtual public void FlushContent()
        {
            DeleteBodyRows();

            // SkipFirstHeader shouldn't be set to true if the table hasn't been added yet.
            if (this.numberOfWrittenRows > 0) {
                SkipFirstHeader = true;
            }
        }

        internal virtual void AddNumberOfRowsWritten(int numberOfWrittenRows) {
            this.numberOfWrittenRows += numberOfWrittenRows;
        }

        /**
        * @since   iText 2.0.8
        * @see com.lowagie.text.LargeElement#isComplete()
        */

        virtual public bool ElementComplete
        {
            get { return complete; }
            set { complete = value; }
        }

        virtual public bool LoopCheck
        {
            get { return loopCheck; }
            set { this.loopCheck = value; }
        }

        virtual public PdfObject GetAccessibleAttribute(PdfName key) {
            if (accessibleAttributes != null) {
                PdfObject value;
                accessibleAttributes.TryGetValue(key, out value);
                return value;
            } else
                return null;
        }

        virtual public void SetAccessibleAttribute(PdfName key, PdfObject value) {
            if (accessibleAttributes == null)
                accessibleAttributes = new Dictionary<PdfName, PdfObject>();
            accessibleAttributes[key] = value;
        }



        virtual public Dictionary<PdfName, PdfObject> GetAccessibleAttributes() {
            return accessibleAttributes;
        }

        virtual public PdfName Role {
            get { return role; }
            set { this.role = value; }
        }

        virtual public AccessibleElementId ID {
            get { return id; }
            set { id = value; }
        }

        public virtual bool IsInline {
            get { return false; }
        }

        virtual public PdfPTableHeader GetHeader() {
            if (header == null)
                header = new PdfPTableHeader();
            return header;
        }

        virtual public PdfPTableBody GetBody() {
            if (body == null)
                body = new PdfPTableBody();
            return body;
        }

        virtual public PdfPTableFooter GetFooter() {
            if (footer == null)
                footer = new PdfPTableFooter();
            return footer;
        }

    
        // Contributed by Deutsche Bahn Systel GmbH (Thorsten Seitz), splitting row spans
        /**
         * Gets row index where cell overlapping (rowIdx, colIdx) starts
         * @param rowIdx
         * @param colIdx
         * @return row index
         * @since iText 5.4.3
         */
        virtual public int GetCellStartRowIndex(int rowIdx, int colIdx) {
            int lastRow = rowIdx;
            while (GetRow(lastRow).GetCells()[colIdx] == null && lastRow > 0) {
                --lastRow;
            }
            return lastRow;
        }

        // Contributed by Deutsche Bahn Systel GmbH (Thorsten Seitz), splitting row spans
        /**
         * 
         * @since iText 5.4.3
         */
        public class FittingRows
        {
            public readonly int firstRow;
            public readonly int lastRow;

            public readonly float height, completedRowsHeight;

            private readonly Dictionary<int, float> correctedHeightsForLastRow;

            public FittingRows(int firstRow, int lastRow, float height, float completedRowsHeight,
                    Dictionary<int, float> correctedHeightsForLastRow) {
                this.firstRow = firstRow;
                this.lastRow = lastRow;
                this.height = height;
                this.completedRowsHeight = completedRowsHeight;
                this.correctedHeightsForLastRow = correctedHeightsForLastRow;
            }

            /**
             *  Correct chosen last fitting row so that the content of all cells with open rowspans will fit on the page,
             *  i.e. the cell content won't be split.
             * (Only to be used with splitLate == true)
             */
            virtual public void CorrectLastRowChosen(PdfPTable table, int k) {
                PdfPRow row = table.GetRow(k);
                float value;
                if (correctedHeightsForLastRow.TryGetValue(k, out value)) {
                    row.SetFinalMaxHeights(value);
                    //System.out.printf("corrected chosen last fitting row: %6.0f\n\n", row.getMaxHeights());
                }
            }
        }

        // Contributed by Deutsche Bahn Systel GmbH (Thorsten Seitz), splitting row spans
        /**
         * 
         * @since iText 5.4.3
         */
        public class ColumnMeasurementState {
            public float height = 0;

            public int rowspan = 1, colspan = 1;

            virtual public void BeginCell(PdfPCell cell, float completedRowsHeight, float rowHeight) {
                rowspan = cell.Rowspan;
                colspan = cell.Colspan;
                height = completedRowsHeight + Math.Max(cell.HasCachedMaxHeight() ? cell.CachedMaxHeight : cell.GetMaxHeight(), rowHeight);
            }

            virtual public void ConsumeRowspan(float completedRowsHeight, float rowHeight) {
        	    --rowspan;
            }

            virtual public bool CellEnds() {
                return rowspan == 1;
            }
        }

        // Contributed by Deutsche Bahn Systel GmbH (Thorsten Seitz), splitting row spans
        /**
         * Determine which rows fit on the page, respecting isSplitLate().
         * Note: sets max heights of the inspected rows as a side effect,
         * just like PdfPTable.getRowHeight(int, boolean) does.
         * Respect row.getMaxHeights() if it has been previously set (which might be independent of the height of
         * individual cells).
         * The last row written on the page will be chosen by the caller who might choose not
         * the calculated one but an earlier one (due to mayNotBreak settings on the rows).
         * The height of the chosen last row has to be corrected if splitLate == true
         * by calling FittingRows.correctLastRowChosen() by the caller to avoid splitting the content of
         * cells with open rowspans.
         * 
         * @since iText 5.4.3
         */
        virtual public FittingRows GetFittingRows(float availableHeight, int startIdx) {
            LOGGER.Info(String.Format("GetFittingRows({0}, {1})", availableHeight, startIdx));
            if (startIdx > 0 && startIdx < rows.Count) {
                System.Diagnostics.Debug.Assert(GetRow(startIdx).GetCells()[0] != null); // top left cell of current page may not be null
            }
            int cols = NumberOfColumns;
            ColumnMeasurementState[] states = new ColumnMeasurementState[cols];
            for (int i = 0; i < cols; ++i) {
                states[i] = new ColumnMeasurementState();
            }
            float completedRowsHeight = 0; // total height of all rows up to k only counting completed cells (with no open
                                           // rowspans)
            float totalHeight = 0; // total height needed to display all rows up to k, respecting rowspans
            Dictionary<int, float> correctedHeightsForLastRow = new Dictionary<int, float>();
            int k;
            for (k = startIdx; k < Size; ++k) {
                PdfPRow row = GetRow(k);
                float rowHeight = row.GetMaxRowHeightsWithoutCalculating();
                float maxCompletedRowsHeight = 0;
                int i = 0;
                while (i < cols) {
                    PdfPCell cell = row.GetCells()[i];
                    ColumnMeasurementState state = states[i];
                    if (cell == null) {
                        state.ConsumeRowspan(completedRowsHeight, rowHeight);
                    } else {
                        state.BeginCell(cell, completedRowsHeight, rowHeight);
                        LOGGER.Info(String.Format("Height after BeginCell: {0} (cell: {1})", state.height, cell.CachedMaxHeight));
                
                    }
                    if (state.CellEnds() && state.height > maxCompletedRowsHeight) {
                        maxCompletedRowsHeight = state.height;
                    }
                    for (int j = 1; j < state.colspan; ++j) {
                        states[i + j].height = state.height;
                    }
                    i += state.colspan;
                    //System.out.printf("%6.0f", state.height);
                }
                float maxTotalHeight = 0;
                foreach (ColumnMeasurementState state in states) {
                    if (state.height > maxTotalHeight) {
                        maxTotalHeight = state.height;
                    }
                }
                row.SetFinalMaxHeights(maxCompletedRowsHeight - completedRowsHeight);
                //System.out.printf(" | %6.0f | %6.0f %6.0f | row: %6.0f\n", rowHeight, maxCompletedRowsHeight, maxTotalHeight, row.getMaxHeights());
                float remainingHeight = availableHeight - (SplitLate ? maxTotalHeight : maxCompletedRowsHeight);
                if (remainingHeight < 0) {
                    break;
                }
                correctedHeightsForLastRow[k] =  maxTotalHeight - completedRowsHeight;
                completedRowsHeight = maxCompletedRowsHeight;
                totalHeight = maxTotalHeight;
            }
            rowsNotChecked = false;
            return new FittingRows(startIdx, k - 1, totalHeight, completedRowsHeight, correctedHeightsForLastRow);
        }
    }
}
