using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.awt.geom;

namespace iTextSharp.text.pdf.parser {

    /**
     * Represents a Bezier curve.
     *
     * @since 5.5.6
     */
    public class BezierCurve : IShape {

        private readonly IList<Point2D> controlPoints;

        public BezierCurve(IList<Point2D> controlPoints) {
            this.controlPoints = controlPoints;
        }

        public virtual IList<Point2D> GetBasePoints() {
            return controlPoints;
        }
    }
}
