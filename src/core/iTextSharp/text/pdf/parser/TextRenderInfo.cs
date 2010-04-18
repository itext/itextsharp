using System;
using System.Collections;
using System.Collections.Generic;
using iTextSharp.text.pdf;
/*
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2009 1T3XT BVBA
 * Authors: Kevin Day, Bruno Lowagie, Paulo Soares, et al.
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
namespace iTextSharp.text.pdf.parser {

    /**
     * Provides information and calculations needed by render listeners
     * to display/evaluate text render operations.
     * <br><br>
     * This is passed between the {@link PdfContentStreamProcessor} and 
     * {@link RenderListener} objects as text rendering operations are
     * discovered
     */
    public class TextRenderInfo {
        
        private String text;
        private Matrix textToUserSpaceTransformMatrix;
        private GraphicsState gs;
        /**
         * Array containing marked content info for the text.
         * @since 5.0.2
         */
        private ICollection<MarkedContentInfo> markedContentInfos;
        
        /**
         * Creates a new TextRenderInfo object
         * @param text the text that should be displayed
         * @param gs the graphics state (note: at this time, this is not immutable, so don't cache it)
         * @param textMatrix the text matrix at the time of the render operation
         * @param markedContentInfo the marked content sequence, if available
         */
        internal TextRenderInfo(String text, GraphicsState gs, Matrix textMatrix, ICollection markedContentInfo) {
            this.text = text;
            this.textToUserSpaceTransformMatrix = textMatrix.Multiply(gs.ctm);
            this.gs = gs;
            this.markedContentInfos = new List<MarkedContentInfo>();
            foreach (MarkedContentInfo m in markedContentInfo) {
                this.markedContentInfos.Add(m);
            }
        }
        
        /**
         * @return the text to render
         */
        public String GetText(){ 
            return text; 
        }

        /**
         * Checks if the text belongs to a marked content sequence
         * with a given mcid.
         * @param mcid a marked content id
         * @return true if the text is marked with this id
         * @since 5.0.2
         */
        public bool HasMcid(int mcid) {
            foreach (MarkedContentInfo info in markedContentInfos) {
                if (info.HasMcid())
                    if (info.GetMcid() == mcid)
                        return true;
            }
            return false;
        }

        /**
         * @return the unscaled (i.e. in Text space) width of the text
         */
        internal float GetUnscaledWidth(){ 
            return GetStringWidth(text); 
        }
        
        /**
         * Gets the baseline for the text (i.e. the line that the text 'sits' on)
         * @return the baseline line segment
         * @since 5.0.2
         */
        public LineSegment GetBaseline(){
            return GetUnscaledBaselineWithOffset(0).TransformBy(textToUserSpaceTransformMatrix);
        }
        
        /**
         * Gets the ascentline for the text (i.e. the line that represents the topmost extent that a string of the current font could have)
         * @return the ascentline line segment
         * @since 5.0.2
         */
        public LineSegment GetAscentLine(){
            float ascent = gs.GetFont().GetFontDescriptor(BaseFont.ASCENT, gs.GetFontSize());
            return GetUnscaledBaselineWithOffset(ascent).TransformBy(textToUserSpaceTransformMatrix);
        }
        
        /**
         * Gets the descentline for the text (i.e. the line that represents the bottom most extent that a string of the current font could have)
         * @return the descentline line segment
         * @since 5.0.2
         */
        public LineSegment GetDescentLine(){
            // per GetFontDescription() API, descent is returned as a negative number, so we apply that as a normal vertical offset
            float descent = gs.GetFont().GetFontDescriptor(BaseFont.DESCENT, gs.GetFontSize());
            return GetUnscaledBaselineWithOffset(descent).TransformBy(textToUserSpaceTransformMatrix);
        }
        
        private LineSegment GetUnscaledBaselineWithOffset(float yOffset){
            return new LineSegment(new Vector(0, yOffset, 1), new Vector(GetUnscaledWidth(), yOffset, 1));
        }

        /**
         * Getter for the font
         * @return the font
         * @since iText 5.0.2
         */
        public DocumentFont GetFont() {
            return gs.GetFont();
        }
        
        /**
         * @return The width, in user space units, of a single space character in the current font
         */
        public float GetSingleSpaceWidth(){
            LineSegment textSpace = new LineSegment(new Vector(0, 0, 1), new Vector(GetUnscaledFontSpaceWidth(), 0, 1));
            LineSegment userSpace = textSpace.TransformBy(textToUserSpaceTransformMatrix);
            return userSpace.GetLength();
        }
        
        /**
         * @return the text render mode that should be used for the text.  From the
         * PDF specification, this means:
         * <ul>
         *   <li>0 = Fill text</li>
         *   <li>1 = Stroke text</li>
         *   <li>2 = Fill, then stroke text</li>
         *   <li>3 = Invisible</li>
         *   <li>4 = Fill text and add to path for clipping</li>
         *   <li>5 = Stroke text and add to path for clipping</li>
         *   <li>6 = Fill, then stroke text and add to path for clipping</li>
         *   <li>7 = Add text to padd for clipping</li>
         * </ul>
         * @since iText 5.0.1
         */
        public int GetTextRenderMode(){
            return gs.renderMode;
        }
        
        /**
         * Calculates the width of a space character.  If the font does not define
         * a width for a standard space character \u0020, we also attempt to use
         * the width of \u00A0 (a non-breaking space in many fonts)
         * @return the width of a single space character in text space units
         */
        private float GetUnscaledFontSpaceWidth(){
            char charToUse = ' ';
            if (gs.font.GetWidth(charToUse) == 0)
                charToUse = '\u00A0';
            return GetStringWidth(charToUse.ToString());
        }
        
        /**
         * Gets the width of a String in text space units
         * @param string    the string that needs measuring
         * @return  the width of a String in text space units
         */
        private float GetStringWidth(String str){
            DocumentFont font = gs.font;
            char[] chars = str.ToCharArray();
            float totalWidth = 0;
            for (int i = 0; i < chars.Length; i++) {
                float w = font.GetWidth(chars[i]) / 1000.0f;
                float wordSpacing = chars[i] == 32 ? gs.wordSpacing : 0f;
                totalWidth += (w * gs.fontSize + gs.characterSpacing + wordSpacing) * gs.horizontalScaling;
            }
            
            return totalWidth;
        }        
    }
}