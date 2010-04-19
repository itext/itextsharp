using System;
using System.IO;
using System.Drawing;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.codec;
/*
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2009 1T3XT BVBA
 * Authors: Bruno Lowagie, Kevin Day, Paulo Soares, et al.
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

namespace iTextSharp.text.pdf.parser {

    /**
     * An object that contains an image dictionary and image bytes.
     * @since 5.0.2
     */
    public class PdfImageObject {

        /** The image dictionary. */
        protected PdfDictionary dictionary;
        /** The image bytes. */
        protected byte[] streamBytes;

        private int pngColorType = -1;
        private int pngBitDepth;
        private int width;
        private int height;
        private int bpc;
        private byte[] palette;
        private byte[] icc;
        int stride;
        public const string TYPE_PNG = "png";
        public const string TYPE_JPG = "jpg";
        public const string TYPE_JP2 = "jp2";
        
        protected string fileType;

        public string GetFileType() {
            return fileType;
        }

        /**
         * Creates a PdfImage object.
         * @param stream a PRStream
         * @throws IOException
         */
        public PdfImageObject(PRStream stream) {
            this.dictionary = stream;
            try {
                if (PdfName.FLATEDECODE.Equals(dictionary.GetAsName(PdfName.FILTER)))
                    streamBytes = PdfReader.GetStreamBytes(stream);
                // else if other filter (not supported yet)
                else
                    streamBytes = PdfReader.GetStreamBytesRaw(stream);
            }
            catch {
                streamBytes = null;
            }
        }
        
        /**
         * Returns an entry from the image dictionary.
         * @param key a key
         * @return the value
         */
        public PdfObject Get(PdfName key) {
            return dictionary.Get(key);
        }
        
        /**
         * Returns the image dictionary.
         * @return the dictionary
         */
        public PdfDictionary GetDictionary() {
            return dictionary;
        }

        /**
         * Returns the image bytes.
         * @return the streamBytes
         */
        public byte[] GetStreamBytes() {
            return streamBytes;
        }
        
        private void FindColorspace(PdfObject colorspace, bool allowIndexed) {
            if (PdfName.DEVICEGRAY.Equals(colorspace)) {
                stride = (width * bpc + 7) / 8;
                pngColorType = 0;
            }
            else if (PdfName.DEVICERGB.Equals(colorspace)) {
                if (bpc == 8 || bpc == 16) {
                    stride = (width * bpc * 3 + 7) / 8;
                    pngColorType = 2;
                }
            }
            else if (colorspace is PdfArray) {
                PdfArray ca = (PdfArray)colorspace;
                PdfObject tyca = ca.GetDirectObject(0);
                if (PdfName.CALGRAY.Equals(tyca)) {
                    stride = (width * bpc + 7) / 8;
                    pngColorType = 0;
                }
                else if (PdfName.CALRGB.Equals(tyca)) {
                    if (bpc == 8 || bpc == 16) {
                        stride = (width * bpc * 3 + 7) / 8;
                        pngColorType = 2;
                    }
                }
                else if (PdfName.ICCBASED.Equals(tyca)) {
                    PRStream pr = (PRStream)ca.GetDirectObject(1);
                    int n = pr.GetAsNumber(PdfName.N).IntValue;
                    if (n == 1) {
                        stride = (width * bpc + 7) / 8;
                        pngColorType = 0;
                        icc = PdfReader.GetStreamBytes(pr);
                    }
                    else if (n == 3) {
                        stride = (width * bpc * 3 + 7) / 8;
                        pngColorType = 2;
                        icc = PdfReader.GetStreamBytes(pr);
                    }
                }
                else if (allowIndexed && PdfName.INDEXED.Equals(tyca)) {
                    FindColorspace(ca.GetDirectObject(1), false);
                    if (pngColorType == 2) {
                        PdfObject id2 = ca.GetDirectObject(3);
                        if (id2 is PdfString) {
                            palette = ((PdfString)id2).GetBytes();
                        }
                        else if (id2 is PRStream) {
                            palette = PdfReader.GetStreamBytes(((PRStream)id2));
                        }
                        stride = (width * bpc + 7) / 8;
                        pngColorType = 3;
                    }
                }
            }
        }

        public byte[] GetFile() {
            if (streamBytes == null)
                return null;
            PdfName filter = dictionary.GetAsName(PdfName.FILTER);
            if (PdfName.DCTDECODE.Equals(filter)) {
                fileType = TYPE_JPG;
                return streamBytes;
            }
            else if (PdfName.JPXDECODE.Equals(filter)) {
                fileType = TYPE_JP2;
                return streamBytes;
            }

            if (filter != null && !PdfName.FLATEDECODE.Equals(filter)) {
                return null;
            }
            pngColorType = -1;
            width = dictionary.GetAsNumber(PdfName.WIDTH).IntValue;
            height = dictionary.GetAsNumber(PdfName.HEIGHT).IntValue;
            bpc = dictionary.GetAsNumber(PdfName.BITSPERCOMPONENT).IntValue;
            pngBitDepth = bpc;
            PdfObject colorspace = dictionary.GetDirectObject(PdfName.COLORSPACE);
            palette = null;
            icc = null;
            stride = 0;
            FindColorspace(colorspace, true);
            if (pngColorType < 0)
                return null;
            MemoryStream ms = new MemoryStream();
            PngWriter png = new PngWriter(ms);
            png.WriteHeader(width, height, pngBitDepth, pngColorType);
            //if (icc != null)
            //    png.WriteIccProfile(icc);
            if (palette != null)
                png.WritePalette(palette);
            png.WriteData(streamBytes, stride);
            png.WriteEnd();
            fileType = TYPE_PNG;
            return ms.ToArray();
        }

        public System.Drawing.Image GetDrawingImage() {
            byte[] r = GetFile();
            if (r == null)
                return null;
            return Bitmap.FromStream(new MemoryStream(r));
        }
    }
}