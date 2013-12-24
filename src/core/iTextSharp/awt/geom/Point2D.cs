using System;

namespace iTextSharp.awt.geom
{
    public abstract class Point2D
    {
        public class Float : Point2D {

            public float x;
            public float y;

            public Float() {
            }

            public Float(float x, float y) {
                this.x = x;
                this.y = y;
            }

            public override double GetX() {
                return x;
            }

            public override double GetY() {
                return y;
            }

            virtual public void SetLocation(float x, float y) {
                this.x = x;
                this.y = y;
            }

            public override void SetLocation(double x, double y) {
                this.x = (float)x;
                this.y = (float)y;
            }

            public override string ToString() {
                return "Point2D:[x=" + x + ",y=" + y + "]"; //$NON-NLS-1$ //$NON-NLS-2$ //$NON-NLS-3$
            }
        }

        public class Double : Point2D {

            public double x;
            public double y;

            public Double() {
            }

            public Double(double x, double y) {
                this.x = x;
                this.y = y;
            }

            public override double GetX() {
                return x;
            }

            public override double GetY() {
                return y;
            }

            public override void SetLocation(double x, double y) {
                this.x = x;
                this.y = y;
            }

            public override string ToString() {
                return "Point2D: [x=" + x + ",y=" + y + "]"; //$NON-NLS-1$ //$NON-NLS-2$ //$NON-NLS-3$
            }
        }

        protected Point2D() {
        }

        public abstract double GetX();

        public abstract double GetY();

        public abstract void SetLocation(double x, double y);

        virtual public void SetLocation(Point2D p) {
            SetLocation(p.GetX(), p.GetY());
        }

        public static double DistanceSq(double x1, double y1, double x2, double y2) {
            x2 -= x1;
            y2 -= y1;
            return x2 * x2 + y2 * y2;
        }

        virtual public double DistanceSq(double px, double py) {
            return Point2D.DistanceSq(GetX(), GetY(), px, py);
        }

        virtual public double DistanceSq(Point2D p) {
            return Point2D.DistanceSq(GetX(), GetY(), p.GetX(), p.GetY());
        }

        public static double Distance(double x1, double y1, double x2, double y2) {
            return Math.Sqrt(DistanceSq(x1, y1, x2, y2));
        }

        virtual public double Distance(double px, double py) {
            return Math.Sqrt(DistanceSq(px, py));
        }

        virtual public double Distance(Point2D p) {
            return Math.Sqrt(DistanceSq(p));
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            if (obj is Point2D) {
                Point2D p = (Point2D) obj;
                return GetX() == p.GetX() && GetY() == p.GetY();
            }
            return false;
        }
    }
}
