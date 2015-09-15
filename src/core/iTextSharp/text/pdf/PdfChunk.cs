using System;
using System.Collections.Generic;
using iTextSharp.text.pdf.interfaces;

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
     * A <CODE>PdfChunk</CODE> is the PDF translation of a <CODE>Chunk</CODE>.
     * <P>
     * A <CODE>PdfChunk</CODE> is a <CODE>PdfString</CODE> in a certain
     * <CODE>PdfFont</CODE> and <CODE>BaseColor</CODE>.
     *
     * @see     PdfString
     * @see     PdfFont
     * @see     iTextSharp.text.Chunk
     * @see     iTextSharp.text.Font
     */

    public class PdfChunk {


        private static char[] singleSpace = {' '};
        private const float ITALIC_ANGLE = 0.21256f;

        /** The allowed attributes in variable <CODE>attributes</CODE>. */
        private static Dictionary<string,object> keysAttributes = new Dictionary<string,object>();
    
        /** The allowed attributes in variable <CODE>noStroke</CODE>. */
        private static Dictionary<string,object> keysNoStroke = new Dictionary<string,object>();
        private const String TABSTOP = "TABSTOP";
    
        static PdfChunk() {
            keysAttributes.Add(Chunk.ACTION, null);
            keysAttributes.Add(Chunk.UNDERLINE, null);
            keysAttributes.Add(Chunk.REMOTEGOTO, null);
            keysAttributes.Add(Chunk.LOCALGOTO, null);
            keysAttributes.Add(Chunk.LOCALDESTINATION, null);
            keysAttributes.Add(Chunk.GENERICTAG, null);
            keysAttributes.Add(Chunk.NEWPAGE, null);
            keysAttributes.Add(Chunk.IMAGE, null);
            keysAttributes.Add(Chunk.BACKGROUND, null);
            keysAttributes.Add(Chunk.PDFANNOTATION, null);
            keysAttributes.Add(Chunk.SKEW, null);
            keysAttributes.Add(Chunk.HSCALE, null);
            keysAttributes.Add(Chunk.SEPARATOR, null);
            keysAttributes.Add(Chunk.TAB, null);
            keysAttributes.Add(Chunk.TABSETTINGS, null);
            keysAttributes.Add(Chunk.CHAR_SPACING, null);
            keysAttributes.Add(Chunk.WORD_SPACING, null);
            keysAttributes.Add(Chunk.LINEHEIGHT, null);
            keysNoStroke.Add(Chunk.SUBSUPSCRIPT, null);
            keysNoStroke.Add(Chunk.SPLITCHARACTER, null);
            keysNoStroke.Add(Chunk.HYPHENATION, null);
            keysNoStroke.Add(Chunk.TEXTRENDERMODE, null);
        }
    
        // membervariables

        /** The value of this object. */
        protected string value = PdfObject.NOTHING;
    
        /** The encoding. */
        protected string encoding = BaseFont.WINANSI;
    
    
        /** The font for this <CODE>PdfChunk</CODE>. */
        protected PdfFont font;

        protected BaseFont baseFont;
    
        protected ISplitCharacter splitCharacter;
        /**
         * Metric attributes.
         * <P>
         * This attributes require the mesurement of characters widths when rendering
         * such as underline.
         */
        protected Dictionary<string,object> attributes = new Dictionary<string,object>();
    
        /**
         * Non metric attributes.
         * <P>
         * This attributes do not require the mesurement of characters widths when rendering
         * such as BaseColor.
         */
        protected Dictionary<string,object> noStroke = new Dictionary<string,object>();
    
        /** <CODE>true</CODE> if the chunk split was cause by a newline. */
        protected bool newlineSplit;
    
        /** The image in this <CODE>PdfChunk</CODE>, if it has one */
        protected Image image;
        protected float imageScalePercentage = 1.0f;
    
        /** The offset in the x direction for the image */
        protected float offsetX;
    
        /** The offset in the y direction for the image */
        protected float offsetY;

        /** Indicates if the height and offset of the Image has to be taken into account */
        protected bool changeLeading = false;

        /** The leading that can overrule the existing leading. */
        protected float leading = 0;
        internal IAccessibleElement accessibleElement = null;

        public const float UNDERLINE_THICKNESS = 1f / 15;
        public const float UNDERLINE_OFFSET = -1f / 3;

        // constructors
    
        /**
         * Constructs a <CODE>PdfChunk</CODE>-object.
         *
         * @param string the content of the <CODE>PdfChunk</CODE>-object
         * @param font the <CODE>PdfFont</CODE>
         * @param attributes the metrics attributes
         * @param noStroke the non metric attributes
         */
    
        internal PdfChunk(string str, PdfChunk other) {
            value = str;
            this.font = other.font;
            this.attributes = other.attributes;
            this.noStroke = other.noStroke;
            this.baseFont = other.baseFont;
            this.changeLeading = other.changeLeading;
            this.leading = other.leading;
            Object[] obj = null;
            if (attributes.ContainsKey(Chunk.IMAGE))
                obj = (Object[])attributes[Chunk.IMAGE];
            if (obj == null)
                image = null;
            else {
                image = (Image)obj[0];
                offsetX = (float)obj[1];
                offsetY = (float)obj[2];
                changeLeading = (bool)obj[3];
            }
            encoding = font.Font.Encoding;
            if (noStroke.ContainsKey(Chunk.SPLITCHARACTER))
                splitCharacter = (ISplitCharacter)noStroke[Chunk.SPLITCHARACTER];
            else
                splitCharacter = DefaultSplitCharacter.DEFAULT;
            accessibleElement = other.accessibleElement;
        }
    
        /**
         * Constructs a <CODE>PdfChunk</CODE>-object.
         *
         * @param chunk the original <CODE>Chunk</CODE>-object
         * @param action the <CODE>PdfAction</CODE> if the <CODE>Chunk</CODE> comes from an <CODE>Anchor</CODE>
         */
   
        internal PdfChunk(Chunk chunk, PdfAction action) {
            value = chunk.Content;
        
            Font f = chunk.Font;
            float size = f.Size;
            if (size == iTextSharp.text.Font.UNDEFINED)
                size = 12;
            baseFont = f.BaseFont;
            //BaseFont bf = f.BaseFont;
            int style = f.Style;
            if (style == iTextSharp.text.Font.UNDEFINED) {
                style = iTextSharp.text.Font.NORMAL;
            }
            if (baseFont == null) {
                // translation of the font-family to a PDF font-family
                baseFont = f.GetCalculatedBaseFont(false);
            }
            else{
                // bold simulation
                if ((style & iTextSharp.text.Font.BOLD) != 0)
                    attributes[Chunk.TEXTRENDERMODE] = new Object[]{PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE, size / 30f, null};
                // italic simulation
                if ((style & iTextSharp.text.Font.ITALIC) != 0)
                    attributes[Chunk.SKEW] = new float[]{0, ITALIC_ANGLE};
            }
            font = new PdfFont(baseFont, size);
            // other style possibilities
            Dictionary<string,object> attr = chunk.Attributes;
            if (attr != null) {
                foreach (KeyValuePair<string,object> entry in attr) {
                    string name = entry.Key;
                    if (keysAttributes.ContainsKey(name)) {
                        attributes[name] = entry.Value;
                    }
                    else if (keysNoStroke.ContainsKey(name)) {
                        noStroke[name] = entry.Value;
                    }
                }
                if (attr.ContainsKey(Chunk.GENERICTAG) && "".Equals(attr[Chunk.GENERICTAG])) {
                    attributes[Chunk.GENERICTAG] = chunk.Content;
                }
            }
            if (f.IsUnderlined()) {
                Object[] obj = { null, new float[] { 0, UNDERLINE_THICKNESS, 0, UNDERLINE_OFFSET, 0 } };
                Object[][] obja = null;
                if (attributes.ContainsKey(Chunk.UNDERLINE))
                    obja = (Object[][])attributes[Chunk.UNDERLINE];
                Object[][] unders = Utilities.AddToArray(obja, obj);
                attributes[Chunk.UNDERLINE] = unders;
            }
            if (f.IsStrikethru()) {
                Object[] obj = {null, new float[]{0, 1f / 15, 0, 1f / 3, 0}};
                Object[][] obja = null;
                if (attributes.ContainsKey(Chunk.UNDERLINE))
                    obja = (Object[][])attributes[Chunk.UNDERLINE];
                Object[][] unders = Utilities.AddToArray(obja, obj);
                attributes[Chunk.UNDERLINE] = unders;
            }
            if (action != null)
                attributes[Chunk.ACTION] = action;
            // the color can't be stored in a PdfFont
            noStroke[Chunk.COLOR] = f.Color;
            noStroke[Chunk.ENCODING] = font.Font.Encoding;
            Object lh;
            if (attributes.TryGetValue(Chunk.LINEHEIGHT, out lh)) {
                changeLeading = true;
                leading = (float)lh;
            }
        
            Object[] obj2 = null;
            if (attributes.ContainsKey(Chunk.IMAGE))
                obj2 = (Object[])attributes[Chunk.IMAGE];
            if (obj2 == null)
                image = null;
            else {
                attributes.Remove(Chunk.HSCALE); // images are scaled in other ways
                image = (Image)obj2[0];
                offsetX = ((float)obj2[1]);
                offsetY = ((float)obj2[2]);
                changeLeading = (bool)obj2[3];
            }
            object hs;
            if (attributes.TryGetValue(Chunk.HSCALE, out hs))
                font.HorizontalScaling = (float)hs;
            encoding = font.Font.Encoding;
            if (noStroke.ContainsKey(Chunk.SPLITCHARACTER))
                splitCharacter = (ISplitCharacter)noStroke[Chunk.SPLITCHARACTER];
            else
                splitCharacter = DefaultSplitCharacter.DEFAULT;
            accessibleElement = chunk;
        }

        /**
         * Constructs a <CODE>PdfChunk</CODE>-object.
         *
         * @param chunk     the original <CODE>Chunk</CODE>-object
         * @param action    the <CODE>PdfAction</CODE> if the <CODE>Chunk</CODE> comes from an <CODE>Anchor</CODE>
         * @param tabSettings  the Phrase tab settings
         */
        internal PdfChunk(Chunk chunk, PdfAction action, TabSettings tabSettings)
            : this(chunk, action) {
            if (tabSettings != null) {
                if (!attributes.ContainsKey(Chunk.TABSETTINGS)
                    || attributes[Chunk.TABSETTINGS] == null)
                attributes[Chunk.TABSETTINGS] = tabSettings;
            }
        }


        // methods
    
        /** Gets the Unicode equivalent to a CID.
        * The (inexistent) CID <FF00> is translated as '\n'. 
        * It has only meaning with CJK fonts with Identity encoding.
        * @param c the CID code
        * @return the Unicode equivalent
        */    
        virtual public int GetUnicodeEquivalent(int c) {
            return baseFont.GetUnicodeEquivalent(c);
        }

        virtual protected int GetWord(string text, int start) {
            int len = text.Length;
            while (start < len) {
                if (!char.IsLetter(text[start]))
                    break;
                ++start;
            }
            return start;
        }
    
        /**
         * Splits this <CODE>PdfChunk</CODE> if it's too long for the given width.
         * <P>
         * Returns <VAR>null</VAR> if the <CODE>PdfChunk</CODE> wasn't truncated.
         *
         * @param       width       a given width
         * @return      the <CODE>PdfChunk</CODE> that doesn't fit into the width.
         */
    
        internal PdfChunk Split(float width) {
            newlineSplit = false;
            if (image != null) {
                if (image.ScaledWidth > width) {
                    PdfChunk pc = new PdfChunk(Chunk.OBJECT_REPLACEMENT_CHARACTER, this);
                    value = "";
                    attributes = new Dictionary<string,object>();
                    image = null;
                    font = PdfFont.DefaultFont;
                    return pc;
                }
                else
                    return null;
            }
            IHyphenationEvent hyphenationEvent = null;
            if (noStroke.ContainsKey(Chunk.HYPHENATION))
                hyphenationEvent = (IHyphenationEvent)noStroke[Chunk.HYPHENATION];
            int currentPosition = 0;
            int splitPosition = -1;
            float currentWidth = 0;
        
            // loop over all the characters of a string
            // or until the totalWidth is reached
            int lastSpace = -1;
            float lastSpaceWidth = 0;
            int length = value.Length;
            char[] valueArray = value.ToCharArray();
            char character = (char)0;
            BaseFont ft = font.Font;
            bool surrogate = false;
            if (ft.FontType == BaseFont.FONT_TYPE_CJK && ft.GetUnicodeEquivalent(' ') != ' ') {
                while (currentPosition < length) {
                    // the width of every character is added to the currentWidth
                    char cidChar = valueArray[currentPosition];
                    character = (char)ft.GetUnicodeEquivalent(cidChar);
                    // if a newLine or carriageReturn is encountered
                    if (character == '\n') {
                        newlineSplit = true;
                        string returnValue = value.Substring(currentPosition + 1);
                        value = value.Substring(0, currentPosition);
                        if (value.Length < 1) {
                            value = "\u0001";
                        }
                        PdfChunk pc = new PdfChunk(returnValue, this);
                        return pc;
                    }
                    currentWidth += GetCharWidth(cidChar);
                    if (character == ' ') {
                        lastSpace = currentPosition + 1;
                        lastSpaceWidth = currentWidth;
                    }
                    if (currentWidth > width)
                        break;
                    // if a split-character is encountered, the splitPosition is altered
                    if (splitCharacter.IsSplitCharacter(0, currentPosition, length, valueArray, new PdfChunk[] {this}))
                        splitPosition = currentPosition + 1;
                    currentPosition++;
                }
            }
            else {
                while (currentPosition < length) {
                    // the width of every character is added to the currentWidth
                    character = valueArray[currentPosition];
                    // if a newLine or carriageReturn is encountered
                    if (character == '\r' || character == '\n') {
                        newlineSplit = true;
                        int inc = 1;
                        if (character == '\r' && currentPosition + 1 < length && valueArray[currentPosition + 1] == '\n')
                            inc = 2;
                        string returnValue = value.Substring(currentPosition + inc);
                        value = value.Substring(0, currentPosition);
                        if (value.Length < 1) {
                            value = " ";
                        }
                        PdfChunk pc = new PdfChunk(returnValue, this);
                        return pc;
                    }
                    surrogate = Utilities.IsSurrogatePair(valueArray, currentPosition);
                    if (surrogate)
                        currentWidth += GetCharWidth(Utilities.ConvertToUtf32(valueArray[currentPosition], valueArray[currentPosition + 1]));
                    else
                        currentWidth += GetCharWidth(character);
                    if (character == ' ') {
                        lastSpace = currentPosition + 1;
                        lastSpaceWidth = currentWidth;
                    }
                    if (surrogate)
                        currentPosition++;
                    if (currentWidth > width)
                        break;
                    // if a split-character is encountered, the splitPosition is altered
                    if (splitCharacter.IsSplitCharacter(0, currentPosition, length, valueArray, null))
                        splitPosition = currentPosition + 1;
                    currentPosition++;
                }
            }
        
            // if all the characters fit in the total width, null is returned (there is no overflow)
            if (currentPosition == length) {
                return null;
            }
            // otherwise, the string has to be truncated
            if (splitPosition < 0) {
                string returnValue = value;
                value = "";
                PdfChunk pc = new PdfChunk(returnValue, this);
                return pc;
            }
            if (lastSpace > splitPosition && splitCharacter.IsSplitCharacter(0, 0, 1, singleSpace, null))
                splitPosition = lastSpace;
            if (hyphenationEvent != null && lastSpace >= 0 && lastSpace < currentPosition) {
                int wordIdx = GetWord(value, lastSpace);
                if (wordIdx > lastSpace) {
                    string pre = hyphenationEvent.GetHyphenatedWordPre(value.Substring(lastSpace, wordIdx - lastSpace), font.Font, font.Size, width - lastSpaceWidth);
                    string post = hyphenationEvent.HyphenatedWordPost;
                    if (pre.Length > 0) {
                        string returnValue = post + value.Substring(wordIdx);
                        value = Trim(value.Substring(0, lastSpace) + pre);
                        PdfChunk pc = new PdfChunk(returnValue, this);
                        return pc;
                    }
                }
            }
            string retVal = value.Substring(splitPosition);
            value = Trim(value.Substring(0, splitPosition));
            PdfChunk tmp = new PdfChunk(retVal, this);
            return tmp;
        }
    
        /**
         * Truncates this <CODE>PdfChunk</CODE> if it's too long for the given width.
         * <P>
         * Returns <VAR>null</VAR> if the <CODE>PdfChunk</CODE> wasn't truncated.
         *
         * @param       width       a given width
         * @return      the <CODE>PdfChunk</CODE> that doesn't fit into the width.
         */
    
        internal PdfChunk Truncate(float width) {
            if (image != null) {
                if (image.ScaledWidth > width) {
                    // Image does not fit the line, resize if requested
                    if (image.ScaleToFitLineWhenOverflow) {
                        //float scalePercent = width / image.Width * 100;
                        //image.ScalePercent(scalePercent);
                        this.ImageScalePercentage = width / image.Width;
                        return null;
                    }
                    PdfChunk pc = new PdfChunk("", this);
                    value = "";
                    attributes.Remove(Chunk.IMAGE);
                    image = null;
                    font = PdfFont.DefaultFont;
                    return pc;
                }
                else
                    return null;
            }
        
            int currentPosition = 0;
            float currentWidth = 0;
        
            // it's no use trying to split if there isn't even enough place for a space
            if (width < font.Width()) {
                string returnValue = value.Substring(1);
                value = value.Substring(0, 1);
                PdfChunk pc = new PdfChunk(returnValue, this);
                return pc;
            }
        
            // loop over all the characters of a string
            // or until the totalWidth is reached
            int length = value.Length;
            bool surrogate = false;
            while (currentPosition < length) {
                // the width of every character is added to the currentWidth
                surrogate = Utilities.IsSurrogatePair(value, currentPosition);
                if (surrogate)
                    currentWidth += GetCharWidth(Utilities.ConvertToUtf32(value, currentPosition));
                else
                    currentWidth += GetCharWidth(value[currentPosition]);
                if (currentWidth > width)
                    break;
                if (surrogate)
                    currentPosition++;
                currentPosition++;
            }
        
            // if all the characters fit in the total width, null is returned (there is no overflow)
            if (currentPosition == length) {
                return null;
            }
        
            // otherwise, the string has to be truncated
            //currentPosition -= 2;
            // we have to chop off minimum 1 character from the chunk
            if (currentPosition == 0) {
                currentPosition = 1;
                if (surrogate)
                    ++currentPosition;
            }
            string retVal = value.Substring(currentPosition);
            value = value.Substring(0, currentPosition);
            PdfChunk tmp = new PdfChunk(retVal, this);
            return tmp;
        }
    
        // methods to retrieve the membervariables
    
        /**
         * Returns the font of this <CODE>Chunk</CODE>.
         *
         * @return  a <CODE>PdfFont</CODE>
         */
    
        internal PdfFont Font {
            get {
                return font;
            }
        }
    
        /**
         * Returns the color of this <CODE>Chunk</CODE>.
         *
         * @return  a <CODE>BaseColor</CODE>
         */
    
        internal BaseColor Color {
            get {
                if (noStroke.ContainsKey(Chunk.COLOR))
                    return (BaseColor)noStroke[Chunk.COLOR];
                else
                    return null;
            }
        }
    
        /**
         * Returns the width of this <CODE>PdfChunk</CODE>.
         *
         * @return  a width
         */

        internal float Width()
        {
            return Width(value);
        }

        internal float Width(string str) {
            if (IsAttribute(Chunk.SEPARATOR))
                return 0;
            if (IsImage())
                return ImageWidth;

            float width = font.Width(str);

            if (IsAttribute(Chunk.CHAR_SPACING))
            {
                float cs = (float)GetAttribute(Chunk.CHAR_SPACING);
                width += str.Length * cs;
            }
            if (IsAttribute(Chunk.WORD_SPACING))
            {
                int numberOfSpaces = 0;
                int idx = -1;
                while ((idx = str.IndexOf(' ', idx + 1)) >= 0)
                    ++numberOfSpaces;
                float ws = (float)GetAttribute(Chunk.WORD_SPACING);
                width += numberOfSpaces * ws;
            }
            return width;
        }

        internal float Height()
        {
            return IsImage() ? ImageHeight : font.Size;
        }


        /**
         * Checks if the <CODE>PdfChunk</CODE> split was caused by a newline.
         * @return <CODE>true</CODE> if the <CODE>PdfChunk</CODE> split was caused by a newline.
         */
    
        virtual public bool IsNewlineSplit() {
            return newlineSplit;
        }
    
        /**
         * Gets the width of the <CODE>PdfChunk</CODE> taking into account the
         * extra character and word spacing.
         * @param charSpacing the extra character spacing
         * @param wordSpacing the extra word spacing
         * @return the calculated width
         */
    
        virtual public float GetWidthCorrected(float charSpacing, float wordSpacing) {
            if (image != null) {
                return image.ScaledWidth + charSpacing;
            }
            int numberOfSpaces = 0;
            int idx = -1;
            while ((idx = value.IndexOf(' ', idx + 1)) >= 0)
                ++numberOfSpaces;
            return font.Width(value) + value.Length * charSpacing + numberOfSpaces * wordSpacing;
        }
        
        /**
        * Gets the text displacement relatiev to the baseline.
        * @return a displacement in points
        */
        virtual public float TextRise {
            get {
                object f = GetAttribute(Chunk.SUBSUPSCRIPT);
                if (f != null) {
                    return (float)f;
                }
                return 0.0f;
            }
        }
    
        /**
         * Trims the last space.
         * @return the width of the space trimmed, otherwise 0
         */
    
        virtual public float TrimLastSpace() {
            BaseFont ft = font.Font;
            if (ft.FontType == BaseFont.FONT_TYPE_CJK && ft.GetUnicodeEquivalent(' ') != ' ') {
                if (value.Length > 1 && value.EndsWith("\u0001")) {
                    value = value.Substring(0, value.Length - 1);
                    return font.Width('\u0001');
                }
            }
            else {
                if (value.Length > 1 && value.EndsWith(" ")) {
                    value = value.Substring(0, value.Length - 1);
                    return font.Width(' ');
                }
            }
            return 0;
        }
    
        virtual public float TrimFirstSpace()
        {
            BaseFont ft = font.Font;
            if (ft.FontType == BaseFont.FONT_TYPE_CJK && ft.GetUnicodeEquivalent(' ') != ' ') {
                if (value.Length > 1 && value.StartsWith("\u0001")) {
                    value = value.Substring(1);
                    return font.Width('\u0001');
                }
            }
            else {
                if (value.Length > 1 && value.StartsWith(" ")) {
                    value = value.Substring(1);
                    return font.Width(' ');
                }
            }
            return 0;
        }
    
        /**
         * Gets an attribute. The search is made in <CODE>attributes</CODE>
         * and <CODE>noStroke</CODE>.
         * @param name the attribute key
         * @return the attribute value or null if not found
         */
    
        internal Object GetAttribute(string name) {
            if (attributes.ContainsKey(name))
                return attributes[name];
            if (noStroke.ContainsKey(name))
                return noStroke[name];
            return null;
        }
    
        /**
         *Checks if the attribute exists.
         * @param name the attribute key
         * @return <CODE>true</CODE> if the attribute exists
         */
    
        internal bool IsAttribute(string name) {
            if (attributes.ContainsKey(name))
                return true;
            return noStroke.ContainsKey(name);
        }
    
        /**
         * Checks if this <CODE>PdfChunk</CODE> needs some special metrics handling.
         * @return <CODE>true</CODE> if this <CODE>PdfChunk</CODE> needs some special metrics handling.
         */
    
        internal bool IsStroked() {
            return (attributes.Count > 0);
        }
    
        /**
        * Checks if this <CODE>PdfChunk</CODE> is a Separator Chunk.
        * @return  true if this chunk is a separator.
        * @since   2.1.2
        */
        internal bool IsSeparator() {
            return IsAttribute(Chunk.SEPARATOR);
        }
        
        /**
        * Checks if this <CODE>PdfChunk</CODE> is a horizontal Separator Chunk.
        * @return  true if this chunk is a horizontal separator.
        * @since   2.1.2
        */
        internal bool IsHorizontalSeparator() {
            if (IsAttribute(Chunk.SEPARATOR)) {
                Object[] o = (Object[])GetAttribute(Chunk.SEPARATOR);
                return !(bool)o[1];
            }
            return false;
        }
        
        /**
        * Checks if this <CODE>PdfChunk</CODE> is a tab Chunk.
        * @return  true if this chunk is a separator.
        * @since   2.1.2
        */
        internal bool IsTab() {
            return IsAttribute(Chunk.TAB);
        }

        /**
        * Correction for the tab position based on the left starting position.
        * @param   newValue    the new value for the left X.
        * @since   2.1.2
        */
        [Obsolete]
        internal void AdjustLeft(float newValue) {
            if (attributes.ContainsKey(Chunk.TAB)) {
                Object[] o = (Object[])attributes[Chunk.TAB];
                attributes[Chunk.TAB] = new Object[]{o[0], o[1], o[2], newValue};
            }
        }

        internal static TabStop GetTabStop(PdfChunk tab, float tabPosition)
        {
            TabStop tabStop = null;
            object o;
            if (tab.attributes.TryGetValue(Chunk.TAB, out o))
            {
                float tabInterval = (float)((object[])o)[0];
                if (float.IsNaN(tabInterval))
                {
                    object obj;
                    tab.attributes.TryGetValue(Chunk.TABSETTINGS, out obj);
                    tabStop = TabSettings.getTabStopNewInstance(tabPosition, (TabSettings)obj);
                }
                else
                    tabStop = TabStop.NewInstance(tabPosition, tabInterval);
            }
            return tabStop;
        }

        internal TabStop TabStop
        {
            get
            {
                object obj;
                if (attributes.TryGetValue(TABSTOP, out obj))
                    return (TabStop) obj;
                return null;
            } 
            set { attributes[TABSTOP] = value; }
        }


        /**
         * Checks if there is an image in the <CODE>PdfChunk</CODE>.
         * @return <CODE>true</CODE> if an image is present
         */
    
        internal bool IsImage() {
            return image != null;
        }
    
        /**
         * Gets the image in the <CODE>PdfChunk</CODE>.
         * @return the image or <CODE>null</CODE>
         */
    
        internal Image Image {
            get {
                return image;
            }
        }

        internal float ImageHeight
        {
            get { return image.ScaledHeight*imageScalePercentage; }
            
        }

        internal float ImageWidth
        {
            get { return image.ScaledWidth* imageScalePercentage; }
        }

        /**
         * Returns a scalePercentage in case the image needs to be scaled.
         * Sets a scale percentage in case the image needs to be scaled.
         */
        virtual public float ImageScalePercentage
        {
            get { return imageScalePercentage; }
            set { imageScalePercentage = value; }
        }

        /**
         * Gets the image offset in the x direction
         * @return the image offset in the x direction
         */
    
        internal float ImageOffsetX {
            get {
                return offsetX;
            }

            set {
                this.offsetX = value;
            }
        }
    
        /**
         * Gets the image offset in the y direction
         * @return Gets the image offset in the y direction
         */
    
        internal float ImageOffsetY {
            get {
                return offsetY;
            }

            set {
                this.offsetY = value;
            }
        }
    
        /**
         * sets the value.
         */
    
        internal string Value {
            set {
                this.value = value;
            }
        }

        public override string ToString() {
            return value;
        }

        /**
         * Tells you if this string is in Chinese, Japanese, Korean or Identity-H.
         */
    
        internal bool IsSpecialEncoding() {
            return encoding.Equals(CJKFont.CJK_ENCODING) || encoding.Equals(BaseFont.IDENTITY_H);
        }
    
        /**
         * Gets the encoding of this string.
         *
         * @return      a <CODE>string</CODE>
         */
    
        internal string Encoding {
            get {
                return encoding;
            }
        }

        internal int Length {
            get {
                return value.Length;
            }
        }

        internal int LengthUtf32 {
            get {
                if (!BaseFont.IDENTITY_H.Equals(encoding))
                    return value.Length;
                int total = 0;
                int len = value.Length;
                for (int k = 0; k < len; ++k) {
                    if (Utilities.IsSurrogateHigh(value[k]))
                        ++k;
                    ++total;
                }
                return total;
            }
        }

        internal bool IsExtSplitCharacter(int start, int current, int end, char[] cc, PdfChunk[] ck) {
            return splitCharacter.IsSplitCharacter(start, current, end, cc, ck);
        }
    
        /**
         * Removes all the <VAR>' '</VAR> and <VAR>'-'</VAR>-characters on the right of a <CODE>string</CODE>.
         * <P>
         * @param   string      the <CODE>string<CODE> that has to be trimmed.
         * @return  the trimmed <CODE>string</CODE>
         */    
        internal string Trim(string str) {
            BaseFont ft = font.Font;
            if (ft.FontType == BaseFont.FONT_TYPE_CJK && ft.GetUnicodeEquivalent(' ') != ' ') {
                while (str.EndsWith("\u0001")) {
                    str = str.Substring(0, str.Length - 1);
                }
            }
            else {
                while (str.EndsWith(" ") || str.EndsWith("\t")) {
                    str = str.Substring(0, str.Length - 1);
                }
            }
            return str;
        }

        virtual public bool ChangeLeading {
            get {
                return changeLeading;
            }
        }


        virtual public float Leading {
            get {
                return leading;
            }
        }
    
        internal float GetCharWidth(int c) {
            if (NoPrint(c))
                return 0;
            if (IsAttribute(Chunk.CHAR_SPACING)) {
        	    float cs = (float) GetAttribute(Chunk.CHAR_SPACING);
			    return font.Width(c) + cs * font.HorizontalScaling;
		    }
            if (IsImage())
                return ImageWidth;
            return font.Width(c);
        }
    
        public static bool NoPrint(int c) {
            return ((c >= 0x200b && c <= 0x200f) || (c >= 0x202a && c <= 0x202e));
        }    
    }
}
