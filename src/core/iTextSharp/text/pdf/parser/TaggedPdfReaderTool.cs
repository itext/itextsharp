using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using iTextSharp.text.error_messages;
using iTextSharp.text.xml;
/*
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, et al.
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
 * The interactive user interfaces in modified source and obj code versions
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
namespace iTextSharp.text.pdf.parser {

    /**
     * Converts a tagged PDF document into an XML file.
     * 
     * @since 5.0.2
     */
    public class TaggedPdfReaderTool {

        /** The reader obj from which the content streams are read. */
        internal protected PdfReader reader;
        /** The writer obj to which the XML will be written */
        internal protected StreamWriter outp;

        /**
         * Parses a string with structured content.
         * 
         * @param reader
         *            the PdfReader that has access to the PDF file
         * @param os
         *            the Stream to which the resulting xml will be written
         * @param charset
         *            the charset to encode the data
         * @since 5.0.5
         */
        public virtual void ConvertToXml(PdfReader reader, Stream os, Encoding encoding) {
            this.reader = reader;
            outp = new StreamWriter(os, encoding);
            // get the StructTreeRoot from the root obj
            PdfDictionary catalog = reader.Catalog;
            PdfDictionary struc = catalog.GetAsDict(PdfName.STRUCTTREEROOT);
            if (struc == null)
                throw new IOException(MessageLocalization.GetComposedMessage("no.structtreeroot.found"));
            // Inspect the child or children of the StructTreeRoot
            InspectChild(struc.GetDirectObject(PdfName.K));
            outp.Flush();
            outp.Close();
        }

        /**
         * Parses a string with structured content.
         * 
         * @param reader
         *            the PdfReader that has access to the PDF file
         * @param os
         *            the Stream to which the resulting xml will be written
         */
        virtual public void ConvertToXml(PdfReader reader, Stream os) {
            ConvertToXml(reader, os, Encoding.UTF8);
        }

        /**
         * Inspects a child of a structured element. This can be an array or a
         * dictionary.
         * 
         * @param k
         *            the child to inspect
         * @throws IOException
         */
        virtual public void InspectChild(PdfObject k) {
            if (k == null)
                return;
            if (k is PdfArray)
                InspectChildArray((PdfArray) k);
            else if (k is PdfDictionary)
                InspectChildDictionary((PdfDictionary) k);
        }

        /**
         * If the child of a structured element is an array, we need to loop over
         * the elements.
         * 
         * @param k
         *            the child array to inspect
         */
        virtual public void InspectChildArray(PdfArray k) {
            if (k == null)
                return;
            for (int i = 0; i < k.Size; i++) {
                InspectChild(k.GetDirectObject(i));
            }
        }

            /**
         * If the child of a structured element is a dictionary, we inspect the
         * child; we may also draw a tag.
         *
         * @param k
         *            the child dictionary to inspect
         */
        public virtual void InspectChildDictionary(PdfDictionary k){
            InspectChildDictionary(k, false);
        }

        /**
         * If the child of a structured element is a dictionary, we inspect the
         * child; we may also draw a tag.
         * 
         * @param k
         *            the child dictionary to inspect
         */
        virtual public void InspectChildDictionary(PdfDictionary k, bool inspectAttributes) {
            if (k == null)
                return;
            PdfName s = k.GetAsName(PdfName.S);
            if (s != null) {
                String tagN = PdfName.DecodeName(s.ToString());
			    String tag = FixTagName(tagN);
                outp.Write("<");
                outp.Write(tag);
                if (inspectAttributes) {
                    PdfDictionary a = k.GetAsDict(PdfName.A);
                    if (a != null) {
                        Dictionary<PdfName, PdfObject>.KeyCollection keys = a.Keys;
                        foreach (PdfName key in keys) {
                            outp.Write(' ');
                            PdfObject value = a.Get(key);
                            value = PdfReader.GetPdfObject(value);
                            outp.Write(XmlName(key));
                            outp.Write("=\"");
                            outp.Write(value.ToString());
                            outp.Write("\"");
                        }
                    }
                }
                outp.Write(">");
                PdfObject alt = k.Get(PdfName.ALT);
                if (alt != null && alt.ToString() != null) {
                    outp.Write("<alt><![CDATA[");
                    outp.Write(Regex.Replace(alt.ToString(), "[\\000]*", ""));
                    outp.Write("]]></alt>");
                }
                PdfDictionary dict = k.GetAsDict(PdfName.PG);
                if (dict != null)
                    ParseTag(tagN, k.GetDirectObject(PdfName.K), dict);
                InspectChild(k.GetDirectObject(PdfName.K));
                outp.Write("</");
                outp.Write(tag);
                outp.WriteLine(">");
            } else
                InspectChild(k.GetDirectObject(PdfName.K));
        }

        virtual protected String XmlName(PdfName name)
        {
            String oldName = name.ToString();
            String xmlName = oldName.Remove(oldName.IndexOf("/"), 1);
            xmlName = (xmlName.ToLower()[0])
                       + xmlName.Substring(1);
            return xmlName;
        }


        private static String FixTagName(String tag) {
            StringBuilder sb = new StringBuilder();
            for (int k = 0; k < tag.Length; ++k) {
                char c = tag[k];
                bool nameStart =
                    c == ':'
                    || (c >= 'A' && c <= 'Z')
                    || c == '_'
                    || (c >= 'a' && c <= 'z')
                    || (c >= '\u00c0' && c <= '\u00d6')
                    || (c >= '\u00d8' && c <= '\u00f6')
                    || (c >= '\u00f8' && c <= '\u02ff')
                    || (c >= '\u0370' && c <= '\u037d')
                    || (c >= '\u037f' && c <= '\u1fff')
                    || (c >= '\u200c' && c <= '\u200d')
                    || (c >= '\u2070' && c <= '\u218f')
                    || (c >= '\u2c00' && c <= '\u2fef')
                    || (c >= '\u3001' && c <= '\ud7ff')
                    || (c >= '\uf900' && c <= '\ufdcf')
                    || (c >= '\ufdf0' && c <= '\ufffd');
                bool nameMiddle =
                    c == '-'
                    || c == '.'
                    || (c >= '0' && c <= '9')
                    || c == '\u00b7'
                    || (c >= '\u0300' && c <= '\u036f')
                    || (c >= '\u203f' && c <= '\u2040')
                    || nameStart;
                if (k == 0) {
                    if (!nameStart)
                        c = '_';
                }
                else {
                    if (!nameMiddle)
                        c = '-';
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        /**
         * Searches for a tag in a page.
         * 
         * @param tag
         *            the name of the tag
         * @param obj
         *            an identifier to find the marked content
         * @param page
         *            a page dictionary
         * @throws IOException
         */
        public virtual void ParseTag(String tag, PdfObject obj, PdfDictionary page) {
            // if the identifier is a number, we can extract the content right away
            if (obj is PdfNumber) {
                PdfNumber mcid = (PdfNumber) obj;
                RenderFilter filter = new MarkedContentRenderFilter(mcid.IntValue);
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                FilteredTextRenderListener listener = new FilteredTextRenderListener(strategy, new RenderFilter[]{filter});
                PdfContentStreamProcessor processor = new PdfContentStreamProcessor(
                        listener);
                processor.ProcessContent(PdfReader.GetPageContent(page), page
                        .GetAsDict(PdfName.RESOURCES));
                outp.Write(XMLUtil.EscapeXML(listener.GetResultantText(), true));
            }
            // if the identifier is an array, we call the parseTag method
            // recursively
            else if (obj is PdfArray) {
                PdfArray arr = (PdfArray) obj;
                int n = arr.Size;
                for (int i = 0; i < n; i++) {
                    ParseTag(tag, arr[i], page);
                    if (i < n - 1)
                        outp.WriteLine();
                }
            }
            // if the identifier is a dictionary, we get the resources from the
            // dictionary
            else if (obj is PdfDictionary) {
                PdfDictionary mcr = (PdfDictionary) obj;
                ParseTag(tag, mcr.GetDirectObject(PdfName.MCID), mcr
                        .GetAsDict(PdfName.PG));
            }
        }
    }
}
