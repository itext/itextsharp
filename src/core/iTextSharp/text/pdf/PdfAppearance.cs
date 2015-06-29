using System;
using System.Collections.Generic;

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

    /**
     * Implements the appearance stream to be used with form fields..
     */

    public class PdfAppearance : PdfTemplate {

        public static Dictionary<String, PdfName> stdFieldFontNames = new Dictionary<string,PdfName>();

        static PdfAppearance() {
            stdFieldFontNames["Courier-BoldOblique"] = new PdfName("CoBO");
            stdFieldFontNames["Courier-Bold"] = new PdfName("CoBo");
            stdFieldFontNames["Courier-Oblique"] = new PdfName("CoOb");
            stdFieldFontNames["Courier"] = new PdfName("Cour");
            stdFieldFontNames["Helvetica-BoldOblique"] = new PdfName("HeBO");
            stdFieldFontNames["Helvetica-Bold"] = new PdfName("HeBo");
            stdFieldFontNames["Helvetica-Oblique"] = new PdfName("HeOb");
            stdFieldFontNames["Helvetica"] = PdfName.HELV;
            stdFieldFontNames["Symbol"] = new PdfName("Symb");
            stdFieldFontNames["Times-BoldItalic"] = new PdfName("TiBI");
            stdFieldFontNames["Times-Bold"] = new PdfName("TiBo");
            stdFieldFontNames["Times-Italic"] = new PdfName("TiIt");
            stdFieldFontNames["Times-Roman"] = new PdfName("TiRo");
            stdFieldFontNames["ZapfDingbats"] = PdfName.ZADB;
            stdFieldFontNames["HYSMyeongJo-Medium"] = new PdfName("HySm");
            stdFieldFontNames["HYGoThic-Medium"] = new PdfName("HyGo");
            stdFieldFontNames["HeiseiKakuGo-W5"] = new PdfName("KaGo");
            stdFieldFontNames["HeiseiMin-W3"] = new PdfName("KaMi");
            stdFieldFontNames["MHei-Medium"] = new PdfName("MHei");
            stdFieldFontNames["MSung-Light"] = new PdfName("MSun");
            stdFieldFontNames["STSong-Light"] = new PdfName("STSo");
            stdFieldFontNames["MSungStd-Light"] = new PdfName("MSun");
            stdFieldFontNames["STSongStd-Light"] = new PdfName("STSo");
            stdFieldFontNames["HYSMyeongJoStd-Medium"] = new PdfName("HySm");
            stdFieldFontNames["KozMinPro-Regular"] = new PdfName("KaMi");
        }
        
        /**
        *Creates a <CODE>PdfAppearance</CODE>.
        */
    
        internal PdfAppearance() : base() {
            separator = ' ';
        }
    
        internal PdfAppearance(PdfIndirectReference iref) {
            thisReference = iref;
        }
        /**
         * Creates new PdfTemplate
         *
         * @param wr the <CODE>PdfWriter</CODE>
         */
    
        internal PdfAppearance(PdfWriter wr) : base(wr) {
            separator = ' ';
        }
    
        /**
         * Creates a new appearance to be used with form fields.
         *
         * @param width the bounding box width
         * @param height the bounding box height
         * @return the appearance created
         */
        public static PdfAppearance CreateAppearance(PdfWriter writer, float width, float height) {
            return CreateAppearance(writer, width, height, null);
        }
        
        internal static PdfAppearance CreateAppearance(PdfWriter writer, float width, float height, PdfName forcedName) {
            PdfAppearance template = new PdfAppearance(writer);
            template.Width = width;
            template.Height = height;
            writer.AddDirectTemplateSimple(template, forcedName);
            return template;
        }

        /**
        * Set the font and the size for the subsequent text writing.
        *
        * @param bf the font
        * @param size the font size in points
        */
        public override void SetFontAndSize(BaseFont bf, float size) {
            CheckWriter();
            state.size = size;
            if (bf.FontType == BaseFont.FONT_TYPE_DOCUMENT) {
                state.fontDetails = new FontDetails(null, ((DocumentFont)bf).IndirectReference, bf);
            }
            else
                state.fontDetails = writer.AddSimple(bf);
            PdfName psn;
            stdFieldFontNames.TryGetValue(bf.PostscriptFontName, out psn);
            if (psn == null) {
                if (bf.Subset && bf.FontType == BaseFont.FONT_TYPE_TTUNI)
                    psn = state.fontDetails.FontName;
                else {
                    psn = new PdfName(bf.PostscriptFontName);
                    state.fontDetails.Subset = false;
                }
            }
            PageResources prs = PageResources;
            prs.AddFont(psn, state.fontDetails.IndirectReference);
            content.Append(psn.GetBytes()).Append(' ').Append(size).Append(" Tf").Append_i(separator);
        }

        public override PdfContentByte Duplicate {
            get {
                PdfAppearance tpl = new PdfAppearance();
                tpl.writer = writer;
                tpl.pdf = pdf;
                tpl.thisReference = thisReference;
                tpl.pageResources = pageResources;
                tpl.bBox = new Rectangle(bBox);
                tpl.group = group;
                tpl.layer = layer;
                if (matrix != null) {
                    tpl.matrix = new PdfArray(matrix);
                }
                tpl.separator = separator;
                return tpl;
            }
        }
    }
}
