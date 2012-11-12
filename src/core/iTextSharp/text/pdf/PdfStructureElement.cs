using System;
using iTextSharp.text.error_messages;
using iTextSharp.text.pdf.interfaces;

/*
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2012 1T3XT BVBA
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

namespace iTextSharp.text.pdf {
    /**
    * This is a node in a document logical structure. It may contain a mark point or it may contain
    * other nodes.
    * @author Paulo Soares
    */
    public class PdfStructureElement : PdfDictionary, IPdfStructureElement {
        
        /**
        * Holds value of property kids.
        */
        private PdfStructureElement parent;
        private PdfStructureTreeRoot top;
        
        /**
        * Holds value of property reference.
        */
        private PdfIndirectReference reference;
        
        /**
        * Creates a new instance of PdfStructureElement.
        * @param parent the parent of this node
        * @param structureType the type of structure. It may be a standard type or a user type mapped by the role map
        */
        public PdfStructureElement(PdfStructureElement parent, PdfName structureType) {
            top = parent.top;
            Init(parent, structureType);
            this.parent = parent;
            Put(PdfName.P, parent.reference);
            Put(PdfName.TYPE, PdfName.STRUCTELEM);
        }

        internal PdfStructureElement(PdfDictionary parent, PdfName structureType) {
            if (parent is PdfStructureElement) {
                top = ((PdfStructureElement) parent).top;
                Init(parent, structureType);
                this.parent = (PdfStructureElement) parent;
                Put(PdfName.P, ((PdfStructureElement) parent).reference);
                Put(PdfName.TYPE, PdfName.STRUCTELEM);
            } else if (parent is PdfStructureTreeRoot) {
                top = (PdfStructureTreeRoot) parent;
                Init(parent, structureType);
                Put(PdfName.P, ((PdfStructureTreeRoot) parent).Reference);
                Put(PdfName.TYPE, PdfName.STRUCTELEM);
            } else {}
        }

        /**
        * Creates a new instance of PdfStructureElement.
        * @param parent the parent of this node
        * @param structureType the type of structure. It may be a standard type or a user type mapped by the role map
        */    
        public PdfStructureElement(PdfStructureTreeRoot parent, PdfName structureType) {
            top = parent;
            Init(parent, structureType);
            Put(PdfName.P, parent.Reference);
            Put(PdfName.TYPE, PdfName.STRUCTELEM);
        }
        
        private void Init(PdfDictionary parent, PdfName structureType) {
            PdfObject kido = parent.Get(PdfName.K);
            PdfArray kids = null;
            if (kido == null) {
                kids = new PdfArray();
                parent.Put(PdfName.K, kids);
            } else if (kido is PdfArray) {
                kids = (PdfArray)kido;
            } else {
                kids = new PdfArray();
                kids.Add(kido);
                parent.Put(PdfName.K, kids);
            }
            kids.Add(this);
            Put(PdfName.S, structureType);
            reference = top.Writer.PdfIndirectReference;
        }
        
        /**
        * Gets the parent of this node.
        * @return the parent of this node
        */    
        public PdfDictionary Parent {
            get {
                return parent;
            }
        }
        
        internal void SetPageMark(int page, int mark) {
            if (mark >= 0)
                Put(PdfName.K, new PdfNumber(mark));
            if (parent == null)
                top.SetPageMark(page, reference);
        }
        
        /**
        * Gets the reference this object will be written to.
        * @return the reference this object will be written to
        */    
        public PdfIndirectReference Reference {
            get {
                return this.reference;
            }
        }

        /**
         * Gets the first entarance of attribute.
         * @returns PdfObject
         * @since 5.3.4
         */
        public PdfObject GetAttribute(PdfName name) {
            PdfDictionary attr = GetAsDict(PdfName.A);
            if (attr != null) {
                if (attr.Contains(name))
                    return attr.Get(name);
            }
            PdfDictionary parent = Parent;
            if (parent is PdfStructureElement)
                return ((PdfStructureElement) parent).GetAttribute(name);
            if (parent is PdfStructureTreeRoot)
                return ((PdfStructureTreeRoot) parent).GetAttribute(name);

            return new PdfNull();
        }

        /**
         * Sets the attribute value.
         * @since 5.3.4
         */
        public void SetAttribute(PdfName name, PdfObject obj) {
            PdfDictionary attr = GetAsDict(PdfName.A);
            if (attr == null) {
                attr = new PdfDictionary();
                Put(PdfName.A, attr);
            }
            attr.Put(name, obj);
        }
    }
}
