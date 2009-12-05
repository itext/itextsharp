using System;
using System.util;
using System.Collections;

/*
 * $Id: HeaderFooter.cs,v 1.6 2008/05/13 11:25:10 psoares33 Exp $
 * 
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2009 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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
    /// A HeaderFooter-object is a Rectangle with text
    /// that can be put above and/or below every page.
    /// </summary>
    /// <example>
    /// <code>
    /// <strong>HeaderFooter header = new HeaderFooter(new Phrase("This is a header."), false);
    /// HeaderFooter footer = new HeaderFooter(new Phrase("This is page "), new Phrase("."));</strong>
    /// document.SetHeader(header);
    /// document.SetFooter(footer);
    /// </code>
    /// </example>
    public class HeaderFooter : Rectangle {
    
        // membervariables
    
        /// <summary> Does the page contain a pagenumber? </summary>
        private bool numbered;
    
        /// <summary> This is the Phrase that comes before the pagenumber. </summary>
        private Phrase before = null;
    
        /// <summary> This is number of the page. </summary>
        private int pageN;
    
        /// <summary> This is the Phrase that comes after the pagenumber. </summary>
        private Phrase after = null;
    
        /// <summary> This is alignment of the header/footer. </summary>
        private int alignment;
    
        // constructors
    
        /// <summary>
        /// Constructs a HeaderFooter-object.
        /// </summary>
        /// <param name="before">the Phrase before the pagenumber</param>
        /// <param name="after">the Phrase after the pagenumber</param>
        public HeaderFooter(Phrase before, Phrase after) : base(0, 0, 0, 0) {
            this.Border = TOP_BORDER + BOTTOM_BORDER;
            this.BorderWidth = 1;
        
            numbered = true;
            this.before = before;
            this.after = after;
        }
    
        /// <summary>
        /// Constructs a Header-object with a pagenumber at the end.
        /// </summary>
        /// <param name="before">the Phrase before the pagenumber</param>
        /// <param name="numbered">true if the page has to be numbered</param>
        public HeaderFooter(Phrase before, bool numbered) : base(0, 0, 0, 0) {
            this.Border = TOP_BORDER + BOTTOM_BORDER;
            this.BorderWidth = 1;
        
            this.numbered = numbered;
            this.before = before;
        }

        public HeaderFooter(Properties attributes) : base(0, 0, 0, 0) {
            string value;
            
            if ((value = attributes.Remove(ElementTags.NUMBERED)) != null) {
                this.numbered = bool.Parse(value);
            }
            if ((value = attributes.Remove(ElementTags.ALIGN)) != null) {
                this.SetAlignment(value);
            }
            if ((value = attributes.Remove("border")) != null) {
                this.Border = int.Parse(value);
            } else {
                this.Border = TOP_BORDER + BOTTOM_BORDER;
            }
        }
    
        // methods
    
        /// <summary>
        /// Checks if the HeaderFooter contains a page number.
        /// </summary>
        /// <returns>true if the page has to be numbered</returns>
        public bool IsNumbered() {
            return numbered;
        }
    
        /// <summary>
        /// Get/set the part that comes before the pageNumber.
        /// </summary>
        /// <value>a Phrase</value>
        public Phrase Before {
            get {
                return before;
            }

            set {
                this.before = value;
            }
        }
    
        /// <summary>
        /// Get/set the part that comes after the pageNumber.
        /// </summary>
        /// <value>a Phrase</value>
        public Phrase After {
            get {
                return after;
            }

            set {
                this.after = value;
            }
        }
    
        /// <summary>
        /// Sets the page number.
        /// </summary>
        /// <value>the new page number</value>
        public int PageNumber {
            set {
                this.pageN = value;
            }
        }
    
        /// <summary>
        /// Sets the Element.
        /// </summary>
        /// <value>the new alignment</value>
        public int Alignment{
            set {
                this.alignment = value;
            }
            get {
                return this.alignment;
            }
        }

        /// <summary>
        /// Sets the alignment of this HeaderFooter.
        /// </summary>
        /// <param name="alignment">the new alignment as a string</param>
        public void SetAlignment(string alignment) {
            if (Util.EqualsIgnoreCase(alignment, ElementTags.ALIGN_CENTER)) {
                this.alignment = Element.ALIGN_CENTER;
                return;
            }
            if (Util.EqualsIgnoreCase(alignment, ElementTags.ALIGN_RIGHT)) {
                this.alignment = Element.ALIGN_RIGHT;
                return;
            }
            if (Util.EqualsIgnoreCase(alignment, ElementTags.ALIGN_JUSTIFIED)) {
                this.alignment = Element.ALIGN_JUSTIFIED;
                return;
            }
            this.alignment = Element.ALIGN_LEFT;
        }
    
        // methods to retrieve the membervariables
    
        /// <summary>
        /// Gets the Paragraph that can be used as header or footer.
        /// </summary>
        /// <returns>a Paragraph</returns>
        public Paragraph Paragraph {
            get {
                Paragraph paragraph = new Paragraph(before.Leading);
                paragraph.Add(before);
                if (numbered) {
                    paragraph.AddSpecial(new Chunk(pageN.ToString(), before.Font));
                }
                if (after != null) {
                    paragraph.AddSpecial(after);
                }
                paragraph.Alignment = alignment;
                return paragraph;
            }
        }
    }
}
