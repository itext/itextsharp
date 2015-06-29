using System;

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

namespace iTextSharp.text.pdf.codec.wmf {
    public class MetaFont : MetaObject {
        static string[] fontNames = {
                                        "Courier", "Courier-Bold", "Courier-Oblique", "Courier-BoldOblique",
                                        "Helvetica", "Helvetica-Bold", "Helvetica-Oblique", "Helvetica-BoldOblique",
                                        "Times-Roman", "Times-Bold", "Times-Italic", "Times-BoldItalic",
                                        "Symbol", "ZapfDingbats"};

        internal const int MARKER_BOLD = 1;
        internal const int MARKER_ITALIC = 2;
        internal const int MARKER_COURIER = 0;
        internal const int MARKER_HELVETICA = 4;
        internal const int MARKER_TIMES = 8;
        internal const int MARKER_SYMBOL = 12;

        internal const int DEFAULT_PITCH = 0;
        internal const int FIXED_PITCH = 1;
        internal const int VARIABLE_PITCH = 2;
        internal const int FF_DONTCARE = 0;
        internal const int FF_ROMAN = 1;
        internal const int FF_SWISS = 2;
        internal const int FF_MODERN = 3;
        internal const int FF_SCRIPT = 4;
        internal const int FF_DECORATIVE = 5;
        internal const int BOLDTHRESHOLD = 600;    
        internal const int nameSize = 32;
        internal const int ETO_OPAQUE = 2;
        internal const int ETO_CLIPPED = 4;

        int height;
        float angle;
        int bold;
        int italic;
        bool underline;
        bool strikeout;
        int charset;
        int pitchAndFamily;
        string faceName = "arial";
        BaseFont font = null;

        public MetaFont() {
            type = META_FONT;
        }

        virtual public void Init(InputMeta meta) {
            height = Math.Abs(meta.ReadShort());
            meta.Skip(2);
            angle = (float)(meta.ReadShort() / 1800.0 * Math.PI);
            meta.Skip(2);
            bold = (meta.ReadShort() >= BOLDTHRESHOLD ? MARKER_BOLD : 0);
            italic = (meta.ReadByte() != 0 ? MARKER_ITALIC : 0);
            underline = (meta.ReadByte() != 0);
            strikeout = (meta.ReadByte() != 0);
            charset = meta.ReadByte();
            meta.Skip(3);
            pitchAndFamily = meta.ReadByte();
            byte[] name = new byte[nameSize];
            int k;
            for (k = 0; k < nameSize; ++k) {
                int c = meta.ReadByte();
                if (c == 0) {
                    break;
                }
                name[k] = (byte)c;
            }
            try {
                faceName = System.Text.Encoding.GetEncoding(1252).GetString(name, 0, k);
            }
            catch {
                faceName = System.Text.ASCIIEncoding.ASCII.GetString(name, 0, k);
            }
            faceName = faceName.ToLower(System.Globalization.CultureInfo.InvariantCulture);
        }
    
        virtual public BaseFont Font {
            get {
                if (font != null)
                    return font;
                iTextSharp.text.Font ff2 = FontFactory.GetFont(faceName, BaseFont.CP1252, true, 10, ((italic != 0) ? iTextSharp.text.Font.ITALIC : 0) | ((bold != 0) ? iTextSharp.text.Font.BOLD : 0));
                font = ff2.BaseFont;
                if (font != null)
                    return font;
                string fontName;
                if (faceName.IndexOf("courier") != -1 || faceName.IndexOf("terminal") != -1
                    || faceName.IndexOf("fixedsys") != -1) {
                    fontName = fontNames[MARKER_COURIER + italic + bold];
                }
                else if (faceName.IndexOf("ms sans serif") != -1 || faceName.IndexOf("arial") != -1
                    || faceName.IndexOf("system") != -1) {
                    fontName = fontNames[MARKER_HELVETICA + italic + bold];
                }
                else if (faceName.IndexOf("arial black") != -1) {
                    fontName = fontNames[MARKER_HELVETICA + italic + MARKER_BOLD];
                }
                else if (faceName.IndexOf("times") != -1 || faceName.IndexOf("ms serif") != -1
                    || faceName.IndexOf("roman") != -1) {
                    fontName = fontNames[MARKER_TIMES + italic + bold];
                }
                else if (faceName.IndexOf("symbol") != -1) {
                    fontName = fontNames[MARKER_SYMBOL];
                }
                else {
                    int pitch = pitchAndFamily & 3;
                    int family = (pitchAndFamily >> 4) & 7;
                    switch (family) {
                        case FF_MODERN:
                            fontName = fontNames[MARKER_COURIER + italic + bold];
                            break;
                        case FF_ROMAN:
                            fontName = fontNames[MARKER_TIMES + italic + bold];
                            break;
                        case FF_SWISS:
                        case FF_SCRIPT:
                        case FF_DECORATIVE:
                            fontName = fontNames[MARKER_HELVETICA + italic + bold];
                            break;
                        default: {
                            switch (pitch) {
                                case FIXED_PITCH:
                                    fontName = fontNames[MARKER_COURIER + italic + bold];
                                    break;
                                default:
                                    fontName = fontNames[MARKER_HELVETICA + italic + bold];
                                    break;
                            }
                            break;
                        }
                    }
                }
                font = BaseFont.CreateFont(fontName, BaseFont.CP1252, false);
        
                return font;
            }
        }
    
        virtual public float Angle {
            get {
                return angle;
            }
        }
    
        virtual public bool IsUnderline() {
            return underline;
        }
    
        virtual public bool IsStrikeout() {
            return strikeout;
        }
    
        virtual public float GetFontSize(MetaState state) {
            return Math.Abs(state.TransformY(height) - state.TransformY(0)) * Document.WmfFontCorrection;
        }
    }
}
