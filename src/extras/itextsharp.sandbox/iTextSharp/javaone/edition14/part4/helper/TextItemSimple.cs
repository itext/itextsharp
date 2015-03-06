using iTextSharp.awt.geom;
using iTextSharp.text;
using iTextSharp.text.pdf.parser;

namespace iTextSharp.javaone.edition14.part4.helper
{
    /// <summary>
    /// Subclass of the MyItem class that is used to store the coordinates
    /// of a text snippet.
    /// </summary>
    public class TextItemSimple : MyItem
    {
        /// <summary>
        /// Position of the baseline of the text.
        /// </summary>
        float baseline;

        /// <summary>
        /// Creates a TextItem based on a TextRenderInfo object.
        /// </summary>
        /// <param name="textRenderInfo">the TextRenderInfo object</param>
        public TextItemSimple(TextRenderInfo textRenderInfo)
        {
            baseline = textRenderInfo.GetBaseline().GetStartPoint()[1];
            rectangle = GetRectangle(textRenderInfo);
            color = BaseColor.BLUE;
        }

        /// <see cref="MyItem.GetLL"/>
        public override Point GetLL()
        {
            return new Point(Rectangle.Left, baseline);
        }

        /// <summary>
        /// Stores the start and end points and the ascent and descent info from
        /// a text snippet into a Rectangle object.
        /// </summary>
        /// <param name="textRenderInfo">Object that contains info about a text snippet</param>
        /// <returns>coordinates in the form of a Rectangle object</returns>
        static Rectangle GetRectangle(TextRenderInfo textRenderInfo)
        {
            LineSegment descentLine = textRenderInfo.GetDescentLine();
            LineSegment ascentLine = textRenderInfo.GetAscentLine();
            float x0 = descentLine.GetStartPoint()[0];
            float x1 = descentLine.GetEndPoint()[0];
            float y0 = descentLine.GetStartPoint()[1];
            float y1 = ascentLine.GetEndPoint()[1];
            return new Rectangle(x0, y0, x1, y1);
        }

    }
}
