using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using iTextSharp.text.error_messages;
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
    /** Selects the appropriate fonts that contain the glyphs needed to
    * render text correctly. The fonts are checked in order until the 
    * character is found.
    * <p>
    * The built in fonts "Symbol" and "ZapfDingbats", if used, have a special encoding
    * to allow the characters to be referred by Unicode.
    * @author Paulo Soares
    */
    public class FontSelector {
        
        protected List<Font> fonts = new List<Font>();
        protected Font currentFont = null;

        /**
        * Adds a <CODE>Font</CODE> to be searched for valid characters.
        * @param font the <CODE>Font</CODE>
        */    
        virtual public void AddFont(Font font) {
            if (font.BaseFont != null) {
                fonts.Add(font);
                return;
            }
            BaseFont bf = font.GetCalculatedBaseFont(true);
            Font f2 = new Font(bf, font.Size, font.CalculatedStyle, font.Color);
            fonts.Add(f2);
        }
        
        /**
        * Process the text so that it will render with a combination of fonts
        * if needed.
        * @param text the text
        * @return a <CODE>Phrase</CODE> with one or more chunks
        */
        public virtual Phrase Process(String text) {
            if (fonts.Count == 0)
                throw new ArgumentOutOfRangeException(MessageLocalization.GetComposedMessage("no.font.is.defined"));
            char[] cc = text.ToCharArray();
            int len = cc.Length;
            StringBuilder sb = new StringBuilder();
            Phrase ret = new Phrase();
            currentFont = null;
            for (int k = 0; k < len; ++k) {
                Chunk newChunk = ProcessChar(cc, k, sb);
                if (newChunk != null) {
                    ret.Add(newChunk);
                }
            }
            if (sb.Length > 0) {
                Chunk ck = new Chunk(sb.ToString(), currentFont ?? fonts[0]);
                ret.Add(ck);
            }
            return ret;
        }

        protected virtual Chunk ProcessChar(char[] cc, int k, StringBuilder sb) {
            Chunk newChunk = null;
            char c = cc[k];
            if(c == '\n' || c == '\r') {
                sb.Append(c);
            }
            else {
                Font font = null;
                if(Utilities.IsSurrogatePair(cc, k)) {
                    int u = Utilities.ConvertToUtf32(cc, k);
                    for(int f = 0; f < fonts.Count; ++f) {
                        font = fonts[f];
                        if (font.BaseFont.CharExists(u) ||
                            CharUnicodeInfo.GetUnicodeCategory(char.ConvertFromUtf32(u), 0) == UnicodeCategory.Format) {
                            if (currentFont != font) {
                                if (sb.Length > 0 && currentFont != null) {
                                    newChunk = new Chunk(sb.ToString(), currentFont);
                                    sb.Length = 0;
                                }
                                currentFont = font;
                            }
                            sb.Append(c);
                            sb.Append(cc[++k]);
                            break;
                        }
                    }
                }
                else {
                    for(int f = 0; f < fonts.Count; ++f) {
                        font = fonts[f];
                        if(font.BaseFont.CharExists(c) || char.GetUnicodeCategory(c) == UnicodeCategory.Format) {
                            if(currentFont != font) {
                                if(sb.Length > 0 && currentFont != null) {
                                    newChunk = new Chunk(sb.ToString(), currentFont);
                                    sb.Length = 0;
                                }
                                currentFont = font;
                            }
                            sb.Append(c);
                            break;
                        }
                    }
                }
            }
            return newChunk;
        }
    }
}
