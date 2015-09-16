using System;
using iTextSharp.text.pdf;
using iTextSharp.text;
using iTextSharp.text.pdf.interfaces;
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

    /**
    * Stores the PDF version information,
    * knows how to write a PDF Header,
    * and how to add the version to the catalog (if necessary).
    */

    public class PdfVersionImp : IPdfVersion {

        /** Contains different strings that are part of the header. */
        public static readonly byte[][] HEADER = {
            DocWriter.GetISOBytes("\n"),
            DocWriter.GetISOBytes("%PDF-"),
            DocWriter.GetISOBytes("\n%\u00e2\u00e3\u00cf\u00d3\n")
        };
        /** Indicates if the header was already written. */
        protected bool headerWasWritten = false;
        /** Indicates if we are working in append mode. */
        protected bool appendmode = false;
        /** The version that was or will be written to the header. */
        protected char header_version = PdfWriter.VERSION_1_4;
        /** The version that will be written to the catalog. */
        protected PdfName catalog_version = null;
        /** The version that user can use to get the actual version of PDF document **/
        protected char version = PdfWriter.VERSION_1_4;

        /**
         * The extensions dictionary.
         * @since	2.1.6
         */
        protected PdfDictionary extensions = null;
        
        /**
        * @see com.lowagie.text.pdf.interfaces.PdfVersion#setPdfVersion(char)
        */
        virtual public char PdfVersion {
            set {
          		this.version = value;
                if (headerWasWritten || appendmode) {
                    SetPdfVersion(GetVersionAsName(value));
                }
                else {
                    this.header_version = value;
                }
            }
        }
        
        /**
        * @see com.lowagie.text.pdf.interfaces.PdfVersion#setAtLeastPdfVersion(char)
        */
        virtual public void SetAtLeastPdfVersion(char version) {
            if (version > header_version) {
                PdfVersion = version;
            }
        }
        
        /**
        * @see com.lowagie.text.pdf.interfaces.PdfVersion#setPdfVersion(com.lowagie.text.pdf.PdfName)
        */
        virtual public void SetPdfVersion(PdfName version) {
            if (catalog_version == null || catalog_version.CompareTo(version) < 0) {
                this.catalog_version = version;
            }
        }
        
        /**
        * Sets the append mode.
        */
        virtual public void SetAppendmode(bool appendmode) {
            this.appendmode = appendmode;
        }
        
        /**
        * Writes the header to the OutputStreamCounter.
        * @throws IOException 
        */
        virtual public void WriteHeader(OutputStreamCounter os) {
            if (appendmode) {
                os.Write(HEADER[0], 0, HEADER[0].Length);
            }
            else {
                os.Write(HEADER[1], 0, HEADER[1].Length);
                os.Write(GetVersionAsByteArray(header_version), 0, GetVersionAsByteArray(header_version).Length);
                os.Write(HEADER[2], 0, HEADER[2].Length);
                headerWasWritten = true;
            }
        }
        
        /**
        * Returns the PDF version as a name.
        * @param version    the version character.
        */
        virtual public PdfName GetVersionAsName(char version) {
            switch (version) {
            case PdfWriter.VERSION_1_2:
                return PdfWriter.PDF_VERSION_1_2;
            case PdfWriter.VERSION_1_3:
                return PdfWriter.PDF_VERSION_1_3;
            case PdfWriter.VERSION_1_4:
                return PdfWriter.PDF_VERSION_1_4;
            case PdfWriter.VERSION_1_5:
                return PdfWriter.PDF_VERSION_1_5;
            case PdfWriter.VERSION_1_6:
                return PdfWriter.PDF_VERSION_1_6;
            case PdfWriter.VERSION_1_7:
                return PdfWriter.PDF_VERSION_1_7;
            default:
                return PdfWriter.PDF_VERSION_1_4;
            }
        }
        
        /**
        * Returns the version as a byte[].
        * @param version the version character
        */
        virtual public byte[] GetVersionAsByteArray(char version) {
            return DocWriter.GetISOBytes(GetVersionAsName(version).ToString().Substring(1));
        }

	    /** Adds the version to the Catalog dictionary. */
	    virtual public void AddToCatalog(PdfDictionary catalog) {
		    if(catalog_version != null) {
			    catalog.Put(PdfName.VERSION, catalog_version);
		    }
            if (extensions != null) {
                catalog.Put(PdfName.EXTENSIONS, extensions);
            }
        }

        /**
        * @see com.lowagie.text.pdf.interfaces.PdfVersion#addDeveloperExtension(com.lowagie.text.pdf.PdfDeveloperExtension)
        * @since   2.1.6
        */
        virtual public void AddDeveloperExtension(PdfDeveloperExtension de) {
            if (extensions == null) {
                extensions = new PdfDictionary();
            }
            else {
                PdfDictionary extension = extensions.GetAsDict(de.Prefix);
                if (extension != null) {
                    int diff = de.Baseversion.CompareTo(extension.GetAsName(PdfName.BASEVERSION));
                    if (diff < 0)
                        return;
                    diff = de.ExtensionLevel - extension.GetAsNumber(PdfName.EXTENSIONLEVEL).IntValue;
                    if (diff <= 0)
                        return;
                }
            }
            extensions.Put(de.Prefix, de.GetDeveloperExtensions());
        }

        virtual public char Version
        {
            get { return version; }
        }
    }
}
