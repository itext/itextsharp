using System;
using System.Text;
using System.Collections.Generic;

/*
 * $Id$
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

namespace iTextSharp.text.pdf {

    /**
     * <CODE>PdfLine</CODE> defines an array with <CODE>PdfChunk</CODE>-objects
     * that fit into 1 line.
     */

    public class PdfLine {
    
        // membervariables
    
        /** The arraylist containing the chunks. */
        protected internal List<PdfChunk> line;
    
        /** The left indentation of the line. */
        protected internal float left;
    
        /** The width of the line. */
        protected internal float width;
    
        /** The alignment of the line. */
        protected internal int alignment;
    
        /** The heigth of the line. */
        protected internal float height;
    
        /** The listsymbol (if necessary). */
        //protected internal Chunk listSymbol = null;
    
        /** The listsymbol (if necessary). */
        //protected internal float symbolIndent;
    
        /** <CODE>true</CODE> if the chunk splitting was caused by a newline. */
        protected internal bool newlineSplit = false;
    
        /** The original width. */
        protected internal float originalWidth;
    
        protected internal bool isRTL = false;

        protected internal ListItem listItem = null;

        protected TabStop tabStop = null;

        protected float tabStopAnchorPosition = float.NaN;

        protected float tabPosition = float.NaN;
    
        // constructors
    
        /**
         * Constructs a new <CODE>PdfLine</CODE>-object.
         *
         * @param    left        the limit of the line at the left
         * @param    right        the limit of the line at the right
         * @param    alignment    the alignment of the line
         * @param    height        the height of the line
         */
    
        internal PdfLine(float left, float right, int alignment, float height) {
            this.left = left;
            this.width = right - left;
            this.originalWidth = this.width;
            this.alignment = alignment;
            this.height = height;
            this.line = new List<PdfChunk>();
        }
    
        /**
        * Creates a PdfLine object.
        * @param left              the left offset
        * @param originalWidth     the original width of the line
        * @param remainingWidth    bigger than 0 if the line isn't completely filled
        * @param alignment         the alignment of the line
        * @param newlineSplit      was the line splitted (or does the paragraph end with this line)
        * @param line              an array of PdfChunk objects
        * @param isRTL             do you have to read the line from Right to Left?
        */
        internal PdfLine(float left, float originalWidth, float remainingWidth, int alignment, bool newlineSplit, List<PdfChunk> line, bool isRTL) {
            this.left = left;
            this.originalWidth = originalWidth;
            this.width = remainingWidth;
            this.alignment = alignment;
            this.line = line;
            this.newlineSplit = newlineSplit;
            this.isRTL = isRTL;
        }
    
        // methods
    
        /**
         * Adds a <CODE>PdfChunk</CODE> to the <CODE>PdfLine</CODE>.
         *
         * @param		chunk		        the <CODE>PdfChunk</CODE> to add
         * @param		currentLeading		new value for the height of the line
         * @return		<CODE>null</CODE> if the chunk could be added completely; if not
         *				a <CODE>PdfChunk</CODE> containing the part of the chunk that could
         *				not be added is returned
         */

        internal PdfChunk Add(PdfChunk chunk, float currentLeading) {
            //we set line height to correspond to the current leading
            if (chunk != null && !chunk.ToString().Equals("")) {
                //whitespace shouldn't change leading
                if (!chunk.ToString().Equals(" ")) {
                    if (this.height < currentLeading || this.line.Count == 0)
                        this.height = currentLeading;
                }
            }
            return Add(chunk);
        }

            /**
             * Adds a <CODE>PdfChunk</CODE> to the <CODE>PdfLine</CODE>.
             *
             * @param        chunk        the <CODE>PdfChunk</CODE> to add
             * @return        <CODE>null</CODE> if the chunk could be added completely; if not
             *                a <CODE>PdfChunk</CODE> containing the part of the chunk that could
             *                not be added is returned
             */
    
        internal PdfChunk Add(PdfChunk chunk) {
            // nothing happens if the chunk is null.
            if (chunk == null || chunk.ToString().Equals("")) {
                return null;
            }
        
            // we split the chunk to be added
            PdfChunk overflow = chunk.Split(width);
            newlineSplit = (chunk.IsNewlineSplit() || overflow == null);
            //        if (chunk.IsNewlineSplit() && alignment == Element.ALIGN_JUSTIFIED)
            //            alignment = Element.ALIGN_LEFT;
            if (chunk.IsTab()) {
                Object[] tab = (Object[]) chunk.GetAttribute(Chunk.TAB);
                if (chunk.IsAttribute(Chunk.TABSETTINGS)) {
                    bool isWhiteSpace = (bool) tab[1];
                    if (!isWhiteSpace || line.Count > 0) {
                        Flush();
                        tabStopAnchorPosition = float.NaN;
                        tabStop = PdfChunk.GetTabStop(chunk, originalWidth - width);
                        if (tabStop.Position > originalWidth) {
                            if (isWhiteSpace)
                                overflow = null;
                            else if (Math.Abs(originalWidth - width) < 0.001) {
                                AddToLine(chunk);
                                overflow = null;
                            } else {
                                overflow = chunk;
                            }
							width = 0;
                        } else {
                            chunk.TabStop = tabStop;
                            if (!isRTL && tabStop.Align == TabStop.Alignment.LEFT) {
                                width = originalWidth - tabStop.Position;
                                tabStop = null;
                                tabPosition = float.NaN;
                            } else
                                tabPosition = originalWidth - width;
                            AddToLine(chunk);
                        }
                    } else
                        return null;
                } else {
                    //Keep deprecated tab logic for backward compatibility...
                    float tabStopPosition = (float) tab[1];
                    bool newline = (bool) tab[2];
                    if (newline && tabStopPosition < originalWidth - width)
                        return chunk;
                    chunk.AdjustLeft(left);
                    width = originalWidth - tabStopPosition;
                    AddToLine(chunk);
                }
            } // if the length of the chunk > 0 we add it to the line 
            else if (chunk.Length > 0 || chunk.IsImage()) {
                if (overflow != null)
                    chunk.TrimLastSpace();
                width -= chunk.Width();
                AddToLine(chunk);
            }
        
            // if the length == 0 and there were no other chunks added to the line yet,
            // we risk to end up in an endless loop trying endlessly to add the same chunk
            else if (line.Count < 1) {
                chunk = overflow;
                overflow = chunk.Truncate(width);
                width -= chunk.Width();
                if (chunk.Length > 0) {
                    AddToLine(chunk);
                    return overflow;
                }
                // if the chunck couldn't even be truncated, we add everything, so be it
                else {
                    if (overflow != null)
                        AddToLine(chunk);
                    return null;
                }
            }
            else {
                width += line[line.Count - 1].TrimLastSpace();
            }
            return overflow;
        }
    
        private void AddToLine(PdfChunk chunk) {
            if (chunk.ChangeLeading) {
                float f;
                if (chunk.IsImage()) {
                    Image img = chunk.Image;
                    f = chunk.ImageHeight + chunk.ImageOffsetY
                            + img.BorderWidthTop + img.SpacingBefore;
                } else {
                    f = chunk.Leading;
                }
                if (f > height) height = f;
            }
            if (tabStop != null && tabStop.Align == TabStop.Alignment.ANCHOR && float.IsNaN(tabStopAnchorPosition))
            {
                String value = chunk.ToString();
                int anchorIndex = value.IndexOf(tabStop.AnchorChar);
                if (anchorIndex != -1)
                {
                    float subWidth = chunk.Width(value.Substring(anchorIndex));
                    tabStopAnchorPosition = originalWidth - width - subWidth;
                }
            }
            line.Add(chunk);
        }

        // methods to retrieve information
    
        /**
         * Returns the number of chunks in the line.
         *
         * @return    a value
         */
    
        virtual public int Size {
            get {
                return line.Count;
            }
        }
    
        /**
         * Returns an iterator of <CODE>PdfChunk</CODE>s.
         *
         * @return    an <CODE>Iterator</CODE>
         */
    
        virtual public IEnumerator<PdfChunk> GetEnumerator() {
            return line.GetEnumerator();
        }
    
        /**
         * Returns the height of the line.
         *
         * @return    a value
         */
    
        internal float Height {
            get {
                return height;
            }
        }
    
        /**
         * Returns the left indentation of the line taking the alignment of the line into account.
         *
         * @return    a value
         */
    
        internal  float IndentLeft {
            get {
                if (isRTL) {
                    switch (alignment) {
                        case Element.ALIGN_CENTER:
                            return left + width / 2f;
                        case Element.ALIGN_RIGHT:
                            return left;
                        case Element.ALIGN_JUSTIFIED:
                            return left + (HasToBeJustified() ? 0 : width);
                        case Element.ALIGN_LEFT:
                        default:
                            return left + width;
                    }
                } else if (this.GetSeparatorCount() <= 0) {
                    switch (alignment) {
                        case Element.ALIGN_RIGHT:
                            return left + width;
                        case Element.ALIGN_CENTER:
                            return left + (width / 2f);
                    }
                }
                return left;
            }
        }
    
        /**
         * Checks if this line has to be justified.
         *
         * @return    <CODE>true</CODE> if the alignment equals <VAR>ALIGN_JUSTIFIED</VAR> and there is some width left.
         */
    
        virtual public bool HasToBeJustified() {
            return ((alignment == Element.ALIGN_JUSTIFIED && !newlineSplit) || alignment == Element.ALIGN_JUSTIFIED_ALL) && width != 0;
        }
    
        /**
         * Resets the alignment of this line.
         * <P>
         * The alignment of the last line of for instance a <CODE>Paragraph</CODE>
         * that has to be justified, has to be reset to <VAR>ALIGN_LEFT</VAR>.
         */
    
        virtual public void ResetAlignment() {
            if (alignment == Element.ALIGN_JUSTIFIED) {
                alignment = Element.ALIGN_LEFT;
            }
        }
    
        /** Adds extra indentation to the left (for Paragraph.setFirstLineIndent). */
        internal void SetExtraIndent(float extra) {
            left += extra;
            width -= extra;
            originalWidth -= extra;
        }

        /**
         * Returns the width that is left, after a maximum of characters is added to the line.
         *
         * @return    a value
         */
    
        internal float WidthLeft {
            get {
                return width;
            }
        }
    
        /**
         * Returns the number of space-characters in this line.
         *
         * @return    a value
         */
    
        internal int NumberOfSpaces {
            get
            {
                int numberOfSpaces = 0;
                foreach (PdfChunk pdfChunk in line)
                {
                    String tmp = pdfChunk.ToString();
                    int length = tmp.Length;
                    for (int i = 0; i < length; i++)
                    {
                        if (tmp[i] == ' ')
                        {
                            numberOfSpaces++;
                        }
                    }
                }
                return numberOfSpaces;
            }
        }
    
        /**
         * Sets the listsymbol of this line.
         * <P>
         * This is only necessary for the first line of a <CODE>ListItem</CODE>.
         *
         * @param listItem the list symbol
         */
    
        virtual public ListItem ListItem {
            set {
                this.listItem = value;
                //this.listSymbol = value.ListSymbol;
                //this.symbolIndent = value.IndentationLeft;
            }
            get { return listItem; }
        }
    
        /**
         * Returns the listsymbol of this line.
         *
         * @return    a <CODE>PdfChunk</CODE> if the line has a listsymbol; <CODE>null</CODE> otherwise
         */
    
        virtual public Chunk ListSymbol {
            get {
                return listItem != null ? listItem.ListSymbol : null;
            }
        }
    
        /**
         * Return the indentation needed to show the listsymbol.
         *
         * @return    a value
         */
    
        virtual public float ListIndent {
            get {
                return listItem != null ? listItem.IndentationLeft : 0;
            }
        }

        /**
         * Get the string representation of what is in this line.
         *
         * @return    a <CODE>string</CODE>
         */
    
        public override string ToString() {
            StringBuilder tmp = new StringBuilder();
            foreach (PdfChunk c in line) {
                tmp.Append(c.ToString());
            }
            return tmp.ToString();
        }
    
        virtual public int GetLineLengthUtf32() {
            int total = 0;
            foreach (PdfChunk c in line) {
                total += c.LengthUtf32;
            }
            return total;
        }
    
        /**
         * Checks if a newline caused the line split.
         * @return <CODE>true</CODE> if a newline caused the line split
         */
        virtual public bool NewlineSplit {
            get {
                return newlineSplit && (alignment != Element.ALIGN_JUSTIFIED_ALL);
            }
        }
    
        /**
         * Gets the index of the last <CODE>PdfChunk</CODE> with metric attributes
         * @return the last <CODE>PdfChunk</CODE> with metric attributes
         */
        virtual public int LastStrokeChunk {
            get {
                int lastIdx = line.Count - 1;
                for (; lastIdx >= 0; --lastIdx) {
                    PdfChunk chunk = line[lastIdx];
                    if (chunk.IsStroked())
                        break;
                }
                return lastIdx;
            }
        }
    
        /**
         * Gets a <CODE>PdfChunk</CODE> by index.
         * @param idx the index
         * @return the <CODE>PdfChunk</CODE> or null if beyond the array
         */
        virtual public PdfChunk GetChunk(int idx) {
            if (idx < 0 || idx >= line.Count)
                return null;
            return line[idx];
        }
    
        /**
         * Gets the original width of the line.
         * @return the original width of the line
         */
        virtual public float OriginalWidth {
            get {
                return originalWidth;
            }
        }
    
        /**
         * Gets the difference between the "normal" leading and the maximum
         * size (for instance when there are images in the chunk and the leading
         * has to be taken into account).
         * @return  an extra leading for images
         * @since   2.1.5
         */
        internal float[] GetMaxSize(float fixedLeading, float multipliedLeading) {
            float normal_leading = 0;
            float image_leading = -10000;
            PdfChunk chunk;
            for (int k = 0; k < line.Count; ++k) {
                chunk = line[k];
                if (chunk.IsImage()) {
                    Image img = chunk.Image;
                    if (chunk.ChangeLeading) {
                        float height = chunk.ImageHeight + chunk.ImageOffsetY + img.SpacingBefore;
                        image_leading = Math.Max(height, image_leading);
                    }
                }else {
                    if (chunk.ChangeLeading)
                        normal_leading = Math.Max(chunk.Leading, normal_leading);
                    else
                        normal_leading = Math.Max(fixedLeading + multipliedLeading * chunk.Font.Size, normal_leading);
                }
            }
            return new float[]{normal_leading > 0 ? normal_leading : fixedLeading, image_leading};
        }
    
        internal bool RTL {
            get {
                return isRTL;
            }
        }
    
        /**
        * Gets the number of separators in the line.
        * Returns -1 if there's a tab in the line.
        * @return  the number of separators in the line
        * @since   2.1.2
        */
        internal int GetSeparatorCount() {
            int s = 0;
            foreach (PdfChunk ck in line) {
                if (ck.IsTab()) {
                    if (ck.IsAttribute(Chunk.TABSETTINGS))
                        continue;
                    //It seems justification was forbidden in the deprecated tab logic!!!
                    return -1;
                }
                if (ck.IsHorizontalSeparator()) {
                    s++;
                }
            }
            return s;
        }

        virtual public float GetWidthCorrected(float charSpacing, float wordSpacing) {
            float total = 0;
            for (int k = 0; k < line.Count; ++k) {
                PdfChunk ck = line[k];
                total += ck.GetWidthCorrected(charSpacing, wordSpacing);
            }
            return total;
        }

        /**
        * Gets the maximum size of the ascender for all the fonts used
        * in this line.
        * @return maximum size of all the ascenders used in this line
        */
        virtual public float Ascender {
            get {
                float ascender = 0;
                foreach (PdfChunk ck in line) {
                    if (ck.IsImage())
                        ascender = Math.Max(ascender, ck.ImageHeight + ck.ImageOffsetY);
                    else {
                        PdfFont font = ck.Font;
                        float textRise = ck.TextRise;
                        ascender = Math.Max(ascender, (textRise > 0 ? textRise : 0) + font.Font.GetFontDescriptor(BaseFont.ASCENT, font.Size));
                    }
                }
                return ascender;
            }
        }

        /**
        * Gets the biggest descender for all the fonts used 
        * in this line.  Note that this is a negative number.
        * @return maximum size of all the ascenders used in this line
        */
        virtual public float Descender {
            get {
                float descender = 0;
                foreach (PdfChunk ck in line) {
                    if (ck.IsImage())
                        descender = Math.Min(descender, ck.ImageOffsetY);
                    else {
                        PdfFont font = ck.Font;
                        float textRise = ck.TextRise;
                        descender = Math.Min(descender, (textRise < 0 ? textRise : 0) + font.Font.GetFontDescriptor(BaseFont.DESCENT, font.Size));
                    }
                }
                return descender;
            }
        }

        virtual public void Flush()
        {
            if (tabStop != null)
            {
                float textWidth = originalWidth - width - tabPosition;
                float tabStopPosition = tabStop.GetPosition(tabPosition, originalWidth - width, tabStopAnchorPosition);
                width = originalWidth - tabStopPosition - textWidth;
                if (width < 0)
                    tabStopPosition += width;
                if (!isRTL)
                    tabStop.Position = tabStopPosition;
                else
                    tabStop.Position = originalWidth - width - tabPosition;
                tabStop = null;
                tabPosition = float.NaN;
            }
        }
    }
}
