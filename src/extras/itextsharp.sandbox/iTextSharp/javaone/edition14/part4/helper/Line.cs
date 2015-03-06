using System.Collections.Generic;
using iTextSharp.text;

namespace iTextSharp.javaone.edition14.part4.helper
{
    /// <summary>
    /// MyItem implementation that gets its coordinates and color from a list
    /// of items that are all on the same line.
    /// </summary>
    public class Line : MyItem
    {
        /// <summary>
        /// Creates a Line object based on a list of items that have the same
        /// offset of their baseline.
        /// </summary>
        /// <param name="items">a list of MyItem objects</param>
        public Line(List<MyItem> items) : base()
        {
            rectangle = GetItemsRect(items);
            color = items[0].Color;
        }

        /// <summary>
        /// Creates a rectangle that encompasses all the coordinate rectangles
        /// of the items that belong to this line.
        /// </summary>
        /// <param name="items">the items that belong to a line</param>
        /// <returns>a rectangle that encompasses all items belonging to a line</returns>
        private static Rectangle GetItemsRect(List<MyItem> items)
        {
            float left = float.MaxValue;
            float right = 0;
            float top = 0;
            float bottom = float.MaxValue;
            foreach (MyItem item in items)
            {
                if (item.Rectangle.Left < left)
                    left = item.Rectangle.Left;
                if (item.Rectangle.Right > right)
                    right = item.Rectangle.Right;
                if (item.Rectangle.Top > top)
                    top = item.Rectangle.Top;
                if (item.Rectangle.Bottom < bottom)
                    bottom = item.Rectangle.Bottom;
            }
            return new Rectangle(left, bottom, right, top);
        }
    }
}
