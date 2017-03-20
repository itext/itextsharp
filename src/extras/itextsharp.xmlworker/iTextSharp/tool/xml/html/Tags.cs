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
            factory.AddProcessor(HTML.Tag.XML, dummyTagProcessor);
            factory.AddProcessor("!doctype", dummyTagProcessor);
            factory.AddProcessor(HTML.Tag.HTML, dummyTagProcessor);
            factory.AddProcessor(HTML.Tag.HEAD, dummyTagProcessor);
            factory.AddProcessor(HTML.Tag.META, dummyTagProcessor);
            factory.AddProcessor(HTML.Tag.OBJECT, dummyTagProcessor);
            factory.AddProcessor(HTML.Tag.TITLE, defaultpackage + "head.Title");
            factory.AddProcessor(HTML.Tag.LINK, defaultpackage + "head.Link");
            factory.AddProcessor(HTML.Tag.STYLE, defaultpackage + "head.Style");
            factory.AddProcessor(HTML.Tag.BODY, defaultpackage + "Body");
            factory.AddProcessor(HTML.Tag.DIV, defaultpackage + "Div");
            factory.AddProcessor(HTML.Tag.A, defaultpackage + "Anchor");
            factory.AddProcessor(HTML.Tag.TABLE, defaultpackage + "table.Table");
            factory.AddProcessor(HTML.Tag.TR, defaultpackage + "table.TableRow");
            factory.AddProcessor(HTML.Tag.TD, defaultpackage + "table.TableData");
            factory.AddProcessor(HTML.Tag.TH, defaultpackage + "table.TableData");
            factory.AddProcessor(HTML.Tag.CAPTION, paragraph);
            factory.AddProcessor(HTML.Tag.P, paragraph);
            factory.AddProcessor(HTML.Tag.DT, paragraph);
            factory.AddProcessor(HTML.Tag.DD, paragraph);
            factory.AddProcessor(HTML.Tag.BLOCKQUOTE, paragraph);
            factory.AddProcessor(HTML.Tag.BR, defaultpackage + "Break");
            factory.AddProcessor(HTML.Tag.SPAN, span);
            factory.AddProcessor(HTML.Tag.SMALL, span);
            factory.AddProcessor(HTML.Tag.BIG, span);
            factory.AddProcessor(HTML.Tag.S, span);
            factory.AddProcessor(HTML.Tag.STRIKE, span);
            factory.AddProcessor(HTML.Tag.DEL, span);
            factory.AddProcessor(HTML.Tag.SUB, span);
            factory.AddProcessor(HTML.Tag.SUP, span);
            factory.AddProcessor(HTML.Tag.B, span);
            factory.AddProcessor(HTML.Tag.STRONG, span);
            factory.AddProcessor(HTML.Tag.FONT, span);
            factory.AddProcessor(HTML.Tag.I, span);
            factory.AddProcessor(HTML.Tag.CITE, span);
            factory.AddProcessor(HTML.Tag.EM, span);
            factory.AddProcessor(HTML.Tag.ADDRESS, span);
            factory.AddProcessor(HTML.Tag.DFN, span);
            factory.AddProcessor(HTML.Tag.VAR, span);
            factory.AddProcessor(HTML.Tag.PRE, nonSanitized);
            factory.AddProcessor(HTML.Tag.TT, nonSanitized);
            factory.AddProcessor(HTML.Tag.CODE, nonSanitized);
            factory.AddProcessor(HTML.Tag.KBD, nonSanitized);
            factory.AddProcessor(HTML.Tag.SAMP, nonSanitized);
            factory.AddProcessor(HTML.Tag.U, span);
            factory.AddProcessor(HTML.Tag.INS, span);
            factory.AddProcessor(HTML.Tag.IMG, defaultpackage + "Image");
            factory.AddProcessor(HTML.Tag.UL, defaultpackage + "OrderedUnorderedList");
            factory.AddProcessor(HTML.Tag.OL, defaultpackage + "OrderedUnorderedList");
            factory.AddProcessor(HTML.Tag.LI, defaultpackage + "OrderedUnorderedListItem");
            factory.AddProcessor(HTML.Tag.H1, headers);
            factory.AddProcessor(HTML.Tag.H2, headers);
            factory.AddProcessor(HTML.Tag.H3, headers);
            factory.AddProcessor(HTML.Tag.H4, headers);
            factory.AddProcessor(HTML.Tag.H5, headers);
            factory.AddProcessor(HTML.Tag.H6, headers);
            factory.AddProcessor(HTML.Tag.HR, defaultpackage + "HorizontalRule");
            //added by Jeroen Nouws
            factory.AddProcessor(HTML.Tag.LABEL, span);
            return factory;
        }
    }
}
