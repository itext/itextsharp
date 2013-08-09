/*
 * $Id: StructureItems.java 5945 2013-08-08 10:27:41Z blowagie $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2013 1T3XT BVBA
 * Authors: Bruno Lowagie, et al.
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

using System.Collections.Generic;

namespace iTextSharp.text.pdf.mc {

    /**
     * Creates a list of meaningful StructureItem objects extracted from the
     * Structure Tree of a PDF document.
     */

    public class StructureItems : List<StructureItem> {

        /** The StructTreeRoot dictionary */
        protected PdfDictionary structTreeRoot;

        /** The StructParents number tree. */
        protected Dictionary<int, PdfObject> parentTree;

        /**
	 * Creates a list of StructuredItem objects.
	 * @param reader the reader holding the PDF to examine
	 */

        public StructureItems(PdfReader reader)
            : base() {
            PdfDictionary catalog = reader.Catalog;
            structTreeRoot = catalog.GetAsDict(PdfName.STRUCTTREEROOT);
            if (structTreeRoot == null)
                return;
            parentTree = PdfNumberTree.ReadTree(structTreeRoot.GetAsDict(PdfName.PARENTTREE));
            structTreeRoot.Remove(PdfName.STRUCTPARENTS);
            InspectKids(structTreeRoot);
        }

        /**
	 * Inspects the value of the K entry of a structure element
	 * and stores all meaningful StructureItem objects that are encountered.
	 * @param structElem a structure element
	 */

        protected void InspectKids(PdfDictionary structElem) {
            if (structElem == null)
                return;
            PdfObject obj = structElem.GetDirectObject(PdfName.K);
            if (obj == null)
                return;
            switch (obj.Type) {
                case PdfObject.DICTIONARY:
                    AddStructureItem((PdfDictionary) obj, structElem.GetAsIndirectObject(PdfName.K));
                    break;
                case PdfObject.ARRAY:
                    PdfArray array = (PdfArray) obj;
                    for (int i = 0; i < array.Size; i++) {
                        AddStructureItem(array.GetAsDict(i), array.GetAsIndirectObject(i));
                    }
                    break;
            }
        }

        /**
	 * Looks at a kid of a structure item, adds it as a
	 * structure item (if necessary) and inspects its kids
	 * (if any).
	 * @param dict
	 */

        protected void AddStructureItem(PdfDictionary dict, PdfIndirectReference reference) {
            if (dict == null)
                return;
            StructureItem item = new StructureItem(dict, reference);
            InspectKids(dict);
            if (item.IsRealContent())
                Add(item);
        }

        /**
	 * Removes a StructParent from the parent tree.
	 * @param	PdfNumber	the number to remove
	 */

        public void RemoveFromParentTree(PdfNumber structParent) {
            parentTree.Remove(structParent.IntValue);
        }

        public int ProcessMCID(PdfNumber structParents, StructureItem item) {
            PdfObject obj = parentTree[structParents.IntValue];
            PdfArray array = (PdfArray) PdfReader.GetPdfObject(obj);
            array.Add(item.GetRef());
            return array.Size - 1;
        }

        /**
	 * Returns the number tree with the StructParents.
	 * @param writer	The writer to which the StructParents have to be written
	 * @throws IOException 
	 */

        public void WriteParentTree(PdfWriter writer) {
            if (structTreeRoot == null)
                return;
            List<int> numbers = new List<int>(parentTree.Keys);
            numbers.Sort();
            structTreeRoot.Put(PdfName.PARENTTREENEXTKEY, new PdfNumber(numbers[numbers.Count - 1] + 1));
            structTreeRoot.Put(PdfName.PARENTTREE, PdfNumberTree.WriteTree(parentTree, writer));
        }
    }
}
