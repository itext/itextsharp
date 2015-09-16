using System;
using System.Collections.Generic;
using iTextSharp.text.html;
using System.util;
/*
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
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

namespace iTextSharp.text.html.simpleparser {

    /**
     * This class maps tags such as div and span to their corresponding
     * TagProcessor classes.
     * @deprecated since 5.5.2
     */
    [Obsolete]
    public class HTMLTagProcessors : Dictionary<String, IHTMLTagProcessor> {

        /**
         * Creates a Map containing supported tags.
         */
        public HTMLTagProcessors() {
           this[HtmlTags.A] = A;
           this[HtmlTags.B] = EM_STRONG_STRIKE_SUP_SUP;
           this[HtmlTags.BODY] = DIV;
           this[HtmlTags.BR] = BR;
           this[HtmlTags.DIV] = DIV;
           this[HtmlTags.EM] = EM_STRONG_STRIKE_SUP_SUP;
           this[HtmlTags.FONT] = SPAN;
           this[HtmlTags.H1] = H;
           this[HtmlTags.H2] = H;
           this[HtmlTags.H3] = H;
           this[HtmlTags.H4] = H;
           this[HtmlTags.H5] = H;
           this[HtmlTags.H6] = H;
           this[HtmlTags.HR] = HR;
           this[HtmlTags.I] = EM_STRONG_STRIKE_SUP_SUP;
           this[HtmlTags.IMG] = IMG;
           this[HtmlTags.LI] = LI;
           this[HtmlTags.OL] = UL_OL;
           this[HtmlTags.P] = DIV;
           this[HtmlTags.PRE] = PRE;
           this[HtmlTags.S] = EM_STRONG_STRIKE_SUP_SUP;
           this[HtmlTags.SPAN] = SPAN;
           this[HtmlTags.STRIKE] = EM_STRONG_STRIKE_SUP_SUP;
           this[HtmlTags.STRONG] = EM_STRONG_STRIKE_SUP_SUP;
           this[HtmlTags.SUB] = EM_STRONG_STRIKE_SUP_SUP;
           this[HtmlTags.SUP] = EM_STRONG_STRIKE_SUP_SUP;
           this[HtmlTags.TABLE] = TABLE;
           this[HtmlTags.TD] = TD;
           this[HtmlTags.TH] = TD;
           this[HtmlTags.TR] = TR;
           this[HtmlTags.U] = EM_STRONG_STRIKE_SUP_SUP;
           this[HtmlTags.UL] = UL_OL;
        }

        /**
         * Object that processes the following tags:
         * i, em, b, strong, s, strike, u, sup, sub
         */
        public static IHTMLTagProcessor EM_STRONG_STRIKE_SUP_SUP = new HTMLTagProcessor_EM_STRONG_STRIKE_SUP_SUP();
        
        private class HTMLTagProcessor_EM_STRONG_STRIKE_SUP_SUP : IHTMLTagProcessor {
            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#startElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String)
             */
            virtual public void StartElement(HTMLWorker worker, String tag, IDictionary<String, String> attrs) {
                tag = MapTag(tag);
                attrs[tag] = null;
                worker.UpdateChain(tag, attrs);
            }
            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#endElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String)
             */
            virtual public void EndElement(HTMLWorker worker, String tag) {
                tag = MapTag(tag);
                worker.UpdateChain(tag);
            }
            /**
             * Maps em to i, strong to b, and strike to s.
             * This is a convention: the style parser expects i, b and s.
             * @param tag the original tag
             * @return the mapped tag
             */
            private String MapTag(String tag) {
                if (Util.EqualsIgnoreCase(HtmlTags.EM, tag))
                    return HtmlTags.I;
                if (Util.EqualsIgnoreCase(HtmlTags.STRONG, tag))
                    return HtmlTags.B;
                if (Util.EqualsIgnoreCase(HtmlTags.STRIKE, tag))
                    return HtmlTags.S;
                return tag;
            }

        }

        /**
         * Object that processes the a tag.
         */
        public static IHTMLTagProcessor A = new HTMLTagProcessor_A();

        private class HTMLTagProcessor_A : IHTMLTagProcessor {

            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#startElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String)
             */
            virtual public void StartElement(HTMLWorker worker, String tag, IDictionary<String, String> attrs) {
                worker.UpdateChain(tag, attrs);
                worker.FlushContent();
            }
            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#endElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String)
             */
            virtual public void EndElement(HTMLWorker worker, String tag) {
                worker.ProcessLink();
                worker.UpdateChain(tag);
            }
        }

        /**
         * Object that processes the br tag.
         */
        public static IHTMLTagProcessor BR = new HTMLTagProcessor_BR();
        
        private class HTMLTagProcessor_BR : IHTMLTagProcessor {
            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#startElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String, java.util.Map)
             */
            virtual public void StartElement(HTMLWorker worker, String tag, IDictionary<String, String> attrs) {
                worker.NewLine();
            }
            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#endElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String)
             */
            virtual public void EndElement(HTMLWorker worker, String tag) {
            }

        }

        public static IHTMLTagProcessor UL_OL = new HTMLTagProcessor_UL_OL();

        private class HTMLTagProcessor_UL_OL : IHTMLTagProcessor {

            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#startElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String, java.util.Map)
             */
            virtual public void StartElement(HTMLWorker worker, String tag, IDictionary<String, String> attrs) {
                worker.CarriageReturn();
                if (worker.IsPendingLI())
                    worker.EndElement(HtmlTags.LI);
                worker.SetSkipText(true);
                worker.UpdateChain(tag, attrs);;
                worker.PushToStack(worker.CreateList(tag));
            }

            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#endElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String)
             */
            virtual public void EndElement(HTMLWorker worker, String tag) {
                worker.CarriageReturn();
                if (worker.IsPendingLI())
                    worker.EndElement(HtmlTags.LI);
                worker.SetSkipText(false);
                worker.UpdateChain(tag);
                worker.ProcessList();
            }

        }

        public static IHTMLTagProcessor HR = new HTMLTagProcessor_HR();

        private class HTMLTagProcessor_HR : IHTMLTagProcessor {

            virtual public void StartElement(HTMLWorker worker, String tag, IDictionary<String, String> attrs) {
                worker.CarriageReturn();
                worker.PushToStack(worker.CreateLineSeparator(attrs));
            }

            virtual public void EndElement(HTMLWorker worker, String tag) {
            }

        }

        public static IHTMLTagProcessor SPAN = new HTMLTagProcessor_SPAN();

        private class HTMLTagProcessor_SPAN : IHTMLTagProcessor {

            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#startElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String, java.util.Map)
             */
            virtual public void StartElement(HTMLWorker worker, String tag, IDictionary<String, String> attrs) {
                worker.UpdateChain(tag, attrs);
            }

            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#endElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String)
             */
            virtual public void EndElement(HTMLWorker worker, String tag) {
                worker.UpdateChain(tag);
            }

        }

        public static IHTMLTagProcessor H = new HTMLTagProcessor_H();

        private class HTMLTagProcessor_H : IHTMLTagProcessor {
            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#startElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String, java.util.Map)
             */
            virtual public void StartElement(HTMLWorker worker, String tag, IDictionary<String, String> attrs) {
                worker.CarriageReturn();
                if (!attrs.ContainsKey(HtmlTags.SIZE)) {
                    int v = 7 - int.Parse(tag.Substring(1));
                    attrs[HtmlTags.SIZE] = v.ToString();
                }
                worker.UpdateChain(tag, attrs);
            }

            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#endElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String)
             */
            virtual public void EndElement(HTMLWorker worker, String tag) {
                worker.CarriageReturn();
                worker.UpdateChain(tag);
            }

        }

        public static IHTMLTagProcessor LI = new HTMLTagProcessor_LI();

        private class HTMLTagProcessor_LI : IHTMLTagProcessor {

            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#startElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String, java.util.Map)
             */
            virtual public void StartElement(HTMLWorker worker, String tag, IDictionary<String, String> attrs) {
                worker.CarriageReturn();
                if (worker.IsPendingLI())
                    worker.EndElement(tag);
                worker.SetSkipText(false);
                worker.SetPendingLI(true);
                worker.UpdateChain(tag, attrs);
                worker.PushToStack(worker.CreateListItem());
            }

            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#endElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String)
             */
            virtual public void EndElement(HTMLWorker worker, String tag) {
                worker.CarriageReturn();
                worker.SetPendingLI(false);
                worker.SetSkipText(true);
                worker.UpdateChain(tag);
                worker.ProcessListItem();
            }

        }

        public static IHTMLTagProcessor PRE = new HTMLTagProcessor_PRE();

        private class HTMLTagProcessor_PRE : IHTMLTagProcessor {

            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#startElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String, java.util.Map)
             */
            virtual public void StartElement(HTMLWorker worker, String tag, IDictionary<String, String> attrs) {
                worker.CarriageReturn();
                if (!attrs.ContainsKey(HtmlTags.FACE)) {
                    attrs[HtmlTags.FACE] = "Courier";
                }
                worker.UpdateChain(tag, attrs);
                worker.SetInsidePRE(true);
            }

            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#endElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String)
             */
            virtual public void EndElement(HTMLWorker worker, String tag) {
                worker.CarriageReturn();
                worker.UpdateChain(tag);
                worker.SetInsidePRE(false);
            }

        }

        public static IHTMLTagProcessor DIV = new HTMLTagProcessor_DIV();

        private class HTMLTagProcessor_DIV : IHTMLTagProcessor {

            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#startElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String, java.util.Map)
             */
            virtual public void StartElement(HTMLWorker worker, String tag, IDictionary<String, String> attrs) {
                worker.CarriageReturn();
                worker.UpdateChain(tag, attrs);
            }

            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#endElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String)
             */
            virtual public void EndElement(HTMLWorker worker, String tag) {
                worker.CarriageReturn();
                worker.UpdateChain(tag);
            }

        }


        public static IHTMLTagProcessor TABLE = new HTMLTagProcessor_TABLE();
      
        private class HTMLTagProcessor_TABLE : IHTMLTagProcessor {

            /**
             * @throws DocumentException
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#startElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String, java.util.Map)
             */
            virtual public void StartElement(HTMLWorker worker, String tag, IDictionary<String, String> attrs) {
                worker.CarriageReturn();
                TableWrapper table = new TableWrapper(attrs);
                worker.PushToStack(table);
                worker.PushTableState();
                worker.SetPendingTD(false);
                worker.SetPendingTR(false);
                worker.SetSkipText(true);
                // Table alignment should not affect children elements, thus remove
                attrs.Remove(HtmlTags.ALIGN);
                // In case this is a nested table reset colspan and rowspan
			    attrs[HtmlTags.COLSPAN] = "1";
			    attrs[HtmlTags.ROWSPAN] = "1";
                worker.UpdateChain(tag, attrs);
            }

            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#endElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String)
             */
            virtual public void EndElement(HTMLWorker worker, String tag) {
                worker.CarriageReturn();
                if (worker.IsPendingTR())
                    worker.EndElement(HtmlTags.TR);
                worker.UpdateChain(tag);
                worker.ProcessTable();
                worker.PopTableState();
                worker.SetSkipText(false);
            }

        }

        public static IHTMLTagProcessor TR = new HTMLTagProcessor_TR();

        private class HTMLTagProcessor_TR : IHTMLTagProcessor {

            /**
             * @throws DocumentException
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#startElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String, java.util.Map)
             */
            virtual public void StartElement(HTMLWorker worker, String tag, IDictionary<String, String> attrs) {
                worker.CarriageReturn();
                if (worker.IsPendingTR())
                    worker.EndElement(tag);
                worker.SetSkipText(true);
                worker.SetPendingTR(true);
                worker.UpdateChain(tag, attrs);
            }

            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#endElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String)
             */
            virtual public void EndElement(HTMLWorker worker, String tag) {
                worker.CarriageReturn();
                if (worker.IsPendingTD())
                    worker.EndElement(HtmlTags.TD);
                worker.SetPendingTR(false);
                worker.UpdateChain(tag);
                worker.ProcessRow();
                worker.SetSkipText(true);
            }

        }

        public static IHTMLTagProcessor TD = new HTMLTagProcessor_TD();

        private class HTMLTagProcessor_TD : IHTMLTagProcessor {

            /**
             * @throws DocumentException
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#startElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String, java.util.Map)
             */
            virtual public void StartElement(HTMLWorker worker, String tag, IDictionary<String, String> attrs) {
                worker.CarriageReturn();
                if (worker.IsPendingTD())
                    worker.EndElement(tag);
                worker.SetSkipText(false);
                worker.SetPendingTD(true);
                worker.UpdateChain(HtmlTags.TD, attrs);
                worker.PushToStack(worker.CreateCell(tag));
            }

            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#endElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String)
             */
            virtual public void EndElement(HTMLWorker worker, String tag) {
                worker.CarriageReturn();
                worker.SetPendingTD(false);
                worker.UpdateChain(HtmlTags.TD);
                worker.SetSkipText(true);
            }

        }

        public static IHTMLTagProcessor IMG = new HTMLTagProcessor_IMG();

        private class HTMLTagProcessor_IMG : IHTMLTagProcessor {

            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#startElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String, java.util.Map)
             */
            virtual public void StartElement(HTMLWorker worker, String tag, IDictionary<String, String> attrs) {
                worker.UpdateChain(tag, attrs);
                worker.ProcessImage(worker.CreateImage(attrs), attrs);
                worker.UpdateChain(tag);
            }

            /**
             * @see com.itextpdf.text.html.simpleparser.HTMLTagProcessors#endElement(com.itextpdf.text.html.simpleparser.HTMLWorker, java.lang.String)
             */
            virtual public void EndElement(HTMLWorker worker, String tag) {
            }

        }
    }
}
