using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.text.pdf;

namespace iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup {

    public class LineDashPattern {

        private PdfArray dashArray;
        private float dashPhase;

        private int currentIndex;
        private int elemOrdinalNumber = 1;
        private DashArrayElem currentElem;

        public LineDashPattern(PdfArray dashArray, float dashPhase) {
            this.dashArray = new PdfArray(dashArray);
            this.dashPhase = dashPhase;
            InitFirst(dashPhase);
        }

        public PdfArray DashArray {
            get { return dashArray; }
            set { dashArray = value; }
        }

        public float DashPhase {
            get { return dashPhase; }
            set { dashPhase = value; }
        }

        public DashArrayElem Next() {
            DashArrayElem ret = currentElem;

            if (dashArray.Size > 0) {
                currentIndex = (currentIndex + 1) % DashArray.Size;
                currentElem = new DashArrayElem(((PdfNumber) dashArray[currentIndex]).FloatValue,
                    IsEven(++elemOrdinalNumber));
            }

            return ret;
        }

        public void Reset() {
            currentIndex = 0;
            elemOrdinalNumber = 1;
            InitFirst(dashPhase);
        }

        private void InitFirst(float phase) {
            if (dashArray.Size > 0) {
                while (phase > 0) {
                    phase -= ((PdfNumber) dashArray[currentIndex]).FloatValue;
                    currentIndex = (currentIndex + 1) % DashArray.Size;
                    elemOrdinalNumber++;
                }

                if (phase < 0) {
                    --elemOrdinalNumber;
                    --currentIndex;
                    currentElem = new DashArrayElem(-phase, IsEven(elemOrdinalNumber));
                } else {
                    currentElem = new DashArrayElem(((PdfNumber) dashArray[currentIndex]).FloatValue, 
                        IsEven(elemOrdinalNumber));
                }
            }
        }

        private bool IsEven(int num) {
            return (num % 2) == 0;
        }

        public class DashArrayElem {

            private float val;
            private bool isGap;

            public DashArrayElem(float val, bool isGap) {
                this.val = val;
                this.isGap = isGap;
            }

            public float Value
            {
                get { return val; }
                set { val = value; }
            }

            public bool IsGap
            {
                get { return isGap; }
                set { isGap = value; }
            }
        }
    }
}
