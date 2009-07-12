using System;
using System.IO;
using System.Xml;
using System.Text;
/*
 * Copyright 2008 by Bruno Lowagie
 *
 * The contents of this file are subject to the Mozilla Public License Version 1.1
 * (the "License"); you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the License.
 *
 * The Original Code is 'iText, a free JAVA-PDF library'.
 *
 * The Initial Developer of the Original Code is Bruno Lowagie. Portions created by
 * the Initial Developer are Copyright (C) 1999, 2000, 2001, 2002 by Bruno Lowagie.
 * All Rights Reserved.
 * Co-Developer of the code is Paulo Soares. Portions created by the Co-Developer
 * are Copyright (C) 2008 by Paulo Soares. All Rights Reserved.
 *
 * Contributor(s): all the names of the contributors are added in the source code
 * where applicable.
 *
 * Alternatively, the contents of this file may be used under the terms of the
 * LGPL license (the "GNU LIBRARY GENERAL PUBLIC LICENSE"), in which case the
 * provisions of LGPL are applicable instead of those above.  If you wish to
 * allow use of your version of this file only under the terms of the LGPL
 * License and not to allow others to use your version of this file under
 * the MPL, indicate your decision by deleting the provisions above and
 * replace them with the notice and other provisions required by the LGPL.
 * If you do not delete the provisions above, a recipient may use your version
 * of this file under either the MPL or the GNU LIBRARY GENERAL PUBLIC LICENSE.
 *
 * This library is free software; you can redistribute it and/or modify it
 * under the terms of the MPL as stated above or under the terms of the GNU
 * Library General Public License as published by the Free Software Foundation;
 * either version 2 of the License, or any later version.
 *
 * This library is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 * FOR A PARTICULAR PURPOSE. See the GNU Library general Public License for more
 * details.
 *
 * If you didn't download this code from the following link, you should check if
 * you aren't using an obsolete version:
 * http://www.lowagie.com/iText/
 */
namespace iTextSharp.text.xml.xmp {


    /**
    * Reads an XMP stream into an org.w3c.dom.Document objects.
    * Allows you to replace the contents of a specific tag.
    * @since 2.1.3
    */

    public class XmpReader {

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
	    public bool Replace(String namespaceURI, String localName, String value) {
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
	    * Adds a tag.
	    * @param	namespaceURI	the URI of the namespace
	    * @param	parent			the tag name of the parent
	    * @param	localName		the name of the tag to add
	    * @param	value			the new content for the tag
	    * @return	true if the content was successfully added
	    * @since	2.1.6
	    */
	    public bool Add(String parent, String namespaceURI, String localName, String value) {
		    XmlNodeList nodes = domDocument.GetElementsByTagName(parent);
		    if (nodes.Count == 0)
			    return false;
		    XmlNode pNode;
		    XmlNode node;
		    for (int i = 0; i < nodes.Count; i++) {
			    pNode = nodes[i];
			    XmlAttributeCollection attrs = pNode.Attributes;
			    for (int j = 0; j < attrs.Count; j++) {
				    node = attrs[j];
				    if (namespaceURI.Equals(node.Value)) {
					    node = domDocument.CreateElement(localName);
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
        public bool SetNodeText(XmlDocument domDocument, XmlNode n, String value) {
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
	    public byte[] SerializeDoc() {
            MemoryStream fout = new MemoryStream();
            byte[] b = new UTF8Encoding(false).GetBytes(XmpWriter.XPACKET_PI_BEGIN);
            fout.Write(b, 0, b.Length);
            fout.Flush();
            XmlNodeList xmpmeta = domDocument.GetElementsByTagName("x:xmpmeta");
            XmlTextWriter xw = new XmlTextWriter(fout, new UTF8Encoding(false));
            xw.WriteNode(new XmlNodeReader(xmpmeta[0]), true);
            xw.Flush();
            b = new UTF8Encoding(false).GetBytes(XmpWriter.EXTRASPACE);
            for (int i = 0; i < 20; i++) {
                fout.Write(b, 0, b.Length);
            }
            b = new UTF8Encoding(false).GetBytes(XmpWriter.XPACKET_PI_END_W);
            fout.Write(b, 0, b.Length);
            fout.Close();
            return fout.ToArray();
	    }
    }
}