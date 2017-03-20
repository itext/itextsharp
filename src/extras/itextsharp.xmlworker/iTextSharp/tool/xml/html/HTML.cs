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
namespace iTextSharp.tool.xml.html {

    /**
     * Contains Strings of all used HTML tags and attributes.
     * @author redlab_b
     *
     */
    public class HTML {

        /**
         *
         * All Tags used in HTML.
         *
         */
        public static class Tag {

            public const String XML = "xml";
            public const String THEAD = "thead";
            public const String TBODY = "tbody";
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
            public const String STYLE = "style";
            public const String ADDRESS = "address";
            public const String ARTICLE = "article";
            public const String ASIDE = "aside";
            public const String AUDIO = "audio";
            public const String BLOCKQUOTE = "blockquote";
            public const String CANVAS = "canvas";
            public const String FIELDSET = "fieldset";
            public const String FIGCAPTION = "figcaption";
            public const String FIGURE = "figure";
            public const String FOOTER = "footer";
            public const String FONT = "font";
            public const String FORM = "form";
            public const String HEADER = "header";
            public const String HGROUP = "hgroup";
            public const String NOSCRIPT = "noscript";
            public const String OUTPUT = "output";
            public const String SECTION = "section";
            public const String VIDEO = "video";
            public const String BASE = "base";
            public const String COMMAND = "command";
            public const String TITLE = "title";
            public const String A = "a";
            public const String ABBR = "abbr";
            public const String B = "b";
            public const String BDO = "bdo";
            public const String BUTTON = "button";
            public const String DETAILS = "details";
            public const String CODE = "code";
            public const String DEL = "del";
            public const String DATALIST = "datalist";
            public const String DFN = "dfn";
            public const String EMBED = "embed";
            public const String CITE = "cite";
            public const String DL = "dl";
            public const String EM = "em";
            public const String I = "i";
            public const String IFRAME = "iframe";
            public const String INPUT = "input";
            public const String IMG = "img";
            public const String INS = "ins";
            public const String MAP = "map";
            public const String KEYGEN = "keygen";
            public const String METER = "meter";
            public const String MENU = "menu";
            public const String NAV = "nav";
            public const String KBD = "kbd";
            public const String MATH = "math";
            public const String MARK = "mark";
            public const String LABEL = "label";
            public const String Q = "q";
            public const String SAMP = "samp";
            public const String PROGRESS = "progress";
            public const String RUBY = "ruby";
            public const String OBJECT = "object";
            public const String SMALL = "small";
            public const String SUB = "sub";
            public const String SUP = "sup";
            public const String STRONG = "strong";
            public const String SELECT = "select";
            public const String SPAN = "span";
            public const String SVG = "svg";
            public const String WBR = "wbr";
            public const String TIME = "time";
            public const String TEXTAREA = "textarea";
            public const String VAR = "var";
            public const String TR = "tr";
            public const String BIG = "big";
            public const String S = "s";
            public const String STRIKE = "strike";
            public const String TT = "tt";
            public const String U = "u";
        }

        /**
         * Collections that combine different tags.
         *
         */
        public static class Category {

            /**
             * List with the tags that are not visible in the browser.<br />
             * Tag.HTML, Tag.HEAD, Tag.META, Tag.SCRIPT, Tag.LINK, Tag.STYLE,
             * Tag.TITLE
             */
            public static Dictionary<string,object> NOT_VISIBLE = ToDic(new String[] { Tag.HTML,
                    Tag.HEAD, Tag.META, Tag.SCRIPT, Tag.LINK, Tag.STYLE, Tag.TITLE });
            /**
             * Groups all tags that are block level tags by default.<br />
             * Tag.ADDRESS, Tag.ARTICLE, Tag.ASIDE, Tag.AUDIO, Tag.BLOCKQUOTE,
             * Tag.CANVAS, Tag.DD, Tag.DIV, Tag.FIELDSET, Tag.FIGCAPTION,
             * Tag.FIGURE, Tag.FOOTER, Tag.FORM, Tag.H1, Tag.H2, Tag.H3, Tag.H4,
             * Tag.H5, Tag.H6, Tag.HEADER, Tag.HGROUP, Tag.HR, Tag.NOSCRIPT, Tag.OL,
             * Tag.OUTPUT, Tag.P, Tag.PRE, Tag.SECTION, Tag.TABLE, Tag.UL, Tag.VIDEO
             */
            public static Dictionary<string,object> BLOCK_LEVEL = ToDic(new String[] { Tag.ADDRESS,
                    Tag.ARTICLE, Tag.ASIDE, Tag.AUDIO, Tag.BLOCKQUOTE, Tag.CANVAS, Tag.DD, Tag.DIV, Tag.FIELDSET,
                    Tag.FIGCAPTION, Tag.FIGURE, Tag.FOOTER, Tag.FORM, Tag.H1, Tag.H2, Tag.H3, Tag.H4, Tag.H5, Tag.H6,
                    Tag.HEADER, Tag.HGROUP, Tag.HR, Tag.NOSCRIPT, Tag.OL, Tag.OUTPUT, Tag.P, Tag.PRE, Tag.SECTION,
                    Tag.TABLE, Tag.UL, Tag.VIDEO });

            /**
             * HTML5<br />
             * Elements belonging to the <em>metadata content</em> category modify
             * the presentation or the behavior of the rest of the document, set up
             * links to others documents, or convey other <em>out of band</em>
             * information.<br />
             * Tag.BASE, Tag.COMMAND, Tag.LINK, Tag.META, Tag.NOSCRIPT, Tag.STYLE,
             * Tag.TITLE
             */
            public static Dictionary<string,object> METADATA = ToDic(new String[] { Tag.BASE,
                    Tag.COMMAND, Tag.LINK, Tag.META, Tag.NOSCRIPT, Tag.STYLE, Tag.TITLE });

            /**
             * HTML5<br />
             * Elements belonging to the flow content category typically contain
             * text or embedded content.<br />
             * Tag.A, Tag.ABBR, Tag.ADDRESS, Tag.ARTICLE, Tag.ASIDE, Tag.AUDIO,
             * Tag.B, Tag.BDO, Tag.BLOCKQUOTE, Tag.BR, Tag.BUTTON, Tag.CANVAS,
             * Tag.CITE, Tag.CODE, Tag.COMMAND, Tag.DATALIST, Tag.DEL, Tag.DETAILS,
             * Tag.DFN, Tag.DIV, Tag.DL, Tag.EM, Tag.EMBED, Tag.FIELDSET,
             * Tag.FIGURE, Tag.FOOTER, Tag.FORM, Tag.H1, Tag.H2, Tag.H3, Tag.H4,
             * Tag.H5, Tag.H6, Tag.HEADER, Tag.HGROUP, Tag.HR, Tag.I, Tag.IFRAME,
             * Tag.IMG, Tag.INPUT, Tag.INS, Tag.KBD, Tag.KEYGEN, Tag.LABEL, Tag.MAP,
             * Tag.MARK, Tag.MATH, Tag.MENU, Tag.METER, Tag.NAV, Tag.NOSCRIPT,
             * Tag.OBJECT, Tag.OL, Tag.OUTPUT, Tag.P, Tag.PRE, Tag.PROGRESS, Tag.Q,
             * Tag.RUBY, Tag.SAMP, Tag.SCRIPT, Tag.SECTION, Tag.SELECT, Tag.SMALL,
             * Tag.SPAN, Tag.STRONG, Tag.SUB, Tag.SUP, Tag.SVG, Tag.TABLE,
             * Tag.TEXTAREA, Tag.TIME, Tag.UL, Tag.VAR, Tag.VIDEO, Tag.WBR
             */
            public static Dictionary<string,object> FLOW_CONTENT = ToDic(new String[] { Tag.A,
                    Tag.ABBR, Tag.ADDRESS, Tag.ARTICLE, Tag.ASIDE, Tag.AUDIO, Tag.B, Tag.BDO, Tag.BLOCKQUOTE, Tag.BR,
                    Tag.BUTTON, Tag.CANVAS, Tag.CITE, Tag.CODE, Tag.COMMAND, Tag.DATALIST, Tag.DEL, Tag.DETAILS, Tag.DFN,
                    Tag.DIV, Tag.DL, Tag.EM, Tag.EMBED, Tag.FIELDSET, Tag.FIGURE, Tag.FOOTER, Tag.FORM, Tag.H1, Tag.H2,
                    Tag.H3, Tag.H4, Tag.H5, Tag.H6, Tag.HEADER, Tag.HGROUP, Tag.HR, Tag.I, Tag.IFRAME, Tag.IMG, Tag.INPUT,
                    Tag.INS, Tag.KBD, Tag.KEYGEN, Tag.LABEL, Tag.MAP, Tag.MARK, Tag.MATH, Tag.MENU, Tag.METER, Tag.NAV,
                    Tag.NOSCRIPT, Tag.OBJECT, Tag.OL, Tag.OUTPUT, Tag.P, Tag.PRE, Tag.PROGRESS, Tag.Q, Tag.RUBY, Tag.SAMP,
                    Tag.SCRIPT, Tag.SECTION, Tag.SELECT, Tag.SMALL, Tag.SPAN, Tag.FONT, Tag.STRONG, Tag.SUB, Tag.SUP, Tag.SVG,
                    Tag.TABLE, Tag.TEXTAREA, Tag.TIME, Tag.UL, Tag.VAR, Tag.VIDEO, Tag.WBR });
            /**
             * HTML5<br />
             * Elements belonging to the sectioning content model create a <a
             * rel="internal" href=
             * "https://developer.mozilla.org/en/Sections_and_Outlines_of_an_HTML5_document"
             * >section in the current outline</a> that defines the scope of&nbsp;
             * <code><a rel="custom" href="https://developer.mozilla.org/en/HTML/Element/header">&lt;header&gt;</a></code>
             * elements,
             * <code><a rel="custom" href="https://developer.mozilla.org/en/HTML/Element/footer">&lt;footer&gt;</a></code>
             * elements, and <a rel="internal"
             * href="https://developer.mozilla.org/#heading_content">heading
             * content</a>. <footer> elements, and heading content.<br />
             * Tag.ARTICLE, Tag.ASIDE, Tag.NAV, Tag.SECTION
             */
            public static Dictionary<string,object> SECTIONING_CONTENT = ToDic(new String[] {
                    Tag.ARTICLE, Tag.ASIDE, Tag.NAV, Tag.SECTION });
            /**
             * HTML5<br />
             * Heading content defines the title of a section, whether marked by an
             * explicit <a
             * href="https://developer.mozilla.org/#sectioning_content">sectioning
             * content</a> element or implicitly defined by the heading content
             * itself.<br />
             * Tag.H1, Tag.H2, Tag.H3, Tag.H4, Tag.H5, Tag.H6, Tag.HGROUP
             */
            public static Dictionary<string,object> HEADING_CONTENT = ToDic(new String[] {
                    Tag.H1, Tag.H2, Tag.H3, Tag.H4, Tag.H5, Tag.H6, Tag.HGROUP
            });
            /**
             * HTML5<br />
             * Phrasing content defines the text and the mark-up it contains. Runs
             * of phrasing content make up paragraphs.<br />
             * Tag.ABBR, Tag.AUDIO, Tag.B, Tag.BDO, Tag.BR, Tag.BUTTON, Tag.CANVAS,
             * Tag.CITE, Tag.CODE, Tag.COMMAND, Tag.DATALIST, Tag.DFN, Tag.EM,
             * Tag.EMBED, Tag.I, Tag.IFRAME, Tag.IMG, Tag.INPUT, Tag.KBD,
             * Tag.KEYGEN, Tag.LABEL, Tag.MARK, Tag.MATH, Tag.METER, Tag.NOSCRIPT,
             * Tag.OBJECT, Tag.OUTPUT, Tag.PROGRESS, Tag.Q, Tag.RUBY, Tag.SAMP,
             * Tag.SCRIPT, Tag.SELECT, Tag.SMALL, Tag.SPAN, Tag.STRONG, Tag.SUB,
             * Tag.SUP, Tag.SVG, Tag.TEXTAREA, Tag.TIME, Tag.VAR, Tag.VIDEO, Tag.WBR
             */
            public static Dictionary<string,object> PHRASING_CONTENT = ToDic(new String[] {
                    Tag.ABBR, Tag.AUDIO, Tag.B, Tag.BDO, Tag.BR, Tag.BUTTON, Tag.CANVAS, Tag.CITE, Tag.CODE, Tag.COMMAND,
                    Tag.DATALIST, Tag.DFN, Tag.EM, Tag.EMBED, Tag.I, Tag.IFRAME, Tag.IMG, Tag.INPUT, Tag.KBD, Tag.KEYGEN,
                    Tag.LABEL, Tag.MARK, Tag.MATH, Tag.METER, Tag.NOSCRIPT, Tag.OBJECT, Tag.OUTPUT, Tag.PROGRESS, Tag.Q,
                    Tag.RUBY, Tag.SAMP, Tag.SCRIPT, Tag.SELECT, Tag.SMALL, Tag.SPAN, Tag.FONT, Tag.STRONG, Tag.SUB, Tag.SUP, Tag.SVG,
                    Tag.TEXTAREA, Tag.TIME, Tag.VAR, Tag.VIDEO, Tag.WBR
            });
            /**
             * HTML5<br />
             * Embedded content imports another resource or inserts content from
             * another mark-up language or namespace into the document.<br />
             * Tag.AUDIO, Tag.CANVAS, Tag.EMBED, Tag.IFRAME, Tag.IMG, Tag.MATH,
             * Tag.OBJECT, Tag.SVG, Tag.VIDEO
             */
            public static Dictionary<string,object> EMBEDDED_CONTENT = ToDic(new String[] {
                    Tag.AUDIO, Tag.CANVAS, Tag.EMBED, Tag.IFRAME, Tag.IMG, Tag.MATH, Tag.OBJECT, Tag.SVG, Tag.VIDEO
            });

            private static Dictionary<string,object> ToDic(string[] ss) {
                Dictionary<string,object> dic = new Dictionary<string,object>();
                foreach (string s in ss) {
                    dic[s] = null;
                }
                return dic;
            }
        }
        
        /**
         * Attributes used in HTML tags.
         */
        public static class Attribute {

            public const String ALT = "alt";
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
            public const String VALIGN  = "valign";
            public const String ALIGN  = "align";
            public const String FACE  = "face";
            public const String SIZE  = "size";
            public const String COLOR = "color";
            public const String START = "start";
            public const String DIR = "dir";

            /**
             * Possible attribute values.
             *
             * @author itextpdf.com
             *
             */
            public static class Value {
                public const String TEXTCSS = "text/css";
            }
        }
    }
}
