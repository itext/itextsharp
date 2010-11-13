using System;
using System.Collections.Generic;

/*
 * $Id$
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

namespace iTextSharp.text.html {

    /**
     * A class that contains all the possible tagnames and their attributes.
     */

    public class HtmlTags {
    
        /** the root tag. */
        public const string HTML = "html";
    
        /** the head tag */
        public const string HEAD = "head";
    
        /** This is a possible HTML attribute for the HEAD tag. */
        public const string CONTENT = "content";
    
        /** the meta tag */
        public const string META = "meta";
    
        /** attribute of the root tag */
        public const string SUBJECT = "subject";
    
        /** attribute of the root tag */
        public const string KEYWORDS = "keywords";
    
        /** attribute of the root tag */
        public const string AUTHOR = "author";
    
        /** the title tag. */
        public const string TITLE = "title";
    
        /** the script tag. */
        public const string SCRIPT = "script";

        /** This is a possible HTML attribute for the SCRIPT tag. */
        public const string LANGUAGE = "language";

        /** This is a possible value for the LANGUAGE attribute. */
        public const string JAVASCRIPT = "JavaScript";

        /** the body tag. */
        public const string BODY = "body";
    
        /** This is a possible HTML attribute for the BODY tag */
        public const string JAVASCRIPT_ONLOAD = "onLoad";

        /** This is a possible HTML attribute for the BODY tag */
        public const string JAVASCRIPT_ONUNLOAD = "onUnLoad";

        /** This is a possible HTML attribute for the BODY tag. */
        public const string TOPMARGIN = "topmargin";
    
        /** This is a possible HTML attribute for the BODY tag. */
        public const string BOTTOMMARGIN = "bottommargin";
    
        /** This is a possible HTML attribute for the BODY tag. */
        public const string LEFTMARGIN = "leftmargin";
    
        /** This is a possible HTML attribute for the BODY tag. */
        public const string RIGHTMARGIN = "rightmargin";
    
        // Phrases, Anchors, Lists and Paragraphs
    
        /** the chunk tag */
        public const string CHUNK = "font";
    
        /** the phrase tag */
        public const string CODE = "code";
    
        /** the phrase tag */
        public const string VAR = "var";
    
        /** the anchor tag */
        public const string ANCHOR = "a";
    
        /** the list tag */
        public const string ORDEREDLIST = "ol";
    
        /** the list tag */
        public const string UNORDEREDLIST = "ul";
    
        /** the listitem tag */
        public const string LISTITEM = "li";
    
        /** the paragraph tag */
        public const string PARAGRAPH = "p";
    
        /** attribute of anchor tag */
        public const string NAME = "name";
    
        /** attribute of anchor tag */
        public const string REFERENCE = "href";
    
        /** attribute of anchor tag */
        public static string[] H = {"h1", "h2", "h3", "h4", "h5", "h6"};
    
        // Chunks
    
        /** attribute of the chunk tag */
        public const string FONT = "face";
    
        /** attribute of the chunk tag */
        public const string SIZE = "point-size";
    
        /** attribute of the chunk/table/cell tag */
        public const string COLOR = "color";
    
        /** some phrase tag */
        public const string EM = "em";
    
        /** some phrase tag */
        public const string I = "i";
    
        /** some phrase tag */
        public const string STRONG = "strong";
    
        /** some phrase tag */
        public const string B = "b";
    
        /** some phrase tag */
        public const string S = "s";
    
        /** some phrase tag */
        public const string U = "u";
    
        /** some phrase tag */
        public const string SUB = "sub";
    
        /** some phrase tag */
        public const string SUP = "sup";
    
        /** the possible value of a tag */
        public const string HORIZONTALRULE = "hr";
    
        // tables/cells
    
        /** the table tag */
        public const string TABLE = "table";
    
        /** the cell tag */
        public const string ROW = "tr";
    
        /** the cell tag */
        public const string CELL = "td";
    
        /** attribute of the cell tag */
        public const string HEADERCELL = "th";
    
        /** attribute of the table tag */
        public const string COLUMNS = "cols";
    
        /** attribute of the table tag */
        public const string CELLPADDING = "cellpadding";
    
        /** attribute of the table tag */
        public const string CELLSPACING = "cellspacing";
    
        /** attribute of the cell tag */
        public const string COLSPAN = "colspan";
    
        /** attribute of the cell tag */
        public const string ROWSPAN = "rowspan";
    
        /** attribute of the cell tag */
        public const string NOWRAP = "nowrap";
    
        /** attribute of the table/cell tag */
        public const string BORDERWIDTH = "border";
    
        /** attribute of the table/cell tag */
        public const string WIDTH = "width";
    
        /** attribute of the table/cell tag */
        public const string BACKGROUNDCOLOR = "bgcolor";
    
        /** attribute of the table/cell tag */
        public const string BORDERCOLOR = "bordercolor";
    
        /** attribute of paragraph/image/table tag */
        public const string ALIGN = "align";
    
        /** attribute of chapter/section/paragraph/table/cell tag */
        public const string LEFT = "left";
    
        /** attribute of chapter/section/paragraph/table/cell tag */
        public const string RIGHT = "right";
    
        /** attribute of the cell tag */
        public const string HORIZONTALALIGN = "align";
    
        /** attribute of the cell tag */
        public const string VERTICALALIGN = "valign";
    
        /** attribute of the table/cell tag */
        public const string TOP = "top";
    
        /** attribute of the table/cell tag */
        public const string BOTTOM = "bottom";
    
        // Misc
    
        /** the image tag */
        public const string IMAGE = "img";
    
        /** attribute of the image tag */
        public const string URL = "src";
    
        /** attribute of the image tag */
        public const string ALT = "alt";
    
        /** attribute of the image tag */
        public const string PLAINWIDTH = "width";
    
        /** attribute of the image tag */
        public const string PLAINHEIGHT = "height";
    
        /** the newpage tag */
        public const string NEWLINE = "br";
    
        // alignment attribute values
    
        /** the possible value of an alignment attribute */
        public const string ALIGN_LEFT = "Left";
    
        /** the possible value of an alignment attribute */
        public const string ALIGN_CENTER = "Center";
    
        /** the possible value of an alignment attribute */
        public const string ALIGN_RIGHT = "Right";
    
        /** the possible value of an alignment attribute */
        public const string ALIGN_JUSTIFIED = "Justify";
    
        /** the possible value of an alignment attribute */
        public const string ALIGN_TOP = "Top";
    
        /** the possible value of an alignment attribute */
        public const string ALIGN_MIDDLE = "Middle";
    
        /** the possible value of an alignment attribute */
        public const string ALIGN_BOTTOM = "Bottom";
    
        /** the possible value of an alignment attribute */
        public const string ALIGN_BASELINE = "Baseline";
    
        /** the possible value of an alignment attribute */
        public const string DEFAULT = "Default";

        /** The DIV tag. */
        public const string DIV = "div";

        /** The SPAN tag. */
        public const string SPAN = "span";
        /** The LINK tag. */
        public const string LINK = "link";
        
        /** This is a possible HTML attribute for the LINK tag. */
        public const string TEXT_CSS = "text/css";

        /** This is a possible HTML attribute for the LINK tag. */
        public const string REL = "rel";

        /** This is used for inline css style information */
        public const string STYLE = "style";

        /** This is a possible HTML attribute for the LINK tag. */
        public const string TYPE = "type";

        /** This is a possible HTML attribute. */
        public const string STYLESHEET = "stylesheet";
	    /** This is a possible HTML attribute for auto-formated 
        * @since 2.1.3
        */
	    public const String PRE = "pre";
	
        /**
         * Set containing tags that trigger a new line.
         * @since iText 5.0.6
         */
        private static readonly Dictionary<string,object> newLineTags = new Dictionary<string,object>();

        static HtmlTags() {
            // Following list are the basic html tags that force new lines
            // List may be extended as we discover them
            newLineTags[PARAGRAPH] = null;
            newLineTags["blockquote"] = null;
            newLineTags["br"] = null;
        }   
        
        /**
         * Returns true if the tag causes a new line like p, br etc.
         * @since iText 5.0.6
         */
        public static bool IsNewLineTag(string tag) {
            return newLineTags.ContainsKey(tag);
        }
    }
}
