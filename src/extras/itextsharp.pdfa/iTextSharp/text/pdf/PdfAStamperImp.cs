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
using System.IO;
using iTextSharp.text.error_messages;
using iTextSharp.text.log;
using iTextSharp.text.pdf.interfaces;
using iTextSharp.text.xml.xmp;
using iTextSharp.text.pdf.intern;
using iTextSharp.xmp;
using iTextSharp.xmp.impl;
using iTextSharp.xmp.properties;
namespace iTextSharp.text.pdf {
    /**
     * Extension to PdfStamperImp that will attempt to keep a file
     * in conformance with the PDF/A standard.
     */
    public class PdfAStamperImp : PdfStamperImp {

        protected ICounter COUNTER = CounterFactory.GetCounter(typeof(PdfAStamper));

        protected  IXmpMeta xmpMeta = null;

        protected override ICounter GetCounter() {
            return COUNTER;
        }

        /**
         * Creates new PdfStamperImp.
         * @param reader reads the PDF
         * @param os the output destination
         * @param pdfVersion the new pdf version or '\0' to keep the same version as the original document
         * @param append
         * @param conformanceLevel PDF/A conformance level of a new PDF document
         * @throws DocumentException on error
         * @throws IOException
         */
        internal PdfAStamperImp(PdfReader reader, Stream os, char pdfVersion, bool append, PdfAConformanceLevel conformanceLevel)
            : base(reader, os, pdfVersion, append) {
            ((IPdfAConformance)pdfIsoConformance).SetConformanceLevel(conformanceLevel);
            PdfAWriter.SetPdfVersion(this, conformanceLevel);
            ReadPdfAInfo();
        }

        protected override void ReadColorProfile() {
            PdfObject outputIntents = reader.Catalog.GetAsArray(PdfName.OUTPUTINTENTS);
            if (outputIntents != null && ((PdfArray) outputIntents).Size > 0) {
                PdfStream iccProfileStream = null;
                for (int i = 0; i < ((PdfArray) outputIntents).Size; i++) {
                    PdfDictionary outputIntentDictionary = ((PdfArray) outputIntents).GetAsDict(i);
                    if (outputIntentDictionary != null) {
                        PdfName gts = outputIntentDictionary.GetAsName(PdfName.S);
                        if (iccProfileStream == null || PdfName.GTS_PDFA1.Equals(gts)) {
                            iccProfileStream = outputIntentDictionary.GetAsStream(PdfName.DESTOUTPUTPROFILE);
                            if (iccProfileStream != null && PdfName.GTS_PDFA1.Equals(gts))
                                break;
                        }
                    }
                }
                if (iccProfileStream is PRStream) {
                    colorProfile = ICC_Profile.GetInstance(PdfReader.GetStreamBytes((PRStream) iccProfileStream));
                }
            }
        }

        /**
         * @see PdfStamperImp#setOutputIntents(String, String, String, String, ICC_Profile)
         */
        override public void SetOutputIntents(String outputConditionIdentifier, String outputCondition, String registryName, String info, ICC_Profile colorProfile) {
            base.SetOutputIntents(outputConditionIdentifier, outputCondition, registryName, info, colorProfile);
            PdfArray a = extraCatalog.GetAsArray(PdfName.OUTPUTINTENTS);
            if(a != null) {
                PdfDictionary d = a.GetAsDict(0);
                if(d != null)
                    d.Put(PdfName.S, PdfName.GTS_PDFA1);
            }
        }

        /**
         * Always throws an exception since PDF/X conformance level cannot be set for PDF/A conformant documents.
         * @param pdfx
         */
        virtual public void SetPDFXConformance(int pdfx) {
            throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("pdfx.conformance.cannot.be.set.for.PdfAStamperImp.instance"));
        }

        /**
         * @see com.itextpdf.text.pdf.PdfStamperImp#GetTtfUnicodeWriter()
         */

        internal protected override TtfUnicodeWriter GetTtfUnicodeWriter() {
            if(ttfUnicodeWriter == null)
                ttfUnicodeWriter = new PdfATtfUnicodeWriter(this, ((IPdfAConformance) pdfIsoConformance).ConformanceLevel);
            return ttfUnicodeWriter;
        }

        override protected internal XmpWriter CreateXmpWriter(MemoryStream baos, PdfDictionary info) {
            return new PdfAXmpWriter(baos, info, ((IPdfAConformance) pdfIsoConformance).ConformanceLevel, this);
        }

        override protected internal XmpWriter CreateXmpWriter(MemoryStream baos, IDictionary<String, String> info) {
            return new PdfAXmpWriter(baos, info, ((IPdfAConformance) pdfIsoConformance).ConformanceLevel, this);
        }

        override public IPdfIsoConformance InitPdfIsoConformance() {
            return new PdfAConformanceImp(this);
        }

        private void ReadPdfAInfo() {
            byte[] metadata = null;
            
            IXmpProperty pdfaidConformance = null;
            IXmpProperty pdfaidPart = null;
            try {
                metadata = reader.Metadata;
                xmpMeta = XmpMetaParser.Parse(metadata, null);
                pdfaidConformance = xmpMeta.GetProperty(XmpConst.NS_PDFA_ID, "pdfaid:conformance");
                pdfaidPart = xmpMeta.GetProperty(XmpConst.NS_PDFA_ID, "pdfaid:part");
            } catch(Exception e) {
                throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("only.pdfa.documents.can.be.opened.in.PdfAStamper"));
            }
            if(pdfaidConformance == null || pdfaidPart == null) {
                throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("only.pdfa.documents.can.be.opened.in.PdfAStamper"));
            }
            switch(((IPdfAConformance)pdfIsoConformance).ConformanceLevel) {
                case PdfAConformanceLevel.PDF_A_1A:
                case PdfAConformanceLevel.PDF_A_1B:
                    if(!"1".Equals(pdfaidPart.Value)) {
                        throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("only.pdfa.1.documents.can.be.opened.in.PdfAStamper", "1"));
                    }
                    break;
                case PdfAConformanceLevel.PDF_A_2A:
                case PdfAConformanceLevel.PDF_A_2B:
                case PdfAConformanceLevel.PDF_A_2U:
                    if(!"2".Equals(pdfaidPart.Value)) {
                        throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("only.pdfa.1.documents.can.be.opened.in.PdfAStamper", "2"));
                    }
                    break;
                case PdfAConformanceLevel.PDF_A_3A:
                case PdfAConformanceLevel.PDF_A_3B:
                case PdfAConformanceLevel.PDF_A_3U:
                    if(!"3".Equals(pdfaidPart.Value)) {
                        throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("only.pdfa.1.documents.can.be.opened.in.PdfAStamper", "3"));
                    }
                    break;
            }
        }

        protected internal override void CacheObject(PdfIndirectObject iobj) {
            PdfAChecker.CacheObject(iobj.IndirectReference, iobj.objecti);
        }

        private PdfAChecker PdfAChecker {
            get { return ((PdfAConformanceImp) pdfIsoConformance).PdfAChecker; }
        }

        protected internal override void Close(IDictionary<string, string> moreInfo) {
            base.Close(moreInfo);
            PdfAChecker.Close(this);
        }

        public override PdfAnnotation CreateAnnotation(Rectangle rect, PdfName subtype) {
            PdfAnnotation a = base.CreateAnnotation(rect, subtype);
            if (!PdfName.POPUP.Equals(subtype))
                a.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            return a;
        }

        public override PdfAnnotation CreateAnnotation(float llx, float lly, float urx, float ury, PdfString title, PdfString content, PdfName subtype) {
            PdfAnnotation a = base.CreateAnnotation(llx, lly, urx, ury, title, content, subtype);
            if (!PdfName.POPUP.Equals(subtype))
                a.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            return a;
        }

        public override PdfAnnotation CreateAnnotation(float llx, float lly, float urx, float ury, PdfAction action, PdfName subtype) {
            PdfAnnotation a = base.CreateAnnotation(llx, lly, urx, ury, action, subtype);
            if (!PdfName.POPUP.Equals(subtype))
                a.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            return a;
        }

        public IXmpMeta GetXmpMeta() {
            return xmpMeta;
        }
    }
}
