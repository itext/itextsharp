using System;
using System.Text;
using System.Collections.Generic;
using iTextSharp.text.pdf;
using iTextSharp.text.error_messages;

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

namespace iTextSharp.text {
    /// <summary>
    /// A Phrase is a series of Chunks.
    /// </summary>
    /// <remarks>
    /// A Phrase has a main Font, but some chunks
    /// within the phrase can have a Font that differs from the
    /// main Font. All the Chunks in a Phrase
    /// have the same leading.
    /// </remarks>
    /// <example>
    /// <code>
    /// // When no parameters are passed, the default leading = 16
    /// <strong>Phrase phrase0 = new Phrase();
    /// Phrase phrase1 = new Phrase("this is a phrase");</strong>
    /// // In this example the leading is passed as a parameter
    /// <strong>Phrase phrase2 = new Phrase(16, "this is a phrase with leading 16");</strong>
    /// // When a Font is passed (explicitely or embedded in a chunk), the default leading = 1.5 * size of the font
    /// <strong>Phrase phrase3 = new Phrase("this is a phrase with a red, normal font Courier, size 12", FontFactory.GetFont(FontFactory.COURIER, 12, Font.NORMAL, new Color(255, 0, 0)));
    /// Phrase phrase4 = new Phrase(new Chunk("this is a phrase"));
    /// Phrase phrase5 = new Phrase(18, new Chunk("this is a phrase", FontFactory.GetFont(FontFactory.HELVETICA, 16, Font.BOLD, new Color(255, 0, 0)));</strong>
    /// </code>
    /// </example>
    public class Phrase : List<IElement>, ITextElementArray {
    
        // membervariables
    
        /// <summary>This is the leading of this phrase.</summary>
        protected Single leading = Single.NaN;

        /** The text leading that is multiplied by the biggest font size in the line. */
        protected float multipliedLeading = 0;
    
        ///<summary> This is the font of this phrase. </summary>
        protected Font font;

        /** Null, unless the Phrase has to be hyphenated.
        * @since   2.1.2
        */
        protected IHyphenationEvent hyphenation = null;

        /**
         * Predefined tab position and properties(alignment, leader and etc.);
         * @since	5.4.1
         */
        protected TabSettings tabSettings = null;


        // constructors
    
        /// <summary>
        /// Constructs a Phrase without specifying a leading.
        /// </summary>
        /// <overloads>
        /// Has nine overloads.
        /// </overloads>
        public Phrase() : this(16) {}
    
        /**
        * Copy constructor for <CODE>Phrase</CODE>.
        */
        public Phrase(Phrase phrase) : base() {
            this.AddAll(phrase);
            SetLeading(phrase.Leading, phrase.MultipliedLeading);
            font = phrase.Font;
            tabSettings = phrase.TabSettings;
            hyphenation = phrase.hyphenation;
        }

        /// <summary>
        /// Constructs a Phrase with a certain leading.
        /// </summary>
        /// <param name="leading">the leading</param>
        public Phrase(float leading) {
            this.leading = leading;
            font = new Font();
        }
    
        /// <summary>
        /// Constructs a Phrase with a certain Chunk.
        /// </summary>
        /// <param name="chunk">a Chunk</param>
        public Phrase(Chunk chunk) {
            base.Add(chunk);
            font = chunk.Font;
            hyphenation = chunk.GetHyphenation();
        }
    
        /// <summary>
        /// Constructs a Phrase with a certain Chunk and a certain leading.
        /// </summary>
        /// <param name="leading">the leading</param>
        /// <param name="chunk">a Chunk</param>
        public Phrase(float leading, Chunk chunk) {
            this.leading = leading;
            base.Add(chunk);
            font = chunk.Font;
            hyphenation = chunk.GetHyphenation();
        }
    
        /// <summary>
        /// Constructs a Phrase with a certain string.
        /// </summary>
        /// <param name="str">a string</param>
        public Phrase(string str) : this(float.NaN, str, new Font()) {}
    
        /// <summary>
        /// Constructs a Phrase with a certain string and a certain Font.
        /// </summary>
        /// <param name="str">a string</param>
        /// <param name="font">a Font</param>
        public Phrase(string str, Font font) : this(float.NaN, str, font) {
        }
    
        /// <summary>
        /// Constructs a Phrase with a certain leading and a certain string.
        /// </summary>
        /// <param name="leading">the leading</param>
        /// <param name="str">a string</param>
        public Phrase(float leading, string str) : this(leading, str, new Font()) {}
    
        public Phrase(float leading, string str, Font font) {
            this.leading = leading;
            this.font = font;
            /* bugfix by August Detlefsen */
            if (!string.IsNullOrEmpty(str)) {
                base.Add(new Chunk(str, font));
            }
        }
        
        // implementation of the Element-methods
    
        /// <summary>
        /// Processes the element by adding it (or the different parts) to an
        /// <see cref="iTextSharp.text.IElementListener"/>.
        /// </summary>
        /// <param name="listener">an IElementListener</param>
        /// <returns>true if the element was processed successfully</returns>
        public virtual bool Process(IElementListener listener) {
            try {
                foreach (IElement ele in this) {
                    listener.Add(ele);
                }
                return true;
            }
            catch (DocumentException) {
                return false;
            }
        }
    
        /// <summary>
        /// Gets the type of the text element.
        /// </summary>
        /// <value>a type</value>
        public virtual int Type {
            get {
                return Element.PHRASE;
            }
        }
    
        /// <summary>
        /// Gets all the chunks in this element.
        /// </summary>
        /// <value>an ArrayList</value>
        public virtual IList<Chunk> Chunks {
            get {
                List<Chunk> tmp = new List<Chunk>();
                foreach (IElement ele in this) {
                    tmp.AddRange(ele.Chunks);
                }
                return tmp;
            }
        }
    
        /**
        * @see com.lowagie.text.Element#isContent()
        * @since   iText 2.0.8
        */
        virtual public bool IsContent() {
            return true;
        }

        /**
        * @see com.lowagie.text.Element#isNestable()
        * @since   iText 2.0.8
        */
        virtual public bool IsNestable() {
            return true;
        }

        // overriding some of the ArrayList-methods
    
        /// <summary>
        /// Adds a Chunk, an Anchor or another Phrase
        /// to this Phrase.
        /// </summary>
        /// <param name="index">index at which the specified element is to be inserted</param>
        /// <param name="o">an object of type Chunk, Anchor, or Phrase</param>
        public virtual void Add(int index, IElement element) {
            if (element == null) return;
            switch (element.Type) {
                case Element.CHUNK:
                    Chunk chunk = (Chunk)element;
                    if (!font.IsStandardFont()) {
                        chunk.Font = font.Difference(chunk.Font);
                    }
                    if (hyphenation != null && chunk.GetHyphenation() == null && !chunk.IsEmpty()) {
                        chunk.SetHyphenation(hyphenation);
                    }
                    base.Insert(index, chunk);
                    return;
                case Element.PHRASE:
                case Element.PARAGRAPH:
                case Element.MARKED:
                case Element.DIV:
                case Element.ANCHOR:
                case Element.ANNOTATION:
                case Element.PTABLE:
                case Element.LIST:
                case Element.YMARK:
                case Element.WRITABLE_DIRECT:
                    base.Insert(index, element);
                    return;
                default:
                    throw new Exception(MessageLocalization.GetComposedMessage("insertion.of.illegal.element.1", element.Type.ToString()));
            }
        }
    
        /**
         * Adds a <CODE>String</CODE> to this <CODE>Phrase</CODE>.
         *
         * @param   s       a string
         * @return  a boolean
         * @since 5.0.1
         */
        virtual public bool Add(String s) {
            if (s == null) {
                return false;
            }
            base.Add(new Chunk(s, font));
            return true;
        }

        /// <summary>
        /// Adds a Chunk, Anchor or another Phrase
        /// to this Phrase.
        /// </summary>
        /// <param name="o">an object of type Chunk, Anchor or Phrase</param>
        /// <returns>a bool</returns>
        public virtual new bool Add(IElement element) {
            if (element == null) return false;
            try {
                switch (element.Type) {
                    case Element.CHUNK:
                        return AddChunk((Chunk)element);
                    case Element.PHRASE:
                    case Element.PARAGRAPH:
                        Phrase phrase = (Phrase)element;
                        bool success = true;
                        foreach (IElement e in phrase) {
                            if (e is Chunk) {
                                success &= AddChunk((Chunk)e);
                            }
                            else {
                                success &= this.Add(e);
                            }
                        }
                        return success;
                    case Element.MARKED:
                    case Element.DIV:
                    case Element.ANCHOR:
                    case Element.ANNOTATION:
                    case Element.PTABLE: // case added by Karen Vardanyan
                    case Element.LIST:
                    case Element.YMARK:
                    case Element.WRITABLE_DIRECT:
                        base.Add(element);
                        return true;
                    default:
                        throw new Exception(element.Type.ToString());
                }
            }
            catch (Exception cce) {
                throw new Exception(MessageLocalization.GetComposedMessage("insertion.of.illegal.element.1", cce.Message));
            }
        }
    
        /// <summary>
        /// Adds a collection of Chunks
        /// to this Phrase.
        /// </summary>
        /// <param name="collection">a collection of Chunks, Anchors and Phrases.</param>
        /// <returns>true if the action succeeded, false if not.</returns>
        virtual public bool AddAll<T>(ICollection<T> collection) where T : IElement {
            if (collection.Count == 0)
                return false;
            foreach (IElement itm in collection) {
                this.Add(itm);
            }
            return true;
        }
    
        /// <summary>
        /// Adds a Chunk.
        /// </summary>
        /// <remarks>
        /// This method is a hack to solve a problem I had with phrases that were split between chunks
        /// in the wrong place.
        /// </remarks>
        /// <param name="chunk">a Chunk</param>
        /// <returns>a bool</returns>
        virtual protected bool AddChunk(Chunk chunk) {
    	    Font f = chunk.Font;
    	    String c = chunk.Content;
            if (font != null && !font.IsStandardFont()) {
                f = font.Difference(chunk.Font);
            }
            if (Count > 0 && !chunk.HasAttributes()) {
                try {
                    Chunk previous = (Chunk) this[Count - 1];
                    if (!previous.HasAttributes() && !chunk.HasAccessibleAttributes()
                            && (f == null
                            || f.CompareTo(previous.Font) == 0)
                            && previous.Font.CompareTo(f) == 0
                            && !"".Equals(previous.Content.Trim())
                            && !"".Equals(c.Trim())) {
                        previous.Append(c);
                        return true;
                    }
                }
                catch {
                }
            }
            Chunk newChunk = new Chunk(c, f);
            newChunk.Attributes = chunk.Attributes;
            newChunk.role = chunk.Role;
            newChunk.accessibleAttributes = chunk.GetAccessibleAttributes();
            if (hyphenation != null && newChunk.GetHyphenation() == null && !newChunk.IsEmpty()) {
                newChunk.SetHyphenation(hyphenation);
            }
            base.Add(newChunk);
            return true;
        }
    
        /// <summary>
        /// Adds a Object to the Paragraph.
        /// </summary>
        /// <param name="obj">the object to add.</param>
        virtual public void AddSpecial(IElement obj) {
            base.Add(obj);
        }
    
        // methods
    
        // methods to retrieve information
    
        /// <summary>
        /// Checks is this Phrase contains no or 1 empty Chunk.
        /// </summary>
        /// <returns>
        /// false if the Phrase
        /// contains more than one or more non-emptyChunks.
        /// </returns>
        virtual public bool IsEmpty() {
            switch (Count) {
                case 0:
                    return true;
                case 1:
                    IElement element = this[0];
                    if (element.Type == Element.CHUNK && ((Chunk) element).IsEmpty()) {
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }
    
        virtual public bool HasLeading() {
            if (float.IsNaN(leading)) {
                return false;
            }
            return true;
        }

        /**
         * Sets the leading fixed and variable. The resultant leading will be
         * <ul>
         * <li>If Phrase is added to the ColumnText: fixedLeading+multipliedLeading*maxFontSize, where maxFontSize is the size of the biggest font in the line;<li/>
         * <li>If Phrase is added to the PdfDocument: fixedLeading+multipliedLeading*phraseFontSize, where phraseFontSize is the size of the font applied to the current phrase.<li/>
         * <ul/>
         * @param fixedLeading the fixed leading
         * @param multipliedLeading the variable leading
         */
        virtual public void SetLeading(float fixedLeading, float multipliedLeading) {
            this.leading = fixedLeading;
            this.multipliedLeading = multipliedLeading;
        }

        /**
         * Gets the variable leading
         * @return the leading
         */
        /**
         * Sets the variable leading. The resultant leading will be
         * <ul>
         * <li>If Phrase is added to the ColumnText: fixedLeading+multipliedLeading*maxFontSize, where maxFontSize is the size of the biggest font in the line;<li/>
         * <li>If Phrase is added to the PdfDocument: fixedLeading+multipliedLeading*phraseFontSize, where phraseFontSize is the size of the font applied to the current phrase.<li/>
         * <ul/>
         * @param multipliedLeading the variable leading
         */
        virtual public float MultipliedLeading {
            get {
                return multipliedLeading;
            }
            set {
                this.leading = 0;
                this.multipliedLeading = value;
            }
        }

        /// <summary>
        /// Gets/sets the leading of this phrase.
        /// </summary>
        /// <value>the linespacing</value>
        public virtual float Leading {
            get {
                if (float.IsNaN(leading) && font != null) {
                    return font.GetCalculatedLeading(1.5f);
                }
                return leading;
            }
            set {
                this.leading = value;
                this.multipliedLeading = 0;
            }
        }

        /**
         * Gets the total leading.
         * This method is based on the assumption that the
         * font of the Paragraph is the font of all the elements
         * that make part of the paragraph. This isn't necessarily
         * true.
         * @return the total leading (fixed and multiplied)
         */
        public virtual float TotalLeading {
            get {
                float m = font == null ? Font.DEFAULTSIZE*multipliedLeading : font.GetCalculatedLeading(multipliedLeading);
                if (m > 0 && !HasLeading()) {
                    return m;
                }
                return Leading + m;
            }
        }

        /// <summary>
        /// Gets the font of the first Chunk that appears in this Phrase.
        /// </summary>
        /// <value>a Font</value>
        virtual public Font Font {
            get {
                return font;
            }
            set {
                font = value;
            }
        }
    
        /**
        * Returns the content as a String object.
        * This method differs from toString because toString will return an ArrayList with the toString value of the Chunks in this Phrase.
        */
        virtual public String Content {
            get {
    	        StringBuilder buf = new StringBuilder();
                foreach (Chunk obj in Chunks)
    		        buf.Append(obj.ToString());
    	        return buf.ToString();
            }
        }

        /**
        * Setter/getter for the hyphenation.
        * @param   hyphenation a HyphenationEvent instance
        * @since   2.1.2
        */
        virtual public IHyphenationEvent Hyphenation {
            set {
                hyphenation = value;
            }
            get {
                return hyphenation;
            }
        }

        /**
        * Setter/getter for the tabSettings.
        * @param   tabSettings a TabSettings instance
        * @since   5.4.1
        */

        virtual public TabSettings TabSettings
        {
            get { return tabSettings; }
            set { tabSettings = value; }
        }

        // kept for historical reasons; people should use FontSelector
        // eligable for deprecation, but the methods are mentioned in the book p277.
        
        /**
        * Constructs a Phrase that can be used in the static GetInstance() method.
        * @param	dummy	a dummy parameter
        */
        private Phrase(bool dummy) {
        }
        
        /**
        * Gets a special kind of Phrase that changes some characters into corresponding symbols.
        * @param string
        * @return a newly constructed Phrase
        */
        public static Phrase GetInstance(String str) {
    	    return GetInstance(16, str, new Font());
        }
        
        /**
        * Gets a special kind of Phrase that changes some characters into corresponding symbols.
        * @param leading
        * @param string
        * @return a newly constructed Phrase
        */
        public static Phrase GetInstance(int leading, String str) {
    	    return GetInstance(leading, str, new Font());
        }
        
        /**
        * Gets a special kind of Phrase that changes some characters into corresponding symbols.
        * @param leading
        * @param string
        * @param font
        * @return a newly constructed Phrase
        */
        public static Phrase GetInstance(int leading, String str, Font font) {
    	    Phrase p = new Phrase(true);
    	    p.Leading = leading;
    	    p.font = font;
    	    if (font.Family != Font.FontFamily.SYMBOL && font.Family != Font.FontFamily.ZAPFDINGBATS && font.BaseFont == null) {
                int index;
                while ((index = SpecialSymbol.Index(str)) > -1) {
                    if (index > 0) {
                        String firstPart = str.Substring(0, index);
                        p.Add(new Chunk(firstPart, font));
                        str = str.Substring(index);
                    }
                    Font symbol = new Font(Font.FontFamily.SYMBOL, font.Size, font.Style, font.Color);
                    StringBuilder buf = new StringBuilder();
                    buf.Append(SpecialSymbol.GetCorrespondingSymbol(str[0]));
                    str = str.Substring(1);
                    while (SpecialSymbol.Index(str) == 0) {
                        buf.Append(SpecialSymbol.GetCorrespondingSymbol(str[0]));
                        str = str.Substring(1);
                    }
                    p.Add(new Chunk(buf.ToString(), symbol));
                }
            }
            if (!string.IsNullOrEmpty(str)) {
        	    p.Add(new Chunk(str, font));
            }
    	    return p;
        }

        virtual public bool Trim() {
            while (this.Count > 0) {
                IElement firstChunk = this[0];
                if (firstChunk is Chunk && ((Chunk)firstChunk).IsWhitespace()) {
                    this.Remove(firstChunk);
                } else {
                    break;
                }
            }
            while (this.Count > 0) {
                IElement lastChunk = this[this.Count - 1];
                if (lastChunk is Chunk && ((Chunk)lastChunk).IsWhitespace()) {
                    this.Remove(lastChunk);
                } else {
                    break;
                }
            }
            return Count > 0;
        }
    }
}
