using System;
using System.IO;
using System.Drawing;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.codec;
using iTextSharp.text.exceptions;
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
        private PdfDictionary dictionary;
        /** The decoded image bytes (after applying filters), or the raw image bytes if unable to decode */
        private byte[] streamBytes;

        private int pngColorType = -1;
        private int pngBitDepth;
        private int width;
        private int height;
        private int bpc;
        private byte[] palette;
        private byte[] icc;
        private int stride;
        private bool decoded;
        public const string TYPE_PNG = "png";
        public const string TYPE_JPG = "jpg";
        public const string TYPE_JP2 = "jp2";
        public const string TYPE_TIF = "tif";
        
        protected string fileType;

        public string GetFileType() {
            return fileType;
        }

        /**
         * Creates a PdfImage object.
         * @param stream a PRStream
         * @throws IOException
         */
        public PdfImageObject(PRStream stream) : this(stream, PdfReader.GetStreamBytesRaw(stream)) {
        }
        
        /**
         * Creats a PdfImage object using an explicitly provided dictionary and image bytes
         * @param dictionary the dictionary for the image
         * @param samples the samples
         * @since 5.0.3
         */
        protected internal PdfImageObject(PdfDictionary dictionary, byte[] samples)  {
            this.dictionary = dictionary;
            try{
                streamBytes = PdfReader.DecodeBytes(samples, dictionary);
                decoded = true;
            } catch (UnsupportedPdfException){
                // it's possible that the filter type was jpx or jpg, in which case we can still use the streams as-is, so we'll just hold onto the samples
                streamBytes = samples;
                decoded = false;
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

        public byte[] GetImageAsBytes() {
            if (streamBytes == null)
                return null;
            if (!decoded) {
                // if the stream hasn't been decoded, check to see if it is a single stage JPG or JPX encoded stream.  If it is,
                // then we can just use stream as-is
                PdfName filter = dictionary.GetAsName(PdfName.FILTER);
                if (filter == null){
                    PdfArray filterArray = dictionary.GetAsArray(PdfName.FILTER);
                    if (filterArray.Size == 1){
                        filter = filterArray.GetAsName(0);
                    } else {
                        throw new UnsupportedPdfException("Multi-stage filters not supported here (" + filterArray + ")");
                    }
                }
                if (PdfName.DCTDECODE.Equals(filter)) {
                    fileType = TYPE_JPG;
                    return streamBytes;
                }
                else if (PdfName.JPXDECODE.Equals(filter)) {
                    fileType = TYPE_JP2;
                    return streamBytes;
                }
                throw new UnsupportedPdfException("Unsupported stream filter " + filter);
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
            MemoryStream ms = new MemoryStream();
            if (pngColorType < 0) {
                if (bpc != 8)
                    return null;
                if (PdfName.DEVICECMYK.Equals(colorspace)) {
                }
                else if (colorspace is PdfArray) {
                    PdfArray ca = (PdfArray)colorspace;
                    PdfObject tyca = ca.GetDirectObject(0);
                    if (!PdfName.ICCBASED.Equals(tyca))
                        return null;
                    PRStream pr = (PRStream)ca.GetDirectObject(1);
                    int n = pr.GetAsNumber(PdfName.N).IntValue;
                    if (n != 4) {
                        return null;
                    }
                    icc = PdfReader.GetStreamBytes(pr);
                }
                else
                    return null;
                stride = 4 * width;
                TiffWriter wr = new TiffWriter();
                wr.AddField(new TiffWriter.FieldShort(TIFFConstants.TIFFTAG_SAMPLESPERPIXEL, 4));
                wr.AddField(new TiffWriter.FieldShort(TIFFConstants.TIFFTAG_BITSPERSAMPLE, new int[]{8,8,8,8}));
                wr.AddField(new TiffWriter.FieldShort(TIFFConstants.TIFFTAG_PHOTOMETRIC, TIFFConstants.PHOTOMETRIC_SEPARATED));
                wr.AddField(new TiffWriter.FieldLong(TIFFConstants.TIFFTAG_IMAGEWIDTH, width));
                wr.AddField(new TiffWriter.FieldLong(TIFFConstants.TIFFTAG_IMAGELENGTH, height));
                wr.AddField(new TiffWriter.FieldShort(TIFFConstants.TIFFTAG_COMPRESSION, TIFFConstants.COMPRESSION_LZW));
                wr.AddField(new TiffWriter.FieldShort(TIFFConstants.TIFFTAG_PREDICTOR, TIFFConstants.PREDICTOR_HORIZONTAL_DIFFERENCING));
                wr.AddField(new TiffWriter.FieldLong(TIFFConstants.TIFFTAG_ROWSPERSTRIP, height));
                wr.AddField(new TiffWriter.FieldRational(TIFFConstants.TIFFTAG_XRESOLUTION, new int[]{300,1}));
                wr.AddField(new TiffWriter.FieldRational(TIFFConstants.TIFFTAG_YRESOLUTION, new int[]{300,1}));
                wr.AddField(new TiffWriter.FieldShort(TIFFConstants.TIFFTAG_RESOLUTIONUNIT, TIFFConstants.RESUNIT_INCH));
                wr.AddField(new TiffWriter.FieldAscii(TIFFConstants.TIFFTAG_SOFTWARE, Document.Version));
                MemoryStream comp = new MemoryStream();
                TiffWriter.CompressLZW(comp, 2, streamBytes, height, 4, stride);
                byte[] buf = comp.ToArray();
                wr.AddField(new TiffWriter.FieldImage(buf));
                wr.AddField(new TiffWriter.FieldLong(TIFFConstants.TIFFTAG_STRIPBYTECOUNTS, buf.Length));
                if (icc != null)
                    wr.AddField(new TiffWriter.FieldUndefined(TIFFConstants.TIFFTAG_ICCPROFILE, icc));
                wr.WriteFile(ms);
                fileType = TYPE_TIF;
                return ms.ToArray();
            }
            PngWriter png = new PngWriter(ms);
            png.WriteHeader(width, height, pngBitDepth, pngColorType);
            if (icc != null)
                png.WriteIccProfile(icc);
            if (palette != null)
                png.WritePalette(palette);
            png.WriteData(streamBytes, stride);
            png.WriteEnd();
            fileType = TYPE_PNG;
            return ms.ToArray();
        }

        public System.Drawing.Image GetDrawingImage() {
            byte[] r = GetImageAsBytes();
            if (r == null)
                return null;
            return Bitmap.FromStream(new MemoryStream(r));
        }
    }
}