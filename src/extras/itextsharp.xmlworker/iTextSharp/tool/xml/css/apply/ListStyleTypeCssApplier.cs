using System;
using System.Collections.Generic;
using System.IO;
using System.util;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.net;
using iTextSharp.tool.xml.net.exc;
using iTextSharp.tool.xml.pipeline.html;
/*
 * $Id: ListStyleTypeCssApplier.java 165 2011-06-07 14:22:09Z emielackermann $
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
namespace iTextSharp.tool.xml.css.apply {

    /**
     * @author itextpdf.com
     *
     */
    public class ListStyleTypeCssApplier {

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
        public List Apply(List list, Tag t, HtmlPipelineContext htmlPipelineContext) {
            float fontSize = FontSizeTranslator.GetInstance().GetFontSize(t);
            List lst = list;
            IDictionary<String, String> css = t.CSS;
            String styleType;
            css.TryGetValue(CSS.Property.LIST_STYLE_TYPE, out styleType);
            if (null != styleType) {
                if (Util.EqualsIgnoreCase(styleType, CSS.Value.NONE)) {
                    lst.Lettered = false;
                    lst.Numbered = false;
                    lst.SetListSymbol("");
                } else if (Util.EqualsIgnoreCase(CSS.Value.DECIMAL, styleType)) {
                    lst = new List(List.ORDERED);
                    SynchronizeSymbol(fontSize, lst);
                } else if (Util.EqualsIgnoreCase(CSS.Value.DISC, styleType)) {
                    lst = new ZapfDingbatsList(108);
                    ShrinkSymbol(lst, fontSize);
                } else if (Util.EqualsIgnoreCase(CSS.Value.SQUARE, styleType)) {
                    lst = new ZapfDingbatsList(110);
                    ShrinkSymbol(lst, fontSize);
                } else if (Util.EqualsIgnoreCase(CSS.Value.CIRCLE, styleType)) {
                    lst = new ZapfDingbatsList(109);
                    ShrinkSymbol(lst, fontSize);
                } else if (CSS.Value.LOWER_ROMAN.Equals(styleType)) {
                    lst = new RomanList(true, 0);
                    SynchronizeSymbol(fontSize, lst);
                } else if (CSS.Value.UPPER_ROMAN.Equals(styleType)) {
                    lst = new RomanList(false, 0);
                    SynchronizeSymbol(fontSize, lst);
                } else if (CSS.Value.LOWER_GREEK.Equals(styleType)) {
                    lst = new GreekList(true, 0);
                    SynchronizeSymbol(fontSize, lst);
                } else if (CSS.Value.UPPER_GREEK.Equals(styleType)) {
                    lst = new GreekList(false, 0);
                    SynchronizeSymbol(fontSize, lst);
                } else if (CSS.Value.LOWER_ALPHA.Equals(styleType) || CSS.Value.LOWER_LATIN.Equals(styleType)) {
                    lst = new List(List.ORDERED, List.ALPHABETICAL);
                    SynchronizeSymbol(fontSize, lst);
                    lst.Lowercase = true;
                } else if (CSS.Value.UPPER_ALPHA.Equals(styleType) || CSS.Value.UPPER_LATIN.Equals(styleType)) {
                    lst = new List(List.ORDERED, List.ALPHABETICAL);
                    SynchronizeSymbol(fontSize, lst);
                    lst.Lowercase = false;
                }
            } else if (Util.EqualsIgnoreCase(t.Name, HTML.Tag.OL)) {
                lst = new List(List.ORDERED);
                SynchronizeSymbol(fontSize, lst);
            } else if (Util.EqualsIgnoreCase(t.Name, HTML.Tag.UL)) {
                lst = new List(List.UNORDERED);
                ShrinkSymbol(lst, fontSize);
            }
            if (css.ContainsKey(CSS.Property.LIST_STYLE_IMAGE)
                    && !Util.EqualsIgnoreCase(css[CSS.Property.LIST_STYLE_IMAGE], CSS.Value.NONE)) {
                lst = new List();
                String url = utils.ExtractUrl(css[CSS.Property.LIST_STYLE_IMAGE]);
                iTextSharp.text.Image img = null;
                try {
                    if (htmlPipelineContext == null) {
                        img = new ImageRetrieve().RetrieveImage(url);
                    } else {
                        try {
                            img = new ImageRetrieve(htmlPipelineContext.GetImageProvider()).RetrieveImage(url);
                        } catch (NoImageProviderException) {
                            if (LOG.IsLogging(Level.TRACE)) {
                                LOG.Trace(String.Format(LocaleMessages.GetInstance().GetMessage("pipeline.html.noimageprovider"), htmlPipelineContext.GetType().FullName));
                            }
                            img = new ImageRetrieve().RetrieveImage(url);
                        }
                    }
                    lst.ListSymbol = new Chunk(img, 0, 0, false);
                    lst.SymbolIndent = img.Width;
                    if (LOG.IsLogging(Level.TRACE)) {
                        LOG.Trace(String.Format(LocaleMessages.GetInstance().GetMessage("html.tag.list"), url));
                    }
                } catch (IOException e) {
                    if (LOG.IsLogging(Level.ERROR)) {
                        LOG.Error(String.Format(LocaleMessages.GetInstance().GetMessage("html.tag.list.failed"), url), e);
                    }
                    lst = new List(List.UNORDERED);
                } catch (NoImageException e) {
                    if (LOG.IsLogging(Level.ERROR)) {
                        LOG.Error(e.Message, e);
                    }
                    lst = new List(List.UNORDERED);
                }
            }
            lst.Alignindent = false;
            lst.Autoindent = false;
            float leftIndent = 0;
            if (css.ContainsKey(CSS.Property.LIST_STYLE_POSITION) && Util.EqualsIgnoreCase(css[CSS.Property.LIST_STYLE_POSITION], CSS.Value.INSIDE)) {
                leftIndent += 30;
            } else {
                leftIndent += 15;
            }
            leftIndent += css.ContainsKey(CSS.Property.MARGIN_LEFT)?utils.ParseValueToPt(css[CSS.Property.MARGIN_LEFT],fontSize):0;
            leftIndent += css.ContainsKey(CSS.Property.PADDING_LEFT)?utils.ParseValueToPt(css[CSS.Property.PADDING_LEFT],fontSize):0;
            lst.IndentationLeft = leftIndent;
            return lst;
        }

        private void SynchronizeSymbol(float fontSize, List lst) {
            lst.Symbol.Font.Size = fontSize;
            lst.SymbolIndent = fontSize;
        }

        private void ShrinkSymbol(List lst, float fontSize) {
            lst.SymbolIndent = 12;
            Chunk symbol = lst.Symbol;
            symbol.SetTextRise(2);
            symbol.Font.Size = 7;
        }
    }
}