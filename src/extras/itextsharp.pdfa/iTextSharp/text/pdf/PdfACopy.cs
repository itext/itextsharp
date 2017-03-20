/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
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
using iTextSharp.text;
using iTextSharp.text.error_messages;
using iTextSharp.text.log;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.interfaces;
using iTextSharp.text.pdf.intern;
using iTextSharp.text.xml.xmp;
using iTextSharp.xmp;
using iTextSharp.xmp.impl;
using iTextSharp.xmp.properties;

namespace itextsharp.pdfa.iTextSharp.text.pdf {
    /**
     * Extension of PdfCopy that will attempt to keep a file
     * in conformance with the PDF/A standard.
     * @see PdfCopy
     */
    public class PdfACopy : PdfCopy {
         /**
         * Constructor
         *
         * @param document document
         * @param os       outputstream
         */
        public PdfACopy(Document document, Stream os, PdfAConformanceLevel conformanceLevel) : base(document, os) {
            ((IPdfAConformance) pdfIsoConformance).SetConformanceLevel(conformanceLevel);
            PdfAWriter.SetPdfVersion(this, conformanceLevel);
        }

        protected ICounter COUNTER = CounterFactory.GetCounter(typeof (PdfACopy));

        protected override ICounter GetCounter() {
            return COUNTER;
        }

        public override IPdfIsoConformance InitPdfIsoConformance() {
            return new PdfAConformanceImp(this);
        }

        protected internal override void CacheObject(PdfIndirectObject iobj) {
            base.CacheObject(iobj);
            PdfAChecker.CacheObject(iobj.IndirectReference, iobj.objecti);
        }

        private PdfAChecker PdfAChecker {
            get { return ((PdfAConformanceImp) pdfIsoConformance).PdfAChecker; }
        }

        public override void AddDocument(PdfReader reader) {
            CheckPdfAInfo(reader);
            base.AddDocument(reader);
        }

        public override void AddPage(PdfImportedPage iPage) {
            CheckPdfAInfo(iPage.readerInstance.Reader);
            base.AddPage(iPage);
        }

        public override PageStamp CreatePageStamp(PdfImportedPage iPage) {
            CheckPdfAInfo(iPage.readerInstance.Reader);
            return base.CreatePageStamp(iPage);
        }

        public override void SetOutputIntents(String outputConditionIdentifier, String outputCondition, String registryName,
            String info, ICC_Profile colorProfile) {
            base.SetOutputIntents(outputConditionIdentifier, outputCondition, registryName, info, colorProfile);
            PdfArray a = extraCatalog.GetAsArray(PdfName.OUTPUTINTENTS);
            if (a != null) {
                PdfDictionary d = a.GetAsDict(0);
                if (d != null) {
                    d.Put(PdfName.S, PdfName.GTS_PDFA1);
                }
            }
        }

        /**
        * Copies the output intent dictionary from other document to this one.
        * @param reader the other document
        * @param checkExistence <CODE>true</CODE> to just check for the existence of a valid output intent
        * dictionary, <CODE>false</CODE> to insert the dictionary if it exists
        * @throws IOException on error
        * @return <CODE>true</CODE> if the output intent dictionary exists, <CODE>false</CODE>
        * otherwise
        */
        public override bool SetOutputIntents(PdfReader reader, bool checkExistence)
        {
            PdfDictionary catalog = reader.Catalog;
            PdfArray outs = catalog.GetAsArray(PdfName.OUTPUTINTENTS);
            if (outs == null)
                return false;
            if (outs.Size == 0)
                return false;
            PdfDictionary outa = outs.GetAsDict(0);
            PdfObject obj = PdfReader.GetPdfObject(outa.Get(PdfName.S));
            if (obj == null || !PdfName.GTS_PDFA1.Equals(obj))
                return false;
            if (checkExistence)
                return true;
            PRStream stream = (PRStream)PdfReader.GetPdfObject(outa.Get(PdfName.DESTOUTPUTPROFILE));
            byte[] destProfile = null;
            if (stream != null)
            {
                destProfile = PdfReader.GetStreamBytes(stream);
            }
            SetOutputIntents(GetNameString(outa, PdfName.OUTPUTCONDITIONIDENTIFIER), GetNameString(outa, PdfName.OUTPUTCONDITION),
                GetNameString(outa, PdfName.REGISTRYNAME), GetNameString(outa, PdfName.INFO), destProfile);
            return true;
        }

        protected internal override XmpWriter CreateXmpWriter(MemoryStream baos, PdfDictionary info) {
            return new PdfAXmpWriter(baos, info, ((IPdfAConformance) pdfIsoConformance).ConformanceLevel, this);
        }

        protected internal override XmpWriter CreateXmpWriter(MemoryStream baos, IDictionary<String, String> info) {
            return new PdfAXmpWriter(baos, info, ((IPdfAConformance) pdfIsoConformance).ConformanceLevel, this);
        }

        /**
         * @see com.itextpdf.text.pdf.PdfWriter#getTtfUnicodeWriter()
         */
        protected internal override TtfUnicodeWriter GetTtfUnicodeWriter() {
            if (ttfUnicodeWriter == null)
                ttfUnicodeWriter = new PdfATtfUnicodeWriter(this, ((IPdfAConformance) pdfIsoConformance).ConformanceLevel);
            return ttfUnicodeWriter;
        }

        public override void Close() {
            base.Close();
            PdfAChecker.Close(this);
        }

        private void CheckPdfAInfo(PdfReader reader) {
            byte[] metadata;
            IXmpMeta xmpMeta;
            IXmpProperty pdfaidConformance;
            IXmpProperty pdfaidPart;
            try {
                metadata = reader.Metadata;
                xmpMeta = XmpMetaParser.Parse(metadata, null);
                pdfaidConformance = xmpMeta.GetProperty(XmpConst.NS_PDFA_ID, "pdfaid:conformance");
                pdfaidPart = xmpMeta.GetProperty(XmpConst.NS_PDFA_ID, "pdfaid:part");
            } catch (Exception e) {
                throw new PdfAConformanceException(
                    MessageLocalization.GetComposedMessage("only.pdfa.documents.can.be.added.in.PdfACopy"));
            }
            if (pdfaidConformance == null || pdfaidPart == null) {
                throw new PdfAConformanceException(
                    MessageLocalization.GetComposedMessage("only.pdfa.documents.can.be.added.in.PdfACopy"));
            }

            switch (((IPdfAConformance) pdfIsoConformance).ConformanceLevel) {
                case PdfAConformanceLevel.PDF_A_1A:
                case PdfAConformanceLevel.PDF_A_1B:
                    if (!"1".Equals(pdfaidPart.Value)) {
                        throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("different.pdf.a.version", "1"));
                    }
                    break;
                case PdfAConformanceLevel.PDF_A_2A:
                case PdfAConformanceLevel.PDF_A_2B:
                case PdfAConformanceLevel.PDF_A_2U:
                    if (!"2".Equals(pdfaidPart.Value)) {
                        throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("different.pdf.a.version", "2"));
                    }
                    break;
                case PdfAConformanceLevel.PDF_A_3A:
                case PdfAConformanceLevel.PDF_A_3B:
                case PdfAConformanceLevel.PDF_A_3U:
                case PdfAConformanceLevel.ZUGFeRD:
                    if (!"3".Equals(pdfaidPart.Value)) {
                        throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("different.pdf.a.version", "3"));
                    }
                    break;
            }

            switch (((IPdfAConformance) pdfIsoConformance).ConformanceLevel) {
                case PdfAConformanceLevel.PDF_A_1A:
                case PdfAConformanceLevel.PDF_A_2A:
                case PdfAConformanceLevel.PDF_A_3A:
                    if (!"A".Equals(pdfaidConformance.Value)) {
                        throw new PdfAConformanceException(
                            MessageLocalization.GetComposedMessage("incompatible.pdf.a.conformance.level", "a"));
                    }
                    break;
                case PdfAConformanceLevel.PDF_A_2U:
                case PdfAConformanceLevel.PDF_A_3U:
                    if ("B".Equals(pdfaidConformance.Value)) {
                        throw new PdfAConformanceException(
                            MessageLocalization.GetComposedMessage("incompatible.pdf.a.conformance.level", "u"));
                    }
                    break;
            }
        }
    }
}
