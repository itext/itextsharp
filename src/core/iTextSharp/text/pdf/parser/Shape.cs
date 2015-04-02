using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.awt.geom;

namespace iTextSharp.text.pdf.parser {

    /**
     * Represents segment from a PDF path.
     * 
     * @since 5.5.6
     */
    public interface IShape {

        /**
         * Treat base points as the points which are enough to construct a shape.
         * E.g. for a bezier curve they are control points, for a line segment - the start and the end points
         * of the segment.
         *
         * @return Ordered list consisting of shape's base points.
         */
        IList<Point2D> GetBasePoints();
    }
}
