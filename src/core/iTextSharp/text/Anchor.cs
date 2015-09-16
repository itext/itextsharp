using System;
using System.Collections.Generic;
using System.util;

using iTextSharp.text.html;
using iTextSharp.text.factories;

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

namespace iTextSharp.text 
{
    /// <summary>
    /// An Anchor can be a reference or a destination of a reference.
    /// </summary>
    /// <remarks>
    /// An Anchor is a special kind of <see cref="T:iTextSharp.text.Phrase"/>.
    /// It is constructed in the same way.
    /// </remarks>
    /// <seealso cref="T:iTextSharp.text.Element"/>
    /// <seealso cref="T:iTextSharp.text.Phrase"/>
    public class Anchor : Phrase 
    {
    
        // membervariables
    
        /// <summary>
        /// This is the name of the Anchor.
        /// </summary>
        protected string name = null;
    
        /// <summary>
        /// This is the reference of the Anchor.
        /// </summary>
        protected string reference = null;
    
        // constructors
    
        /// <summary>
        /// Constructs an Anchor without specifying a leading.
        /// </summary>
        /// <overloads>
        /// Has nine overloads.
        /// </overloads>
        public Anchor() : base(16) {}
    
        /// <summary>
        /// Constructs an Anchor with a certain leading.
        /// </summary>
        /// <param name="leading">the leading</param>
        public Anchor(float leading) : base(leading) {}
    
        /// <summary>
        /// Constructs an Anchor with a certain Chunk.
        /// </summary>
        /// <param name="chunk">a Chunk</param>
        public Anchor(Chunk chunk) : base(chunk) {}
    
        /// <summary>
        /// Constructs an Anchor with a certain string.
        /// </summary>
        /// <param name="str">a string</param>
        public Anchor(string str) : base(str) {}
    
        /// <summary>
        /// Constructs an Anchor with a certain string
        /// and a certain Font.
        /// </summary>
        /// <param name="str">a string</param>
        /// <param name="font">a Font</param>
        public Anchor(string str, Font font) : base(str, font) {}
    
        /// <summary>
        /// Constructs an Anchor with a certain Chunk
        /// and a certain leading.
        /// </summary>
        /// <param name="leading">the leading</param>
        /// <param name="chunk">a Chunk</param>
        public Anchor(float leading, Chunk chunk) : base(leading, chunk) {}
    
        /// <summary>
        /// Constructs an Anchor with a certain leading
        /// and a certain string.
        /// </summary>
        /// <param name="leading">the leading</param>
        /// <param name="str">a string</param>
        public Anchor(float leading, string str) : base(leading, str) {}
    
        /// <summary>
        /// Constructs an Anchor with a certain leading,
        /// a certain string and a certain Font.
        /// </summary>
        /// <param name="leading">the leading</param>
        /// <param name="str">a string</param>
        /// <param name="font">a Font</param>
        public Anchor(float leading, string str, Font font) : base(leading, str, font) {}
    
        /**
        * Constructs an <CODE>Anchor</CODE> with a certain <CODE>Phrase</CODE>.
        *
        * @param   phrase      a <CODE>Phrase</CODE>
        */    
        public Anchor(Phrase phrase) : base(phrase) {
            if (phrase is Anchor) {
                Anchor a = (Anchor) phrase;
                Name = a.name;
                Reference = a.reference;
            }
        }
        // implementation of the Element-methods
    
        /// <summary>
        /// Processes the element by adding it (or the different parts) to an
        /// <see cref="T:iTextSharp.text.IElementListener"/>
        /// </summary>
        /// <param name="listener">an IElementListener</param>
        /// <returns>true if the element was processed successfully</returns>
        public override bool Process(IElementListener listener) 
        {
            try 
            {
                bool localDestination = (reference != null && reference.StartsWith("#"));
                bool notGotoOK = true;
                foreach (Chunk chunk in this.Chunks) 
                {
                    if (name != null && notGotoOK && !chunk.IsEmpty()) 
                    {
                        chunk.SetLocalDestination(name);
                        notGotoOK = false;
                    }
                    if (localDestination) 
                    {
                        chunk.SetLocalGoto(reference.Substring(1));
                    }
                    else if (reference != null)
                        chunk.SetAnchor(reference);
                    listener.Add(chunk);
                }
                return true;
            }
            catch (DocumentException) 
            {
                return false;
            }
        }
    
        /// <summary>
        /// Gets all the chunks in this element.
        /// </summary>
        /// <value>an ArrayList</value>
        public override IList<Chunk> Chunks 
        {
            get
            {
                List<Chunk> tmp = new List<Chunk>();
                bool localDestination = (reference != null && reference.StartsWith("#"));
                bool notGotoOK = true;
                foreach (IElement element in this) {
                    if (element is Chunk) {
                        Chunk chunk = (Chunk) element;
                        notGotoOK = ApplyAnchor(chunk, notGotoOK, localDestination);
                        tmp.Add(chunk);
                    } else {
                        foreach (Chunk c in element.Chunks) {
                            notGotoOK = ApplyAnchor(c, notGotoOK, localDestination);
                            tmp.Add(c);
                        }
                    }
                }

                return tmp;
            }
        }


        /**
         * Applies the properties of the Anchor to a Chunk.
         * @param chunk			the Chunk (part of the Anchor)
         * @param notGotoOK		if true, this chunk will determine the local destination
         * @param localDestination	true if the chunk is a local goto and the reference a local destination
         * @return	the value of notGotoOK or false, if a previous Chunk was used to determine the local destination
         */
        virtual protected bool ApplyAnchor(Chunk chunk, bool notGotoOK, bool localDestination) {
            if (name != null && notGotoOK && !chunk.IsEmpty()) {
                chunk.SetLocalDestination(name);
                notGotoOK = false;
            }
            if (localDestination) {
                chunk.SetLocalGoto(reference.Substring(1));
            } else if (reference != null)
                chunk.SetAnchor(reference);
            return notGotoOK;
        }
    
        /// <summary>
        /// Gets the type of the text element.
        /// </summary>
        /// <value>a type</value>
        public override int Type 
        {
            get 
            {
                return Element.ANCHOR;
            }
        }
    
        // methods
    
        /// <summary>
        /// Name of this Anchor.
        /// </summary>
        virtual public string Name {
            get {
                return this.name;
            }

            set {
                this.name = value;
            }
        }
    
        // methods to retrieve information
    
        /// <summary>
        /// reference of this Anchor.
        /// </summary>
        virtual public string Reference {
            get {
                return reference;
            }

            set {
                this.reference = value;
            }
        }
    
        /// <summary>
        /// reference of this Anchor.
        /// </summary>
        /// <value>an Uri</value>
        virtual public Uri Url {
            get {
                try {
                    return new Uri(reference);
                }
                catch {
                    return null;
                }
            }
        }
    }
}
