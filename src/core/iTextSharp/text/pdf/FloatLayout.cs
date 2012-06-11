/*
 * $Id: FloatLayout.cs $
 * 
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2012 1T3XT BVBA
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

using System;
using System.Collections.Generic;
using iTextSharp.text.api;

namespace iTextSharp.text.pdf {

    public class FloatLayout {
        protected float maxY;

        protected float minY;

        protected float leftX;

        protected float rightX;

        public float getYLine() {
            return yLine;
        }

        public void setYLine(float yLine) {
            this.yLine = yLine;
        }

        protected float yLine;

        protected float floatLeftX;

        protected float floatRightX;

        public float getFilledWidth() {
            return filledWidth;
        }

        public void setFilledWidth(float filledWidth) {
            this.filledWidth = filledWidth;
        }

        protected float filledWidth;

        protected ColumnText compositeColumn;

        protected List<IElement> content;

        public FloatLayout(ColumnText compositeColumn, List<IElement> elements) {
            this.compositeColumn = ColumnText.Duplicate(compositeColumn);
            content = elements;
        }

        public FloatLayout(PdfContentByte canvas, List<IElement> elements) {
            this.compositeColumn = new ColumnText(canvas);
            compositeColumn.UseAscender = false;
            content = elements;
        }

        public void SetSimpleColumn(float llx, float lly, float urx, float ury) {
            leftX = Math.Min(llx, urx);
            maxY = Math.Max(lly, ury);
            minY = Math.Min(lly, ury);
            rightX = Math.Max(llx, urx);
            floatLeftX = leftX;
            floatRightX = rightX;
            yLine = maxY;
            filledWidth = 0;
        }

        public int layout(bool simulate) {
            int status = ColumnText.NO_MORE_TEXT;
            filledWidth = 0;

            List<IElement> floatingElements = new List<IElement>();

            while (content.Count > 0) {
                if (content[0] is PdfDiv) {
                    PdfDiv floatingElement = (PdfDiv)content[0];
                    if (floatingElement.Float == PdfDiv.FloatType.LEFT || floatingElement.Float == PdfDiv.FloatType.RIGHT) {
                        floatingElements.Add(floatingElement);
                        content.RemoveAt(0);
                    } else {
                        if (floatingElements.Count > 0) {
                            status = floatingLayout(floatingElements, simulate);
                            //if ((status & ColumnText.NO_MORE_TEXT) == 0) {
                            //    break;
                            //}
                        }

                        status = floatingElement.layout(compositeColumn, true, floatLeftX, minY, floatRightX, yLine);

                        if (!simulate) {
                            status = floatingElement.layout(compositeColumn, simulate, floatLeftX, minY, floatRightX, yLine);
                        }

                        yLine -= floatingElement.getActualHeight();

                        if (floatingElement.getActualWidth() > filledWidth) {
                            filledWidth = floatingElement.getActualWidth();
                        }
                        //if ((status & ColumnText.NO_MORE_TEXT) == 0) {
                        //    break;
                        //}
                        content.RemoveAt(0);
                    }
                } else {
                    floatingElements.Add(content[0]);
                    content.RemoveAt(0);
                }
            }

            if (/*(status & ColumnText.NO_MORE_TEXT) != 0 && */floatingElements.Count > 0) {
                status = floatingLayout(floatingElements, simulate);
            }

            //foreach (IElement floatingElement in floatingElements) {
            //    content.Add(floatingElement);    
            //}


            return status;
        }

        private int floatingLayout(List<IElement> floatingElements, bool simulate) {
            int status = ColumnText.NO_MORE_TEXT;
            float minYLine = yLine;
            float leftWidth = 0;
            float rightWidth = 0;

            while (floatingElements.Count > 0) {
                if (floatingElements[0] is PdfDiv) {
                    PdfDiv floatingElement = (PdfDiv)floatingElements[0];
                    status = floatingElement.layout(compositeColumn, true, floatLeftX, minY, floatRightX, yLine);
                    if ((status & ColumnText.NO_MORE_TEXT) == 0) {
                        yLine = minYLine;
                        floatLeftX = leftX;
                        floatRightX = rightX;
                        status = floatingElement.layout(compositeColumn, true, floatLeftX, minY, floatRightX, yLine);
                        //if ((status & ColumnText.NO_MORE_TEXT) == 0) {
                        //    break;
                        //}
                    }
                    if (floatingElement.Float == PdfDiv.FloatType.LEFT) {
                        status = floatingElement.layout(compositeColumn, simulate, floatLeftX, minY, floatRightX, yLine);
                        floatLeftX += floatingElement.getActualWidth();
                        leftWidth += floatingElement.getActualWidth();
                    } else if (floatingElement.Float == PdfDiv.FloatType.RIGHT) {
                        status = floatingElement.layout(compositeColumn, simulate, floatRightX - floatingElement.getActualWidth() - 0.01f, minY, floatRightX, yLine);
                        floatRightX -= floatingElement.getActualWidth();
                        rightWidth += floatingElement.getActualWidth();
                    }
                    minYLine = Math.Min(minYLine, yLine - floatingElement.getActualHeight());
                } else {
                    IElement firstElement = floatingElements[0];
                    if (firstElement is ISpaceable) {
                        yLine -= ((ISpaceable)firstElement).SpacingBefore;
                    }
                    compositeColumn.AddElement(firstElement);
                    if (yLine > minYLine)
                        compositeColumn.SetSimpleColumn(floatLeftX, yLine, floatRightX, minYLine);
                    else
                        compositeColumn.SetSimpleColumn(floatLeftX, yLine, floatRightX, minY);

                    compositeColumn.FilledWidth = 0;

                    status = compositeColumn.Go(simulate);
                    if (yLine > minYLine && (floatLeftX > leftX || floatRightX < rightX) && (status & ColumnText.NO_MORE_TEXT) == 0) {
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
                        compositeColumn.SetSimpleColumn(floatLeftX, yLine, floatRightX, minY);
                        status = compositeColumn.Go(simulate);
                        minYLine = compositeColumn.YLine + compositeColumn.Descender;
                        yLine = minYLine;
                        if (compositeColumn.FilledWidth > filledWidth) {
                            filledWidth = compositeColumn.FilledWidth;
                        }
                    } else {
                        if (rightWidth > 0) {
                            rightWidth += compositeColumn.FilledWidth;
                        } else if (leftWidth > 0) {
                            leftWidth += compositeColumn.FilledWidth;
                        } else if (compositeColumn.FilledWidth > filledWidth) {
                            filledWidth = compositeColumn.FilledWidth;
                        }
                        minYLine = Math.Min(compositeColumn.YLine + compositeColumn.Descender, minYLine);
                        yLine = compositeColumn.YLine + compositeColumn.Descender;
                    }

                    compositeColumn.CompositeElements.Clear();
                    //if ((status & ColumnText.NO_MORE_TEXT) == 0) {
                    //    break;
                    //}
                }

                floatingElements.RemoveAt(0);
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
