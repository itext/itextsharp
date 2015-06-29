using System;
using System.IO;
using System.Xml;
using System.Collections;

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

namespace iTextSharp.text.xml
{
    /// <summary>
    /// The <CODE>ParserBase</CODE>-class provides XML document parsing.
    /// </summary>
    public abstract class ParserBase
    {
        virtual public void Parse(XmlDocument xDoc) {
            string xml = xDoc.OuterXml;
            StringReader stringReader = new StringReader(xml);
            XmlTextReader reader = new XmlTextReader(stringReader);
            this.Parse(reader);
        }

        virtual public void Parse(XmlTextReader reader) {
            try {
                while (reader.Read()) {
                    switch (reader.NodeType) {
                        case XmlNodeType.Element:
                            string namespaceURI = reader.NamespaceURI;
                            string name = reader.Name;
                            bool isEmpty = reader.IsEmptyElement;
                            Hashtable attributes = new Hashtable();
                            if (reader.HasAttributes) {
                                for (int i = 0; i < reader.AttributeCount; i++) {
                                    reader.MoveToAttribute(i);
                                    attributes.Add(reader.Name,reader.Value);
                                }
                            }
                            this.StartElement(namespaceURI, name, name, attributes);
                            if (isEmpty) {
                                EndElement(namespaceURI,
                                    name, name);
                            }
                            break;
                        case XmlNodeType.EndElement:
                            EndElement(reader.NamespaceURI,
                                reader.Name, reader.Name);
                            break;
                        case XmlNodeType.Text:
                            Characters(reader.Value, 0, reader.Value.Length);
                            break;
                            // There are many other types of nodes, but
                            // we are not interested in them
                        case XmlNodeType.Whitespace:
                            Characters(reader.Value, 0, reader.Value.Length);
                            break;
                    }
                }
            } catch (XmlException e) {
                Console.Out.WriteLine(e.Message);
            } finally {
                if (reader != null) {
                    reader.Close();
                }
            }
        }

        /// <summary>
        /// Begins the process of processing an XML document
        /// </summary>
        /// <param name="url">the XML document to parse</param>
        virtual public void Parse(string url) {
            XmlTextReader reader = null;
            reader = new XmlTextReader(url);
            this.Parse(reader);
        }

        /// <summary>
        /// This method gets called when a start tag is encountered.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="lname"></param>
        /// <param name="name">the name of the tag that is encountered</param>
        /// <param name="attrs">the list of attributes</param>
        public abstract void StartElement(String uri, String lname, String name, Hashtable attrs);

        /// <summary>
        /// This method gets called when an end tag is encountered.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="lname"></param>
        /// <param name="name">the name of the tag that ends</param>
        public abstract void EndElement(String uri, String lname, String name);

        /// <summary>
        /// This method gets called when characters are encountered.
        /// </summary>
        /// <param name="content">an array of characters</param>
        /// <param name="start">the start position in the array</param>
        /// <param name="length">the number of characters to read from the array</param>
        public abstract void Characters(string content, int start, int length);
    }
}
