using System;
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

    /**
     * A <CODE>PdfSpotColor</CODE> defines a ColorSpace
     *
     * @see     PdfDictionary
     */
    public class PdfSpotColor : ICachedColorSpace, IPdfSpecialColorSpace {
    
        /*  The color name */
        public PdfName name;
    
        /* The alternative color space */
        public BaseColor altcs;
        // constructors

        public ColorDetails altColorDetails;
    
        /**
         * Constructs a new <CODE>PdfSpotColor</CODE>.
         *
         * @param       name        a string value
         * @param       tint        a tint value between 0 and 1
         * @param       altcs       a altnative colorspace value
         */
        public PdfSpotColor(string name, BaseColor altcs) {
            this.name = new PdfName(name);
            this.altcs = altcs;
        }

        public virtual ColorDetails[] GetColorantDetails(PdfWriter writer) {
            if (altColorDetails == null && this.altcs is ExtendedColor &&
                ((ExtendedColor) this.altcs).Type == ExtendedColor.TYPE_LAB) {
                altColorDetails = writer.AddSimple(((LabColor) altcs).LabColorSpace);
            }
            return new ColorDetails[] {altColorDetails};
        }
    
        virtual public BaseColor AlternativeCS {
            get {
                return altcs;
            }
        }

        public virtual PdfName Name {
            get { return name; }
        }

        [Obsolete]
        protected internal virtual PdfObject GetSpotObject(PdfWriter writer) {
            return GetPdfObject(writer);
        }

        public virtual PdfObject GetPdfObject(PdfWriter writer) {
            PdfArray array = new PdfArray(PdfName.SEPARATION);
            array.Add(name);
            PdfFunction func = null;
            if (altcs is ExtendedColor) {
                int type = ((ExtendedColor) altcs).Type;
                switch (type) {
                    case ExtendedColor.TYPE_GRAY:
                        array.Add(PdfName.DEVICEGRAY);
                        func = PdfFunction.Type2(writer, new float[] {0, 1}, null, new float[] {1},
                            new float[] {((GrayColor) altcs).Gray}, 1);
                        break;
                    case ExtendedColor.TYPE_CMYK:
                        array.Add(PdfName.DEVICECMYK);
                        CMYKColor cmyk = (CMYKColor) altcs;
                        func = PdfFunction.Type2(writer, new float[] {0, 1}, null, new float[] {0, 0, 0, 0},
                            new float[] {cmyk.Cyan, cmyk.Magenta, cmyk.Yellow, cmyk.Black}, 1);
                        break;
                    case ExtendedColor.TYPE_LAB:
                        LabColor lab = (LabColor) altcs;
                        if (altColorDetails != null)
                            array.Add(altColorDetails.IndirectReference);
                        else
                            array.Add(lab.LabColorSpace.GetPdfObject(writer));
                        func = PdfFunction.Type2(writer, new float[] {0, 1}, null, new float[] {100f, 0f, 0f},
                            new float[] {lab.L, lab.A, lab.B}, 1);
                        break;
                    default:
                        throw new Exception(
                            MessageLocalization.GetComposedMessage(
                                "only.rgb.gray.and.cmyk.are.supported.as.alternative.color.spaces"));
                }
            } else {
                array.Add(PdfName.DEVICERGB);
                func = PdfFunction.Type2(writer, new float[] {0, 1}, null, new float[] {1, 1, 1},
                    new float[] {(float) altcs.R/255, (float) altcs.G/255, (float) altcs.B/255}, 1);
            }
            array.Add(func.Reference);
            return array;
        }


        public override bool Equals(Object o) {
            if (this == o) return true;
            if (!(o is PdfSpotColor)) return false;

            PdfSpotColor spotColor = (PdfSpotColor) o;

            if (!altcs.Equals(spotColor.altcs)) return false;
            if (!name.Equals(spotColor.name)) return false;

            return true;
        }

        public override int GetHashCode() {
            int result = name.GetHashCode();
            result = 31*result + altcs.GetHashCode();
            return result;
        }
    }
}
