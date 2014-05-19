using System;
using System.util;
using iTextSharp.text.error_messages;

namespace iTextSharp.text.pdf {
    public class PdfLabColor : ICachedColorSpace {
        float[] whitePoint = new float[] {0.9505f, 1.0f, 1.0890f};
        float[] blackPoint = null;
        float[] range = null;

        public PdfLabColor() {
        }

        public PdfLabColor(float[] whitePoint) {
            if (whitePoint == null
                || whitePoint.Length != 3
                || whitePoint[0] < 0.000001f || whitePoint[2] < 0.000001f
                || whitePoint[1] < 0.999999f || whitePoint[1] > 1.000001f)
                throw new Exception(MessageLocalization.GetComposedMessage("lab.cs.white.point"));
            this.whitePoint = whitePoint;
        }

        public PdfLabColor(float[] whitePoint, float[] blackPoint) : this(whitePoint) {
            this.blackPoint = blackPoint;
        }

        public PdfLabColor(float[] whitePoint, float[] blackPoint, float[] range) : this(whitePoint, blackPoint) {
            this.range = range;
        }

        public virtual PdfObject GetPdfObject(PdfWriter writer) {
            PdfArray array = new PdfArray(PdfName.LAB);
            PdfDictionary dictionary = new PdfDictionary();
            if (whitePoint == null
                || whitePoint.Length != 3
                || whitePoint[0] < 0.000001f || whitePoint[2] < 0.000001f
                || whitePoint[1] < 0.999999f || whitePoint[1] > 1.000001f)
                throw new Exception(MessageLocalization.GetComposedMessage("lab.cs.white.point"));
            dictionary.Put(PdfName.WHITEPOINT, new PdfArray(whitePoint));
            if (blackPoint != null) {
                if (blackPoint.Length != 3
                    || blackPoint[0] < -0.000001f || blackPoint[1] < -0.000001f || blackPoint[2] < -0.000001f)
                    throw new Exception(MessageLocalization.GetComposedMessage("lab.cs.black.point"));
                dictionary.Put(PdfName.BLACKPOINT, new PdfArray(blackPoint));
            }
            if (range != null) {
                if (range.Length != 4 || range[0] > range[1] || range[2] > range[3])
                    throw new Exception(MessageLocalization.GetComposedMessage("lab.cs.range"));
                dictionary.Put(PdfName.RANGE, new PdfArray(range));
            }
            array.Add(dictionary);
            return array;
        }

        public virtual BaseColor Lab2Rgb(float l, float a, float b) {
            double[] clinear = Lab2RgbLinear(l, a, b);
            return new BaseColor((float) clinear[0], (float) clinear[1], (float) clinear[2]);
        }

        internal virtual CMYKColor Lab2Cmyk(float l, float a, float b) {
            double[] clinear = Lab2RgbLinear(l, a, b);

            double r = clinear[0];
            double g = clinear[1];
            double bee = clinear[2];
            double computedC = 0, computedM = 0, computedY = 0, computedK = 0;

            // BLACK
            if (r == 0 && g == 0 && b == 0) {
                computedK = 1;
            } else {
                computedC = 1 - r;
                computedM = 1 - g;
                computedY = 1 - bee;

                double minCMY = Math.Min(computedC,
                    Math.Min(computedM, computedY));
                computedC = (computedC - minCMY)/(1 - minCMY);
                computedM = (computedM - minCMY)/(1 - minCMY);
                computedY = (computedY - minCMY)/(1 - minCMY);
                computedK = minCMY;
            }

            return new CMYKColor((float) computedC, (float) computedM, (float) computedY, (float) computedK);
        }

        protected virtual double[] Lab2RgbLinear(float l, float a, float b) {
            if (range != null && range.Length == 4) {
                if (a < range[0])
                    a = range[0];
                if (a > range[1])
                    a = range[1];
                if (b < range[2])
                    b = range[2];
                if (b > range[3])
                    b = range[3];
            }
            double theta = 6.0/29.0;

            double fy = (l + 16)/116.0;
            double fx = fy + (a/500.0);
            double fz = fy - (b/200.0);

            double x = fx > theta ? whitePoint[0]*(fx*fx*fx) : (fx - 16.0/116.0)*3*(theta*theta)*whitePoint[0];
            double y = fy > theta ? whitePoint[1]*(fy*fy*fy) : (fy - 16.0/116.0)*3*(theta*theta)*whitePoint[1];
            double z = fz > theta ? whitePoint[2]*(fz*fz*fz) : (fz - 16.0/116.0)*3*(theta*theta)*whitePoint[2];

            double[] clinear = new double[3];
            clinear[0] = x*3.2410 - y*1.5374 - z*0.4986; // red
            clinear[1] = -x*0.9692 + y*1.8760 - z*0.0416; // green
            clinear[2] = x*0.0556 - y*0.2040 + z*1.0570; // blue

            for (int i = 0; i < 3; i++) {
                clinear[i] = (clinear[i] <= 0.0031308)
                    ? 12.92*clinear[i]
                    : (1 + 0.055)*Math.Pow(clinear[i], (1.0/2.4)) - 0.055;
                if (clinear[i] < 0)
                    clinear[i] = 0;
                else if (clinear[i] > 1f)
                    clinear[i] = 1.0;
            }

            return clinear;
        }

        public virtual LabColor Rgb2lab(BaseColor baseColor) {
            double rLinear = baseColor.R/255f;
            double gLinear = baseColor.G/255f;
            double bLinear = baseColor.B/255f;

            // convert to a sRGB form
            double r = (rLinear > 0.04045) ? Math.Pow((rLinear + 0.055)/(1 + 0.055), 2.2) : (rLinear/12.92);
            double g = (gLinear > 0.04045) ? Math.Pow((gLinear + 0.055)/(1 + 0.055), 2.2) : (gLinear/12.92);
            double b = (bLinear > 0.04045) ? Math.Pow((bLinear + 0.055)/(1 + 0.055), 2.2) : (bLinear/12.92);

            // converts
            double x = r*0.4124 + g*0.3576 + b*0.1805;
            double y = r*0.2126 + g*0.7152 + b*0.0722;
            double z = r*0.0193 + g*0.1192 + b*0.9505;

            float l = (float)Math.Round((116.0*FXyz(y/whitePoint[1]) - 16)*1000)/1000f;
            float a = (float)Math.Round((500.0*(FXyz(x/whitePoint[0]) - FXyz(y/whitePoint[1])))*1000)/1000f;
            float bee = (float)Math.Round((200.0*(FXyz(y/whitePoint[1]) - FXyz(z/whitePoint[2])))*1000)/1000f;

            return new LabColor(this, l, a, bee);
        }

        private static double FXyz(double t) {
            return ((t > 0.008856) ? Math.Pow(t, (1.0/3.0)) : (7.787*t + 16.0/116.0));
        }

        public override bool Equals(Object o) {
            if (this == o) return true;
            if (!(o is PdfLabColor)) return false;

            PdfLabColor that = (PdfLabColor) o;

            if (!Util.ArraysAreEqual(blackPoint, that.blackPoint)) return false;
            if (!Util.ArraysAreEqual(range, that.range)) return false;
            if (!Util.ArraysAreEqual(whitePoint, that.whitePoint)) return false;

            return true;
        }

        public override int GetHashCode() {
            int result = Util.GetArrayHashCode(whitePoint);
            result = 31*result + (blackPoint != null ? Util.GetArrayHashCode(blackPoint) : 0);
            result = 31*result + (range != null ? Util.GetArrayHashCode(range) : 0);
            return result;
        }
    }
}
