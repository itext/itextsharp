/*
 * $Id: FloatLayout.cs $
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

using System;
using System.Collections.Generic;
using iTextSharp.text.api;

namespace iTextSharp.text.pdf {

    public class FloatLayout {
        protected float maxY;

        protected float minY;

        protected float leftX;

        protected float rightX;

        virtual public float YLine {
            get { return yLine; }
            set { yLine = value; }
        }
        
        protected float yLine;

        protected float floatLeftX;

        protected float floatRightX;

        virtual public float FilledWidth {
            get { return filledWidth; }
            set { filledWidth = value; }
        }

        protected float filledWidth;

        public ColumnText compositeColumn;

        public List<IElement> content;

        protected readonly bool useAscender;

        public FloatLayout(List<IElement> elements, bool useAscender) {
            this.compositeColumn = new ColumnText(null);
            compositeColumn.UseAscender = useAscender;
            this.useAscender = useAscender;
            content = elements;
        }

        virtual public void SetSimpleColumn( float llx, float lly, float urx, float ury) {
            leftX = Math.Min(llx, urx);
            maxY = Math.Max(lly, ury);
            minY = Math.Min(lly, ury);
            rightX = Math.Max(llx, urx);
            floatLeftX = leftX;
            floatRightX = rightX;
            yLine = maxY;
            filledWidth = 0;
        }

        public int RunDirection {
            get { return compositeColumn.RunDirection; }
            set { compositeColumn.RunDirection = value; }
        }

        virtual public int Layout(PdfContentByte canvas, bool simulate) {
            compositeColumn.Canvas = canvas;
            int status = ColumnText.NO_MORE_TEXT;

            List<IElement> floatingElements = new List<IElement>();
            List<IElement> content = simulate ? new List<IElement>(this.content) : this.content;

            while (content.Count > 0) {
                if (content[0] is PdfDiv) {
                    PdfDiv floatingElement = (PdfDiv)content[0];
                    if (floatingElement.Float == PdfDiv.FloatType.LEFT || floatingElement.Float == PdfDiv.FloatType.RIGHT) {
                        floatingElements.Add(floatingElement);
                        content.RemoveAt(0);
                    } else {
                        if (floatingElements.Count > 0) {
                            status = FloatingLayout(floatingElements, simulate);
                            if ((status & ColumnText.NO_MORE_TEXT) == 0) {
                                break;
                            }
                        }

                        content.RemoveAt(0);

                        status = floatingElement.Layout(canvas, useAscender, true, floatLeftX, minY, floatRightX, yLine);

                        if (floatingElement.KeepTogether && (status & ColumnText.NO_MORE_TEXT) == 0)
                        {
                            //check for empty page
                            if (compositeColumn.Canvas.PdfDocument.currentHeight > 0 || yLine != maxY) {
                                content.Insert(0,floatingElement);
                                break;
                            }
                        }

                        if (!simulate) {
                            canvas.OpenMCBlock(floatingElement);
                            status = floatingElement.Layout(canvas, useAscender, simulate, floatLeftX, minY, floatRightX, yLine);
                            canvas.CloseMCBlock(floatingElement);
                        }

                        if (floatingElement.getActualWidth() > filledWidth) {
                            filledWidth = floatingElement.getActualWidth();
                        }
                        if ((status & ColumnText.NO_MORE_TEXT) == 0) {
                            content.Insert(0, floatingElement);
                            yLine = floatingElement.YLine;
                            break;
                        } else {
                            yLine -= floatingElement.getActualHeight();
                        }
                    }
                } else {
                    floatingElements.Add(content[0]);
                    content.RemoveAt(0);
                }
            }

            if ((status & ColumnText.NO_MORE_TEXT) != 0 && floatingElements.Count > 0) {
                status = FloatingLayout(floatingElements, simulate);
            }

            content.InsertRange(0, floatingElements);

            return status;
        }

        private int FloatingLayout(List<IElement> floatingElements, bool simulate) {
            int status = ColumnText.NO_MORE_TEXT;
            float minYLine = yLine;
            float leftWidth = 0;
            float rightWidth = 0;

            ColumnText currentCompositeColumn = compositeColumn;
            if (simulate) {
                currentCompositeColumn = ColumnText.Duplicate(compositeColumn);
            }

            bool ignoreSpacingBefore = maxY == yLine;

            while (floatingElements.Count > 0) {
                IElement nextElement = floatingElements[0];
                floatingElements.RemoveAt(0);
                if (nextElement is PdfDiv) {
                    PdfDiv floatingElement = (PdfDiv) nextElement;
                    status = floatingElement.Layout(compositeColumn.Canvas, useAscender, true, floatLeftX, minY, floatRightX, yLine);
                    if ((status & ColumnText.NO_MORE_TEXT) == 0) {
                        yLine = minYLine;
                        floatLeftX = leftX;
                        floatRightX = rightX;
                        status = floatingElement.Layout(compositeColumn.Canvas, useAscender, true, floatLeftX, minY, floatRightX, yLine);
                        if ((status & ColumnText.NO_MORE_TEXT) == 0) {
                            floatingElements.Insert(0, floatingElement);
                            break;
                        }
                    }
                    if (floatingElement.Float == PdfDiv.FloatType.LEFT) {
                        status = floatingElement.Layout(compositeColumn.Canvas, useAscender, simulate, floatLeftX, minY, floatRightX, yLine);
                        floatLeftX += floatingElement.getActualWidth();
                        leftWidth += floatingElement.getActualWidth();
                    } else if (floatingElement.Float == PdfDiv.FloatType.RIGHT) {
                        status = floatingElement.Layout(compositeColumn.Canvas, useAscender, simulate, floatRightX - floatingElement.getActualWidth() - 0.01f, minY, floatRightX, yLine);
                        floatRightX -= floatingElement.getActualWidth();
                        rightWidth += floatingElement.getActualWidth();
                    }
                    minYLine = Math.Min(minYLine, yLine - floatingElement.getActualHeight());
                } else {
                    if (minY > minYLine) {
                        status = ColumnText.NO_MORE_COLUMN;
                        floatingElements.Insert(0, nextElement);
                        if (currentCompositeColumn != null)
                            currentCompositeColumn.SetText(null);
                        break;
                    } else {
                        if (nextElement is ISpaceable && (!ignoreSpacingBefore || !currentCompositeColumn.IgnoreSpacingBefore || ((ISpaceable)nextElement).PaddingTop != 0))
                        {
                            yLine -= ((ISpaceable) nextElement).SpacingBefore;
                        }
                        if (simulate) {
                            if (nextElement is PdfPTable)
                                currentCompositeColumn.AddElement(new PdfPTable((PdfPTable) nextElement));
                            else
                                currentCompositeColumn.AddElement(nextElement);
                        } else {
                            currentCompositeColumn.AddElement(nextElement);
                        }

                        if (yLine > minYLine)
                            currentCompositeColumn.SetSimpleColumn(floatLeftX, yLine, floatRightX, minYLine);
                        else
                            currentCompositeColumn.SetSimpleColumn(floatLeftX, yLine, floatRightX, minY);

                        currentCompositeColumn.FilledWidth = 0;

                        status = currentCompositeColumn.Go(simulate);
                        if (yLine > minYLine && (floatLeftX > leftX || floatRightX < rightX) &&
                            (status & ColumnText.NO_MORE_TEXT) == 0) {
                            yLine = minYLine;
                            floatLeftX = leftX;
                            floatRightX = rightX;
                            if (leftWidth != 0 && rightWidth != 0) {
                                filledWidth = rightX - leftX;
                            } else {
                                if (leftWidth > filledWidth) {
                                    filledWidth = leftWidth;
                                }
                                if (rightWidth > filledWidth) {
                                    filledWidth = rightWidth;
                                }
                            }

                            leftWidth = 0;
                            rightWidth = 0;
                            if (simulate && nextElement is PdfPTable) {
                                currentCompositeColumn.AddElement(new PdfPTable((PdfPTable) nextElement));
                            }

                            currentCompositeColumn.SetSimpleColumn(floatLeftX, yLine, floatRightX, minY);
                            status = currentCompositeColumn.Go(simulate);
                            minYLine = currentCompositeColumn.YLine + currentCompositeColumn.Descender;
                            yLine = minYLine;
                            if (currentCompositeColumn.FilledWidth > filledWidth) {
                                filledWidth = currentCompositeColumn.FilledWidth;
                            }
                        } else {
                            if (rightWidth > 0) {
                                rightWidth += currentCompositeColumn.FilledWidth;
                            } else if (leftWidth > 0) {
                                leftWidth += currentCompositeColumn.FilledWidth;
                            } else if (currentCompositeColumn.FilledWidth > filledWidth) {
                                filledWidth = currentCompositeColumn.FilledWidth;
                            }
                            minYLine = Math.Min(currentCompositeColumn.YLine + currentCompositeColumn.Descender, minYLine);
                            yLine = currentCompositeColumn.YLine + currentCompositeColumn.Descender;
                        }

                        if ((status & ColumnText.NO_MORE_TEXT) == 0) {
                            if (!simulate) {
                                floatingElements.InsertRange(0, currentCompositeColumn.CompositeElements);
                                currentCompositeColumn.CompositeElements.Clear();
                            } else {
                                floatingElements.Insert(0, nextElement);
                                currentCompositeColumn.SetText(null);
                            }
                            break;
                        } else {
                            currentCompositeColumn.SetText(null);
                        }
                    }
                }
               if (nextElement is Paragraph) {
                Paragraph p = (Paragraph) nextElement;
                foreach (IElement e in p) {
                    if (e is WritableDirectElement) {
                        WritableDirectElement writableElement = (WritableDirectElement) e;
                        if (writableElement.DirectElemenType == WritableDirectElement.DIRECT_ELEMENT_TYPE_HEADER && !simulate) {
                            PdfWriter writer = compositeColumn.Canvas.PdfWriter;
                            PdfDocument doc = compositeColumn.Canvas.PdfDocument;

                            // here is used a little hack:
                            // writableElement.write() method implementation uses PdfWriter.getVerticalPosition() to create PdfDestination (see com.itextpdf.tool.xml.html.Header),
                            // so here we are adjusting document's currentHeight in order to make getVerticalPosition() return value corresponding to real current position
                            float savedHeight = doc.currentHeight;
                            doc.currentHeight = doc.Top - yLine - doc.indentation.indentTop;
                            writableElement.Write(writer, doc);
                            doc.currentHeight = savedHeight;
                        }
                    }
                }
            } 
                if (ignoreSpacingBefore && nextElement.Chunks.Count == 0)
                {
                    if (nextElement is Paragraph)
                    {
                        Paragraph p = (Paragraph)nextElement;
                        IElement e = p[0];
                        if (e is WritableDirectElement)
                        {
                            WritableDirectElement writableDirectElement = (WritableDirectElement)e;
                            if (writableDirectElement.DirectElemenType != WritableDirectElement.DIRECT_ELEMENT_TYPE_HEADER)
                                ignoreSpacingBefore = false;
                        }
                    }
                    else if (nextElement is ISpaceable)
                        ignoreSpacingBefore = false;
                }
                else               
                    ignoreSpacingBefore = false;
                
            }

            if (leftWidth != 0 && rightWidth != 0) {
                filledWidth = rightX - leftX;
            } else {
                if (leftWidth > filledWidth) {
                    filledWidth = leftWidth;
                }
                if (rightWidth > filledWidth) {
                    filledWidth = rightWidth;
                }
            }

            yLine = minYLine;
            floatLeftX = leftX;
            floatRightX = rightX;

            return status;
        }
    }
}
