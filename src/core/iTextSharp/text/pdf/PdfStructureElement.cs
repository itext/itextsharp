using System;
using System.Collections.Generic;
using System.IO;
using com.itextpdf.text.pdf;
using iTextSharp.text.error_messages;
using iTextSharp.text.pdf.interfaces;
using iTextSharp.text.pdf.intern;

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

namespace iTextSharp.text.pdf
{
    /**
    * This is a node in a document logical structure. It may contain a mark point or it may contain
    * other nodes.
    * @author Paulo Soares
    */

    public class PdfStructureElement : PdfDictionary, IPdfStructureElement
    {

        /**
        * Holds value of property kids.
        */
        private PdfStructureElement parent;
        private PdfStructureTreeRoot top;

        /**
        * Holds value of property reference.
        */
        private PdfIndirectReference reference;

        private PdfName structureType;

        /**
        * Creates a new instance of PdfStructureElement.
        * @param parent the parent of this node
        * @param structureType the type of structure. It may be a standard type or a user type mapped by the role map
        */

        public PdfStructureElement(PdfStructureElement parent, PdfName structureType)
        {
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

        public PdfStructureElement(PdfStructureTreeRoot parent, PdfName structureType)
        {
            top = parent;
            Init(parent, structureType);
            Put(PdfName.P, parent.Reference);
            Put(PdfName.TYPE, PdfName.STRUCTELEM);
        }

        internal PdfStructureElement(PdfDictionary parent, PdfName structureType)
        {
            if (parent is PdfStructureElement)
            {
                top = ((PdfStructureElement) parent).top;
                Init(parent, structureType);
                this.parent = (PdfStructureElement) parent;
                Put(PdfName.P, ((PdfStructureElement) parent).reference);
                Put(PdfName.TYPE, PdfName.STRUCTELEM);
            }
            else if (parent is PdfStructureTreeRoot)
            {
                top = (PdfStructureTreeRoot) parent;
                Init(parent, structureType);
                Put(PdfName.P, ((PdfStructureTreeRoot) parent).Reference);
                Put(PdfName.TYPE, PdfName.STRUCTELEM);
            }
            else
            {
            }
        }

        virtual public PdfName StructureType
        {
            get { return structureType; }
        }

        private void Init(PdfDictionary parent, PdfName structureType)
        {
            if(!top.Writer.GetStandardStructElems().Contains(structureType)) {
                PdfDictionary roleMap = top.GetAsDict(PdfName.ROLEMAP);
                if(roleMap == null || !roleMap.Contains(structureType))
                    throw new DocumentException(MessageLocalization.GetComposedMessage("unknown.structure.element.role.1", structureType.ToString()));
                else
                    this.structureType = roleMap.GetAsName(structureType);
            }
            else {
                this.structureType = structureType;
            }
            PdfObject kido = parent.Get(PdfName.K);
            PdfArray kids = null;
            if (kido == null)
            {
                kids = new PdfArray();
                parent.Put(PdfName.K, kids);
            }
            else if (kido is PdfArray)
            {
                kids = (PdfArray) kido;
            }
            else
            {
                kids = new PdfArray();
                kids.Add(kido);
                parent.Put(PdfName.K, kids);
            }
            if (kids.Size > 0)
            {
                if (kids.GetAsNumber(0) != null)
                    kids.Remove(0);
                if (kids.Size > 0)
                {
                    PdfDictionary mcr = kids.GetAsDict(0);
                    if (mcr != null && PdfName.MCR.Equals(mcr.GetAsName(PdfName.TYPE)))
                        kids.Remove(0);
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

        virtual public PdfDictionary Parent
        {
            get { return GetParent(false); }
        }

        virtual public PdfDictionary GetParent(bool includeStructTreeRoot)
        {
            if (parent == null && includeStructTreeRoot)
                return top;
            else
                return parent;
        }

        internal virtual void SetPageMark(int page, int mark) {
            if (mark >= 0)
                Put(PdfName.K, new PdfNumber(mark));
            top.SetPageMark(page, reference);
        }

        internal virtual void SetAnnotation(PdfAnnotation annot, PdfIndirectReference currentPage) {
            PdfArray kArray = GetAsArray(PdfName.K);
            if (kArray == null) {
                kArray = new PdfArray();
                PdfObject k = Get(PdfName.K);
                if (k != null) {
                    kArray.Add(k);
                }
                Put(PdfName.K, kArray);
            }
            PdfDictionary dict = new PdfDictionary();
            dict.Put(PdfName.TYPE, PdfName.OBJR);
            dict.Put(PdfName.OBJ, annot.IndirectReference);
            if (annot.Role == PdfName.FORM)
                dict.Put(PdfName.PG, currentPage);
            kArray.Add(dict);
        }

        /**
        * Gets the reference this object will be written to.
        * @return the reference this object will be written to
        */

        virtual public PdfIndirectReference Reference
        {
            get { return this.reference; }
        }

        /**
         * Gets the first entarance of attribute.
         * @returns PdfObject
         * @since 5.3.4
         */

        virtual public PdfObject GetAttribute(PdfName name)
        {
            PdfDictionary attr = GetAsDict(PdfName.A);
            if (attr != null && attr.Contains(name))
                return attr.Get(name);
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

        virtual public void SetAttribute(PdfName name, PdfObject obj)
        {
            PdfDictionary attr = GetAsDict(PdfName.A);
            if (attr == null)
            {
                attr = new PdfDictionary();
                Put(PdfName.A, attr);
            }
            attr.Put(name, obj);
        }


        public virtual void WriteAttributes(IAccessibleElement element) {
            // I do remember that these lines were necessary to avoid creation of files which are not valid from Acrobat 10 preflight perspective.
            // Now it seems that in Acrobat 11 there's no such problem (I think Acrobat 10 behavior can be considered as a bug) and we can remove those lines.
            // if(top.Writer.GetPdfVersion().Version < PdfWriter.VERSION_1_7)
            //    return;

            if (element is ListItem)
                WriteAttributes((ListItem) element);
            else if (element is Paragraph)
                WriteAttributes((Paragraph) element);
            else if (element is Chunk)
                WriteAttributes((Chunk) element);
            else if (element is Image)
                WriteAttributes((Image) element);
            else if (element is List)
                WriteAttributes((List) element);
            else if (element is ListLabel)
                WriteAttributes((ListLabel) element);
            else if (element is ListBody)
                WriteAttributes((ListBody) element);
            else if (element is PdfPTable)
                WriteAttributes((PdfPTable) element);
            else if (element is PdfPRow)
                WriteAttributes((PdfPRow) element);
            else if (element is PdfPHeaderCell)
                WriteAttributes((PdfPHeaderCell) element);
            else if (element is PdfPCell)
                WriteAttributes((PdfPCell) element);
            else if (element is PdfPTableHeader)
                WriteAttributes((PdfPTableHeader) element);
            else if (element is PdfPTableFooter)
                WriteAttributes((PdfPTableFooter) element);
            else if (element is PdfPTableBody)
                WriteAttributes((PdfPTableBody) element);
            else if (element is PdfDiv)
                WriteAttributes((PdfDiv) element);
            else if (element is PdfTemplate)
                WriteAttributes((PdfTemplate) element);
            else if (element is Document)
                WriteAttributes((Document) element);

            if (element.GetAccessibleAttributes() != null) {
                foreach (PdfName key in element.GetAccessibleAttributes().Keys) {
                    if (key.Equals(PdfName.ID)) {
                        PdfObject attr = element.GetAccessibleAttribute(key);
                        Put(key, attr);
                        top.PutIDTree(attr.ToString(), Reference);
                    } else if (key.Equals(PdfName.LANG) || key.Equals(PdfName.ALT) || key.Equals(PdfName.ACTUALTEXT) || key.Equals(PdfName.E) || key.Equals(PdfName.T)) {
                        Put(key, element.GetAccessibleAttribute(key));
                    } else
                        SetAttribute(key, element.GetAccessibleAttribute(key));
                }
            }
        }

        private void WriteAttributes(Chunk chunk)
        {
            if (chunk != null)
            {
                if (chunk.GetImage() != null)
                    WriteAttributes(chunk.GetImage());
                else
                {
                    Dictionary<String, Object> attr = chunk.Attributes;
                    if (attr != null)
                    {
                        this.SetAttribute(PdfName.O, PdfName.LAYOUT);
                        // Setting non-inheritable attributes
                        if (attr.ContainsKey(Chunk.UNDERLINE))
                            this.SetAttribute(PdfName.TEXTDECORATIONTYPE, PdfName.UNDERLINE);
                        if (attr.ContainsKey(Chunk.BACKGROUND))
                        {
                            Object[] back = (Object[]) attr[Chunk.BACKGROUND];
                            BaseColor color = (BaseColor) back[0];
                            this.SetAttribute(PdfName.BACKGROUNDCOLOR,
                                              new PdfArray(new float[] {color.R/255f, color.G/255f, color.B/255f}));
                        }

                        // Setting inheritable attributes
                        IPdfStructureElement parent = (IPdfStructureElement) this.GetParent(true);
                        PdfObject obj = GetParentAttribute(parent, PdfName.COLOR);
                        if ((chunk.Font != null) && (chunk.Font.Color != null))
                        {
                            BaseColor c = chunk.Font.Color;
                            SetColorAttribute(c, obj, PdfName.COLOR);
                        }
                        PdfObject decorThickness = GetParentAttribute(parent, PdfName.TEXTDECORATIONTHICKNESS);
                        PdfObject decorColor = GetParentAttribute(parent, PdfName.TEXTDECORATIONCOLOR);
                        if (attr.ContainsKey(Chunk.UNDERLINE))
                        {
                            Object[][] unders = (Object[][]) attr[Chunk.UNDERLINE];
                            Object[] arr = unders[unders.Length - 1];
                            BaseColor color = (BaseColor) arr[0];
                            float[] floats = (float[]) arr[1];
                            float thickness = floats[0];
                            // Setting thickness
                            if (decorThickness is PdfNumber)
                            {
                                float t = ((PdfNumber) decorThickness).FloatValue;
                                if (thickness.CompareTo(t) != 0)
                                    this.SetAttribute(PdfName.TEXTDECORATIONTHICKNESS, new PdfNumber(thickness));
                            }
                            else
                                this.SetAttribute(PdfName.TEXTDECORATIONTHICKNESS, new PdfNumber(thickness));

                            // Setting decoration color
                            if (color != null)
                                SetColorAttribute(color, decorColor, PdfName.TEXTDECORATIONCOLOR);
                        }

                        if (attr.ContainsKey(Chunk.LINEHEIGHT))
                        {
                            float height = (float) attr[Chunk.LINEHEIGHT];
                            PdfObject parentLH = GetParentAttribute(parent, PdfName.LINEHEIGHT);
                            if (parentLH is PdfNumber)
                            {
                                float pLH = ((PdfNumber) parentLH).FloatValue;
                                if (pLH.CompareTo(height) != 0)
                                    this.SetAttribute(PdfName.LINEHEIGHT, new PdfNumber(height));
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
                this.SetAttribute(PdfName.O, PdfName.LAYOUT);
                if (image.Width > 0)
                    this.SetAttribute(PdfName.WIDTH, new PdfNumber(image.Width));
                if (image.Height > 0)
                    this.SetAttribute(PdfName.HEIGHT, new PdfNumber(image.Height));
                PdfRectangle rect = new PdfRectangle(image, ((Rectangle) image).Rotation);
                this.SetAttribute(PdfName.BBOX, rect);
                if (image.BackgroundColor != null) {
                    BaseColor color = image.BackgroundColor;
                    this.SetAttribute(PdfName.BACKGROUNDCOLOR, new PdfArray(new float[] { color.R / 255f, color.G / 255f, color.B / 255f }));
                }
            }
        }

        private void WriteAttributes(PdfTemplate template) {
            if (template != null) {
                this.SetAttribute(PdfName.O, PdfName.LAYOUT);
                if (template.Width > 0) {
                    this.SetAttribute(PdfName.WIDTH, new PdfNumber(template.Width));
                }
                if (template.Height > 0) {
                    this.SetAttribute(PdfName.HEIGHT, new PdfNumber(template.Height));
                }
                PdfRectangle rect = new PdfRectangle(template.BoundingBox);
                this.SetAttribute(PdfName.BBOX, rect);
            }
        }

        private void WriteAttributes(Paragraph paragraph) {
            if (paragraph != null) {
                this.SetAttribute(PdfName.O, PdfName.LAYOUT);
                // Setting non-inheritable attributes
                if (paragraph.SpacingBefore.CompareTo(0f) != 0)
                    this.SetAttribute(PdfName.SPACEBEFORE, new PdfNumber(paragraph.SpacingBefore));
                if (paragraph.SpacingAfter.CompareTo(0f) != 0)
                    this.SetAttribute(PdfName.SPACEAFTER, new PdfNumber(paragraph.SpacingAfter));

                // Setting inheritable attributes
                IPdfStructureElement parent = (IPdfStructureElement) this.GetParent(true);
                PdfObject obj = GetParentAttribute(parent, PdfName.COLOR);
                if ((paragraph.Font != null) && (paragraph.Font.Color != null)) {
                    BaseColor c = paragraph.Font.Color;
                    SetColorAttribute(c, obj, PdfName.COLOR);
                }
                obj = GetParentAttribute(parent, PdfName.TEXTINDENT);
                if (paragraph.FirstLineIndent.CompareTo(0f) != 0) {
                    bool writeIndent = true;
                    if (obj is PdfNumber) {
                        if (((PdfNumber) obj).FloatValue.CompareTo(paragraph.FirstLineIndent) == 0)
                            writeIndent = false;
                    }
                    if (writeIndent)
                        this.SetAttribute(PdfName.TEXTINDENT, new PdfNumber(paragraph.FirstLineIndent));
                }
                obj = GetParentAttribute(parent, PdfName.STARTINDENT);
                if (obj is PdfNumber) {
                    float startIndent = ((PdfNumber) obj).FloatValue;
                    if (startIndent.CompareTo(paragraph.IndentationLeft) != 0)
                        this.SetAttribute(PdfName.STARTINDENT, new PdfNumber(paragraph.IndentationLeft));
                } else {
                    if (Math.Abs(paragraph.IndentationLeft) > float.Epsilon)
                        this.SetAttribute(PdfName.STARTINDENT, new PdfNumber(paragraph.IndentationLeft));
                }

                obj = GetParentAttribute(parent, PdfName.ENDINDENT);
                if (obj is PdfNumber) {
                    float endIndent = ((PdfNumber) obj).FloatValue;
                    if (endIndent.CompareTo(paragraph.IndentationRight) != 0)
                        this.SetAttribute(PdfName.ENDINDENT, new PdfNumber(paragraph.IndentationRight));
                } else {
                    if (paragraph.IndentationRight.CompareTo(0) != 0)
                        this.SetAttribute(PdfName.ENDINDENT, new PdfNumber(paragraph.IndentationRight));
                }

                SetTextAlignAttribute(paragraph.Alignment);
            }
        }

        private void WriteAttributes(List list) {
            if (list != null) {
                this.SetAttribute(PdfName.O, PdfName.LIST);
                if (list.Autoindent) {
                    if (list.Numbered) {
                        if (list.Lettered) {
                            if (list.IsLowercase)
                                this.SetAttribute(PdfName.LISTNUMBERING, PdfName.LOWERROMAN);
                            else
                                this.SetAttribute(PdfName.LISTNUMBERING, PdfName.UPPERROMAN);
                        } else {
                            this.SetAttribute(PdfName.LISTNUMBERING, PdfName.DECIMAL);
                        }
                    } else if (list.Lettered) {
                        if (list.IsLowercase)
                            this.SetAttribute(PdfName.LISTNUMBERING, PdfName.LOWERALPHA);
                        else
                            this.SetAttribute(PdfName.LISTNUMBERING, PdfName.UPPERALPHA);
                    }
                }
                PdfObject obj = GetParentAttribute(parent, PdfName.STARTINDENT);
                if (obj is PdfNumber) {
                    float startIndent = ((PdfNumber) obj).FloatValue;
                    if (startIndent != list.IndentationLeft)
                        this.SetAttribute(PdfName.STARTINDENT, new PdfNumber(list.IndentationLeft));
                } else {
                    if (Math.Abs(list.IndentationLeft) > float.Epsilon)
                        this.SetAttribute(PdfName.STARTINDENT, new PdfNumber(list.IndentationLeft));
                }

                obj = GetParentAttribute(parent, PdfName.ENDINDENT);
                if (obj is PdfNumber) {
                    float endIndent = ((PdfNumber) obj).FloatValue;
                    if (endIndent != list.IndentationRight)
                        this.SetAttribute(PdfName.ENDINDENT, new PdfNumber(list.IndentationRight));
                } else {
                    if (list.IndentationRight > float.Epsilon)
                        this.SetAttribute(PdfName.ENDINDENT, new PdfNumber(list.IndentationRight));
                }
            }
        }

        private void WriteAttributes(ListItem listItem) {
            if (listItem != null) {
                PdfObject obj = parent.GetParentAttribute(parent, PdfName.STARTINDENT);
                if (obj is PdfNumber) {
                    float startIndent = ((PdfNumber) obj).FloatValue;
                    if (startIndent.CompareTo(listItem.IndentationLeft) != 0)
                        this.SetAttribute(PdfName.STARTINDENT, new PdfNumber(listItem.IndentationLeft));
                } else {
                    if (Math.Abs(listItem.IndentationLeft) > float.Epsilon)
                        this.SetAttribute(PdfName.STARTINDENT, new PdfNumber(listItem.IndentationLeft));
                }

                obj = GetParentAttribute(parent, PdfName.ENDINDENT);
                if (obj is PdfNumber) {
                    float endIndent = ((PdfNumber) obj).FloatValue;
                    if (endIndent.CompareTo(listItem.IndentationRight) != 0)
                        this.SetAttribute(PdfName.ENDINDENT, new PdfNumber(listItem.IndentationRight));
                } else {
                    if (Math.Abs(listItem.IndentationRight) > float.Epsilon)
                        this.SetAttribute(PdfName.ENDINDENT, new PdfNumber(listItem.IndentationRight));
                }
            }
        }

        private void WriteAttributes(ListBody listBody) {
            if (listBody != null) {
            }
        }

        private void WriteAttributes(ListLabel listLabel) {
            if (listLabel != null) {
                PdfObject obj = GetParentAttribute(parent, PdfName.STARTINDENT);
                if (obj is PdfNumber) {
                    float startIndent = ((PdfNumber) obj).FloatValue;
                    if (startIndent != listLabel.Indentation)
                        this.SetAttribute(PdfName.STARTINDENT, new PdfNumber(listLabel.Indentation));
                } else {
                    if (Math.Abs(listLabel.Indentation) > float.Epsilon)
                        this.SetAttribute(PdfName.STARTINDENT, new PdfNumber(listLabel.Indentation));
                }
            }
        }

        private void WriteAttributes(PdfPTable table)
        {
            if (table != null)
            {
                this.SetAttribute(PdfName.O, PdfName.TABLE);

                // Setting non-inheritable attributes
                if (table.SpacingBefore > float.Epsilon)
                    this.SetAttribute(PdfName.SPACEBEFORE, new PdfNumber(table.SpacingBefore));

                if (table.SpacingAfter > float.Epsilon)
                    this.SetAttribute(PdfName.SPACEAFTER, new PdfNumber(table.SpacingAfter));


                if (table.TotalHeight > 0)
                    this.SetAttribute(PdfName.HEIGHT, new PdfNumber(table.TotalHeight));
                if (table.TotalWidth > 0)
                    this.SetAttribute(PdfName.WIDTH, new PdfNumber(table.TotalWidth));
            }
        }

        private void WriteAttributes(PdfPRow row)
        {
            if (row != null)
                this.SetAttribute(PdfName.O, PdfName.TABLE);
        }

        private void WriteAttributes(PdfPCell cell)
        {
            if (cell != null)
            {
                this.SetAttribute(PdfName.O, PdfName.TABLE);
                if (cell.Colspan != 1)
                    this.SetAttribute(PdfName.COLSPAN, new PdfNumber(cell.Colspan));
                if (cell.Rowspan != 1)
                    this.SetAttribute(PdfName.ROWSPAN, new PdfNumber(cell.Rowspan));
                if (cell.Headers != null)
                {
                    PdfArray headers = new PdfArray();
                    List<PdfPHeaderCell> list = cell.Headers;
                    foreach (PdfPHeaderCell header in list)
                        if (header.Name != null)
                            headers.Add(new PdfString(header.Name));
                    if (!headers.IsEmpty())
                        this.SetAttribute(PdfName.HEADERS, headers);
                }

                if (cell.CalculatedHeight > 0)
                    this.SetAttribute(PdfName.HEIGHT, new PdfNumber(cell.CalculatedHeight));

                if (cell.Width > 0)
                    this.SetAttribute(PdfName.WIDTH, new PdfNumber(cell.Width));

                if (cell.BackgroundColor != null)
                {
                    BaseColor color = cell.BackgroundColor;
                    this.SetAttribute(PdfName.BACKGROUNDCOLOR, new PdfArray(new float[] { color.R / 255f, color.G / 255f, color.B / 255f }));
                }
            }
        }

        private void WriteAttributes(PdfPHeaderCell headerCell)
        {
            if (headerCell != null)
            {
                if (headerCell.Scope != PdfPHeaderCell.NONE)
                {
                    switch (headerCell.Scope)
                    {
                        case PdfPHeaderCell.ROW:
                            this.SetAttribute(PdfName.SCOPE, PdfName.ROW);
                            break;
                        case PdfPHeaderCell.COLUMN:
                            this.SetAttribute(PdfName.SCOPE, PdfName.COLUMN);
                            break;
                        case PdfPHeaderCell.BOTH:
                            this.SetAttribute(PdfName.SCOPE, PdfName.BOTH);
                            break;
                    }
                }
                if (headerCell.Name != null)
                    this.SetAttribute(PdfName.NAME, new PdfName(headerCell.Name));
                WriteAttributes((PdfPCell)headerCell);
            }
        }

        private void WriteAttributes(PdfPTableHeader header)
        {
            if (header != null)
            {
                this.SetAttribute(PdfName.O, PdfName.TABLE);
            }
        }

        private void WriteAttributes(PdfPTableBody body)
        {
            if (body != null)
            {

            }
        }

        private void WriteAttributes(PdfPTableFooter footer)
        {
            if (footer != null)
            {

            }
        }

        private void WriteAttributes(PdfDiv div)
        {
            if (div != null)
            {
                // Setting non-inheritable attributes
                if (div.BackgroundColor != null)
                    SetColorAttribute(div.BackgroundColor, null, PdfName.BACKGROUNDCOLOR);

                // Setting inheritable attributes
                SetTextAlignAttribute(div.TextAlignment);
            }
        }

        private void WriteAttributes(Document document)
        {
            if (document != null)
            {

            }
        }


        private bool ColorsEqual(PdfArray parentColor, float[] color)
        {
            if (color[0].CompareTo(parentColor.GetAsNumber(0).FloatValue) != 0)
                return false;
            if (color[1].CompareTo(parentColor.GetAsNumber(1).FloatValue) != 0)
                return false;
            if (color[2].CompareTo(parentColor.GetAsNumber(2).FloatValue) != 0)
                return false;
            return true;
        }

        private void SetColorAttribute(BaseColor newColor, PdfObject oldColor, PdfName attributeName)
        {
            float[] colorArr = new float[] {newColor.R/255f, newColor.G/255f, newColor.B/255f};
            if ((oldColor != null) && (oldColor is PdfArray))
            {
                PdfArray oldC = (PdfArray) oldColor;
                if (ColorsEqual(oldC, colorArr))
                    this.SetAttribute(attributeName, new PdfArray(colorArr));
                else
                    this.SetAttribute(attributeName, new PdfArray(colorArr));
            }
            else
                this.SetAttribute(attributeName, new PdfArray(colorArr));
        }

        private void SetTextAlignAttribute(int elementAlign)
        {
            PdfName align = null;
            switch (elementAlign)
            {
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
            PdfObject obj = GetParentAttribute(parent, PdfName.TEXTALIGN);
            if (obj is PdfName)
            {
                PdfName textAlign = ((PdfName) obj);
                if (align != null && !textAlign.Equals(align))
                    this.SetAttribute(PdfName.TEXTALIGN, align);
            }
            else
            {
                if (align != null && !PdfName.START.Equals(align))
                    this.SetAttribute(PdfName.TEXTALIGN, align);
            }
        }

        public override void ToPdf(PdfWriter writer, Stream os) {
            PdfWriter.CheckPdfIsoConformance(writer, PdfIsoKeys.PDFISOKEY_STRUCTELEM, this);
            base.ToPdf(writer, os);
        }

        private PdfObject GetParentAttribute(IPdfStructureElement parent, PdfName name) {
            if (parent == null) {
                return null;
            }

            return parent.GetAttribute(name);
        }
    }
}
