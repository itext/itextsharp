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
using System.util.collections;
using iTextSharp.text.error_messages;
using iTextSharp.text.log;

namespace iTextSharp.text.pdf.intern
{
    public abstract class PdfAChecker {

        protected static ILogger LOGGER = LoggerFactory.GetLogger(typeof(PdfAChecker));

        protected PdfAConformanceLevel conformanceLevel;
        protected Dictionary<RefKey, PdfObject> cachedObjects = new Dictionary<RefKey, PdfObject>();
        private HashSet2<PdfName> keysForCheck;
        private static byte[] emptyByteArray = new byte[] { };
        protected String pdfaOutputIntentColorSpace = null;
 	    protected PdfObject pdfaDestOutputIntent = null;
 	    protected bool isCheckOutputIntent = false;
        internal PdfAChecker(PdfAConformanceLevel conformanceLevel) {
            keysForCheck = InitKeysForCheck();
            this.conformanceLevel = conformanceLevel;
        }

        abstract protected HashSet2<PdfName> InitKeysForCheck();

        virtual public void CacheObject(PdfIndirectReference iref, PdfObject obj) {
            if (obj.Type == 0) {
                cachedObjects[new RefKey(iref)] = obj;
            }
            else if (obj is PdfDictionary) {
                cachedObjects[new RefKey(iref)] = CleverPdfDictionaryClone((PdfDictionary) obj);
            }
            else if (obj.IsArray()) {
                cachedObjects[new RefKey(iref)] = CleverPdfArrayClone((PdfArray) obj);
            }
        }

        abstract public void Close(PdfWriter writer);

        private PdfObject CleverPdfArrayClone(PdfArray array) {
            PdfArray newArray = new PdfArray();
            for (int i = 0; i < array.Size; i++) {
                PdfObject obj = array[i];
                if (obj is PdfDictionary)
                    newArray.Add(CleverPdfDictionaryClone((PdfDictionary) obj));
                else
                    newArray.Add(obj);
            }

            return newArray;
        }

        private PdfObject CleverPdfDictionaryClone(PdfDictionary dict) {
            PdfDictionary newDict;
            if (dict.IsStream()) {
                newDict = new PdfStream(emptyByteArray);
                newDict.Remove(PdfName.LENGTH);
            }
            else
                newDict = new PdfDictionary();

            foreach (PdfName key in dict.Keys)
                if (keysForCheck.Contains(key))
                    newDict.Put(key, dict.Get(key));

            return newDict;
        }

        virtual protected PdfObject GetDirectObject(PdfObject obj) {
            if (obj == null)
                return null;
            //use counter to prevent indirect reference cycling
            int count = 0;
            // resolve references
            while (obj is PdfIndirectReference) {
                PdfObject curr;
                if (obj.IsIndirect())
                    curr = PdfReader.GetPdfObject(obj);
                else
                    cachedObjects.TryGetValue(new RefKey((PdfIndirectReference) obj), out curr);
                if (curr == null) break;
                obj = curr;
                //10 - is max allowed reference chain
                if (count++ > 10)
                    break;
            }
            return obj;
        }

        virtual protected PdfDictionary GetDirectDictionary(PdfObject obj) {
            obj = GetDirectObject(obj);
            if (obj != null && obj is PdfDictionary)
                return (PdfDictionary) obj;
            return null;
        }

        virtual protected PdfStream GetDirectStream(PdfObject obj) {
            obj = GetDirectObject(obj);
            if (obj != null && obj.IsStream())
                return (PdfStream) obj;
            return null;
        }

        virtual protected PdfArray GetDirectArray(PdfObject obj) {
            obj = GetDirectObject(obj);
            if (obj != null && obj.IsArray())
                return (PdfArray) obj;
            return null;
        }

        protected abstract void CheckFont(PdfWriter writer, int key, Object obj1);

        protected abstract void CheckImage(PdfWriter writer, int key, Object obj1);

        abstract protected void CheckInlineImage(PdfWriter writer, int key, Object obj1);

        abstract protected void CheckFormXObj(PdfWriter writer, int key, Object obj1);

        protected abstract void CheckGState(PdfWriter writer, int key, Object obj1);

        protected abstract void CheckLayer(PdfWriter writer, int key, Object obj1);

        protected abstract void CheckTrailer(PdfWriter writer, int key, Object obj1);

        protected abstract void CheckStream(PdfWriter writer, int key, Object obj1);

        protected abstract void CheckFileSpec(PdfWriter writer, int key, Object obj1);

        protected abstract void CheckPdfObject(PdfWriter writer, int key, Object obj1);

        protected abstract void CheckCanvas(PdfWriter writer, int key, Object obj1);

        protected abstract void CheckColor(PdfWriter writer, int key, Object obj1);

        protected abstract void CheckAnnotation(PdfWriter writer, int key, Object obj1);

        protected abstract void CheckAction(PdfWriter writer, int key, Object obj1);

        protected abstract void CheckForm(PdfWriter writer, int key, Object obj1);

        protected abstract void CheckStructElem(PdfWriter writer, int key, Object obj1);

        abstract protected void CheckOutputIntent(PdfWriter writer, int key, Object obj1);


        internal void CheckPdfAConformance(PdfWriter writer, int key, Object obj1) {
            if(writer == null || !writer.IsPdfIso())
                return;
            switch(key) {
                case PdfIsoKeys.PDFISOKEY_FONT:
                    CheckFont(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_IMAGE:
                    CheckImage(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_GSTATE:
                    CheckGState(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_LAYER:
                    CheckLayer(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_TRAILER:
                    CheckTrailer(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_STREAM:
                    CheckStream(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_FILESPEC:
                    CheckFileSpec(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_OBJECT:
                    CheckPdfObject(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_CANVAS:
                    CheckCanvas(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_COLOR:
                case PdfIsoKeys.PDFISOKEY_CMYK:
                case PdfIsoKeys.PDFISOKEY_RGB:
                    CheckColor(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_ANNOTATION:
                    CheckAnnotation(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_ACTION:
                    CheckAction(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_FORM:
                    CheckForm(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_STRUCTELEM:
                    if(CheckStructure(conformanceLevel))
                        CheckStructElem(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_INLINE_IMAGE:
                    CheckInlineImage(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_OUTPUTINTENT:
                    CheckOutputIntent(writer, key, obj1);
                    break;
                case PdfIsoKeys.PDFISOKEY_FORM_XOBJ:
                    CheckFormXObj(writer, key, obj1);
                    break;
                default:
                    break;
            }
        }

        public static bool CheckStructure(PdfAConformanceLevel conformanceLevel) {
            return conformanceLevel == PdfAConformanceLevel.PDF_A_1A
                || conformanceLevel == PdfAConformanceLevel.PDF_A_2A
                || conformanceLevel == PdfAConformanceLevel.PDF_A_3A;
        }

        protected static bool CheckFlag(int flags, int flag) {
            return (flags & flag) != 0;
        }

        protected void CheckOutputIntentsInStamperMode(PdfWriter writer) {
 	       if (writer is PdfAStamperImp && !isCheckOutputIntent) {
 	            PdfReader pdfReader = ((PdfAStamperImp) writer).GetPdfReader();
 	            PdfArray outPutIntentsDic = pdfReader.Catalog.GetAsArray(PdfName.OUTPUTINTENTS);
 	            if (outPutIntentsDic != null) {
 	                if (outPutIntentsDic.Size > 1) {
 	                    throw new PdfAConformanceException(outPutIntentsDic, MessageLocalization.GetComposedMessage("a.pdfa.file.may.have.only.one.pdfa.outputintent"));
 		                } else {
 	                        PdfDictionary outPutIntentDic = outPutIntentsDic.GetAsDict(0);
                            if (outPutIntentDic != null) {
 	                            CheckPdfObject(writer, PdfIsoKeys.PDFISOKEY_OBJECT, outPutIntentDic);
 	                        }
 		                }
 	            }
 	        }
 	    }
    }
}
