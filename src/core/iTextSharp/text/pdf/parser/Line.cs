using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.awt.geom;

namespace iTextSharp.text.pdf.parser {

    /**
     * Represents a line.
     *
     * @since 5.5.6
     */
    public class Line : IShape {

        private readonly Point2D p1;
        private readonly Point2D p2;

        /**
         * Constructs a new zero-length line starting at zero.
         */
        public Line() : this(0, 0, 0, 0) {
        }

        /**
         * Constructs a new line based on the given coordinates.
         */
        public Line(float x1, float y1, float x2, float y2) {
            p1 = new Point2D.Float(x1, y1);
            p2 = new Point2D.Float(x2, y2);
        }

        /**
         * Constructs a new line based on the given coordinates.
         */
        public Line(Point2D p1, Point2D p2) : this((float) p1.GetX(), (float) p1.GetY(), 
                                                   (float) p2.GetX(), (float) p2.GetY()) {
        }

        public virtual IList<Point2D> GetBasePoints() {
            IList<Point2D> basePoints = new List<Point2D>(2);
            basePoints.Add(p1);
            basePoints.Add(p2);

            return basePoints;
        }
    }
}
