using System;
/*
 * $Id: Tags.java 135 2011-05-30 11:38:45Z redlab_b $
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
     * @author redlab_b
     *
     */
    public class Tags {

        private const string defaultpackage = "iTextSharp.tool.xml.html.";
        private const string dummyTagProcessor = defaultpackage + "DummyTagProcessor";
        private const string headers = defaultpackage + "Header";
        private const string span = defaultpackage + "Span";
        private const string nonSanitized = defaultpackage + "NonSanitizedTag";
        private const string paragraph = defaultpackage + "ParaGraph";

        /**
         * Returns a new {@link DefaultTagProcessorFactory}
         * @return a default XHTML {@link TagProcessorFactory}
         */
        public static ITagProcessorFactory GetHtmlTagProcessorFactory() {
            DefaultTagProcessorFactory factory = new DefaultTagProcessorFactory();
            factory.AddProcessor("xml", dummyTagProcessor);
            factory.AddProcessor("!doctype", dummyTagProcessor);
            factory.AddProcessor("html", dummyTagProcessor);
            factory.AddProcessor("head", dummyTagProcessor);
            factory.AddProcessor("meta", dummyTagProcessor);
            factory.AddProcessor("object", dummyTagProcessor);
            factory.AddProcessor("title", defaultpackage + "head.Title");
            factory.AddProcessor("link", defaultpackage + "head.Link");
            factory.AddProcessor("style", defaultpackage + "head.Style");
            factory.AddProcessor("body", defaultpackage + "Body");
            factory.AddProcessor("div", defaultpackage + "Div");
            factory.AddProcessor("a", defaultpackage + "Anchor");
            factory.AddProcessor("table", defaultpackage + "table.Table");
            factory.AddProcessor("tr", defaultpackage + "table.TableRow");
            factory.AddProcessor("td", defaultpackage + "table.TableData");
            factory.AddProcessor(HTML.Tag.TH, defaultpackage + "table.TableData");
            factory.AddProcessor("caption", paragraph);
            factory.AddProcessor("p", paragraph);
            factory.AddProcessor("dt", paragraph);
            factory.AddProcessor("dd", paragraph);
            factory.AddProcessor("br", defaultpackage + "Break");
            factory.AddProcessor("span", span);
            factory.AddProcessor("small", span);
            factory.AddProcessor("big", span);
            factory.AddProcessor("s", span);
            factory.AddProcessor("strike", span);
            factory.AddProcessor("del", span);
            factory.AddProcessor("sub", span);
            factory.AddProcessor("sup", span);
            factory.AddProcessor("b", span);
            factory.AddProcessor("strong", span);
            factory.AddProcessor("i", span);
            factory.AddProcessor("cite", span);
            factory.AddProcessor("em", span);
            factory.AddProcessor("address", span);
            factory.AddProcessor("dfn", span);
            factory.AddProcessor("var", span);
            factory.AddProcessor("pre", nonSanitized);
            factory.AddProcessor("tt", nonSanitized);
            factory.AddProcessor("code", nonSanitized);
            factory.AddProcessor("kbd", nonSanitized);
            factory.AddProcessor("samp", nonSanitized);
            factory.AddProcessor("u", span);
            factory.AddProcessor("ins", span);
            factory.AddProcessor("img", defaultpackage + "Image");
            factory.AddProcessor("ul", defaultpackage + "OrderedUnorderedList");
            factory.AddProcessor("ol", defaultpackage + "OrderedUnorderedList");
            factory.AddProcessor("li", defaultpackage + "OrderedUnorderedListItem");
            factory.AddProcessor("h1", headers);
            factory.AddProcessor("h2", headers);
            factory.AddProcessor("h3", headers);
            factory.AddProcessor("h4", headers);
            factory.AddProcessor("h5", headers);
            factory.AddProcessor("h6", headers);
            factory.AddProcessor("hr", defaultpackage + "HorizontalRule");
            return factory;
        }
    }
}