using System;
using iTextSharp.text.pdf;

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

namespace iTextSharp.text.pdf.collection {

    public class PdfCollection : PdfDictionary {

        /** A type of PDF Collection */
        public const int DETAILS = 0;
        /** A type of PDF Collection */
        public const int TILE = 1;
        /** A type of PDF Collection */
        public const int HIDDEN = 2;
        /** A type of PDF Collection */
        public const int CUSTOM = 3;
        
        /**
        * Constructs a PDF Collection.
        * @param   type    the type of PDF collection.
        */
        public PdfCollection(int type) : base(PdfName.COLLECTION) {
            switch(type) {
            case TILE:
                Put(PdfName.VIEW, PdfName.T);
                break;
            case HIDDEN:
                Put(PdfName.VIEW, PdfName.H);
                break;
            case CUSTOM:
                Put(PdfName.VIEW, PdfName.C);
                break;
            default:
                Put(PdfName.VIEW, PdfName.D);
                break;
            }
        }
        
        /**
        * Identifies the document that will be initially presented
        * in the user interface.
        * @param description   the description that was used when attaching the file to the document
        */
        virtual public String InitialDocument {
            set {
                Put(PdfName.D, new PdfString(value, null));
            }
        }
        
        /**
        * Sets the Collection schema dictionary.
        * @param schema    an overview of the collection fields
        */
        virtual public PdfCollectionSchema Schema {
            set {
                Put(PdfName.SCHEMA, value);
            }
            get {
                return (PdfCollectionSchema)Get(PdfName.SCHEMA);
            }
        }
        
        /**
        * Sets the Collection sort dictionary.
        * @param sort  a collection sort dictionary
        */
        virtual public PdfCollectionSort Sort {
            set {
                Put(PdfName.SORT, value);
            }
        }
    }
}
