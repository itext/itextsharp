using System;
/*
 * $Id: HTML.java 168 2011-06-08 08:37:18Z redlab_b $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2011 1T3XT BVBA
 * Authors: Balder Van Camp, Emiel Ackermann, et al.
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
namespace iTextSharp.tool.xml.html {

    /**
     * Contains Strings of all used HTML tags and attributes.
     * @author redlab_b
     *
     */
    public class HTML {

        /**
         *
         * All Tags.
         *
         */
        public static class Tag {

            public const String THEAD = "thead";
            public const String TBODY = "thead";
            public const String TFOOT = "tfoot";
            public const String OL = "ol";
            public const String UL = "ul";
            public const String CAPTION = "caption";
            public const String PRE = "pre";
            public const String P = "p";
            public const String DIV = "div";
            public const String H1 = "h1";
            public const String H2 = "h2";
            public const String H3 = "h3";
            public const String H4 = "h4";
            public const String H5 = "h5";
            public const String H6 = "h6";
            public const String TD = "td";
            public const String BR = "br";
            public const String LI = "li";
            public const String DD = "dd";
            public const String DT = "dt";
            public const String TH = "th";
            public const String HR = "hr";
            public const String BODY = "body";
            public const String HTML = "html";
            public const String TABLE = "table";
            public const String SCRIPT = "script";
            public const String HEAD = "head";
            public const String LINK = "link";
            public const String META = "meta";
        }
        /**
         * All attributes
         */
        public static class Attribute {

            public const String CELLPADDING = "cellpadding";
            public const String CELLSPACING = "cellspacing";
            public const String STYLE = "style";
            public const String CLASS = "class";
            public const String ID = "id";
            public const String HREF = "href";
            public const String NAME = "name";
            public const String SRC = "src";
            public const String WIDTH = "width";
            public const String HEIGHT = "height";
            public const String TYPE = "type";
            public const String COLSPAN = "colspan";
            public const String ROWSPAN = "rowspan";

        }
    }
}