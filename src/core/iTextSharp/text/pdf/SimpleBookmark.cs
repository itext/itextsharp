using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.util;
using iTextSharp.text.xml;
using iTextSharp.text.xml.simpleparser;
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
    * Bookmark processing in a simple way. It has some limitations, mainly the only
    * action types supported are GoTo, GoToR, URI and Launch.
    * <p>
    * The list structure is composed by a number of Hashtable, keyed by strings, one Hashtable
    * for each bookmark.
    * The element values are all strings with the exception of the key "Kids" that has
    * another list for the child bookmarks.
    * <p>
    * All the bookmarks have a "Title" with the
    * bookmark title and optionally a "Style" that can be "bold", "italic" or a
    * combination of both. They can also have a "Color" key with a value of three
    * floats separated by spaces. The key "Open" can have the values "true" or "false" and
    * signals the open status of the children. It's "true" by default.
    * <p>
    * The actions and the parameters can be:
    * <ul>
    * <li>"Action" = "GoTo" - "Page" | "Named"
    * <ul>
    * <li>"Page" = "3 XYZ 70 400 null" - page number followed by a destination (/XYZ is also accepted)
    * <li>"Named" = "named_destination"
    * </ul>
    * <li>"Action" = "GoToR" - "Page" | "Named" | "NamedN", "File", ["NewWindow"]
    * <ul>
    * <li>"Page" = "3 XYZ 70 400 null" - page number followed by a destination (/XYZ is also accepted)
    * <li>"Named" = "named_destination_as_a_string"
    * <li>"NamedN" = "named_destination_as_a_name"
    * <li>"File" - "the_file_to_open"
    * <li>"NewWindow" - "true" or "false"
    * </ul>
    * <li>"Action" = "URI" - "URI"
    * <ul>
    * <li>"URI" = "http://sf.net" - URI to jump to
    * </ul>
    * <li>"Action" = "Launch" - "File"
    * <ul>
    * <li>"File" - "the_file_to_open_or_execute"
    * </ul>
    * @author Paulo Soares
    */
    public sealed class SimpleBookmark : ISimpleXMLDocHandler {
        
        private List<Dictionary<String, Object>> topList;
        private Stack<Dictionary<String, Object>> attr = new Stack<Dictionary<String, Object>>();
        
        /** Creates a new instance of SimpleBookmark */
        private SimpleBookmark() {
        }
        
        private static IList<Dictionary<String, Object>> BookmarkDepth(PdfReader reader, PdfDictionary outline, IntHashtable pages, bool processCurrentOutlineOnly) {
            List<Dictionary<String, Object>> list = new List<Dictionary<String, Object>>();
            while (outline != null) {
                Dictionary<String, Object> map = new Dictionary<string,object>();
                PdfString title = (PdfString)PdfReader.GetPdfObjectRelease(outline.Get(PdfName.TITLE));
                map["Title"] = title.ToUnicodeString();
                PdfArray color = (PdfArray)PdfReader.GetPdfObjectRelease(outline.Get(PdfName.C));
                if (color != null && color.Size == 3) {
                    ByteBuffer outp = new ByteBuffer();
                    outp.Append(color.GetAsNumber(0).FloatValue).Append(' ');
                    outp.Append(color.GetAsNumber(1).FloatValue).Append(' ');
                    outp.Append(color.GetAsNumber(2).FloatValue);
                    map["Color"] = PdfEncodings.ConvertToString(outp.ToByteArray(), null);
                }
                PdfNumber style = (PdfNumber)PdfReader.GetPdfObjectRelease(outline.Get(PdfName.F));
                if (style != null) {
                    int f = style.IntValue;
                    String s = "";
                    if ((f & 1) != 0)
                        s += "italic ";
                    if ((f & 2) != 0)
                        s += "bold ";
                    s = s.Trim();
                    if (s.Length != 0) 
                        map["Style"] = s;
                }
                PdfNumber count = (PdfNumber)PdfReader.GetPdfObjectRelease(outline.Get(PdfName.COUNT));
                if (count != null && count.IntValue < 0)
                    map["Open"] = "false";
                try {
                    PdfObject dest = PdfReader.GetPdfObjectRelease(outline.Get(PdfName.DEST));
                    if (dest != null) {
                        MapGotoBookmark(map, dest, pages); //changed by ujihara 2004-06-13
                    }
                    else {
                        PdfDictionary action = (PdfDictionary)PdfReader.GetPdfObjectRelease(outline.Get(PdfName.A));
                        if (action != null) {
                            if (PdfName.GOTO.Equals(PdfReader.GetPdfObjectRelease(action.Get(PdfName.S)))) {
                                dest = PdfReader.GetPdfObjectRelease(action.Get(PdfName.D));
                                if (dest != null) {
                                    MapGotoBookmark(map, dest, pages);
                                }
                            }
                            else if (PdfName.URI.Equals(PdfReader.GetPdfObjectRelease(action.Get(PdfName.S)))) {
                                map["Action"] = "URI";
                                map["URI"] = ((PdfString)PdfReader.GetPdfObjectRelease(action.Get(PdfName.URI))).ToUnicodeString();
                            }
                            else if (PdfName.JAVASCRIPT.Equals(PdfReader.GetPdfObjectRelease(action.Get(PdfName.S)))) {
                                map["Action"] = "JS";
                                map["Code"] = PdfReader.GetPdfObjectRelease(action.Get(PdfName.JS)).ToString();
                            }
                            else if (PdfName.GOTOR.Equals(PdfReader.GetPdfObjectRelease(action.Get(PdfName.S)))) {
                                dest = PdfReader.GetPdfObjectRelease(action.Get(PdfName.D));
                                if (dest != null) {
                                    if (dest.IsString())
                                        map["Named"] = dest.ToString();
                                    else if (dest.IsName())
                                        map["NamedN"] = PdfName.DecodeName(dest.ToString());
                                    else if (dest.IsArray()) {
                                        PdfArray arr = (PdfArray)dest;
                                        StringBuilder s = new StringBuilder();
                                        s.Append(arr[0].ToString());
                                        s.Append(' ').Append(arr[1].ToString());
                                        for (int k = 2; k < arr.Size; ++k)
                                            s.Append(' ').Append(arr[k].ToString());
                                        map["Page"] = s.ToString();
                                    }
                                }
                                map["Action"] = "GoToR";
                                PdfObject file = PdfReader.GetPdfObjectRelease(action.Get(PdfName.F));
                                if (file != null) {
                                    if (file.IsString())
                                        map["File"] = ((PdfString)file).ToUnicodeString();
                                    else if (file.IsDictionary()) {
                                        file = PdfReader.GetPdfObject(((PdfDictionary)file).Get(PdfName.F));
                                        if (file.IsString())
                                            map["File"] = ((PdfString)file).ToUnicodeString();
                                    }
                                }
                                PdfObject newWindow = PdfReader.GetPdfObjectRelease(action.Get(PdfName.NEWWINDOW));
                                if (newWindow != null)
                                    map["NewWindow"] = newWindow.ToString();
                            }
                            else if (PdfName.LAUNCH.Equals(PdfReader.GetPdfObjectRelease(action.Get(PdfName.S)))) {
                                map["Action"] = "Launch";
                                PdfObject file = PdfReader.GetPdfObjectRelease(action.Get(PdfName.F));
                                if (file == null)
                                    file = PdfReader.GetPdfObjectRelease(action.Get(PdfName.WIN));
                                if (file != null) {
                                    if (file.IsString())
                                        map["File"] = ((PdfString)file).ToUnicodeString();
                                    else if (file.IsDictionary()) {
                                        file = PdfReader.GetPdfObjectRelease(((PdfDictionary)file).Get(PdfName.F));
                                        if (file.IsString())
                                            map["File"] = ((PdfString)file).ToUnicodeString();
                                    }
                                }
                            }
                        }
                    }
                }
                catch  {
                    //empty on purpose
                }
                PdfDictionary first = (PdfDictionary)PdfReader.GetPdfObjectRelease(outline.Get(PdfName.FIRST));
                if (first != null) {
                    map["Kids"] = BookmarkDepth(reader, first, pages, false);
                }
                list.Add(map);
                if (!processCurrentOutlineOnly)
                    outline = (PdfDictionary)PdfReader.GetPdfObjectRelease(outline.Get(PdfName.NEXT));
                else
                    outline = null;
            }
            return list;
        }
        
        private static void MapGotoBookmark(Dictionary<String, Object> map, PdfObject dest, IntHashtable pages) 
        {
            if (dest.IsString())
                map["Named"] = dest.ToString();
            else if (dest.IsName())
                map["Named"] = PdfName.DecodeName(dest.ToString());
            else if (dest.IsArray()) 
                map["Page"] = MakeBookmarkParam((PdfArray)dest, pages); //changed by ujihara 2004-06-13
            map["Action"] = "GoTo";
        }

        private static String MakeBookmarkParam(PdfArray dest, IntHashtable pages)
        {
            StringBuilder s = new StringBuilder();
            PdfObject obj = dest[0];
            if (obj.IsNumber())
                s.Append(((PdfNumber)obj).IntValue + 1);
            else
                s.Append(pages[GetNumber((PdfIndirectReference)obj)]); //changed by ujihara 2004-06-13
            s.Append(' ').Append(dest[1].ToString().Substring(1));
            for (int k = 2; k < dest.Size; ++k)
                s.Append(' ').Append(dest[k].ToString());
            return s.ToString();
        }
        
        /**
        * Gets number of indirect. If type of directed indirect is PAGES, it refers PAGE object through KIDS.
        * (Contributed by Kazuya Ujihara)
        * @param indirect 
        * 2004-06-13
        */
        private static int GetNumber(PdfIndirectReference indirect)
        {
            PdfDictionary pdfObj = (PdfDictionary)PdfReader.GetPdfObjectRelease(indirect);
            if (pdfObj.Contains(PdfName.TYPE) && pdfObj.Get(PdfName.TYPE).Equals(PdfName.PAGES) && pdfObj.Contains(PdfName.KIDS)) 
            {
                PdfArray kids = (PdfArray)pdfObj.Get(PdfName.KIDS);
                indirect = (PdfIndirectReference)kids[0];
            }
            return indirect.Number;
        }
        
        /**
        * Gets a <CODE>List</CODE> with the bookmarks. It returns <CODE>null</CODE> if
        * the document doesn't have any bookmarks.
        * @param reader the document
        * @return a <CODE>List</CODE> with the bookmarks or <CODE>null</CODE> if the
        * document doesn't have any
        */    
        public static IList<Dictionary<String, Object>> GetBookmark(PdfReader reader) {
            PdfDictionary catalog = reader.Catalog;
            PdfObject obj = PdfReader.GetPdfObjectRelease(catalog.Get(PdfName.OUTLINES));
            if (obj == null || !obj.IsDictionary())
                return null;
            PdfDictionary outlines = (PdfDictionary)obj;
            return SimpleBookmark.GetBookmark(reader, outlines, false);
        }

        /**
        * Gets a <CODE>List</CODE> with the bookmarks that are children of <CODE>outline</CODE>. It returns <CODE>null</CODE> if
        * the document doesn't have any bookmarks.
        * @param reader the document
        * @param outline the outline dictionary to get bookmarks from
        * @param includeRoot indicates if to include <CODE>outline</CODE> parameter itself into returned list of bookmarks
        * @return a <CODE>List</CODE> with the bookmarks or <CODE>null</CODE> if the
        * document doesn't have any
        */
        public static IList<Dictionary<String, Object>> GetBookmark(PdfReader reader, PdfDictionary outline, bool includeRoot) {
            if (outline == null)
                return null;
            IntHashtable pages = new IntHashtable();
            int numPages = reader.NumberOfPages;
            for (int k = 1; k <= numPages; ++k) {
                pages[reader.GetPageOrigRef(k).Number] = k;
                reader.ReleasePage(k);
            }
            if (includeRoot)
                return BookmarkDepth(reader, outline, pages, true);
            else
                return BookmarkDepth(reader, (PdfDictionary)PdfReader.GetPdfObjectRelease(outline.Get(PdfName.FIRST)), pages, false);
        }

        /**
        * Removes the bookmark entries for a number of page ranges. The page ranges
        * consists of a number of pairs with the start/end page range. The page numbers
        * are inclusive.
        * @param list the bookmarks
        * @param pageRange the page ranges, always in pairs.
        */    
        public static void EliminatePages(IList<Dictionary<String, Object>> list, int[] pageRange) {
            if (list == null)
                return;

            for (ListIterator<Dictionary<String, Object>> it = new ListIterator<Dictionary<string,object>>(list); it.HasNext();) {
                Dictionary<String, Object> map = it.Next();
                bool hit = false;
                if (map.ContainsKey("Action") && "GoTo".Equals(map["Action"])) {
                    String page = null;
                    if (map.ContainsKey("Page"))
                        page = (String)map["Page"];
                    if (page != null) {
                        page = page.Trim();
                        int idx = page.IndexOf(' ');
                        int pageNum;
                        if (idx < 0)
                            pageNum = int.Parse(page);
                        else
                            pageNum = int.Parse(page.Substring(0, idx));
                        int len = pageRange.Length & 0x7ffffffe;
                        for (int k = 0; k < len; k += 2) {
                            if (pageNum >= pageRange[k] && pageNum <= pageRange[k + 1]) {
                                hit = true;
                                break;
                            }
                        }
                    }
                }
                IList<Dictionary<String, Object>> kids = null;
                if (map.ContainsKey("Kids"))
                    kids = (IList<Dictionary<String, Object>>)map["Kids"];
                if (kids != null) {
                    EliminatePages(kids, pageRange);
                    if (kids.Count == 0) {
                        map.Remove("Kids");
                        kids = null;
                    }
                }
                if (hit) {
                    if (kids == null)
                        it.Remove();
                    else {
                        map.Remove("Action");
                        map.Remove("Page");
                        map.Remove("Named");
                    }
                }
            }
        }
        
        /**
        * For the pages in range add the <CODE>pageShift</CODE> to the page number.
        * The page ranges
        * consists of a number of pairs with the start/end page range. The page numbers
        * are inclusive.
        * @param list the bookmarks
        * @param pageShift the number to add to the pages in range
        * @param pageRange the page ranges, always in pairs. It can be <CODE>null</CODE>
        * to include all the pages
        */    
        public static void ShiftPageNumbers(IList<Dictionary<String, Object>> list, int pageShift, int[] pageRange) {
            if (list == null)
                return;
            foreach (Dictionary<String, Object> map in list) {
                if (map.ContainsKey("Action") && "GoTo".Equals(map["Action"])) {
                    String page = null;
                    if (map.ContainsKey("Page"))
                        page = (String)map["Page"];
                    if (page != null) {
                        page = page.Trim();
                        int idx = page.IndexOf(' ');
                        int pageNum;
                        if (idx < 0)
                            pageNum = int.Parse(page);
                        else
                            pageNum = int.Parse(page.Substring(0, idx));
                        bool hit = false;
                        if (pageRange == null)
                            hit = true;
                        else {
                            int len = pageRange.Length & 0x7ffffffe;
                            for (int k = 0; k < len; k += 2) {
                                if (pageNum >= pageRange[k] && pageNum <= pageRange[k + 1]) {
                                    hit = true;
                                    break;
                                }
                            }
                        }
                        if (hit) {
                            if (idx < 0)
                                page = (pageNum + pageShift) + "";
                            else
                                page = (pageNum + pageShift) + page.Substring(idx);
                        }
                        map["Page"] = page;
                    }
                }
                IList<Dictionary<String, Object>> kids = null;
                if (map.ContainsKey("Kids"))
                    kids = (IList<Dictionary<String, Object>>)map["Kids"];
                if (kids != null)
                    ShiftPageNumbers(kids, pageShift, pageRange);
            }
        }

        public static string GetVal(Dictionary<String, Object> map, string key) {
            object v;
            map.TryGetValue(key, out v);
            return (string)v;
        }

        internal static void CreateOutlineAction(PdfDictionary outline, Dictionary<String, Object> map, PdfWriter writer, bool namedAsNames) {
            try {
                String action = GetVal(map, "Action");
                if ("GoTo".Equals(action)) {
                    String p;
                    if ((p = GetVal(map, "Named")) != null) {
                        if (namedAsNames)
                            outline.Put(PdfName.DEST, new PdfName(p));
                        else
                            outline.Put(PdfName.DEST, new PdfString(p, null));
                    }
                    else if ((p = GetVal(map, "Page")) != null) {
                        PdfArray ar = new PdfArray();
                        StringTokenizer tk = new StringTokenizer(p);
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
                        outline.Put(PdfName.DEST, ar);
                    }
                }
                else if ("GoToR".Equals(action)) {
                    String p;
                    PdfDictionary dic = new PdfDictionary();
                    if ((p = GetVal(map, "Named")) != null)
                        dic.Put(PdfName.D, new PdfString(p, null));
                    else if ((p = GetVal(map, "NamedN")) != null)
                        dic.Put(PdfName.D, new PdfName(p));
                    else if ((p = GetVal(map, "Page")) != null){
                        PdfArray ar = new PdfArray();
                        StringTokenizer tk = new StringTokenizer(p);
                        ar.Add(new PdfNumber(tk.NextToken()));
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
                        dic.Put(PdfName.D, ar);
                    }
                    String file = GetVal(map, "File");
                    if (dic.Size > 0 && file != null) {
                        dic.Put(PdfName.S,  PdfName.GOTOR);
                        dic.Put(PdfName.F, new PdfString(file));
                        String nw = GetVal(map, "NewWindow");
                        if (nw != null) {
                            if (nw.Equals("true"))
                                dic.Put(PdfName.NEWWINDOW, PdfBoolean.PDFTRUE);
                            else if (nw.Equals("false"))
                                dic.Put(PdfName.NEWWINDOW, PdfBoolean.PDFFALSE);
                        }
                        outline.Put(PdfName.A, dic);
                    }
                }
                else if ("URI".Equals(action)) {
                    String uri = GetVal(map, "URI");
                    if (uri != null) {
                        PdfDictionary dic = new PdfDictionary();
                        dic.Put(PdfName.S, PdfName.URI);
                        dic.Put(PdfName.URI, new PdfString(uri));
                        outline.Put(PdfName.A, dic);
                    }
                }
                else if ("JS".Equals(action)) {
                    String code = GetVal(map, "Code");
                    if(code != null) {
                        outline.Put(PdfName.A, PdfAction.JavaScript(code, writer));
                    }
                }
                else if ("Launch".Equals(action)) {
                    String file = GetVal(map, "File");
                    if (file != null) {
                        PdfDictionary dic = new PdfDictionary();
                        dic.Put(PdfName.S, PdfName.LAUNCH);
                        dic.Put(PdfName.F, new PdfString(file));
                        outline.Put(PdfName.A, dic);
                    }
                }
            }
            catch  {
                // empty on purpose
            }
        }

        public static Object[] IterateOutlines(PdfWriter writer, PdfIndirectReference parent, IList<Dictionary<String, Object>> kids, bool namedAsNames) {
            PdfIndirectReference[] refs = new PdfIndirectReference[kids.Count];
            for (int k = 0; k < refs.Length; ++k)
                refs[k] = writer.PdfIndirectReference;
            int ptr = 0;
            int count = 0;
            foreach (Dictionary<String, Object> map in kids) {
                Object[] lower = null;
                IList<Dictionary<String, Object>> subKid = null;
                if (map.ContainsKey("Kids"))
                    subKid = (IList<Dictionary<String, Object>>)map["Kids"];
                if (subKid != null && subKid.Count > 0)
                    lower = IterateOutlines(writer, refs[ptr], subKid, namedAsNames);
                PdfDictionary outline = new PdfDictionary();
                ++count;
                if (lower != null) {
                    outline.Put(PdfName.FIRST, (PdfIndirectReference)lower[0]);
                    outline.Put(PdfName.LAST, (PdfIndirectReference)lower[1]);
                    int n = (int)lower[2];
                    if (map.ContainsKey("Open") && "false".Equals(map["Open"])) {
                        outline.Put(PdfName.COUNT, new PdfNumber(-n));
                    }
                    else {
                        outline.Put(PdfName.COUNT, new PdfNumber(n));
                        count += n;
                    }
                }
                outline.Put(PdfName.PARENT, parent);
                if (ptr > 0)
                    outline.Put(PdfName.PREV, refs[ptr - 1]);
                if (ptr < refs.Length - 1)
                    outline.Put(PdfName.NEXT, refs[ptr + 1]);
                outline.Put(PdfName.TITLE, new PdfString((String)map["Title"], PdfObject.TEXT_UNICODE));
                String color = null;
                if (map.ContainsKey("Color"))
                    color = (String)map["Color"];
                if (color != null) {
                    try {
                        PdfArray arr = new PdfArray();
                        StringTokenizer tk = new StringTokenizer(color);
                        for (int k = 0; k < 3; ++k) {
                            float f = float.Parse(tk.NextToken(), System.Globalization.NumberFormatInfo.InvariantInfo);
                            if (f < 0) f = 0;
                            if (f > 1) f = 1;
                            arr.Add(new PdfNumber(f));
                        }
                        outline.Put(PdfName.C, arr);
                    } catch {} //in case it's malformed
                }
                String style = GetVal(map, "Style");
                if (style != null) {
                    style = style.ToLower(System.Globalization.CultureInfo.InvariantCulture);
                    int bits = 0;
                    if (style.IndexOf("italic") >= 0)
                        bits |= 1;
                    if (style.IndexOf("bold") >= 0)
                        bits |= 2;
                    if (bits != 0)
                        outline.Put(PdfName.F, new PdfNumber(bits));
                }
                CreateOutlineAction(outline, map, writer, namedAsNames);
                writer.AddToBody(outline, refs[ptr]);
                ++ptr;
            }
            return new Object[]{refs[0], refs[refs.Length - 1], count};
        }
        
        /**
        * Exports the bookmarks to XML. Only of use if the generation is to be include in
        * some other XML document.
        * @param list the bookmarks
        * @param out the export destination. The writer is not closed
        * @param indent the indentation level. Pretty printing significant only. Use <CODE>-1</CODE> for no indents.
        * @param onlyASCII codes above 127 will always be escaped with &amp;#nn; if <CODE>true</CODE>,
        * whatever the encoding
        * @throws IOException on error
        */
        public static void ExportToXMLNode(IList<Dictionary<String, Object>> list, TextWriter outp, int indent, bool onlyASCII) {
            String dep = "";
            if (indent != -1) {
                for (int k = 0; k < indent; ++k)
                    dep += "  ";
            }
            foreach (Dictionary<String, Object> map in list) {
                String title = null;
                outp.Write(dep);
                outp.Write("<Title ");
                IList<Dictionary<String, Object>> kids = null;
                foreach (KeyValuePair<string,object> entry in map) {
                    String key = entry.Key;
                    if (key.Equals("Title")) {
                        title = (String)entry.Value;
                        continue;
                    }
                    else if (key.Equals("Kids")) {
                        kids = (IList<Dictionary<String, Object>>)entry.Value;
                        continue;
                    }
                    else {
                        outp.Write(key);
                        outp.Write("=\"");
                        String value = (String)entry.Value;
                        if (key.Equals("Named") || key.Equals("NamedN"))
                            value = EscapeBinaryString(value);
                        outp.Write(XMLUtil.EscapeXML(value, onlyASCII));
                        outp.Write("\" ");
                    }
                }
                outp.Write(">");
                if (title == null)
                    title = "";
                outp.Write(XMLUtil.EscapeXML(title, onlyASCII));
                if (kids != null) {
                    outp.Write("\n");
                    ExportToXMLNode(kids, outp, indent == -1 ? indent : indent + 1, onlyASCII);
                    outp.Write(dep);
                }
                outp.Write("</Title>\n");
            }
        }

        /**
        * Exports the bookmarks to XML. The DTD for this XML is:
        * <p>
        * <pre>
        * &lt;?xml version='1.0' encoding='UTF-8'?&gt;
        * &lt;!ELEMENT Title (#PCDATA|Title)*&gt;
        * &lt;!ATTLIST Title
        *    Action CDATA #IMPLIED
        *    Open CDATA #IMPLIED
        *    Page CDATA #IMPLIED
        *    URI CDATA #IMPLIED
        *    File CDATA #IMPLIED
        *    Named CDATA #IMPLIED
        *    NamedN CDATA #IMPLIED
        *    NewWindow CDATA #IMPLIED
        *    Style CDATA #IMPLIED
        *    Color CDATA #IMPLIED
        * &gt;
        * &lt;!ELEMENT Bookmark (Title)*&gt;
        * </pre>
        * @param list the bookmarks
        * @param out the export destination. The stream is not closed
        * @param encoding the encoding according to IANA conventions
        * @param onlyASCII codes above 127 will always be escaped with &amp;#nn; if <CODE>true</CODE>,
        * whatever the encoding
        * @throws IOException on error
        */    
        public static void ExportToXML(IList<Dictionary<String, Object>> list, Stream outp, String encoding, bool onlyASCII) {
            StreamWriter wrt = new StreamWriter(outp, IanaEncodings.GetEncodingEncoding(encoding));
            ExportToXML(list, wrt, encoding, onlyASCII);
        }
        
        /**
        * Exports the bookmarks to XML.
        * @param list the bookmarks
        * @param wrt the export destination. The writer is not closed
        * @param encoding the encoding according to IANA conventions
        * @param onlyASCII codes above 127 will always be escaped with &amp;#nn; if <CODE>true</CODE>,
        * whatever the encoding
        * @throws IOException on error
        */
        public static void ExportToXML(IList<Dictionary<String, Object>> list, TextWriter wrt, String encoding, bool onlyASCII) {
            wrt.Write("<?xml version=\"1.0\" encoding=\"");
            wrt.Write(XMLUtil.EscapeXML(encoding, onlyASCII));
            wrt.Write("\"?>\n<Bookmark>\n");
            ExportToXMLNode(list, wrt, 1, onlyASCII);
            wrt.Write("</Bookmark>\n");
            wrt.Flush();
        }
        
        /**
        * Import the bookmarks from XML.
        * @param in the XML source. The stream is not closed
        * @throws IOException on error
        * @return the bookmarks
        */    
        public static IList<Dictionary<String, Object>> ImportFromXML(Stream inp) {
            SimpleBookmark book = new SimpleBookmark();
            SimpleXMLParser.Parse(book, inp);
            return book.topList;
        }
        
        /**
        * Import the bookmarks from XML.
        * @param in the XML source. The reader is not closed
        * @throws IOException on error
        * @return the bookmarks
        */
        public static IList<Dictionary<String, Object>> ImportFromXML(TextReader inp) {
            SimpleBookmark book = new SimpleBookmark();
            SimpleXMLParser.Parse(book, inp);
            return book.topList;
        }
        
        public static String EscapeBinaryString(String s) {
            StringBuilder buf = new StringBuilder();
            char[] cc = s.ToCharArray();
            int len = cc.Length;
            for (int k = 0; k < len; ++k) {
                char c = cc[k];
                if (c < ' ') {
                    buf.Append('\\');
                    int v = (int)c;
                    string octal = "";
                    do {
                        int x = v % 8;
                        octal = x.ToString() + octal;
                        v /= 8;
                    } while (v > 0);
                    buf.Append(octal.PadLeft(3, '0'));
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
            if (tag.Equals("Bookmark")) {
                if (attr.Count == 0)
                    return;
                else
                    throw new Exception(MessageLocalization.GetComposedMessage("bookmark.end.tag.out.of.place"));
            }
            if (!tag.Equals("Title"))
                throw new Exception(MessageLocalization.GetComposedMessage("invalid.end.tag.1", tag));
            Dictionary<String, Object> attributes = attr.Pop();
            String title = (String)attributes["Title"];
            attributes["Title"] = title.Trim();
            String named = GetVal(attributes, "Named");
            if (named != null)
                attributes["Named"] = UnEscapeBinaryString(named);
            named = GetVal(attributes, "NamedN");
            if (named != null)
                attributes["NamedN"] = UnEscapeBinaryString(named);
            if (attr.Count == 0)
                topList.Add(attributes);
            else {
                Dictionary<String, Object> parent = attr.Peek();
                IList<Dictionary<String, Object>> kids = null;
                if (parent.ContainsKey("Kids"))
                    kids = (IList<Dictionary<String, Object>>)parent["Kids"];
                if (kids == null) {
                    kids = new List<Dictionary<String, Object>>();
                    parent["Kids"] = kids;
                }
                kids.Add(attributes);
            }
        }
        
        public void StartDocument() {
        }
        
        public void StartElement(String tag, IDictionary<string,string> h) {
            if (topList == null) {
                if (tag.Equals("Bookmark")) {
                    topList = new List<Dictionary<String, Object>>();
                    return;
                }
                else
                    throw new Exception(MessageLocalization.GetComposedMessage("root.element.is.not.bookmark.1", tag));
            }
            if (!tag.Equals("Title"))
                throw new Exception(MessageLocalization.GetComposedMessage("tag.1.not.allowed", tag));
            Dictionary<String, Object> attributes = new Dictionary<String, Object>();
            foreach (KeyValuePair<string,string> kv in h)
                attributes[kv.Key] = kv.Value;
            attributes["Title"] = "";
            attributes.Remove("Kids");
            attr.Push(attributes);
        }
        
        public void Text(String str) {
            if (attr.Count == 0)
                return;
            Dictionary<String, Object> attributes = attr.Peek();
            String title = (String)attributes["Title"];
            title += str;
            attributes["Title"] = title;
        }    
    }
}
