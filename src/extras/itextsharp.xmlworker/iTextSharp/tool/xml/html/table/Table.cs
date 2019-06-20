/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
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
using System.Collections;
using System.Collections.Generic;
using System.util;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.css.apply;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html.pdfelement;
using iTextSharp.tool.xml.pipeline.html;
using iTextSharp.tool.xml.util;

namespace iTextSharp.tool.xml.html.table {

    /**
     * @author Emiel Ackermann
     *
     */
    public class Table : AbstractTagProcessor {

        public const float DEFAULT_CELL_BORDER_WIDTH = 0.75f;

        private static ILogger LOG = LoggerFactory.GetLogger(typeof(Table));
        private static CssUtils utils = CssUtils.GetInstance();
        private static FontSizeTranslator fst = FontSizeTranslator.GetInstance();

        /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.tool.xml.ITagProcessor#endElement(com.itextpdf.tool.xml.Tag,
         * java.util.List, com.itextpdf.text.Document)
         */
        public override IList<IElement> End(IWorkerContext ctx, Tag tag, IList<IElement> currentContent) {
            try {
			    bool percentage = false;
                String widthValue = null;
                tag.CSS.TryGetValue(HTML.Attribute.WIDTH, out widthValue);
                if (widthValue == null) {
                    tag.Attributes.TryGetValue(HTML.Attribute.WIDTH, out widthValue);
                }
			    if(widthValue != null && widthValue.Trim().EndsWith("%")) {
				    percentage = true;
			    }

                int numberOfColumns = 0;
                List<TableRowElement> tableRows = new List<TableRowElement>(currentContent.Count);
                IList<IElement> invalidRowElements = new List<IElement>(1);
                String repeatHeader;
                tag.CSS.TryGetValue(CSS.Property.REPEAT_HEADER, out repeatHeader);
                String repeatFooter;
                tag.CSS.TryGetValue(CSS.Property.REPEAT_FOOTER, out repeatFooter);
                int headerRows = 0;
                int footerRows = 0;
                foreach (IElement e in currentContent) {
                    int localNumCols = 0;
                    if (e is TableRowElement) {
                        TableRowElement tableRowElement = (TableRowElement) e;
                        foreach (HtmlCell cell in tableRowElement.Content) {
                            localNumCols += cell.Colspan;
                        }
                        if (localNumCols > numberOfColumns) {
                            numberOfColumns = localNumCols;
                        }
                        tableRows.Add(tableRowElement);
                        if (repeatHeader != null && Util.EqualsIgnoreCase(repeatHeader, "yes") && tableRowElement.RowPlace.Equals(TableRowElement.Place.HEADER)) {
                            headerRows++;
                        }
                        if (repeatFooter != null && Util.EqualsIgnoreCase(repeatFooter, "yes") && tableRowElement.RowPlace.Equals(TableRowElement.Place.FOOTER)){
                            footerRows++;
                        }
                    } else {
                        invalidRowElements.Add(e);
                    }
                }
                if(repeatFooter == null || !Util.EqualsIgnoreCase(repeatFooter, "yes")) {
                    SortUtil.MergeSort<TableRowElement>(tableRows, delegate(TableRowElement o1, TableRowElement o2) {
                        return o1.RowPlace.Normal.CompareTo(o2.RowPlace.Normal);
                    });
                } else {
                    SortUtil.MergeSort<TableRowElement>(tableRows, delegate(TableRowElement o1, TableRowElement o2) {
                        return o1.RowPlace.Repeated.CompareTo(o2.RowPlace.Repeated);
                    });
                }
                PdfPTable table = IntPdfPTable(numberOfColumns);
                table.HeaderRows = headerRows + footerRows;
                table.FooterRows = footerRows;

                if (tag.Attributes.ContainsKey(HTML.Attribute.ALIGN)) {
                    String value = tag.Attributes[HTML.Attribute.ALIGN];
                    // TODO this property is inverted when RTL. so we should counter-invert here, probably.
                    table.HorizontalAlignment = CSS.GetElementAlignment(value);
                }


                int direction = GetRunDirection(tag);

//                if (direction != PdfWriter.RUN_DIRECTION_DEFAULT) {
                    table.RunDirection = direction;
//                }
                foreach (KeyValuePair<String, String> entry in tag.CSS) {
				    if (Util.EqualsIgnoreCase(entry.Key,CSS.Property.PAGE_BREAK_INSIDE)) {
					    if (Util.EqualsIgnoreCase(entry.Value,CSS.Value.AVOID.ToLower())) {
						    table.KeepTogether = true;
					    }
				    }
			    }

                TableStyleValues styleValues = SetStyleValues(tag);
                table.TableEvent = new TableBorderEvent(styleValues);
                SetVerticalMargin(table, tag, styleValues, ctx);
                WidenLastCell(tableRows, styleValues.HorBorderSpacing);
                float[] columnWidths = new float[numberOfColumns];
                float[] widestWords = new float[numberOfColumns];
                float[] fixedWidths = new float[numberOfColumns];
                float[] colspanWidestWords = new float[numberOfColumns];
                int[] rowspanValue = new int[numberOfColumns];
                float largestColumn = 0;
                float largestColspanColumn = 0;
                int indexOfLargestColumn = -1;
                int indexOfLargestColspanColumn = -1;

                // Initial fill of the widths arrays
                foreach (TableRowElement row in tableRows) {
                    int column = 0;
                    foreach (HtmlCell cell in row.Content) {
                        // check whether the current column should be skipped due to a
                        // rowspan value of higher cell in this column.
                        // Contribution made by Arnost Havelka (Asseco): added while condition
                        while ((column < numberOfColumns) && (rowspanValue[column] > 0)) {
                            rowspanValue[column] = rowspanValue[column] - 1;
                            ++column;
                        }
                        // sets a rowspan counter for current column (counter not
                        // needed for last column).
                        if (cell.Rowspan > 1 && column != numberOfColumns - 1 && column < rowspanValue.Length) {
                            rowspanValue[column] = cell.Rowspan - 1;
                        }
                        int colspan = cell.Colspan;
                        if (cell.FixedWidth != 0) {
                            float fixedWidth = cell.FixedWidth + GetCellStartWidth(cell);
                            float colSpanWidthSum = 0;
                            int nonZeroColspanCols = 0;
                            // Contribution made by Arnost Havelka (Asseco) (modified)
                            for (int c = column; c < column + colspan && c < numberOfColumns; c++) {
                                colSpanWidthSum += fixedWidths[c];
                                if (fixedWidths[c] != 0)
                                    nonZeroColspanCols++;
                            }
                            for (int c = column; c < column + colspan && c < numberOfColumns; c++) {
                                if (fixedWidths[c] == 0) {
                                    fixedWidths[c] = (fixedWidth - colSpanWidthSum)/(colspan - nonZeroColspanCols);
                                    columnWidths[c] = (fixedWidth - colSpanWidthSum)/(colspan - nonZeroColspanCols);
                                }
                            }
                        }
                        if (cell.CompositeElements != null) {
                            float[] widthValues = SetCellWidthAndWidestWord(cell);
                            float cellWidth = widthValues[0] / colspan;
                            float widestWordOfCell = widthValues[1] / colspan;
                            for (int i = 0; i < colspan; i++) {
                                int c = column + i;
                                // Contribution made by Arnost Havelka (Asseco)
                                if (c >= numberOfColumns) {
                                    continue;
                                }
                                if (fixedWidths[c] == 0 && cellWidth > columnWidths[c]) {
                                    columnWidths[c] = cellWidth;
                                    if (colspan == 1) {
                                        if (cellWidth > largestColumn) {
                                            largestColumn = cellWidth;
                                            indexOfLargestColumn = c;
                                        }
                                    } else {
                                        if (cellWidth > largestColspanColumn) {
                                            largestColspanColumn = cellWidth;
                                            indexOfLargestColspanColumn = c;
                                        }
                                    }
                                }
                                if (colspan == 1) {
                                    if (widestWordOfCell > widestWords[c]) {
                                        widestWords[c] = widestWordOfCell;
                                    }
                                } else {
                                    if (widestWordOfCell > colspanWidestWords[c]) {
                                        colspanWidestWords[c] = widestWordOfCell;
                                    }
                                }
                            }
                        }
                        if (colspan > 1) {
                            if (LOG.IsLogging(Level.TRACE)) {
                                LOG.Trace(String.Format(LocaleMessages.GetInstance().GetMessage(LocaleMessages.COLSPAN), colspan));
                            }
                            column += colspan - 1;
                        }
                        column++;
                    }
                }

                if (indexOfLargestColumn == -1) {
                    indexOfLargestColumn = indexOfLargestColspanColumn;
                    if (indexOfLargestColumn == -1) {
                        indexOfLargestColumn = 0;
                    }

                    for (int column = 0; column < numberOfColumns; column++) {
                        widestWords[column] = colspanWidestWords[column];
                    }
                }
                float outerWidth = GetTableOuterWidth(tag, styleValues.HorBorderSpacing, ctx);
                float initialTotalWidth = GetTableWidth(columnWidths, 0);
    //          float targetWidth = calculateTargetWidth(tag, columnWidths, outerWidth, ctx);
                float targetWidth = 0;
                HtmlPipelineContext htmlPipelineContext = GetHtmlPipelineContext(ctx);
                float max = htmlPipelineContext.PageSize.Width - outerWidth;
                bool tableWidthFixed = false;
                if (tag.Attributes.ContainsKey(CSS.Property.WIDTH) || tag.CSS.ContainsKey(CSS.Property.WIDTH)) {
                    targetWidth = new WidthCalculator().GetWidth(tag, htmlPipelineContext.GetRootTags(), htmlPipelineContext.PageSize.Width, initialTotalWidth);
                    if (targetWidth > max) {
                        targetWidth = max;
                    }
                    tableWidthFixed = true;
                } else if (initialTotalWidth <= max) {
                    targetWidth = initialTotalWidth;
                } else if (null == tag.Parent || (null != tag.Parent && htmlPipelineContext.GetRootTags().Contains(tag.Parent.Name))) {
                    targetWidth = max;
                } else /* this table is an inner table and width adjustment is done in outer table */{
                    targetWidth = GetTableWidth(columnWidths, outerWidth);
                }
                float totalFixedColumnWidth = GetTableWidth(fixedWidths, 0);
                float targetPercentage = 0;
                if (totalFixedColumnWidth == initialTotalWidth) { // all column widths are fixed
                    targetPercentage = targetWidth / initialTotalWidth;
                    if (initialTotalWidth > targetWidth) {
                        for (int column = 0; column < columnWidths.Length; column++) {
                            columnWidths[column] *= targetPercentage;
                        }
                    } else if(tableWidthFixed && targetPercentage != 1){
                        for (int column = 0; column < columnWidths.Length; column++) {
                            columnWidths[column] *= targetPercentage;
                        }
                    }
                } else {
                    targetPercentage = (targetWidth - totalFixedColumnWidth) / (initialTotalWidth - totalFixedColumnWidth);
                    // Reduce width of columns if the columnWidth array + borders +
                    // paddings
                    // is too large for the given targetWidth.
                    if (initialTotalWidth > targetWidth) {
                        float leftToReduce = 0;
                        for (int column = 0; column < columnWidths.Length; column++) {
                            if (fixedWidths[column] == 0) {
                                // Reduce width of the column to its targetWidth, if
                                // widestWord of column still fits in the targetWidth of
                                // the
                                // column.
                                if (widestWords[column] <= columnWidths[column] * targetPercentage) {
                                    columnWidths[column] *= targetPercentage;
                                    // else take the widest word and calculate space
                                    // left to
                                    // reduce.
                                } else {
                                    columnWidths[column] = widestWords[column];
                                    leftToReduce += widestWords[column] - columnWidths[column] * targetPercentage;
                                }
                                // if widestWord of a column does not fit in the
                                // fixedWidth,
                                // set the column width to the widestWord.
                            } else if (fixedWidths[column] < widestWords[column]) {
                                columnWidths[column] = widestWords[column];
                                leftToReduce += widestWords[column] - fixedWidths[column];
                            }
                        }
                        if (leftToReduce != 0) {
                            // Reduce width of the column with the most text, if its
                            // widestWord still fits in the reduced column.
                            if (widestWords[indexOfLargestColumn] <= columnWidths[indexOfLargestColumn] - leftToReduce) {
                                columnWidths[indexOfLargestColumn] -= leftToReduce;
                            } else { // set all columns to their minimum, with the
                                        // widestWord array.
                                for (int column = 0; leftToReduce != 0 && column < columnWidths.Length; column++) {
                                    if (fixedWidths[column] == 0 && columnWidths[column] > widestWords[column]) {
                                        float difference = columnWidths[column] - widestWords[column];
                                        if (difference <= leftToReduce) {
                                            leftToReduce -= difference;
                                            columnWidths[column] = widestWords[column];
                                        } else {
                                            columnWidths[column] -= leftToReduce;
                                            leftToReduce = 0;
                                        }
                                    }
                                }
                                if (leftToReduce != 0) {
                                    // If the table has an insufficient fixed width
                                    // by
                                    // an
                                    // attribute or style, try to enlarge the table
                                    // to
                                    // its
                                    // minimum width (= widestWords array).
                                    float pageWidth = GetHtmlPipelineContext(ctx).PageSize.Width;
                                    if (GetTableWidth(widestWords, outerWidth) < pageWidth) {
                                        targetWidth = GetTableWidth(widestWords, outerWidth);
                                        leftToReduce = 0;
                                    } else {
                                        // If all columnWidths are set to the
                                        // widestWordWidths and the table is still
                                        // to
                                        // wide
                                        // content will fall off the edge of a page,
                                        // which
                                        // is similar to HTML.
                                        targetWidth = pageWidth - outerWidth;
                                        leftToReduce = 0;
                                    }
                                }
                            }
                        }
                        // Enlarge width of columns to fit the targetWidth.
                    } else if (initialTotalWidth < targetWidth) {
                        for (int column = 0; column < columnWidths.Length; column++) {
                            if (fixedWidths[column] == 0) {
                                columnWidths[column] *= targetPercentage;
                            }
                        }
                    }
                }
                try {
                    table.SetTotalWidth(columnWidths);
                    table.LockedWidth = true;
                    table.DefaultCell.Border = Rectangle.NO_BORDER;
                } catch (DocumentException e) {
                    throw new RuntimeWorkerException(LocaleMessages.GetInstance().GetMessage(LocaleMessages.NO_CUSTOM_CONTEXT), e);
                }
                float? tableHeight = new HeightCalculator().GetHeight(tag, GetHtmlPipelineContext(ctx).PageSize.Height);
                float? tableRowHeight = null;
                if (tableHeight != null && tableHeight > 0)
                    tableRowHeight = tableHeight / tableRows.Count;
                int rowNumber = 0;
                foreach (TableRowElement row in tableRows) {
                    int columnNumber = -1;
                    float? computedRowHeight = null;
                    /*if ( tableHeight != null &&  tableRows.IndexOf(row) == tableRows.Count - 1) {
                        float computedTableHeigt = table.CalculateHeights();
                        computedRowHeight = tableHeight - computedTableHeigt;
                    }*/
                    IList<HtmlCell> rowContent = row.Content;
                    if(rowContent.Count < 1)
                        continue;
                    foreach (HtmlCell cell in rowContent) {
                        IList<IElement> compositeElements = cell.CompositeElements;
                        if (compositeElements != null) {
                            foreach (IElement baseLevel in compositeElements) {
                                if (baseLevel is PdfPTable) {
                                    TableStyleValues cellValues = cell.CellValues;
                                    float totalBordersWidth = cellValues.IsLastInRow ? styleValues.HorBorderSpacing * 2
                                            : styleValues.HorBorderSpacing;
                                    totalBordersWidth += cellValues.BorderWidthLeft + cellValues.BorderWidthRight;
                                    float columnWidth = 0;
                                    for (int currentColumnNumber = columnNumber + 1; currentColumnNumber <= columnNumber + cell.Colspan; currentColumnNumber++){
                                        columnWidth += columnWidths[currentColumnNumber];
                                    }
                                    IPdfPTableEvent tableEvent = ((PdfPTable) baseLevel).TableEvent;
                                    TableStyleValues innerStyleValues = ((TableBorderEvent) tableEvent).TableStyleValues;
                                    totalBordersWidth += innerStyleValues.BorderWidthLeft;
                                    totalBordersWidth += innerStyleValues.BorderWidthRight;
                                    ((PdfPTable) baseLevel).TotalWidth = columnWidth - totalBordersWidth;
                                }
                            }
                        }
                        columnNumber += cell.Colspan;

                        table.AddCell(cell);
                    }
                    table.CompleteRow();
                    if ((computedRowHeight == null || computedRowHeight <= 0) && tableRowHeight != null)
                        computedRowHeight = tableRowHeight;
                    if (computedRowHeight != null && computedRowHeight > 0) {
                        float rowHeight = table.GetRow(rowNumber).MaxHeights;
                        if (rowHeight < computedRowHeight) {
                            table.GetRow(rowNumber).MaxHeights = computedRowHeight.Value;
                        }
                        else if (tableRowHeight != null && tableRowHeight < rowHeight)
                        {
                            tableRowHeight = (tableHeight - rowHeight - rowNumber * tableRowHeight)
                                    / (tableRows.Count - rowNumber - 1);
                        }
                    }
                    rowNumber++;
                }
                if (percentage) {
				    table.WidthPercentage = utils.ParsePxInCmMmPcToPt(widthValue);
				    table.LockedWidth = false;
			    }
                List<IElement> elems = new List<IElement>();
                if (invalidRowElements.Count > 0) {
                    // all invalid row elements taken as caption
                    int i = 0;
                    Tag captionTag = tag.Children[i++];
                    while (!Util.EqualsIgnoreCase(captionTag.Name, HTML.Tag.CAPTION) && i < tag.Children.Count) {
                        captionTag = tag.Children[i];
                        i++;
                    }
                    String captionSideValue;
                    captionTag.CSS.TryGetValue(CSS.Property.CAPTION_SIDE, out captionSideValue);
                    if (captionSideValue != null && Util.EqualsIgnoreCase(captionSideValue, CSS.Value.BOTTOM)) {
                        elems.Add(table);
                        elems.AddRange(invalidRowElements);
                    } else {
                        elems.AddRange(invalidRowElements);
                        elems.Add(table);
                    }
                } else {
                    elems.Add(table);
                }
                return elems;
            } catch (NoCustomContextException e) {
                throw new RuntimeWorkerException(LocaleMessages.GetInstance().GetMessage(LocaleMessages.NO_CUSTOM_CONTEXT), e);
            }
        }

        protected internal virtual PdfPTable IntPdfPTable(int numberOfColumn) {
            PdfPTable table = new PdfPTable(numberOfColumn);

            table.HorizontalAlignment = Element.ALIGN_LEFT;
            table.SplitLate = false;

            return table;
        }

        /**
         * Calculates the target width. First checks:
         * <ol>
         * <li>if the attribute or style "width" is found in the given tag and it is not wider than pageWidth - outerWidth, then the
         * targetWidth = width value</li>
         * <li>if the columnWidths array in total is not wider than pageWidth - outerWidth, then the
         * targetWidth = the total of the columnWidths array</li>
         * <li>if table's parent is a root tag or table has no parent, then the
         * targetWidth = width of the page - outerWidth
         * {@link Table#getTableOuterWidth(Tag, float)}.</li>
         * </ol>
         * If none of the above is true, the width of the table is set to its
         * default with the columnWidths array.
         *
         * @param tag containing attributes and css.
         * @param columnWidths float[] containing the widest lines of text found in
         *            the columns.
         * @param outerWidth width needed for margins and borders.
         * @param ctx
         * @return float the target width of a table.
         * @throws NoCustomContextException
         */
        private float CalculateTargetWidth(Tag tag, float[] columnWidths, float outerWidth,
                IWorkerContext ctx) {
            float targetWidth = 0;
            HtmlPipelineContext htmlPipelineContext = GetHtmlPipelineContext(ctx);
            float max = htmlPipelineContext.PageSize.Width - outerWidth;
            float start = GetTableWidth(columnWidths, 0);
            if (tag.Attributes.ContainsKey(CSS.Property.WIDTH) || tag.CSS.ContainsKey(CSS.Property.WIDTH)) {
                targetWidth = new WidthCalculator().GetWidth(tag, htmlPipelineContext.GetRootTags(), htmlPipelineContext
                        .PageSize.Width);
                if (targetWidth > max) {
                    targetWidth = max;
                }
            } else if (start <= max) {
                targetWidth = start;
            } else if (null == tag.Parent
                    || (null != tag.Parent && htmlPipelineContext.GetRootTags().Contains(tag.Parent.Name))) {
                targetWidth = max;
            } else /*
                     * this table is an inner table and width adjustment is done in
                     * outer table
                     */{
                targetWidth = GetTableWidth(columnWidths, outerWidth);
            }
            return targetWidth;
        }

        /**
         * Adds horizontal border spacing to the right padding of the last cell of each row.
         * @param tableRows List of {@link TableRowElement} objects of the table.
         * @param horBorderSpacing float containing the horizontal border spacing of the table.
         */
        private void WidenLastCell(IList<TableRowElement> tableRows, float horBorderSpacing) {
            foreach (TableRowElement row in tableRows) {
                IList<HtmlCell> cells = row.Content;
                if(cells.Count < 1)
                    continue;
                HtmlCell last = cells[cells.Count - 1];
                last.CellValues.IsLastInRow = true;
                last.PaddingRight = last.PaddingRight + horBorderSpacing;
            }
        }

        /** Set the table style values in a {@link TableStyleValues} object based on attributes and css of the given tag.
         * @param tag containing attributes and css.
         * @return a {@link TableStyleValues} object containing the table's style values.
         */
        public static TableStyleValues SetStyleValues(Tag tag) {
            TableStyleValues styleValues = new TableStyleValues();
            IDictionary<String, String> css = tag.CSS;
            IDictionary<String, String> attributes = tag.Attributes;
            string v;
            if (attributes.ContainsKey(CSS.Property.BORDER)) {
                styleValues.BorderColor = BaseColor.BLACK;
                String borderValue = null;
                if (attributes.TryGetValue(CSS.Property.BORDER, out borderValue) && "".Equals(borderValue))
                    styleValues.BorderWidth = DEFAULT_CELL_BORDER_WIDTH;
                else
                    styleValues.BorderWidth = utils.ParsePxInCmMmPcToPt(borderValue);
            } else {
                foreach (KeyValuePair<String, String> entry in css) {
                    String key = entry.Key;
                    String value = entry.Value;
                    if (Util.EqualsIgnoreCase(key, CSS.Property.BORDER_LEFT_STYLE)
                        && Util.EqualsIgnoreCase(CSS.Value.SOLID, value)) {
                        styleValues.BorderColorLeft = BaseColor.BLACK;
                        styleValues.BorderWidthLeft = DEFAULT_CELL_BORDER_WIDTH;
                    } else if (Util.EqualsIgnoreCase(key, CSS.Property.BORDER_RIGHT_STYLE)
                        && Util.EqualsIgnoreCase(CSS.Value.SOLID, value)) {
                        styleValues.BorderColorRight = BaseColor.BLACK;
                        styleValues.BorderWidthRight = DEFAULT_CELL_BORDER_WIDTH;
                    } else if (Util.EqualsIgnoreCase(key,CSS.Property.BORDER_TOP_STYLE)
                        && Util.EqualsIgnoreCase(CSS.Value.SOLID, value)) {
                        styleValues.BorderColorTop = BaseColor.BLACK;
                        styleValues.BorderWidthTop = DEFAULT_CELL_BORDER_WIDTH;
                    }  else if (Util.EqualsIgnoreCase(key, CSS.Property.BORDER_BOTTOM_STYLE)
                        && Util.EqualsIgnoreCase(CSS.Value.SOLID, value)) {
                        styleValues.BorderColorBottom = BaseColor.BLACK;
                        styleValues.BorderWidthBottom = DEFAULT_CELL_BORDER_WIDTH;
                    }
                }
                
                String color = null;
                if (css.TryGetValue(CSS.Property.BORDER_BOTTOM_COLOR, out color)) {
			        styleValues.BorderColorBottom = HtmlUtilities.DecodeColor(color);
                }
                if (css.TryGetValue(CSS.Property.BORDER_TOP_COLOR, out color)) {
                    styleValues.BorderColorTop = HtmlUtilities.DecodeColor(color);
                }
                if (css.TryGetValue(CSS.Property.BORDER_LEFT_COLOR, out color)) {
                    styleValues.BorderColorLeft = HtmlUtilities.DecodeColor(color);
                }
                if (css.TryGetValue(CSS.Property.BORDER_RIGHT_COLOR, out color)) {
                    styleValues.BorderColorRight = HtmlUtilities.DecodeColor(color);
                }

                float? width = utils.CheckMetricStyle(css, CSS.Property.BORDER_BOTTOM_WIDTH);
                if (width != null) {
			        styleValues.BorderWidthBottom = (float)width;
                }
                width =  utils.CheckMetricStyle(css, CSS.Property.BORDER_TOP_WIDTH);
                if (width != null) {
                    styleValues.BorderWidthTop = (float)width;
                }
                width =  utils.CheckMetricStyle(css, CSS.Property.BORDER_RIGHT_WIDTH);
                if (width != null) {
                    styleValues.BorderWidthRight = (float) width;
                }
                width = utils.CheckMetricStyle(css, CSS.Property.BORDER_LEFT_WIDTH);
                if (width != null) {
                    styleValues.BorderWidthLeft = (float)width;
                }
            }

            css.TryGetValue(CSS.Property.BACKGROUND_COLOR, out v);
            styleValues.Background = HtmlUtilities.DecodeColor(v);
            styleValues.HorBorderSpacing = GetBorderOrCellSpacing(true, css, attributes);
            styleValues.VerBorderSpacing = GetBorderOrCellSpacing(false, css, attributes);
            return styleValues;
        }

        public static TableStyleValues SetBorderAttributeForCell(Tag tag) {
            TableStyleValues styleValues = new TableStyleValues();

            IDictionary<String, String> attributes = tag.Attributes;
            IDictionary<String, String> css = tag.CSS;
            String border = null;
            if (attributes.TryGetValue(CSS.Property.BORDER, out border) 
                && ("".Equals(border) || utils.ParsePxInCmMmPcToPt(border) > 0)) {
                styleValues.BorderColor = BaseColor.BLACK;
                styleValues.BorderWidth = DEFAULT_CELL_BORDER_WIDTH;
            }

            styleValues.HorBorderSpacing = GetBorderOrCellSpacing(true, css, attributes);
            styleValues.VerBorderSpacing = GetBorderOrCellSpacing(false, css, attributes);

            return styleValues;
        }

        /**
         * Extracts and parses the style border-spacing or the attribute cellspacing
         * of a table tag, if present. Favors the style border-spacing over the
         * attribute cellspacing. <br />
         * If style="border-collapse:collapse" is found in the css, the spacing is
         * always 0f. <br />
         * If no spacing is set, the default of 1.5pt is returned.
         *
         * @param getHor true for horizontal spacing, false for vertical spacing.
         * @param css of the table tag.
         * @param attributes of the table tag.
         * @return horizontal or vertical spacing between two cells or a cell and
         *         the border of the table.
         */
        static public float GetBorderOrCellSpacing(bool getHor, IDictionary<String, String> css, IDictionary<String, String> attributes) {
            float spacing = 0f;
            String collapse;

            css.TryGetValue(CSS.Property.BORDER_COLLAPSE, out collapse);
		if (collapse == null || collapse.Equals(CSS.Value.SEPARATE)) {
			String borderSpacing;
            css.TryGetValue(CSS.Property.BORDER_SPACING, out borderSpacing);
            String cellSpacing;
            attributes.TryGetValue(HTML.Attribute.CELLSPACING, out cellSpacing);
                if(borderSpacing != null) {
                    if(borderSpacing.Contains(" ")){
                        if(getHor) {
                            spacing = utils.ParsePxInCmMmPcToPt(borderSpacing.Split(' ')[0]);
                        } else {
                            spacing = utils.ParsePxInCmMmPcToPt(borderSpacing.Split(' ')[1]);
                        }
                    } else {
                        spacing = utils.ParsePxInCmMmPcToPt(borderSpacing);
                    }
                } else if (cellSpacing != null){
                    spacing = utils.ParsePxInCmMmPcToPt(cellSpacing);
                } else {
                    spacing = 2f * DEFAULT_CELL_BORDER_WIDTH;
                }
            } else if (collapse.Equals(CSS.Value.COLLAPSE)) {
                spacing = 0;
            }
            return spacing;
        }

        /**
         * Sets the default cell width and widest word of a cell.
         * <ul>
         * <li>cell width = {@link Table#getCellStartWidth(HtmlCell)} + the width of the widest line of text.</li>
         * <li>widest word = {@link Table#getCellStartWidth(HtmlCell)} + the widest word of the cell.</li>
         * </ul>
         * These 2 widths are used as the starting point when determining the width of the table in
         * @param cell HtmlCell of which the widths are needed.
         * @return float array containing the default cell width and the widest word.
         * <ul>
         * <li>float[0] = cell width.</li>
         * <li>float[1] = widest word.</li>
         * </ul>
         */
        private float[] SetCellWidthAndWidestWord(HtmlCell cell) {
            IList<float> rulesWidth = new List<float>();
            float widestWordOfCell = 0f;
            float startWidth = GetCellStartWidth(cell);
            float cellWidth;
            float widthDeviation = 0.001f;
            IList<IElement> compositeElements = cell.CompositeElements;
            if (compositeElements != null) {
                foreach (IElement baseLevel in compositeElements) {
                    cellWidth = float.NaN;
                    if (baseLevel is Phrase) {
                        for(int i = 0; i < ((Phrase)baseLevel).Count; i++) {
                            IElement inner = ((Phrase)baseLevel)[i];
                            if (inner is Chunk) {
                                if(float.IsNaN(cellWidth))
                                    cellWidth = startWidth + widthDeviation;
                                cellWidth += ((Chunk)inner).GetWidthPoint();
                                float widestWord = startWidth + widthDeviation + GetCssAppliers().ChunkCssAplier.GetWidestWord((Chunk)inner);
                                if(widestWord > widestWordOfCell) {
                                    widestWordOfCell = widestWord;
                                }
                            }
                        }
                        if(!float.IsNaN(cellWidth))
                            rulesWidth.Add(cellWidth);
                    } else if (baseLevel is List) {
                        foreach (IElement li in ((List)baseLevel).Items) {
                            cellWidth = startWidth + widthDeviation + ((ListItem)li).IndentationLeft;
                            foreach (Chunk c in li.Chunks) {
                                cellWidth += c.GetWidthPoint();
                                float widestWord = new ChunkCssApplier().GetWidestWord(c);
                                if(startWidth + widthDeviation + widestWord > widestWordOfCell) {
                                    widestWordOfCell = startWidth + widthDeviation + widestWord;
                                }
                            }
                            rulesWidth.Add(cellWidth);
                        }
                    } else if (baseLevel is PdfPTable) {
                        cellWidth = startWidth + widthDeviation + ((PdfPTable)baseLevel).TotalWidth;
                        foreach (PdfPRow innerRow in ((PdfPTable)baseLevel).Rows) {
                            int size = innerRow.GetCells().Length;
                            TableBorderEvent evente = (TableBorderEvent) ((PdfPTable)baseLevel).TableEvent;
                            TableStyleValues values = evente.TableStyleValues;
                            float minRowWidth = values.BorderWidthLeft+(size+1)*values.HorBorderSpacing+values.BorderWidthRight;
                            int celnr = 0;
                            foreach (PdfPCell innerCell in innerRow.GetCells()) {
                                celnr++;
                                if(innerCell != null) {
                                    float innerWidestWordOfCell = SetCellWidthAndWidestWord(new HtmlCell(innerCell, celnr == size))[1];
                                    minRowWidth += innerWidestWordOfCell;
                                }
                            }
                            if(minRowWidth > widestWordOfCell){
                                widestWordOfCell = minRowWidth;
                            }
                        }
                        rulesWidth.Add(cellWidth);
				    }
                    else if (baseLevel is PdfDiv) {
                        PdfDiv div = (PdfDiv) baseLevel;

                        float? divActualWidth = div.Width;

                        if (divActualWidth == null) {
                            divActualWidth = CalculateDivWidestElementWidth(div.Content);
                        }

                        cellWidth = startWidth + widthDeviation + (float)divActualWidth;
                        rulesWidth.Add(cellWidth);
                    }
                }
            }
            cellWidth = startWidth;
            foreach (float width in rulesWidth) {
                if(width > cellWidth) {
                    cellWidth = width;
                }
            }
            return new float[]{cellWidth, widestWordOfCell};
        }

        /**
         * An attempt to calculate a valid div width in case it is not fixed. It is used as alternative to
         * div.getActualWidth, which doesn't work here in case of not fixed div's width (it returns 0).
         *
         * This method is probably has to be improved in future.
         *
         * The main idea of this method is to return the widest element's width, so the created cell will be able to contain it.
         */

        private float CalculateDivWidestElementWidth(List<IElement> divContent) {
            float maxWidth = 0;
            foreach (IElement element in divContent) {
                float width = 0;
                // judging by the com.itextpdf.tool.xml.html.Div end() method, the div in XmlWorker can
 	            // contain only paragraph, table and another div
                if (element is PdfDiv) {
                    width = CalculateDivWidestElementWidth(((PdfDiv) element).Content);
                } else if (element is PdfPTable) {
                    width = ((PdfPTable)element).TotalWidth;
                } else if (element is Paragraph) {
                    Paragraph p = (Paragraph) element;
                    float widestWordOfParagraph = 0;

                    foreach (IElement inner in p) {
                        float widestWord = 0;
                        if (inner is Chunk) {
                            IDictionary<string, Object> chunkAttributes = ((Chunk)inner).Attributes;
                            if (chunkAttributes != null && chunkAttributes.ContainsKey(Chunk.IMAGE)) {
                                Object o;
                                chunkAttributes.TryGetValue(Chunk.IMAGE, out o);
                                if (o is Object[] && ((Object[]) o)[0] is text.Image) {
                                    widestWord = ((text.Image) ((Object[]) o)[0]).Width;
                                }
                            }
                            else {
                                widestWord = GetCssAppliers().ChunkCssAplier.GetWidestWord((Chunk)inner);
                            }
                        }

                        if (widestWord > widestWordOfParagraph) {
                            widestWordOfParagraph = widestWord;
                        }
                    }
                    width = widestWordOfParagraph;
                }

                if (width > maxWidth) {
                    maxWidth = width;
                }
            }
            return maxWidth;
        }

        /**
         * Calculates the total width based on the given widths array and the given
         * outer width.
         *
         * @param widths array of floats containing column width values.
         * @param outer width equals the required space outside of the table for
         *            margins and borders.
         * @return a table's width.
         * @throws NoCustomContextException
         */
        private float GetTableWidth(float[] widths, float outerWidth) {
            float width = 0;
            foreach (float f in widths) {
                width += f;
            }
            return width + outerWidth;
        }

        /**
         * Adds horizontal values of a table and its parent if present. Following values are added up:
         * <ul>
         * <li>left and right margins of the table.</li>
         * <li>left and right border widths of the table.</li>
         * <li>left and right margins of the parent of the table is present.</li>
         * <li>one horizontal border spacing.</li>
         * </ul>
         * @param tag
         * @param horBorderSpacing
         * @return
         * @throws NoCustomContextException
         */
        private float GetTableOuterWidth(Tag tag, float horBorderSpacing, IWorkerContext ctx) {
            float total = utils.GetLeftAndRightMargin(tag, GetHtmlPipelineContext(ctx).PageSize.Width)
                + utils.CheckMetricStyle(tag, CSS.Property.BORDER_LEFT_WIDTH)
                + utils.CheckMetricStyle(tag, CSS.Property.BORDER_RIGHT_WIDTH)
                + horBorderSpacing;
            Tag parent = tag.Parent;
            if (parent != null) {
                total += utils.GetLeftAndRightMargin(parent, GetHtmlPipelineContext(ctx).PageSize.Width);
            }
            return total;
        }

        /**
         * Calculates the start width of a cell. Following values are added up:
         * <ul>
         * <li>padding left, this includes left border width and a horizontal border spacing.</li>
         * <li>padding right, this includes right border width.</li>
         * <li>the (colspan - 1) * horizontal border spacing.</li>
         * </ul>
         * @param cell HtmlCell of which the start width is needed.
         * @return float containing the start width.
         */
        private float GetCellStartWidth(HtmlCell cell) {
            TableStyleValues cellStyleValues = cell.CellValues;
            // colspan - 1, because one horBorderSpacing has been added to paddingLeft for all cells.
            int spacingMultiplier = cell.Colspan - 1;
            float spacing = spacingMultiplier*cellStyleValues.HorBorderSpacing;
            return spacing + cell.PaddingLeft + cell.PaddingRight;
        }

        /**
         * Sets the top and bottom margin of the given table.
         *
         * @param table PdfPTable on which the margins need to be set.
         * @param t Tag containing the margin styles and font size if needed.
         * @param values {@link TableStyleValues} containing border widths and border spacing values.
         * @throws NoCustomContextException
         */
        private void SetVerticalMargin(PdfPTable table, Tag t, TableStyleValues values, IWorkerContext ctx)
        {
            float spacingBefore = values.BorderWidthTop;
            float spacingAfter = values.VerBorderSpacing + values.BorderWidthBottom;
            foreach (KeyValuePair<String, String> css in t.CSS) {
                String key = css.Key;
                String value = css.Value;
                if(Util.EqualsIgnoreCase(CSS.Property.MARGIN_TOP, key)) {
                    CssUtils utils = CssUtils.GetInstance();
				    spacingBefore += utils.CalculateMarginTop(value, fst.GetFontSize(t), GetHtmlPipelineContext(ctx));
                } else if (Util.EqualsIgnoreCase(CSS.Property.MARGIN_BOTTOM, key)) {
                    float marginBottom = utils.ParseValueToPt(value, fst.GetFontSize(t));
                    spacingAfter += marginBottom;
                    GetHtmlPipelineContext(ctx).GetMemory()[HtmlPipelineContext.LAST_MARGIN_BOTTOM] = marginBottom;
                } else if (Util.EqualsIgnoreCase(CSS.Property.PADDING_TOP, key))
                {
                    table.PaddingTop = utils.ParseValueToPt(value, fst.GetFontSize(t));
                }

            }
            table.SpacingBefore = spacingBefore;
            table.SpacingAfter = spacingAfter;
        }
        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.ITagProcessor#isStackOwner()
         */
        public override bool IsStackOwner() {
            return true;
        }


    }
}
