using System;
using System.Collections.Generic;
using com.itextpdf.text.pdf;
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
            if (kids.Size > 0) {
                if (kids.GetAsNumber(0) != null)
                    kids.Remove(0);
                if (kids.Size > 0) {
                    PdfDictionary mcr = kids.GetAsDict(0);
                    if (mcr != null && PdfName.MCR.Equals(mcr.GetAsName(PdfName.TYPE))) {
                        kids.Remove(0);
                    }
                }
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
                return GetParent(false);
            }
        }

        public PdfDictionary GetParent(bool includeStructTreeRoot) {
            if (parent == null && includeStructTreeRoot)
                return top;
            else
                return parent;
        }

        internal void SetPageMark(int page, int mark) {
            if (mark >= 0)
                Put(PdfName.K, new PdfNumber(mark));
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

        
        public void WriteAttributes(IAccessibleElement element) {
            if (element is Paragraph) {
                WriteAttributes((Paragraph) element);
            } else if (element is Chunk) {
                WriteAttributes((Chunk)element);
            } else if (element is Image) {
                WriteAttributes((Image)element);
            } else if (element is List) {
                WriteAttributes((List)element);
            } else if (element is ListItem) {
                WriteAttributes((ListItem)element);
            } else if (element is ListLabel) {
                WriteAttributes((ListLabel)element);
            } else if (element is ListBody) {
                WriteAttributes((ListBody)element);
            } else if (element is PdfPTable) {
                WriteAttributes((PdfPTable)element);
            } else if (element is PdfPRow) {
                WriteAttributes((PdfPRow)element);
            } else if (element is PdfPCell) {
                WriteAttributes((PdfPCell)element);
            } else if (element is PdfPTableHeader) {
                WriteAttributes((PdfPTableHeader)element);
            } else if (element is PdfPTableFooter) {
                WriteAttributes((PdfPTableFooter)element);
            } else if (element is PdfPTableBody) {
                WriteAttributes((PdfPTableBody)element);
            }
            if (element.GetAccessibleAttributes() != null) {
                foreach (PdfName key in element.GetAccessibleAttributes().Keys) {
                    if (key.Equals(PdfName.LANG) || key.Equals(PdfName.ALT) || key.Equals(PdfName.ACTUALTEXT) || key.Equals(PdfName.E)) {
                        Put(key, element.GetAccessibleAttribute(key));
                    } else {
                        SetAttribute(key, element.GetAccessibleAttribute(key));
                    }
                }
            }
        }

        private void WriteAttributes(Chunk chunk) {
            if (chunk != null) {
                if (chunk.GetImage() != null) {
                    WriteAttributes(chunk.GetImage());
                } else {
                    Dictionary<String, Object> attr = chunk.Attributes;
                    if (attr != null){
                        // Setting non-inheritable attributes
                        if (attr.ContainsKey(Chunk.UNDERLINE)){
                            this.SetAttribute(PdfName.TEXTDECORATIONTYPE, PdfName.UNDERLINE);
                        }
                        if (attr.ContainsKey(Chunk.BACKGROUND)){
                            Object[] back = (Object[])attr[Chunk.BACKGROUND];
                            BaseColor color = (BaseColor)back[0];
                            this.SetAttribute(PdfName.BACKGROUNDCOLOR, new PdfArray(new float[] {color.R/255f, color.G/255f, color.B/255f}) );
                        }

                        // Setting inheritable attributes
                        IPdfStructureElement parent = (IPdfStructureElement) this.GetParent(true);
                        PdfObject obj = parent.GetAttribute(PdfName.COLOR);
                        if ((chunk.Font != null) && (chunk.Font.Color != null)) {
                            BaseColor c = chunk.Font.Color;
                            SetColorAttribute(c, obj, PdfName.COLOR);
                        }
                        PdfObject decorThickness  = parent.GetAttribute(PdfName.TEXTDECORATIONTHICKNESS);
                        PdfObject decorColor  = parent.GetAttribute(PdfName.TEXTDECORATIONCOLOR);
                        if (attr.ContainsKey(Chunk.UNDERLINE)){
                            Object[][] unders = (Object[][])attr[Chunk.UNDERLINE];
                            Object[] arr = unders[unders.Length-1];
                            BaseColor color = (BaseColor)arr[0];
                            float [] floats = (float[]) arr[1];
                            float thickness = floats[0];
                            // Setting thickness
                            if (decorThickness is PdfNumber){
                                float t = ((PdfNumber) decorThickness).FloatValue;
                                if (thickness.CompareTo(t) != 0){
                                    this.SetAttribute(PdfName.TEXTDECORATIONTHICKNESS, new PdfNumber(thickness));
                                }
                            }
                            else
                                this.SetAttribute(PdfName.TEXTDECORATIONTHICKNESS, new PdfNumber(thickness));

                            // Setting decoration color
                            if (color != null){
                                SetColorAttribute(color, decorColor, PdfName.TEXTDECORATIONCOLOR);
                            }
                        }
                    
                        if (attr.ContainsKey(Chunk.LINEHEIGHT)){
                            float height = (float)attr[Chunk.LINEHEIGHT];
                            PdfObject parentLH = parent.GetAttribute(PdfName.LINEHEIGHT);
                            if (parentLH is PdfNumber){
                                float pLH = ((PdfNumber)parentLH).FloatValue;
                                if (pLH.CompareTo(height) != 0){
                                    this.SetAttribute(PdfName.LINEHEIGHT, new PdfNumber(height));
                                }
                            }
                            else
                                this.SetAttribute(PdfName.LINEHEIGHT, new PdfNumber(height));
                        }
                    }
                }
            }
        }

        private void WriteAttributes(Image image) {
            if (image != null) {
                if (image.Width > 0){
                    this.SetAttribute(PdfName.WIDTH, new PdfNumber(image.Width));
                }
                if (image.Height > 0){
                    this.SetAttribute(PdfName.HEIGHT, new PdfNumber(image.Height));
                }
                image.Rotation = 10;
                image.Rotation = 10f;
                PdfRectangle rect = new PdfRectangle(image, ((Rectangle)image).Rotation);
                if (rect != null){
                    this.SetAttribute(PdfName.BBOX, rect);
                }
                if (image.Alt != null){
                    Put(PdfName.ALT, new PdfString(image.Alt));
                }

            }
        }

        private void WriteAttributes(Paragraph paragraph) {
            if (paragraph != null) {
                // Setting non-inheritable attributes
                if (paragraph.SpacingBefore.CompareTo(0f) != 0)
                    this.SetAttribute(PdfName.SPACEBEFORE, new PdfNumber(paragraph.SpacingBefore));
                if (paragraph.SpacingAfter.CompareTo(0f) != 0)
                    this.SetAttribute(PdfName.SPACEAFTER, new PdfNumber(paragraph.SpacingAfter));
                
                // Setting inheritable attributes
                IPdfStructureElement parent = (IPdfStructureElement) this.GetParent(true);
                PdfObject obj = parent.GetAttribute(PdfName.COLOR);
                if ((paragraph.Font != null) && (paragraph.Font.Color != null)) {
                    BaseColor c = paragraph.Font.Color;
                    SetColorAttribute(c, obj, PdfName.COLOR);
                }
                obj = parent.GetAttribute(PdfName.TEXTINDENT);
                if (paragraph.FirstLineIndent.CompareTo(0f) != 0) {
                    bool writeIndent = true;
                    if (obj is PdfNumber){
                        if (((PdfNumber)obj).FloatValue.CompareTo(paragraph.FirstLineIndent) == 0)
                            writeIndent = false;
                    }
                    if (writeIndent)
                        this.SetAttribute(PdfName.TEXTINDENT, new PdfNumber(paragraph.FirstLineIndent));
                }
                obj = parent.GetAttribute(PdfName.STARTINDENT);
                if (obj is PdfNumber) {
                    float startIndent = ((PdfNumber) obj).FloatValue;
                    if (startIndent.CompareTo(paragraph.IndentationLeft) != 0)
                        this.SetAttribute(PdfName.STARTINDENT, new PdfNumber(paragraph.IndentationLeft));
                } else {
                    if (Math.Abs(paragraph.IndentationLeft) > float.MinValue)
                        this.SetAttribute(PdfName.STARTINDENT, new PdfNumber(paragraph.IndentationLeft));
                }

                obj = parent.GetAttribute(PdfName.ENDINDENT);
                if (obj is PdfNumber) {
                    float endIndent = ((PdfNumber) obj).FloatValue;
                    if (endIndent.CompareTo(paragraph.IndentationRight) != 0)
                        this.SetAttribute(PdfName.ENDINDENT, new PdfNumber(paragraph.IndentationRight));
                } else {
                    if (paragraph.IndentationRight.CompareTo(0) != 0)
                        this.SetAttribute(PdfName.ENDINDENT, new PdfNumber(paragraph.IndentationRight));
                }

                PdfName align = null;
                switch (paragraph.Alignment) {
                    case Element.ALIGN_LEFT:
                        align = PdfName.START;
                        break;
                    case Element.ALIGN_CENTER:
                        align = PdfName.CENTER;
                        break;
                    case Element.ALIGN_RIGHT:
                        align = PdfName.END;
                        break;
                    case Element.ALIGN_JUSTIFIED:
                        align = PdfName.JUSTIFY;
                        break;
                }
                obj = parent.GetAttribute(PdfName.TEXTALIGN);
                if (obj is PdfName) {
                    PdfName textAlign = ((PdfName) obj);
                    if (align != null && !textAlign.Equals(align))
                        this.SetAttribute(PdfName.TEXTALIGN, align);
                } else {
                    if (align != null && !PdfName.START.Equals(align))
                        this.SetAttribute(PdfName.TEXTALIGN, align);
                }
            }
        }

        private void WriteAttributes(List list) {
            if (list != null) {

            }
        }

        private void WriteAttributes(ListItem listItem) {
            if (listItem != null) {

            }
        }

        private void WriteAttributes(ListBody listBody) {
            if (listBody != null) {

            }
        }

        private void WriteAttributes(ListLabel listLabel) {
            if (listLabel != null) {

            }
        }

        private void WriteAttributes(PdfPTable table) {
            if (table != null) {

            }
        }

        private void WriteAttributes(PdfPRow row) {
            if (row != null) {

            }
        }

        private void WriteAttributes(PdfPCell cell) {
            if (cell != null) {

            }
        }

        private void WriteAttributes(PdfPTableHeader header) {
            if (header != null) {

            }
        }

        private void WriteAttributes(PdfPTableBody body) {
            if (body != null) {

            }
        }

        private void WriteAttributes(PdfPTableFooter footer) {
            if (footer != null) {

            }
        }
    
        private bool ColorsEqual(PdfArray parentColor, float [] color){
            if (color[0].CompareTo(parentColor.GetAsNumber(0).FloatValue) != 0) {
                return false;
            }
            if (color[1].CompareTo(parentColor.GetAsNumber(1).FloatValue) != 0) {
                return false;
            }
            if (color[2].CompareTo(parentColor.GetAsNumber(2).FloatValue) != 0) {
                return false;
            }
            return true;
        }

        private void SetColorAttribute(BaseColor newColor, PdfObject oldColor, PdfName attributeName){
            float [] colorArr = new float[]{newColor.R/255f, newColor.G/255f, newColor.B/255f};
            if (oldColor is PdfArray){
                PdfArray oldC = (PdfArray)oldColor;
                if (ColorsEqual(oldC, colorArr))
                {
                    this.SetAttribute(attributeName, new PdfArray(colorArr));
                }
                else
                    this.SetAttribute(attributeName, new PdfArray(colorArr));
            }
            else
                this.SetAttribute(attributeName, new PdfArray(colorArr));
        }
    }
}
