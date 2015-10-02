using System;
using System.Collections.Generic;
using iTextSharp.text.pdf;
using iTextSharp.text;
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

namespace iTextSharp.text.pdf.intern {

    public class PdfAnnotationsImp {

        /**
        * This is the AcroForm object for the complete document.
        */
        protected internal PdfAcroForm acroForm;

        /**
        * This is the array containing the references to annotations
        * that were added to the document.
        */
        protected internal List<PdfAnnotation> annotations = new List<PdfAnnotation>();
        
        /**
        * This is an array containg references to some delayed annotations
        * (that were added for a page that doesn't exist yet).
        */
        protected internal List<PdfAnnotation> delayedAnnotations = new List<PdfAnnotation>();
        
        
        public PdfAnnotationsImp(PdfWriter writer) {
            acroForm = new PdfAcroForm(writer);
        }
        
        /**
        * Checks if the AcroForm is valid.
        */
        virtual public bool HasValidAcroForm() {
            return acroForm.IsValid();
        }
        
        /**
        * Gets the AcroForm object.
        * @return the PdfAcroform object of the PdfDocument
        */
        virtual public PdfAcroForm AcroForm {
            get {
                return acroForm;
            }
        }
        
        virtual public int SigFlags {
            set {
                acroForm.SigFlags = value;
            }
        }
        
        virtual public void AddCalculationOrder(PdfFormField formField) {
            acroForm.AddCalculationOrder(formField);
        }
        
        virtual public void AddAnnotation(PdfAnnotation annot) {
            if (annot.IsForm()) {
                PdfFormField field = (PdfFormField)annot;
                if (field.Parent == null)
                    AddFormFieldRaw(field);
            }
            else
                annotations.Add(annot);
        }
        
        virtual public void AddPlainAnnotation(PdfAnnotation annot) {
            annotations.Add(annot);
        }
        
        void AddFormFieldRaw(PdfFormField field) {
            annotations.Add(field);
            List<PdfFormField> kids = field.Kids;
            if (kids != null) {
                for (int k = 0; k < kids.Count; ++k) {
                    PdfFormField kid = kids[k];
                    if (!kid.IsUsed()) {
                        AddFormFieldRaw(kid);
                    }
                }
            }
        }
        
        virtual public bool HasUnusedAnnotations() {
            return annotations.Count > 0;
        }

        virtual public void ResetAnnotations() {
            annotations = delayedAnnotations;
            delayedAnnotations = new List<PdfAnnotation>();
        }
        
        virtual public PdfArray RotateAnnotations(PdfWriter writer, Rectangle pageSize) {
            PdfArray array = new PdfArray();
            int rotation = pageSize.Rotation % 360;
            int currentPage = writer.CurrentPageNumber;
            for (int k = 0; k < annotations.Count; ++k) {
                PdfAnnotation dic = annotations[k];
                int page = dic.PlaceInPage;
                if (page > currentPage) {
                    delayedAnnotations.Add(dic);
                    continue;
                }
                if (dic.IsForm()) {
                    if (!dic.IsUsed()) {
                        Dictionary<PdfTemplate,object> templates = dic.Templates;
                        if (templates != null)
                            acroForm.AddFieldTemplates(templates);
                    }
                    PdfFormField field = (PdfFormField)dic;
                    if (field.Parent == null)
                        acroForm.AddDocumentField(field.IndirectReference);
                }
                if (dic.IsAnnotation()) {
                    array.Add(dic.IndirectReference);
                    if (!dic.IsUsed()) {
                        PdfArray tmp = dic.GetAsArray(PdfName.RECT);
                        PdfRectangle rect;
                        if (tmp.Size == 4)
                        {
                            rect = new PdfRectangle(tmp.GetAsNumber(0).FloatValue, tmp.GetAsNumber(1).FloatValue, tmp.GetAsNumber(2).FloatValue, tmp.GetAsNumber(3).FloatValue);
                        } else {
                            rect = new PdfRectangle(tmp.GetAsNumber(0).FloatValue, tmp.GetAsNumber(1).FloatValue);
                        }
                        switch (rotation)
                        {
                                case 90:
                                    dic.Put(PdfName.RECT, new PdfRectangle(
                                    pageSize.Top - rect.Bottom,
                                    rect.Left,
                                    pageSize.Top - rect.Top,
                                    rect.Right));
                                    break;
                                case 180:
                                    dic.Put(PdfName.RECT, new PdfRectangle(
                                    pageSize.Right - rect.Left,
                                    pageSize.Top - rect.Bottom,
                                    pageSize.Right - rect.Right,
                                    pageSize.Top - rect.Top));
                                    break;
                                case 270:
                                    dic.Put(PdfName.RECT, new PdfRectangle(
                                    rect.Bottom,
                                    pageSize.Right - rect.Left,
                                    rect.Top,
                                    pageSize.Right - rect.Right));
                                    break;
                        }
                    }
                }
                if (!dic.IsUsed()) {
                    dic.SetUsed();
                    writer.AddToBody(dic, dic.IndirectReference);
                }
            }
            return array;
        }
        
        public static PdfAnnotation ConvertAnnotation(PdfWriter writer, Annotation annot, Rectangle defaultRect) {
            switch (annot.AnnotationType) {
                case Annotation.URL_NET:
                    return writer.CreateAnnotation(annot.GetLlx(), annot.GetLly(), annot.GetUrx(), annot.GetUry(), new PdfAction((Uri)annot.Attributes[Annotation.URL]), null);
                case Annotation.URL_AS_STRING:
                    return writer.CreateAnnotation(annot.GetLlx(), annot.GetLly(), annot.GetUrx(), annot.GetUry(), new PdfAction((String)annot.Attributes[Annotation.FILE]), null);
                case Annotation.FILE_DEST:
                    return writer.CreateAnnotation(annot.GetLlx(), annot.GetLly(), annot.GetUrx(), annot.GetUry(), new PdfAction((String)annot.Attributes[Annotation.FILE], (String)annot.Attributes[Annotation.DESTINATION]), null);
                case Annotation.SCREEN:
                    bool[] sparams = (bool[])annot.Attributes[Annotation.PARAMETERS];
                    String fname = (String) annot.Attributes[Annotation.FILE];
                    String mimetype = (String) annot.Attributes[Annotation.MIMETYPE];
                    PdfFileSpecification fs;
                    if (sparams[0])
                        fs = PdfFileSpecification.FileEmbedded(writer, fname, fname, null);
                    else
                        fs = PdfFileSpecification.FileExtern(writer, fname);
                    PdfAnnotation ann = PdfAnnotation.CreateScreen(writer, new Rectangle(annot.GetLlx(), annot.GetLly(), annot.GetUrx(), annot.GetUry()),
                            fname, fs, mimetype, sparams[1]);
                    return ann;
                case Annotation.FILE_PAGE:
                    return writer.CreateAnnotation(annot.GetLlx(), annot.GetLly(), annot.GetUrx(), annot.GetUry(), new PdfAction((String)annot.Attributes[Annotation.FILE], ((int?)annot.Attributes[Annotation.PAGE]).Value), null);
                case Annotation.NAMED_DEST:
                    return writer.CreateAnnotation(annot.GetLlx(), annot.GetLly(), annot.GetUrx(), annot.GetUry(), new PdfAction(((int?)annot.Attributes[Annotation.NAMED]).Value), null);
                case Annotation.LAUNCH:
                    return writer.CreateAnnotation(annot.GetLlx(), annot.GetLly(), annot.GetUrx(), annot.GetUry(), new PdfAction((String)annot.Attributes[Annotation.APPLICATION], (String)annot.Attributes[Annotation.PARAMETERS], (String)annot.Attributes[Annotation.OPERATION], (String)annot.Attributes[Annotation.DEFAULTDIR]), null);
                default:
                    return writer.CreateAnnotation(defaultRect.Left, defaultRect.Bottom, defaultRect.Right, defaultRect.Top, new PdfString(annot.Title, PdfObject.TEXT_UNICODE), new PdfString(annot.Content, PdfObject.TEXT_UNICODE), null);
            }
        }
    }
}
