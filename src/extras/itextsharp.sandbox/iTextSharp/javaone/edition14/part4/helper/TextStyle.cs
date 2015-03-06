using System;
using iTextSharp.text.pdf.parser;

namespace iTextSharp.javaone.edition14.part4.helper
{
    /// <summary>
    /// Stores the font name and font size of a text item.
    /// </summary>
    public class TextStyle
    {
        /// <summary>
        /// The name of the font.
        /// </summary>
        String fontName;

        /// <summary>
        /// The size of the font.
        /// </summary>
        float fontSize;


        /// <summary>
        /// Creates a TextStyle object.
        /// </summary>
        /// <param name="fontName">fontName the name of the font</param>
        /// <param name="fontSize">the size of the font</param>
        public TextStyle(String fontName, float fontSize)
        {
            this.fontName = fontName;
            this.fontSize = fontSize;
        }

        /// <summary>
        /// Creates a TextStyle object by getting the font name and font size
        /// from a TextRenderInfo object.
        /// </summary>
        /// <param name="textRenderInfo">Object that contains info about a text snippet</param>
        public TextStyle(TextRenderInfo textRenderInfo)
        {
            String font = textRenderInfo.GetFont().FullFontName[0][3];
            if (font.Contains("+"))
                font = font.Substring(font.IndexOf("+") + 1, font.Length - font.IndexOf("+") - 1);
            if (font.Contains("-"))
                font = font.Substring(0, font.IndexOf("-"));
            this.fontName = font;
            this.fontSize = textRenderInfo.GetAscentLine().GetStartPoint()[1] - textRenderInfo.GetDescentLine().GetStartPoint()[1];
        }

        public override int GetHashCode()
        {
            return fontName.GetHashCode() ^ (int)Math.Round(fontSize * 10);
        }

        public override bool Equals(object obj)
        {
            if (obj is TextStyle)
            {
                TextStyle ts = (TextStyle) obj;
                return fontName.Equals(ts.fontName) && Math.Abs(fontSize - ts.fontSize) < 0.05;
            }
            return false;
        }
    }
}
