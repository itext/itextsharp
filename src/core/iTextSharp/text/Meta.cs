using System;
using System.Text;
using System.Collections.Generic;
using System.util;

/*
 * $Id$
 * 
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

namespace iTextSharp.text {
    /// <summary>
    /// This is an Element that contains
    /// some meta information about the document.
    /// </summary>
    /// <remarks>
    /// An object of type Meta can not be constructed by the user.
    /// Userdefined meta information should be placed in a Header-object.
    /// Meta is reserved for: Subject, Keywords, Author, Title, Producer
    /// and Creationdate information.
    /// </remarks>
    /// <seealso cref="T:iTextSharp.text.Element"/>
    /// <seealso cref="T:iTextSharp.text.Header"/>
    public class Meta : IElement {
    
        // membervariables
    
        ///<summary> This is the type of Meta-information this object contains. </summary>
        private int type;
    
        ///<summary> This is the content of the Meta-information. </summary>
        private StringBuilder content;

        /**
         * The possible value of an alignment attribute.
         * @since 5.0.6 (moved from ElementTags)
         */
        public const String UNKNOWN = "unknown";

        /**
         * The possible value of an alignment attribute.
         * @since 5.0.6 (moved from ElementTags)
         */
        public const String PRODUCER = "producer";

        /**
         * The possible value of an alignment attribute.
         * @since 5.0.6 (moved from ElementTags)
         */
        public const String CREATIONDATE = "creationdate";

        /**
         * The possible value of an alignment attribute.
         * @since 5.0.6 (moved from ElementTags)
         */
        public const String AUTHOR = "author";

        /**
         * The possible value of an alignment attribute.
         * @since 5.0.6 (moved from ElementTags)
         */
        public const String KEYWORDS = "keywords";

        /**
         * The possible value of an alignment attribute.
         * @since 5.0.6 (moved from ElementTags)
         */
        public const String SUBJECT = "subject";

        /**
         * The possible value of an alignment attribute.
         * @since 5.0.6 (moved from ElementTags)
         */
        public const String TITLE = "title";

        // constructors
    
        /// <summary>
        /// Constructs a Meta.
        /// </summary>
        /// <param name="type">the type of meta-information</param>
        /// <param name="content">the content</param>
        public Meta(int type, string content) {
            this.type = type;
            this.content = new StringBuilder(content);
        }
    
        /// <summary>
        /// Constructs a Meta.
        /// </summary>
        /// <param name="tag">the tagname of the meta-information</param>
        /// <param name="content">the content</param>
        public Meta(string tag, string content) {
            this.type = Meta.GetType(tag);
            this.content = new StringBuilder(content);
        }
    
        // implementation of the Element-methods
    
        /// <summary>
        /// Processes the element by adding it (or the different parts) to a
        /// IElementListener.
        /// </summary>
        /// <param name="listener">the IElementListener</param>
        /// <returns>true if the element was processed successfully</returns>
        virtual public bool Process(IElementListener listener) {
            try {
                return listener.Add(this);
            }
            catch (DocumentException) {
                return false;
            }
        }
    
        /// <summary>
        /// Gets the type of the text element.
        /// </summary>
        /// <value>a type</value>
        virtual public int Type {
            get {
                return type;
            }
        }
    
        /// <summary>
        /// Gets all the chunks in this element.
        /// </summary>
        /// <value>an ArrayList</value>
        virtual public IList<Chunk> Chunks {
            get {
                return new List<Chunk>();
            }
        }
    
        /**
        * @see com.lowagie.text.Element#isContent()
        * @since   iText 2.0.8
        */
        virtual public bool IsContent() {
            return false;
        }

        /**
        * @see com.lowagie.text.Element#isNestable()
        * @since   iText 2.0.8
        */
        virtual public bool IsNestable() {
            return false;
        }

        // methods
    
        /// <summary>
        /// appends some text to this Meta.
        /// </summary>
        /// <param name="str">a string</param>
        /// <returns>a StringBuilder</returns>
        virtual public StringBuilder Append(string str) {
            return content.Append(str);
        }
    
        // methods to retrieve information
    
        /// <summary>
        /// Returns the content of the meta information.
        /// </summary>
        /// <value>a string</value>
        virtual public string Content {
            get {
                return content.ToString();
            }
        }
    
        /// <summary>
        /// Returns the name of the meta information.
        /// </summary>
        /// <value>a string</value>
        public virtual string Name {
            get {
                switch (type) {
                    case Element.SUBJECT:
                        return Meta.SUBJECT;
                    case Element.KEYWORDS:
                        return Meta.KEYWORDS;
                    case Element.AUTHOR:
                        return Meta.AUTHOR;
                    case Element.TITLE:
                        return Meta.TITLE;
                    case Element.PRODUCER:
                        return Meta.PRODUCER;
                    case Element.CREATIONDATE:
                        return Meta.CREATIONDATE;
                    default:
                        return Meta.UNKNOWN;
                }
            }
        }
    
        /// <summary>
        /// Returns the name of the meta information.
        /// </summary>
        /// <param name="tag">name to match</param>
        /// <returns>a string</returns>
        public static int GetType(string tag) {
            if (Meta.SUBJECT.Equals(tag)) {
                return Element.SUBJECT;
            }
            if (Meta.KEYWORDS.Equals(tag)) {
                return Element.KEYWORDS;
            }
            if (Meta.AUTHOR.Equals(tag)) {
                return Element.AUTHOR;
            }
            if (Meta.TITLE.Equals(tag)) {
                return Element.TITLE;
            }
            if (Meta.PRODUCER.Equals(tag)) {
                return Element.PRODUCER;
            }
            if (Meta.CREATIONDATE.Equals(tag)) {
                return Element.CREATIONDATE;
            }
            return Element.HEADER;
        }
    
    
        public override string ToString() {
            return base.ToString();
        }
    }
}
