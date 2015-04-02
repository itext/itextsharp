using System;
using System.Collections.Generic;
using System.Text;

namespace iTextSharp.text.pdf.parser {

    /**
     * Contains information relating to construction the current path.
     *
     * @since 5.5.6
     */
    public class PathConstructionRenderInfo {

        /**
         * See {@link com.itextpdf.text.pdf.parser.Path#moveTo(float, float)}
         */
        public const int MOVETO = 1;

        /**
         * See {@link com.itextpdf.text.pdf.parser.Path#lineTo(float, float)}
         */
        public const int LINETO = 2;

        /**
         * See {@link com.itextpdf.text.pdf.parser.Path#curveTo(float, float, float, float, float, float)}
         */
        public const int CURVE_123 = 3;

        /**
         * See {@link com.itextpdf.text.pdf.parser.Path#curveTo(float, float, float, float)}
         */
        public const int CURVE_23 = 4;

        /**
         * See {@link com.itextpdf.text.pdf.parser.Path#curveFromTo(float, float, float, float)}
         */
        public const int CURVE_13 = 5;

        /**
         * See {@link com.itextpdf.text.pdf.parser.Path#closeSubpath()}
         */
        public const int CLOSE = 6;

        /**
         * See {@link com.itextpdf.text.pdf.parser.Path#rectangle(float, float, float, float)}
         */
        public const int RECT = 7;

        private int operation;
        private IList<float> segmentData;
        private Matrix ctm;

        /**
         * @param operation   Indicates which path-construction operation should be performed.
         * @param segmentData Contains data of a new segment being added to the current path.
         *                    E.g. x, y, w, h for rectangle; x, y for line etc.
         * @param ctm         Current transformation matrix.
         */
        public PathConstructionRenderInfo(int operation, IList<float> segmentData, Matrix ctm) {
            this.operation = operation;
            this.segmentData = segmentData;
            this.ctm = ctm;
        }

        /**
         * See {@link #PathConstructionRenderInfo(int, java.util.List, Matrix)}
         */
        public PathConstructionRenderInfo(int operation, Matrix ctm) : this(operation, null, ctm) {
        }

        /**
         * @return construction operation should be performed on the current path.
         */
        public virtual int Operation {
            get { return operation; }
        }

        /**
         * @return {@link java.util.List} containing data of a new segment (E.g. x, y, w, h for rectangle;
         *         x, y for line etc.) if the specified operation relates to adding the segment to the
         *         current path, <CODE>null</CODE> otherwise.
         */
        public virtual IList<float> SegmentData {
            get { return segmentData; }
        }

        /**
         * @return Current transformation matrix.
         */
        public virtual Matrix Ctm {
            get { return ctm; }
        }
    }
}
