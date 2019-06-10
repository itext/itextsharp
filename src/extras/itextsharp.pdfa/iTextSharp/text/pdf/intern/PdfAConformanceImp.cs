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
using itextsharp.pdfa.iTextSharp.text.pdf.intern;
using iTextSharp.text.pdf.interfaces;

namespace iTextSharp.text.pdf.intern{
    /**
     * Implementation of the PdfAConformance interface,
     * including the level of conformance.
     * @see PdfAConformance
     */
    public class PdfAConformanceImp : IPdfAConformance {

        /**
         * The PDF conformance level, e.g. PDF/A1 Level A, PDF/A3 Level U,...
         */
        protected PdfAConformanceLevel conformanceLevel;
        protected PdfAChecker pdfAChecker;
        protected PdfWriter writer;

        public PdfAConformanceImp(PdfWriter writer) {
            this.writer = writer;
        }

        virtual public void CheckPdfIsoConformance(int key, Object obj1) {
            pdfAChecker.CheckPdfAConformance(writer, key, obj1);
        }



        /**
         * @see com.itextpdf.text.pdf.interfaces.PdfAConformance#getConformanceLevel()
         */
        virtual public PdfAConformanceLevel ConformanceLevel {
            get { return conformanceLevel; }
        }

        /**
         * @see PdfAConformance#setConformanceLevel(com.itextpdf.text.pdf.PdfAConformanceLevel)
         */
        virtual public void SetConformanceLevel(PdfAConformanceLevel conformanceLevel) {
            this.conformanceLevel = conformanceLevel;
            switch(this.conformanceLevel) {
                case PdfAConformanceLevel.PDF_A_1A:
                case PdfAConformanceLevel.PDF_A_1B:
                    pdfAChecker = new PdfA1Checker(conformanceLevel);
                    break;
                case PdfAConformanceLevel.PDF_A_2A:
                case PdfAConformanceLevel.PDF_A_2B:
                case PdfAConformanceLevel.PDF_A_2U:
                    pdfAChecker = new PdfA2Checker(conformanceLevel);
                    break;
                case PdfAConformanceLevel.PDF_A_3A:
                case PdfAConformanceLevel.PDF_A_3B:
                case PdfAConformanceLevel.PDF_A_3U:
                    pdfAChecker = new PdfA3Checker(conformanceLevel);
                    break;
                case PdfAConformanceLevel.ZUGFeRD:
                case PdfAConformanceLevel.ZUGFeRDComfort:
                case PdfAConformanceLevel.ZUGFeRDBasic:
                case PdfAConformanceLevel.ZUGFeRDExtended:
                    pdfAChecker = new ZugferdChecker(conformanceLevel);
                    break;
                default:
                    pdfAChecker = new PdfA1Checker(conformanceLevel);
                    break;
            }
        }

        /**
         * @see com.itextpdf.text.pdf.interfaces.PdfAConformance#isPdfIso()
         */
        virtual public bool IsPdfIso() {
            return true;
        }

        virtual public PdfAChecker PdfAChecker {
            get { return pdfAChecker; }
        }
    }
}
