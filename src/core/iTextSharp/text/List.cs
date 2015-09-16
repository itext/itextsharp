using System;
using System.Collections.Generic;
using iTextSharp.text.api;
using iTextSharp.text.factories;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.interfaces;

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
    /// A List contains several ListItems.
    /// </summary>
    /// <example>
    /// <B>Example 1:</B>
    /// <code>
    /// <strong>List list = new List(true, 20);
    /// list.Add(new ListItem("First line"));
    /// list.Add(new ListItem("The second line is longer to see what happens once the end of the line is reached. Will it start on a new line?"));
    /// list.Add(new ListItem("Third line"));</strong>
    /// </code>
    /// 
    /// The result of this code looks like this:
    /// <OL>
    ///     <LI>
    ///         First line
    ///     </LI>
    ///     <LI>
    ///         The second line is longer to see what happens once the end of the line is reached. Will it start on a new line?
    ///     </LI>
    ///     <LI>
    ///         Third line
    ///     </LI>
    /// </OL>
    /// 
    /// <B>Example 2:</B>
    /// <code>
    /// <strong>List overview = new List(false, 10);
    /// overview.Add(new ListItem("This is an item"));
    /// overview.Add("This is another item");</strong>
    /// </code>
    /// 
    /// The result of this code looks like this:
    /// <UL>
    ///        <LI>
    ///            This is an item
    ///        </LI>
    ///        <LI>
    ///            This is another item
    ///        </LI>
    ///    </UL>
    /// </example>
    /// <seealso cref="T:iTextSharp.text.Element"/>
    /// <seealso cref="T:iTextSharp.text.ListItem"/>
    public class List : ITextElementArray, IIndentable, IAccessibleElement {
    
        // membervariables
        /** a possible value for the numbered parameter */
        public const bool ORDERED = true;
        /** a possible value for the numbered parameter */
        public const bool UNORDERED = false;
        /** a possible value for the lettered parameter */
        public const bool NUMERICAL = false;
        /** a possible value for the lettered parameter */
        public const bool ALPHABETICAL = true;
        /** a possible value for the lettered parameter */
        public const bool UPPERCASE = false;
        /** a possible value for the lettered parameter */
        public const bool LOWERCASE = true;
    
        /// <summary> This is the ArrayList containing the different ListItems. </summary>
        protected List<IElement> list = new List<IElement>();
    
        /** Indicates if the list has to be numbered. */
        protected bool numbered = false;
        /** Indicates if the listsymbols are numerical or alphabetical. */
        protected bool lettered = false;
        /** Indicates if the listsymbols are lowercase or uppercase. */
        protected bool lowercase = false;
        /** Indicates if the indentation has to be set automatically. */
        protected bool autoindent = false;
        /** Indicates if the indentation of all the items has to be aligned. */
        protected bool alignindent = false;
    
        /// <summary> This variable indicates the first number of a numbered list. </summary>
        protected int first = 1;
    
        /// <summary> This is the listsymbol of a list that is not numbered. </summary>
        protected Chunk symbol = new Chunk("-");
        /**
        * In case you are using numbered/lettered lists, this String is added before the number/letter.
        * @since   iText 2.1.1
        */
        protected String preSymbol = "";
        /**
        * In case you are using numbered/lettered lists, this String is added after the number/letter. 
        * @since   iText 2.1.1
        */
        protected String postSymbol = ". ";
    
        /// <summary> The indentation of this list on the left side. </summary>
        protected float indentationLeft = 0;
    
        /// <summary> The indentation of this list on the right side. </summary>
        protected float indentationRight = 0;
    
        /// <summary> The indentation of the listitems. </summary>
        protected float symbolIndent = 0;

        protected PdfName role = PdfName.L;
        protected Dictionary<PdfName, PdfObject> accessibleAttributes = null;
        protected AccessibleElementId id = new AccessibleElementId();

        // constructors
        
        /**
        * Constructs a <CODE>List</CODE>.
        */
        public List() : this(false, false) {
        }
        
        /**
        * Constructs a <CODE>List</CODE> with a specific symbol indentation.
        * @param   symbolIndent    the symbol indentation
        * @since   iText 2.0.8
        */
        public List(float symbolIndent) {
            this.symbolIndent = symbolIndent;
        }

        /**
        * Constructs a <CODE>List</CODE>.
        *
        * @param    numbered        a bool
        */
        public List(bool numbered) : this(numbered, false) {
        }
            
        /**
        * Constructs a <CODE>List</CODE>.
        *
        * @param    numbered        a bool
        * @param lettered has the list to be 'numbered' with letters
        */
        public List(bool numbered, bool lettered) {
            this.numbered = numbered;
            this.lettered = lettered;
            this.autoindent = true;
            this.alignindent = true;
        }
        

        /// <summary>
        /// Constructs a List.
        /// </summary>
        /// <remarks>
        /// the parameter symbolIndent is important for instance when
        /// generating PDF-documents; it indicates the indentation of the listsymbol.
        /// </remarks>
        /// <param name="numbered">a bool</param>
        /// <param name="symbolIndent">the indentation that has to be used for the listsymbol</param>
        public List(bool numbered, float symbolIndent) : this(numbered, false, symbolIndent) {
        }
    
        /// <summary>
        /// Constructs a List.
        /// </summary>
        /// <param name="numbered">a bool</param>
        /// <param name="lettered">a bool</param>
        /// <param name="symbolIndent">the indentation that has to be used for the listsymbol</param>
        public List(bool numbered, bool lettered, float symbolIndent ) {
            this.numbered = numbered;
            this.lettered = lettered;
            this.symbolIndent = symbolIndent;
        }
    
        // implementation of the Element-methods
    
        /// <summary>
        /// Processes the element by adding it (or the different parts) to an
        /// IElementListener.
        /// </summary>
        /// <param name="listener">an IElementListener</param>
        /// <returns>true if the element was processed successfully</returns>
        virtual public bool Process(IElementListener listener) {
            try {
                foreach (IElement ele in list) {
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
        virtual public int Type {
            get {
                return Element.LIST;
            }
        }
    
        /// <summary>
        /// Gets all the chunks in this element.
        /// </summary>
        /// <value>an ArrayList</value>
        virtual public IList<Chunk> Chunks {
            get {
                List<Chunk> tmp = new List<Chunk>();
                foreach (IElement ele in list) {
                    tmp.AddRange(ele.Chunks);
                }
                return tmp;
            }
        }
    
        // methods to set the membervariables
    
        public virtual bool Add(string s) {
            if (s != null)
                return this.Add(new ListItem(s));
            return false;
        }

        /// <summary>
        /// Adds an Object to the List.
        /// </summary>
        /// <param name="o">the object to add</param>
        /// <returns>true is successful</returns>
        public virtual bool Add(IElement o) {
            if (o is ListItem) {
                ListItem item = (ListItem) o;
                if (numbered || lettered) {
                    Chunk chunk = new Chunk(preSymbol, symbol.Font);
                    chunk.Attributes = symbol.Attributes;
                    int index = first + list.Count;
                    if (lettered)
                        chunk.Append(RomanAlphabetFactory.GetString(index, lowercase));
                    else
                        chunk.Append(index.ToString());
                    chunk.Append(postSymbol);
                    item.ListSymbol = chunk;
                }
                else {
                    item.ListSymbol = symbol;
                }
                item.SetIndentationLeft(symbolIndent, autoindent);
                item.IndentationRight = 0;
                list.Add(item);
                return true;
            }
            else if (o is List) {
                List nested = (List) o;
                nested.IndentationLeft = nested.IndentationLeft + symbolIndent;
                first--;
                list.Add(nested);
                return true;
            }
            return false;
        }

        public virtual List CloneShallow() {
            List clone = new List();
            PopulateProperties(clone);
            return clone;
        }

        protected void PopulateProperties(List clone) {
            clone.indentationLeft = indentationLeft;
            clone.indentationRight = indentationRight;
            clone.autoindent = autoindent;
            clone.alignindent = alignindent;
            clone.symbolIndent = symbolIndent;
            clone.symbol = symbol;
        }
    
        // extra methods
        
        /** Makes sure all the items in the list have the same indentation. */
        virtual public void NormalizeIndentation() {
            float max = 0;
            foreach (IElement o in list) {
                if (o is ListItem) {
                    max = Math.Max(max, ((ListItem)o).IndentationLeft);
                }
            }
            foreach (IElement o in list) {
                if (o is ListItem) {
                    ((ListItem)o).IndentationLeft = max;
                }
            }
        }

        //setters/getters

        virtual public bool Numbered {
            set {
                numbered = value;
            }
            get {
                return numbered;
            }
        }

        virtual public bool Lettered {
            set {
                lettered = value;
            }
            get {
                return lettered;
            }
        }

        virtual public bool Lowercase {
            set {
                lowercase = value;
            }
            get {
                return lowercase;
            }
        }

        /// <summary>
        /// Alias for VB.NET compatibility.
        /// </summary>
        virtual public bool IsLowercase {
            set {
                lowercase = value;
            }
            get {
                return lowercase;
            }
        }

        virtual public bool Autoindent {
            set {
                autoindent = value;
            }
            get {
                return autoindent;
            }
        }

        virtual public bool Alignindent {
            set {
                alignindent = value;
            }
            get {
                return alignindent;
            }
        }

        /// <summary>
        /// Get/set the first number
        /// </summary>
        /// <value>an int</value>
        virtual public int First {
            get {
                return first;
            }

            set {
                this.first = value;
            }
        }
    
        /// <summary>
        /// Sets the symbol
        /// </summary>
        /// <value>a Chunk</value>
        virtual public Chunk ListSymbol {
            set {
                this.symbol = value;
            }
        }

        /// <summary>
        /// Sets the listsymbol.
        /// </summary>
        /// <remarks>
        /// This is a shortcut for SetListSymbol(Chunk symbol).
        /// </remarks>
        /// <param name="symbol">a string</param>
        virtual public void SetListSymbol(string symbol) {
            this.symbol = new Chunk(symbol);
        }
    
        /// <summary>
        /// Get/set the indentation of this paragraph on the left side.
        /// </summary>
        /// <value>the indentation</value>
        virtual public float IndentationLeft {
            get {
                return indentationLeft;
            }

            set {
                this.indentationLeft = value;
            }
        }
    
        /// <summary>
        /// Get/set the indentation of this paragraph on the right side.
        /// </summary>
        /// <value>the indentation</value>
        virtual public float IndentationRight {
            get {
                return indentationRight;
            }

            set {
                this.indentationRight = value;
            }
        }
    
        /// <summary>
        /// Gets the symbol indentation.
        /// </summary>
        /// <value>the symbol indentation</value>
        virtual public float SymbolIndent {
            set {
                symbolIndent = value;
            }
            get {
                return symbolIndent;
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

        // methods to retrieve information
    
        /// <summary>
        /// Gets all the items in the list.
        /// </summary>
        /// <value>an ArrayList containing ListItems</value>
        virtual public List<IElement> Items {
            get {
                return list;
            }
        }
    
        /// <summary>
        /// Gets the size of the list.
        /// </summary>
        /// <value>a size</value>
        virtual public int Size {
            get {
                return list.Count;
            }
        }
    
        /**
        * Returns <CODE>true</CODE> if the list is empty.
        * 
        * @return <CODE>true</CODE> if the list is empty
        */
        public virtual bool IsEmpty() {
    	    return list.Count == 0;
        }

        /// <summary>
        /// Gets the leading of the first listitem.
        /// </summary>
        /// <value>a leading</value>
        virtual public float TotalLeading {
            get {
                if (list.Count < 1) {
                    return -1;
                }
                ListItem item = (ListItem)list[0];
                return item.TotalLeading;
            }
        }
    
        /// <summary>
        /// Get/set the symbol indentation.
        /// </summary>
        /// <value>a Chunk</value>
        virtual public Chunk Symbol {
            get {
                return symbol;
            }
            set {
                this.symbol = value;
            }
        }

	    /**
	    * Returns the String that is after a number or letter in the list symbol.
	    * @return	the String that is after a number or letter in the list symbol
	    * @since	iText 2.1.1
	    */
	    virtual public String getPostSymbol() {
		    return postSymbol;
	    }

	    /**
	    * Sets the String that has to be added after a number or letter in the list symbol.
	    * @since	iText 2.1.1
	    * @param	postSymbol the String that has to be added after a number or letter in the list symbol.
	    */
	    virtual public String PostSymbol {
            set {
		        postSymbol = value;
            }
            get {
                return postSymbol;
            }
	    }

	    /**
	    * Sets the String that has to be added before a number or letter in the list symbol.
	    * @since	iText 2.1.1
	    * @param	preSymbol the String that has to be added before a number or letter in the list symbol.
	    */
	    virtual public String PreSymbol {
            set {
		        preSymbol = value;
            }
            get {
                return preSymbol;
            }
	    }

        virtual public ListItem GetFirstItem() {
            IElement lastElement = list.Count > 0 ? list[0] : null;
            if (lastElement != null) {
                if (lastElement is ListItem) {
                    return (ListItem)lastElement;
                } else if (lastElement is List) {
                    return ((List)lastElement).GetFirstItem();
                }
            }
            return null;
        }

        virtual public ListItem GetLastItem() {
            IElement lastElement = list.Count > 0 ? list[list.Count - 1] : null;
            if (lastElement != null) {
                if (lastElement is ListItem) {
                    return (ListItem)lastElement;
                } else if (lastElement is List) {
                    return ((List)lastElement).GetLastItem();
                }
            }
            return null;
        }

        virtual public PdfObject GetAccessibleAttribute(PdfName key) {
            if (accessibleAttributes != null)
            {
                PdfObject obj;
                accessibleAttributes.TryGetValue(key, out obj);
                return obj;
            }
            else
                return null;
        }

        virtual public void SetAccessibleAttribute(PdfName key, PdfObject value) {
            if (accessibleAttributes == null)
                accessibleAttributes = new Dictionary<PdfName, PdfObject>();
            accessibleAttributes[key] = value;
        }

        virtual public Dictionary<PdfName, PdfObject> GetAccessibleAttributes() {
            return accessibleAttributes;
        }

        public virtual PdfName Role {
            get { return role; }
            set { this.role = value; }
        }

        public virtual AccessibleElementId ID {
            get {
                if (id == null)
                    id = new AccessibleElementId();
                return id;
            }
            set { id = value; }
        }

        public virtual bool IsInline {
            get { return false; }
        }
    }
}
