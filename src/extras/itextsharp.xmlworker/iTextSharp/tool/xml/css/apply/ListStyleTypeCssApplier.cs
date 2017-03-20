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
using System.IO;
using System.util;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.text.html;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.net;
using iTextSharp.tool.xml.net.exc;
using iTextSharp.tool.xml.pipeline.html;
using Image = iTextSharp.text.Image;

namespace iTextSharp.tool.xml.css.apply {

    /**
     * @author itextpdf.com
     *
     */
    public class ListStyleTypeCssApplier : CssApplier<List> {

        private CssUtils utils = CssUtils.GetInstance();
        private static ILogger LOG = LoggerFactory.GetLogger(typeof(ListStyleTypeCssApplier));

        /**
         *
         */
        public ListStyleTypeCssApplier() {
        }

        /**
         * The ListCssApplier has the capabilities to change the type of the given {@link List} dependable on the css.
         * This means: <strong>Always replace your list with the returned one and add content to the list after applying!</strong>
         */
        // not implemented: list-style-type:armenian, georgian, decimal-leading-zero.
        public virtual List Apply(List list, Tag t, HtmlPipelineContext context) {
            return (List) Apply(list, t, null, null, context);
        }

        public override List Apply(List lst, Tag t, IMarginMemory configuration, IPageSizeContainable psc, HtmlPipelineContext context) {
            float fontSize = FontSizeTranslator.GetInstance().GetFontSize(t);
            IDictionary<String, String> css = t.CSS;
            String styleType;
            css.TryGetValue(CSS.Property.LIST_STYLE_TYPE, out styleType);
            BaseColor color = HtmlUtilities.DecodeColor(css.ContainsKey(CSS.Property.COLOR) ? css[CSS.Property.COLOR] : null);
            if (null == color) color = BaseColor.BLACK;

            if (null != styleType) {
                if (Util.EqualsIgnoreCase(styleType, CSS.Value.NONE)) {
                    lst.Lettered = false;
                    lst.Numbered = false;
                    lst.SetListSymbol("");
                } else if (Util.EqualsIgnoreCase(CSS.Value.DECIMAL, styleType)) {
                    lst = new List(List.ORDERED);
                } else if (Util.EqualsIgnoreCase(CSS.Value.DISC, styleType)) {
                    lst = new ZapfDingbatsList(108);
                    lst.Autoindent = false;
                    lst.SymbolIndent = 7.75f;
                    Chunk symbol = lst.Symbol;
                    symbol.SetTextRise(1.5f);
                    Font font = symbol.Font;
                    font.Size = 4.5f;
                    font.Color = color;
                } else if (Util.EqualsIgnoreCase(CSS.Value.SQUARE, styleType)) {
                    lst = new ZapfDingbatsList(110);
                    ShrinkSymbol(lst, fontSize, color);
                } else if (Util.EqualsIgnoreCase(CSS.Value.CIRCLE, styleType)) {
                    lst = new ZapfDingbatsList(109);
                    lst.Autoindent = false;
                    lst.SymbolIndent = 7.75f;
                    Chunk symbol = lst.Symbol;
                    symbol.SetTextRise(1.5f);
                    Font font = symbol.Font;
                    font.Size = 4.5f;
                    font.Color = color;
                } else if (CSS.Value.LOWER_ROMAN.Equals(styleType)) {
                    lst = new RomanList(true, 0);
                    lst.Autoindent = true;
                    SynchronizeSymbol(fontSize, lst, color);
                } else if (CSS.Value.UPPER_ROMAN.Equals(styleType)) {
                    lst = new RomanList(false, 0);
                    SynchronizeSymbol(fontSize, lst, color);
                    lst.Autoindent = true;
                } else if (CSS.Value.LOWER_GREEK.Equals(styleType)) {
                    lst = new GreekList(true, 0);
                    SynchronizeSymbol(fontSize, lst, color);
                    lst.Autoindent = true;
                } else if (CSS.Value.UPPER_GREEK.Equals(styleType)) {
                    lst = new GreekList(false, 0);
                    SynchronizeSymbol(fontSize, lst, color);
                    lst.Autoindent = true;
                } else if (CSS.Value.LOWER_ALPHA.Equals(styleType) || CSS.Value.LOWER_LATIN.Equals(styleType)) {
                    lst = new List(List.ORDERED, List.ALPHABETICAL);
                    SynchronizeSymbol(fontSize, lst, color);
                    lst.Lowercase = true;
                    lst.Autoindent = true;
                } else if (CSS.Value.UPPER_ALPHA.Equals(styleType) || CSS.Value.UPPER_LATIN.Equals(styleType)) {
                    lst = new List(List.ORDERED, List.ALPHABETICAL);
                    SynchronizeSymbol(fontSize, lst, color);
                    lst.Lowercase = false;
                    lst.Autoindent = true;
                }
            } else if (Util.EqualsIgnoreCase(t.Name, HTML.Tag.OL)) {
                lst = new List(List.ORDERED);
                 String type = null;
                 t.Attributes.TryGetValue("type", out type);
 		         if (type != null) {
                   if (type.Equals("A")) {
 	                     lst.Lettered = true;
 	                    } else if (type.Equals("a")) {
 		                 lst.Lettered = true;
 	                     lst.Lowercase = true;
 		                }
 	               }
                SynchronizeSymbol(fontSize, lst, color);
                lst.Autoindent = true;
            } else if (Util.EqualsIgnoreCase(t.Name, HTML.Tag.UL)) {
                lst = new List(List.UNORDERED);
                ShrinkSymbol(lst, fontSize, color);
            }
            if (css.ContainsKey(CSS.Property.LIST_STYLE_IMAGE)
                    && !Util.EqualsIgnoreCase(css[CSS.Property.LIST_STYLE_IMAGE], CSS.Value.NONE)) {
                lst = new List();
                String url = utils.ExtractUrl(css[CSS.Property.LIST_STYLE_IMAGE]);
                try {
                    Image img = new ImageRetrieve(context.ResourcePath, context.GetImageProvider()).RetrieveImage(url);
                    lst.ListSymbol = new Chunk(img, 0, 0, false);
                    lst.SymbolIndent = img.Width;
                    if (LOG.IsLogging(Level.TRACE)) {
                        LOG.Trace(String.Format(LocaleMessages.GetInstance().GetMessage("html.tag.list"), url));
                    }
                } catch (NoImageException e) {
                    if (LOG.IsLogging(Level.ERROR)) {
                        LOG.Error(String.Format(LocaleMessages.GetInstance().GetMessage("html.tag.img.failed"), url), e);
                    }
                    lst = new List(List.UNORDERED);
                }
                lst.Autoindent = false;
            }
            lst.Alignindent = false;
            float leftIndent = 0;
            if (css.ContainsKey(CSS.Property.LIST_STYLE_POSITION) && Util.EqualsIgnoreCase(css[CSS.Property.LIST_STYLE_POSITION], CSS.Value.INSIDE)) {
                leftIndent += 30;
            } else {
                leftIndent += 15;
            }
            leftIndent += css.ContainsKey(CSS.Property.MARGIN_LEFT)?utils.ParseValueToPt(css[CSS.Property.MARGIN_LEFT],fontSize):0;
            leftIndent += css.ContainsKey(CSS.Property.PADDING_LEFT)?utils.ParseValueToPt(css[CSS.Property.PADDING_LEFT],fontSize):0;
            lst.IndentationLeft = leftIndent;
            String startAtr = null;
            t.Attributes.TryGetValue(HTML.Attribute.START, out startAtr);
            if (startAtr != null) {
                try {
                    int start = int.Parse(startAtr);
                    lst.First = start;
                } catch (FormatException exc) {
                }
            }
            return lst;
        }

        private void SynchronizeSymbol(float fontSize, List lst, BaseColor color) {
            Font font = lst.Symbol.Font;
            font.Size = fontSize;
            font.Color = color;
            lst.SymbolIndent = fontSize;
        }

        private void ShrinkSymbol(List lst, float fontSize, BaseColor color) {
            lst.SymbolIndent = 12;
            Chunk symbol = lst.Symbol;
            //symbol.SetTextRise(2);
            Font font = symbol.Font;
            font.Size = fontSize;
            font.Color = color;
        }
    }
}
