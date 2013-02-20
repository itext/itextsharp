/*
 * $Id: PdfDiv.cs $
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
using System.Drawing.Drawing2D;
using iTextSharp.text.api;
using iTextSharp.text.pdf.interfaces;

namespace iTextSharp.text.pdf {

    public class PdfDiv : IElement, ISpaceable, IAccessibleElement
    {
        public enum FloatType
        {
            NONE,
            LEFT,
            RIGHT
        };

        public enum PositionType
        {
            STATIC,
            ABSOLUTE,
            FIXED,
            RELATIVE
        };

        private List<IElement> content;

        private float? left = null;

        private float? top = null;

        private float? right = null;

        private float? bottom = null;

        private float? width = null;

        private float? height = null;

        private float? percentageHeight = null;

        private float? percentageWidth = null;

        private float contentWidth = 0;

        private float contentHeight = 0;

        private int textAlignment = Element.ALIGN_UNDEFINED;

        private float paddingLeft = 0;

        private float paddingRight = 0;

        private float paddingTop = 0;

        private float paddingBottom = 0;

        private BaseColor backgroundColor = null;

        /**
         * The spacing before the table.
         */
        protected float spacingBefore;

        /**
         * The spacing after the table.
         */
        protected float spacingAfter;

        private FloatType floatType = FloatType.NONE;

        private PositionType position = PositionType.STATIC;

        private FloatLayout floatLayout = null;

        protected PdfName role = PdfName.DIV;

        protected Dictionary<PdfName, PdfObject> accessibleAttributes = null;

        protected Guid id = Guid.NewGuid();


        public float? Left
        {
            get { return left; }
            set { left = value; }
        }

        public float? Top
        {
            get { return top; }
            set { top = value; }
        }

        public float? Right
        {
            get { return right; }
            set { right = value; }
        }

        public float? Bottom
        {
            get { return bottom; }
            set { bottom = value; }
        }

        public float? Width
        {
            get { return width; }
            set { width = value; }
        }

        public float? Height
        {
            get { return height; }
            set { height = value; }
        }

        public float? PercentageHeight
        {
            get { return percentageHeight; }
            set { percentageHeight = value; }
        }

        public float? PercentageWidth
        {
            get { return percentageWidth; }
            set { percentageWidth = value; }
        }

        public float ContentWidth
        {
            get { return contentWidth; }
            set { contentWidth = value; }
        }

        public float ContentHeight
        {
            get { return contentHeight; }
            set { contentHeight = value; }
        }

        public float getActualHeight()
        {
            return height != null && height >= contentHeight ? (float) height : contentHeight;
        }

        public float getActualWidth()
        {
            return width != null && width >= contentWidth ? (float) width : contentWidth;
        }

        public int TextAlignment
        {
            get { return textAlignment; }
            set { textAlignment = value; }
        }

        public float PaddingLeft
        {
            get { return paddingLeft; }
            set { paddingLeft = value; }
        }

        public float PaddingRight
        {
            get { return paddingRight; }
            set { paddingRight = value; }
        }

        public float PaddingTop
        {
            get { return paddingTop; }
            set { paddingTop = value; }
        }

        public float PaddingBottom
        {
            get { return paddingBottom; }
            set { paddingBottom = value; }
        }

        public FloatType Float
        {
            get { return floatType; }
            set { floatType = value; }
        }

        public PositionType Position
        {
            get { return position; }
            set { position = value; }
        }

        public FloatLayout FloatLayout
        {
            get { return floatLayout; }
            set { floatLayout = value; }
        }

        public BaseColor BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        public float SpacingBefore
        {
            get { return spacingBefore; }
            set { spacingBefore = value; }
        }

        public float SpacingAfter
        {
            get { return spacingAfter; }
            set { spacingAfter = value; }
        }

        public List<IElement> Content
        {
            get { return content; }
        }

        public PdfDiv()
        {
            content = new List<IElement>();
        }

        /**
         * Gets all the chunks in this element.
         *
         * @return	an <CODE>ArrayList</CODE>
         */

        public IList<Chunk> Chunks
        {
            get { return new List<Chunk>(); }
        }

        /**
         * Gets the type of the text element.
         *
         * @return	a type
         */

        public int Type
        {
            get { return Element.DIV; }
        }

        /**
         * @see com.itextpdf.text.Element#isContent()
         * @since	iText 2.0.8
         */
        public bool IsContent()
        {
            return true;
        }

        /**
         * @see com.itextpdf.text.Element#isNestable()
         * @since	iText 2.0.8
         */
        public bool IsNestable()
        {
            return true;
        }

        /**
         * Processes the element by adding it (or the different parts) to an
         * <CODE>ElementListener</CODE>.
         *
         * @param	listener	an <CODE>ElementListener</CODE>
         * @return	<CODE>true</CODE> if the element was processed successfully
         */
        public bool Process(IElementListener listener)
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

        public void AddElement(IElement element)
        {
            if (element is PdfPTable)
            {
                ((PdfPTable) element).SplitLate = false;
            }
            content.Add(element);
        }

        public int Layout(PdfContentByte canvas, bool useAscender, bool simulate, float llx, float lly, float urx, float ury)
        {
            float leftX = Math.Min(llx, urx);
            float maxY = Math.Max(lly, ury);
            float minY = Math.Min(lly, ury);
            float rightX = Math.Max(llx, urx);
            float yLine = maxY;
            bool contentCutByFixedHeight = false;

            if (width != null && width > 0)
            {
                if (width < rightX - leftX)
                    rightX = leftX + (float) width;
                else if (width > rightX - leftX)
                    return ColumnText.NO_MORE_COLUMN;
            }
            else if (percentageWidth != null)
            {
                contentWidth = (rightX - leftX)*(float) percentageWidth;
                rightX = leftX + contentWidth;
            }

            if (height != null && height > 0)
            {
                if (height < maxY - minY)
                {
                    contentCutByFixedHeight = true;
                    minY = maxY - (float) height;
                }
                else if (height > maxY - minY)
                {
                    return ColumnText.NO_MORE_COLUMN;
                }
            }
            else if (percentageHeight != null)
            {
                if (percentageHeight < 1.0)
                    contentCutByFixedHeight = true;
                contentHeight = (maxY - minY)*(float) percentageHeight;
                minY = maxY - contentHeight;
            }

            if (!simulate && position == PdfDiv.PositionType.RELATIVE)
            {
                float? translationX = null;
                if (left != null)
                    translationX = left;
                else if (right != null)
                    translationX = -right;
                else
                    translationX = 0f;

                float? translationY = null;
                if (top != null)
                    translationY = -top;
                else if (bottom != null)
                    translationY = bottom;
                else
                    translationY = 0f;
                canvas.SaveState();
                canvas.Transform(new Matrix(1f, 0, 0, 1f, translationX.Value, translationY.Value));
            }

            if (!simulate)
            {
                if (backgroundColor != null && getActualWidth() > 0 && getActualHeight() > 0)
                {
                    float backgroundWidth = getActualWidth();
                    float backgroundHeight = getActualHeight();
                    if (width != null)
                        backgroundWidth = width > 0 ? (float) width : 0;
                    if (height != null)
                        backgroundHeight = height > 0 ? (float) height : 0;
                    if (backgroundWidth > 0 && backgroundHeight > 0)
                    {
                        Rectangle background = new Rectangle(leftX, maxY - backgroundHeight, leftX + backgroundWidth, maxY);
                        background.BackgroundColor = backgroundColor;
                        canvas.Rectangle(background);
                    }
                }
            }

            if (percentageWidth == null)
                contentWidth = 0;
            if (percentageHeight == null)
                contentHeight = 0;

            minY += paddingBottom;
            leftX += paddingLeft;
            rightX -= paddingRight;

            yLine -= paddingTop;

            int status = ColumnText.NO_MORE_TEXT;

            if (content.Count > 0)
            {
                FloatLayout floatLay = null;
                if (this.floatLayout == null)
                {
                    List<IElement> floatingElements = new List<IElement>(content);
                    if (simulate)
                        floatLay = new FloatLayout(floatingElements, useAscender);
                    else
                        floatLay = this.floatLayout = new FloatLayout(floatingElements, useAscender);
                }
                else
                {
                    if (simulate)
                    {
                        List<IElement> floatingElements = new List<IElement>(this.floatLayout.content);
                        floatLay = new FloatLayout(floatingElements, useAscender);
                    }
                    else
                    {
                        floatLay = this.floatLayout;
                    }
                }

                floatLay.SetSimpleColumn(leftX, minY, rightX, yLine);
                status = floatLay.Layout(canvas, simulate);
                yLine = floatLay.YLine;
                if (percentageWidth == null && contentWidth < floatLay.FilledWidth)
                    contentWidth = floatLay.FilledWidth;
            }


            if (!simulate && position == PdfDiv.PositionType.RELATIVE)
                canvas.RestoreState();

            yLine -= paddingBottom;
            if (percentageHeight == null)
                contentHeight = maxY - yLine;

            if (percentageWidth == null)
                contentWidth += paddingLeft + paddingRight;

            return contentCutByFixedHeight ? ColumnText.NO_MORE_TEXT : status;
        }

        public PdfObject GetAccessibleAttribute(PdfName key)
        {
            PdfObject result;
            if (accessibleAttributes != null && accessibleAttributes.TryGetValue(key, out result))
                return result;
            else
                return null;
        }

        public void SetAccessibleAttribute(PdfName key, PdfObject value)
        {
            if (accessibleAttributes == null)
                accessibleAttributes = new Dictionary<PdfName, PdfObject>();
            accessibleAttributes[key] = value;
        }

        public Dictionary<PdfName, PdfObject> GetAccessibleAttributes()
        {
            return accessibleAttributes;
        }

        public PdfName Role
        {
            get { return role; }
            set { role = value; }
        }

        public Guid ID
        {
            get { return id; }
            set { id = value; }
        }
    }
}
