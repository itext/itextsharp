using System;

/*
 * $Id$
 * 
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */

namespace iTextSharp.text 
{

    /// <summary>
    /// Interface for a text element.
    /// </summary>
    /// <seealso cref="T:iTextSharp.text.Anchor"/>
    /// <seealso cref="T:iTextSharp.text.Cell"/>
    /// <seealso cref="T:iTextSharp.text.Chapter"/>
    /// <seealso cref="T:iTextSharp.text.Chunk"/>
    /// <seealso cref="T:iTextSharp.text.Gif"/>
    /// <seealso cref="T:iTextSharp.text.Graphic"/>
    /// <seealso cref="T:iTextSharp.text.Header"/>
    /// <seealso cref="T:iTextSharp.text.Image"/>
    /// <seealso cref="T:iTextSharp.text.Jpeg"/>
    /// <seealso cref="T:iTextSharp.text.List"/>
    /// <seealso cref="T:iTextSharp.text.ListItem"/>
    /// <seealso cref="T:iTextSharp.text.Meta"/>
    /// <seealso cref="T:iTextSharp.text.Paragraph"/>
    /// <seealso cref="T:iTextSharp.text.Phrase"/>
    /// <seealso cref="T:iTextSharp.text.Rectangle"/>
    /// <seealso cref="T:iTextSharp.text.Row"/>
    /// <seealso cref="T:iTextSharp.text.Section"/>
    /// <seealso cref="T:iTextSharp.text.Table"/>
    public class Element 
    {
    
        // static membervariables (meta information)
    
        /// <summary> This is a possible type of Element. </summary>
        public const int HEADER = 0;
    
        /// <summary> This is a possible type of Element. </summary>
        public const int TITLE = 1;
    
        /// <summary> This is a possible type of Element. </summary>
        public const int SUBJECT = 2;
    
        /// <summary> This is a possible type of Element. </summary>
        public const int KEYWORDS = 3;
    
        /// <summary> This is a possible type of Element. </summary>
        public const int AUTHOR = 4;
    
        /// <summary> This is a possible type of Element. </summary>
        public const int PRODUCER = 5;
    
        /// <summary> This is a possible type of Element. </summary>
        public const int CREATIONDATE = 6;
    
        /// <summary> This is a possible type of Element. </summary>
        public const int CREATOR = 7;

        /// <summary> This is a possible type of Element. </summary>
        public const int LANGUAGE = 8;
        // static membervariables (content)
    
        /// <summary> This is a possible type of Element. </summary>
        public const int CHUNK = 10;
    
        /// <summary> This is a possible type of Element. </summary>
        public const int PHRASE = 11;
    
        /// <summary> This is a possible type of Element. </summary>
        public const int PARAGRAPH = 12;
    
        /// <summary> This is a possible type of Element </summary>
        public const int SECTION = 13;
    
        /// <summary> This is a possible type of Element </summary>
        public const int LIST = 14;
    
        /// <summary> This is a possible type of Element </summary>
        public const int LISTITEM = 15;
    
        /// <summary> This is a possible type of Element </summary>
        public const int CHAPTER = 16;
    
        /// <summary> This is a possible type of Element </summary>
        public const int ANCHOR = 17;
    
        // static membervariables (tables)
    
        /// <summary> This is a possible type of Element. </summary>
        public const int PTABLE = 23;
    
        // static membervariables (annotations)
    
        /// <summary> This is a possible type of Element. </summary>
        public const int ANNOTATION = 29;
    
        // static membervariables (geometric figures)
    
        /// <summary> This is a possible type of Element. </summary>
        public const int RECTANGLE = 30;
    
        /// <summary> This is a possible type of Element. </summary>
        public const int JPEG = 32;
    
	    /** This is a possible type of <CODE>Element</CODE>. */
	    public const int JPEG2000 = 33;

        /// <summary> This is a possible type of Element. </summary>
        public const int IMGRAW = 34;
    
        /// <summary> This is a possible type of Element. </summary>
        public const int IMGTEMPLATE = 35;
    
        /**
         * This is a possible type of <CODE>Element</CODE>.
         * @since	2.1.5
         */
        public const int JBIG2 = 36;

        /**
         * This is a possible type of <CODE>Element</CODE>.
         * @since	5.3.0
         */
        public const int DIV = 37;

        public const int BODY = 38;
	
        /** This is a possible type of <CODE>Element</CODE>. */
        public const int MARKED = 50;
    
        /** This is a possible type of <CODE>Element</CODE>.
        * @since 2.1.2
        */
        public const int YMARK = 55;

        /**
         * This is an element thats not an element.
         * @see WritableDirectElement
         */
        public const int WRITABLE_DIRECT = 666;

        // static membervariables (alignment)
       
        /// <summary>
        /// A possible value for paragraph Element.  This
        /// specifies that the text is aligned to the left
        /// indent and extra whitespace should be placed on
        /// the right.
        /// </summary>
        public const int ALIGN_UNDEFINED = -1;
    
        /// <summary>
        /// A possible value for paragraph Element.  This
        /// specifies that the text is aligned to the left
        /// indent and extra whitespace should be placed on
        /// the right.
        /// </summary>
        public const int ALIGN_LEFT = 0;
    
        /// <summary>
        /// A possible value for paragraph Element.  This
        /// specifies that the text is aligned to the center
        /// and extra whitespace should be placed equally on
        /// the left and right.
        /// </summary>
        public const int ALIGN_CENTER = 1;
    
        /// <summary>
        /// A possible value for paragraph Element.  This
        /// specifies that the text is aligned to the right
        /// indent and extra whitespace should be placed on
        /// the left.
        /// </summary>
        public const int ALIGN_RIGHT = 2;
    
        /// <summary>
        /// A possible value for paragraph Element.  This
        /// specifies that extra whitespace should be spread
        /// out through the rows of the paragraph with the
        /// text lined up with the left and right indent
        /// except on the last line which should be aligned
        /// to the left.
        /// </summary>
        public const int ALIGN_JUSTIFIED = 3;
    
        /// <summary>
        /// A possible value for vertical Element.
        /// </summary>
        public const int ALIGN_TOP = 4;
    
        /// <summary>
        /// A possible value for vertical Element.
        /// </summary>
        public const int ALIGN_MIDDLE = 5;
    
        /// <summary>
        /// A possible value for vertical Element.
        /// </summary>
        public const int ALIGN_BOTTOM = 6;
    
        /// <summary>
        /// A possible value for vertical Element.
        /// </summary>
        public const int ALIGN_BASELINE = 7;

        /// <summary>
        /// Does the same as ALIGN_JUSTIFIED but the last line is also spread out.
        /// </summary>
        public const int ALIGN_JUSTIFIED_ALL = 8;

        // static member variables for CCITT compression
    
        /// <summary>
        /// Pure two-dimensional encoding (Group 4)
        /// </summary>
        public const int CCITTG4 = 0x100;
        /// <summary>
        /// Pure one-dimensional encoding (Group 3, 1-D)
        /// </summary>
        public const int CCITTG3_1D = 0x101;
        /// <summary>
        /// Mixed one- and two-dimensional encoding (Group 3, 2-D)
        /// </summary>
        public const int CCITTG3_2D = 0x102;    
        /// <summary>
        /// A flag indicating whether 1-bits are to be interpreted as black pixels
        /// and 0-bits as white pixels,
        /// </summary>
        public const int CCITT_BLACKIS1 = 1;
        /// <summary>
        /// A flag indicating whether the filter expects extra 0-bits before each
        /// encoded line so that the line begins on a byte boundary.
        /// </summary>
        public const int CCITT_ENCODEDBYTEALIGN = 2;
        /// <summary>
        /// A flag indicating whether end-of-line bit patterns are required to be
        ///  present in the encoding.
        /// </summary>
        public const int CCITT_ENDOFLINE = 4;
        /// <summary>
        /// A flag indicating whether the filter expects the encoded data to be
        /// terminated by an end-of-block pattern, overriding the Rows
        /// parameter. The use of this flag will set the key /EndOfBlock to false.
        /// </summary>
        public const int CCITT_ENDOFBLOCK = 8;
    
    }
}
