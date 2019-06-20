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
using System.util.collections;
using iTextSharp.text.error_messages;

namespace iTextSharp.text.pdf.intern
{
    public class PdfA3Checker : PdfA2Checker {

        private static HashSet2<PdfName> allowedAFRelationships = new HashSet2<PdfName>(new PdfName[] {
            AFRelationshipValue.Source, AFRelationshipValue.Data, AFRelationshipValue.Alternative,
            AFRelationshipValue.Supplement, AFRelationshipValue.Unspecified});

        internal PdfA3Checker(PdfAConformanceLevel conformanceLevel)
            :base(conformanceLevel) {
        }

        protected override HashSet2<PdfName> InitKeysForCheck() {
            HashSet2<PdfName> keysForCheck = base.InitKeysForCheck();
            keysForCheck.Add(PdfName.PARAMS);
            keysForCheck.Add(PdfName.MODDATE);
            keysForCheck.Add(PdfName.F);
            return keysForCheck;
        }

        protected override void CheckFileSpec(PdfWriter writer, int key, Object obj1) {
            if (obj1 is PdfFileSpecification) {
                PdfDictionary fileSpec = (PdfFileSpecification) obj1;
                if (!fileSpec.Contains(PdfName.UF) || !fileSpec.Contains(PdfName.F)
                    || !fileSpec.Contains(PdfName.DESC)) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("file.specification.dictionary.shall.contain.f.uf.and.desc.entries"));
                }

                PdfObject obj = fileSpec.Get(PdfName.AFRELATIONSHIP);

                if (obj == null || !obj.IsName() || !allowedAFRelationships.Contains(obj as PdfName)) {
                    throw new PdfAConformanceException(obj1, MessageLocalization.GetComposedMessage("file.specification.dictionary.shall.contain.correct.afrelationship.key"));
                }

                if (fileSpec.Contains(PdfName.EF)) {
                    PdfDictionary dict = GetDirectDictionary(fileSpec.Get(PdfName.EF));
                    if (dict == null || !dict.Contains(PdfName.F)) {
                        throw new PdfAConformanceException(obj1,
                            MessageLocalization.GetComposedMessage("ef.key.of.file.specification.dictionary.shall.contain.dictionary.with.valid.f.key"));
                    }

                    PdfDictionary embeddedFile = GetDirectDictionary(dict.Get(PdfName.F));
                    if (embeddedFile == null) {
                        throw new PdfAConformanceException(obj1,
                            MessageLocalization.GetComposedMessage("ef.key.of.file.specification.dictionary.shall.contain.dictionary.with.valid.f.key"));
                    }

                    CheckEmbeddedFile(embeddedFile);
                }
            }
        }

        protected override void CheckEmbeddedFile(PdfDictionary embeddedFile) {
            PdfObject _params = GetDirectObject(embeddedFile.Get(PdfName.PARAMS));
            if (_params == null) {
                throw new PdfAConformanceException(embeddedFile,
                    MessageLocalization.GetComposedMessage("embedded.file.shall.contain.valid.params.key"));
            } else if (_params.IsDictionary()) {
                PdfObject modDate = ((PdfDictionary) _params).Get(PdfName.MODDATE);
                if (modDate == null || !(modDate is PdfString)) {
                    throw new PdfAConformanceException(embeddedFile,
                        MessageLocalization.GetComposedMessage("embedded.file.shall.contain.params.key.with.valid.moddate.key"));
                }
            }
        }

        protected override void CheckPdfObject(PdfWriter writer, int key, Object obj1) {
            base.CheckPdfObject(writer, key, obj1);
            if (obj1 is PdfDictionary) {
                PdfDictionary dictionary = (PdfDictionary) obj1;
                PdfName type = dictionary.GetAsName(PdfName.TYPE);
                if (PdfName.EMBEDDEDFILE.Equals(type)) {
                    CheckEmbeddedFile(dictionary);
                }
            }
        }

    }
}
