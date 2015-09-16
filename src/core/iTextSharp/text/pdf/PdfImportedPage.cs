using System;
using iTextSharp.text.error_messages;

using iTextSharp.text;

/*
 * $Id$
 * 
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

namespace iTextSharp.text.pdf {

    /** Represents an imported page.
     *
     * @author Paulo Soares
     */
    public class PdfImportedPage : PdfTemplate {

        internal PdfReaderInstance readerInstance;
        internal int pageNumber;
        internal int rotation;
        /**
         * True if the imported page has been copied to a writer.
         * @since iText 5.0.4
         */
        protected internal bool toCopy = true;
    
        internal PdfImportedPage(PdfReaderInstance readerInstance, PdfWriter writer, int pageNumber) {
            this.readerInstance = readerInstance;
            this.pageNumber = pageNumber;
            this.rotation = readerInstance.Reader.GetPageRotation(pageNumber);
            this.writer = writer;
            bBox = readerInstance.Reader.GetPageSize(pageNumber);
            SetMatrix(1, 0, 0, 1, -bBox.Left, -bBox.Bottom);
            type = TYPE_IMPORTED;
        }

        /** Reads the content from this <CODE>PdfImportedPage</CODE>-object from a reader.
        *
        * @return self
        *
        */
        virtual public PdfImportedPage FromReader {
            get {
                return this;
            }
        }

        virtual public int PageNumber {
            get {
                return pageNumber;
            }
        }

        virtual public int Rotation {
            get { return rotation; }
        }

        /** Always throws an error. This operation is not allowed.
         * @param image dummy
         * @param a dummy
         * @param b dummy
         * @param c dummy
         * @param d dummy
         * @param e dummy
         * @param f dummy
         * @throws DocumentException  dummy */    
        public override void AddImage(Image image, float a, float b, float c, float d, float e, float f) {
            ThrowError();
        }
    
        /** Always throws an error. This operation is not allowed.
         * @param template dummy
         * @param a dummy
         * @param b dummy
         * @param c dummy
         * @param d dummy
         * @param e dummy
         * @param f  dummy */    
        public override void AddTemplate(PdfTemplate template, float a, float b, float c, float d, float e, float f) {
            ThrowError();
        }
    
        /** Always throws an error. This operation is not allowed.
         * @return  dummy */    
        public override PdfContentByte Duplicate {
            get {
                ThrowError();
                return null;
            }
        }
    
        /**
        * Gets the stream representing this page.
        *
        * @param   compressionLevel    the compressionLevel
        * @return the stream representing this page
        * @since   2.1.3   (replacing the method without param compressionLevel)
        */
        override public PdfStream GetFormXObject(int compressionLevel) {
            return readerInstance.GetFormXObject(pageNumber, compressionLevel);
        }
    
        public override void SetColorFill(PdfSpotColor sp, float tint) {
            ThrowError();
        }
    
        public override void SetColorStroke(PdfSpotColor sp, float tint) {
            ThrowError();
        }
    
        internal override PdfObject Resources {
            get {
                return readerInstance.GetResources(pageNumber);
            }
        }
    
        /** Always throws an error. This operation is not allowed.
         * @param bf dummy
         * @param size dummy */    
        public override void SetFontAndSize(BaseFont bf, float size) {
            ThrowError();
        }
    
        public override PdfTransparencyGroup Group {
            set {
                ThrowError();
            }
        }

        internal void ThrowError() {
            throw new Exception(MessageLocalization.GetComposedMessage("content.can.not.be.added.to.a.pdfimportedpage"));
        }
    
        internal PdfReaderInstance PdfReaderInstance {
            get {
                return readerInstance;
            }
        }

        /**
         * Checks if the page has to be copied.
         * @return true if the page has to be copied.
         * @since iText 5.0.4
         */
        virtual public bool IsToCopy() {
            return toCopy;
        }

        /**
         * Indicate that the resources of the imported page have been copied.
         * @since iText 5.0.4
         */
        virtual public void SetCopied() {
            toCopy = false;
        }
    }
}
