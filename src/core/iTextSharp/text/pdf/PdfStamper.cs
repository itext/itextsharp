using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text.pdf.interfaces;
using iTextSharp.text.pdf.collection;
using Org.BouncyCastle.X509;
using iTextSharp.text.error_messages;
using iTextSharp.text.pdf.security;
using iTextSharp.text.xml.xmp;

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
    /** Applies extra content to the pages of a PDF document.
    * This extra content can be all the objects allowed in PdfContentByte
    * including pages from other Pdfs. The original PDF will keep
    * all the interactive elements including bookmarks, links and form fields.
    * <p>
    * It is also possible to change the field values and to
    * flatten them. New fields can be added but not flattened.
    * @author Paulo Soares
    */
    public class PdfStamper : IPdfViewerPreferences, IPdfEncryptionSettings, IDisposable {
        /**
        * The writer
        */    
        protected PdfStamperImp stamper;
        private IDictionary<String, String> moreInfo;
        internal protected bool hasSignature;
        protected PdfSignatureAppearance sigApp;
        protected XmlSignatureAppearance sigXmlApp;
        private LtvVerification verification;

        /** Starts the process of adding extra content to an existing PDF
        * document.
        * <p>
        * The reader will be closed when this PdfStamper is closed
        * @param reader the original document. It cannot be reused
        * @param os the output stream
        * @throws DocumentException on error
        * @throws IOException on error
        */
        public PdfStamper(PdfReader reader, Stream os) {
            stamper = new PdfStamperImp(reader, os, '\0', false);
        }

        /**
        * Starts the process of adding extra content to an existing PDF
        * document.
        * <p>
        * The reader will be closed when this PdfStamper is closed
        * @param reader the original document. It cannot be reused
        * @param os the output stream
        * @param pdfVersion the new pdf version or '\0' to keep the same version as the original
        * document
        * @throws DocumentException on error
        * @throws IOException on error
        */
        public PdfStamper(PdfReader reader, Stream os, char pdfVersion) {
            stamper = new PdfStamperImp(reader, os, pdfVersion, false);
        }

        /**
        * Starts the process of adding extra content to an existing PDF
        * document, possibly as a new revision.
        * The reader passed into the constructor will also be closed.
        * <p>
        * @param reader the original document. It cannot be reused
        * @param os the output stream
        * @param pdfVersion the new pdf version or '\0' to keep the same version as the original
        * document
        * @param append if <CODE>true</CODE> appends the document changes as a new revision. This is
        * only useful for multiple signatures as nothing is gained in speed or memory
        * @throws DocumentException on error
        * @throws IOException on error
        */
        public PdfStamper(PdfReader reader, Stream os, char pdfVersion, bool append) {
            stamper = new PdfStamperImp(reader, os, pdfVersion, append);
        }

        protected PdfStamper() { 
        
        }

        /** Gets the optional <CODE>String</CODE> map to add or change values in
        * the info dictionary.
        * @return the map or <CODE>null</CODE>
        *
        */
        /** An optional <CODE>String</CODE> map to add or change values in
        * the info dictionary. Entries with <CODE>null</CODE>
        * values delete the key in the original info dictionary
        * @param moreInfo additional entries to the info dictionary
        *
        */
        virtual public IDictionary<String, String> MoreInfo {
            set {
                moreInfo = value;
            }
            get {
                return moreInfo;
            }
        }
        
        /**
        * Replaces a page from this document with a page from other document. Only the content
        * is replaced not the fields and annotations. This method must be called before 
        * getOverContent() or getUndercontent() are called for the same page.
        * @param r the <CODE>PdfReader</CODE> from where the new page will be imported
        * @param pageImported the page number of the imported page
        * @param pageReplaced the page to replace in this document
        */
        virtual public void ReplacePage(PdfReader r, int pageImported, int pageReplaced) {
            stamper.ReplacePage(r, pageImported, pageReplaced);
        }

        /**
        * Inserts a blank page. All the pages above and including <CODE>pageNumber</CODE> will
        * be shifted up. If <CODE>pageNumber</CODE> is bigger than the total number of pages
        * the new page will be the last one.
        * @param pageNumber the page number position where the new page will be inserted
        * @param mediabox the size of the new page
        */    
        virtual public void InsertPage(int pageNumber, Rectangle mediabox) {
            stamper.InsertPage(pageNumber, mediabox);
        }
        
        /**
        * Gets the signing instance. The appearances and other parameters can the be set.
        * @return the signing instance
        */    
        virtual public PdfSignatureAppearance SignatureAppearance {
            get {
                return sigApp;
            }
        }

        /**
         * Gets the xml signing instance. The appearances and other parameters can the be set.
         * @return the signing instance
         */
        virtual public XmlSignatureAppearance XmlSignatureAppearance {
            get {
                return sigXmlApp;
            }
        }

        /**
        * Closes the document. No more content can be written after the
        * document is closed.
        * <p>
        * If closing a signed document with an external signature the closing must be done
        * in the <CODE>PdfSignatureAppearance</CODE> instance.
        * @throws DocumentException on error
        * @throws IOException on error
        */
        virtual public void Close() {
            if (stamper.closed)
                return;
            if (!hasSignature) {
                MergeVerification();
                stamper.Close(moreInfo);
            }
            else {
                throw new DocumentException("Signature defined. Must be closed in PdfSignatureAppearance.");
            }
        }

        /** Gets a <CODE>PdfContentByte</CODE> to write under the page of
        * the original document.
        * @param pageNum the page number where the extra content is written
        * @return a <CODE>PdfContentByte</CODE> to write under the page of
        * the original document
        */    
        virtual public PdfContentByte GetUnderContent(int pageNum) {
            return stamper.GetUnderContent(pageNum);
        }

        /** Gets a <CODE>PdfContentByte</CODE> to write over the page of
        * the original document.
        * @param pageNum the page number where the extra content is written
        * @return a <CODE>PdfContentByte</CODE> to write over the page of
        * the original document
        */    
        virtual public PdfContentByte GetOverContent(int pageNum) {
            return stamper.GetOverContent(pageNum);
        }
        
        /** Checks if the content is automatically adjusted to compensate
        * the original page rotation.
        * @return the auto-rotation status
        */    
        /** Flags the content to be automatically adjusted to compensate
        * the original page rotation. The default is <CODE>true</CODE>.
        * @param rotateContents <CODE>true</CODE> to set auto-rotation, <CODE>false</CODE>
        * otherwise
        */    
        virtual public bool RotateContents {
            set {
                stamper.RotateContents = value;
            }
            get {
                return stamper.RotateContents;
            }
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
        * @throws DocumentException if anything was already written to the output
        */
        virtual public void SetEncryption(byte[] userPassword, byte[] ownerPassword, int permissions, bool strength128Bits) {
            if (stamper.append)
                throw new DocumentException(MessageLocalization.GetComposedMessage("append.mode.does.not.support.changing.the.encryption.status"));
            if (stamper.ContentWritten)
                throw new DocumentException(MessageLocalization.GetComposedMessage("content.was.already.written.to.the.output"));
            stamper.SetEncryption(userPassword, ownerPassword, permissions, strength128Bits ? PdfWriter.STANDARD_ENCRYPTION_128 : PdfWriter.STANDARD_ENCRYPTION_40);
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
        * @param encryptionType the type of encryption. It can be one of STANDARD_ENCRYPTION_40, STANDARD_ENCRYPTION_128 or ENCRYPTION_AES128.
        * Optionally DO_NOT_ENCRYPT_METADATA can be ored to output the metadata in cleartext
        * @throws DocumentException if the document is already open
        */
        virtual public void SetEncryption(byte[] userPassword, byte[] ownerPassword, int permissions, int encryptionType) {
            if (stamper.IsAppend())
                throw new DocumentException(MessageLocalization.GetComposedMessage("append.mode.does.not.support.changing.the.encryption.status"));
            if (stamper.ContentWritten)
                throw new DocumentException(MessageLocalization.GetComposedMessage("content.was.already.written.to.the.output"));
            stamper.SetEncryption(userPassword, ownerPassword, permissions, encryptionType);
        }

        /**
        * Sets the encryption options for this document. The userPassword and the
        *  ownerPassword can be null or have zero length. In this case the ownerPassword
        *  is replaced by a random string. The open permissions for the document can be
        *  AllowPrinting, AllowModifyContents, AllowCopy, AllowModifyAnnotations,
        *  AllowFillIn, AllowScreenReaders, AllowAssembly and AllowDegradedPrinting.
        *  The permissions can be combined by ORing them.
        * @param strength <code>true</code> for 128 bit key length, <code>false</code> for 40 bit key length
        * @param userPassword the user password. Can be null or empty
        * @param ownerPassword the owner password. Can be null or empty
        * @param permissions the user permissions
        * @throws DocumentException if anything was already written to the output
        */
        virtual public void SetEncryption(bool strength, String userPassword, String ownerPassword, int permissions) {
            SetEncryption(DocWriter.GetISOBytes(userPassword), DocWriter.GetISOBytes(ownerPassword), permissions, strength);
        }

        /**
        * Sets the encryption options for this document. The userPassword and the
        *  ownerPassword can be null or have zero length. In this case the ownerPassword
        *  is replaced by a random string. The open permissions for the document can be
        *  AllowPrinting, AllowModifyContents, AllowCopy, AllowModifyAnnotations,
        *  AllowFillIn, AllowScreenReaders, AllowAssembly and AllowDegradedPrinting.
        *  The permissions can be combined by ORing them.
        * @param encryptionType the type of encryption. It can be one of STANDARD_ENCRYPTION_40, STANDARD_ENCRYPTION_128 or ENCRYPTION_AES128.
        * Optionally DO_NOT_ENCRYPT_METADATA can be ored to output the metadata in cleartext
        * @param userPassword the user password. Can be null or empty
        * @param ownerPassword the owner password. Can be null or empty
        * @param permissions the user permissions
        * @throws DocumentException if the document is already open
        */
        virtual public void SetEncryption(int encryptionType, String userPassword, String ownerPassword, int permissions) {
            SetEncryption(DocWriter.GetISOBytes(userPassword), DocWriter.GetISOBytes(ownerPassword), permissions, encryptionType);
        }

        /**
        * Sets the certificate encryption options for this document. An array of one or more public certificates
        * must be provided together with an array of the same size for the permissions for each certificate.
        *  The open permissions for the document can be
        *  AllowPrinting, AllowModifyContents, AllowCopy, AllowModifyAnnotations,
        *  AllowFillIn, AllowScreenReaders, AllowAssembly and AllowDegradedPrinting.
        *  The permissions can be combined by ORing them.
        * Optionally DO_NOT_ENCRYPT_METADATA can be ored to output the metadata in cleartext
        * @param certs the public certificates to be used for the encryption
        * @param permissions the user permissions for each of the certicates
        * @param encryptionType the type of encryption. It can be one of STANDARD_ENCRYPTION_40, STANDARD_ENCRYPTION_128 or ENCRYPTION_AES128.
        * @throws DocumentException if the encryption was set too late
        */
        virtual public void SetEncryption(X509Certificate[] certs, int[] permissions, int encryptionType) {
            if (stamper.IsAppend())
                throw new DocumentException(MessageLocalization.GetComposedMessage("append.mode.does.not.support.changing.the.encryption.status"));
            if (stamper.ContentWritten)
                throw new DocumentException(MessageLocalization.GetComposedMessage("content.was.already.written.to.the.output"));
            stamper.SetEncryption(certs, permissions, encryptionType);
        }

        /** Gets a page from other PDF document. Note that calling this method more than
        * once with the same parameters will retrieve the same object.
        * @param reader the PDF document where the page is
        * @param pageNumber the page number. The first page is 1
        * @return the template representing the imported page
        */
        virtual public PdfImportedPage GetImportedPage(PdfReader reader, int pageNumber) {
            return stamper.GetImportedPage(reader, pageNumber);
        }
        
        /** Gets the underlying PdfWriter.
        * @return the underlying PdfWriter
        */    
        virtual public PdfWriter Writer {
            get {
                return stamper;
            }
        }
        
        /** Gets the underlying PdfReader.
        * @return the underlying PdfReader
        */
        virtual public PdfReader Reader {
            get {
                return stamper.reader;
            }
        }

        /** Gets the <CODE>AcroFields</CODE> object that allows to get and set field values
        * and to merge FDF forms.
        * @return the <CODE>AcroFields</CODE> object
        */    
        virtual public AcroFields AcroFields {
            get {
                return stamper.GetAcroFields();
            }
        }
        
        /** Determines if the fields are flattened on close. The fields added with
        * {@link #addAnnotation(PdfAnnotation,int)} will never be flattened.
        * @param flat <CODE>true</CODE> to flatten the fields, <CODE>false</CODE>
        * to keep the fields
        */    
        virtual public bool FormFlattening {
            set {
                stamper.FormFlattening = value;
            }
        }

        /** Determines if the FreeText annotations are flattened on close. 
        * @param flat <CODE>true</CODE> to flatten the FreeText annotations, <CODE>false</CODE>
        * (the default) to keep the FreeText annotations as active content.
        */
        virtual public bool FreeTextFlattening {
            set {
                stamper.FreeTextFlattening = value;
            }
        }

        /**
        * Flatten annotations with an appearance stream on close().
        *
        * @param flat boolean to indicate whether iText should flatten annotations or not.
        */
        public virtual bool AnnotationFlattening {
            set {
                stamper.FlatAnnotations = value;
            }
        }

        /**
        * Adds an annotation of form field in a specific page. This page number
        * can be overridden with {@link PdfAnnotation#setPlaceInPage(int)}.
        * @param annot the annotation
        * @param page the page
        */    
        virtual public void AddAnnotation(PdfAnnotation annot, int page) {
            stamper.AddAnnotation(annot, page);
        }

        /**
        * Adds an empty signature.
        * @param name   the name of the signature
        * @param page   the page number
        * @param llx    lower left x coordinate of the signature's position
        * @param lly    lower left y coordinate of the signature's position
        * @param urx    upper right x coordinate of the signature's position
        * @param ury    upper right y coordinate of the signature's position
        * @return   a signature form field
        * @since    2.1.4
        */
        virtual public PdfFormField AddSignature(String name, int page, float llx, float lly, float urx, float ury) {
            PdfAcroForm acroForm = stamper.AcroForm;
            PdfFormField signature = PdfFormField.CreateSignature(stamper);
            acroForm.SetSignatureParams(signature, name, llx, lly, urx, ury);
            acroForm.DrawSignatureAppearences(signature, llx, lly, urx, ury);
            AddAnnotation(signature, page);
            return signature;
        }
            
        /**
        * Adds the comments present in an FDF file.
        * @param fdf the FDF file
        * @throws IOException on error
        */    
        virtual public void AddComments(FdfReader fdf) {
            stamper.AddComments(fdf);
        }
        
        /**
        * Sets the bookmarks. The list structure is defined in
        * {@link SimpleBookmark}.
        * @param outlines the bookmarks or <CODE>null</CODE> to remove any
        */    
        virtual public IList<Dictionary<String, Object>> Outlines {
            set {
                stamper.Outlines = value;
            }
        }

        /**
        * Sets the thumbnail image for a page.
        * @param image the image
        * @param page the page
        * @throws PdfException on error
        * @throws DocumentException on error
        */    
        virtual public void SetThumbnail(Image image, int page) {
            stamper.SetThumbnail(image, page);
        }
        
        /**
        * Adds <CODE>name</CODE> to the list of fields that will be flattened on close,
        * all the other fields will remain. If this method is never called or is called
        * with invalid field names, all the fields will be flattened.
        * <p>
        * Calling <CODE>setFormFlattening(true)</CODE> is needed to have any kind of
        * flattening.
        * @param name the field name
        * @return <CODE>true</CODE> if the field exists, <CODE>false</CODE> otherwise
        */    
        virtual public bool PartialFormFlattening(String name) {
            return stamper.PartialFormFlattening(name);
        }
        
        /** Adds a JavaScript action at the document level. When the document
        * opens all this JavaScript runs. The existing JavaScript will be replaced.
        * @param js the JavaScript code
        */
        virtual public string JavaScript {
            set {
                stamper.AddJavaScript(value, !PdfEncodings.IsPdfDocEncoding(value));
            }
        }

        /** Adds a JavaScript action at the document level. When the document
         * opens all this JavaScript runs. The existing JavaScript will be replaced.
         * @param name the name for the JavaScript snippet in the name tree
         * @param js the JavaScript code
         */
        public virtual void AddJavaScript(String name, String js) {
            stamper.AddJavaScript(name, PdfAction.JavaScript(js, stamper, !PdfEncodings.IsPdfDocEncoding(js)));
        }
        
        /** Adds a file attachment at the document level. Existing attachments will be kept.
        * @param description the file description
        * @param fileStore an array with the file. If it's <CODE>null</CODE>
        * the file will be read from the disk
        * @param file the path to the file. It will only be used if
        * <CODE>fileStore</CODE> is not <CODE>null</CODE>
        * @param fileDisplay the actual file name stored in the pdf
        * @throws IOException on error
        */    
        virtual public void AddFileAttachment(String description, byte[] fileStore, String file, String fileDisplay) {
            AddFileAttachment(description, PdfFileSpecification.FileEmbedded(stamper, file, fileDisplay, fileStore));
        }

        /** Adds a file attachment at the document level. Existing attachments will be kept.
        * @param description the file description
        * @param fs the file specification
        */    
        virtual public void AddFileAttachment(String description, PdfFileSpecification fs) {
            stamper.AddFileAttachment(description, fs);
        }

        /**
        * This is the most simple way to change a PDF into a
        * portable collection. Choose one of the following names:
        * <ul>
        * <li>PdfName.D (detailed view)
        * <li>PdfName.T (tiled view)
        * <li>PdfName.H (hidden)
        * </ul>
        * Pass this name as a parameter and your PDF will be
        * a portable collection with all the embedded and
        * attached files as entries.
        * @param initialView can be PdfName.D, PdfName.T or PdfName.H
        */
        virtual public void MakePackage( PdfName initialView ) {
            PdfCollection collection = new PdfCollection(0);
            collection.Put(PdfName.VIEW, initialView);
            stamper.MakePackage( collection );
        }
        
        /**
        * Adds or replaces the Collection Dictionary in the Catalog.
        * @param    collection  the new collection dictionary.
        */
        virtual public void MakePackage(PdfCollection collection) {
            stamper.MakePackage(collection);        
        }

        /**
        * Sets the viewer preferences.
        * @param preferences the viewer preferences
        * @see PdfViewerPreferences#setViewerPreferences(int)
        */
        public virtual int ViewerPreferences {
            set {
                stamper.ViewerPreferences = value;
            }
        }

        /** Adds a viewer preference
        * @param preferences the viewer preferences
        * @see PdfViewerPreferences#addViewerPreference
        */
        
        public virtual void AddViewerPreference(PdfName key, PdfObject value) {
    	    stamper.AddViewerPreference(key, value);
        }

        /**
        * Sets the XMP metadata.
        * @param xmp
        * @see PdfWriter#setXmpMetadata(byte[])
        */
        virtual public byte[] XmpMetadata {
            set {
                stamper.XmpMetadata = value;
            }
        }

        virtual public void CreateXmpMetadata() {
            stamper.CreateXmpMetadata();
        }

        virtual public XmpWriter XmpWriter {
            get { return stamper.XmpWriter; }
        }

        /**
        * Gets the 1.5 compression status.
        * @return <code>true</code> if the 1.5 compression is on
        */
        virtual public bool FullCompression {
            get {
                return stamper.FullCompression;
            }
        }

         /**
         * Sets the document's compression to the new 1.5 mode with object streams and xref
         * streams. Be attentive!!! If you want set full compression , you should set immediately after creating PdfStamper,
         * before editing the document.It can be set once and it can't be unset.
         */
        virtual public void SetFullCompression() {
            if (stamper.append)
                return;
            stamper.fullCompression = true;
            stamper.SetAtLeastPdfVersion(PdfWriter.VERSION_1_5);
        }    

        /**
        * Sets the open and close page additional action.
        * @param actionType the action type. It can be <CODE>PdfWriter.PAGE_OPEN</CODE>
        * or <CODE>PdfWriter.PAGE_CLOSE</CODE>
        * @param action the action to perform
        * @param page the page where the action will be applied. The first page is 1
        * @throws PdfException if the action type is invalid
        */    
        virtual public void SetPageAction(PdfName actionType, PdfAction action, int page) {
            stamper.SetPageAction(actionType, action, page);
        }

        /**
        * Sets the display duration for the page (for presentations)
        * @param seconds   the number of seconds to display the page. A negative value removes the entry
        * @param page the page where the duration will be applied. The first page is 1
        */
        virtual public void SetDuration(int seconds, int page) {
            stamper.SetDuration(seconds, page);
        }
        
        /**
        * Sets the transition for the page
        * @param transition   the transition object. A <code>null</code> removes the transition
        * @param page the page where the transition will be applied. The first page is 1
        */
        virtual public void SetTransition(PdfTransition transition, int page) {
            stamper.SetTransition(transition, page);
        }

        /**
        * Applies a digital signature to a document, possibly as a new revision, making
        * possible multiple signatures. The returned PdfStamper
        * can be used normally as the signature is only applied when closing.
        * <p>
        * A possible use for adding a signature without invalidating an existing one is:
        * <p>
        * <pre>
        * KeyStore ks = KeyStore.getInstance("pkcs12");
        * ks.load(new FileInputStream("my_private_key.pfx"), "my_password".toCharArray());
        * String alias = (String)ks.aliases().nextElement();
        * PrivateKey key = (PrivateKey)ks.getKey(alias, "my_password".toCharArray());
        * Certificate[] chain = ks.getCertificateChain(alias);
        * PdfReader reader = new PdfReader("original.pdf");
        * FileOutputStream fout = new FileOutputStream("signed.pdf");
        * PdfStamper stp = PdfStamper.createSignature(reader, fout, '\0', new
        * File("/temp"), true);
        * PdfSignatureAppearance sap = stp.getSignatureAppearance();
        * sap.setCrypto(key, chain, null, PdfSignatureAppearance.WINCER_SIGNED);
        * sap.setReason("I'm the author");
        * sap.setLocation("Lisbon");
        * // comment next line to have an invisible signature
        * sap.setVisibleSignature(new Rectangle(100, 100, 200, 200), 1, null);
        * stp.close();
        * </pre>
        * @param reader the original document
        * @param os the output stream or <CODE>null</CODE> to keep the document in the temporary file
        * @param pdfVersion the new pdf version or '\0' to keep the same version as the original
        * document
        * @param tempFile location of the temporary file. If it's a directory a temporary file will be created there.
        *     If it's a file it will be used directly. The file will be deleted on exit unless <CODE>os</CODE> is null.
        *     In that case the document can be retrieved directly from the temporary file. If it's <CODE>null</CODE>
        *     no temporary file will be created and memory will be used
        * @param append if <CODE>true</CODE> the signature and all the other content will be added as a
        * new revision thus not invalidating existing signatures
        * @return a <CODE>PdfStamper</CODE>
        * @throws DocumentException on error
        * @throws IOException on error
        */
        public static PdfStamper CreateSignature(PdfReader reader, Stream os, char pdfVersion, string tempFile, bool append) {
            PdfStamper stp;
            if (tempFile == null) {
                ByteBuffer bout = new ByteBuffer();
                stp = new PdfStamper(reader, bout, pdfVersion, append);
                stp.sigApp = new PdfSignatureAppearance(stp.stamper);
                stp.sigApp.Sigout = bout;
            }
            else {
                if (Directory.Exists(tempFile))
                    tempFile = Path.GetTempFileName();
                FileStream fout = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
                stp = new PdfStamper(reader, fout, pdfVersion, append);
                stp.sigApp = new PdfSignatureAppearance(stp.stamper);
                stp.sigApp.SetTempFile(tempFile);
            }
            stp.sigApp.Originalout = os;
            stp.sigApp.SetStamper(stp);
            stp.hasSignature = true;
            PdfDictionary catalog = reader.Catalog;
            PdfDictionary acroForm = (PdfDictionary)PdfReader.GetPdfObject(catalog.Get(PdfName.ACROFORM), catalog);
            if (acroForm != null) {
                acroForm.Remove(PdfName.NEEDAPPEARANCES);
                stp.stamper.MarkUsed(acroForm);
            }
            return stp;
        }

        /**
        * Applies a digital signature to a document. The returned PdfStamper
        * can be used normally as the signature is only applied when closing.
        * <p>
        * Note that the pdf is created in memory.
        * <p>
        * A possible use is:
        * <p>
        * <pre>
        * KeyStore ks = KeyStore.getInstance("pkcs12");
        * ks.load(new FileInputStream("my_private_key.pfx"), "my_password".toCharArray());
        * String alias = (String)ks.aliases().nextElement();
        * PrivateKey key = (PrivateKey)ks.getKey(alias, "my_password".toCharArray());
        * Certificate[] chain = ks.getCertificateChain(alias);
        * PdfReader reader = new PdfReader("original.pdf");
        * FileOutputStream fout = new FileOutputStream("signed.pdf");
        * PdfStamper stp = PdfStamper.createSignature(reader, fout, '\0');
        * PdfSignatureAppearance sap = stp.getSignatureAppearance();
        * sap.setCrypto(key, chain, null, PdfSignatureAppearance.WINCER_SIGNED);
        * sap.setReason("I'm the author");
        * sap.setLocation("Lisbon");
        * // comment next line to have an invisible signature
        * sap.setVisibleSignature(new Rectangle(100, 100, 200, 200), 1, null);
        * stp.close();
        * </pre>
        * @param reader the original document
        * @param os the output stream
        * @param pdfVersion the new pdf version or '\0' to keep the same version as the original
        * document
        * @throws DocumentException on error
        * @throws IOException on error
        * @return a <CODE>PdfStamper</CODE>
        */
        public static PdfStamper CreateSignature(PdfReader reader, Stream os, char pdfVersion) {
            return CreateSignature(reader, os, pdfVersion, null, false);
        }
        
        /**
        * Applies a digital signature to a document. The returned PdfStamper
        * can be used normally as the signature is only applied when closing.
        * <p>
        * A possible use is:
        * <p>
        * <pre>
        * KeyStore ks = KeyStore.getInstance("pkcs12");
        * ks.load(new FileInputStream("my_private_key.pfx"), "my_password".toCharArray());
        * String alias = (String)ks.aliases().nextElement();
        * PrivateKey key = (PrivateKey)ks.getKey(alias, "my_password".toCharArray());
        * Certificate[] chain = ks.getCertificateChain(alias);
        * PdfReader reader = new PdfReader("original.pdf");
        * FileOutputStream fout = new FileOutputStream("signed.pdf");
        * PdfStamper stp = PdfStamper.createSignature(reader, fout, '\0', new File("/temp"));
        * PdfSignatureAppearance sap = stp.getSignatureAppearance();
        * sap.setCrypto(key, chain, null, PdfSignatureAppearance.WINCER_SIGNED);
        * sap.setReason("I'm the author");
        * sap.setLocation("Lisbon");
        * // comment next line to have an invisible signature
        * sap.setVisibleSignature(new Rectangle(100, 100, 200, 200), 1, null);
        * stp.close();
        * </pre>
        * @param reader the original document
        * @param os the output stream or <CODE>null</CODE> to keep the document in the temporary file
        * @param pdfVersion the new pdf version or '\0' to keep the same version as the original
        * document
        * @param tempFile location of the temporary file. If it's a directory a temporary file will be created there.
        *     If it's a file it will be used directly. The file will be deleted on exit unless <CODE>os</CODE> is null.
        *     In that case the document can be retrieved directly from the temporary file. If it's <CODE>null</CODE>
        *     no temporary file will be created and memory will be used
        * @return a <CODE>PdfStamper</CODE>
        * @throws DocumentException on error
        * @throws IOException on error
        */
        public static PdfStamper CreateSignature(PdfReader reader, Stream os, char pdfVersion, string tempFile) {
            return CreateSignature(reader, os, pdfVersion, tempFile, false);
        }

        public static PdfStamper createXmlSignature(PdfReader reader, Stream os) {
            PdfStamper stp = new PdfStamper(reader, os);
            stp.sigXmlApp = new XmlSignatureAppearance(stp.stamper);
            //stp.sigApp.setSigout(bout);
            //stp.sigApp.setOriginalout(os);
            stp.sigXmlApp.SetStamper(stp);

            return stp;
        }

        /**
        * Gets the PdfLayer objects in an existing document as a Map
        * with the names/titles of the layers as keys.
        * @return   a Map with all the PdfLayers in the document (and the name/title of the layer as key)
        * @since    2.1.2
        */
        virtual public Dictionary<string,PdfLayer> GetPdfLayers() {
            return stamper.GetPdfLayers();
        }

        virtual public void Dispose() {
            Close();
        }

        virtual public void MarkUsed(PdfObject obj) {
            stamper.MarkUsed(obj);
        }

        virtual public LtvVerification LtvVerification {
            get {
                if (verification == null)
                    verification = new LtvVerification(this);
                return verification;
            }
        }
        
        internal void MergeVerification() {
            if (verification == null)
                return;
            verification.Merge();
        }
    }
}
