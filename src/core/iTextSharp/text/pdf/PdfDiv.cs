/*
 * $Id: PdfDiv.cs $
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
using iTextSharp.awt.geom;
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

        public enum DisplayType {
            DEFAULT_NULL_VALUE, NONE, BLOCK, INLINE, INLINE_BLOCK, INLINE_TABLE, LIST_ITEM, RUN_IN, TABLE, TABLE_CAPTION, TABLE_CELL, TABLE_COLUMN_GROUP, TABLE_COLUMN, TABLE_FOOTER_GROUP,
            TABLE_HEADER_GROUP, TABLE_ROW, TABLE_ROW_GROUP
        };

        public enum BorderTopStyle
        {
            NONE,
            DOTTED, 
            DASHED, 
            SOLID, 
            DOUBLE, 
            GROOVE, 
            RIDGE, 
            INSET, 
            OUTSET
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
        
        private Image backgroundImage = null;

        private float? backgroundImageWidth;

        private float backgroundImageHeight = 0;

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

        private DisplayType display = DisplayType.DEFAULT_NULL_VALUE;

        private FloatLayout floatLayout = null;

        private BorderTopStyle borderTopStyle = BorderTopStyle.NONE;

        private float yLine;

        protected int runDirection = PdfWriter.RUN_DIRECTION_DEFAULT;

        protected PdfName role = PdfName.DIV;

        protected Dictionary<PdfName, PdfObject> accessibleAttributes = null;

        protected AccessibleElementId id = new AccessibleElementId();

        /**
         * Defines if the div should be kept on one page if possible
         */
        private bool keepTogether;

      
        virtual public float? Left
        {
            get { return left; }
            set { left = value; }
        }

        virtual public float? Top
        {
            get { return top; }
            set { top = value; }
        }

        virtual public float? Right
        {
            get { return right; }
            set { right = value; }
        }

        virtual public float? Bottom
        {
            get { return bottom; }
            set { bottom = value; }
        }

        virtual public float? Width
        {
            get { return width; }
            set { width = value; }
        }

        virtual public float? Height
        {
            get { return height; }
            set { height = value; }
        }

        virtual public float? PercentageHeight
        {
            get { return percentageHeight; }
            set { percentageHeight = value; }
        }

        virtual public float? PercentageWidth
        {
            get { return percentageWidth; }
            set { percentageWidth = value; }
        }

        virtual public float ContentWidth
        {
            get { return contentWidth; }
            set { contentWidth = value; }
        }

        virtual public float ContentHeight
        {
            get { return contentHeight; }
            set { contentHeight = value; }
        }

        virtual public float getActualHeight()
        {
            return height != null && height >= contentHeight ? (float) height : contentHeight;
        }

        virtual public float getActualWidth()
        {
            return width != null && width >= contentWidth ? (float) width : contentWidth;
        }

        virtual public int TextAlignment
        {
            get { return textAlignment; }
            set { textAlignment = value; }
        }

        virtual public float PaddingLeft
        {
            get { return paddingLeft; }
            set { paddingLeft = value; }
        }

        virtual public float PaddingRight
        {
            get { return paddingRight; }
            set { paddingRight = value; }
        }

        virtual public float PaddingTop
        {
            get { return paddingTop; }
            set { paddingTop = value; }
        }

        virtual public float PaddingBottom
        {
            get { return paddingBottom; }
            set { paddingBottom = value; }
        }

        virtual public FloatType Float
        {
            get { return floatType; }
            set { floatType = value; }
        }

        virtual public PositionType Position
        {
            get { return position; }
            set { position = value; }
        }

        virtual public FloatLayout FloatLayout
        {
            get { return floatLayout; }
            set { floatLayout = value; }
        }

        virtual public DisplayType Display 
        {
            get { return display;  }
            set { display = value; }
        }

        virtual public BaseColor BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }

        virtual public Image BackgroundImage 
        {
            set { backgroundImage = value; }
        }

        /** 	
          * Image will be scaled to fit in the div occupied area.
          */
        virtual public void SetBackgroundImage(Image image, float width, float height) {
            backgroundImage = image;
            backgroundImageWidth = width;
            backgroundImageHeight = height;
        }

        virtual public float YLine
        {
            get { return yLine; }
        }

        public virtual int RunDirection {
            get { return runDirection; }
            set { runDirection = value; }
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

        virtual public List<IElement> Content
        {
            get { return content; }
        }

        virtual public BorderTopStyle BorderStyle
        {
            get { return borderTopStyle; }
            set { borderTopStyle = value; }
        }

        virtual public bool KeepTogether {
            get { return keepTogether; }
            set { keepTogether = value; }
        }

       

        public PdfDiv()
        {
            content = new List<IElement>();
            keepTogether = false;
        }

        /**
         * Gets all the chunks in this element.
         *
         * @return	an <CODE>ArrayList</CODE>
         */

        virtual public IList<Chunk> Chunks
        {
            get { return new List<Chunk>(); }
        }

        /**
         * Gets the type of the text element.
         *
         * @return	a type
         */

        virtual public int Type
        {
            get { return Element.DIV; }
        }

        /**
         * @see com.itextpdf.text.Element#isContent()
         * @since	iText 2.0.8
         */
        virtual public bool IsContent()
        {
            return true;
        }

        /**
         * @see com.itextpdf.text.Element#isNestable()
         * @since	iText 2.0.8
         */
        virtual public bool IsNestable()
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

        virtual public void AddElement(IElement element)
        {
            content.Add(element);
        }

        virtual public int Layout(PdfContentByte canvas, bool useAscender, bool simulate, float llx, float lly, float urx, float ury)
        {
            float leftX = Math.Min(llx, urx);
            float maxY = Math.Max(lly, ury);
            float minY = Math.Min(lly, ury);
            float rightX = Math.Max(llx, urx);
            yLine = maxY;
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
            else if (percentageWidth == null) 
            {
                if (this.floatType == FloatType.NONE && (this.display == DisplayType.DEFAULT_NULL_VALUE ||
                    this.display == DisplayType.BLOCK || this.display == DisplayType.LIST_ITEM ||
                    this.display == DisplayType.RUN_IN)) 
                {
                    contentWidth = rightX - leftX;
                }
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
                canvas.Transform(new AffineTransform(1f, 0, 0, 1f, translationX.Value, translationY.Value));
            }

            if (!simulate)
            {
                if ((backgroundColor != null || backgroundImage != null) && getActualWidth() > 0 && getActualHeight() > 0)
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
                        if (backgroundColor != null) {
                            background.BackgroundColor = backgroundColor;
                            PdfArtifact artifact = new PdfArtifact();
                            canvas.OpenMCBlock(artifact);
                            canvas.Rectangle(background);
                            canvas.CloseMCBlock(artifact);
                        }
                        if (backgroundImage != null) {
                            if (backgroundImageWidth == null) {
                                backgroundImage.ScaleToFit(background);
                            }
                            else {
                                backgroundImage.ScaleAbsolute((float)backgroundImageWidth, backgroundImageHeight);
                            }
                            backgroundImage.SetAbsolutePosition(background.Left, background.Bottom);
                            canvas.OpenMCBlock(backgroundImage);
                            canvas.AddImage(backgroundImage);
                            canvas.CloseMCBlock(backgroundImage);
                        }
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

            if (content.Count > 0) {
                if (floatLayout == null) {
                    List<IElement> floatingElements = new List<IElement>(content);
                    floatLayout = new FloatLayout(floatingElements, useAscender);
                    floatLayout.RunDirection = runDirection;
                }

                floatLayout.SetSimpleColumn(leftX, minY, rightX, yLine);
                if (borderTopStyle != BorderTopStyle.NONE)
                    floatLayout.compositeColumn.IgnoreSpacingBefore = false;
                status = floatLayout.Layout(canvas, simulate);
                yLine = floatLayout.YLine;

                if (percentageWidth == null && contentWidth < floatLayout.FilledWidth) {
                    contentWidth = floatLayout.FilledWidth;
                }
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

        virtual public PdfObject GetAccessibleAttribute(PdfName key)
        {
            PdfObject result;
            if (accessibleAttributes != null && accessibleAttributes.TryGetValue(key, out result))
                return result;
            else
                return null;
        }

        public virtual void SetAccessibleAttribute(PdfName key, PdfObject value) {
            if (accessibleAttributes == null)
                accessibleAttributes = new Dictionary<PdfName, PdfObject>();
            accessibleAttributes[key] = value;
        }

        public virtual Dictionary<PdfName, PdfObject> GetAccessibleAttributes() {
            return accessibleAttributes;
        }

        public virtual PdfName Role {
            get { return role; }
            set { role = value; }
        }

        public virtual AccessibleElementId ID {
            get { return id; }
            set { id = value; }
        }

        public virtual bool IsInline {
            get { return false; }
        }
    }
}
