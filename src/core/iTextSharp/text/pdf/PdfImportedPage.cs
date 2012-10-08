using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using iTextSharp.text.error_messages;

using iTextSharp.text;

/*
 * $Id$
 * 
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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
        /**
         * True if the imported page has been copied to a writer.
         * @since iText 5.0.4
         */
        protected internal bool toCopy = true;
    
        internal PdfImportedPage(PdfReaderInstance readerInstance, PdfWriter writer, int pageNumber) {
            this.readerInstance = readerInstance;
            this.pageNumber = pageNumber;
            this.writer = writer;
            bBox = readerInstance.Reader.GetPageSize(pageNumber); int rotationAngle = readerInstance.Reader.GetPageRotation(pageNumber);
            rotationAngle = rotationAngle % 360;
            if (rotationAngle != 0) {
                PointF[] pts = new PointF[2];
                pts[0]= new PointF(bBox.Left, bBox.Bottom);
                pts[1] = new PointF(bBox.Right, bBox.Top);
                Matrix rotation = new Matrix();
                rotation.Rotate(-rotationAngle);
                rotation.TransformPoints(pts);
                Rectangle transformedRectangle = new Rectangle(pts[0].X, pts[0].Y, pts[1].X, pts[1].Y);
                transformedRectangle.Normalize();
                Matrix transformation = new Matrix(1, 0, 0, 1, -transformedRectangle.Left, -transformedRectangle.Bottom);
                transformation.Multiply(rotation);
                float[] transformationMatrix = transformation.Elements;
                SetMatrix(transformationMatrix[0], transformationMatrix[1],
                          transformationMatrix[2], transformationMatrix[3],
                          transformationMatrix[4], transformationMatrix[5]);
            } else {
                SetMatrix(1, 0, 0, 1, -bBox.Left, -bBox.Bottom);
            }
            type = TYPE_IMPORTED;
        }

        /** Reads the content from this <CODE>PdfImportedPage</CODE>-object from a reader.
        *
        * @return self
        *
        */
        public PdfImportedPage FromReader {
            get {
                return this;
            }
        }

        public int PageNumber {
            get {
                return pageNumber;
            }
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
        public bool IsToCopy() {
            return toCopy;
        }

        /**
         * Indicate that the resources of the imported page have been copied.
         * @since iText 5.0.4
         */
        public void SetCopied() {
            toCopy = false;
        }
    }
}
