using System;
using System.IO;
using System.Xml;
using System.Text;
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
namespace iTextSharp.text.xml.xmp {


    /**
    * Reads an XMP stream into an org.w3c.dom.Document objects.
    * Allows you to replace the contents of a specific tag.
    * @since 2.1.3
    */
    [Obsolete]
    public class XmpReader {


        /** String used to fill the extra space. */
        public const String EXTRASPACE = "                                                                                                   \n";

        /**
        * Processing Instruction required at the start of an XMP stream
        * @since iText 2.1.6
        */
        public const String XPACKET_PI_BEGIN = "<?xpacket begin=\"\uFEFF\" id=\"W5M0MpCehiHzreSzNTczkc9d\"?>\n";

        /**
        * Processing Instruction required at the end of an XMP stream for XMP streams that can be updated
        * @since iText 2.1.6
        */
        public const String XPACKET_PI_END_W = "<?xpacket end=\"w\"?>";


        private XmlDocument domDocument;
        
        /**
        * Constructs an XMP reader
        * @param	bytes	the XMP content
        * @throws ExceptionConverter 
        * @throws IOException 
        * @throws SAXException 
        */
	    public XmpReader(byte[] bytes)  {
            MemoryStream bout = new MemoryStream();
            bout.Write(bytes, 0, bytes.Length);
            bout.Seek(0, SeekOrigin.Begin);
            XmlTextReader xtr = new XmlTextReader(bout);
            domDocument = new XmlDocument();
            domDocument.PreserveWhitespace = true;
            domDocument.Load(xtr);
	    }
    	
	    /**
	    * Replaces the content of a tag.
	    * @param	namespaceURI	the URI of the namespace
	    * @param	localName		the tag name
	    * @param	value			the new content for the tag
	    * @return	true if the content was successfully replaced
	    * @since	2.1.6 the return type has changed from void to boolean
	    */
	    virtual public bool ReplaceNode(String namespaceURI, String localName, String value) {
		    XmlNodeList nodes = domDocument.GetElementsByTagName(localName, namespaceURI);
		    XmlNode node;
		    if (nodes.Count == 0)
			    return false;
		    for (int i = 0; i < nodes.Count; i++) {
			    node = nodes[i];
			    SetNodeText(domDocument, node, value);
		    }
		    return true;
	    }    
        
        /**
        * Replaces the content of an attribute in the description tag.
        * @param    namespaceURI    the URI of the namespace
        * @param    localName       the tag name
        * @param    value           the new content for the tag
        * @return   true if the content was successfully replaced
        * @since    5.0.0 the return type has changed from void to boolean
        */
        virtual public bool ReplaceDescriptionAttribute(String namespaceURI, String localName, String value) {
            XmlNodeList descNodes = domDocument.GetElementsByTagName("Description", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            if(descNodes.Count == 0) {
                return false;
            }
            XmlNode node;
            for(int i = 0; i < descNodes.Count; i++) {
                node = descNodes.Item(i);
                XmlNode attr = node.Attributes.GetNamedItem(localName, namespaceURI);
                if(attr != null) {
                    attr.Value = value;
                    return true;
                }
            }
            return false;
        }

        /**
	    * Adds a tag.
	    * @param	namespaceURI	the URI of the namespace
	    * @param	parent			the tag name of the parent
	    * @param	localName		the name of the tag to add
	    * @param	value			the new content for the tag
	    * @return	true if the content was successfully added
	    * @since	2.1.6
	    */
	    virtual public bool Add(String parent, String namespaceURI, String localName, String value) {
		    XmlNodeList nodes = domDocument.GetElementsByTagName(parent);
		    if (nodes.Count == 0)
			    return false;
		    XmlNode pNode;
		    XmlNode node;
            String prefix;
		    for (int i = 0; i < nodes.Count; i++) {
			    pNode = nodes[i];
			    XmlAttributeCollection attrs = pNode.Attributes;
			    for (int j = 0; j < attrs.Count; j++) {
				    node = attrs[j];
				    if (namespaceURI.Equals(node.Value)) {
                        prefix = node.LocalName;
                        node = domDocument.CreateElement(localName, namespaceURI);
                        node.Prefix = prefix;
					    node.AppendChild(domDocument.CreateTextNode(value));
					    pNode.AppendChild(node);
					    return true;
				    }
			    }
		    }
		    return false;
	    }
    	
        /**
        * Sets the text of this node. All the child's node are deleted and a new
        * child text node is created.
        * @param domDocument the <CODE>Document</CODE> that contains the node
        * @param n the <CODE>Node</CODE> to add the text to
        * @param value the text to add
        */
        virtual public bool SetNodeText(XmlDocument domDocument, XmlNode n, String value) {
            if (n == null)
                return false;
            XmlNode nc = null;
            while ((nc = n.FirstChild) != null) {
                n.RemoveChild(nc);
            }
            n.AppendChild(domDocument.CreateTextNode(value));
            return true;
        }
    	
        /**
         * Writes the document to a byte array.
         */
        virtual public byte[] SerializeDoc() {
            XmlDomWriter xw = new XmlDomWriter();
            MemoryStream fout = new MemoryStream();
            xw.SetOutput(fout, null);
            byte[] b = new UTF8Encoding(false).GetBytes(XPACKET_PI_BEGIN);
            fout.Write(b, 0, b.Length);
            fout.Flush();
            XmlNodeList xmpmeta = domDocument.GetElementsByTagName("x:xmpmeta");
            xw.Write(xmpmeta[0]);
            fout.Flush();
            b = new UTF8Encoding(false).GetBytes(EXTRASPACE);
            for (int i = 0; i < 20; i++) {
                fout.Write(b, 0, b.Length);
            }
            b = new UTF8Encoding(false).GetBytes(XPACKET_PI_END_W);
            fout.Write(b, 0, b.Length);
            fout.Close();
            return fout.ToArray();
        }
    }
}
