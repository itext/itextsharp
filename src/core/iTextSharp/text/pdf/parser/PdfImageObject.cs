using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text.error_messages;
using iTextSharp.text.pdf.codec;
using iTextSharp.text.exceptions;
/*
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Kevin Day, Paulo Soares, et al.
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

namespace iTextSharp.text.pdf.parser {

    /**
     * An object that contains an image dictionary and image bytes.
     * @since 5.0.2
     */
    public class PdfImageObject {

        /**
         * Different types of data that can be stored in the bytes of a {@link PdfImageObject}
         * @since 5.0.4
         */
        public sealed class ImageBytesType{
            public static readonly ImageBytesType PNG = new ImageBytesType("png"); // the stream contains png encoded data
            public static readonly ImageBytesType JPG = new ImageBytesType("jpg"); // the stream contains jpg encoded data
            public static readonly ImageBytesType JP2 = new ImageBytesType("jp2"); // the stream contains jp2 encoded data
            public static readonly ImageBytesType CCITT = new ImageBytesType("tif"); // the stream contains ccitt encoded data
            public static readonly ImageBytesType JBIG2 = new ImageBytesType("jbig2"); // the stream contains JBIG2 encoded data
            
            /**
             * the recommended file extension for streams of this type
             */
            private String fileExtension;
            
            /**
             * @param fileExtension the recommended file extension for use with data of this type (for example, if the bytes were just saved to a file, what extension should the file have)
             */
            private ImageBytesType(String fileExtension) {
                this.fileExtension = fileExtension;
            }
            
            /**
             * @return the file extension registered when this type was created
             */
            public String FileExtension {
                get {
                    return fileExtension;
                }
            }
        }

        /**
         * A filter that does nothing, but keeps track of the filter type that was used
         * @since 5.0.4 
         */
        private sealed class TrackingFilter : FilterHandlers.IFilterHandler{
            public PdfName lastFilterName = null;
            
            public byte[] Decode(byte[] b, PdfName filterName, PdfObject decodeParams, PdfDictionary streamDictionary) {
                lastFilterName = filterName;
                return b;
            }
        }

        /** The image dictionary. */
        private PdfDictionary dictionary;
        /** The decoded image bytes (after applying filters), or the raw image bytes if unable to decode */
        private byte[] imageBytes;
        private PdfDictionary colorSpaceDic;

        private int pngColorType = -1;
        private int pngBitDepth;
        private int width;
        private int height;
        private int bpc;
        private byte[] palette;
        private byte[] icc;
        private int stride;
        
        /**
         * Tracks the type of data that is actually stored in the streamBytes member
         */
        private ImageBytesType streamContentType = null;

        virtual public string GetFileType() {
            return streamContentType.FileExtension;
        }

        /**
         * @return the type of image data that is returned by getImageBytes()
         */
        virtual public ImageBytesType GetImageBytesType(){
            return streamContentType;
        }
        
        /**
         * Creates a PdfImage object.
         * @param stream a PRStream
         * @throws IOException
         */
        public PdfImageObject(PRStream stream) : this(stream, PdfReader.GetStreamBytesRaw(stream), null) {
        }
        
        /**
         * Creates a PdfImage object.
         * @param stream a PRStream
         * @param colorSpaceDic a color space dictionary
         * @throws IOException
         */
        public PdfImageObject(PRStream stream, PdfDictionary colorSpaceDic) : this(stream, PdfReader.GetStreamBytesRaw(stream), colorSpaceDic) {
        }

        /**
         * Creats a PdfImage object using an explicitly provided dictionary and image bytes
         * @param dictionary the dictionary for the image
         * @param samples the samples
         * @since 5.0.3
         */
        protected internal PdfImageObject(PdfDictionary dictionary, byte[] samples, PdfDictionary colorSpaceDic)  {
            this.dictionary = dictionary;
            this.colorSpaceDic = colorSpaceDic;
            TrackingFilter trackingFilter = new TrackingFilter();
            IDictionary<PdfName, FilterHandlers.IFilterHandler> handlers = new Dictionary<PdfName, FilterHandlers.IFilterHandler>(FilterHandlers.GetDefaultFilterHandlers());
            handlers[PdfName.JBIG2DECODE] = trackingFilter;
            handlers[PdfName.DCTDECODE] = trackingFilter;
            handlers[PdfName.JPXDECODE] = trackingFilter;

            imageBytes = PdfReader.DecodeBytes(samples, dictionary, handlers);
            
            if (trackingFilter.lastFilterName != null){
                if (PdfName.JBIG2DECODE.Equals(trackingFilter.lastFilterName))
                    streamContentType = ImageBytesType.JBIG2;
                else if (PdfName.DCTDECODE.Equals(trackingFilter.lastFilterName))
                    streamContentType = ImageBytesType.JPG;
                else if (PdfName.JPXDECODE.Equals(trackingFilter.lastFilterName))
                    streamContentType = ImageBytesType.JP2;
            } else {
                DecodeImageBytes();
            }
        }
        
        /**
         * Returns an entry from the image dictionary.
         * @param key a key
         * @return the value
         */
        virtual public PdfObject Get(PdfName key) {
            return dictionary.Get(key);
        }
        
        /**
         * Returns the image dictionary.
         * @return the dictionary
         */
        virtual public PdfDictionary GetDictionary() {
            return dictionary;
        }

        /**
         * Sets state of this object according to the color space 
         * @param colorspace the colorspace to use
         * @param allowIndexed whether indexed color spaces will be resolved (used for recursive call)
         * @throws IOException if there is a problem with reading from the underlying stream  
         */
        private void FindColorspace(PdfObject colorspace, bool allowIndexed) {
            if (colorspace == null && bpc == 1){ // handle imagemasks
                stride = (width*bpc + 7) / 8;
                pngColorType = 0;
            }
            else if (PdfName.DEVICEGRAY.Equals(colorspace)) {
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

        /**
         * decodes the bytes currently captured in the streamBytes and replaces it with an image representation of the bytes
         * (this will either be a png or a tiff, depending on the color depth of the image)
         * @throws IOException
         */
        private void DecodeImageBytes() {
            if (streamContentType != null)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("Decoding.can't.happen.on.this.type.of.stream.(.1.)", streamContentType.FileExtension));
            pngColorType = -1;
            PdfArray decode = dictionary.GetAsArray(PdfName.DECODE);
            width = dictionary.GetAsNumber(PdfName.WIDTH).IntValue;
            height = dictionary.GetAsNumber(PdfName.HEIGHT).IntValue;
            bpc = dictionary.GetAsNumber(PdfName.BITSPERCOMPONENT).IntValue;
            pngBitDepth = bpc;
            PdfObject colorspace = dictionary.GetDirectObject(PdfName.COLORSPACE);
            if (colorspace is PdfName && colorSpaceDic != null){
                PdfObject csLookup = colorSpaceDic.GetDirectObject((PdfName)colorspace);
                if (csLookup != null)
                    colorspace = csLookup;
            }

            palette = null;
            icc = null;
            stride = 0;
            FindColorspace(colorspace, true);
            MemoryStream ms = new MemoryStream();
            if (pngColorType < 0) {
                if (bpc != 8)
                    throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("the.color.depth.1.is.not.supported", bpc));
                if (PdfName.DEVICECMYK.Equals(colorspace)) {
                }
                else if (colorspace is PdfArray) {
                    PdfArray ca = (PdfArray)colorspace;
                    PdfObject tyca = ca.GetDirectObject(0);
                    if (!PdfName.ICCBASED.Equals(tyca))
                        throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("the.color.space.1.is.not.supported", colorspace));
                    PRStream pr = (PRStream)ca.GetDirectObject(1);
                    int n = pr.GetAsNumber(PdfName.N).IntValue;
                    if (n != 4) {
                        throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("N.value.1.is.not.supported", n));
                    }
                    icc = PdfReader.GetStreamBytes(pr);
                }
                else
                    throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("the.color.space.1.is.not.supported", colorspace));
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
                wr.AddField(new TiffWriter.FieldAscii(TIFFConstants.TIFFTAG_SOFTWARE, Version.GetInstance().GetVersion));
                MemoryStream comp = new MemoryStream();
                TiffWriter.CompressLZW(comp, 2, imageBytes, height, 4, stride);
                byte[] buf = comp.ToArray();
                wr.AddField(new TiffWriter.FieldImage(buf));
                wr.AddField(new TiffWriter.FieldLong(TIFFConstants.TIFFTAG_STRIPBYTECOUNTS, buf.Length));
                if (icc != null)
                    wr.AddField(new TiffWriter.FieldUndefined(TIFFConstants.TIFFTAG_ICCPROFILE, icc));
                wr.WriteFile(ms);
                streamContentType = ImageBytesType.CCITT;
                imageBytes = ms.ToArray();
                return;
            } 
            else {
                PngWriter png = new PngWriter(ms);
                if (decode != null){
                    if (pngBitDepth == 1){
                        // if the decode array is 1,0, then we need to invert the image
                        if (decode.GetAsNumber(0).IntValue == 1 && decode.GetAsNumber(1).IntValue == 0){
                            int len = imageBytes.Length;
                            for (int t = 0; t < len; ++t) {
                                imageBytes[t] ^= 0xff;
                            }
                        } else {
                            // if the decode array is 0,1, do nothing.  It's possible that the array could be 0,0 or 1,1 - but that would be silly, so we'll just ignore that case
                        }
                    } else {
                        // todo: add decode transformation for other depths
                    }
                }
                png.WriteHeader(width, height, pngBitDepth, pngColorType);
                if (icc != null)
                    png.WriteIccProfile(icc);
                if (palette != null)
                    png.WritePalette(palette);
                png.WriteData(imageBytes, stride);
                png.WriteEnd();
                streamContentType = ImageBytesType.PNG;
                imageBytes = ms.ToArray();
            }
        }

        /**
         * @return the bytes of the image (the format will be as specified in {@link PdfImageObject#getImageBytesType()}
         * @throws IOException
         * @since 5.0.4
         */
        virtual public byte[] GetImageAsBytes() {
            return imageBytes; 
        }

#if DRAWING
        public System.Drawing.Image GetDrawingImage() {
            byte[] r = GetImageAsBytes();
            if (r == null)
                return null;
            return System.Drawing.Bitmap.FromStream(new MemoryStream(r));
        }
#endif// DRAWING
    }
}
