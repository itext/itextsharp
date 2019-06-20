/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html.pdfelement;

namespace iTextSharp.tool.xml.html.table {

    /**
     * @author redlab_b
     *
     */
    public class TableRowElement : IElement {

        /**
         * Enumeration used for determining the correct order of TableRowElements when adding the table to a document.
         * <br />
         * Possible values:
         * <ul>
         * <li>CAPTION_TOP</li>
         * <li>HEADER</li>
         * <li>BODY</li>
         * <li>FOOTER</li>
         * <li>CAPTION_BOTTOM</li>
         * </ul>
         *
         * @author Emiel Ackermann
         *
         */
        public class Place {
            /**
             * The caption element on top
             */
            public static readonly Place CAPTION_TOP = new Place(-2, -2);
            /*
             *
             * A header row
             */
            public static readonly Place HEADER = new Place(-1, -1);
            /**
             *
             * Body rows
             */
            public static readonly Place BODY = new Place(0, 1);
                /**
             *  Footer rows
             */
            public static readonly Place FOOTER = new Place(1, 0); 
            /**
             * The caption element in the bottom
             */
            public static readonly Place CAPTION_BOTTOM = new Place(2, 2);

            private int normal;
            private int repeated;

            private Place(int normal, int repeated) {
                this.normal = normal;
                this.repeated = repeated;
            }
            /**
             * The position when header/footers should not be repeated on each page.
             * @return an integer position
             */
            virtual public int Normal {
                get {
                    return normal;
                }
            }
            /**
             * The position when headers/footers should be repeated on each page.
             * @return an integer position
             */
            virtual public int Repeated {
                get {
                    return repeated;
                }
            }

            public override bool Equals(object obj) {
                if (!(obj is Place))
                    return false;
                if (obj == this)
                    return true;
                Place obp = (Place)obj;
                return obp.normal == normal && obp.repeated == repeated;
            }

            public override int GetHashCode() {
                return normal + repeated * 256;
            }
        }

        private Place place;
        private IList<HtmlCell> content;

        /**
         * Constructor based on the currentContent and a {@link Place}. All none {@link TableData} elements are filtered out of the current content list.
         * @param currentContent IList<IElement> containing all elements found between <tr> and </tr>.
         * @param place a {@link Place} in the table (caption, header, body or footer).
         */
        public TableRowElement(IList<IElement> currentContent, Place place) {
            // filter out none TD elements, discard others
            content = new List<HtmlCell>(currentContent.Count);
            foreach (IElement e in currentContent) {
                if (e is HtmlCell) {
                    content.Add((HtmlCell) e);
                }
            }
            this.place = place;
        }

        /* (non-Javadoc)
         * @see com.itextpdf.text.Element#process(com.itextpdf.text.ElementListener)
         */
        virtual public bool Process(IElementListener listener) {
            throw new iTextSharp.tool.xml.exceptions.NotImplementedException();
        }

        /* (non-Javadoc)
         * @see com.itextpdf.text.Element#type()
         */
        virtual public int Type {
            get {
                throw new iTextSharp.tool.xml.exceptions.NotImplementedException();
            }
        }

        /* (non-Javadoc)
         * @see com.itextpdf.text.Element#isContent()
         */
        virtual public bool IsContent() {
            throw new iTextSharp.tool.xml.exceptions.NotImplementedException();
        }

        /* (non-Javadoc)
         * @see com.itextpdf.text.Element#isNestable()
         */
        virtual public bool IsNestable() {
            throw new iTextSharp.tool.xml.exceptions.NotImplementedException();
        }

        /* (non-Javadoc)
         * @see com.itextpdf.text.Element#getChunks()
         */
        virtual public IList<Chunk> Chunks {
            get {
                throw new iTextSharp.tool.xml.exceptions.NotImplementedException();
            }
        }

        /**
         * @return the content.
         */
        virtual public IList<HtmlCell> Content {
            get {
                return content;
            }
        }

        /**
         * @return the {@link Place} of the row.
         */
        virtual public Place RowPlace {
            get {
                return place;
            }
        }
    }
}
