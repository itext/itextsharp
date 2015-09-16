/*
 * $Id$
 *
 * This file is part of the iText (R) project.
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

using System.Collections.Generic;
using iTextSharp.text.error_messages;

namespace iTextSharp.text.pdf
{
    public class PdfStructTreeController
    {
        private PdfDictionary structTreeRoot;
        private PdfCopy writer;
        private PdfStructureTreeRoot structureTreeRoot;
        private PdfDictionary parentTree;
        protected internal PdfReader reader;
        private PdfDictionary roleMap = null;
        private PdfDictionary sourceRoleMap = null;
        private PdfDictionary sourceClassMap = null;
        private PdfIndirectReference nullReference = null;
        //private Dictionary<int, object> openedDocuments = new Dictionary<int, object>();
        public enum ReturnType {BELOW, FOUND, ABOVE, NOTFOUND};

        protected internal PdfStructTreeController(PdfReader reader, PdfCopy writer){
            if (!writer.IsTagged())
                throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("no.structtreeroot.found"));
            this.writer = writer;
            structureTreeRoot = writer.StructureTreeRoot;
            structureTreeRoot.Put(PdfName.PARENTTREE, new PdfDictionary(PdfName.STRUCTELEM));
            SetReader(reader);
        }

        virtual protected internal void SetReader(PdfReader reader){
            this.reader = reader;
            PdfObject obj = reader.Catalog.Get(PdfName.STRUCTTREEROOT);
            obj = GetDirectObject(obj);
            if ((obj == null) || (!obj.IsDictionary()))
                throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("no.structtreeroot.found"));
            structTreeRoot = (PdfDictionary)obj;
            obj = GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            if (obj == null || !obj.IsDictionary())
                throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("the.document.does.not.contain.parenttree"));
            parentTree = (PdfDictionary)obj;
            sourceRoleMap = null;
            sourceClassMap = null;
            nullReference = null;
        }

        static public bool CheckTagged(PdfReader reader) {
            PdfObject obj = reader.Catalog.Get(PdfName.STRUCTTREEROOT);
            obj = GetDirectObject(obj);
            if (obj == null || !obj.IsDictionary())
                return false;
            PdfDictionary structTreeRoot = (PdfDictionary)obj;
            obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
            if (obj == null || !obj.IsDictionary())
                return false;
            return true;
        }

        public static PdfObject GetDirectObject(PdfObject obj) {
            if (obj == null)
                return null;
            while (obj.IsIndirect())
                obj = PdfReader.GetPdfObjectRelease(obj);
            return obj;
        }

        virtual public void CopyStructTreeForPage(PdfNumber sourceArrayNumber, int newArrayNumber) {
//            int documentHash = GetDocumentHash(reader);
//            if (!openedDocuments.ContainsKey(documentHash))
//            {
//                openedDocuments.Add(documentHash, null);
//                AddKid(structureTreeRoot, writer.CopyObject(structTreeRoot.Get(PdfName.K), true, true));
//            }
            if (CopyPageMarks(parentTree, sourceArrayNumber, newArrayNumber) == ReturnType.NOTFOUND) {
                throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("structparent.not.found"));
            }
        }

        private ReturnType CopyPageMarks(PdfDictionary parentTree, PdfNumber arrayNumber, int newArrayNumber) {
            PdfArray pages = (PdfArray)GetDirectObject(parentTree.Get(PdfName.NUMS));
            if (pages == null) {
                PdfArray kids = (PdfArray)GetDirectObject(parentTree.Get(PdfName.KIDS));
                if (kids == null)
                    return ReturnType.NOTFOUND;
                int cur = kids.Size/2;
                int begin = 0;
                while (true) {
                    PdfDictionary kidTree = (PdfDictionary)GetDirectObject(kids.GetPdfObject(cur + begin));
                    switch (CopyPageMarks(kidTree,arrayNumber,newArrayNumber)) {
                        case ReturnType.FOUND:
                            return ReturnType.FOUND;
                        case ReturnType.ABOVE:
                            begin += cur;
                            cur /= 2;
                            if (cur == 0)
                                cur = 1;
                            if (cur + begin == kids.Size)
                                return ReturnType.ABOVE;
                            break;
                        case ReturnType.BELOW:
                            if (cur + begin == 0)
                                return ReturnType.BELOW;
                            if (cur == 0)
                                return ReturnType.NOTFOUND;
                            cur /= 2;
                            break;
                        default:
                            return ReturnType.NOTFOUND;
                    }
                }
            } else {
                if (pages.Size == 0)
                    return ReturnType.NOTFOUND;
                return FindAndCopyMarks(pages, arrayNumber.IntValue, newArrayNumber);
            }
        }

        private ReturnType FindAndCopyMarks(PdfArray pages, int arrayNumber, int newArrayNumber){
            if (pages.GetAsNumber(0).IntValue > arrayNumber)
                return ReturnType.BELOW;
            if (pages.GetAsNumber(pages.Size - 2).IntValue < arrayNumber)
                return ReturnType.ABOVE;
            int cur = pages.Size/4;
            int begin = 0;
            int curNumber;
            while (true) {
                curNumber = pages.GetAsNumber((begin + cur)*2).IntValue;
                if (curNumber == arrayNumber) {
                    PdfObject obj = pages.GetPdfObject((begin + cur)*2 + 1);
                    PdfObject obj1 = obj;
                    while (obj.IsIndirect())
                        obj = PdfReader.GetPdfObjectRelease(obj);
                    //invalid Nums
                    if (obj.IsArray()) {
                        PdfObject firstNotNullKid = null;
                        foreach (PdfObject numObj in (PdfArray) obj) {
                            if (numObj.IsNull()) {
                                if (nullReference == null)
                                    nullReference = writer.AddToBody(new PdfNull()).IndirectReference;
                                structureTreeRoot.SetPageMark(newArrayNumber, nullReference);
                            } else {
                                PdfObject res = writer.CopyObject(numObj, true, false);
                                if (firstNotNullKid == null)
                                    firstNotNullKid = res;
                                structureTreeRoot.SetPageMark(newArrayNumber, (PdfIndirectReference) res);
                            }
                        }
                        AttachStructTreeRootKids(firstNotNullKid);
                    } else if (obj.IsDictionary()) {
                        PdfDictionary k = GetKDict((PdfDictionary) obj);
                        if (k == null)
                            return ReturnType.NOTFOUND;
                        PdfObject res = writer.CopyObject(obj1, true, false);
                        structureTreeRoot.SetAnnotationMark(newArrayNumber, (PdfIndirectReference) res);
                    } else {
                        return ReturnType.NOTFOUND;
                    }
                    return ReturnType.FOUND;
                }
                if (curNumber < arrayNumber) {
                    begin += cur;
                    cur /= 2;
                    if (cur == 0)
                        cur = 1;
                    if (cur + begin == pages.Size)
                        return ReturnType.NOTFOUND;
                    continue;
                }
                if (cur + begin == 0)
                    return ReturnType.BELOW;
                if (cur == 0)
                    return ReturnType.NOTFOUND;
                cur /= 2;
            }
        }

        /**
         * Add kid to structureTreeRoot from structTreeRoot
         */
        protected internal virtual void AttachStructTreeRootKids(PdfObject firstNotNullKid) {
            PdfObject structKids = structTreeRoot.Get(PdfName.K);
            if (structKids == null || (!structKids.IsArray() && !structKids.IsIndirect())) {
                // incorrect syntax of tags
                AddKid(structureTreeRoot, firstNotNullKid);
            } else {
                if (structKids.IsIndirect()) {
                    AddKid(structKids);
                } else {
                    //structKids.isArray()
                    foreach (PdfObject kid in (PdfArray) structKids)
                        AddKid(kid);
                }
            }
        }

        internal static PdfDictionary GetKDict(PdfDictionary obj) {
            PdfDictionary k = obj.GetAsDict(PdfName.K);
            if (k != null) {
                if (PdfName.OBJR.Equals(k.GetAsName(PdfName.TYPE))) {
                    return k;
                }
            } else {
                PdfArray k1 = obj.GetAsArray(PdfName.K);
                if (k1 == null)
                    return null;
                for (int i = 0; i < k1.Size; i++) {
                    k = k1.GetAsDict(i);
                    if (k != null) {
                        if (PdfName.OBJR.Equals(k.GetAsName(PdfName.TYPE))) {
                            return k;
                        }
                    }
                }
            }
            return null;
        }

        private void AddKid(PdfObject obj)
        {
            if (!obj.IsIndirect())
                return;
            PRIndirectReference currRef = (PRIndirectReference)obj;
            RefKey key =  new RefKey(currRef);
            if (!writer.indirects.ContainsKey(key)) {
                writer.CopyIndirect(currRef, true, false);
            }
            PdfIndirectReference newKid = writer.indirects[key].Ref;

            if (writer.updateRootKids) {
                AddKid(structureTreeRoot, newKid);
                writer.StructureTreeRootKidsForReaderImported(reader);
            }
        }

        private static PdfArray GetDirectArray(PdfArray input) {
            PdfArray output = new PdfArray();
            for (int i = 0; i < input.Size; ++i) {
                PdfObject value = GetDirectObject(input.GetPdfObject(i));
                if (value == null)
                    continue;
                if (value.IsArray()) {
                    output.Add(GetDirectArray((PdfArray)value));
                } else if (value.IsDictionary()) {
                    output.Add(GetDirectDict((PdfDictionary)value));
                } else {
                    output.Add(value);
                }
            }
            return output;
        }       
        
        private static PdfDictionary GetDirectDict(PdfDictionary input) {
            PdfDictionary output = new PdfDictionary();
            foreach (KeyValuePair<PdfName,PdfObject> entry in input.hashMap) {
                PdfObject value = GetDirectObject(entry.Value);
                if (value == null)
                    continue;
                if (value.IsArray()) {
                    output.Put(entry.Key, GetDirectArray((PdfArray)value));
                } else if (value.IsDictionary()) {
                    output.Put(entry.Key, GetDirectDict((PdfDictionary)value));
                } else {
                    output.Put(entry.Key,value);
                }
            }
            return output;
        }

        public static bool CompareObjects(PdfObject value1, PdfObject value2) {
            value2 = GetDirectObject(value2);
            if (value2 == null)
                return false;
            if (value1.Type != value2.Type)
                return false;

            if (value1.IsBoolean()){
                if (value1 == value2)
                    return true;
                if (value2 is PdfBoolean) {
                    return ((PdfBoolean)value1).BooleanValue == ((PdfBoolean)value2).BooleanValue;
                }
                return false;
            } else if (value1.IsName()) {
                return value1.Equals(value2);
            } else if (value1.IsNumber()){
                if (value1 == value2)
                    return true;
                if (value2 is PdfNumber) {
                    return ((PdfNumber)value1).DoubleValue == ((PdfNumber)value2).DoubleValue;
                }
                return false;
            } else if (value1.IsNull()){
                if (value1 == value2)
                    return true;
                if (value2 is PdfNull)
                    return true;
                return false;
            } else if (value1.IsString()){
                if (value1 == value2)
                    return true;
                if (value2 is PdfString) {
                    return ((value2 == null && value1.ToString() == null)
                        || value1.ToString() == value2.ToString());
                }
                return false;
            }
            if (value1.IsArray()) {
                PdfArray array1 = (PdfArray)value1;
                PdfArray array2 = (PdfArray)value2;
                if (array1.Size != array2.Size)
                    return false;
                for (int i = 0; i < array1.Size; ++i)
                    if (!CompareObjects(array1.GetPdfObject(i),array2.GetPdfObject(i)))
                        return false;
                return true;
            }
            if (value1.IsDictionary()) {
                PdfDictionary first = (PdfDictionary)value1;
                PdfDictionary second = (PdfDictionary)value2;
                if (first.Size != second.Size)
                    return false;
                foreach (PdfName name in first.hashMap.Keys) {
                    if (!CompareObjects(first.Get(name),second.Get(name)))
                        return false;
                }
                return true;
            }
            return false;
        }

        internal void AddClass(PdfObject obj) {
            obj = GetDirectObject(obj);
            if (obj.IsDictionary()) {
                PdfObject curClass = ((PdfDictionary)obj).Get(PdfName.C);
                if (curClass == null)
                    return;
                if (curClass.IsArray()) {
                    PdfArray array = (PdfArray)curClass;
                    for (int i = 0; i < array.Size; ++i) {
                        AddClass(array.GetPdfObject(i));
                    }
                } else if (curClass.IsName())
                    AddClass(curClass);
            } else if (obj.IsName()) {
                PdfName name = (PdfName)obj;
                if (sourceClassMap == null) {
                    obj = GetDirectObject(structTreeRoot.Get(PdfName.CLASSMAP));
                    if (obj == null || !obj.IsDictionary()) {
                        return;
                    }
                    sourceClassMap = (PdfDictionary)obj;
                }
                obj = GetDirectObject(sourceClassMap.Get(name));
                if (obj == null) {
                    return;
                }
                PdfObject put = structureTreeRoot.GetMappedClass(name);
                if (put != null) {
                    if (!CompareObjects(put,obj)) {
                        throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("conflict.in.classmap",name));
                    }
                } else {
                    if (obj.IsDictionary())
                        structureTreeRoot.MapClass(name, GetDirectDict((PdfDictionary)obj));
                    else if (obj.IsArray()) {
                        structureTreeRoot.MapClass(name, GetDirectArray((PdfArray)obj));
                    }
                }
            }
        }

        internal void AddRole(PdfName structType)
        {
            if (structType == null) {
                return;
            }
            foreach (PdfName name in writer.GetStandardStructElems()) {
                if (name.Equals(structType))
                    return;
            }
            PdfObject obj;
            if (sourceRoleMap == null) {
                obj = GetDirectObject(structTreeRoot.Get(PdfName.ROLEMAP));
                if (obj == null || !obj.IsDictionary()) {
                    return;
                }
                sourceRoleMap = (PdfDictionary)obj;
            }
            obj = sourceRoleMap.Get(structType);
            if (obj == null || !obj.IsName()) {
                return;
            }
            PdfObject currentRole;
            if (roleMap == null) {
                roleMap = new PdfDictionary();
                structureTreeRoot.Put(PdfName.ROLEMAP, roleMap);
                roleMap.Put(structType,obj);
            } else if ((currentRole = roleMap.Get(structType)) != null) {
                if (!currentRole.Equals(obj)) {
                    throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("conflict.in.rolemap",obj));
                }
            } else {
                roleMap.Put(structType,obj);
            }
        }

        virtual protected void AddKid(PdfDictionary parent, PdfObject kid){
            PdfObject kidObj = parent.Get(PdfName.K);
            PdfArray kids;
            if (kidObj is PdfArray){
                kids = (PdfArray)kidObj;
            } else {
                kids = new PdfArray();
                if (kidObj != null)
                    kids.Add(kidObj);
            }
            kids.Add(kid);
            parent.Put(PdfName.K, kids);
        }

//        private int GetDocumentHash(PdfReader reader) {
//            PdfDictionary trailer = reader.trailer;
//            int hash = trailer.Size;
//            Dictionary<String, String> info = reader.Info;
//            PdfArray id = trailer.GetAsArray(PdfName.ID);
//            if (id != null) {
//                foreach (PdfObject idPart in id) {
//                    if (idPart is PdfString) {
//                        hash = hash ^ ((PdfString)idPart).ToUnicodeString().GetHashCode();
//                    }
//                }
//            }
//            foreach (String key in info.Keys) {
//                String value = info[key];
//                if (value != null) {
//                    hash = hash ^ key.GetHashCode() ^ value.GetHashCode();
//                }
//            }
//            return hash;
//        }

    }
}
