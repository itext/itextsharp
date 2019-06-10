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
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.intern;
using iTextSharp.text.xml.xmp;
using iTextSharp.xmp;

namespace itextsharp.pdfa.iTextSharp.text.pdf.intern {
    public class ZugferdChecker : PdfA3Checker {

        private IList<PdfFileSpecification> attachments = new List<PdfFileSpecification>();

        protected internal ZugferdChecker(PdfAConformanceLevel conformanceLevel) : base(conformanceLevel) {
        }

        protected override void CheckFileSpec(PdfWriter writer, int key, object obj1) {
            base.CheckFileSpec(writer, key, obj1);
            attachments.Add((PdfFileSpecification)obj1);
        }

        public override void Close(PdfWriter writer) {
            base.Close(writer);
            bool ok = false;

            IXmpMeta xmpMeta = null;
            if (writer.XmpWriter == null) {
               if (writer is PdfAStamperImp) {
                xmpMeta = ((PdfAStamperImp) writer).GetXmpMeta();
                PdfReader pdfReader = ((PdfAStamperImp) writer).GetPdfReader();
                PdfArray pdfArray = pdfReader.Catalog.GetAsArray(PdfName.AF);
                    if (pdfArray != null) {
                        for (int i = 0; i < pdfArray.Size; i++) {
                            PdfFileSpecification pdfFileSpecification = new PdfFileSpecification();
                            pdfFileSpecification.PutAll((PdfDictionary) pdfArray.GetDirectObject(i));
                            attachments.Add(pdfFileSpecification);
                        }
                    }
                }
            } else {
                xmpMeta = writer.XmpWriter.XmpMeta;
            }

            if (xmpMeta == null) {
                writer.CreateXmpMetadata();
                xmpMeta = writer.XmpWriter.XmpMeta;
            }

            try {
                String docFileName = xmpMeta.GetPropertyString(PdfAXmpWriter.zugferdSchemaNS,
                    PdfAXmpWriter.zugferdDocumentFileName);
                foreach (PdfFileSpecification attachment in attachments) {
                    if ((attachment.GetAsString(PdfName.UF) != null && docFileName.Equals(attachment.GetAsString(PdfName.UF).ToString()))
                            || (attachment.GetAsString(PdfName.F) != null && docFileName.Equals(attachment.GetAsString(PdfName.F).ToString())))
                    {

                        PdfName relationship = attachment.GetAsName(PdfName.AFRELATIONSHIP);
                        if (!AFRelationshipValue.Alternative.Equals(relationship)) {
                            attachments.Clear();
                            throw new PdfAConformanceException(attachment,
                                MessageLocalization.GetComposedMessage("afrelationship.value.shall.be.alternative"));
                        }
                        ok = true;
                        break;
                    }
                }
            } catch (Exception e) {
                attachments.Clear();
                throw e;
            }
            attachments.Clear();
            if (!ok) {
                throw new PdfAConformanceException(xmpMeta,
                    MessageLocalization.GetComposedMessage("zugferd.xmp.schema.shall.contain.attachment.name"));
            }
        }
    }
}
