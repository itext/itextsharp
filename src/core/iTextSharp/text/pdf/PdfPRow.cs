using System;
using System.Collections.Generic;
using iTextSharp.text.log;
using iTextSharp.text.pdf.interfaces;

/*
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

namespace iTextSharp.text.pdf {

    /**
    * A row in a PdfPTable.
    * 
    * @author Paulo Soares
    */
    public class PdfPRow : IAccessibleElement {
        private readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof(PdfPTable));
        
        /** True if the table may not break after this row. */
        public bool mayNotBreak = false;
        /** the bottom limit (bottom right y) */
        public const float BOTTOM_LIMIT = -(1 << 30);
        /**
        * the right limit
        * @since    2.1.5
        */
        public const float RIGHT_LIMIT = 20000;

        protected PdfPCell[] cells;

        protected float[] widths;
        
        /**
        * extra heights that needs to be added to a cell because of rowspans.
        * @since    2.1.6
        */
        protected float[] extraHeights;

        protected internal float maxHeight = 0;
        
        protected internal bool calculated = false;
        protected bool adjusted = false;
        
        private int[] canvasesPos;

        protected PdfName role = PdfName.TR;
        protected Dictionary<PdfName, PdfObject> accessibleAttributes = null;
        protected AccessibleElementId id = new AccessibleElementId();
    
        
        /**
        * Constructs a new PdfPRow with the cells in the array that was passed
        * as a parameter.
        * 
        * @param cells
        */
        public PdfPRow(PdfPCell[] cells) : this(cells, null)
        {
        }

        public PdfPRow(PdfPCell[] cells, PdfPRow source) {
            this.cells = cells;
            widths = new float[cells.Length];
            InitExtraHeights();
            if (source != null) {
                this.id = source.ID;
                this.role = source.Role;
                if (source.accessibleAttributes != null)
                    this.accessibleAttributes = new Dictionary<PdfName, PdfObject>(source.GetAccessibleAttributes());
            }
        }

        /**
        * Makes a copy of an existing row.
        * 
        * @param row
        */
        public PdfPRow(PdfPRow row) {
            mayNotBreak = row.mayNotBreak;
            maxHeight = row.maxHeight;
            calculated = row.calculated;
            cells = new PdfPCell[row.cells.Length];
            for (int k = 0; k < cells.Length; ++k) {
                if (row.cells[k] != null)
                {
                    if (row.cells[k] is PdfPHeaderCell)
                        cells[k] = new PdfPHeaderCell((PdfPHeaderCell)row.cells[k]);
                    else
                        cells[k] = new PdfPCell(row.cells[k]);
                }
            }
            widths = new float[cells.Length];
            System.Array.Copy(row.widths, 0, widths, 0, cells.Length);
            InitExtraHeights();
            this.id = row.ID;
            this.role = row.Role;
            if (row.accessibleAttributes != null)
                this.accessibleAttributes = new Dictionary<PdfName, PdfObject>(row.GetAccessibleAttributes());
        }

        /**
        * Sets the widths of the columns in the row.
        * 
        * @param widths
        * @return true if everything went right
        */
        virtual public bool SetWidths(float[] widths) {
            if (widths.Length != cells.Length)
                return false;
            System.Array.Copy(widths, 0, this.widths, 0, cells.Length);
            float total = 0;
            calculated = false;
            for (int k = 0; k < widths.Length; ++k) {
                PdfPCell cell = cells[k];
                
                if (cell == null) {
                    total += widths[k];
                    continue;
                }
                
                cell.Left = total;
                int last = k + cell.Colspan;
                for (; k < last; ++k)
                    total += widths[k];
                --k;
                cell.Right = total;
                cell.Top = 0;
            }
            return true;
        }

        /**
        * Initializes the extra heights array.
        * @since    2.1.6
        */
        virtual protected internal void InitExtraHeights() {
            extraHeights = new float[cells.Length];
            for (int i = 0; i < extraHeights.Length; i++) {
                extraHeights[i] = 0;
            }
        }
        
        /**
        * Sets an extra height for a cell.
        * @param    cell    the index of the cell that needs an extra height
        * @param    height  the extra height
        * @since    2.1.6
        */
        virtual public void SetExtraHeight(int cell, float height) {
            if (cell < 0 || cell >= cells.Length)
                return;
            extraHeights[cell] = height;
        }
        
        /**
         * Calculates the heights of each cell in the row.
         * 
         * @return the maximum height of the row.
         */
        virtual protected internal void CalculateHeights() {
            maxHeight = 0;
            LOGGER.Info("CalculateHeights");
            for (int k = 0; k < cells.Length; ++k) {
                PdfPCell cell = cells[k];
                float height = 0;
                if (cell != null) {
                    height = cell.HasCalculatedHeight() ? cell.CalculatedHeight : cell.GetMaxHeight();

                    if ((height > maxHeight) && (cell.Rowspan == 1))
                        maxHeight = height;
                }
            }
            calculated = true;
        }

        virtual public bool MayNotBreak
        {
            get { return mayNotBreak; }
            set { mayNotBreak = value; }
        }

        /**
        * Writes the border and background of one cell in the row.
        * 
        * @param xPos The x-coordinate where the table starts on the canvas
        * @param yPos The y-coordinate where the table starts on the canvas
        * @param currentMaxHeight The height of the cell to be drawn.
        * @param cell
        * @param canvases
        * @since    2.1.6   extra parameter currentMaxHeight
        */
        virtual public void WriteBorderAndBackground(float xPos, float yPos, float currentMaxHeight, PdfPCell cell, PdfContentByte[] canvases) {
            BaseColor background = cell.BackgroundColor;
            if (background != null || cell.HasBorders()) {
                // Add xPos resp. yPos to the cell's coordinates for absolute coordinates
                float right = cell.Right + xPos;
                float top = cell.Top + yPos;
                float left = cell.Left + xPos;
                float bottom = top - currentMaxHeight;
                
                if (background != null) {
                    PdfContentByte backgr = canvases[PdfPTable.BACKGROUNDCANVAS];
                    backgr.SetColorFill(background);
                    backgr.Rectangle(left, bottom, right - left, top - bottom);
                    backgr.Fill();
                }
                if (cell.HasBorders()) {
                    Rectangle newRect = new Rectangle(left, bottom, right, top);
                    // Clone non-position parameters except for the background color
                    newRect.CloneNonPositionParameters(cell);
                    newRect.BackgroundColor = null;
                    // Write the borders on the line canvas
                    PdfContentByte lineCanvas = canvases[PdfPTable.LINECANVAS];
                    lineCanvas.Rectangle(newRect);
                }
            }
        }

        /**
        * @since    2.1.6 private is now protected
        */
        virtual protected void SaveAndRotateCanvases(PdfContentByte[] canvases, float a, float b, float c, float d, float e, float f) {
            int last = PdfPTable.TEXTCANVAS + 1;
            if (canvasesPos == null)
                canvasesPos = new int[last * 2];
            for (int k = 0; k < last; ++k) {
                ByteBuffer bb = canvases[k].InternalBuffer;
                canvasesPos[k * 2] = bb.Size;
                canvases[k].SaveState();
                canvases[k].ConcatCTM(a, b, c, d, e, f);
                canvasesPos[k * 2 + 1] = bb.Size;
            }
        }

        /**
        * @since    2.1.6 private is now protected
        */
        virtual protected void RestoreCanvases(PdfContentByte[] canvases) {
            int last = PdfPTable.TEXTCANVAS + 1;
            for (int k = 0; k < last; ++k) {
                ByteBuffer bb = canvases[k].InternalBuffer;
                int p1 = bb.Size;
                canvases[k].RestoreState();
                if (p1 == canvasesPos[k * 2 + 1])
                    bb.Size = canvasesPos[k * 2];
            }
        }

        /**
        * @since	3.0.0 protected is now public static
        */
        public static float SetColumn(ColumnText ct, float left, float bottom, float right, float top) {
            if (left > right)
                right = left;
            if (bottom > top)
                top = bottom;
            ct.SetSimpleColumn(left, bottom, right, top);
            return top;
        }
        
        /**
        * Writes a number of cells (not necessarily all cells).
        * 
        * @param    colStart The first column to be written.
        * Remember that the column index starts with 0.
        * @param    colEnd The last column to be written.
        * Remember that the column index starts with 0.
        * If -1, all the columns to the end are written.
        * @param    xPos The x-coordinate where the table starts on the canvas
        * @param    yPos The y-coordinate where the table starts on the canvas
         * @param   reusable if set to false, the content in the cells is "consumed";
         * if true, you can reuse the cells, the row, the parent table as many times you want.
         * @since 5.1.0 added the reusable parameter
         */
        virtual public void WriteCells(int colStart, int colEnd, float xPos, float yPos, PdfContentByte[] canvases, bool reusable) {
            if (!calculated)
                CalculateHeights();
            if (colEnd < 0)
                colEnd = cells.Length;
            else
                colEnd = Math.Min(colEnd, cells.Length);
            if (colStart < 0)
                colStart = 0;
            if (colStart >= colEnd)
                return;
            
            int newStart;
            for (newStart = colStart; newStart >= 0; --newStart) {
                if (cells[newStart] != null)
                    break;
                if (newStart > 0)
                    xPos -= widths[newStart - 1];
            }
            
            if (newStart < 0)
                newStart = 0;
            if (cells[newStart] != null)
                xPos -= cells[newStart].Left;

            if (IsTagged(canvases[PdfPTable.TEXTCANVAS]))
            {
                canvases[PdfPTable.TEXTCANVAS].OpenMCBlock(this);
            }
            for (int k = newStart; k < colEnd; ++k) {
                PdfPCell cell = cells[k];
                if (cell == null)
                    continue;
                if (IsTagged(canvases[PdfPTable.TEXTCANVAS]))
                {
                    canvases[PdfPTable.TEXTCANVAS].OpenMCBlock(cell);
                }
                float currentMaxHeight = maxHeight + extraHeights[k];
                
                WriteBorderAndBackground(xPos, yPos, currentMaxHeight, cell, canvases);

                Image img = cell.Image;
                
                float tly = cell.Top + yPos - cell.EffectivePaddingTop;
                if (cell.Height <= currentMaxHeight) {
                    switch (cell.VerticalAlignment) {
                    case Element.ALIGN_BOTTOM:
                        tly = cell.Top + yPos - currentMaxHeight + cell.Height
                                - cell.EffectivePaddingTop;
                        break;
                    case Element.ALIGN_MIDDLE:
                        tly = cell.Top + yPos + (cell.Height - currentMaxHeight) / 2
                                - cell.EffectivePaddingTop;
                        break;
                    }
                }
                if (img != null) {
                    if (cell.Rotation != 0) {
                        img = Image.GetInstance(img);
                        img.Rotation = img.GetImageRotation() + (float)(cell.Rotation * Math.PI / 180.0);
                    }
                    bool vf = false;
                    if (cell.Height > currentMaxHeight) {
                        if (!img.ScaleToFitHeight) {
                            continue;
                        }
                        img.ScalePercent(100);
                        float scale = (currentMaxHeight - cell.EffectivePaddingTop - cell
                                .EffectivePaddingBottom)
                                / img.ScaledHeight;
                        img.ScalePercent(scale * 100);
                        vf = true;
                    }
                    float left = cell.Left + xPos
                            + cell.EffectivePaddingLeft;
                    if (vf) {
                        switch (cell.HorizontalAlignment) {
                        case Element.ALIGN_CENTER:
                            left = xPos
                                    + (cell.Left + cell.EffectivePaddingLeft
                                            + cell.Right
                                            - cell.EffectivePaddingRight - img
                                            .ScaledWidth) / 2;
                            break;
                        case Element.ALIGN_RIGHT:
                            left = xPos + cell.Right
                                    - cell.EffectivePaddingRight
                                    - img.ScaledWidth;
                            break;
                        }
                        tly = cell.Top + yPos - cell.EffectivePaddingTop;
                    }
                    img.SetAbsolutePosition(left, tly - img.ScaledHeight);
                    if (IsTagged(canvases[PdfPTable.TEXTCANVAS]))
                    {
                        canvases[PdfPTable.TEXTCANVAS].OpenMCBlock(img);
                    }
                    canvases[PdfPTable.TEXTCANVAS].AddImage(img);
                    if (IsTagged(canvases[PdfPTable.TEXTCANVAS]))
                    {
                        canvases[PdfPTable.TEXTCANVAS].CloseMCBlock(img);
                    }
                } else {
                    // rotation sponsored by Connection GmbH
                    if (cell.Rotation == 90 || cell.Rotation == 270) {
                        float netWidth = currentMaxHeight - cell.EffectivePaddingTop - cell.EffectivePaddingBottom;
                        float netHeight = cell.Width - cell.EffectivePaddingLeft - cell.EffectivePaddingRight;
                        ColumnText ct = ColumnText.Duplicate(cell.Column);
                        ct.Canvases = canvases;
                        ct.SetSimpleColumn(0, 0, netWidth + 0.001f, -netHeight);
                        ct.Go(true);
                        float calcHeight = -ct.YLine;
                        if (netWidth <= 0 || netHeight <= 0)
                            calcHeight = 0;
                        if (calcHeight > 0) {
                            if (cell.UseDescender)
                                calcHeight -= ct.Descender;
                            if (reusable)
                                ct = ColumnText.Duplicate(cell.Column);
                            else
                                ct = cell.Column;
                            ct.Canvases = canvases;
                            ct.SetSimpleColumn(-0.003f, -0.001f, netWidth + 0.003f, calcHeight);
                            float pivotX;
                            float pivotY;
                            if (cell.Rotation == 90) {
                                pivotY = cell.Top + yPos - currentMaxHeight + cell.EffectivePaddingBottom;
                                switch (cell.VerticalAlignment) {
                                case Element.ALIGN_BOTTOM:
                                    pivotX = cell.Left + xPos + cell.Width - cell.EffectivePaddingRight;
                                    break;
                                case Element.ALIGN_MIDDLE:
                                    pivotX = cell.Left + xPos + (cell.Width + cell.EffectivePaddingLeft - cell.EffectivePaddingRight + calcHeight) / 2;
                                    break;
                                default: //top
                                    pivotX = cell.Left + xPos + cell.EffectivePaddingLeft + calcHeight;
                                    break;
                                }
                                SaveAndRotateCanvases(canvases, 0,1,-1,0,pivotX,pivotY);
                            }
                            else {
                                pivotY = cell.Top + yPos - cell.EffectivePaddingTop;
                                switch (cell.VerticalAlignment) {
                                case Element.ALIGN_BOTTOM:
                                    pivotX = cell.Left + xPos + cell.EffectivePaddingLeft;
                                    break;
                                case Element.ALIGN_MIDDLE:
                                    pivotX = cell.Left + xPos + (cell.Width + cell.EffectivePaddingLeft - cell.EffectivePaddingRight - calcHeight) / 2;
                                    break;
                                default: //top
                                    pivotX = cell.Left + xPos + cell.Width - cell.EffectivePaddingRight - calcHeight;
                                    break;
                                }
                                SaveAndRotateCanvases(canvases, 0,-1,1,0,pivotX,pivotY);
                            }
                            try {
                                ct.Go();
                            } finally {
                                RestoreCanvases(canvases);
                            }
                        }
                    } 
                    else {
                        float fixedHeight = cell.FixedHeight;
                        float rightLimit = cell.Right + xPos
                                - cell.EffectivePaddingRight;
                        float leftLimit = cell.Left + xPos
                                + cell.EffectivePaddingLeft;
                        if (cell.NoWrap) {
                            switch (cell.HorizontalAlignment) {
                                case Element.ALIGN_CENTER:
                                    rightLimit += 10000;
                                    leftLimit -= 10000;
                                    break;
                                case Element.ALIGN_RIGHT:
                                    if (cell.Rotation == 180) {
                                        rightLimit += RIGHT_LIMIT;
                                    }
                                    else {
                                        leftLimit -= RIGHT_LIMIT;
                                    }
                                    break;
                                default:
                                    if (cell.Rotation == 180) {
                                        leftLimit -= RIGHT_LIMIT;
                                    }
                                    else {
                                        rightLimit += RIGHT_LIMIT;
                                    }
                                    break;
                            }
                        }
                        ColumnText ct;
                        if (reusable)
                            ct = ColumnText.Duplicate(cell.Column);
                        else
                            ct = cell.Column;
                        ct.Canvases = canvases;
                        float bry = tly
                                - (currentMaxHeight
                                - cell.EffectivePaddingTop - cell.EffectivePaddingBottom);
                        if (fixedHeight > 0) {
                            if (cell.Height > currentMaxHeight) {
                                tly = cell.Top + yPos - cell.EffectivePaddingTop;
                                bry = cell.Top + yPos - currentMaxHeight + cell.EffectivePaddingBottom;
                            }
                        }
                        if ((tly > bry || ct.ZeroHeightElement()) && leftLimit < rightLimit) {
                            ct.SetSimpleColumn(leftLimit, bry - 0.001f, rightLimit, tly);
                            if (cell.Rotation == 180) {
                                float shx = leftLimit + rightLimit;
                                float shy = yPos + yPos - currentMaxHeight + cell.EffectivePaddingBottom - cell.EffectivePaddingTop;
                                SaveAndRotateCanvases(canvases, -1,0,0,-1,shx,shy);
                            }
                            try {
                                ct.Go();
                            } finally {
                                if (cell.Rotation == 180) {
                                    RestoreCanvases(canvases);
                                }
                            }
                        }
                    }
                }
                IPdfPCellEvent evt = cell.CellEvent;
                if (evt != null) {
                    Rectangle rect = new Rectangle(cell.Left + xPos, cell.Top
                            + yPos - currentMaxHeight, cell.Right + xPos, cell.Top
                            + yPos);
                    evt.CellLayout(cell, rect, canvases);
                }
                if (IsTagged(canvases[PdfPTable.TEXTCANVAS])) {
                    canvases[PdfPTable.TEXTCANVAS].CloseMCBlock(cell);
                }
		    }
            if (IsTagged(canvases[PdfPTable.TEXTCANVAS])) {
                canvases[PdfPTable.TEXTCANVAS].CloseMCBlock(this);
            }
        }
        
        /**
        * Checks if the dimensions of the columns were calculated.
        * 
        * @return true if the dimensions of the columns were calculated
        */
        virtual public bool IsCalculated() {
            return calculated;
        }

        /**
        * Gets the maximum height of the row (i.e. of the 'highest' cell).
        * @return the maximum height of the row
        */
        virtual public float MaxHeights {
            get {
                if (!calculated)
                    CalculateHeights();
                return maxHeight;
            }
            set {
                this.maxHeight = value;
            }
        }

        //end add

        internal float[] GetEventWidth(float xPos, float[] absoluteWidths) {
            int n = 1;
            for (int k = 0; k < cells.Length; ) {
                if (cells[k] != null) {
                    n++;
                    k += cells[k].Colspan;
                }
                else {
                    while (k < cells.Length && cells[k] == null) {
                        n++;
                        k++;
                    }
                }
            }
            float[] width = new float[n];
            width[0] = xPos;
            n = 1;
            for (int k = 0; k < cells.Length && n < width.Length; ) {
                if (cells[k] != null) {
                    int colspan = cells[k].Colspan;
                    width[n] = width[n - 1];
                    for (int i = 0; i < colspan && k < absoluteWidths.Length; i++) {
                        width[n] += absoluteWidths[k++];
                    }
                    n++;
                }
                else {
                    width[n] = width[n - 1];
                    while (k < cells.Length && cells[k] == null) {
                        width[n] += absoluteWidths[k++];
                    }
                    n++;
                }
            }
            return width;
        }

        /**
         * Copies the content of a specific row in a table to this row.
         * Don't do this if the rows have a different number of cells.
         * @param table the table from which you want to copy a row
         * @param idx   the index of the row that needs to be copied
         * @since 5.1.0
         */
        virtual public void CopyRowContent(PdfPTable table, int idx) {
            if (table == null) {
                return;
            }
            PdfPCell copy;
            for (int i = 0; i < cells.Length; ++i) {
                int lastRow = idx;
                copy = table.GetRow(lastRow).GetCells()[i];
                while (copy == null && lastRow > 0) {
                    copy = table.GetRow(--lastRow).GetCells()[i];
                }
                if (cells[i] != null && copy != null) {
                    cells[i].Column = copy.Column;
                    this.calculated = false;
                }
            }
        }

        /**
        * Splits a row to newHeight.
        * The returned row is the remainder. It will return null if the newHeight
        * was so small that only an empty row would result.
        * 
        * @param new_height the new height
        * @return the remainder row or null if the newHeight was so small that only
        * an empty row would result
        */
        virtual public PdfPRow SplitRow(PdfPTable table, int rowIndex, float new_height) {
            LOGGER.Info("Splitting " + rowIndex + " " + new_height);
            // second part of the row
            PdfPCell[] newCells = new PdfPCell[cells.Length];
            float[] calHs = new float[cells.Length];
            float[] fixHs = new float[cells.Length];
            float[] minHs = new float[cells.Length];
            bool allEmpty = true;
            // loop over all the cells
            for (int k = 0; k < cells.Length; ++k) {
                float newHeight = new_height;
                PdfPCell cell = cells[k];
                if (cell == null) {
                    int index = rowIndex;
                    if (table.RowSpanAbove(index, k)) {
                        while (table.RowSpanAbove(--index, k)) {
                            newHeight += table.GetRow(index).MaxHeights;
                        }
                        PdfPRow row = table.GetRow(index);
                        if (row != null && row.GetCells()[k] != null) {
                            newCells[k] = new PdfPCell(row.GetCells()[k]);
                            newCells[k].Column = null;
                            newCells[k].Rowspan = row.GetCells()[k].Rowspan - rowIndex + index;
                            allEmpty = false;
                        }
                    }
                    continue;
                }
                calHs[k] = cell.CalculatedHeight;
                fixHs[k] = cell.FixedHeight;
                minHs[k] = cell.MinimumHeight;
                Image img = cell.Image;
                PdfPCell newCell = new PdfPCell(cell);
                if (img != null) {
                    float padding = cell.EffectivePaddingBottom + cell.EffectivePaddingTop + 2;
                    if ((img.ScaleToFitHeight || img.ScaledHeight + padding < newHeight)
                        && newHeight > padding) {
                        newCell.Phrase = null;
                        allEmpty = false;
                    }
                }
                else {
                    float y;
                    ColumnText ct = ColumnText.Duplicate(cell.Column);
                    float left = cell.Left + cell.EffectivePaddingLeft;
                    float bottom = cell.Top + cell.EffectivePaddingBottom - newHeight;
                    float right = cell.Right - cell.EffectivePaddingRight;
                    float top = cell.Top - cell.EffectivePaddingTop;
                    switch (cell.Rotation) {
                        case 90:
                        case 270:
                            y = SetColumn(ct, bottom, left, top, right);
                            break;
                        default:
                            y = SetColumn(ct, left, bottom + 0.00001f, cell.NoWrap ? RIGHT_LIMIT : right, top);
                            break;
                    }
                    int status = ct.Go(true);
                    bool thisEmpty = (ct.YLine == y);
                    if (thisEmpty) {
                        newCell.Column = ColumnText.Duplicate(cell.Column);
                        ct.FilledWidth = 0;
                    }
                    else if ((status & ColumnText.NO_MORE_TEXT) == 0) {
                        newCell.Column = ct;
                        ct.FilledWidth = 0;
                    }
                    else
                        newCell.Phrase = null;
                    allEmpty = (allEmpty && thisEmpty);
                }
                newCells[k] = newCell;
                cell.CalculatedHeight = newHeight;
            }
            if (allEmpty) {
                for (int k = 0; k < cells.Length; ++k) {
                    PdfPCell cell = cells[k];
                    if (cell == null) {
                        continue;
                    }

                    cell.CalculatedHeight = calHs[k];

                    if (fixHs[k] > 0)
                        cell.FixedHeight = fixHs[k];
                    else
                        cell.MinimumHeight = minHs[k];
                }
                return null;
            }
            CalculateHeights();
            PdfPRow split = new PdfPRow(newCells, this);
            split.widths = (float[]) widths.Clone();
            return split;
        }


        // Contributed by Deutsche Bahn Systel GmbH (Thorsten Seitz), splitting row spans
        virtual public float GetMaxRowHeightsWithoutCalculating() {
            return maxHeight;
        }

        // Contributed by Deutsche Bahn Systel GmbH (Thorsten Seitz), splitting row spans
        virtual public void SetFinalMaxHeights(float maxHeight) {
            MaxHeights = maxHeight;
            calculated = true; // otherwise maxHeight would be recalculated in getter
        }

        // Contributed by Deutsche Bahn Systel GmbH (Thorsten Seitz), splitting row spans
        /**
         * Split rowspan of cells with rowspan on next page by inserting copies with the remaining rowspan
         * and reducing the previous rowspan appropriately, i.e. if a cell with rowspan 7 gets split after 3 rows
         * of that rowspan have been laid out, its column on the next page should start with an empty cell
         * having the same attributes and rowspan 7 - 3 = 4.
         * 
         * @since iText 5.4.3
         */
        virtual public void SplitRowspans(PdfPTable original, int originalIdx, PdfPTable part, int partIdx) {
            if(original == null || part == null) {
                return;
            }
            int i = 0;
            while(i < cells.Length) {
                if(cells[i] == null) {
                    int splittedRowIdx = original.GetCellStartRowIndex(originalIdx, i);
                    int copyRowIdx = part.GetCellStartRowIndex(partIdx, i);
                    PdfPCell splitted = original.GetRow(splittedRowIdx)
                            .GetCells()[i]; // need this to reduce its rowspan
                    PdfPCell copy = part.GetRow(copyRowIdx)
                            .GetCells()[i]; // need this for (partially) consumed ColumnText
                    if(splitted != null) {
                        System.Diagnostics.Debug.Assert(copy != null); // both null or none
                        cells[i] = new PdfPCell(copy);
                        int rowspanOnPreviousPage = partIdx - copyRowIdx + 1;
                        cells[i].Rowspan = copy.Rowspan - rowspanOnPreviousPage;
                        splitted.Rowspan = rowspanOnPreviousPage;
                        this.calculated = false;
                    }
                    ++i;
                }
                else {
                    i += cells[i].Colspan;
                }
            }
        }


        /**
        * Returns the array of cells in the row.
        * Please be extremely careful with this method.
        * Use the cells as read only objects.
        * 
        * @return   an array of cells
        * @since    2.1.1
        */
        virtual public PdfPCell[] GetCells() {
            return cells;
        }

        /**
         * Checks if a cell in the row has a rowspan greater than 1.
         * @since 5.1.0
         */
        virtual public bool HasRowspan() {
            for (int i = 0; i < cells.Length; i++) {
                if (cells[i] != null && cells[i].Rowspan > 1)
                    return true;
            }
            return false;
        }

        virtual public bool Adjusted {
            get { return adjusted; }
            set { adjusted = value; }
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

        static private bool IsTagged(PdfContentByte canvas) {
            return canvas != null && canvas.writer != null && canvas.writer.IsTagged();
        }

        public virtual bool IsInline {
            get { return false; }
        }
    }
}
