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
using iTextSharp.text.error_messages;
using iTextSharp.text.log;

namespace iTextSharp.text.pdf.mc {

    /**
     * Creates a list of StructureItem objects extracted from the
     * Structure Tree of a PDF document.
     */

    public class StructureItems : List<StructureItem> {

        /** The Logger instance */
        protected static readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof (StructureItems));

        /** The StructTreeRoot dictionary */
        protected PdfDictionary structTreeRoot;

        /** The StructParents number tree values. */
        protected Dictionary<int, PdfObject> parentTree;

        /**
	     * Creates a list of StructuredItem objects.
	     * @param reader the reader holding the PDF to examine
	     */

        public StructureItems(PdfReader reader) {
            PdfDictionary catalog = reader.Catalog;
            structTreeRoot = catalog.GetAsDict(PdfName.STRUCTTREEROOT);
            if (structTreeRoot == null)
                throw new DocumentException(MessageLocalization.GetComposedMessage("can.t.read.document.structure"));
            // Storing the parent tree
            parentTree = PdfNumberTree.ReadTree(structTreeRoot.GetAsDict(PdfName.PARENTTREE));
            structTreeRoot.Remove(PdfName.STRUCTPARENTS);
            // Examining the StructTreeRoot
            PdfObject objecta = structTreeRoot.GetDirectObject(PdfName.K);
            if (objecta == null)
                return;
            switch (objecta.Type) {
                case PdfObject.DICTIONARY:
                    LOGGER.Info("StructTreeRoot refers to dictionary");
                    ProcessStructElems((PdfDictionary) objecta, structTreeRoot.GetAsIndirectObject(PdfName.K));
                    break;
                case PdfObject.ARRAY:
                    LOGGER.Info("StructTreeRoot refers to array");
                    PdfArray array = (PdfArray) objecta;
                    for (int i = 0; i < array.Size; i++) {
                        ProcessStructElems(array.GetAsDict(i), array.GetAsIndirectObject(i));
                    }
                    break;
            }
        }


        /**
         * Looks at a StructElem dictionary, and processes it.
         * @param dict	the StructElem dictionary that needs to be examined
         * @param ref	the reference to the StructElem dictionary
         * @throws DocumentException
         */

        protected virtual void ProcessStructElems(PdfDictionary structElem, PdfIndirectReference refa) {
            if (LOGGER.IsLogging(Level.INFO)) {
                LOGGER.Info(String.Format("addStructureItems({0}, {1})", structElem, refa));
            }
            if (structElem == null)
                return;
            ProcessStructElemKids(structElem, refa, structElem.GetDirectObject(PdfName.K));
        }

        /**
         * Processes the kids object of a StructElem dictionary.
         * This kids object can be a number (MCID), another StructElem dictionary,
         * an MCR dictionary, an OBJR dictionary, or an array of the above.
         * @param structElem	the StructElem dictionary
         * @param ref			the reference to the StructElem dictionary
         * @param object		the kids object
         */

        protected virtual void ProcessStructElemKids(PdfDictionary structElem, PdfIndirectReference refa, PdfObject objecta) {
            if (LOGGER.IsLogging(Level.INFO)) {
                LOGGER.Info(String.Format("addStructureItem({0}, {1}, {2})", structElem, refa, objecta));
            }
            if (objecta == null)
                return;
            StructureItem item;
            switch (objecta.Type) {
                case PdfObject.NUMBER:
                    item = new StructureMCID(structElem.GetAsIndirectObject(PdfName.PG), (PdfNumber) objecta);
                    Add(item);
                    LOGGER.Info("Added " + item);
                    break;
                case PdfObject.ARRAY:
                    PdfArray array = (PdfArray) objecta;
                    for (int i = 0; i < array.Size; i++) {
                        ProcessStructElemKids(structElem, array.GetAsIndirectObject(i), array.GetDirectObject(i));
                    }
                    break;
                case PdfObject.DICTIONARY:
                    PdfDictionary dict = (PdfDictionary) objecta;
                    if (dict.CheckType(PdfName.MCR)) {
                        item = new StructureMCID(dict);
                        Add(item);
                        LOGGER.Info("Added " + item);
                    } else if (dict.CheckType(PdfName.OBJR)) {
                        item = new StructureObject(structElem, refa, dict);
                        Add(item);
                        LOGGER.Info("Added " + item);
                    } else {
                        ProcessStructElems(dict, refa);
                    }
                    break;
            }
        }

        /**
         * Removes a StructParent from the parent tree.
         * @param	PdfNumber	the number to remove
         */

        public virtual void RemoveFromParentTree(PdfNumber structParent) {
            parentTree.Remove(structParent.IntValue);
        }

        /**
         * Creates a new MCID in the parent tree of the page
         * and returns that new MCID so that it can be used
         * in the content stream
         * @param structParents	the StructParents entry in the page dictionary
         * @param item	the item for which we need a new MCID
         * @return	a new MCID
         * @throws DocumentException
         */

        public virtual int ProcessMCID(PdfNumber structParents, PdfIndirectReference refa) {
            if (refa == null)
                throw new DocumentException(MessageLocalization.GetComposedMessage("can.t.read.document.structure"));
            PdfObject objecta;
            parentTree.TryGetValue(structParents.IntValue, out objecta);
            PdfArray array = (PdfArray) PdfReader.GetPdfObject(objecta);
            int i = GetNextMCID(structParents);
            if (i < array.Size) {
                array[i] = refa;
                return i;
            }
            array.Add(refa);
            return array.Size - 1;
        }

        /**
         * Finds the next available MCID, which is either the lowest empty ID in
         * the existing range, or the first available higher number.
         * @param structParents	the StructParents entry in the page dictionary
         * @return	the first available MCID
         */
        public virtual int GetNextMCID(PdfNumber structParents) {
            PdfObject objecta;
            parentTree.TryGetValue(structParents.IntValue, out objecta);
            PdfArray array = (PdfArray)PdfReader.GetPdfObject(objecta);
            for (int i = 0; i < array.Size; i++) {
                if (array.GetAsIndirectObject(i) == null) {
                    return i;
                }
            }
            return array.Size;
        }

        /**
         * Writes the altered parent tree to a PdfWriter and updates the StructTreeRoot entry.
         * @param writer	The writer to which the StructParents have to be written
         * @throws IOException 
         */

        public virtual void WriteParentTree(PdfWriter writer) {
            if (structTreeRoot == null)
                return;
            int[] numbers = new int[parentTree.Count];
            parentTree.Keys.CopyTo(numbers, 0);
            Array.Sort(numbers);
            structTreeRoot.Put(PdfName.PARENTTREENEXTKEY, new PdfNumber(numbers[numbers.Length - 1] + 1));
            structTreeRoot.Put(PdfName.PARENTTREE, PdfNumberTree.WriteTree(parentTree, writer));
        }
    }
}
