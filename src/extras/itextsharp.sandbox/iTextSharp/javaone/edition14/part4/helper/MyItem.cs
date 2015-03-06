using System;
using iTextSharp.awt.geom;
using iTextSharp.text;

namespace iTextSharp.javaone.edition14.part4.helper
{
    /// <summary>
    /// Abstract class that is used as a superclass for specific item classes
    /// such as TextItem, ImageItem, Line and Structure.
    /// </summary>
    public abstract class MyItem : IComparable<MyItem>
    {
        /// <summary>
        /// If we want to compare item positions we should add some tolerance.
        /// </summary>
        public static readonly float ItemPositionTolerance = 3f;

        /// <summary>
        /// Rectangle that defines the coordinates of an item.
        /// </summary>
        protected Rectangle rectangle;

        /// <summary>
        /// Color that will be used to mark the item.
        /// </summary>
        protected BaseColor color;

        /// <summary>
        /// Rectangle that defines the coordinates of an item.
        /// </summary>
        /// <returns>coordinates in the form of a Rectangle object</returns>
        public Rectangle Rectangle 
        {
            get { return rectangle; }
        }

        /// <summary>
        /// Color that will be used to mark the item.
        /// </summary>
        /// <returns>a BaseColor object</returns>
        public BaseColor Color 
        {
            get { return color; }
        }

        /// <summary>
        /// * Gets the lower left corner of the item.
        /// For image items it returns lower left corner of bounding box.
        /// For text items it returns start point of a baseline.
        /// </summary>
        /// <returns>point of the lower left corner</returns>
        public virtual Point GetLL()
        {
            return new Point(Rectangle.Left, Rectangle.Bottom);
        }

        /// <summary>
        /// Creates an instance of the MyItem object
        /// </summary>
        protected MyItem()
        {
            this.rectangle = rectangle;
            this.color = color;
        }
        
        public int CompareTo(MyItem o)
        {
            double left = GetLL().x;
            double bottom = GetLL().y;
            double oLeft = o.GetLL().x;
            double oBottom = o.GetLL().y;
            if (bottom - oBottom > ItemPositionTolerance)
                return -1;
            else if (oBottom - bottom > ItemPositionTolerance)
                return 1;
            else
                return left.CompareTo(oLeft);
        }
    }
}
