using System;
using iTextSharp.text;
/*
 * $Id: PdfPRow.java 3913 2009-04-26 08:51:11Z blowagie $
 *
 * Copyright 2001, 2002 Paulo Soares
 *
 * The contents of this file are subject to the Mozilla Public License Version 1.1
 * (the "License"); you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the License.
 *
 * The Original Code is 'iText, a free JAVA-PDF library'.
 *
 * The Initial Developer of the Original Code is Bruno Lowagie. Portions created by
 * the Initial Developer are Copyright (C) 1999, 2000, 2001, 2002 by Bruno Lowagie.
 * All Rights Reserved.
 * Co-Developer of the code is Paulo Soares. Portions created by the Co-Developer
 * are Copyright (C) 2000, 2001, 2002 by Paulo Soares. All Rights Reserved.
 *
 * Contributor(s): all the names of the contributors are added in the source code
 * where applicable.
 *
 * Alternatively, the contents of this file may be used under the terms of the
 * LGPL license (the "GNU LIBRARY GENERAL PUBLIC LICENSE"), in which case the
 * provisions of LGPL are applicable instead of those above.  If you wish to
 * allow use of your version of this file only under the terms of the LGPL
 * License and not to allow others to use your version of this file under
 * the MPL, indicate your decision by deleting the provisions above and
 * replace them with the notice and other provisions required by the LGPL.
 * If you do not delete the provisions above, a recipient may use your version
 * of this file under either the MPL or the GNU LIBRARY GENERAL PUBLIC LICENSE.
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the MPL as stated above or under the terms of the GNU
 * Library General Public License as published by the Free Software Foundation;
 * either version 2 of the License, or any later version.
 *
 * This library is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU Library general Public License for more
 * details.
 *
 * If you didn't download this code from the following link, you should check if
 * you aren't using an obsolete version:
 * http://www.lowagie.com/iText/
 */

namespace iTextSharp.text.pdf {

    /**
    * A row in a PdfPTable.
    * 
    * @author Paulo Soares (psoares@consiste.pt)
    */
    public class PdfPRow {

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

        protected float maxHeight = 0;
        
        protected bool calculated = false;
        
        private int[] canvasesPos;
        
        /**
        * Constructs a new PdfPRow with the cells in the array that was passed
        * as a parameter.
        * 
        * @param cells
        */
        public PdfPRow(PdfPCell[] cells) {
            this.cells = cells;
            widths = new float[cells.Length];
            InitExtraHeights();
        }

        /**
        * Makes a copy of an existing row.
        * 
        * @param row
        */
        public PdfPRow(PdfPRow row) {
            maxHeight = row.maxHeight;
            calculated = row.calculated;
            cells = new PdfPCell[row.cells.Length];
            for (int k = 0; k < cells.Length; ++k) {
                if (row.cells[k] != null)
                    cells[k] = new PdfPCell(row.cells[k]);
            }
            widths = new float[cells.Length];
            System.Array.Copy(row.widths, 0, widths, 0, cells.Length);
            InitExtraHeights();
        }

        /**
        * Sets the widths of the columns in the row.
        * 
        * @param widths
        * @return true if everything went right
        */
        public bool SetWidths(float[] widths) {
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
        public void InitExtraHeights() {
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
        public void SetExtraHeight(int cell, float height) {
            if (cell < 0 || cell >= cells.Length)
                return;
            extraHeights[cell] = height;
        }
        
        /**
        * Calculates the heights of each cell in the row.
        * 
        * @return the maximum height of the row.
        */
        public float CalculateHeights() {
            maxHeight = 0;
            for (int k = 0; k < cells.Length; ++k) {
                PdfPCell cell = cells[k];
                float height = 0;
                if (cell == null) {
                    continue;
                }
                else {
                    bool pivoted = (cell.Rotation == 90 || cell.Rotation == 270);
                    Image img = cell.Image;
                    if (img != null) {
                        img.ScalePercent(100);
                        float refWidth = pivoted ? img.ScaledHeight : img.ScaledWidth;
                        float scale = (cell.Right - cell.EffectivePaddingRight
                            - cell.EffectivePaddingLeft - cell.Left) / refWidth;
                        img.ScalePercent(scale * 100);
                        float refHeight = pivoted ? img.ScaledWidth : img.ScaledHeight;
                        cell.Bottom = cell.Top - cell.EffectivePaddingTop
                            - cell.EffectivePaddingBottom - refHeight;
                    }
                    else {
                        if (pivoted && cell.HasFixedHeight())
                            cell.Bottom = cell.Top - cell.FixedHeight;
                        else {
                            ColumnText ct = ColumnText.Duplicate(cell.Column);
                            float right, top, left, bottom;
                            if (pivoted) {
                                right = RIGHT_LIMIT;
                                top = cell.Right - cell.EffectivePaddingRight;
                                left = 0;
                                bottom = cell.Left + cell.EffectivePaddingLeft;
                            }
                            else {
                                right = cell.NoWrap ? RIGHT_LIMIT : cell.Right - cell.EffectivePaddingRight;
                                top = cell.Top - cell.EffectivePaddingTop;
                                left = cell.Left + cell.EffectivePaddingLeft;
                                bottom = cell.HasFixedHeight() ? top + cell.EffectivePaddingBottom - cell.FixedHeight : BOTTOM_LIMIT;
                            }
                            SetColumn(ct, left, bottom, right, top);
                            ct.Go(true);
                            if (pivoted)
                                cell.Bottom = cell.Top - cell.EffectivePaddingTop - cell.EffectivePaddingBottom - ct.FilledWidth;
                            else {
                                float yLine = ct.YLine;
                                if (cell.UseDescender)
                                    yLine += ct.Descender;
                                cell.Bottom = yLine - cell.EffectivePaddingBottom;
                            }
                        }
                    }
                    height = cell.FixedHeight;
                    if (height <= 0)
                        height = cell.Height;
                    if (height < cell.FixedHeight)
                        height = cell.FixedHeight;
                    else if (height < cell.MinimumHeight)
                        height = cell.MinimumHeight;
                    if ((height > maxHeight) && (cell.Rowspan == 1))
                        maxHeight = height;
                }
            }
            calculated = true;
            return maxHeight;
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
        public void WriteBorderAndBackground(float xPos, float yPos, float currentMaxHeight, PdfPCell cell, PdfContentByte[] canvases) {
            Color background = cell.BackgroundColor;
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
        protected void SaveAndRotateCanvases(PdfContentByte[] canvases, float a, float b, float c, float d, float e, float f) {
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
        protected void RestoreCanvases(PdfContentByte[] canvases) {
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
        * @since    2.1.6 private is now protected
        */
        protected internal float SetColumn(ColumnText ct, float left, float bottom, float right, float top) {
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
        */
        public void WriteCells(int colStart, int colEnd, float xPos, float yPos, PdfContentByte[] canvases) {
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
            
            for (int k = newStart; k < colEnd; ++k) {
                PdfPCell cell = cells[k];
                if (cell == null)
                    continue;
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
                    default:
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
                        default:
                            break;
                        }
                        tly = cell.Top + yPos - cell.EffectivePaddingTop;
                    }
                    img.SetAbsolutePosition(left, tly - img.ScaledHeight);
                    canvases[PdfPTable.TEXTCANVAS].AddImage(img);
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
                            ct = ColumnText.Duplicate(cell.Column);
                            ct.Canvases = canvases;
                            ct.SetSimpleColumn(-0.001f, -0.001f, netWidth + 0.001f, calcHeight);
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
                        ColumnText ct = ColumnText.Duplicate(cell.Column);
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
            }
        }
        
        /**
        * Checks if the dimensions of the columns were calculated.
        * 
        * @return true if the dimensions of the columns were calculated
        */
        public bool IsCalculated() {
            return calculated;
        }

        /**
        * Gets the maximum height of the row (i.e. of the 'highest' cell).
        * @return the maximum height of the row
        */
        public float MaxHeights {
            get {
                if (calculated)
                    return maxHeight;
                else
                    return CalculateHeights();
            }
            set {
                this.maxHeight = value;
            }
        }

        //end add

        internal float[] GetEventWidth(float xPos) {
            int n = 0;
            for (int k = 0; k < cells.Length; ++k) {
                if (cells[k] != null)
                    ++n;
            }
            float[] width = new float[n + 1];
            n = 0;
            width[n++] = xPos;
            for (int k = 0; k < cells.Length; ++k) {
                if (cells[k] != null) {
                    width[n] = width[n - 1] + cells[k].Width;
                    ++n;
                }
            }
            return width;
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
        public PdfPRow SplitRow(PdfPTable table, int rowIndex, float new_height) {
            PdfPCell[] newCells = new PdfPCell[cells.Length];
            float[] fixHs = new float[cells.Length];
            float[] minHs = new float[cells.Length];
            bool allEmpty = true;
            for (int k = 0; k < cells.Length; ++k) {
                float newHeight = new_height;
                PdfPCell cell = cells[k];
                if (cell == null) {
                    int index = rowIndex;
                    if (table.RowSpanAbove(index, k)) {
                        newHeight += table.GetRowHeight(index);
                        while (table.RowSpanAbove(--index, k)) {
                            newHeight += table.GetRowHeight(index);
                        }
                        PdfPRow row = table.GetRow(index);
                        if (row != null && row.GetCells()[k] != null) {
                            newCells[k] = new PdfPCell(row.GetCells()[k]);
                            newCells[k].ConsumeHeight(newHeight);
                            newCells[k].Rowspan = row.GetCells()[k].Rowspan - rowIndex + index;
                            allEmpty = false;
                        }
                    }
                    continue;
                }
                fixHs[k] = cell.FixedHeight;
                minHs[k] = cell.MinimumHeight;
                Image img = cell.Image;
                PdfPCell newCell = new PdfPCell(cell);
                if (img != null) {
                    if (newHeight > cell.EffectivePaddingBottom + cell.EffectivePaddingTop + 2) {
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
                            y = SetColumn(ct, left, bottom, cell.NoWrap ? RIGHT_LIMIT : right, top);
                            break;
                    }
                    int status;
                    status = ct.Go(true);
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
                cell.FixedHeight = newHeight;
            }
            if (allEmpty) {
                for (int k = 0; k < cells.Length; ++k) {
                    PdfPCell cell = cells[k];
                    if (cell == null)
                        continue;
                    if (fixHs[k] > 0)
                        cell.FixedHeight = fixHs[k];
                    else
                        cell.MinimumHeight = minHs[k];
                }
                return null;
            }
            CalculateHeights();
            PdfPRow split = new PdfPRow(newCells);
            split.widths = (float[]) widths.Clone();
            split.CalculateHeights();
            return split;
        }
        
        /**
        * Returns the array of cells in the row.
        * Please be extremely careful with this method.
        * Use the cells as read only objects.
        * 
        * @return   an array of cells
        * @since    2.1.1
        */
        public PdfPCell[] GetCells() {
            return cells;
        }
    }
}