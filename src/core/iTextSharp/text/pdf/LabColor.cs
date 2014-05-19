namespace iTextSharp.text.pdf {
    public class LabColor : ExtendedColor {
        PdfLabColor labColorSpace;
        private float l;
        private float a;
        private float b;

        public LabColor(PdfLabColor labColorSpace, float l, float a, float b)
            : base(ExtendedColor.TYPE_LAB) {
            this.labColorSpace = labColorSpace;
            this.l = l;
            this.a = a;
            this.b = b;
            BaseColor altRgbColor = labColorSpace.Lab2Rgb(l, a, b);
            SetValue(altRgbColor.R, altRgbColor.G, altRgbColor.B, 255);
        }

        public virtual PdfLabColor LabColorSpace {
            get { return labColorSpace; }
        }

        public virtual float L {
            get { return l; }
        }

        public virtual float A {
            get { return a; }
        }

        public virtual float B {
            get { return b; }
        }

        public virtual BaseColor ToRgb() {
            return labColorSpace.Lab2Rgb(l, a, b);
        }

        internal virtual CMYKColor ToCmyk() {
            return labColorSpace.Lab2Cmyk(l, a, b);
        }
    }
}
