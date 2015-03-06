using System.Collections.Generic;
using iTextSharp.awt.geom;
using iTextSharp.text;
using iTextSharp.text.pdf.parser;

namespace iTextSharp.javaone.edition14.part4.helper
{
    /// <summary>
    /// Subclass of the MyItem class that is used to store the coordinates
    /// of a text snippet.
    /// </summary>
    public class TextItem : MyItem
    {
        /// <summary>
        /// Color to be used to mark text that is part of an artifact.
        /// </summary>
        private static readonly BaseColor artifactColor = BaseColor.GRAY;

        /// <summary>
        /// Colors to be used to mark text based on the font that is used.
        /// </summary>
        private static readonly Dictionary<TextStyle, BaseColor> textStyles = new Dictionary<TextStyle, BaseColor>();

        /// <summary>
        /// Position of the baseline of the text.
        /// </summary>
        float baseline;

        /// <summary>
        /// Creates a TextItem based on a TextRenderInfo object.
        /// </summary>
        /// <param name="textRenderInfo">the TextRenderInfo object</param>
        /// <param name="top">the Y coordinate of the top margin</param>
        public TextItem(TextRenderInfo textRenderInfo, float top)
        {
            textStyles.Add(new TextStyle("FranklinGothic", 10.5f), BaseColor.ORANGE);
            textStyles.Add(new TextStyle("FranklinGothic", 8f), BaseColor.GREEN);
            textStyles.Add(new TextStyle("NewBaskerville", 10f), BaseColor.BLUE);
            textStyles.Add(new TextStyle("Courier", 9.5f), BaseColor.BLUE);
            textStyles.Add(new TextStyle("CombiNumerals", 13.5f), BaseColor.PINK);
        

            baseline = textRenderInfo.GetBaseline().GetStartPoint()[1];
            rectangle = GetRectangle(textRenderInfo);
            color = GetColor(textRenderInfo, top);
        }

        /// <see cref="MyItem.GetLL"/>
        public override Point GetLL()
        {
            return new Point(Rectangle.Left, baseline);
        }

        /// <summary>
        /// Determines the color that will mark the text snippet based on the
        /// position of the snippet (in case it's an artifact) or it's style
        /// (font name and size).
        /// </summary>
        /// <param name="textRenderInfo">the TextRenderInfo object</param>
        /// <param name="top">the Y position of the top margin</param>
        /// <returns>a color that will be used to mark the text snippet</returns>
        static BaseColor GetColor(TextRenderInfo textRenderInfo, float top)
        {
            if (textRenderInfo.GetBaseline().GetStartPoint()[1] > top)
                return artifactColor;
            TextStyle ts = new TextStyle(textRenderInfo);
            return textStyles[ts];
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
