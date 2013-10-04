namespace iTextSharp.awt.geom
{
    public class Point
    {
        public double x;
        public double y;

        public Point() {
            SetLocation(0, 0);
        }

        public Point(int x, int y) {
            SetLocation(x, y);
        }

        public Point(double x, double y) {
            SetLocation(x, y);
        }

        public Point(Point p) {
            SetLocation(p.x, p.y);
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            if (obj is Point) {
                Point p = (Point)obj;
                return x == p.x && y == p.y;
            }
            return false;
        }

        public override string ToString() {
            return "Point: [x=" + x + ",y=" + y + "]"; //$NON-NLS-1$ //$NON-NLS-2$ //$NON-NLS-3$
        }

        public double GetX() {
            return x;
        }

        public double GetY() {
            return y;
        }

        public Point GetLocation() {
            return new Point(x, y);
        }

        public void SetLocation(Point p) {
            SetLocation(p.x, p.y);
        }

        public void SetLocation(int x, int y) {
            SetLocation((double)x, (double)y);
        }

        public void SetLocation(double x, double y) {
    	    this.x = x;
    	    this.y = y;
        }

        public void Move(int x, int y) {
            Move((double)x, (double)y);
        }

        public void Move(double x, double y) {
            SetLocation(x, y);
        }

        public void Translate(int dx, int dy) {
            Translate((double)x, (double)y);
        }
        public void Translate(double dx, double dy) {
            x += dx;
            y += dy;
        } 
    }
}