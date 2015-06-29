using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.util;
using iTextSharp.text.xml.simpleparser;
using iTextSharp.text.xml;
using iTextSharp.text.error_messages;
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
namespace iTextSharp.text.pdf {

    /**
    *
    * @author Paulo Soares
    */
    public sealed class SimpleNamedDestination : ISimpleXMLDocHandler {
        
        private Dictionary<string,string> xmlNames;
        private Dictionary<string,string> xmlLast;

        private SimpleNamedDestination() {
        }
        
        public static Dictionary<string,string> GetNamedDestination(PdfReader reader, bool fromNames) {
            IntHashtable pages = new IntHashtable();
            int numPages = reader.NumberOfPages;
            for (int k = 1; k <= numPages; ++k)
                pages[reader.GetPageOrigRef(k).Number] = k;
            Dictionary<string,PdfObject> names = fromNames ? reader.GetNamedDestinationFromNames() : reader.GetNamedDestinationFromStrings();
            Dictionary<string,string> n2 = new Dictionary<string,string>(names.Count);
            String[] keys = new String[names.Count];
            names.Keys.CopyTo(keys, 0);
            foreach (String name in keys) {
                PdfArray arr = (PdfArray)names[name];
                StringBuilder s = new StringBuilder();
                try {
                    s.Append(pages[(arr.GetAsIndirectObject(0)).Number]);
                    s.Append(' ').Append(arr[1].ToString().Substring(1));
                    for (int k = 2; k < arr.Size; ++k)
                        s.Append(' ').Append(arr[k].ToString());
                    n2[name] = s.ToString();
                }
                catch {
                }
            }
            return n2;
        }
        
        /**
        * Exports the destinations to XML. The DTD for this XML is:
        * <p>
        * <pre>
        * &lt;?xml version='1.0' encoding='UTF-8'?&gt;
        * &lt;!ELEMENT Name (#PCDATA)&gt;
        * &lt;!ATTLIST Name
        *    Page CDATA #IMPLIED
        * &gt;
        * &lt;!ELEMENT Destination (Name)*&gt;
        * </pre>
        * @param names the names
        * @param outp the export destination. The stream is not closed
        * @param encoding the encoding according to IANA conventions
        * @param onlyASCII codes above 127 will always be escaped with &amp;#nn; if <CODE>true</CODE>,
        * whatever the encoding
        * @throws IOException on error
        */
        public static void ExportToXML(Dictionary<string,string> names, Stream outp, String encoding, bool onlyASCII) {
            StreamWriter wrt = new StreamWriter(outp, IanaEncodings.GetEncodingEncoding(encoding));
            ExportToXML(names, wrt, encoding, onlyASCII);
        }
        
        /**
        * Exports the bookmarks to XML.
        * @param names the names
        * @param wrt the export destination. The writer is not closed
        * @param encoding the encoding according to IANA conventions
        * @param onlyASCII codes above 127 will always be escaped with &amp;#nn; if <CODE>true</CODE>,
        * whatever the encoding
        * @throws IOException on error
        */
        public static void ExportToXML(Dictionary<string,string> names, TextWriter wrt, String encoding, bool onlyASCII) {
            wrt.Write("<?xml version=\"1.0\" encoding=\"");
            wrt.Write(XMLUtil.EscapeXML(encoding, onlyASCII));
            wrt.Write("\"?>\n<Destination>\n");
            foreach (String key in names.Keys) {
                String value = names[key];
                wrt.Write("  <Name Page=\"");
                wrt.Write(XMLUtil.EscapeXML(value, onlyASCII));
                wrt.Write("\">");
                wrt.Write(XMLUtil.EscapeXML(EscapeBinaryString(key), onlyASCII));
                wrt.Write("</Name>\n");
            }
            wrt.Write("</Destination>\n");
            wrt.Flush();
        }
        
        /**
        * Import the names from XML.
        * @param inp the XML source. The stream is not closed
        * @throws IOException on error
        * @return the names
        */
        public static Dictionary<string,string> ImportFromXML(Stream inp) {
            SimpleNamedDestination names = new SimpleNamedDestination();
            SimpleXMLParser.Parse(names, inp);
            return names.xmlNames;
        }
        
        /**
        * Import the names from XML.
        * @param inp the XML source. The reader is not closed
        * @throws IOException on error
        * @return the names
        */
        public static Dictionary<string,string> ImportFromXML(TextReader inp) {
            SimpleNamedDestination names = new SimpleNamedDestination();
            SimpleXMLParser.Parse(names, inp);
            return names.xmlNames;
        }

        internal static PdfArray CreateDestinationArray(String value, PdfWriter writer) {
            PdfArray ar = new PdfArray();
            StringTokenizer tk = new StringTokenizer(value);
            int n = int.Parse(tk.NextToken());
            ar.Add(writer.GetPageReference(n));
            if (!tk.HasMoreTokens()) {
                ar.Add(PdfName.XYZ);
                ar.Add(new float[]{0, 10000, 0});
            }
            else {
                String fn = tk.NextToken();
                if (fn.StartsWith("/"))
                    fn = fn.Substring(1);
                ar.Add(new PdfName(fn));
                for (int k = 0; k < 4 && tk.HasMoreTokens(); ++k) {
                    fn = tk.NextToken();
                    if (fn.Equals("null"))
                        ar.Add(PdfNull.PDFNULL);
                    else
                        ar.Add(new PdfNumber(fn));
                }
            }
            return ar;
        }
        
        public static PdfDictionary OutputNamedDestinationAsNames(Dictionary<string,string> names, PdfWriter writer) {
            PdfDictionary dic = new PdfDictionary();
            foreach (String key in names.Keys) {
                try {
                    String value = names[key];
                    PdfArray ar = CreateDestinationArray(value, writer);
                    PdfName kn = new PdfName(key);
                    dic.Put(kn, ar);
                }
                catch {
                    // empty on purpose
                }            
            }
            return dic;
        }
        
        public static PdfDictionary OutputNamedDestinationAsStrings(Dictionary<string,string> names, PdfWriter writer) {
            Dictionary<string,PdfObject> n2 = new Dictionary<string,PdfObject>(names.Count);
            foreach (String key in names.Keys) {
                try {
                    String value = names[key];
                    PdfArray ar = CreateDestinationArray(value, writer);
                    n2[key] = writer.AddToBody(ar).IndirectReference;
                }
                catch {
                    // empty on purpose
                }
            }
            return PdfNameTree.WriteTree(n2, writer);
        }
        
        public static String EscapeBinaryString(String s) {
            StringBuilder buf = new StringBuilder();
            char[] cc = s.ToCharArray();
            int len = cc.Length;
            for (int k = 0; k < len; ++k) {
                char c = cc[k];
                if (c < ' ') {
                    buf.Append('\\');
                    ((int)c).ToString("", System.Globalization.CultureInfo.InvariantCulture);
                    String octal = "00" + Convert.ToString((int)c, 8);
                    buf.Append(octal.Substring(octal.Length - 3));
                }
                else if (c == '\\')
                    buf.Append("\\\\");
                else
                    buf.Append(c);
            }
            return buf.ToString();
        }
        
        public static String UnEscapeBinaryString(String s) {
            StringBuilder buf = new StringBuilder();
            char[] cc = s.ToCharArray();
            int len = cc.Length;
            for (int k = 0; k < len; ++k) {
                char c = cc[k];
                if (c == '\\') {
                    if (++k >= len) {
                        buf.Append('\\');
                        break;
                    }
                    c = cc[k];
                    if (c >= '0' && c <= '7') {
                        int n = c - '0';
                        ++k;
                        for (int j = 0; j < 2 && k < len; ++j) {
                            c = cc[k];
                            if (c >= '0' && c <= '7') {
                                ++k;
                                n = n * 8 + c - '0';
                            }
                            else {
                                break;
                            }
                        }
                        --k;
                        buf.Append((char)n);
                    }
                    else
                        buf.Append(c);
                }
                else
                    buf.Append(c);
            }
            return buf.ToString();
        }
        
        public void EndDocument() {
        }
        
        public void EndElement(String tag) {
            if (tag.Equals("Destination")) {
                if (xmlLast == null && xmlNames != null)
                    return;
                else
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("destination.end.tag.out.of.place"));
            }
            if (!tag.Equals("Name"))
                throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.end.tag.1", tag));
            if (xmlLast == null || xmlNames == null)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("name.end.tag.out.of.place"));
            if (!xmlLast.ContainsKey("Page"))
                throw new ArgumentException(MessageLocalization.GetComposedMessage("page.attribute.missing"));
            xmlNames[UnEscapeBinaryString(xmlLast["Name"])] = xmlLast["Page"];
            xmlLast = null;
        }
        
        public void StartDocument() {
        }
        
        public void StartElement(String tag, IDictionary<string,string> h) {
            if (xmlNames == null) {
                if (tag.Equals("Destination")) {
                    xmlNames = new Dictionary<string,string>();
                    return;
                }
                else
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("root.element.is.not.destination"));
            }
            if (!tag.Equals("Name"))
                throw new ArgumentException(MessageLocalization.GetComposedMessage("tag.1.not.allowed", tag));
            if (xmlLast != null)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("nested.tags.are.not.allowed"));
            xmlLast = new Dictionary<string,string>(h);
            xmlLast["Name"] = "";
        }
        
        public void Text(String str) {
            if (xmlLast == null)
                return;
            String name = xmlLast["Name"];
            name += str;
            xmlLast["Name"] = name;
        }    
    }
}
