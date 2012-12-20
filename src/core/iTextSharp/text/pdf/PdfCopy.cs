using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
/*
 * $Id$
 * 
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
    * Make copies of PDF documents. Documents can be edited after reading and
    * before writing them out.
    * @author Mark Thompson
    */

    public class PdfCopy : PdfWriter {
        /**
        * This class holds information about indirect references, since they are
        * renumbered by iText.
        */
        public class IndirectReferences {
            PdfIndirectReference theRef;
            bool hasCopied;
            internal IndirectReferences(PdfIndirectReference refi) {
                theRef = refi;
                hasCopied = false;
            }
            internal void SetCopied() { hasCopied = true; }
            internal bool Copied {
                get {
                    return hasCopied; 
                }
                set {
                    hasCopied = value;
                }
            }
            internal PdfIndirectReference Ref {
                get {
                    return theRef; 
                }
            }
        };
        protected internal Dictionary<RefKey, IndirectReferences> indirects;
        protected Dictionary<PdfReader,Dictionary<RefKey, IndirectReferences>>  indirectMap;
        protected Dictionary<PdfObject, PdfObject> parentObjects;
        protected Dictionary<PdfObject, PdfObject> disableIndirects;
        protected PdfReader reader;
        protected PdfIndirectReference acroForm;
        protected int[] namePtr = {0};
        /** Holds value of property rotateContents. */
        private bool rotateContents = true;
        protected internal PdfArray fieldArray;
        protected internal Dictionary<PdfTemplate, object> fieldTemplates; 
        private PdfStructTreeController structTreeController = null;
        private int currentStructArrayNumber = 0;

        /**
        * A key to allow us to hash indirect references
        */
        public class RefKey {
            internal int num;
            internal int gen;
            internal RefKey(int num, int gen) {
                this.num = num;
                this.gen = gen;
            }
            internal RefKey(PdfIndirectReference refi) {
                num = refi.Number;
                gen = refi.Generation;
            }
            internal RefKey(PRIndirectReference refi) {
                num = refi.Number;
                gen = refi.Generation;
            }
            public override int GetHashCode() {
                return (gen<<16)+num;
            }
            public override bool Equals(Object o) {
                if (!(o is RefKey)) return false;
                RefKey other = (RefKey)o;
                return this.gen == other.gen && this.num == other.num;
            }
            public override String ToString() {
                return "" + num + " " + gen;
            }
        }
        
        /**
        * Constructor
        * @param document
        * @param os outputstream
        */
        public PdfCopy(Document document, Stream os) : base(new PdfDocument(), os) {
            document.AddDocListener(pdf);
            pdf.AddWriter(this);
            indirectMap = new Dictionary<PdfReader,Dictionary<RefKey,IndirectReferences>>();
            parentObjects = new Dictionary<PdfObject, PdfObject>();
            disableIndirects = new Dictionary<PdfObject, PdfObject>();
        }

        /**
         * Setting page events isn't possible with Pdf(Smart)Copy.
         * Use the PageStamp class if you want to add content to copied pages.
         * @see com.itextpdf.text.pdf.PdfWriter#setPageEvent(com.itextpdf.text.pdf.PdfPageEvent)
         */
        public override IPdfPageEvent PageEvent {
            set {
                throw new InvalidOperationException();
            }
        }

        /** Checks if the content is automatically adjusted to compensate
        * the original page rotation.
        * @return the auto-rotation status
        */    
        /** Flags the content to be automatically adjusted to compensate
        * the original page rotation. The default is <CODE>true</CODE>.
        * @param rotateContents <CODE>true</CODE> to set auto-rotation, <CODE>false</CODE>
        * otherwise
        */    
        public bool RotateContents {
            set {
                rotateContents = value;
            }
            get {
                return rotateContents;
            }
        }

        /**
        * Grabs a page from the input document
        * @param reader the reader of the document
        * @param pageNumber which page to get
        * @return the page
        */
        public override PdfImportedPage GetImportedPage(PdfReader reader, int pageNumber) {
             if (structTreeController != null)
                structTreeController.reader = null;
            return GetImportedPageImpl(reader, pageNumber);
        }

        public PdfImportedPage GetImportedPage(PdfReader reader, int pageNumber, bool keepTaggedPdfStructure) {
            if (!keepTaggedPdfStructure)
                return GetImportedPage(reader, pageNumber);
            else {
                if (structTreeController != null) {
                    if (reader != structTreeController.reader)
                        structTreeController.SetReader(reader);
                } else {
                    structTreeController = new PdfStructTreeController(reader, this);
                }
                disableIndirects.Clear();
                parentObjects.Clear();
                return GetImportedPageImpl(reader, pageNumber);
            }
        }

        protected PdfImportedPage GetImportedPageImpl(PdfReader reader, int pageNumber) {
                if (currentPdfReaderInstance != null) {
                    if (currentPdfReaderInstance.Reader != reader) {
                        // TODO: Removed - the user should be responsible for closing all PdfReaders.  But, this could cause a lot of memory leaks in code out there that hasn't been properly closing things - maybe add a finalizer to PdfReader that calls PdfReader#close() ??            	
                        //try {
                        //    currentPdfReaderInstance.Reader.Close();
                        //    currentPdfReaderInstance.ReaderFile.Close();
                        //}
                        //catch (IOException) {
                        //    // empty on purpose
                        //}
                        currentPdfReaderInstance = base.GetPdfReaderInstance(reader);
                    }
                }
                else {
                    currentPdfReaderInstance = base.GetPdfReaderInstance(reader);
                }
                return currentPdfReaderInstance.GetImportedPage(pageNumber);            
            }
        
        
        /**
        * Translate a PRIndirectReference to a PdfIndirectReference
        * In addition, translates the object numbers, and copies the
        * referenced object to the output file.
        * NB: PRIndirectReferences (and PRIndirectObjects) really need to know what
        * file they came from, because each file has its own namespace. The translation
        * we do from their namespace to ours is *at best* heuristic, and guaranteed to
        * fail under some circumstances.
        */
        protected virtual PdfIndirectReference CopyIndirect(PRIndirectReference inp, bool keepStructure, bool directRootKids) {
            PdfIndirectReference theRef;
            RefKey key = new RefKey(inp);
            IndirectReferences iRef;
            indirects.TryGetValue(key, out iRef);
            PdfObject obj = PdfReader.GetPdfObjectRelease(inp);
            if ((keepStructure) && (directRootKids))
                if (obj is PdfDictionary) {
                    PdfDictionary dict = (PdfDictionary) obj;
                    if (dict.Contains(PdfName.PG))
                        return null;
                }
            if (iRef != null) {
                theRef = iRef.Ref;
                if (iRef.Copied) {
                    return theRef;
                }
            }
            else {
                theRef = body.PdfIndirectReference;
                iRef = new IndirectReferences(theRef);
                indirects[key] =  iRef;
            }
            if (obj != null && obj.IsDictionary()) {
                PdfObject type = PdfReader.GetPdfObjectRelease(((PdfDictionary)obj).Get(PdfName.TYPE));
                if (type != null && PdfName.PAGE.Equals(type)) {
                    return theRef;
                }
            }
            iRef.SetCopied();
            parentObjects[obj] = inp;
            PdfObject res = CopyObject(obj, keepStructure, directRootKids);
            if (disableIndirects.ContainsKey(obj))
                iRef.Copied = false;
            if ((res != null) && !(res is PdfNull)) {
                AddToBody(res, theRef);
                return theRef;
            }
            indirects.Remove(key);
            return null;
        }
         /**
        * Translate a PRIndirectReference to a PdfIndirectReference
        * In addition, translates the object numbers, and copies the
        * referenced object to the output file.
        * NB: PRIndirectReferences (and PRIndirectObjects) really need to know what
        * file they came from, because each file has its own namespace. The translation
        * we do from their namespace to ours is *at best* heuristic, and guaranteed to
        * fail under some circumstances.
        */
        protected virtual PdfIndirectReference CopyIndirect(PRIndirectReference inp) {
            return CopyIndirect(inp, false, false);
        }

        /**
        * Translate a PRDictionary to a PdfDictionary. Also translate all of the
        * objects contained in it.
        */
        protected PdfDictionary CopyDictionary(PdfDictionary inp, bool keepStruct, bool directRootKids) {
            PdfDictionary outp = new PdfDictionary();
            PdfObject type = PdfReader.GetPdfObjectRelease(inp.Get(PdfName.TYPE));
            
             if (keepStruct)
            {
                if ((directRootKids) && (inp.Contains(PdfName.PG)))
                {
                    PdfObject curr = inp;
                    disableIndirects.Add(curr, null);
                    while (parentObjects.ContainsKey(curr) && !(disableIndirects.ContainsKey(curr))) {
                        curr = parentObjects[curr];
                        disableIndirects.Add(curr, null);
                    }
                    return null;
                }
                    
                PdfName structType = inp.GetAsName(PdfName.S);
                structTreeController.AddRole(structType);
                structTreeController.AddClass(inp);
            }

            foreach (PdfName key in inp.Keys) {
                PdfObject value = inp.Get(key);
                if (structTreeController != null && structTreeController.reader != null && (key.Equals(PdfName.STRUCTPARENT) || key.Equals(PdfName.STRUCTPARENTS))) {
                    outp.Put(key, new PdfNumber(currentStructArrayNumber));
                    structTreeController.CopyStructTreeForPage((PdfNumber)value, currentStructArrayNumber++);
                    continue;
                }
                if (type != null && PdfName.PAGE.Equals(type)) {
                    if (!key.Equals(PdfName.B) && !key.Equals(PdfName.PARENT)) {
                        parentObjects[value] = inp;
                        PdfObject res = CopyObject(value, keepStruct, directRootKids);
                        if ((res != null) && !(res is PdfNull))
                            outp.Put(key, res);
                    }
                }
                else {
                    PdfObject res = CopyObject(value, keepStruct, directRootKids);
                    if ((res != null) && !(res is PdfNull))
                        outp.Put(key, res);
                }
            }
            return outp;
        }

        /**
        * Translate a PRDictionary to a PdfDictionary. Also translate all of the
        * objects contained in it.
        */
        protected PdfDictionary CopyDictionary(PdfDictionary inp) {
            return CopyDictionary(inp, false, false);
        }

        /**
        * Translate a PRStream to a PdfStream. The data part copies itself.
        */
        protected PdfStream CopyStream(PRStream inp) {
            PRStream outp = new PRStream(inp, null);
            
            foreach (PdfName key in inp.Keys) {
                PdfObject value = inp.Get(key);
                parentObjects[value] = inp;
                PdfObject res = CopyObject(value);
                if ((res != null) && !(res is PdfNull))
                    outp.Put(key, res);
            }
            
            return outp;
        }
        
        
        /**
        * Translate a PRArray to a PdfArray. Also translate all of the objects contained
        * in it
        */
        protected PdfArray CopyArray(PdfArray inp, bool keepStruct, bool directRootKids) {
            PdfArray outp = new PdfArray();
            
            foreach (PdfObject value in inp.ArrayList) {
                parentObjects[value] = inp;
                PdfObject res = CopyObject(value, keepStruct, directRootKids);
                if ((res != null) && !(res is PdfNull))
                    outp.Add(res);
            }
            return outp;
        }

        /**
        * Translate a PRArray to a PdfArray. Also translate all of the objects contained
        * in it
        */
        protected PdfArray CopyArray(PdfArray inp) {
            return CopyArray(inp, false, false);
        }

        /**
        * Translate a PR-object to a Pdf-object
        */
        protected internal PdfObject CopyObject(PdfObject inp, bool keepStruct, bool directRootKidds) {
            if (inp == null)
                return PdfNull.PDFNULL;
            switch (inp.Type) {
                case PdfObject.DICTIONARY:
                    return CopyDictionary((PdfDictionary)inp, keepStruct, directRootKidds);
                case PdfObject.INDIRECT:
                    if (!keepStruct && !directRootKidds)
                        // fix for PdfSmartCopy
                        return CopyIndirect((PRIndirectReference)inp);
                    else
                        return CopyIndirect((PRIndirectReference)inp, keepStruct, directRootKidds);
                case PdfObject.ARRAY:
                    return CopyArray((PdfArray)inp, keepStruct, directRootKidds);
                case PdfObject.NUMBER:
                case PdfObject.NAME:
                case PdfObject.STRING:
                case PdfObject.NULL:
                case PdfObject.BOOLEAN:
                case 0:
                    return inp;
                case PdfObject.STREAM:
                    return CopyStream((PRStream)inp);
                    //                return in;
                default:
                    if (inp.Type < 0) {
                        String lit = ((PdfLiteral)inp).ToString();
                        if (lit.Equals("true") || lit.Equals("false")) {
                            return new PdfBoolean(lit);
                        }
                        return new PdfLiteral(lit);
                    }
                    return null;
            }
        }

         /**
        * Translate a PR-object to a Pdf-object
        */
        protected internal PdfObject CopyObject(PdfObject inp) {
            return CopyObject(inp, false, false);
        }

        /**
        * convenience method. Given an importedpage, set our "globals"
        */
        protected int SetFromIPage(PdfImportedPage iPage) {
            int pageNum = iPage.PageNumber;
            PdfReaderInstance inst = currentPdfReaderInstance = iPage.PdfReaderInstance;
            reader = inst.Reader;
            SetFromReader(reader);
            return pageNum;
        }
        
        /**
        * convenience method. Given a reader, set our "globals"
        */
        protected void SetFromReader(PdfReader reader) {
            this.reader = reader;
            indirectMap.TryGetValue(reader, out indirects);
            if (indirects == null) {
                indirects = new Dictionary<RefKey,IndirectReferences>();
                indirectMap[reader] = indirects;
                PdfDictionary catalog = reader.Catalog;
                PRIndirectReference refi = null;
                PdfObject o = catalog.Get(PdfName.ACROFORM);
                if (o == null || o.Type != PdfObject.INDIRECT)
                    return;
                refi = (PRIndirectReference)o;
                if (acroForm == null) acroForm = body.PdfIndirectReference;
                indirects[new RefKey(refi)] =  new IndirectReferences(acroForm);
            }
        }
        /**
        * Add an imported page to our output
        * @param iPage an imported page
        * @throws IOException, BadPdfFormatException
        */
        public void AddPage(PdfImportedPage iPage) {
            int pageNum = SetFromIPage(iPage);
            
            PdfDictionary thePage = reader.GetPageN(pageNum);
            PRIndirectReference origRef = reader.GetPageOrigRef(pageNum);
            reader.ReleasePage(pageNum);
            RefKey key = new RefKey(origRef);
            PdfIndirectReference pageRef;
            IndirectReferences iRef;
            indirects.TryGetValue(key, out iRef);
            if (iRef != null && !iRef.Copied) {
                pageReferences.Add(iRef.Ref);
                iRef.SetCopied();
            }
            pageRef = CurrentPage;
            if (iRef == null) {
                iRef = new IndirectReferences(pageRef);
                indirects[key] = iRef;
            }
            iRef.SetCopied();
            PdfDictionary newPage = CopyDictionary(thePage);
            root.AddPage(newPage);
            iPage.SetCopied();
            ++currentPageNumber;
        }

        /**
         * Adds a blank page.
         * @param	rect The page dimension
         * @param	rotation The rotation angle in degrees
         * @since	2.1.5
         * @throws DocumentException
         */
        public void AddPage(Rectangle rect, int rotation) {
            PdfRectangle mediabox = new PdfRectangle(rect, rotation);
            PageResources resources = new PageResources();
            PdfPage page = new PdfPage(mediabox, new Dictionary<String, PdfRectangle>(), resources.Resources, 0);
            page.Put(PdfName.TABS, Tabs);
            root.AddPage(page);
            ++currentPageNumber;
        }

        /**
        * Copy the acroform for an input document. Note that you can only have one,
        * we make no effort to merge them.
        * @param reader The reader of the input file that is being copied
        * @throws IOException, BadPdfFormatException
        */
        public void CopyAcroForm(PdfReader reader) {
            SetFromReader(reader);
            
            PdfDictionary catalog = reader.Catalog;
            PRIndirectReference hisRef = null;
            PdfObject o = catalog.Get(PdfName.ACROFORM);
            if (o != null && o.Type == PdfObject.INDIRECT)
                hisRef = (PRIndirectReference)o;
            if (hisRef == null) return; // bugfix by John Engla
            RefKey key = new RefKey(hisRef);
            PdfIndirectReference myRef;
            IndirectReferences iRef;
            indirects.TryGetValue(key, out iRef);
            if (iRef != null) {
                acroForm = myRef = iRef.Ref;
            }
            else {
                acroForm = myRef = body.PdfIndirectReference;
                iRef = new IndirectReferences(myRef);
                indirects[key] =  iRef;
            }
            if (! iRef.Copied) {
                iRef.SetCopied();
                PdfDictionary theForm = CopyDictionary((PdfDictionary)PdfReader.GetPdfObject(hisRef));
                AddToBody(theForm, myRef);
            }
        }
        
        /*
        * the getCatalog method is part of PdfWriter.
        * we wrap this so that we can extend it
        */
        protected override PdfDictionary GetCatalog(PdfIndirectReference rootObj) {
            PdfDictionary theCat = pdf.GetCatalog(rootObj);
            BuildStructTreeRootForTagged(theCat);
            if (fieldArray == null) {
                if (acroForm != null) theCat.Put(PdfName.ACROFORM, acroForm);
            }
            else
                AddFieldResources(theCat);
            return theCat;
        }
        
        private void AddFieldResources(PdfDictionary catalog) {
            if (fieldArray == null)
                return;
            PdfDictionary acroForm = new PdfDictionary();
            catalog.Put(PdfName.ACROFORM, acroForm);
            acroForm.Put(PdfName.FIELDS, fieldArray);
            acroForm.Put(PdfName.DA, new PdfString("/Helv 0 Tf 0 g "));
            if (fieldTemplates.Count == 0)
                return;
            PdfDictionary dr = new PdfDictionary();
            acroForm.Put(PdfName.DR, dr);
            foreach (PdfTemplate template in fieldTemplates.Keys) {
                PdfFormField.MergeResources(dr, (PdfDictionary)template.Resources);
            }
            PdfDictionary fonts = dr.GetAsDict(PdfName.FONT);
            if (fonts == null) {
                fonts = new PdfDictionary();
                dr.Put(PdfName.FONT, fonts);
            }
            if (!fonts.Contains(PdfName.HELV)) {
                PdfDictionary dic = new PdfDictionary(PdfName.FONT);
                dic.Put(PdfName.BASEFONT, PdfName.HELVETICA);
                dic.Put(PdfName.ENCODING, PdfName.WIN_ANSI_ENCODING);
                dic.Put(PdfName.NAME, PdfName.HELV);
                dic.Put(PdfName.SUBTYPE, PdfName.TYPE1);
                fonts.Put(PdfName.HELV, AddToBody(dic).IndirectReference);
            }
            if (!fonts.Contains(PdfName.ZADB)) {
                PdfDictionary dic = new PdfDictionary(PdfName.FONT);
                dic.Put(PdfName.BASEFONT, PdfName.ZAPFDINGBATS);
                dic.Put(PdfName.NAME, PdfName.ZADB);
                dic.Put(PdfName.SUBTYPE, PdfName.TYPE1);
                fonts.Put(PdfName.ZADB, AddToBody(dic).IndirectReference);
            }
        }
        
        /**
        * Signals that the <CODE>Document</CODE> was closed and that no other
        * <CODE>Elements</CODE> will be added.
        * <P>
        * The pages-tree is built and written to the outputstream.
        * A Catalog is constructed, as well as an Info-object,
        * the referencetable is composed and everything is written
        * to the outputstream embedded in a Trailer.
        */
        
        public override void Close() {
            if (open) {
                PdfReaderInstance ri = currentPdfReaderInstance;
                pdf.Close();
                base.Close();
                // Users are responsible for closing PdfReaders
                //if (ri != null) {
                //    try {
                //        ri.Reader.Close();
                //        ri.ReaderFile.Close();
                //    }
                //    catch (IOException) {
                //        // empty on purpose
                //    }
                //}
            }
        }

        public override void AddAnnotation(PdfAnnotation annot) {  }
        internal override PdfIndirectReference Add(PdfPage page, PdfContents contents) { return null; }

        public override void FreeReader(PdfReader reader) {
            indirectMap.Remove(reader);
// TODO: Removed - the user should be responsible for closing all PdfReaders.  But, this could cause a lot of memory leaks in code out there that hasn't been properly closing things - maybe add a finalizer to PdfReader that calls PdfReader#close() ??            	
            //if (currentPdfReaderInstance != null) {
            //    if (currentPdfReaderInstance.Reader == reader) {
            //        try {
            //            currentPdfReaderInstance.Reader.Close();
            //            currentPdfReaderInstance.ReaderFile.Close();
            //        }
            //        catch (IOException) {
            //            // empty on purpose
            //        }
                    currentPdfReaderInstance = null;
            //    }
            //}
            base.FreeReader(reader);
        }

        /**
        * Create a page stamp. New content and annotations, including new fields, are allowed.
        * The fields added cannot have parents in another pages. This method modifies the PdfReader instance.<p>
        * The general usage to stamp something in a page is:
        * <p>
        * <pre>
        * PdfImportedPage page = copy.GetImportedPage(reader, 1);
        * PdfCopy.PageStamp ps = copy.CreatePageStamp(page);
        * ps.AddAnnotation(PdfAnnotation.CreateText(copy, new Rectangle(50, 180, 70, 200), "Hello", "No Thanks", true, "Comment"));
        * PdfContentByte under = ps.GetUnderContent();
        * under.AddImage(img);
        * PdfContentByte over = ps.GetOverContent();
        * over.BeginText();
        * over.SetFontAndSize(bf, 18);
        * over.SetTextMatrix(30, 30);
        * over.ShowText("total page " + totalPage);
        * over.EndText();
        * ps.AlterContents();
        * copy.AddPage(page);
        * </pre>
        * @param iPage an imported page
        * @return the <CODE>PageStamp</CODE>
        */
        public PageStamp CreatePageStamp(PdfImportedPage iPage) {
            int pageNum = iPage.PageNumber;
            PdfReader reader = iPage.PdfReaderInstance.Reader;
            PdfDictionary pageN = reader.GetPageN(pageNum);
            return new PageStamp(reader, pageN, this);
        }

        public class PageStamp {
            
            PdfDictionary pageN;
            PdfCopy.StampContent under;
            PdfCopy.StampContent over;
            PageResources pageResources;
            PdfReader reader;
            PdfCopy cstp;
            
            internal PageStamp(PdfReader reader, PdfDictionary pageN, PdfCopy cstp) {
                this.pageN = pageN;
                this.reader = reader;
                this.cstp = cstp;
            }
            
            public PdfContentByte GetUnderContent(){
                if (under == null) {
                    if (pageResources == null) {
                        pageResources = new PageResources();
                        PdfDictionary resources = pageN.GetAsDict(PdfName.RESOURCES);
                        pageResources.SetOriginalResources(resources, cstp.namePtr);
                    }
                    under = new PdfCopy.StampContent(cstp, pageResources);
                }
                return under;
            }
            
            public PdfContentByte GetOverContent(){
                if (over == null) {
                    if (pageResources == null) {
                        pageResources = new PageResources();
                        PdfDictionary resources = pageN.GetAsDict(PdfName.RESOURCES);
                        pageResources.SetOriginalResources(resources, cstp.namePtr);
                    }
                    over = new PdfCopy.StampContent(cstp, pageResources);
                }
                return over;
            }

            public void AlterContents() {
                if (over == null && under == null)
                    return;
                PdfArray ar = null;
                PdfObject content = PdfReader.GetPdfObject(pageN.Get(PdfName.CONTENTS), pageN);
                if (content == null) {
                    ar = new PdfArray();
                    pageN.Put(PdfName.CONTENTS, ar);
                }
                else if (content.IsArray()) {
                    ar = (PdfArray)content;
                }
                else if (content.IsStream()) {
                    ar = new PdfArray();
                    ar.Add(pageN.Get(PdfName.CONTENTS));
                    pageN.Put(PdfName.CONTENTS, ar);
                }
                else {
                    ar = new PdfArray();
                    pageN.Put(PdfName.CONTENTS, ar);
                }
                ByteBuffer out_p = new ByteBuffer();
                if (under != null) {
                    out_p.Append(PdfContents.SAVESTATE);
                    ApplyRotation(pageN, out_p);
                    out_p.Append(under.InternalBuffer);
                    out_p.Append(PdfContents.RESTORESTATE);
                }
                if (over != null)
                    out_p.Append(PdfContents.SAVESTATE);
                PdfStream stream = new PdfStream(out_p.ToByteArray());
                stream.FlateCompress(cstp.CompressionLevel);
                PdfIndirectReference ref1 = cstp.AddToBody(stream).IndirectReference;
                ar.AddFirst(ref1);
                out_p.Reset();
                if (over != null) {
                    out_p.Append(' ');
                    out_p.Append(PdfContents.RESTORESTATE);
                    out_p.Append(PdfContents.SAVESTATE);
                    ApplyRotation(pageN, out_p);
                    out_p.Append(over.InternalBuffer);
                    out_p.Append(PdfContents.RESTORESTATE);
                    stream = new PdfStream(out_p.ToByteArray());
                    stream.FlateCompress(cstp.CompressionLevel);
                    ar.Add(cstp.AddToBody(stream).IndirectReference);
                }
                pageN.Put(PdfName.RESOURCES, pageResources.Resources);
            }

            void ApplyRotation(PdfDictionary pageN, ByteBuffer out_p) {
                if (!cstp.rotateContents)
                    return;
                Rectangle page = reader.GetPageSizeWithRotation(pageN);
                int rotation = page.Rotation;
                switch (rotation) {
                    case 90:
                        out_p.Append(PdfContents.ROTATE90);
                        out_p.Append(page.Top);
                        out_p.Append(' ').Append('0').Append(PdfContents.ROTATEFINAL);
                        break;
                    case 180:
                        out_p.Append(PdfContents.ROTATE180);
                        out_p.Append(page.Right);
                        out_p.Append(' ');
                        out_p.Append(page.Top);
                        out_p.Append(PdfContents.ROTATEFINAL);
                        break;
                    case 270:
                        out_p.Append(PdfContents.ROTATE270);
                        out_p.Append('0').Append(' ');
                        out_p.Append(page.Right);
                        out_p.Append(PdfContents.ROTATEFINAL);
                        break;
                }
            }

            private void AddDocumentField(PdfIndirectReference refi) {
                if (cstp.fieldArray == null)
                    cstp.fieldArray = new PdfArray();
                cstp.fieldArray.Add(refi);
            }

            private void ExpandFields(PdfFormField field, List<PdfAnnotation> allAnnots) {
                allAnnots.Add(field);
                List<PdfFormField> kids = field.Kids;
                if (kids != null) {
                    foreach (PdfFormField f in kids)
                        ExpandFields(f, allAnnots);
                }
            }

            public void AddAnnotation(PdfAnnotation annot) {
                List<PdfAnnotation> allAnnots = new List<PdfAnnotation>();
                if (annot.IsForm()) {
                    PdfFormField field = (PdfFormField)annot;
                    if (field.Parent != null)
                        return;
                    ExpandFields(field, allAnnots);
                    if (cstp.fieldTemplates == null)
                        cstp.fieldTemplates = new Dictionary<PdfTemplate,object>();
                }
                else
                    allAnnots.Add(annot);
                for (int k = 0; k < allAnnots.Count; ++k) {
                    annot = allAnnots[k];
                    if (annot.IsForm()) {
                        if (!annot.IsUsed()) {
                            Dictionary<PdfTemplate,object> templates = annot.Templates;
                            if (templates != null) {
                                foreach (PdfTemplate tpl in templates.Keys) {
                                    cstp.fieldTemplates[tpl] = null;
                                }
                            }
                        }
                        PdfFormField field = (PdfFormField)annot;
                        if (field.Parent == null)
                            AddDocumentField(field.IndirectReference);
                    }
                    if (annot.IsAnnotation()) {
                        PdfObject pdfobj = PdfReader.GetPdfObject(pageN.Get(PdfName.ANNOTS), pageN);
                        PdfArray annots = null;
                        if (pdfobj == null || !pdfobj.IsArray()) {
                            annots = new PdfArray();
                            pageN.Put(PdfName.ANNOTS, annots);
                        }
                        else 
                            annots = (PdfArray)pdfobj;
                        annots.Add(annot.IndirectReference);
                        if (!annot.IsUsed()) {
                            PdfRectangle rect = (PdfRectangle)annot.Get(PdfName.RECT);
                            if (rect != null && (rect.Left != 0 || rect.Right != 0 || rect.Top != 0 || rect.Bottom != 0)) {
                                int rotation = reader.GetPageRotation(pageN);
                                Rectangle pageSize = reader.GetPageSizeWithRotation(pageN);
                                switch (rotation) {
                                    case 90:
                                        annot.Put(PdfName.RECT, new PdfRectangle(
                                            pageSize.Top - rect.Bottom,
                                            rect.Left,
                                            pageSize.Top - rect.Top,
                                            rect.Right));
                                        break;
                                    case 180:
                                        annot.Put(PdfName.RECT, new PdfRectangle(
                                            pageSize.Right - rect.Left,
                                            pageSize.Top - rect.Bottom,
                                            pageSize.Right - rect.Right,
                                            pageSize.Top - rect.Top));
                                        break;
                                    case 270:
                                        annot.Put(PdfName.RECT, new PdfRectangle(
                                            rect.Bottom,
                                            pageSize.Right - rect.Left,
                                            rect.Top,
                                            pageSize.Right - rect.Right));
                                        break;
                                }
                            }
                        }
                    }
                    if (!annot.IsUsed()) {
                        annot.SetUsed();
                        cstp.AddToBody(annot, annot.IndirectReference);
                    }
                }
            }
        }

        public class StampContent : PdfContentByte {
            PageResources pageResources;
            
            /** Creates a new instance of StampContent */
            internal StampContent(PdfWriter writer, PageResources pageResources) : base(writer) {
                this.pageResources = pageResources;
            }

            /**
            * Gets a duplicate of this <CODE>PdfContentByte</CODE>. All
            * the members are copied by reference but the buffer stays different.
            *
            * @return a copy of this <CODE>PdfContentByte</CODE>
            */
            public override PdfContentByte Duplicate {
                get {
                    return new PdfCopy.StampContent(writer, pageResources);
                }
            }

            internal override PageResources PageResources {
                get {
                    return pageResources;
                }
            }
        }
    }
}
