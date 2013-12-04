using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Collections;
/*
 * Copyright 1999-2005 The Apache Software Foundation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace iTextSharp.text.xml {

    /**
     *
     * @author psoares
     */
    public class XmlDomWriter {
        
        /** Print writer. */
        protected TextWriter fOut;
        
        /** Canonical output. */
        protected bool fCanonical;
        
        /** Processing XML 1.1 document. */
        protected bool fXML11;
        
        //
        // Constructors
        //
        
        /** Default constructor. */
        public XmlDomWriter() {
        } // <init>()
        
        public XmlDomWriter(bool canonical) {
            fCanonical = canonical;
        } // <init>(bool)
        
        //
        // Public methods
        //
        
        /** Sets whether output is canonical. */
        public void SetCanonical(bool canonical) {
            fCanonical = canonical;
        } // SetCanonical(bool)
        
        /** Sets the output stream for printing. */
        public void SetOutput(Stream stream, String encoding) {
            Encoding enc;
            if (encoding == null)
                enc = new UTF8Encoding(false);
            else
                enc = Encoding.GetEncoding(encoding);
            
            fOut = new StreamWriter(stream, enc);
            
        } // SetOutput(Stream,String)
        
        /** Sets the output writer. */
        public void SetOutput(TextWriter writer) {
            
            fOut = writer;
        } // SetOutput(java.io.Writer)
        
        /** Writes the specified node, recursively. */
        public void Write(XmlNode node) {
            
            // is there anything to do?
            if (node == null) {
                return;
            }
            
            XmlNodeType type = node.NodeType;
            switch (type) {
                case XmlNodeType.Document: {
                    XmlDocument document = (XmlDocument)node;
                    fXML11 = false; //"1.1".Equals(GetVersion(document));
                    if (!fCanonical) {
                        if (fXML11) {
                            fOut.WriteLine("<?xml version=\"1.1\" encoding=\"UTF-8\"?>");
                        } else {
                            fOut.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                        }
                        fOut.Flush();
                        Write(document.DocumentType);
                    }
                    Write(document.DocumentElement);
                    break;
                }
                
                case XmlNodeType.DocumentType: {
                    XmlDocumentType doctype = (XmlDocumentType)node;
                    fOut.Write("<!DOCTYPE ");
                    fOut.Write(doctype.Name);
                    String publicId = doctype.PublicId;
                    String systemId = doctype.SystemId;
                    if (publicId != null) {
                        fOut.Write(" PUBLIC '");
                        fOut.Write(publicId);
                        fOut.Write("' '");
                        fOut.Write(systemId);
                        fOut.Write('\'');
                    } else if (systemId != null) {
                        fOut.Write(" SYSTEM '");
                        fOut.Write(systemId);
                        fOut.Write('\'');
                    }
                    String internalSubset = doctype.InternalSubset;
                    if (internalSubset != null) {
                        fOut.WriteLine(" [");
                        fOut.Write(internalSubset);
                        fOut.Write(']');
                    }
                    fOut.WriteLine('>');
                    break;
                }
                
                case XmlNodeType.Element: {
                    fOut.Write('<');
                    fOut.Write(node.Name);
                    XmlAttribute[] attrs = SortAttributes(node.Attributes);
                    for (int i = 0; i < attrs.Length; i++) {
                        XmlAttribute attr = attrs[i];
                        fOut.Write(' ');
                        fOut.Write(attr.Name);
                        fOut.Write("=\"");
                        NormalizeAndPrint(attr.Value, true);
                        fOut.Write('"');
                    }
                    fOut.Write('>');
                    fOut.Flush();
                    
                    XmlNode child = node.FirstChild;
                    while (child != null) {
                        Write(child);
                        child = child.NextSibling;
                    }
                    break;
                }
                
                case XmlNodeType.EntityReference: {
                    if (fCanonical) {
                        XmlNode child = node.FirstChild;
                        while (child != null) {
                            Write(child);
                            child = child.NextSibling;
                        }
                    } else {
                        fOut.Write('&');
                        fOut.Write(node.Name);
                        fOut.Write(';');
                        fOut.Flush();
                    }
                    break;
                }
                
                case XmlNodeType.CDATA: {
                    if (fCanonical) {
                        NormalizeAndPrint(node.Value, false);
                    } else {
                        fOut.Write("<![CDATA[");
                        fOut.Write(node.Value);
                        fOut.Write("]]>");
                    }
                    fOut.Flush();
                    break;
                }
                
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.Whitespace:
                case XmlNodeType.Text: {
                    NormalizeAndPrint(node.Value, false);
                    fOut.Flush();
                    break;
                }
                
                case XmlNodeType.ProcessingInstruction: {
                    fOut.Write("<?");
                    fOut.Write(node.Name);
                    String data = node.Value;
                    if (data != null && data.Length > 0) {
                        fOut.Write(' ');
                        fOut.Write(data);
                    }
                    fOut.Write("?>");
                    fOut.Flush();
                    break;
                }
                
                case XmlNodeType.Comment: {
                    if (!fCanonical) {
                        fOut.Write("<!--");
                        String comment = node.Value;
                        if (comment != null && comment.Length > 0) {
                            fOut.Write(comment);
                        }
                        fOut.Write("-->");
                        fOut.Flush();
                    }
                    break;
                }
            }
            
            if (type == XmlNodeType.Element) {
                fOut.Write("</");
                fOut.Write(node.Name);
                fOut.Write('>');
                fOut.Flush();
            }
            
        } // Write(Node)
        
        /** Returns a sorted list of attributes. */
        protected XmlAttribute[] SortAttributes(XmlAttributeCollection attrs) {
            
            int len = (attrs != null) ? attrs.Count : 0;
            XmlAttribute[] array = new XmlAttribute[len];
            for (int i = 0; i < len; i++) {
                array[i] = attrs[i];
            }
            for (int i = 0; i < len - 1; i++) {
                String name = array[i].Name;
                int index = i;
                for (int j = i + 1; j < len; j++) {
                    String curName = array[j].Name;
                    if (curName.CompareTo(name) < 0) {
                        name = curName;
                        index = j;
                    }
                }
                if (index != i) {
                    XmlAttribute temp = array[i];
                    array[i] = array[index];
                    array[index] = temp;
                }
            }
            
            return array;
            
        } // SortAttributes(NamedNodeMap):Attr[]
        
        //
        // Protected methods
        //
        
        /** Normalizes and prints the given string. */
        protected void NormalizeAndPrint(String s, bool isAttValue) {
            
            int len = (s != null) ? s.Length : 0;
            for (int i = 0; i < len; i++) {
                char c = s[i];
                NormalizeAndPrint(c, isAttValue);
            }
            
        } // NormalizeAndPrint(String,bool)
        
        /** Normalizes and print the given character. */
        protected void NormalizeAndPrint(char c, bool isAttValue) {
            
            switch (c) {
                case '<': {
                    fOut.Write("&lt;");
                    break;
                }
                case '>': {
                    fOut.Write("&gt;");
                    break;
                }
                case '&': {
                    fOut.Write("&amp;");
                    break;
                }
                case '"': {
                    // A '"' that appears in character data
                    // does not need to be escaped.
                    if (isAttValue) {
                        fOut.Write("&quot;");
                    } else {
                        fOut.Write("\"");
                    }
                    break;
                }
                case '\r': {
                    // If CR is part of the document's content, it
                    // must not be printed as a literal otherwise
                    // it would be normalized to LF when the document
                    // is reparsed.
                    fOut.Write("&#xD;");
                    break;
                }
                case '\n': {
                    if (fCanonical) {
                        fOut.Write("&#xA;");
                        break;
                    }
                    // else, default print char
                    goto default;
                }
                default: {
                    // In XML 1.1, control chars in the ranges [#x1-#x1F, #x7F-#x9F] must be escaped.
                    //
                    // Escape space characters that would be normalized to #x20 in attribute values
                    // when the document is reparsed.
                    //
                    // Escape NEL (0x85) and LSEP (0x2028) that appear in content
                    // if the document is XML 1.1, since they would be normalized to LF
                    // when the document is reparsed.
                    if (fXML11 && ((c >= 0x01 && c <= 0x1F && c != 0x09 && c != 0x0A)
                    || (c >= 0x7F && c <= 0x9F) || c == 0x2028)
                    || isAttValue && (c == 0x09 || c == 0x0A)) {
                        fOut.Write("&#x");
                        int ci = (int)c;
                        fOut.Write(ci.ToString("X"));
                        fOut.Write(";");
                    } else {
                        fOut.Write(c);
                    }
                    break;
                }
            }
        } // NormalizeAndPrint(char,bool)
    }
}