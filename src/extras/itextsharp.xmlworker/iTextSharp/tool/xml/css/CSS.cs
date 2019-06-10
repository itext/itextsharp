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
using System.Collections.Generic;
using iTextSharp.text;

namespace iTextSharp.tool.xml.css {

    /**
     * CSS Property-Value container.
     *
     */
    public static class CSS {

        /**
         * Contains CSS Properties
         *
         */
        public static class Property {
            public const String BACKGROUND = "background";
            public const String BACKGROUND_IMAGE = "background-image";
            public const String BACKGROUND_REPEAT = "background-repeat";
            public const String BACKGROUND_ATTACHMENT = "background-attachment";
            public const String BACKGROUND_POSITION = "background-position";
            public const String BACKGROUND_COLOR = "background-color";
            public const String LIST_STYLE = "list-style";
            public const String LIST_STYLE_TYPE = "list-style-type";
            public const String LIST_STYLE_POSITION = "list-style-position";
            public const String LIST_STYLE_IMAGE = "list-style-image";
            public const String MARGIN = "margin";
            public const String TOP = "top";
            public const String MARGIN_LEFT = "margin-left";
            public const String MARGIN_RIGHT = "margin-right";
            public const String MARGIN_TOP = "margin-top";
            public const String MARGIN_BOTTOM = "margin-bottom";
            public const String BORDER = "border";
            public const String BORDER_LEFT = "border-left";
            public const String BORDER_TOP = "border-top";
            public const String BORDER_RIGHT = "border-right";
            public const String BORDER_BOTTOM = "border-bottom";
            public const String BORDER_WIDTH = "border-width";
            public const String BORDER_STYLE = "border-style";
            public const String BORDER_COLOR = "border-color";
            public const String BORDER_COLLAPSE = "border-collapse";
            public const String BORDER_SPACING = "border-spacing";
            public const String BORDER_TOP_WIDTH = "border-top-width";
            public const String BORDER_BOTTOM_WIDTH = "border-bottom-width";
            public const String BORDER_LEFT_WIDTH = "border-left-width";
            public const String BORDER_RIGHT_WIDTH = "border-right-width";
            public const String BORDER_TOP_COLOR = "border-top-color";
            public const String BORDER_BOTTOM_COLOR = "border-bottom-color";
            public const String BORDER_LEFT_COLOR = "border-left-color";
            public const String BORDER_RIGHT_COLOR = "border-right-color";
            public const String BORDER_TOP_STYLE = "border-top-style";
            public const String BORDER_BOTTOM_STYLE = "border-bottom-style";
            public const String BORDER_LEFT_STYLE = "border-left-style";
            public const String BORDER_RIGHT_STYLE = "border-right-style";
            public const String PADDING = "padding";
            public const String PADDING_TOP = "padding-top";
            public const String PADDING_BOTTOM = "padding-bottom";
            public const String PADDING_LEFT = "padding-left";
            public const String PADDING_RIGHT = "padding-right";
            public const String FONT = "font";
            public const String FONT_WEIGHT = "font-weight";
            public const String FONT_SIZE = "font-size";
            public const String FONT_STYLE = "font-style";
            public const String FONT_FAMILY = "font-family";
            public const String TEXT_DECORATION = "text-decoration";
            public const String COLOR = "color";
            public const String TAB_INTERVAL = "tab-interval";
            public const String XFA_TAB_COUNT = "xfa-tab-count";
            public const String XFA_FONT_HORIZONTAL_SCALE = "xfa-font-horizontal-scale";
            public const String XFA_FONT_VERTICAL_SCALE = "xfa-font-vertical-scale";
            public const String BEFORE = "before";
            public const String AFTER = "after";
            public const String HEIGHT = "height";
            public const String WIDTH = "width";
            public const String LETTER_SPACING = "letter-spacing";
            public const String VERTICAL_ALIGN = "vertical-align";
            public const String LINE_HEIGHT = "line-height";
            public const String TEXT_ALIGN = "text-align";
            public const String TEXT_VALIGN = "text-valign";
            public const String TEXT_INDENT = "text-indent";
            public const String POSITION = "position";
            public const String EMPTY_CELLS = "empty-cells";
            public const String CELLPADDING = "cellpadding";
            //deprecated
            public const String CELLPADDING_LEFT = "cellpadding-left";
            public const String CELLPADDING_TOP = "cellpadding-top";
            public const String CELLPADDING_RIGHT = "cellpadding-right";
            public const String CELLPADDING_BOTTOM = "cellpadding-bottom";
            
            public const String CAPTION_SIDE = "caption-side";
            public const String TAB_STOPS = "tab-stops";
            public const String XFA_TAB_STOPS = "xfa-tab-stops";
            public const String PAGE_BREAK_BEFORE = "page-break-before";
            public const String PAGE_BREAK_INSIDE = "page-break-inside";
            public const String PAGE_BREAK_AFTER = "page-break-after";
            public const String REPEAT_HEADER = "repeat-header";
            public const String REPEAT_FOOTER = "repeat-footer";
		    public const String LEFT = "left";
		    public const String DISPLAY = "display";
		    public const String MIN_WIDTH = "min-width";
		    public const String MAX_WIDTH = "max-width";
            public const String MIN_HEIGHT = "min-height";
            public const String MAX_HEIGHT = "max-height";
            public const String RIGHT = "right";
            public const String BOTTOM = "bottom";
            public const String FLOAT = "float";
            public const String DIRECTION = "direction";
        }

        /**
         * Contains CSS Values for properties
         *
         */
        public static class Value {
            public const String THIN = "thin";
            public const String MEDIUM = "medium";
            public const String THICK = "thick";
            public const String NONE = "none";
            public const String HIDDEN = "hidden";
            public const String DOTTED = "dotted";
            public const String DASHED = "dashed";
            public const String SOLID = "solid";
            public const String DOUBLE = "double";
            public const String GROOVE = "groove";
            public const String RIDGE = "ridge";
            public const String INSET = "inset";
            public const String OUTSET = "outset";
            public const String LEFT = "left";
            public const String CENTER = "center";
            public const String JUSTIFY = "justify";
            public const String BOTTOM = "bottom";
            public const String TOP = "top";
            public const String RIGHT = "right";
            public const String REPEAT = "repeat";
            public const String NO_REPEAT = "no-repeat";
            public const String REPEAT_X = "repeat-x";
            public const String REPEAT_Y = "repeat-y";
            public const String FIXED = "fixed";
            public const String SCROLL = "scroll";
            public const String DISC = "disc";
            public const String SQUARE = "square";
            public const String CIRCLE = "circle";
            public const String DECIMAL = "decimal";
            public const String LOWER_ROMAN = "lower-roman";
            public const String UPPER_ROMAN = "upper-roman";
            public const String LOWER_GREEK = "lower-greek";
            public const String UPPER_GREEK = "upper-greek";
            public const String LOWER_ALPHA = "lower-alpha";
            public const String UPPER_ALPHA = "upper-alpha";
            public const String LOWER_LATIN = "lower-latin";
            public const String UPPER_LATIN = "upper-latin";
            public const String INSIDE = "inside";
            public const String OUTSIDE = "outside";
            public const String INHERIT = "inherit";
            public const String UNDERLINE = "underline";
            public const String BOLD = "bold";
            public const String ITALIC = "italic";
            public const String OBLIQUE = "oblique";
            public const String SUPER = "super";
            public const String SUB = "sub";
            public const String TEXT_TOP = "text-top";
            public const String TEXT_BOTTOM = "text-bottom";
            public const String LINE_THROUGH = "line-through";
            public const String RELATIVE = "relative";
            public const String HIDE = "hide";
            public const String XX_SMALL = "xx-small";
            public const String X_SMALL = "x-small";
            public const String SMALL = "small";
            public const String LARGE = "large";
            public const String X_LARGE = "x-large";
            public const String XX_LARGE = "xx-large";
            public const String SMALLER = "smaller";
            public const String LARGER = "larger";
            public const String PX = "px";
            public const String IN = "in";
            public const String CM = "cm";
            public const String MM = "mm";
            public const String PT = "pt";
            public const String PC = "pc";
            public const String PERCENTAGE = "%";
            public const String EM = "em";
            public const String EX = "ex";
            public const String ALWAYS = "always";
            public const String AVOID = "avoid";
            public const String ABSOLUTE = "absolute";
            public const String AUTO = "auto";
		    public const String INLINE = "inline";
		    public const String BLOCK = "block";
            public const String SEPARATE = "separate";
            public const String COLLAPSE = "collapse";
            public const String RTL = "rtl";
            public const String LTR = "ltr";
            public const String INLINE_BLOCK = "inline-block";
		    public const String INLINE_TABLE = "inline-table";
		    public const String LIST_ITEM = "list-item";
		    public const String RUN_IN = "run-in";
		    public const String TABLE = "table";
		    public const String TABLE_CAPTION = "table-caption";
		    public const String TABLE_CELL = "table-cell";
		    public const String TABLE_COLUMN_GROUP = "table-column-group";
		    public const String TABLE_COLUMN = "table-column";
		    public const String TABLE_FOOTER_GROUP = "table-footer-group";
		    public const String TABLE_HEADER_GROUP = "table-header-group";
		    public const String TABLE_ROW = "table-row";
		    public const String TABLE_ROW_GROUP = "table-row-group";
        }

        private static  Dictionary<String, int> cssAlignMap = new Dictionary<String, int>();
        private static  String Default = "default";

        static CSS() {
            cssAlignMap.Add(Value.LEFT.ToLower(), Element.ALIGN_LEFT);
            cssAlignMap.Add(Value.CENTER.ToLower(), Element.ALIGN_CENTER);
            cssAlignMap.Add(Value.RIGHT.ToLower(), Element.ALIGN_RIGHT);
            cssAlignMap.Add(Value.JUSTIFY.ToLower(), Element.ALIGN_JUSTIFIED);
            cssAlignMap.Add(Default.ToLower(), Element.ALIGN_UNDEFINED);
        }
        
        public static int GetElementAlignment(String cssAlignment) {
            String lower = cssAlignment.ToLower();
            if (cssAlignMap.ContainsKey(lower)) {
                return cssAlignMap[lower];
            }
            return cssAlignMap[Default];
        }
    }
}

