using System;
using System.IO;
using System.Net;
using iTextSharp.text.io;
using iTextSharp.text.pdf.collection;
using iTextSharp.text.error_messages;
using iTextSharp.text.pdf.intern;

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
    /** Specifies a file or an URL. The file can be extern or embedded.
    *
    * @author Paulo Soares
    */
    public class PdfFileSpecification : PdfDictionary {
        protected PdfWriter writer;
        protected PdfIndirectReference refi;
        
        /** Creates a new instance of PdfFileSpecification. The static methods are preferred. */
        public PdfFileSpecification() : base(PdfName.FILESPEC) {
        }
        
        /**
        * Creates a file specification of type URL.
        * @param writer the <CODE>PdfWriter</CODE>
        * @param url the URL
        * @return the file specification
        */    
        public static PdfFileSpecification Url(PdfWriter writer, String url) {
            PdfFileSpecification fs = new PdfFileSpecification();
            fs.writer = writer;
            fs.Put(PdfName.FS, PdfName.URL);
            fs.Put(PdfName.F, new PdfString(url));
            return fs;
        }

        /**
        * Creates a file specification with the file embedded. The file may
        * come from the file system or from a byte array. The data is flate compressed.
        * @param writer the <CODE>PdfWriter</CODE>
        * @param filePath the file path
        * @param fileDisplay the file information that is presented to the user
        * @param fileStore the byte array with the file. If it is not <CODE>null</CODE>
        * it takes precedence over <CODE>filePath</CODE>
        * @throws IOException on error
        * @return the file specification
        */    
        public static PdfFileSpecification FileEmbedded(PdfWriter writer, String filePath, String fileDisplay, byte[] fileStore) {
            return FileEmbedded(writer, filePath, fileDisplay, fileStore, PdfStream.BEST_COMPRESSION);
        }

        /**
        * Creates a file specification with the file embedded. The file may
        * come from the file system or from a byte array. The data is flate compressed.
        * @param writer the <CODE>PdfWriter</CODE>
        * @param filePath the file path
        * @param fileDisplay the file information that is presented to the user
        * @param fileStore the byte array with the file. If it is not <CODE>null</CODE>
        * it takes precedence over <CODE>filePath</CODE>
        * @param compressionLevel   the compression level to be used for compressing the file
        * it takes precedence over <CODE>filePath</CODE>
        * @throws IOException on error
        * @return the file specification
        * @since    2.1.3
        */    
        public static PdfFileSpecification FileEmbedded(PdfWriter writer, String filePath, String fileDisplay, byte[] fileStore, int compressionLevel) {
            return FileEmbedded(writer, filePath, fileDisplay, fileStore, null, null, compressionLevel);
        }
        
        
        /**
        * Creates a file specification with the file embedded. The file may
        * come from the file system or from a byte array.
        * @param writer the <CODE>PdfWriter</CODE>
        * @param filePath the file path
        * @param fileDisplay the file information that is presented to the user
        * @param fileStore the byte array with the file. If it is not <CODE>null</CODE>
        * it takes precedence over <CODE>filePath</CODE>
        * @param compress sets the compression on the data. Multimedia content will benefit little
        * from compression
        * @throws IOException on error
        * @return the file specification
        */    
        public static PdfFileSpecification FileEmbedded(PdfWriter writer, String filePath, String fileDisplay, byte[] fileStore, bool compress) {
            return FileEmbedded(writer, filePath, fileDisplay, fileStore, null, null, compress ? PdfStream.BEST_COMPRESSION : PdfStream.NO_COMPRESSION);
        }
        
        /**
        * Creates a file specification with the file embedded. The file may
        * come from the file system or from a byte array.
        * @param writer the <CODE>PdfWriter</CODE>
        * @param filePath the file path
        * @param fileDisplay the file information that is presented to the user
        * @param fileStore the byte array with the file. If it is not <CODE>null</CODE>
        * it takes precedence over <CODE>filePath</CODE>
        * @param compress sets the compression on the data. Multimedia content will benefit little
        * from compression
        * @param mimeType the optional mimeType
        * @param fileParameter the optional extra file parameters such as the creation or modification date
        * @throws IOException on error
        * @return the file specification
        */    
        public static PdfFileSpecification FileEmbedded(PdfWriter writer, String filePath, String fileDisplay, byte[] fileStore, bool compress, String mimeType, PdfDictionary fileParameter) {
            return FileEmbedded(writer, filePath, fileDisplay, fileStore, mimeType, fileParameter, compress ? PdfStream.BEST_COMPRESSION : PdfStream.NO_COMPRESSION);
        }
        
        /**
        * Creates a file specification with the file embedded. The file may
        * come from the file system or from a byte array.
        * @param writer the <CODE>PdfWriter</CODE>
        * @param filePath the file path
        * @param fileDisplay the file information that is presented to the user
        * @param fileStore the byte array with the file. If it is not <CODE>null</CODE>
        * it takes precedence over <CODE>filePath</CODE>
        * @param mimeType the optional mimeType
        * @param fileParameter the optional extra file parameters such as the creation or modification date
        * @param compressionLevel the level of compression
        * @throws IOException on error
        * @return the file specification
        * @since   2.1.3
        */    
        public static PdfFileSpecification FileEmbedded(PdfWriter writer, String filePath, String fileDisplay, byte[] fileStore, String mimeType, PdfDictionary fileParameter, int compressionLevel) {
            PdfFileSpecification fs = new PdfFileSpecification();
            fs.writer = writer;
            fs.Put(PdfName.F, new PdfString(fileDisplay));
            fs.SetUnicodeFileName(fileDisplay, false);
            PdfEFStream stream;
            Stream inp = null;
            PdfIndirectReference refi;
            PdfIndirectReference refFileLength = null;
            try {
                if (fileStore == null) {
                    refFileLength = writer.PdfIndirectReference;
                    if (File.Exists(filePath)) {
                        inp = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    }
                    else {
                        if (filePath.StartsWith("file:/") || filePath.StartsWith("http://") || filePath.StartsWith("https://")) {
                            WebRequest wr = WebRequest.Create(filePath);
                            wr.Credentials = CredentialCache.DefaultCredentials;
                            inp = wr.GetResponse().GetResponseStream();
                        }
                        else {
                            inp = StreamUtil.GetResourceStream(filePath);
                            if (inp == null)
                                throw new IOException(MessageLocalization.GetComposedMessage("1.not.found.as.file.or.resource", filePath));
                        }
                    }
                    stream = new PdfEFStream(inp, writer);
                }
                else
                    stream = new PdfEFStream(fileStore);
                stream.Put(PdfName.TYPE, PdfName.EMBEDDEDFILE);
                stream.FlateCompress(compressionLevel);

                PdfDictionary param = new PdfDictionary();
                if (fileParameter != null)
                    param.Merge(fileParameter);
                if (!param.Contains(PdfName.MODDATE)) {
                    param.Put(PdfName.MODDATE, new PdfDate());
                }
                if (fileStore == null) {
                    stream.Put(PdfName.PARAMS, refFileLength);
                } else {
                    param.Put(PdfName.SIZE, new PdfNumber(stream.RawLength));
                    stream.Put(PdfName.PARAMS, param);
                }

                if (mimeType != null)
                    stream.Put(PdfName.SUBTYPE, new PdfName(mimeType));

                refi = writer.AddToBody(stream).IndirectReference;
                if (fileStore == null) {
                    stream.WriteLength();
                    param.Put(PdfName.SIZE, new PdfNumber(stream.RawLength));
                    writer.AddToBody(param, refFileLength);
                }
            }
            finally {
                if (inp != null)
                    try{inp.Close();}catch{}
            }
            PdfDictionary f = new PdfDictionary();
            f.Put(PdfName.F, refi);
            f.Put(PdfName.UF, refi);
            fs.Put(PdfName.EF, f);
            return fs;
        }
        
        /**
        * Creates a file specification for an external file.
        * @param writer the <CODE>PdfWriter</CODE>
        * @param filePath the file path
        * @return the file specification
        */
        public static PdfFileSpecification FileExtern(PdfWriter writer, String filePath) {
            PdfFileSpecification fs = new PdfFileSpecification();
            fs.writer = writer;
            fs.Put(PdfName.F, new PdfString(filePath));
            return fs;
        }
        
        /**
        * Gets the indirect reference to this file specification.
        * Multiple invocations will retrieve the same value.
        * @throws IOException on error
        * @return the indirect reference
        */    
        virtual public PdfIndirectReference Reference {
            get {
                if (refi != null)
                    return refi;
                refi = writer.AddToBody(this).IndirectReference;
                return refi;
            }
        }
        
        /**
        * Sets the file name (the key /F) string as an hex representation
        * to support multi byte file names. The name must have the slash and
        * backslash escaped according to the file specification rules
        * @param fileName the file name as a byte array
        */    
        virtual public byte[] MultiByteFileName {
            set {
                Put(PdfName.F, new PdfString(value).SetHexWriting(true));
            }
        }

        /**
        * Adds the unicode file name (the key /UF). This entry was introduced
        * in PDF 1.7. The filename must have the slash and backslash escaped
        * according to the file specification rules.
        * @param filename  the filename
        * @param unicode   if true, the filename is UTF-16BE encoded; otherwise PDFDocEncoding is used;
        */    
        virtual public void SetUnicodeFileName(String filename, bool unicode) {
            Put(PdfName.UF, new PdfString(filename, unicode ? PdfObject.TEXT_UNICODE : PdfObject.TEXT_PDFDOCENCODING));
        }
        
        /**
        * Sets a flag that indicates whether an external file referenced by the file
        * specification is volatile. If the value is true, applications should never
        * cache a copy of the file.
        * @param volatile_file if true, the external file should not be cached
        */
        virtual public bool Volatile {
            set {
                Put(PdfName.V, new PdfBoolean(value));
            }
        }
        
        /**
        * Adds a description for the file that is specified here.
        * @param description   some text
        * @param unicode       if true, the text is added as a unicode string
        */
        virtual public void AddDescription(String description, bool unicode) {
            Put(PdfName.DESC, new PdfString(description, unicode ? PdfObject.TEXT_UNICODE : PdfObject.TEXT_PDFDOCENCODING));
        }
        
        /**
        * Adds the Collection item dictionary.
        */
        virtual public void AddCollectionItem(PdfCollectionItem ci) {
            Put(PdfName.CI, ci);
        }

        public override void ToPdf(PdfWriter writer, Stream os) {
            PdfWriter.CheckPdfIsoConformance(writer, PdfIsoKeys.PDFISOKEY_FILESPEC, this);
            base.ToPdf(writer, os);
        }

    }
}
