namespace iTextSharp.text.pdf.parser {

    /**
     * Represents the line dash pattern. The line dash pattern shall control the pattern
     * of dashes and gaps used to stroke paths. It shall be specified by a dash array and
     * a dash phase.
     *
     * @since 5.5.6
     */
    public class LineDashPattern {

        private PdfArray dashArray;
        private float dashPhase;

        private int currentIndex;
        private int elemOrdinalNumber = 1;
        private DashArrayElem currentElem;

        /**
         * Creates new {@link LineDashPattern} object.
         * @param dashArray The dash array. See {@link #getDashArray()}
         * @param dashPhase The dash phase. See {@link #getDashPhase()}
         */
        public LineDashPattern(PdfArray dashArray, float dashPhase) {
            this.dashArray = new PdfArray(dashArray);
            this.dashPhase = dashPhase;
            InitFirst(dashPhase);
        }

        /**
         * Getter and setter for the dash array.
         *
         * The dash array’s elements is number that specify the lengths of
         * alternating dashes and gaps; the numbers are nonnegative. The
         * elements are expressed in user space units.
         *
         * @return The dash array.
         */
        public PdfArray DashArray {
            get { return dashArray; }
            set { dashArray = value; }
        }

        /**
         * Getter and setter for the dash phase.
         *
         * The dash phase shall specify the distance into the dash pattern at which
         * to start the dash. The elements are expressed in user space units.
         *
         * @return The dash phase.
         */
        public float DashPhase {
            get { return dashPhase; }
            set { dashPhase = value; }
        }

        /**
         * Calculates and returns the next element which is either gap or dash.
         * @return The next dash array's element.
         */
        public DashArrayElem Next() {
            DashArrayElem ret = currentElem;

            if (dashArray.Size > 0) {
                currentIndex = (currentIndex + 1) % DashArray.Size;
                currentElem = new DashArrayElem(dashArray.GetAsNumber(currentIndex).FloatValue,
                    IsEven(++elemOrdinalNumber));
            }

            return ret;
        }

        /**
         * Resets the dash array so that the {@link #next()} method will start
         * from the beginning of the dash array.
         */
        public void Reset() {
            currentIndex = 0;
            elemOrdinalNumber = 1;
            InitFirst(dashPhase);
        }

        private void InitFirst(float phase) {
            if (dashArray.Size > 0) {
                while (phase > 0) {
                    phase -= dashArray.GetAsNumber(currentIndex).FloatValue;
                    currentIndex = (currentIndex + 1) % DashArray.Size;
                    elemOrdinalNumber++;
                }

                if (phase < 0) {
                    --elemOrdinalNumber;
                    --currentIndex;
                    currentElem = new DashArrayElem(-phase, IsEven(elemOrdinalNumber));
                } else {
                    currentElem = new DashArrayElem(dashArray.GetAsNumber(currentIndex).FloatValue, 
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
