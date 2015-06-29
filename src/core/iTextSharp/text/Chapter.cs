using System;
using System.Collections.Generic;
using System.util;
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
 *
 */

namespace iTextSharp.text 
{
    /// <summary>
    /// A Chapter is a special Section.
    /// </summary>
    /// <remarks>
    /// A chapter number has to be created using a Paragraph as title
    /// and an int as chapter number. The chapter number is shown be
    /// default. If you don't want to see the chapter number, you have to set the
    /// numberdepth to 0.
    /// </remarks>
    /// <example>
    /// <code>
    /// Paragraph title2 = new Paragraph("This is Chapter 2", FontFactory.GetFont(FontFactory.HELVETICA, 18, Font.BOLDITALIC, new BaseColor(0, 0, 255)));
    /// <strong>Chapter chapter2 = new Chapter(title2, 2);
    /// chapter2.SetNumberDepth(0);</strong>
    /// Paragraph someText = new Paragraph("This is some text");
    /// <strong>chapter2.Add(someText);</strong>
    /// Paragraph title21 = new Paragraph("This is Section 1 in Chapter 2", FontFactory.GetFont(FontFactory.HELVETICA, 16, Font.BOLD, new BaseColor(255, 0, 0)));
    /// Section section1 = <strong>chapter2.AddSection(title21);</strong>
    /// Paragraph someSectionText = new Paragraph("This is some silly paragraph in a chapter and/or section. It contains some text to test the functionality of Chapters and Section.");
    /// section1.Add(someSectionText);
    /// </code>
    /// </example>
    public class Chapter : Section 
    {
    
        // constructors
    
        /**
        * Constructs a new <CODE>Chapter</CODE>.
        * @param   number      the Chapter number
        */
        
        public Chapter(int number) : base (null, 1) {
            numbers = new List<int>();
            numbers.Add(number);
            triggerNewPage = true;
        }

        /// <summary>
        /// Constructs a new Chapter.
        /// </summary>
        /// <param name="title">the Chapter title (as a Paragraph)</param>
        /// <param name="number">the Chapter number</param>
        /// <overoads>
        /// Has three overloads.
        /// </overoads>
        public Chapter(Paragraph title, int number) : base(title, 1) 
        {
            numbers = new List<int>();
            numbers.Add(number);
            triggerNewPage = true;
        }
    
        /// <summary>
        /// Constructs a new Chapter.
        /// </summary>
        /// <param name="title">the Chapter title (as a string)</param>
        /// <param name="number">the Chapter number</param>
        /// <overoads>
        /// Has three overloads.
        /// </overoads>
        public Chapter(string title, int number) : this(new Paragraph(title), number) {}
    
        // implementation of the Element-methods
    
        /// <summary>
        /// Gets the type of the text element.
        /// </summary>
        /// <value>a type</value>
        public override int Type {
            get {
                return Element.CHAPTER;
            }
        }
    
        /**
        * @see com.lowagie.text.Element#isNestable()
        * @since   iText 2.0.8
        */
        public override bool IsNestable() {
            return false;
        }
    }
}
