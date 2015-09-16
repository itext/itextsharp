using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text.pdf.interfaces;
using Org.BouncyCastle.X509;
/*
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

namespace iTextSharp.text.pdf {
    /**
    * Concatenates PDF documents including form fields. The rules for the form field
    * concatenation are the same as in Acrobat. All the documents are kept in memory unlike
    * PdfCopy.
    * @author  Paulo Soares
    */
    [Obsolete]
    public class PdfCopyFields : IPdfViewerPreferences, IPdfEncryptionSettings {
        
        private PdfCopyFieldsImp fc;
        
        /**
        * Creates a new instance.
        * @param os the output stream
        * @throws DocumentException on error
        * @throws IOException on error
        */    
        public PdfCopyFields(Stream os) {
            fc = new PdfCopyFieldsImp(os);
        }
        
        /**
        * Creates a new instance.
        * @param os the output stream
        * @param pdfVersion the pdf version the output will have
        * @throws DocumentException on error
        * @throws IOException on error
        */    
        public PdfCopyFields(Stream os, char pdfVersion) {
            fc = new PdfCopyFieldsImp(os, pdfVersion);
        }
        
        /**
        * Concatenates a PDF document.
        * @param reader the PDF document
        * @throws DocumentException on error
        */    
        virtual public void AddDocument(PdfReader reader) {
            fc.AddDocument(reader);
        }
        
        /**
        * Concatenates a PDF document selecting the pages to keep. The pages are described as a
        * <CODE>List</CODE> of <CODE>Integer</CODE>. The page ordering can be changed but
        * no page repetitions are allowed.
        * @param reader the PDF document
        * @param pagesToKeep the pages to keep
        * @throws DocumentException on error
        */    
        virtual public void AddDocument(PdfReader reader, IList<int> pagesToKeep) {
            fc.AddDocument(reader, pagesToKeep);
        }

        /**
        * Concatenates a PDF document selecting the pages to keep. The pages are described as
        * ranges. The page ordering can be changed but
        * no page repetitions are allowed.
        * @param reader the PDF document
        * @param ranges the comma separated ranges as described in {@link SequenceList}
        * @throws DocumentException on error
        */    
        virtual public void AddDocument(PdfReader reader, String ranges) {
            fc.AddDocument(reader, SequenceList.Expand(ranges, reader.NumberOfPages));
        }

        /** Sets the encryption options for this document. The userPassword and the
        *  ownerPassword can be null or have zero length. In this case the ownerPassword
        *  is replaced by a random string. The open permissions for the document can be
        *  AllowPrinting, AllowModifyContents, AllowCopy, AllowModifyAnnotations,
        *  AllowFillIn, AllowScreenReaders, AllowAssembly and AllowDegradedPrinting.
        *  The permissions can be combined by ORing them.
        * @param userPassword the user password. Can be null or empty
        * @param ownerPassword the owner password. Can be null or empty
        * @param permissions the user permissions
        * @param strength128Bits <code>true</code> for 128 bit key length, <code>false</code> for 40 bit key length
        * @throws DocumentException if the document is already open
        */
        virtual public void SetEncryption(byte[] userPassword, byte[] ownerPassword, int permissions, bool strength128Bits) {
            fc.SetEncryption(userPassword, ownerPassword, permissions, strength128Bits ? PdfWriter.STANDARD_ENCRYPTION_128 : PdfWriter.STANDARD_ENCRYPTION_40);
        }
        
        /**
        * Sets the encryption options for this document. The userPassword and the
        *  ownerPassword can be null or have zero length. In this case the ownerPassword
        *  is replaced by a random string. The open permissions for the document can be
        *  AllowPrinting, AllowModifyContents, AllowCopy, AllowModifyAnnotations,
        *  AllowFillIn, AllowScreenReaders, AllowAssembly and AllowDegradedPrinting.
        *  The permissions can be combined by ORing them.
        * @param strength true for 128 bit key length. false for 40 bit key length
        * @param userPassword the user password. Can be null or empty
        * @param ownerPassword the owner password. Can be null or empty
        * @param permissions the user permissions
        * @throws DocumentException if the document is already open
        */
        virtual public void SetEncryption(bool strength, String userPassword, String ownerPassword, int permissions) {
            SetEncryption(DocWriter.GetISOBytes(userPassword), DocWriter.GetISOBytes(ownerPassword), permissions, strength);
        }
     
        /**
        * Closes the output document.
        */    
        virtual public void Close() {
            fc.Close();
        }

        /**
        * Opens the document. This is usually not needed as AddDocument() will do it
        * automatically.
        */    
        virtual public void Open() {
            fc.OpenDoc();
        }

        /**
        * Adds JavaScript to the global document
        * @param js the JavaScript
        */    
        virtual public void AddJavaScript(String js) {
            fc.AddJavaScript(js, !PdfEncodings.IsPdfDocEncoding(js));
        }

        /**
        * Sets the bookmarks. The list structure is defined in
        * {@link SimpleBookmark}.
        * @param outlines the bookmarks or <CODE>null</CODE> to remove any
        */    
        virtual public IList<Dictionary<string,object>> Outlines {
            set {
                fc.Outlines = value;
            }
        }
        
        /** Gets the underlying PdfWriter.
        * @return the underlying PdfWriter
        */    
        virtual public PdfWriter Writer {
            get {
                return fc;
            }
        }

        /**
        * Gets the 1.5 compression status.
        * @return <code>true</code> if the 1.5 compression is on
        */
        virtual public bool FullCompression {
            get {
                return fc.FullCompression;
            }
        }
        
        /**
        * Sets the document's compression to the new 1.5 mode with object streams and xref
        * streams. It can be set at any time but once set it can't be unset.
        */
        virtual public void SetFullCompression() {
            fc.SetFullCompression();
        }
    
        /**
        * @see com.lowagie.text.pdf.interfaces.PdfEncryptionSettings#setEncryption(byte[], byte[], int, int)
        */
        virtual public void SetEncryption(byte[] userPassword, byte[] ownerPassword, int permissions, int encryptionType) {
            fc.SetEncryption(userPassword, ownerPassword, permissions, encryptionType);
        }

        /**
        * @see com.lowagie.text.pdf.interfaces.PdfViewerPreferences#addViewerPreference(com.lowagie.text.pdf.PdfName, com.lowagie.text.pdf.PdfObject)
        */
        virtual public void AddViewerPreference(PdfName key, PdfObject value) {
            fc.AddViewerPreference(key, value); 
        }

        /**
        * @see com.lowagie.text.pdf.interfaces.PdfViewerPreferences#setViewerPreferences(int)
        */
        virtual public int ViewerPreferences {
            set {
                fc.ViewerPreferences = value;
            }
        }

        /**
        * @see com.lowagie.text.pdf.interfaces.PdfEncryptionSettings#setEncryption(java.security.cert.Certificate[], int[], int)
        */
        virtual public void SetEncryption(X509Certificate[] certs, int[] permissions, int encryptionType) {
            fc.SetEncryption(certs, permissions, encryptionType);
        }    
    }
}
