using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using iTextSharp.text.xml;
/*
 * $Id$
 *
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
    * Processes XFA forms.
    * @author Paulo Soares
    */
    public class XfaForm {

        private Xml2SomTemplate templateSom;
        private XmlNode templateNode;
        private Xml2SomDatasets datasetsSom;
        private AcroFieldsSearch acroFieldsSom;
        private PdfReader reader;
        private bool xfaPresent;
        private XmlDocument domDocument;
        private bool changed;
        private XmlNode datasetsNode;
        public const String XFA_DATA_SCHEMA = "http://www.xfa.org/schema/xfa-data/1.0/";

        /**
        * An empty constructor to build on.
        */
        public XfaForm() {
        }
        
        /**
        * Return the XFA Object, could be an array, could be a Stream.
        * Returns null f no XFA Object is present.
        * @param    reader  a PdfReader instance
        * @return   the XFA object
        * @since    2.1.3
        */
        public static PdfObject GetXfaObject(PdfReader reader) {
            PdfDictionary af = (PdfDictionary)PdfReader.GetPdfObjectRelease(reader.Catalog.Get(PdfName.ACROFORM));
            if (af == null) {
                return null;
            }
            return PdfReader.GetPdfObjectRelease(af.Get(PdfName.XFA));
        }
        
        /**
        * A constructor from a <CODE>PdfReader</CODE>. It basically does everything
        * from finding the XFA stream to the XML parsing.
        * @param reader the reader
        * @throws java.io.IOException on error
        * @throws javax.xml.parsers.ParserConfigurationException on error
        * @throws org.xml.sax.SAXException on error
        */
        public XfaForm(PdfReader reader) {
            this.reader = reader;
            PdfObject xfa = GetXfaObject(reader);
            if (xfa == null) {
                xfaPresent = false;
                return;
            }
            xfaPresent = true;
            MemoryStream bout = new MemoryStream();
            if (xfa.IsArray()) {
                PdfArray ar = (PdfArray)xfa;
                for (int k = 1; k < ar.Size; k += 2) {
                    PdfObject ob = ar.GetDirectObject(k);
                    if (ob is PRStream) {
                        byte[] b = PdfReader.GetStreamBytes((PRStream)ob);
                        bout.Write(b, 0, b.Length);
                    }
                }
            }
            else if (xfa is PRStream) {
                byte[] b = PdfReader.GetStreamBytes((PRStream)xfa);
                bout.Write(b, 0, b.Length);
            }
            bout.Seek(0, SeekOrigin.Begin);
            XmlTextReader xtr = new XmlTextReader(bout);
            domDocument = new XmlDocument();
            domDocument.PreserveWhitespace = true;
            domDocument.Load(xtr);
            ExtractNodes();
        }
        
        /**
        * Extracts the nodes from the domDocument.
        * @since    2.1.5
        */
        private void ExtractNodes() {
            Dictionary<String, XmlNode> xfaNodes = ExtractXFANodes(domDocument);

            if (xfaNodes.ContainsKey("template")) {
                templateNode = xfaNodes["template"];
                templateSom = new Xml2SomTemplate(templateNode);
            }
            if (xfaNodes.ContainsKey("datasets")) {
                datasetsNode = xfaNodes["datasets"];
                datasetsSom = new Xml2SomDatasets(datasetsNode.FirstChild);
            }

            if (datasetsNode == null)
        	    CreateDatasetsNode(domDocument.FirstChild);
        }

        public static Dictionary<String, XmlNode> ExtractXFANodes(XmlDocument domDocument) {
            Dictionary<String, XmlNode> xfaNodes = new Dictionary<string, XmlNode>();
            XmlNode n = domDocument.FirstChild;
            while (n.NodeType != XmlNodeType.Element || n.ChildNodes.Count == 0)
                n = n.NextSibling;
            n = n.FirstChild;
            while (n != null) {
                if (n.NodeType == XmlNodeType.Element) {
                    String s = n.LocalName;
                    xfaNodes[s] = n;
                }
                n = n.NextSibling;
            }
            return xfaNodes;
        }
        
        /**
         * Some XFA forms don't have a datasets node.
         * If this is the case, we have to add one.
         */
        private void CreateDatasetsNode(XmlNode n) {
            while (n.ChildNodes.Count == 0) {
                n = n.NextSibling;
            }
            if (n != null) {
                XmlElement e = n.OwnerDocument.CreateElement("xfa", "datasets", XFA_DATA_SCHEMA);
                e.SetAttribute("xmlns:xfa", XFA_DATA_SCHEMA);
                datasetsNode = e;
                n.AppendChild(datasetsNode);
            }
        }
        
        /**
        * Sets the XFA key from a byte array. The old XFA is erased.
        * @param form the data
        * @param reader the reader
        * @param writer the writer
        * @throws java.io.IOException on error
        */
        public static void SetXfa(XfaForm form, PdfReader reader, PdfWriter writer) {
            PdfDictionary af = (PdfDictionary)PdfReader.GetPdfObjectRelease(reader.Catalog.Get(PdfName.ACROFORM));
            if (af == null) {
                return;
            }
            PdfObject xfa = GetXfaObject(reader);
            if (xfa.IsArray()) {
                PdfArray ar = (PdfArray)xfa;
                int t = -1;
                int d = -1;
                for (int k = 0; k < ar.Size; k += 2) {
                    PdfString s = ar.GetAsString(k);
                    if ("template".Equals(s.ToString())) {
                        t = k + 1;
                    }
                    if ("datasets".Equals(s.ToString())) {
                        d = k + 1;
                    }
                }
                if (t > -1 && d > -1) {
                    reader.KillXref(ar.GetAsIndirectObject(t));
                    reader.KillXref(ar.GetAsIndirectObject(d));
                    PdfStream tStream = new PdfStream(SerializeDoc(form.templateNode));
                    tStream.FlateCompress(writer.CompressionLevel);
                    ar[t] = writer.AddToBody(tStream).IndirectReference;
                    PdfStream dStream = new PdfStream(SerializeDoc(form.datasetsNode));
                    dStream.FlateCompress(writer.CompressionLevel);
                    ar[d] = writer.AddToBody(dStream).IndirectReference;
                    af.Put(PdfName.XFA, new PdfArray(ar));
                    return;
                }
            }
            reader.KillXref(af.Get(PdfName.XFA));
            PdfStream str = new PdfStream(SerializeDoc(form.domDocument));
            str.FlateCompress(writer.CompressionLevel);
            PdfIndirectReference refe = writer.AddToBody(str).IndirectReference;
            af.Put(PdfName.XFA, refe);
        }

        /**
        * Sets the XFA key from the instance data. The old XFA is erased.
        * @param writer the writer
        * @throws java.io.IOException on error
        */
        virtual public void SetXfa(PdfWriter writer) {
            SetXfa(this, reader, writer);
        }

        /**
         * Serializes a XML document to a byte array.
         * @param n the XML document
         * @throws java.io.IOException on error
         * @return the serialized XML document
         */
        public static byte[] SerializeDoc(XmlNode n) {
            XmlDomWriter xw = new XmlDomWriter();
            MemoryStream fout = new MemoryStream();
            xw.SetOutput(fout, null);
            xw.SetCanonical(false);
            xw.Write(n);
            fout.Close();
            return fout.ToArray();
        }
        
        /**
        * Returns <CODE>true</CODE> if it is a XFA form.
        * @return <CODE>true</CODE> if it is a XFA form
        */
        virtual public bool XfaPresent {
            get {
                return xfaPresent;
            }
            set {
                xfaPresent = value;
            }
        }

        /**
        * Gets the top level DOM document.
        * @return the top level DOM document
        */
        virtual public XmlDocument DomDocument {
            get {
                return domDocument;
            }
            set {
                domDocument = value;
                ExtractNodes();
            }
        }
        
        
        /**
        * Finds the complete field name contained in the "classic" forms from a partial
        * name.
        * @param name the complete or partial name
        * @param af the fields
        * @return the complete name or <CODE>null</CODE> if not found
        */
        virtual public String FindFieldName(String name, AcroFields af) {
            IDictionary<String, AcroFields.Item> items = af.Fields;
            if (items.ContainsKey(name))
                return name;
            if (acroFieldsSom == null) {
        	    if (items.Count == 0 && xfaPresent)
                    acroFieldsSom = new AcroFieldsSearch(datasetsSom.Name2Node.Keys);
                else
                    acroFieldsSom = new AcroFieldsSearch(items.Keys);
            }
            if (acroFieldsSom.AcroShort2LongName.ContainsKey(name))
                return acroFieldsSom.AcroShort2LongName[name];
            return acroFieldsSom.InverseSearchGlobal(Xml2Som.SplitParts(name));
        }
        
        /**
        * Finds the complete SOM name contained in the datasets section from a 
        * possibly partial name.
        * @param name the complete or partial name
        * @return the complete name or <CODE>null</CODE> if not found
        */
        virtual public String FindDatasetsName(String name) {
            if (datasetsSom.Name2Node.ContainsKey(name))
                return name;
            return datasetsSom.InverseSearchGlobal(Xml2Som.SplitParts(name));
        }

        /**
        * Finds the <CODE>Node</CODE> contained in the datasets section from a 
        * possibly partial name.
        * @param name the complete or partial name
        * @return the <CODE>Node</CODE> or <CODE>null</CODE> if not found
        */
        virtual public XmlNode FindDatasetsNode(String name) {
            if (name == null)
                return null;
            name = FindDatasetsName(name);
            if (name == null)
                return null;
            return datasetsSom.Name2Node[name];
        }

        /**
        * Gets all the text contained in the child nodes of this node.
        * @param n the <CODE>Node</CODE>
        * @return the text found or "" if no text was found
        */
        public static String GetNodeText(XmlNode n) {
            if (n == null)
                return "";
            return GetNodeText(n, "");
            
        }
        
        private static String GetNodeText(XmlNode n, String name) {
            XmlNode n2 = n.FirstChild;
            while (n2 != null) {
                if (n2.NodeType == XmlNodeType.Element) {
                    name = GetNodeText(n2, name);
                }
                else if (n2.NodeType == XmlNodeType.Text) {
                    name += n2.Value;
                }
                n2 = n2.NextSibling;
            }
            return name;
        }
        
        /**
        * Sets the text of this node. All the child's node are deleted and a new
        * child text node is created.
        * @param n the <CODE>Node</CODE> to add the text to
        * @param text the text to add
        */
        virtual public void SetNodeText(XmlNode n, String text) {
            if (n == null)
                return;
            XmlNode nc = null;
            while ((nc = n.FirstChild) != null) {
                n.RemoveChild(nc);
            }
            n.Attributes.RemoveNamedItem("dataNode", XFA_DATA_SCHEMA);
            n.AppendChild(domDocument.CreateTextNode(text));
            changed = true;
        }
        
        /**
        * Sets the <CODE>PdfReader</CODE> to be used by this instance.
        * @param reader the <CODE>PdfReader</CODE> to be used by this instance
        */
        virtual public PdfReader Reader {
            set { 
                reader = value;
            }
            get {
                return reader;
            }
        }

        /**
        * Checks if this XFA form was changed.
        * @return <CODE>true</CODE> if this XFA form was changed
        */
        virtual public bool Changed {
            get {
                return changed;
            }
            set {
                changed = value;
            }
        }

        /**
        * A structure to store each part of a SOM name and link it to the next part
        * beginning from the lower hierarchie.
        */
        public class InverseStore {
            protected internal List<String> part = new List<string>();
            protected internal List<object> follow = new List<object>();
            
            /**
            * Gets the full name by traversing the hiearchie using only the
            * index 0.
            * @return the full name
            */
            virtual public String DefaultName {
                get {
                    InverseStore store = this;
                    while (true) {
                        Object obj = store.follow[0];
                        if (obj is String)
                            return (String)obj;
                        store = (InverseStore)obj;
                    }
                }
            }
            
            /**
            * Search the current node for a similar name. A similar name starts
            * with the same name but has a differnt index. For example, "detail[3]" 
            * is similar to "detail[9]". The main use is to discard names that
            * correspond to out of bounds records.
            * @param name the name to search
            * @return <CODE>true</CODE> if a similitude was found
            */
            virtual public bool IsSimilar(String name) {
                int idx = name.IndexOf('[');
                name = name.Substring(0, idx + 1);
                foreach (String n in part) { 
                    if (n.StartsWith(name))
                        return true;
                }
                return false;
            }
        }

        /**
        * Another stack implementation. The main use is to facilitate
        * the porting to other languages.
        */
        public class Stack2<T> : List<T> {
            /**
            * Looks at the object at the top of this stack without removing it from the stack.
            * @return the object at the top of this stack
            */
            virtual public T Peek() {
                if (Count == 0)
                    throw new InvalidOperationException();
                return this[Count - 1];
            }
            
            /**
            * Removes the object at the top of this stack and returns that object as the value of this function.
            * @return the object at the top of this stack 
            */
            virtual public T Pop() {
                if (Count == 0)
                    throw new InvalidOperationException();
                T ret = this[Count - 1];
                RemoveAt(Count - 1);
                return ret;
            }
            
            /**
            * Pushes an item onto the top of this stack.
            * @param item the item to be pushed onto this stack
            * @return the <CODE>item</CODE> argument
            */
            virtual public T Push(T item) {
                Add(item);
                return item;
            }
            
            /**
            * Tests if this stack is empty.
            * @return <CODE>true</CODE> if and only if this stack contains no items; <CODE>false</CODE> otherwise
            */
            virtual public bool Empty() {
                return Count == 0;
            }
        }
        
        /**
        * A class for some basic SOM processing.
        */
        public class Xml2Som {
            /**
            * The order the names appear in the XML, depth first.
            */
            protected List<String> order;
            /**
            * The mapping of full names to nodes.
            */
            protected Dictionary<string,XmlNode> name2Node;
            /**
            * The data to do a search from the bottom hierarchie.
            */
            protected Dictionary<String, InverseStore> inverseSearch;
            /**
            * A stack to be used when parsing.
            */
            protected Stack2<string> stack;
            /**
            * A temporary store for the repetition count.
            */
            protected int anform;

            /**
            * Escapes a SOM string fragment replacing "." with "\.".
            * @param s the unescaped string
            * @return the escaped string
            */
            public static String EscapeSom(String s) {
                if (s == null)
                    return "";
                int idx = s.IndexOf('.');
                if (idx < 0)
                    return s;
                StringBuilder sb = new StringBuilder();
                int last = 0;
                while (idx >= 0) {
                    sb.Append(s.Substring(last, idx - last));
                    sb.Append('\\');
                    last = idx;
                    idx = s.IndexOf('.', idx + 1);
                }
                sb.Append(s.Substring(last));
                return sb.ToString();
            }

            /**
            * Unescapes a SOM string fragment replacing "\." with ".".
            * @param s the escaped string
            * @return the unescaped string
            */
            public static String UnescapeSom(String s) {
                int idx = s.IndexOf('\\');
                if (idx < 0)
                    return s;
                StringBuilder sb = new StringBuilder();
                int last = 0;
                while (idx >= 0) {
                    sb.Append(s.Substring(last, idx - last));
                    last = idx + 1;
                    idx = s.IndexOf('\\', idx + 1);
                }
                sb.Append(s.Substring(last));
                return sb.ToString();
            }

            /**
            * Outputs the stack as the sequence of elements separated
            * by '.'.
            * @return the stack as the sequence of elements separated by '.'
            */
            virtual protected String PrintStack() {
                if (stack.Empty())
                    return "";
                StringBuilder s = new StringBuilder();
                foreach (String part in stack)
                    s.Append('.').Append(part);
                return s.ToString(1, s.Length - 1);
            }
            
            /**
            * Gets the name with the <CODE>#subform</CODE> removed.
            * @param s the long name
            * @return the short name
            */
            public static String GetShortName(String s) {
                int idx = s.IndexOf(".#subform[");
                if (idx < 0)
                    return s;
                int last = 0;
                StringBuilder sb = new StringBuilder();
                while (idx >= 0) {
                    sb.Append(s.Substring(last, idx - last));
                    idx = s.IndexOf("]", idx + 10);
                    if (idx < 0)
                        return sb.ToString();
                    last = idx + 1;
                    idx = s.IndexOf(".#subform[", last);
                }
                sb.Append(s.Substring(last));
                return sb.ToString();
            }
            
            /**
            * Adds a SOM name to the search node chain.
            * @param unstack the SOM name
            */
            virtual public void InverseSearchAdd(String unstack) {
                InverseSearchAdd(inverseSearch, stack, unstack);
            }
            
            /**
            * Adds a SOM name to the search node chain.
            * @param inverseSearch the start point
            * @param stack the stack with the separeted SOM parts
            * @param unstack the full name
            */
            public static void InverseSearchAdd(Dictionary<String, InverseStore> inverseSearch, Stack2<string> stack, String unstack) {
                String last = stack.Peek();
                InverseStore store;
                inverseSearch.TryGetValue(last, out store);
                if (store == null) {
                    store = new InverseStore();
                    inverseSearch[last] = store;
                }
                for (int k = stack.Count - 2; k >= 0; --k) {
                    last = stack[k];
                    InverseStore store2;
                    int idx = store.part.IndexOf(last);
                    if (idx < 0) {
                        store.part.Add(last);
                        store2 = new InverseStore();
                        store.follow.Add(store2);
                    }
                    else
                        store2 = (InverseStore)store.follow[idx];
                    store = store2;
                }
                store.part.Add("");
                store.follow.Add(unstack);
            }

            /**
            * Searchs the SOM hiearchie from the bottom.
            * @param parts the SOM parts
            * @return the full name or <CODE>null</CODE> if not found
            */
            virtual public String InverseSearchGlobal(List<String> parts) {
                if (parts.Count == 0)
                    return null;
                InverseStore store;
                inverseSearch.TryGetValue(parts[parts.Count - 1], out store);
                if (store == null)
                    return null;
                for (int k = parts.Count - 2; k >= 0; --k) {
                    String part = parts[k];
                    int idx = store.part.IndexOf(part);
                    if (idx < 0) {
                        if (store.IsSimilar(part))
                            return null;
                        return store.DefaultName;
                    }
                    store = (InverseStore)store.follow[idx];
                }
                return store.DefaultName;
            }
        
            /**
            * Splits a SOM name in the individual parts.
            * @param name the full SOM name
            * @return the split name
            */
            public static Stack2<String> SplitParts(String name) {
                while (name.StartsWith("."))
                    name = name.Substring(1);
                Stack2<String> parts = new Stack2<String>();
                int last = 0;
                int pos = 0;
                String part;
                while (true) {
                    pos = last;
                    while (true) {
                        pos = name.IndexOf('.', pos);
                        if (pos < 0)
                            break;
                        if (name[pos - 1] == '\\')
                            ++pos;
                        else
                            break;
                    }
                    if (pos < 0)
                        break;
                    part = name.Substring(last, pos - last);
                    if (!part.EndsWith("]"))
                        part += "[0]";
                    parts.Add(part);
                    last = pos + 1;
                }
                part = name.Substring(last);
                if (!part.EndsWith("]"))
                    part += "[0]";
                parts.Add(part);
                return parts;
            }

            /**
            * Gets the order the names appear in the XML, depth first.
            * @return the order the names appear in the XML, depth first
            */
            virtual public List<String> Order {
                get {
                    return order;
                }
                set {
                    order = value;
                }
            }

            /**
            * Gets the mapping of full names to nodes.
            * @return the mapping of full names to nodes
            */
            virtual public Dictionary<String, XmlNode> Name2Node {
                get {
                    return name2Node;
                }
                set {
                    name2Node = value;
                }
            }

            /**
            * Gets the data to do a search from the bottom hierarchie.
            * @return the data to do a search from the bottom hierarchie
            */
            virtual public Dictionary<string,InverseStore> InverseSearch {
                get {
                    return inverseSearch;
                }
                set {
                    inverseSearch = value;
                }
            }
        }
        
        /**
        * Processes the datasets section in the XFA form.
        */
        public class Xml2SomDatasets : Xml2Som {
            /**
            * Creates a new instance from the datasets node. This expects
            * not the datasets but the data node that comes below.
            * @param n the datasets node
            */
            public Xml2SomDatasets(XmlNode n) {
                order = new List<string>();
                name2Node = new Dictionary<string,XmlNode>();
                stack = new Stack2<string>();
                anform = 0;
                inverseSearch = new Dictionary<string,InverseStore>();
                ProcessDatasetsInternal(n);
            }

            /**
            * Inserts a new <CODE>Node</CODE> that will match the short name.
            * @param n the datasets top <CODE>Node</CODE>
            * @param shortName the short name
            * @return the new <CODE>Node</CODE> of the inserted name
            */
            virtual public XmlNode InsertNode(XmlNode n, String shortName) {
                Stack2<string> stack = SplitParts(shortName);
                XmlDocument doc = n.OwnerDocument;
                XmlNode n2 = null;
                n = n.FirstChild;
                while (n.NodeType != XmlNodeType.Element)
                    n = n.NextSibling;
                for (int k = 0; k < stack.Count; ++k) {
                    String part = stack[k];
                    int idx = part.LastIndexOf('[');
                    String name = part.Substring(0, idx);
                    idx = int.Parse(part.Substring(idx + 1, part.Length - idx - 2));
                    int found = -1;
                    for (n2 = n.FirstChild; n2 != null; n2 = n2.NextSibling) {
                        if (n2.NodeType == XmlNodeType.Element) {
                            String s = EscapeSom(n2.LocalName);
                            if (s.Equals(name)) {
                                ++found;
                                if (found == idx)
                                    break;
                            }
                        }
                    }
                    for (; found < idx; ++found) {
                        n2 = doc.CreateElement(name);
                        n2 = n.AppendChild(n2);
                        XmlNode attr = doc.CreateNode(XmlNodeType.Attribute, "dataNode", XFA_DATA_SCHEMA);
                        attr.Value = "dataGroup";
                        n2.Attributes.SetNamedItem(attr);
                    }
                    n = n2;
                }
                InverseSearchAdd(inverseSearch, stack, shortName);
                name2Node[shortName] = n2;
                order.Add(shortName);
                return n2;
            }

            private static bool HasChildren(XmlNode n) {
                XmlNode dataNodeN = n.Attributes.GetNamedItem("dataNode", XFA_DATA_SCHEMA);
                if (dataNodeN != null) {
                    String dataNode = dataNodeN.Value;
                    if ("dataGroup".Equals(dataNode))
                        return true;
                    else if ("dataValue".Equals(dataNode))
                        return false;
                }
                if (!n.HasChildNodes)
                    return false;
                XmlNode n2 = n.FirstChild;
                while (n2 != null) {
                    if (n2.NodeType == XmlNodeType.Element) {
                        return true;
                    }
                    n2 = n2.NextSibling;
                }
                return false;
            }

            private void ProcessDatasetsInternal(XmlNode n) {
                if (n != null) {
                    Dictionary<String, int> ss = new Dictionary<string,int>();
                    XmlNode n2 = n.FirstChild;
                    while (n2 != null) {
                        if (n2.NodeType == XmlNodeType.Element) {
                            String s = EscapeSom(n2.LocalName);
                            int i;
                            if (!ss.ContainsKey(s))
                                i = 0;
                            else
                                i = ss[s] + 1;
                            ss[s] = i;
                            if (HasChildren(n2)) {
                                stack.Push(s + "[" + i.ToString() + "]");
                                ProcessDatasetsInternal(n2);
                                stack.Pop();
                            }
                            else {
                                stack.Push(s + "[" + i.ToString() + "]");
                                String unstack = PrintStack();
                                order.Add(unstack);
                                InverseSearchAdd(unstack);
                                name2Node[unstack] = n2;
                                stack.Pop();
                            }
                        }
                        n2 = n2.NextSibling;
                    }
                }
            }
        }

        /**
        * A class to process "classic" fields.
        */
        public class AcroFieldsSearch : Xml2Som {
            private Dictionary<String, String> acroShort2LongName;
            
            /**
            * Creates a new instance from a Collection with the full names.
            * @param items the Collection
            */
            public AcroFieldsSearch(ICollection<string> items) {
                inverseSearch = new Dictionary<string,InverseStore>();
                acroShort2LongName = new Dictionary<string,string>();
                foreach (String itemName in items) {
                    String itemShort = GetShortName(itemName);
                    acroShort2LongName[itemShort] = itemName;
                    InverseSearchAdd(inverseSearch, SplitParts(itemShort), itemName);
                }
            }

            /**
            * Gets the mapping from short names to long names. A long 
            * name may contain the #subform name part.
            * @return the mapping from short names to long names
            */
            virtual public Dictionary<String, String> AcroShort2LongName {
                get {
                    return acroShort2LongName;
                }
                set {
                    acroShort2LongName = value;
                }
            }
        }

        /**
        * Processes the template section in the XFA form.
        */
        public class Xml2SomTemplate : Xml2Som {
            private bool dynamicForm;
            private int templateLevel;
            
            /**
            * Creates a new instance from the datasets node.
            * @param n the template node
            */
            public Xml2SomTemplate(XmlNode n) {
                order = new List<string>();
                name2Node = new Dictionary<string,XmlNode>();
                stack = new Stack2<string>();
                anform = 0;
                templateLevel = 0;
                inverseSearch = new Dictionary<string,InverseStore>();
                ProcessTemplate(n, null);
            }

            /**
            * Gets the field type as described in the <CODE>template</CODE> section of the XFA.
            * @param s the exact template name
            * @return the field type or <CODE>null</CODE> if not found
            */
            virtual public String GetFieldType(String s) {
                XmlNode n;
                name2Node.TryGetValue(s, out n);
                if (n == null)
                    return null;
                if ("exclGroup".Equals(n.LocalName))
                    return "exclGroup";
                XmlNode ui = n.FirstChild;
                while (ui != null) {
                    if (ui.NodeType == XmlNodeType.Element && "ui".Equals(ui.LocalName)) {
                        break;
                    }
                    ui = ui.NextSibling;
                }
                if (ui == null)
                    return null;
                XmlNode type = ui.FirstChild;
                while (type != null) {
                    if (type.NodeType == XmlNodeType.Element && !("extras".Equals(type.LocalName) && "picture".Equals(type.LocalName))) {
                        return type.LocalName;
                    }
                    type = type.NextSibling;
                }
                return null;
            }

            private void ProcessTemplate(XmlNode n, Dictionary<string,int> ff) {
                if (ff == null)
                    ff = new Dictionary<string,int>();
                Dictionary<string,int> ss = new Dictionary<string,int>();
                XmlNode n2 = n.FirstChild;
                while (n2 != null) {
                    if (n2.NodeType == XmlNodeType.Element) {
                        String s = n2.LocalName;
                        if ("subform".Equals(s)) {
                            XmlNode name = n2.Attributes.GetNamedItem("name");
                            String nn = "#subform";
                            bool annon = true;
                            if (name != null) {
                                nn = EscapeSom(name.Value);
                                annon = false;
                            }
                            int i;
                            if (annon) {
                                i = anform;
                                ++anform;
                            }
                            else {
                                if (!ss.ContainsKey(nn))
                                    i = 0;
                                else
                                    i = ss[nn] + 1;
                                ss[nn] = i;
                            }
                            stack.Push(nn + "[" + i.ToString() + "]");
                            ++templateLevel;
                            if (annon)
                                ProcessTemplate(n2, ff);
                            else
                                ProcessTemplate(n2, null);
                            --templateLevel;
                            stack.Pop();
                        }
                        else if ("field".Equals(s) || "exclGroup".Equals(s)) {
                            XmlNode name = n2.Attributes.GetNamedItem("name");
                            if (name != null) {
                                String nn = EscapeSom(name.Value);
                                int i;
                                if (!ff.ContainsKey(nn))
                                    i = 0;
                                else
                                    i = ff[nn] + 1;
                                ff[nn] = i;
                                stack.Push(nn + "[" + i.ToString() + "]");
                                String unstack = PrintStack();
                                order.Add(unstack);
                                InverseSearchAdd(unstack);
                                name2Node[unstack] = n2;
                                stack.Pop();
                            }
                        }
                        else if (!dynamicForm && templateLevel > 0 && "occur".Equals(s)) {
                            int initial = 1;
                            int min = 1;
                            int max = 1;
                            XmlNode a = n2.Attributes.GetNamedItem("initial");
                            if (a != null)
                                try{initial = int.Parse(a.Value.Trim());}catch{};
                            a = n2.Attributes.GetNamedItem("min");
                            if (a != null)
                                try{min = int.Parse(a.Value.Trim());}catch{};
                            a = n2.Attributes.GetNamedItem("max");
                            if (a != null)
                                try{max = int.Parse(a.Value.Trim());}catch{};
                            if (initial != min || min != max)
                                dynamicForm = true;
                        }
                    }
                    n2 = n2.NextSibling;
                }
            }

            /**
            * <CODE>true</CODE> if it's a dynamic form; <CODE>false</CODE>
            * if it's a static form.
            * @return <CODE>true</CODE> if it's a dynamic form; <CODE>false</CODE>
            * if it's a static form
            */
            virtual public bool DynamicForm {
                get {
                    return dynamicForm;
                }
                set {
                    dynamicForm = value;
                }
            }
        }

        /**
        * Gets the class that contains the template processing section of the XFA.
        * @return the class that contains the template processing section of the XFA
        */
        virtual public Xml2SomTemplate TemplateSom {
            get {
                return templateSom;
            }
            set {
                templateSom = value;
            }
        }

        /**
        * Gets the class that contains the datasets processing section of the XFA.
        * @return the class that contains the datasets processing section of the XFA
        */
        virtual public Xml2SomDatasets DatasetsSom {
            get {
                return datasetsSom;
            }
            set {
                datasetsSom = value;
            }
        }

        /**
        * Gets the class that contains the "classic" fields processing.
        * @return the class that contains the "classic" fields processing
        */
        virtual public AcroFieldsSearch AcroFieldsSom {
            get {
                return acroFieldsSom;
            }
            set {
                acroFieldsSom = value;
            }
        }

        /**
        * Gets the <CODE>Node</CODE> that corresponds to the datasets part.
        * @return the <CODE>Node</CODE> that corresponds to the datasets part
        */
        virtual public XmlNode DatasetsNode {
            get {
                return datasetsNode;
            }
        }

        virtual public void FillXfaForm(string file) {
            using (FileStream fs = new FileStream(file, FileMode.Open)) {
		        FillXfaForm(fs, false);
            }
        }

        virtual public void FillXfaForm(Stream stream) {
            FillXfaForm(stream, false);
        }
        
        virtual public void FillXfaForm(Stream stream, bool readOnly) {
    	    FillXfaForm(new XmlTextReader(stream), readOnly);
        }

        virtual public void FillXfaForm(XmlReader reader) {
            FillXfaForm(reader, false);
        }
        
        virtual public void FillXfaForm(XmlReader reader, bool readOnly) {
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.Load(reader);
    	    FillXfaForm(doc.DocumentElement);
        }

        virtual public void FillXfaForm(XmlNode node) {
            FillXfaForm(node, false);
        }
        
        /**
        * Replaces the data under datasets/data.
        * @since	iText 5.0.0
        */
        virtual public void FillXfaForm(XmlNode node, bool readOnly) {
            if (readOnly) {
                XmlNodeList nodeList = domDocument.GetElementsByTagName("field");
                for (int i = 0; i < nodeList.Count; i++) {
                    ((XmlElement)nodeList.Item(i)).SetAttribute("access", "readOnly");
                }
            }
            XmlNodeList allChilds = datasetsNode.ChildNodes;
            XmlNode data = null;
            foreach (XmlNode n in allChilds) {
                if (n.NodeType == XmlNodeType.Element && n.LocalName.Equals("data") && XFA_DATA_SCHEMA.Equals(n.NamespaceURI)) {
                    data = n;
                    break;
                }
            }
            if (data == null) {
                data = datasetsNode.OwnerDocument.CreateElement("xfa:data", XFA_DATA_SCHEMA);
                datasetsNode.AppendChild(data);
            }
		    XmlNodeList list = data.ChildNodes;
		    if (list.Count == 0) {
			    data.AppendChild(domDocument.ImportNode(node, true));
		    }
		    else {
                // There's a possibility that first child node of XFA data is not an ELEMENT but simply a TEXT. In this case data will be duplicated.
                XmlNode firstNode = GetFirstElementNode(data);
                if (firstNode != null)
                    data.ReplaceChild(domDocument.ImportNode(node, true), firstNode);
		    }
            ExtractNodes();
		    Changed = true;
        }

        private XmlNode GetFirstElementNode(XmlNode src) {
            XmlNode result = null;
            XmlNodeList list = src.ChildNodes;
            for (int i = 0; i < list.Count; i++) {
                if (list[i].NodeType ==  XmlNodeType.Element) {
                    result = list[i];
                    break;
                }
            }
            return result;
        }
    }
}
