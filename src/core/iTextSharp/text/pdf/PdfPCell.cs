using System;
using System.Collections.Generic;
using iTextSharp.text.pdf.events;
using iTextSharp.text.error_messages;
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

    /** A cell in a PdfPTable.
    */

    public class PdfPCell : Rectangle, IAccessibleElement {
        
        private ColumnText column = new ColumnText(null);
        
        /** Holds value of property verticalAlignment. */
        private int verticalAlignment = Element.ALIGN_TOP;
        
        /** Holds value of property paddingLeft. */
        private float paddingLeft = 2;
        
        /** Holds value of property paddingLeft. */
        private float paddingRight = 2;
        
        /** Holds value of property paddingTop. */
        private float paddingTop = 2;
        
        /** Holds value of property paddingBottom. */
        private float paddingBottom = 2;
        
        /** Holds value of property fixedHeight. */
        private float fixedHeight = 0;

        /**
         * Fixed height of the cell.
         */
        private float calculatedHeight = 0;
        
        /** Holds value of property noWrap. */
        private bool noWrap = false;
        
        /** Holds value of property table. */
        private PdfPTable table;
        
        /** Holds value of property minimumHeight. */
        private float minimumHeight;

        /**  
         * This field is used to cache the height which is calculated on getMaxHeight() method call;
         * this helps to avoid unnecessary recalculations on table drawing.
         */
        private float cachedMaxHeight;

        
        /** Holds value of property colspan. */
        private int colspan = 1;
        
        /**
        * Holds value of property rowspan.
        * @since    2.1.6
        */
        private int rowspan = 1;

        /** Holds value of property image. */
        private Image image;
        
        /** Holds value of property cellEvent. */
        private IPdfPCellEvent cellEvent;

        /** Holds value of property useDescender. */
        private bool useDescender = false;

        /** Increases padding to include border if true */
        private bool useBorderPadding = false;


        /** The text in the cell. */
        protected Phrase phrase;

        /**
        * The rotation of the cell. Possible values are
        * 0, 90, 180 and 270.
        */
        private new int rotation;

        protected PdfName role = PdfName.TD;

        protected Dictionary<PdfName, PdfObject> accessibleAttributes = null;
        protected internal AccessibleElementId id = new AccessibleElementId();
        protected List<PdfPHeaderCell> headers = null;

        /** Constructs an empty <CODE>PdfPCell</CODE>.
        * The default padding is 2.
        */
        public PdfPCell() : base(0, 0, 0, 0) {
            borderWidth = 0.5f;
            border = BOX;
            column.SetLeading(0, 1);
        }

        /** Constructs a <CODE>PdfPCell</CODE> with a <CODE>Phrase</CODE>.
        * The default padding is 2.
        * @param phrase the text
        */
        public PdfPCell(Phrase phrase) : base(0, 0, 0, 0) {
            borderWidth = 0.5f;
            border = BOX;
            column.AddText(this.phrase = phrase);
            column.SetLeading(0, 1);
        }
        
        /** Constructs a <CODE>PdfPCell</CODE> with an <CODE>Image</CODE>.
        * The default padding is 0.
        * @param image the <CODE>Image</CODE>
        */
        public PdfPCell(Image image) : this(image, false) {
        }
        
        /** Constructs a <CODE>PdfPCell</CODE> with an <CODE>Image</CODE>.
        * The default padding is 0.25 for a border width of 0.5.
        * @param image the <CODE>Image</CODE>
        * @param fit <CODE>true</CODE> to fit the image to the cell
        */
        public PdfPCell(Image image, bool fit) : base(0, 0, 0, 0) {
            borderWidth = 0.5f;
            border = BOX;
            column.SetLeading(0, 1);
            if (fit) {
                this.image = image;
                Padding = borderWidth / 2;
            }
            else {
                image.ScaleToFitLineWhenOverflow = false;
                column.AddText(this.phrase = new Phrase(new Chunk(image, 0, 0, true)));
                Padding = 0;
            }
        }
        
        /** Constructs a <CODE>PdfPCell</CODE> with a <CODE>PdfPtable</CODE>.
        * This constructor allows nested tables.
        * The default padding is 0.
        * @param table The <CODE>PdfPTable</CODE>
        */
        public PdfPCell(PdfPTable table) : this(table, null) {
        }
        
        /** Constructs a <CODE>PdfPCell</CODE> with a <CODE>PdfPtable</CODE>.
        * This constructor allows nested tables.
        * 
        * @param table The <CODE>PdfPTable</CODE>
        * @param style  The style to apply to the cell (you could use getDefaultCell())
        * @since 2.1.0
        */
        public PdfPCell(PdfPTable table, PdfPCell style) : base(0, 0, 0, 0) {
            borderWidth = 0.5f;
            border = BOX;
            column.SetLeading(0, 1);
            this.table = table;
            table.WidthPercentage = 100;
            table.ExtendLastRow = true;
            column.AddElement(table);
            if (style != null) {
                CloneNonPositionParameters(style);
                verticalAlignment = style.verticalAlignment;
                paddingLeft = style.paddingLeft;
                paddingRight = style.paddingRight;
                paddingTop = style.paddingTop;
                paddingBottom = style.paddingBottom;
                colspan = style.colspan;
                rowspan = style.rowspan;
                cellEvent = style.cellEvent;
                useDescender = style.useDescender;
                useBorderPadding = style.useBorderPadding;
                rotation = style.rotation;
            }
            else {
                Padding = 0;
            }
        }
        
        /** Constructs a deep copy of a <CODE>PdfPCell</CODE>.
        * @param cell the <CODE>PdfPCell</CODE> to duplicate
        */
        public PdfPCell(PdfPCell cell) : base(cell.llx, cell.lly, cell.urx, cell.ury) {
            CloneNonPositionParameters(cell);
            verticalAlignment = cell.verticalAlignment;
            paddingLeft = cell.paddingLeft;
            paddingRight = cell.paddingRight;
            paddingTop = cell.paddingTop;
            paddingBottom = cell.paddingBottom;
            phrase = cell.phrase;
            fixedHeight = cell.fixedHeight;
            minimumHeight = cell.minimumHeight;
            noWrap = cell.noWrap;
            colspan = cell.colspan;
            rowspan = cell.rowspan;
            if (cell.table != null)
                table = new PdfPTable(cell.table);
            image = Image.GetInstance(cell.image);
            cellEvent = cell.cellEvent;
            useDescender = cell.useDescender;
            column = ColumnText.Duplicate(cell.column);
            useBorderPadding = cell.useBorderPadding;
            rotation = cell.rotation;
            id = cell.id;
            role = cell.role;
            if (cell.accessibleAttributes != null)
                accessibleAttributes = new Dictionary<PdfName, PdfObject>(cell.accessibleAttributes);
            headers = cell.headers;
        }
        
        /**
        * Adds an iText element to the cell.
        * @param element
        */
        virtual public void AddElement(IElement element) {
            if (table != null) {
                table = null;
                column.SetText(null);
            }
            if (element is PdfPTable) {
                ((PdfPTable) element).SplitLate = false;
            } else if (element is PdfDiv) {
            foreach (IElement divChildElement in ((PdfDiv)element).Content) {
                if (divChildElement is PdfPTable) {
                    ((PdfPTable) divChildElement).SplitLate = false;
                }
            }
        }
            column.AddElement(element);
        }
        
        /** Gets the <CODE>Phrase</CODE> from this cell.
        * @return the <CODE>Phrase</CODE>
        */
        virtual public Phrase Phrase {
            get {
                return phrase;
            }
            set {
                table = null;
                image = null;
                column.SetText(this.phrase = value);
            }
        }
        
        /** Gets the horizontal alignment for the cell.
        * @return the horizontal alignment for the cell
        */
        virtual public int HorizontalAlignment {
            get {
                return column.Alignment;
            }
            set {
                column.Alignment = value;
            }
        }
        
        /** Gets the vertical alignment for the cell.
        * @return the vertical alignment for the cell
        */
        virtual public int VerticalAlignment {
            get {
                return verticalAlignment;
            }
            set {
                verticalAlignment = value;
                if (table != null)
                    table.ExtendLastRow = (verticalAlignment == Element.ALIGN_TOP);
            }
        }
        
        /** Gets the effective left padding.  This will include
        *  the left border width if {@link #UseBorderPadding} is true.
        * @return effective value of property paddingLeft.
        */
        virtual public float EffectivePaddingLeft {
            get {
                if (UseBorderPadding) {
                    float border = BorderWidthLeft / (UseVariableBorders ? 1f : 2f);
                    return paddingLeft + border;
                }
                return paddingLeft;
            }
        }
        
        /**
        * @return Value of property paddingLeft.
        */
        virtual public float PaddingLeft {
            get {
                return paddingLeft;
            }
            set {
                paddingLeft = value;
            }
        }

        /** Gets the effective right padding.  This will include
        *  the right border width if {@link #UseBorderPadding} is true.
        * @return effective value of property paddingRight.
        */
        virtual public float EffectivePaddingRight {
            get {
                if (UseBorderPadding) {
                    float border = BorderWidthRight / (UseVariableBorders ? 1f : 2f);
                    return paddingRight + border;
                }
                return paddingRight;
            }
        }
        
        /**
        * Getter for property paddingRight.
        * @return Value of property paddingRight.
        */
        virtual public float PaddingRight {
            get {
                return paddingRight;
            }
            set {
                paddingRight = value;
            }
        }

        /** Gets the effective top padding.  This will include
        *  the top border width if {@link #isUseBorderPadding()} is true.
        * @return effective value of property paddingTop.
        */
        virtual public float EffectivePaddingTop {
            get {
                if (UseBorderPadding) {
                    float border = BorderWidthTop/(UseVariableBorders?1f:2f);
                    return paddingTop + border;
                }
                return paddingTop;
            }
        }
        
        /**
        * Getter for property paddingTop.
        * @return Value of property paddingTop.
        */
        virtual public float PaddingTop {
            get {
                return paddingTop;
            }
            set {
                paddingTop = value;
            }
        }

        /**
        /** Gets the effective bottom padding.  This will include
        *  the bottom border width if {@link #UseBorderPadding} is true.
        * @return effective value of property paddingBottom.
        */
        virtual public float EffectivePaddingBottom {
            get {
                if (UseBorderPadding) {
                    float border = BorderWidthBottom/(UseVariableBorders?1f:2f);
                    return paddingBottom + border;
                }
                return paddingBottom;
            }
        }
        
        /**
        * Getter for property paddingBottom.
        * @return Value of property paddingBottom.
        */
        virtual public float PaddingBottom {
            get {
                return paddingBottom;
            }
            set {
                paddingBottom = value;
            }
        }

        /**
        * Sets the padding of the contents in the cell (space between content and border).
        * @param padding
        */
        virtual public float Padding {
            set {
                paddingBottom = value;
                paddingTop = value;
                paddingLeft = value;
                paddingRight = value;
            }
        }

        /**
        * Adjusts effective padding to include border widths.
        * @param use adjust effective padding if true
        */
        virtual public bool UseBorderPadding {
            set {
                useBorderPadding = value;
            }
            get {
                return useBorderPadding;
            }
        }

        /**
        * Sets the leading fixed and variable. The resultant leading will be
        * fixedLeading+multipliedLeading*maxFontSize where maxFontSize is the
        * size of the bigest font in the line.
        * @param fixedLeading the fixed leading
        * @param multipliedLeading the variable leading
        */
        virtual public void SetLeading(float fixedLeading, float multipliedLeading) {
            column.SetLeading(fixedLeading, multipliedLeading);
        }
        
        /**
        * Gets the fixed leading
        * @return the leading
        */
        virtual public float Leading {
            get {
                return column.Leading;
            }
        }
        
        /**
        * Gets the variable leading
        * @return the leading
        */
        virtual public float MultipliedLeading {
            get {
                return column.MultipliedLeading;
            }
        }
        
        /**
        * Gets the first paragraph line indent.
        * @return the indent
        */
        virtual public float Indent {
            get {
                return column.Indent;
            }
            set {
                column.Indent = value;
            }
        }
        
        /**
        * Gets the extra space between paragraphs.
        * @return the extra space between paragraphs
        */
        virtual public float ExtraParagraphSpace {
            get {
                return column.ExtraParagraphSpace;
            }
            set {
                column.ExtraParagraphSpace = value;
            }
        }

        public virtual float CalculatedHeight {
            get { return calculatedHeight; }
            set { calculatedHeight = value; }
        }

        public virtual bool HasCalculatedHeight() {
            return calculatedHeight > 0;
        }

        /**
        * Getter for property fixedHeight.
        * @return Value of property fixedHeight.
        */
        virtual public float FixedHeight {
            get {
                return fixedHeight;
            }
            set {
                fixedHeight = value;
                minimumHeight = 0;
            }
        }
        
        /**
        * Tells you whether the cell has a fixed height.
        * 
        * @return   true is a fixed height was set.
        * @since 2.1.5
        */
        virtual public bool HasFixedHeight() {
            return FixedHeight > 0;
        }

        /**
         * Gets the height which was calculated on last call of getMaxHeight().
         * If cell's bBox and content wasn't changed this value is actual maxHeight of the cell.
         * @return max height which was calculated on last call of getMaxHeight(); if getMaxHeight() wasn't called the return value is 0
         */
        public float CachedMaxHeight {
            get { return cachedMaxHeight; }
        }

        public bool HasCachedMaxHeight() {
            return cachedMaxHeight > 0;
        }

        /**
        * Setter for property noWrap.
        * @param noWrap New value of property noWrap.
        */
        virtual public bool NoWrap {
            set {
                noWrap = value;
            }
            get {
                return noWrap;
            }
        }
        
        /**
        * Getter for property table.
        * @return Value of property table.
        */
        virtual public PdfPTable Table {
            get {
                return table;
            }
            set {
                table = value;
                column.SetText(null);
                image = null;
                if (table != null) {
                    table.ExtendLastRow = (verticalAlignment == Element.ALIGN_TOP);
                    column.AddElement(table);
                    table.WidthPercentage = 100;
                }
            }
        }
        
        /** Getter for property minimumHeight.
        * @return Value of property minimumHeight.
        */
        virtual public float MinimumHeight {
            get {
                return minimumHeight;
            }
            set {
                this.minimumHeight = value;
                fixedHeight = 0;
            }
        }
        
        /**
        * Tells you whether the cell has a minimum height.
        * 
        * @return   true if a minimum height was set.
        * @since 2.1.5
        */
        virtual public bool HasMinimumHeight() {
            return MinimumHeight > 0;
        }

        /** Getter for property colspan.
        * @return Value of property colspan.
        */
        virtual public int Colspan {
            get {
                return colspan;
            }
            set {
                colspan = value;
            }
        }
                
        /** Getter for property rowspan.
        * @return Value of property rowspan.
        */
        virtual public int Rowspan {
            get {
                return rowspan;
            }
            set {
                rowspan = value;
            }
        }
                
        /**
        * Gets the following paragraph lines indent.
        * @return the indent
        */
        virtual public float FollowingIndent {
            get {
                return column.FollowingIndent;
            }
            set {
                column.FollowingIndent = value;
            }
        }
        
        /**
        * Gets the right paragraph lines indent.
        * @return the indent
        */
        virtual public float RightIndent {
            get {
                return column.RightIndent;
            }
            set {
                column.RightIndent = value;
            }
        }
        
        /** Gets the space/character extra spacing ratio for
        * fully justified text.
        * @return the space/character extra spacing ratio
        */
        virtual public float SpaceCharRatio {
            get {
                return column.SpaceCharRatio;
            }
            set {
                column.SpaceCharRatio = value;
            }
        }
        
        /**
        * Gets the run direction of the text content in the cell
        * @return One of the following values: PdfWriter.RUN_DIRECTION_DEFAULT, PdfWriter.RUN_DIRECTION_NO_BIDI, PdfWriter.RUN_DIRECTION_LTR or PdfWriter.RUN_DIRECTION_RTL.
        */
        virtual public int RunDirection {
            get {
                return column.RunDirection;
            }
            set {
                column.RunDirection = value;
            }
        }
        
        /** Getter for property image.
        * @return Value of property image.
        *
        */
        virtual public Image Image {
            get {
                return this.image;
            }
            set {
                column.SetText(null);
                table = null;
                this.image = value;
            }
        }
        
        /** Gets the cell event for this cell.
        * @return the cell event
        *
        */
        virtual public IPdfPCellEvent CellEvent {
            get {
                return this.cellEvent;
            }
            set {
                if (value == null) this.cellEvent = null;
                else if (this.cellEvent == null) this.cellEvent = value;
                else if (this.cellEvent is PdfPCellEventForwarder) ((PdfPCellEventForwarder)this.cellEvent).AddCellEvent(value);
                else {
                    PdfPCellEventForwarder forward = new PdfPCellEventForwarder();
                    forward.AddCellEvent(this.cellEvent);
                    forward.AddCellEvent(value);
                    this.cellEvent = forward;
                }
            }
        }
        
        /** Gets the arabic shaping options.
        * @return the arabic shaping options
        */
        virtual public int ArabicOptions {
            get {
                return column.ArabicOptions;
            }
            set {
                column.ArabicOptions = value;
            }
        }
        
        /** Gets state of first line height based on max ascender
        * @return true if an ascender is to be used.
        */
        virtual public bool UseAscender {
            get {
                return column.UseAscender;
            }
            set {
                column.UseAscender = value;
            }
        }

        /** Getter for property useDescender.
        * @return Value of property useDescender.
        *
        */
        virtual public bool UseDescender {
            get {
                return this.useDescender;
            }
            set {
                useDescender = value;
            }
        }

        /**
        * Gets the ColumnText with the content of the cell.
        * @return a columntext object
        */
        virtual public ColumnText Column {
            get {
                return column;
            }
            set {
                column = value;
            }
        }

        /**
        * Returns the list of composite elements of the column.
        * @return   a List object.
        * @since    2.1.1
        */
        virtual public List<IElement> CompositeElements {
            get {
                return column.compositeElements;
            }
        }

        /**
        * Sets the rotation of the cell. Possible values are
        * 0, 90, 180 and 270.
        * @param rotation the rotation of the cell
        */
        public new int Rotation {
            set {
                int rot = value % 360;
                if (rot < 0)
                    rot += 360;
                if ((rot % 90) != 0)
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("rotation.must.be.a.multiple.of.90"));
                rotation = rot;
            }
            get {
                return rotation;
            }
        }

        /**
        * Returns the height of the cell.
        * @return  the height of the cell
        * @since   3.0.0
        */
        virtual public float GetMaxHeight() {
            bool pivoted = (Rotation == 90 || Rotation == 270);
            Image img = this.Image;
            if (img != null) {
                img.ScalePercent(100);
                float refWidth = pivoted ? img.ScaledHeight : img.ScaledWidth;
                float scale = (Right - EffectivePaddingRight
                        - EffectivePaddingLeft - Left) / refWidth;
                img.ScalePercent(scale * 100);
                float refHeight = pivoted ? img.ScaledWidth : img.ScaledHeight;
                Bottom = Top - EffectivePaddingTop - EffectivePaddingBottom - refHeight;
            }
            else {
                if (pivoted && HasFixedHeight())
                    Bottom = Top - FixedHeight;
                else {
                    ColumnText ct = ColumnText.Duplicate(Column);
                    float right, top, left, bottom;
                    if (pivoted) {
                        right = PdfPRow.RIGHT_LIMIT;
                        top = Right - EffectivePaddingRight;
                        left = 0;
                        bottom = Left + EffectivePaddingLeft;
                    }
                    else {
                        right = NoWrap ? PdfPRow.RIGHT_LIMIT : Right - EffectivePaddingRight;
                        top = Top - EffectivePaddingTop;
                        left = Left + EffectivePaddingLeft;
                        bottom = HasCalculatedHeight() ? Top + EffectivePaddingBottom - CalculatedHeight : PdfPRow.BOTTOM_LIMIT;
                    }
                    PdfPRow.SetColumn(ct, left, bottom, right, top);
                    ct.Go(true);
                    if (pivoted)
                        Bottom = Top - EffectivePaddingTop - EffectivePaddingBottom - ct.FilledWidth;
                    else {
                        float yLine = ct.YLine;
                        if (UseDescender)
                            yLine += ct.Descender;
                        Bottom = yLine - EffectivePaddingBottom;
                    }
                }
            }
            float height = Height;
            if (height == EffectivePaddingTop + EffectivePaddingBottom)
                height = 0;
            if (HasFixedHeight())
                height = FixedHeight;
            else if (HasMinimumHeight() && height < MinimumHeight)
                height = MinimumHeight;
            cachedMaxHeight = height; 
            return height;
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

        public virtual PdfName Role {
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

        public virtual List<PdfPHeaderCell> Headers {
            get { return headers; }
        }

        public virtual void AddHeader(PdfPHeaderCell header) {
            if (headers == null)
                headers = new List<PdfPHeaderCell>();
            headers.Add(header);
        }
    }
}
