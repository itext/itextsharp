using System;
using iTextSharp.text.error_messages;

namespace iTextSharp.text.pdf {
    public class DeviceNColor : ExtendedColor {

        PdfDeviceNColor pdfDeviceNColor;
        float[] tints;

        public DeviceNColor(PdfDeviceNColor pdfDeviceNColor, float[] tints) : base(TYPE_DEVICEN) {
            if (pdfDeviceNColor.SpotColors.Length != tints.Length)
                throw new Exception(MessageLocalization.GetComposedMessage(
                        "devicen.color.shall.have.the.same.number.of.colorants.as.the.destination.DeviceN.color.space"));
            this.pdfDeviceNColor = pdfDeviceNColor;
            this.tints = tints;
        }

        public virtual PdfDeviceNColor PdfDeviceNColor {
            get { return pdfDeviceNColor; }
        }

        public virtual float[] Tints {
            get { return tints; }
        }

        public override bool Equals(Object obj) {
            if (obj is DeviceNColor && ((DeviceNColor) obj).tints.Length == tints.Length) {
                int i = 0;
                foreach (float tint in this.tints) {
                    if (tint != ((DeviceNColor) obj).tints[i])
                        return false;
                    i++;
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode() {
            int hashCode = pdfDeviceNColor.GetHashCode();
            foreach (float tint in this.tints) {
                hashCode ^= tint.GetHashCode();
            }
            return hashCode;
        }
    }
}
