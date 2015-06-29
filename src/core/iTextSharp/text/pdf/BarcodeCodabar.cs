using System;
using iTextSharp.text.error_messages;
/*
 * $Id$
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

namespace iTextSharp.text.pdf
{
    /** Implements the code codabar. The default parameters are:
    * <pre>
    *x = 0.8f;
    *n = 2;
    *font = BaseFont.CreateFont("Helvetica", "winansi", false);
    *size = 8;
    *baseline = size;
    *barHeight = size * 3;
    *textAlignment = Element.ALIGN_CENTER;
    *generateChecksum = false;
    *checksumText = false;
    *startStopText = false;
    * </pre>
    *
    * @author Paulo Soares
    */
    public class BarcodeCodabar : Barcode{

        /** The bars to generate the code.
        */    
        private static readonly byte[][] BARS = new byte[][] {
            new byte[]{0,0,0,0,0,1,1}, // 0
            new byte[]{0,0,0,0,1,1,0}, // 1
            new byte[]{0,0,0,1,0,0,1}, // 2
            new byte[]{1,1,0,0,0,0,0}, // 3
            new byte[]{0,0,1,0,0,1,0}, // 4
            new byte[]{1,0,0,0,0,1,0}, // 5
            new byte[]{0,1,0,0,0,0,1}, // 6
            new byte[]{0,1,0,0,1,0,0}, // 7
            new byte[]{0,1,1,0,0,0,0}, // 8
            new byte[]{1,0,0,1,0,0,0}, // 9
            new byte[]{0,0,0,1,1,0,0}, // -
            new byte[]{0,0,1,1,0,0,0}, // $
            new byte[]{1,0,0,0,1,0,1}, // :
            new byte[]{1,0,1,0,0,0,1}, // /
            new byte[]{1,0,1,0,1,0,0}, // .
            new byte[]{0,0,1,0,1,0,1}, // +
            new byte[]{0,0,1,1,0,1,0}, // a
            new byte[]{0,1,0,1,0,0,1}, // b
            new byte[]{0,0,0,1,0,1,1}, // c
            new byte[]{0,0,0,1,1,1,0}  // d
        };
     
        /** The index chars to <CODE>BARS</CODE>.
        */    
        private const string CHARS = "0123456789-$:/.+ABCD";
        
        private const int START_STOP_IDX = 16;    
        /** Creates a new BarcodeCodabar.
        */    
        public BarcodeCodabar() {
            x = 0.8f;
            n = 2;
            font = BaseFont.CreateFont("Helvetica", "winansi", false);
            size = 8;
            baseline = size;
            barHeight = size * 3;
            textAlignment = Element.ALIGN_CENTER;
            generateChecksum = false;
            checksumText = false;
            startStopText = false;
            codeType = CODABAR;
        }
        
        /** Creates the bars.
        * @param text the text to create the bars
        * @return the bars
        */    
        public static byte[] GetBarsCodabar(String text) {
            text = text.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
            int len = text.Length;
            if (len < 2)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("codabar.must.have.at.least.a.start.and.stop.character"));
            if (CHARS.IndexOf(text[0]) < START_STOP_IDX || CHARS.IndexOf(text[len - 1]) < START_STOP_IDX)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("codabar.must.have.one.of.abcd.as.start.stop.character"));
            byte[] bars = new byte[text.Length * 8 - 1];
            for (int k = 0; k < len; ++k) {
                int idx = CHARS.IndexOf(text[k]);
                if (idx >= START_STOP_IDX && k > 0 && k < len - 1)
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("in.codabar.start.stop.characters.are.only.allowed.at.the.extremes"));
                if (idx < 0)
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("the.character.1.is.illegal.in.codabar", text[k]));
                Array.Copy(BARS[idx], 0, bars, k * 8, 7);
            }
            return bars;
        }
        
        public static String CalculateChecksum(String code) {
            if (code.Length < 2)
                return code;
            String text = code.ToUpper(System.Globalization.CultureInfo.InvariantCulture);
            int sum = 0;
            int len = text.Length;
            for (int k = 0; k < len; ++k)
                sum += CHARS.IndexOf(text[k]);
            sum = (sum + 15) / 16 * 16 - sum;
            return code.Substring(0, len - 1) + CHARS[sum] + code.Substring(len - 1);
        }
        
        /** Gets the maximum area that the barcode and the text, if
        * any, will occupy. The lower left corner is always (0, 0).
        * @return the size the barcode occupies.
        */    
        public override Rectangle BarcodeSize {
            get {
                float fontX = 0;
                float fontY = 0;
                String text = code;
                if (generateChecksum && checksumText)
                    text = CalculateChecksum(code);
                if (!startStopText)
                    text = text.Substring(1, text.Length - 2);
                if (font != null) {
                    if (baseline > 0)
                        fontY = baseline - font.GetFontDescriptor(BaseFont.DESCENT, size);
                    else
                        fontY = -baseline + size;
                    fontX = font.GetWidthPoint(altText != null ? altText : text, size);
                }
                text = code;
                if (generateChecksum)
                    text = CalculateChecksum(code);
                byte[] bars = GetBarsCodabar(text);
                int wide = 0;
                for (int k = 0; k < bars.Length; ++k) {
                    wide += (int)bars[k];
                }
                int narrow = bars.Length - wide;
                float fullWidth = x * (narrow + wide * n);
                fullWidth = Math.Max(fullWidth, fontX);
                float fullHeight = barHeight + fontY;
                return new Rectangle(fullWidth, fullHeight);
            }
        }
        
        /** Places the barcode in a <CODE>PdfContentByte</CODE>. The
        * barcode is always placed at coodinates (0, 0). Use the
        * translation matrix to move it elsewhere.<p>
        * The bars and text are written in the following colors:<p>
        * <P><TABLE BORDER=1>
        * <TR>
        *    <TH><P><CODE>barColor</CODE></TH>
        *    <TH><P><CODE>textColor</CODE></TH>
        *    <TH><P>Result</TH>
        *    </TR>
        * <TR>
        *    <TD><P><CODE>null</CODE></TD>
        *    <TD><P><CODE>null</CODE></TD>
        *    <TD><P>bars and text painted with current fill color</TD>
        *    </TR>
        * <TR>
        *    <TD><P><CODE>barColor</CODE></TD>
        *    <TD><P><CODE>null</CODE></TD>
        *    <TD><P>bars and text painted with <CODE>barColor</CODE></TD>
        *    </TR>
        * <TR>
        *    <TD><P><CODE>null</CODE></TD>
        *    <TD><P><CODE>textColor</CODE></TD>
        *    <TD><P>bars painted with current color<br>text painted with <CODE>textColor</CODE></TD>
        *    </TR>
        * <TR>
        *    <TD><P><CODE>barColor</CODE></TD>
        *    <TD><P><CODE>textColor</CODE></TD>
        *    <TD><P>bars painted with <CODE>barColor</CODE><br>text painted with <CODE>textColor</CODE></TD>
        *    </TR>
        * </TABLE>
        * @param cb the <CODE>PdfContentByte</CODE> where the barcode will be placed
        * @param barColor the color of the bars. It can be <CODE>null</CODE>
        * @param textColor the color of the text. It can be <CODE>null</CODE>
        * @return the dimensions the barcode occupies
        */    
        public override Rectangle PlaceBarcode(PdfContentByte cb, BaseColor barColor, BaseColor textColor) {
            String fullCode = code;
            if (generateChecksum && checksumText)
                fullCode = CalculateChecksum(code);
            if (!startStopText)
                fullCode = fullCode.Substring(1, fullCode.Length - 2);
            float fontX = 0;
            if (font != null) {
                fontX = font.GetWidthPoint(fullCode = altText != null ? altText : fullCode, size);
            }
            byte[] bars = GetBarsCodabar(generateChecksum ? CalculateChecksum(code) : code);
            int wide = 0;
            for (int k = 0; k < bars.Length; ++k) {
                wide += (int)bars[k];
            }
            int narrow = bars.Length - wide;
            float fullWidth = x * (narrow + wide * n);
            float barStartX = 0;
            float textStartX = 0;
            switch (textAlignment) {
                case Element.ALIGN_LEFT:
                    break;
                case Element.ALIGN_RIGHT:
                    if (fontX > fullWidth)
                        barStartX = fontX - fullWidth;
                    else
                        textStartX = fullWidth - fontX;
                    break;
                default:
                    if (fontX > fullWidth)
                        barStartX = (fontX - fullWidth) / 2;
                    else
                        textStartX = (fullWidth - fontX) / 2;
                    break;
            }
            float barStartY = 0;
            float textStartY = 0;
            if (font != null) {
                if (baseline <= 0)
                    textStartY = barHeight - baseline;
                else {
                    textStartY = -font.GetFontDescriptor(BaseFont.DESCENT, size);
                    barStartY = textStartY + baseline;
                }
            }
            bool print = true;
            if (barColor != null)
                cb.SetColorFill(barColor);
            for (int k = 0; k < bars.Length; ++k) {
                float w = (bars[k] == 0 ? x : x * n);
                if (print)
                    cb.Rectangle(barStartX, barStartY, w - inkSpreading, barHeight);
                print = !print;
                barStartX += w;
            }
            cb.Fill();
            if (font != null) {
                if (textColor != null)
                    cb.SetColorFill(textColor);
                cb.BeginText();
                cb.SetFontAndSize(font, size);
                cb.SetTextMatrix(textStartX, textStartY);
                cb.ShowText(fullCode);
                cb.EndText();
            }
            return BarcodeSize;
        }

#if DRAWING
        public override System.Drawing.Image CreateDrawingImage(System.Drawing.Color foreground, System.Drawing.Color background) {
            String fullCode = code;
            if (generateChecksum && checksumText)
                fullCode = CalculateChecksum(code);
            if (!startStopText)
                fullCode = fullCode.Substring(1, fullCode.Length - 2);
            byte[] bars = GetBarsCodabar(generateChecksum ? CalculateChecksum(code) : code);
            int wide = 0;
            for (int k = 0; k < bars.Length; ++k) {
                wide += (int)bars[k];
            }
            int narrow = bars.Length - wide;
            int fullWidth = narrow + wide * (int)n;
            int height = (int)barHeight;
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(fullWidth, height);
            for (int h = 0; h < height; ++h) {
                bool print = true;
                int ptr = 0;
                for (int k = 0; k < bars.Length; ++k) {
                    int w = (bars[k] == 0 ? 1 : (int)n);
                    System.Drawing.Color c = background;
                    if (print)
                        c = foreground;
                    print = !print;
                    for (int j = 0; j < w; ++j)
                        bmp.SetPixel(ptr++, h, c);
                }
            }
            return bmp;
        }
#endif// DRAWING
    }
}
