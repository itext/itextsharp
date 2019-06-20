/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
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
