using System;
using System.util;

/*
 * $Id: ElementTags.cs,v 1.8 2008/05/13 11:25:10 psoares33 Exp $
 * 
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2009 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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
    /// A class that contains all the possible tagnames and their attributes.
    /// </summary>
    public class ElementTags 
    {
    
        /// <summary> the root tag. </summary>
        public const string ITEXT = "itext";
    
        /// <summary> attribute of the root and annotation tag (also a special tag within a chapter or section) </summary>
        public const string TITLE = "title";
    
        /// <summary> attribute of the root tag </summary>
        public const string SUBJECT = "subject";
    
        /// <summary> attribute of the root tag </summary>
        public const string KEYWORDS = "keywords";
    
        /// <summary> attribute of the root tag </summary>
        public const string AUTHOR = "author";
    
        /// <summary> attribute of the root tag </summary>
        public const string CREATIONDATE = "creationdate";
    
        /// <summary> attribute of the root tag </summary>
        public const string PRODUCER = "producer";
    
        // Chapters and Sections
    
        /// <summary> the chapter tag </summary>
        public const string CHAPTER = "chapter";
    
        /// <summary> the section tag </summary>
        public const string SECTION = "section";
    
        /// <summary> attribute of section/chapter tag </summary>
        public const string NUMBERDEPTH = "numberdepth";
    
        /// <summary> attribute of section/chapter tag </summary>
        public const string DEPTH = "depth";
    
        /// <summary> attribute of section/chapter tag </summary>
        public const string NUMBER = "number";
    
        /// <summary> attribute of section/chapter tag </summary>
        public const string INDENT = "indent";
    
        /// <summary> attribute of chapter/section/paragraph/table/cell tag </summary>
        public const string LEFT = "left";
    
        /// <summary> attribute of chapter/section/paragraph/table/cell tag </summary>
        public const string RIGHT = "right";
    
        // Phrases, Anchors, Lists and Paragraphs
    
        /// <summary> the phrase tag </summary>
        public const string PHRASE = "phrase";
    
        /// <summary> the anchor tag </summary>
        public const string ANCHOR = "anchor";
    
        /// <summary> the list tag </summary>
        public const string LIST = "list";
    
        /// <summary> the listitem tag </summary>
        public const string LISTITEM = "listitem";
    
        /// <summary> the paragraph tag </summary>
        public const string PARAGRAPH = "paragraph";
    
        /// <summary> attribute of phrase/paragraph/cell tag </summary>
        public const string LEADING = "leading";
    
        /// <summary> attribute of paragraph/image/table tag </summary>
        public const string ALIGN = "align";
    
        /// <summary> attribute of paragraph </summary>
        public const string KEEPTOGETHER = "keeptogether";
    
        /// <summary> attribute of anchor tag </summary>
        public const string NAME = "name";
    
        /// <summary> attribute of anchor tag </summary>
        public const string REFERENCE = "reference";
    
        /// <summary> attribute of list tag </summary>
        public const string LISTSYMBOL = "listsymbol";
    
        /// <summary> attribute of list tag </summary>
        public const string NUMBERED = "numbered";
    
        /// <summary> attribute of the list tag </summary>
        public const string LETTERED = "lettered";

        /// <summary> attribute of list tag </summary>
        public const string FIRST = "first";
    
        /// <summary> attribute of list tag </summary>
        public const string SYMBOLINDENT = "symbolindent";
    
        /// <summary> attribute of list tag </summary>
        public const string INDENTATIONLEFT = "indentationleft";
    
        /// <summary> attribute of list tag </summary>
        public const string INDENTATIONRIGHT = "indentationright";
    
        // Chunks
    
        /// <summary> the chunk tag </summary>
        public const string IGNORE = "ignore";
    
        /// <summary> the chunk tag </summary>
        public const string ENTITY = "entity";
    
        /// <summary> the chunk tag </summary>
        public const string ID = "id";
    
        /// <summary> the chunk tag </summary>
        public const string CHUNK = "chunk";
    
        /// <summary> attribute of the chunk tag </summary>
        public const string ENCODING = "encoding";
    
        /// <summary> attribute of the chunk tag </summary>
        public const string EMBEDDED = "embedded";
    
        /// <summary> attribute of the chunk/table/cell tag </summary>
        public const string COLOR = "color";
    
        /// <summary> attribute of the chunk/table/cell tag </summary>
        public const string RED = "red";
    
        /// <summary> attribute of the chunk/table/cell tag </summary>
        public const string GREEN = "green";
    
        /// <summary> attribute of the chunk/table/cell tag </summary>
        public const string BLUE = "blue";
    
        /// <summary> attribute of the chunk tag </summary>
        public static readonly string SUBSUPSCRIPT = Chunk.SUBSUPSCRIPT.ToLower(System.Globalization.CultureInfo.InvariantCulture);
    
        /// <summary> attribute of the chunk tag </summary>
        public static readonly string LOCALGOTO = Chunk.LOCALGOTO.ToLower(System.Globalization.CultureInfo.InvariantCulture);
    
        /// <summary> attribute of the chunk tag </summary>
        public static readonly string REMOTEGOTO = Chunk.REMOTEGOTO.ToLower(System.Globalization.CultureInfo.InvariantCulture);
    
        /// <summary> attribute of the chunk tag </summary>
        public static readonly string LOCALDESTINATION = Chunk.LOCALDESTINATION.ToLower(System.Globalization.CultureInfo.InvariantCulture);
    
        /// <summary> attribute of the chunk tag </summary>
        public static readonly string GENERICTAG = Chunk.GENERICTAG.ToLower(System.Globalization.CultureInfo.InvariantCulture);
    
        // tables/cells
    
        /// <summary> the table tag </summary>
        public const string TABLE = "table";
    
        /// <summary> the cell tag </summary>
        public const string ROW = "row";
    
        /// <summary> the cell tag </summary>
        public const string CELL = "cell";
    
        /// <summary> attribute of the table tag </summary>
        public const string COLUMNS = "columns";
    
        /// <summary> attribute of the table tag </summary>
        public const string LASTHEADERROW = "lastHeaderRow";
    
        /// <summary> attribute of the table tag </summary>
        public const string CELLPADDING = "cellpadding";
    
        /// <summary> attribute of the table tag </summary>
        public const string CELLSPACING = "cellspacing";
    
        /// <summary> attribute of the table tag </summary>
        public const string OFFSET = "offset";
    
        /// <summary> attribute of the table tag </summary>
        public const string WIDTHS = "widths";
    
        /// <summary> attribute of the table tag </summary>
        public const string TABLEFITSPAGE = "tablefitspage";
    
        /// <summary> attribute of the table tag </summary>
        public const string CELLSFITPAGE = "cellsfitpage";
    
        /// <summary> attribute of the table tag </summary>
        public const string CONVERT2PDFP = "convert2pdfp";
                
        /// <summary> attribute of the cell tag </summary>
        public const string HORIZONTALALIGN = "horizontalalign";
    
        /// <summary> attribute of the cell tag </summary>
        public const string VERTICALALIGN = "verticalalign";
    
        /// <summary> attribute of the cell tag </summary>
        public const string COLSPAN = "colspan";
    
        /// <summary> attribute of the cell tag </summary>
        public const string ROWSPAN = "rowspan";
    
        /// <summary> attribute of the cell tag </summary>
        public const string HEADER = "header";
    
        /// <summary> attribute of the cell tag </summary>
        public const string FOOTER = "footer";

        /// <summary> attribute of the cell tag </summary>
        public const string NOWRAP = "nowrap";
    
        /// <summary> attribute of the table/cell tag </summary>
        public const string BORDERWIDTH = "borderwidth";
    
        /// <summary> attribute of the table/cell tag </summary>
        public const string TOP = "top";
    
        /// <summary> attribute of the table/cell tag </summary>
        public const string BOTTOM = "bottom";
    
        /// <summary> attribute of the table/cell tag </summary>
        public const string WIDTH = "width";
    
        /// <summary> attribute of the table/cell tag </summary>
        public const string BORDERCOLOR = "bordercolor";
    
        /// <summary> attribute of the table/cell tag </summary>
        public const string BACKGROUNDCOLOR = "backgroundcolor";
    
        /// <summary> attribute of the table/cell tag </summary>
        public const string BGRED = "bgred";
    
        /// <summary> attribute of the table/cell tag </summary>
        public const string BGGREEN = "bggreen";
    
        /// <summary> attribute of the table/cell tag </summary>
        public const string BGBLUE = "bgblue";
    
        /// <summary> attribute of the table/cell tag </summary>
        public const string GRAYFILL = "grayfill";
    
        // Misc
    
        /// <summary> the image tag </summary>
        public const string IMAGE = "image";
    
        /// <summary> the image tag </summary>
        public const string BOOKMARKOPEN = "bookmarkopen";
    
        /// <summary> attribute of the image and annotation tag </summary>
        public const string URL = "url";
    
        /// <summary> attribute of the image tag </summary>
        public const string UNDERLYING = "underlying";
    
        /// <summary> attribute of the image tag </summary>
        public const string TEXTWRAP = "textwrap";
    
        /// <summary> attribute of the image tag </summary>
        public const string ALT = "alt";
    
        /// <summary> attribute of the image tag </summary>
        public const string ABSOLUTEX = "absolutex";
    
        /// <summary> attribute of the image tag </summary>
        public const string ABSOLUTEY = "absolutey";
    
        /// <summary> attribute of the image tag </summary>
        public const string PLAINWIDTH = "plainwidth";
    
        /// <summary> attribute of the image tag </summary>
        public const string PLAINHEIGHT = "plainheight";
    
        /// <summary> attribute of the image tag </summary>
        public const string SCALEDWIDTH = "scaledwidth";
    
        /// <summary> attribute of the image tag </summary>
        public const string SCALEDHEIGHT = "scaledheight";
    
        /// <summary> attribute of the image tag </summary>
        public const string  ROTATION = "rotation";
    
        /// <summary> the newpage tag </summary>
        public const string NEWPAGE = "newpage";
    
        /// <summary> the newpage tag </summary>
        public const string NEWLINE = "newline";
    
        /// <summary> the annotation tag </summary>
        public const string ANNOTATION = "annotation";
    
        /// <summary> attribute of the annotation tag </summary>
        public const string FILE = "file";
    
        /// <summary> attribute of the annotation tag </summary>
        public const string DESTINATION = "destination";
    
        /// <summary> attribute of the annotation tag </summary>
        public const string PAGE = "page";
    
        /// <summary> attribute of the annotation tag </summary>
        public const string NAMED = "named";
    
        /// <summary> attribute of the annotation tag </summary>
        public const string APPLICATION = "application";
    
        /// <summary> attribute of the annotation tag </summary>
        public const string PARAMETERS = "parameters";
    
        /// <summary> attribute of the annotation tag </summary>
        public const string OPERATION = "operation";
    
        /// <summary> attribute of the annotation tag </summary>
        public const string DEFAULTDIR = "defaultdir";
    
        /// <summary> attribute of the annotation tag </summary>
        public const string LLX = "llx";
    
        /// <summary> attribute of the annotation tag </summary>
        public const string LLY = "lly";
    
        /// <summary> attribute of the annotation tag </summary>
        public const string URX = "urx";
    
        /// <summary> attribute of the annotation tag </summary>
        public const string URY = "ury";
    
        /// <summary> attribute of the annotation tag </summary>
        public const string CONTENT = "content";
    
        // alignment attribute values
    
        /// <summary> the possible value of an alignment attribute </summary>
        public const string ALIGN_LEFT = "Left";
    
        /// <summary> the possible value of an alignment attribute </summary>
        public const string ALIGN_CENTER = "Center";
    
        /// <summary> the possible value of an alignment attribute </summary>
        public const string ALIGN_RIGHT = "Right";
    
        /// <summary> the possible value of an alignment attribute </summary>
        public const string ALIGN_JUSTIFIED = "Justify";
    
        /// <summary> the possible value of an alignment attribute </summary>
        public const string ALIGN_JUSTIFIED_ALL = "JustifyAll";
    
        /// <summary> the possible value of an alignment attribute </summary>
        public const string ALIGN_TOP = "Top";
    
        /// <summary> the possible value of an alignment attribute </summary>
        public const string ALIGN_MIDDLE = "Middle";
    
        /// <summary> the possible value of an alignment attribute </summary>
        public const string ALIGN_BOTTOM = "Bottom";
    
        /// <summary> the possible value of an alignment attribute </summary>
        public const string ALIGN_BASELINE = "Baseline";
    
        /// <summary> the possible value of an alignment attribute </summary>
        public const string DEFAULT = "Default";
    
        /// <summary> the possible value of an alignment attribute </summary>
        public const string UNKNOWN = "unknown";
    
        /// <summary> the possible value of an alignment attribute </summary>
        public const string FONT = "font";
    
        /// <summary> the possible value of an alignment attribute </summary>
        public const string SIZE = "size";
    
        /// <summary> the possible value of an alignment attribute </summary>
        public const string STYLE = "fontstyle";
    
        /// <summary> the possible value of a tag </summary>
        public const string HORIZONTALRULE = "horizontalrule";
        /** the possible value of a tag */
        public const string PAGE_SIZE  = "pagesize";

        /** the possible value of a tag */
        public const string ORIENTATION  = "orientation";
    
        /** a possible list attribute */
	    public const String ALIGN_INDENTATION_ITEMS = "alignindent";
    	
	    /** a possible list attribute */
	    public const String AUTO_INDENT_ITEMS = "autoindent";
    	
	    /** a possible list attribute */
	    public const String LOWERCASE = "lowercase";

        /**
            * a possible list attribute
            * @since 2.1.3
            */
        public const String FACE = "face";

        /** attribute of the image or iframe tag
            * @since 2.1.3
            */
        public const String SRC = "src";

        // methods
    
        /// <summary>
        /// Translates the alignment value to a String value.
        /// </summary>
        /// <param name="alignment">the alignment value</param>
        /// <returns>the translated value</returns>
        public static string GetAlignment(int alignment) 
        {
            switch (alignment) 
            {
                case Element.ALIGN_LEFT:
                    return ALIGN_LEFT;
                case Element.ALIGN_CENTER:
                    return ALIGN_CENTER;
                case Element.ALIGN_RIGHT:
                    return ALIGN_RIGHT;
                case Element.ALIGN_JUSTIFIED:
                case Element.ALIGN_JUSTIFIED_ALL:
                    return ALIGN_JUSTIFIED;
                case Element.ALIGN_TOP:
                    return ALIGN_TOP;
                case Element.ALIGN_MIDDLE:
                    return ALIGN_MIDDLE;
                case Element.ALIGN_BOTTOM:
                    return ALIGN_BOTTOM;
                case Element.ALIGN_BASELINE:
                    return ALIGN_BASELINE;
                default:
                    return DEFAULT;
            }
        }

    /**
    * Translates a String value to an alignment value.
    * (written by Norman Richards, integrated into iText by Bruno)
    * @param	a String (one of the ALIGN_ constants of this class)
    * @param	an alignment value (one of the ALIGN_ constants of the Element interface) 
    */
        public static int AlignmentValue(String alignment) {
            if (alignment == null) return Element.ALIGN_UNDEFINED;
            if (Util.EqualsIgnoreCase(ALIGN_CENTER, alignment)) {
                return Element.ALIGN_CENTER;
            }
            if (Util.EqualsIgnoreCase(ALIGN_LEFT, alignment)) {
                return Element.ALIGN_LEFT;
            }
            if (Util.EqualsIgnoreCase(ALIGN_RIGHT, alignment)) {
                return Element.ALIGN_RIGHT;
            }
            if (Util.EqualsIgnoreCase(ALIGN_JUSTIFIED, alignment)) {
                return Element.ALIGN_JUSTIFIED;
            }
            if (Util.EqualsIgnoreCase(ALIGN_JUSTIFIED_ALL, alignment)) {
                return Element.ALIGN_JUSTIFIED_ALL;
            }
            if (Util.EqualsIgnoreCase(ALIGN_TOP, alignment)) {
                return Element.ALIGN_TOP;
            }
            if (Util.EqualsIgnoreCase(ALIGN_MIDDLE, alignment)) {
                return Element.ALIGN_MIDDLE;
            }
            if (Util.EqualsIgnoreCase(ALIGN_BOTTOM, alignment)) {
                return Element.ALIGN_BOTTOM;
            }
            if (Util.EqualsIgnoreCase(ALIGN_BASELINE, alignment)) {
                return Element.ALIGN_BASELINE;
            }

            return Element.ALIGN_UNDEFINED;
        }
    
    }
}
