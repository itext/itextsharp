using System;
using System.Collections.Generic;

using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
/*
 * $Id: $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: VVB, Bruno Lowagie, et al.
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
namespace iTextSharp.tool.xml.svg.graphic {

    public class CssSvgAppliers
    {
        private static CssSvgAppliers myself = new CssSvgAppliers();

        /**
         * @return singleton instance
         */
        public static CssSvgAppliers GetInstance()
        {
            return myself;
        }

        public void ApplyForText(PdfContentByte cb, IDictionary<String, String> css, Chunk chunk)
        {
            SetStrokeAndFillColor(cb, css);
            SetStrokeAndFill(cb, css);

            try
            {
                Font font = new Font(Font.FontFamily.COURIER, 6, Font.NORMAL, BaseColor.BLACK);

                Font font2 = chunk.Font;

                BaseFont bf2 = font2.BaseFont;
                //BaseFont bf = ;

                if (bf2 == null)
                {
                    cb.SetFontAndSize(font.GetCalculatedBaseFont(false), font2.Size);
                }
                else
                {
                    cb.SetFontAndSize(bf2, font2.Size);
                }
            } catch (Exception exp) { }

        }


        void SetStrokeAndFill(PdfContentByte cb, IDictionary<String, String> css)
        {

            //default is true for both
            String fillValue;
            String strokeValue;

            bool fill = (!css.TryGetValue("fill", out fillValue) || fillValue == null || !fillValue.Equals("none"));
            bool stroke = (css.TryGetValue("stroke", out strokeValue) && strokeValue != null && !strokeValue.Equals("none"));

            if (fill && stroke) {
                cb.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE);
            }
            else if (fill) {
                cb.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL);
            }
            else if (stroke) {
                cb.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_STROKE);
            } else {
                cb.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_INVISIBLE);
            }
        }

        public void Apply(PdfContentByte cb, IDictionary<String, String> css)
        {
            SetStrokeAndFillColor(cb, css);

            //line width
            // what is the value in svg, in pdf it is units
            SetLineWidth(cb, css);
            SetLineCap(cb, css);
            SetLineJoin(cb, css);
            SetLineDash(cb, css);
        }

        void SetStrokeAndFillColor(PdfContentByte cb, IDictionary<String, String> css)
        {
            BaseColor fillColor = BaseColor.BLACK;
            String colorValue;
            if (css.TryGetValue(SVGAttributes.FILL, out colorValue)) {
                fillColor = GetColor(colorValue);
                if (fillColor == null) {
                    fillColor = BaseColor.BLACK;
                }
            }
            cb.SetColorFill(fillColor);

            BaseColor strokeColor = fillColor;
            if (css.TryGetValue(SVGAttributes.STROKE, out colorValue)) {
                strokeColor = GetColor(colorValue);
                if (strokeColor == null) {
                    strokeColor = fillColor;
                }
            }
            cb.SetColorStroke(strokeColor);
        }


        private BaseColor GetColor(String name)
        {
            if (name == null || name.Equals("none")) return null;
            BaseColor color = HtmlUtilities.DecodeColor(name);
            if (color == null)
            {
                color = SVGAttributes.CleanColorString(name);
            }
            return color;
        }

        void SetLineWidth(PdfContentByte cb, IDictionary<String, String> css)
        {
            String width;
            if (css.TryGetValue(SVGAttributes.STROKE_WIDTH, out width))
            {
                try
                {
                    cb.SetLineWidth(float.Parse(width));
                } catch (Exception exp) {
                    //DO nothing
                }
            }
        }

        void SetLineDash(PdfContentByte cb, IDictionary<String, String> css)
        {
            String lineDash;
            if (!css.TryGetValue(SVGAttributes.STROKE_DASHARRAY, out lineDash) || lineDash == null || lineDash.Equals("none"))
            {
                return;
            }

            IList<String> list = tags.TagUtils.SplitValueList(lineDash);
            if (list == null) return;
            float[] phase = new float[list.Count * 2];
            //the lineDash was validated before adding to CSS, so this is a valid pattern

            bool allZero = true;
            int i = 0;
            foreach (String str in list)
            {
                try
                {
                    phase[i] = int.Parse(str);
                    phase[i + list.Count] = phase[i];
                    if (phase[i] != 0)
                    {
                        allZero = false;
                    }
                    i++;
                } catch (Exception exp) {
                }
            }
            if (!allZero)
            {
                cb.SetLineDash(phase, 0);
            }
        }

        void SetLineCap(PdfContentByte cb, IDictionary<String, String> css)
        {
            String lineCap;
            if (css.TryGetValue(SVGAttributes.LINE_CAP, out lineCap) && lineCap != null)
            {
                if (lineCap.Equals("round"))
                {
                    cb.SetLineCap(PdfContentByte.LINE_CAP_ROUND);
                } else if (lineCap.Equals("square")) {
                    cb.SetLineCap(PdfContentByte.LINE_CAP_PROJECTING_SQUARE);
                }
                else {
                    cb.SetLineCap(PdfContentByte.LINE_CAP_BUTT);
                }
            } else {
                cb.SetLineCap(PdfContentByte.LINE_CAP_BUTT);
            }
        }

        void SetLineJoin(PdfContentByte cb, IDictionary<String, String> css)
        {
            String lineJoin;
            if (css.TryGetValue(SVGAttributes.LINE_JOIN, out lineJoin) && lineJoin != null)
            {
                if (lineJoin.Equals("round"))
                {
                    cb.SetLineJoin(PdfContentByte.LINE_JOIN_ROUND);
                } else if (lineJoin.Equals("bevel")) {
                    cb.SetLineJoin(PdfContentByte.LINE_JOIN_BEVEL);
                } else {
                    cb.SetLineJoin(PdfContentByte.LINE_JOIN_MITER);
                }
            } else {
                cb.SetLineJoin(PdfContentByte.LINE_JOIN_MITER);
            }
        }

        public void Close(PdfContentByte cb, IDictionary<String, String> css)
        {
            //default is true for both
            String fillValue;
            String strokeValue;

            bool fill = (!css.TryGetValue("fill", out fillValue) || fillValue == null || !fillValue.Equals("none"));
            bool stroke = (!css.TryGetValue("stroke", out strokeValue) || strokeValue == null || !strokeValue.Equals("none"));

            if (fill && stroke) {
                cb.FillStroke();
            } else if (fill) {
                cb.Fill();
            } else if (stroke) {
                cb.Stroke();
            }
        }
    }
}
