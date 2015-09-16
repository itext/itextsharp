using System;
using iTextSharp.text;
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

    /** Implements the shading dictionary (or stream).
     *
     * @author Paulo Soares
     */
    public class PdfShading {

        protected PdfDictionary shading;
    
        protected PdfWriter writer;
    
        protected int shadingType;
    
        protected ColorDetails colorDetails;
    
        protected PdfName shadingName;
    
        protected PdfIndirectReference shadingReference;
    
        /** Holds value of property bBox. */
        protected float[] bBox;
    
        /** Holds value of property antiAlias. */
        protected bool antiAlias = false;

        private BaseColor cspace;
    
        /** Creates new PdfShading */
        protected PdfShading(PdfWriter writer) {
            this.writer = writer;
        }
    
        virtual protected void SetColorSpace(BaseColor color) {
            cspace = color;
            int type = ExtendedColor.GetType(color);
            PdfObject colorSpace = null;
            switch (type) {
                case ExtendedColor.TYPE_GRAY: {
                    colorSpace = PdfName.DEVICEGRAY;
                    break;
                }
                case ExtendedColor.TYPE_CMYK: {
                    colorSpace = PdfName.DEVICECMYK;
                    break;
                }
                case ExtendedColor.TYPE_SEPARATION: {
                    SpotColor spot = (SpotColor)color;
                    colorDetails = writer.AddSimple(spot.PdfSpotColor);
                    colorSpace = colorDetails.IndirectReference;
                    break;
                }
                case ExtendedColor.TYPE_DEVICEN: {
                    DeviceNColor deviceNColor = (DeviceNColor) color;
                    colorDetails = writer.AddSimple(deviceNColor.PdfDeviceNColor);
                    colorSpace = colorDetails.IndirectReference;
                    break;
                }
                case ExtendedColor.TYPE_PATTERN:
                case ExtendedColor.TYPE_SHADING: {
                    ThrowColorSpaceError();
                    break;
                }
                default:
                    colorSpace = PdfName.DEVICERGB;
                    break;
            }
            shading.Put(PdfName.COLORSPACE, colorSpace);
        }
    
        virtual public BaseColor ColorSpace {
            get {
                return cspace;
            }
        }

        public static void ThrowColorSpaceError() {
            throw new ArgumentException(MessageLocalization.GetComposedMessage("a.tiling.or.shading.pattern.cannot.be.used.as.a.color.space.in.a.shading.pattern"));
        }
    
        public static void CheckCompatibleColors(BaseColor c1, BaseColor c2) {
            int type1 = ExtendedColor.GetType(c1);
            int type2 = ExtendedColor.GetType(c2);
            if (type1 != type2)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("both.colors.must.be.of.the.same.type"));
            if (type1 == ExtendedColor.TYPE_SEPARATION && ((SpotColor)c1).PdfSpotColor != ((SpotColor)c2).PdfSpotColor)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.spot.color.must.be.the.same.only.the.tint.can.vary"));
            if (type1 == ExtendedColor.TYPE_PATTERN || type1 == ExtendedColor.TYPE_SHADING)
                ThrowColorSpaceError();
        }
    
        public static float[] GetColorArray(BaseColor color) {
            int type = ExtendedColor.GetType(color);
            switch (type) {
                case ExtendedColor.TYPE_GRAY: {
                    return new float[]{((GrayColor)color).Gray};
                }
                case ExtendedColor.TYPE_CMYK: {
                    CMYKColor cmyk = (CMYKColor)color;
                    return new float[]{cmyk.Cyan, cmyk.Magenta, cmyk.Yellow, cmyk.Black};
                }
                case ExtendedColor.TYPE_SEPARATION: {
                    return new float[]{((SpotColor)color).Tint};
                }
                case ExtendedColor.TYPE_DEVICEN: {
                    return ((DeviceNColor) color).Tints;
                }
                case ExtendedColor.TYPE_RGB: {
                    return new float[]{color.R / 255f, color.G / 255f, color.B / 255f};
                }
            }
            ThrowColorSpaceError();
            return null;
        }

        public static PdfShading Type1(PdfWriter writer, BaseColor colorSpace, float[] domain, float[] tMatrix, PdfFunction function) {
            PdfShading sp = new PdfShading(writer);
            sp.shading = new PdfDictionary();
            sp.shadingType = 1;
            sp.shading.Put(PdfName.SHADINGTYPE, new PdfNumber(sp.shadingType));
            sp.SetColorSpace(colorSpace);
            if (domain != null)
                sp.shading.Put(PdfName.DOMAIN, new PdfArray(domain));
            if (tMatrix != null)
                sp.shading.Put(PdfName.MATRIX, new PdfArray(tMatrix));
            sp.shading.Put(PdfName.FUNCTION, function.Reference);
            return sp;
        }
    
        public static PdfShading Type2(PdfWriter writer, BaseColor colorSpace, float[] coords, float[] domain, PdfFunction function, bool[] extend) {
            PdfShading sp = new PdfShading(writer);
            sp.shading = new PdfDictionary();
            sp.shadingType = 2;
            sp.shading.Put(PdfName.SHADINGTYPE, new PdfNumber(sp.shadingType));
            sp.SetColorSpace(colorSpace);
            sp.shading.Put(PdfName.COORDS, new PdfArray(coords));
            if (domain != null)
                sp.shading.Put(PdfName.DOMAIN, new PdfArray(domain));
            sp.shading.Put(PdfName.FUNCTION, function.Reference);
            if (extend != null && (extend[0] || extend[1])) {
                PdfArray array = new PdfArray(extend[0] ? PdfBoolean.PDFTRUE : PdfBoolean.PDFFALSE);
                array.Add(extend[1] ? PdfBoolean.PDFTRUE : PdfBoolean.PDFFALSE);
                sp.shading.Put(PdfName.EXTEND, array);
            }
            return sp;
        }

        public static PdfShading Type3(PdfWriter writer, BaseColor colorSpace, float[] coords, float[] domain, PdfFunction function, bool[] extend) {
            PdfShading sp = Type2(writer, colorSpace, coords, domain, function, extend);
            sp.shadingType = 3;
            sp.shading.Put(PdfName.SHADINGTYPE, new PdfNumber(sp.shadingType));
            return sp;
        }
    
        public static PdfShading SimpleAxial(PdfWriter writer, float x0, float y0, float x1, float y1, BaseColor startColor, BaseColor endColor, bool extendStart, bool extendEnd) {
            CheckCompatibleColors(startColor, endColor);
            PdfFunction function = PdfFunction.Type2(writer, new float[]{0, 1}, null, GetColorArray(startColor),
                GetColorArray(endColor), 1);
            return Type2(writer, startColor, new float[]{x0, y0, x1, y1}, null, function, new bool[]{extendStart, extendEnd});
        }
    
        public static PdfShading SimpleAxial(PdfWriter writer, float x0, float y0, float x1, float y1, BaseColor startColor, BaseColor endColor) {
            return SimpleAxial(writer, x0, y0, x1, y1, startColor, endColor, true, true);
        }
    
        public static PdfShading SimpleRadial(PdfWriter writer, float x0, float y0, float r0, float x1, float y1, float r1, BaseColor startColor, BaseColor endColor, bool extendStart, bool extendEnd) {
            CheckCompatibleColors(startColor, endColor);
            PdfFunction function = PdfFunction.Type2(writer, new float[]{0, 1}, null, GetColorArray(startColor),
                GetColorArray(endColor), 1);
            return Type3(writer, startColor, new float[]{x0, y0, r0, x1, y1, r1}, null, function, new bool[]{extendStart, extendEnd});
        }

        public static PdfShading SimpleRadial(PdfWriter writer, float x0, float y0, float r0, float x1, float y1, float r1, BaseColor startColor, BaseColor endColor) {
            return SimpleRadial(writer, x0, y0, r0, x1, y1, r1, startColor, endColor, true, true);
        }

        internal PdfName ShadingName {
            get {
                return shadingName;
            }
        }
    
        internal PdfIndirectReference ShadingReference {
            get {
                if (shadingReference == null)
                    shadingReference = writer.PdfIndirectReference;
                return shadingReference;
            }
        }
    
        internal int Name {
            set {
                shadingName = new PdfName("Sh" + value);
            }
        }
    
        virtual public void AddToBody() {
            if (bBox != null)
                shading.Put(PdfName.BBOX, new PdfArray(bBox));
            if (antiAlias)
                shading.Put(PdfName.ANTIALIAS, PdfBoolean.PDFTRUE);
            writer.AddToBody(shading, this.ShadingReference);
        }
    
        internal PdfWriter Writer {
            get {
                return writer;
            }
        }
    
        internal ColorDetails ColorDetails {
            get {
                return colorDetails;
            }
        }
    
        virtual public float[] BBox {
            get {
                return bBox;
            }
            set {
                if (value.Length != 4)
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("bbox.must.be.a.4.element.array"));
                this.bBox = value;
            }
        }
    
        virtual public bool AntiAlias {
            set {
                this.antiAlias = value;
            }
            get {
                return antiAlias;
            }
        }
    
    }
}
