using System;
using System.Collections.Generic;
using iTextSharp.text.error_messages;
using iTextSharp.text.pdf;

namespace iTextSharp.text.pdf{
public class PdfStructTreeController {

    private PdfDictionary structTreeRoot;
    private PdfCopy writer;
    private PdfStructureTreeRoot structureTreeRoot;
    private PdfDictionary parentTree;
    private String fileName;
    protected internal PdfReader reader;
    private PdfDictionary roleMap = null;
    private PdfDictionary sourceRoleMap = null;
    private PdfDictionary sourceClassMap = null;
    private Dictionary<String, Boolean> openedDocuments = new Dictionary<String, Boolean>();
    public enum returnType {BELOW,FOUND,ABOVE,NOTFOUND};
    public static PdfName [] standardTypes = {PdfName.P, PdfName.H, PdfName.H1, PdfName.H2, PdfName.H3, PdfName.H4,
            PdfName.H5, PdfName.H6, PdfName.L, PdfName.LBL, PdfName.LI, PdfName.LBODY, PdfName.TABLE, PdfName.TABLEROW,
            PdfName.TH, PdfName.TD, PdfName.THEAD, PdfName.TBODY, PdfName.TFOOT, PdfName.SPAN, PdfName.QUOTE, PdfName.NOTE,
            PdfName.REFERENCE, PdfName.BIBENTRY, PdfName.CODE, PdfName.LINK, PdfName.ANNOT, PdfName.RUBY, PdfName.WARICHU};

    protected internal PdfStructTreeController(PdfReader reader, PdfCopy writer){
        if (!writer.IsTagged())
            throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("no.structtreeroot.found"));
        this.writer = writer;
        structureTreeRoot = writer.StructureTreeRoot;
        structureTreeRoot.Put(PdfName.PARENTTREE, new PdfDictionary(PdfName.STRUCTELEM));
        SetReader(reader);
    }

    protected internal void SetReader(PdfReader reader){
        this.reader = reader;
        fileName = reader.SafeFile.filename;
        if (!openedDocuments.ContainsKey(fileName)) {
            openedDocuments.Add(fileName, false);
        }
        PdfObject obj = reader.Catalog.Get(PdfName.STRUCTTREEROOT);
        obj = GetDirectObject(obj);
        if ((obj == null) || (!obj.IsDictionary()))
            throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("no.structtreeroot.found"));
        structTreeRoot = (PdfDictionary)obj;
        obj = PdfStructTreeController.GetDirectObject(structTreeRoot.Get(PdfName.PARENTTREE));
        if (!obj.IsDictionary())
            throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("the.document.does.not.contain.parenttree"));
        parentTree = (PdfDictionary)obj;
        sourceRoleMap = null;
        sourceClassMap = null;
    }

    public static PdfObject GetDirectObject(PdfObject obj) {
        if (obj == null)
            return null;
        while (obj.IsIndirect())
            obj = PdfReader.GetPdfObjectRelease(obj);
        return obj;
    }

    public void CopyStructTreeForPage(PdfNumber sourceArrayNumber, int newArrayNumber) {
        if (!openedDocuments[fileName])
        {
            PdfObject res = writer.CopyObject(writer.CopyObject(structTreeRoot.Get(PdfName.K), true, true));
            if (!(res is PdfIndirectReference))
                res = writer.AddToBody(res).IndirectReference;
            structureTreeRoot.AddPageMark(newArrayNumber, (PdfIndirectReference)res);
            AddKid(structureTreeRoot, res);
            openedDocuments.Add(fileName, true);
        }
        if (CopyPageMarks(parentTree, sourceArrayNumber, newArrayNumber) == returnType.NOTFOUND) {
            throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("structparent.not.found"));
        }
    }

    private returnType CopyPageMarks(PdfDictionary parentTree, PdfNumber arrayNumber, int newArrayNumber) {
        PdfArray pages = (PdfArray)GetDirectObject(parentTree.Get(PdfName.NUMS));
        if (pages == null) {
            PdfArray kids = (PdfArray)GetDirectObject(parentTree.Get(PdfName.KIDS));
            if (kids == null)
                return returnType.NOTFOUND;
            int cur = kids.Size/2;
            int begin = 0;
            while (true) {
                PdfDictionary kidTree = (PdfDictionary)GetDirectObject(kids[cur + begin]);
                switch (CopyPageMarks(kidTree,arrayNumber,newArrayNumber)) {
                    case returnType.FOUND:
                        return returnType.FOUND;
                    case returnType.ABOVE:
                        begin += cur;
                        cur /= 2;
                        if (cur == 0)
                            cur = 1;
                        if (cur + begin == kids.Size)
                            return returnType.ABOVE;
                        break;
                    case returnType.BELOW:
                        if (cur + begin == 0)
                            return returnType.BELOW;
                        if (cur == 0)
                            return returnType.NOTFOUND;
                        cur /= 2;
                        break;
                    default:
                        return returnType.NOTFOUND;
                }
            }
        } else {
            return FindAndCopyMarks(pages, arrayNumber.IntValue, newArrayNumber);
        }
    }

    private returnType FindAndCopyMarks(PdfArray pages, int arrayNumber, int newArrayNumber){
        if (pages.GetAsNumber(0).IntValue > arrayNumber)
            return returnType.BELOW;
        if (pages.GetAsNumber(pages.Size - 2).IntValue < arrayNumber)
            return returnType.ABOVE;
        int cur = pages.Size/4;
        int begin = 0;
        int curNumber;
        while (true) {
            curNumber = pages.GetAsNumber((begin + cur) * 2).IntValue;
            if (curNumber == arrayNumber) {
                PdfObject res = writer.CopyObject(pages[((begin + cur) * 2 + 1)], true, false);
                if (!(res is PdfIndirectReference))
                    res = writer.AddToBody(res).IndirectReference;
                structureTreeRoot.AddPageMark(newArrayNumber, (PdfIndirectReference)res);
                AddKid(structureTreeRoot, res);
                return returnType.FOUND;
            }
            if (curNumber < arrayNumber) {
                begin += cur;
                cur /= 2;
                if (cur == 0)
                    cur = 1;
                if (cur + begin == pages.Size)
                    return returnType.NOTFOUND;
                continue;
            }
            if (cur + begin == 0)
                return returnType.BELOW;
            if (cur == 0)
                return returnType.NOTFOUND;
            cur /= 2;
        }
    }
    
    private static PdfArray GetDirectArray(PdfArray input) {
        PdfArray output = new PdfArray();
        for (int i = 0; i < input.Size; ++i) {
            PdfObject value = GetDirectObject(input[i]);
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
        }
        else if (value1.IsName()) {
            return value1.Equals(value2);
        }
        else if (value1.IsNumber()){
            if (value1 == value2)
                return true;
            if (value2 is PdfNumber) {
                return ((PdfNumber)value1).DoubleValue == ((PdfNumber)value2).DoubleValue;
            }
            return false;
        }
        else if (value1.IsNull()){
            if (value1 == value2)
                return true;
            if (value2 is PdfNull)
                return true;
            return false;
        }
        else if (value1.IsString()){
            if (value1 == value2)
                return true;
            if (value2 is PdfString) {
                return ((((PdfString)value2) == null && ((PdfString)value1).ToString() == null)
                        || (((PdfString)value1).ToString() != null && ((PdfString)value1).ToString().Equals(((PdfString)value2).ToString())));
            }
            return false;
        }
        if (value1.IsArray()) {
            PdfArray array1 = (PdfArray)value1;
            PdfArray array2 = (PdfArray)value2;
            if (array1.Size != array2.Size)
                return false;
            for (int i = 0; i < array1.Size; ++i)
                if (!CompareObjects(array1[i],array2[i]))
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
                    AddClass(array[i]);
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
                    throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("conflict.input.classmap",name));
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
        foreach (PdfName name in standardTypes) {
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
                throw new BadPdfFormatException(MessageLocalization.GetComposedMessage("conflict.input.rolemap",obj));
            }
        } else {
            roleMap.Put(structType,obj);
        }
    }

    protected void AddKid(PdfDictionary parent, PdfObject kid){
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

}
}