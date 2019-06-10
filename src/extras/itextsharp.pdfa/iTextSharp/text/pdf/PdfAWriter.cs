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

namespace iTextSharp.text.pdf {
    /**
     * @see PdfWriter
     */
    public class PdfAWriter : PdfWriter {
        public static String MimeTypePdf = "application/pdf";
        public static String MimeTypeOctetStream = "application/octet-stream";

        /**
         * Use this method to get an instance of the <CODE>PdfWriter</CODE>.
         * @param	document	The <CODE>Document</CODE> that has to be written
         * @param	os	The <CODE>Stream</CODE> the writer has to write to.
         * @param conformanceLevel PDF/A conformance level of a new PDF document
         * @return	a new <CODE>PdfWriter</CODE>
         * @throws	DocumentException on error
         */
        public static PdfAWriter GetInstance(Document document, Stream os, PdfAConformanceLevel conformanceLevel) {
            PdfDocument pdf = new PdfDocument();
            document.AddDocListener(pdf);
            PdfAWriter writer = new PdfAWriter(pdf, os, conformanceLevel);
            pdf.AddWriter(writer);
            return writer;
        }

        /**
         * Use this method to get an instance of the <CODE>PdfWriter</CODE>.
         * @param	document	The <CODE>Document</CODE> that has to be written
         * @param	os	The <CODE>Stream</CODE> the writer has to write to.
         * @param listener A <CODE>DocListener</CODE> to pass to the PdfDocument.
         * @param conformanceLevel PDF/A conformance level of a new PDF document
         * @return	a new <CODE>PdfWriter</CODE>
         * @throws	DocumentException on error
         */

        public static PdfAWriter GetInstance(Document document, Stream os, IDocListener listener,
                                             PdfAConformanceLevel conformanceLevel) {
            PdfDocument pdf = new PdfDocument();
            pdf.AddDocListener(listener);
            document.AddDocListener(pdf);
            PdfAWriter writer = new PdfAWriter(pdf, os, conformanceLevel);
            pdf.AddWriter(writer);
            return writer;
        }

        /**
         *
         * @param writer
         * @param conformanceLevel
         */

        public static void SetPdfVersion(PdfWriter writer, PdfAConformanceLevel conformanceLevel) {
            switch (conformanceLevel) {
                case PdfAConformanceLevel.PDF_A_1A:
                case PdfAConformanceLevel.PDF_A_1B:
                    writer.PdfVersion = PdfWriter.VERSION_1_4;
                    break;
                case PdfAConformanceLevel.PDF_A_2A:
                case PdfAConformanceLevel.PDF_A_2B:
                case PdfAConformanceLevel.PDF_A_2U:
                    writer.PdfVersion = PdfWriter.VERSION_1_7;
                    break;
                case PdfAConformanceLevel.PDF_A_3A:
                case PdfAConformanceLevel.PDF_A_3B:
                case PdfAConformanceLevel.PDF_A_3U:
                    writer.PdfVersion = PdfWriter.VERSION_1_7;
                    break;
                default:
                    writer.PdfVersion = PdfWriter.VERSION_1_4;
                    break;
            }
        }

        /**
         * @see PdfWriter#setOutputIntents(String, String, String, String, ICC_Profile)
         */

        public override void SetOutputIntents(String outputConditionIdentifier, String outputCondition,
                                              String registryName, String info, ICC_Profile colorProfile) {
            base.SetOutputIntents(outputConditionIdentifier, outputCondition, registryName, info, colorProfile);
            PdfArray a = extraCatalog.GetAsArray(PdfName.OUTPUTINTENTS);
            if (a != null) {
                PdfDictionary d = a.GetAsDict(0);
                if (d != null)
                    d.Put(PdfName.S, PdfName.GTS_PDFA1);
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

        /**
         * Always throws an exception since PDF/X conformance level cannot be set for PDF/A conformant documents.
         * @param pdfx
         */

        virtual public void SetPDFXConformance(int pdfx) {
            throw new PdfXConformanceException(
                MessageLocalization.GetComposedMessage("pdfx.conformance.cannot.be.set.for.PdfAWriter.instance"));
        }

        /**
         * @param conformanceLevel PDF/A conformance level of a new PDF document
         */
        protected internal PdfAWriter(PdfAConformanceLevel conformanceLevel)
            : base() {
            ((IPdfAConformance) pdfIsoConformance).SetConformanceLevel(conformanceLevel);
            SetPdfVersion(this, conformanceLevel);
        }

        /**
         * Constructs a <CODE>PdfAWriter</CODE>.
         * <P>
         * Remark: a PdfAWriter can only be constructed by calling the method <CODE>getInstance(Document document, Stream os, PdfAconformanceLevel conformanceLevel)</CODE>.
         * @param document the <CODE>PdfDocument</CODE> that has to be written
         * @param os the <CODE>Stream</CODE> the writer has to write to
         * @param conformanceLevel PDF/A conformance level of a new PDF document
         */
        protected internal PdfAWriter(PdfDocument document, Stream os, PdfAConformanceLevel conformanceLevel)
            : base(document, os) {
            ((IPdfAConformance) pdfIsoConformance).SetConformanceLevel(conformanceLevel);
            SetPdfVersion(this, conformanceLevel);
        }

        /**
         * @see com.itextpdf.text.pdf.PdfWriter#getTtfUnicodeWriter()
         */

        protected internal override TtfUnicodeWriter GetTtfUnicodeWriter() {
            if (ttfUnicodeWriter == null)
                ttfUnicodeWriter = new PdfATtfUnicodeWriter(this, ((IPdfAConformance)pdfIsoConformance).ConformanceLevel);
            return ttfUnicodeWriter;
        }

        override protected internal XmpWriter CreateXmpWriter(MemoryStream baos, PdfDictionary info) {
            return
                xmpWriter = new PdfAXmpWriter(baos, info, ((IPdfAConformance) pdfIsoConformance).ConformanceLevel, this);
        }

        override protected internal XmpWriter CreateXmpWriter(MemoryStream baos, IDictionary<String, String> info) {
            return
                xmpWriter = new PdfAXmpWriter(baos, info, ((IPdfAConformance) pdfIsoConformance).ConformanceLevel, this);
        }

        public override IPdfIsoConformance InitPdfIsoConformance() {
            return new PdfAConformanceImp(this);
        }

        protected ICounter COUNTER = CounterFactory.GetCounter(typeof (PdfAWriter));

        protected override ICounter GetCounter() {
            return COUNTER;
        }

        protected internal override void CacheObject(PdfIndirectObject iobj) {
            GetPdfAChecker().CacheObject(iobj.IndirectReference, iobj.objecti);
        }

        private PdfAChecker GetPdfAChecker() {
            return ((PdfAConformanceImp) pdfIsoConformance).PdfAChecker;
        }

        /**
         * Use this method to add a file attachment at the document level.
         * @param description the file description
         * @param fileStore an array with the file. If it's <CODE>null</CODE>
         * the file will be read from the disk
         * @param file the path to the file. It will only be used if
         * <CODE>fileStore</CODE> is not <CODE>null</CODE>
         * @param fileDisplay the actual file name stored in the pdf
         * @param mimeType mime type of the file
         * @param afRelationshipValue AFRelationship key value, @see AFRelationshipValue. If <CODE>null</CODE>, @see AFRelationshipValue.Unspecified will be added.
         * @param fileParameter the optional extra file parameters such as the creation or modification date 
         * @return the file specification
         * @throws IOException on error
         */
        virtual public PdfFileSpecification AddFileAttachment(String description, byte[] fileStore, String file, String fileDisplay,
            String mimeType, PdfName afRelationshipValue, PdfDictionary fileParameter) {
            PdfFileSpecification pdfFileSpecification = PdfFileSpecification.FileEmbedded(this, file, fileDisplay,
                fileStore, mimeType, fileParameter, PdfStream.BEST_COMPRESSION);

            if (afRelationshipValue != null)
                pdfFileSpecification.Put(PdfName.AFRELATIONSHIP, afRelationshipValue);
            else
                pdfFileSpecification.Put(PdfName.AFRELATIONSHIP, AFRelationshipValue.Unspecified);

            AddFileAttachment(description, pdfFileSpecification);
            return pdfFileSpecification;
        }

        /**
         * Use this method to add a file attachment at the document level.
         * @param description the file description
         * @param fileStore an array with the file. If it's <CODE>null</CODE>
         * the file will be read from the disk
         * @param file the path to the file. It will only be used if
         * <CODE>fileStore</CODE> is not <CODE>null</CODE>
         * @param fileDisplay the actual file name stored in the pdf
         * @param mimeType mime type of the file
         * @param afRelationshipValue AFRelationship key value, @see AFRelationshipValue. If <CODE>null</CODE>, @see AFRelationshipValue.Unspecified will be added.
         * @return the file specification
         * @throws IOException on error
         */
        public virtual PdfFileSpecification AddFileAttachment(String description, byte[] fileStore, String file,
            String fileDisplay, String mimeType, PdfName afRelationshipValue) {
            return AddFileAttachment(description, fileStore, file, fileDisplay, mimeType, afRelationshipValue, null);
        }

        /**
         * Use this method to add a file attachment at the document level. Adds @see MimeTypeOctetStream as mime type.
         * @param description the file description
         * @param fileStore an array with the file. If it's <CODE>null</CODE>
         * the file will be read from the disk
         * @param file the path to the file. It will only be used if
         * <CODE>fileStore</CODE> is not <CODE>null</CODE>
         * @param fileDisplay the actual file name stored in the pdf
         * @param afRelationshipValue AFRelationship key value, @see AFRelationshipValue. If <CODE>null</CODE>, @see AFRelationshipValue.Unspecified will be added.
         *
         * @throws IOException on error
         */
        virtual public void AddFileAttachment(String description, byte[] fileStore, String file, String fileDisplay, PdfName afRelationshipValue) {
            AddFileAttachment(description, fileStore, file, fileDisplay, MimeTypeOctetStream, afRelationshipValue);
        }

        /**
         * Use this method to add a file attachment at the document level. Adds @see MimeTypeOctetStream as mime type and @see AFRelationshipValue.Unspecified as AFRelationship.
         * @param description the file description
         * @param fileStore an array with the file. If it's <CODE>null</CODE>
         * the file will be read from the disk
         * @param file the path to the file. It will only be used if
         * <CODE>fileStore</CODE> is not <CODE>null</CODE>
         * @param fileDisplay the actual file name stored in the pdf
         * @throws IOException on error
         */
        public override void AddFileAttachment(String description, byte[] fileStore, String file, String fileDisplay) {
            AddFileAttachment(description, fileStore, file, fileDisplay, AFRelationshipValue.Unspecified);
        }

        /**
         * Use this method to add a file attachment at the document level.  Adds @see MimeTypePdf as mime type and @see AFRelationshipValue.Unspecified as AFRelationship.
         * @param description the file description
         * @param fileStore an array with the file. If it's <CODE>null</CODE>
         * the file will be read from the disk
         * @param file the path to the file. It will only be used if
         * <CODE>fileStore</CODE> is not <CODE>null</CODE>
         * @param fileDisplay the actual file name stored in the pdf
         * @throws IOException on error
         */
        virtual public void AddPdfAttachment(String description, byte[] fileStore, String file, String fileDisplay) {
            AddPdfAttachment(description, fileStore, file, fileDisplay, AFRelationshipValue.Unspecified);
        }

        /**
         * Use this method to add a file attachment at the document level. Adds @see MimeTypePdf as mime type.
         * @param description the file description
         * @param fileStore an array with the file. If it's <CODE>null</CODE>
         * the file will be read from the disk
         * @param file the path to the file. It will only be used if
         * <CODE>fileStore</CODE> is not <CODE>null</CODE>
         * @param fileDisplay the actual file name stored in the pdf
         * @param afRelationshipValue AFRelationship key value, <see>AFRelationshipValue</see>. If <CODE>null</CODE>, @see AFRelationshipValue.Unspecified will be added.
         *
         * @throws IOException on error
         */
        virtual public void AddPdfAttachment(String description, byte[] fileStore, String file, String fileDisplay, PdfName afRelationshipValue) {
            AddFileAttachment(description, fileStore, file, fileDisplay, MimeTypePdf, afRelationshipValue);
        }

        public override void Close() {
            base.Close();
            GetPdfAChecker().Close(this);
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
    }

}
