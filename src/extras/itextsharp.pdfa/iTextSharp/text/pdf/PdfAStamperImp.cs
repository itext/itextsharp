using System;
using System.IO;
using iTextSharp.text.error_messages;
using iTextSharp.text.pdf.interfaces;
using iTextSharp.text.xml.xmp;
using iTextSharp.text.pdf.intern;

/*
 * $Id: PdfAStamperImp.java 322 2012-07-23 09:58:41Z bruno $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: Alexander Chingarev, Bruno Lowagie, et al.
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
namespace iTextSharp.text.pdf
{
    /**
     * Extension to PdfStamperImp that will attempt to keep a file
     * in conformance with the PDF/A standard.
     */
    public class PdfAStamperImp : PdfStamperImp {

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
            : base(reader, os, pdfVersion, append)
        {
            ((IPdfAConformance)pdfIsoConformance).SetConformanceLevel(conformanceLevel);
            PdfAWriter.SetPdfVersion(this, conformanceLevel);
        }

        /**
         * @see PdfStamperImp#setOutputIntents(String, String, String, String, ICC_Profile)
         */
        override public void SetOutputIntents(String outputConditionIdentifier, String outputCondition, String registryName, String info, ICC_Profile colorProfile) {
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
         * @see com.itextpdf.text.pdf.PdfStamperImp#isPdfIso()
         */
        override public bool IsPdfIso() {
            return pdfIsoConformance.IsPdfIso();
        }

        /**
         * Always throws an exception since PDF/X conformance level cannot be set for PDF/A conformant documents.
         * @param pdfx
         */
        public void SetPDFXConformance(int pdfx) {
            throw new PdfAConformanceException(MessageLocalization.GetComposedMessage("pdfx.conformance.cannot.be.set.for.PdfAStamperImp.instance"));
        }

        /**
         * @see com.itextpdf.text.pdf.PdfStamperImp#getTtfUnicodeWriter()
         */
        override protected TtfUnicodeWriter GetTtfUnicodeWriter() {
            if (ttfUnicodeWriter == null)
                ttfUnicodeWriter = new PdfATtfUnicodeWriter(this);
            return ttfUnicodeWriter;
        }

        /**
         * @see PdfStamperImp#getXmpWriter(java.io.MemoryStream, com.itextpdf.text.pdf.PdfDocument.PdfInfo)
         */
        override protected XmpWriter GetXmpWriter(MemoryStream baos, PdfDictionary info)
        {
            if (xmpWriter == null)
                xmpWriter = new PdfAXmpWriter(baos, info, ((IPdfAConformance)pdfIsoConformance).GetConformanceLevel());
            return xmpWriter;
        }

        override public IPdfIsoConformance GetPdfIsoConformance() {
            return new PdfAConformanceImp(this);
        }

    }

}
