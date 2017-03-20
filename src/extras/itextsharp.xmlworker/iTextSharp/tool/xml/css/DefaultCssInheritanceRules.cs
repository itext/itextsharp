/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
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
using System.util;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.html;
namespace iTextSharp.tool.xml.css {

    /**
     * @author redlab_b
     *
     */
    public class DefaultCssInheritanceRules : ICssInheritanceRules {

        /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.tool.xml.css.CssInheritanceRules#inheritCssTag(java.lang
         * .String)
         */
        virtual public bool InheritCssTag(String tag) {
            return true;
        }

        private static readonly IList<String> GLOBAL = new List<string>(new String[] {
            CSS.Property.WIDTH, CSS.Property.HEIGHT,
            CSS.Property.MIN_WIDTH, CSS.Property.MAX_WIDTH,
            CSS.Property.MIN_HEIGHT, CSS.Property.MAX_HEIGHT,
            CSS.Property.MARGIN,
            CSS.Property.MARGIN_LEFT, CSS.Property.MARGIN_RIGHT,
            CSS.Property.MARGIN_TOP, CSS.Property.MARGIN_BOTTOM,
            CSS.Property.PADDING,
            CSS.Property.PADDING_LEFT, CSS.Property.PADDING_RIGHT,
            CSS.Property.PADDING_TOP, CSS.Property.PADDING_BOTTOM,
            CSS.Property.BORDER_TOP_WIDTH, CSS.Property.BORDER_TOP_STYLE, CSS.Property.BORDER_TOP_COLOR,
            CSS.Property.BORDER_BOTTOM_WIDTH,CSS.Property.BORDER_BOTTOM_STYLE, CSS.Property.BORDER_BOTTOM_COLOR,
            CSS.Property.BORDER_LEFT_WIDTH, CSS.Property.BORDER_LEFT_STYLE, CSS.Property.BORDER_LEFT_COLOR,
            CSS.Property.BORDER_RIGHT_WIDTH, CSS.Property.BORDER_RIGHT_STYLE,CSS.Property.BORDER_RIGHT_COLOR,
            CSS.Property.PAGE_BREAK_BEFORE, CSS.Property.PAGE_BREAK_AFTER,
            CSS.Property.LEFT, CSS.Property.TOP,CSS.Property.RIGHT,CSS.Property.BOTTOM,
            CSS.Property.POSITION });
        private static readonly IList<String> PARENT_TO_TABLE = new List<string>(new String[] {"line-height", "font-size",
            "font-style", "font-weight",
            "text-indent",
            CSS.Property.CELLPADDING, CSS.Property.CELLPADDING_LEFT, CSS.Property.CELLPADDING_TOP,
            CSS.Property.CELLPADDING_RIGHT, CSS.Property.CELLPADDING_BOTTOM, CSS.Property.DIRECTION});
        private static readonly IList<String> TABLE_IN_ROW = new List<string>(new String[] { "background-color", CSS.Property.DIRECTION });
        // styles that should not be applied on the content of a td-tag.
        private static readonly IList<String> TD_TO_CONTENT = new List<string>(new String[] { "vertical-align" });
        private static readonly List<String> DIV_TO_CONTENT = new List<string>(new String[] { CSS.Property.BACKGROUND, CSS.Property.BACKGROUND_COLOR, CSS.Property.FLOAT });

        /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.tool.xml.css.CssInheritanceRules#inheritCssSelector(com.
         * itextpdf.tool.xml.Tag, java.lang.String)
         */
        virtual public bool InheritCssSelector(Tag tag, String key) {
            if (GLOBAL.Contains(key)) {
                return false;
            }
            if (HTML.Tag.TABLE.Equals(tag.Name)) {
                return !PARENT_TO_TABLE.Contains(key);
            }
            if (HTML.Tag.TABLE.Equals(tag.Parent.Name)) {
                return !TABLE_IN_ROW.Contains(key);
            }
            if (Util.EqualsIgnoreCase(HTML.Tag.TD, tag.Parent.Name)) {
                return !TD_TO_CONTENT.Contains(key);
            }
            if (Util.EqualsIgnoreCase(HTML.Tag.DIV, tag.Parent.Name)) {
                return !DIV_TO_CONTENT.Contains(key);
            }
            return true;
        }
    }
}
