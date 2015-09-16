using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;

using iTextSharp.text.error_messages;
using iTextSharp.text.api;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.codec;
using iTextSharp.text.pdf.interfaces;
using iTextSharp.text.io;

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

namespace iTextSharp.text {
    /// <summary>
    /// An Image is the representation of a graphic element (JPEG, PNG or GIF)
    /// that has to be inserted into the document
    /// </summary>
    /// <seealso cref="T:iTextSharp.text.Element"/>
    /// <seealso cref="T:iTextSharp.text.Rectangle"/>
    public abstract class Image : Rectangle, IIndentable, ISpaceable, IAccessibleElement, IAlternateDescription {
    
        // static membervariables (concerning the presence of borders)
    
        /// <summary> this is a kind of image Element. </summary>
        public const int DEFAULT = 0;
    
        /// <summary> this is a kind of image Element. </summary>
        public const int RIGHT_ALIGN = 2;
    
        /// <summary> this is a kind of image Element. </summary>
        public const int LEFT_ALIGN = 0;
    
        /// <summary> this is a kind of image Element. </summary>
        public const int MIDDLE_ALIGN = 1;
    
        /// <summary> this is a kind of image Element. </summary>
        public const int TEXTWRAP = 4;
    
        /// <summary> this is a kind of image Element. </summary>
        public const int UNDERLYING = 8;
    
        /// <summary> This represents a coordinate in the transformation matrix. </summary>
        public const int AX = 0;
    
        /// <summary> This represents a coordinate in the transformation matrix. </summary>
        public const int AY = 1;
    
        /// <summary> This represents a coordinate in the transformation matrix. </summary>
        public const int BX = 2;
    
        /// <summary> This represents a coordinate in the transformation matrix. </summary>
        public const int BY = 3;
    
        /// <summary> This represents a coordinate in the transformation matrix. </summary>
        public const int CX = 4;
    
        /// <summary> This represents a coordinate in the transformation matrix. </summary>
        public const int CY = 5;
    
        /// <summary> This represents a coordinate in the transformation matrix. </summary>
        public const int DX = 6;
    
        /// <summary> This represents a coordinate in the transformation matrix. </summary>
        public const int DY = 7;
    
        /** type of image */
        public const int ORIGINAL_NONE = 0;

        /** type of image */
        public const int ORIGINAL_JPEG = 1;

        /** type of image */
        public const int ORIGINAL_PNG = 2;

        /** type of image */
        public const int ORIGINAL_GIF = 3;

        /** type of image */
        public const int ORIGINAL_BMP = 4;

        /** type of image */
        public const int ORIGINAL_TIFF = 5;

        /** type of image */
        public const int ORIGINAL_WMF = 6;

	    /** type of image */
	    public const int ORIGINAL_JPEG2000 = 8;

	    /**
	    * type of image
	    * @since	2.1.5
	    */
	    public const int ORIGINAL_JBIG2 = 9;

        /** Image color inversion */
        protected bool invert = false;
    
        /// <summary> The imagetype. </summary>
        protected int type;
    
        /// <summary> The URL of the image. </summary>
        protected Uri url;
    
        /// <summary> The raw data of the image. </summary>
        protected byte[] rawData;
    
        /// <summary> The template to be treated as an image. </summary>
        protected PdfTemplate[] template = new PdfTemplate[1];
    
        /// <summary> The alignment of the Image. </summary>
        protected int alignment;
    
        /// <summary> Text that can be shown instead of the image. </summary>
        protected string alt;
    
        /// <summary> This is the absolute X-position of the image. </summary>
        protected float absoluteX = float.NaN;
    
        /// <summary> This is the absolute Y-position of the image. </summary>
        protected float absoluteY = float.NaN;
    
        /// <summary> This is the width of the image without rotation. </summary>
        protected float plainWidth;
    
        /// <summary> This is the width of the image without rotation. </summary>
        protected float plainHeight;
    
        /// <summary> This is the scaled width of the image taking rotation into account. </summary>
        protected float scaledWidth;
    
        /// <summary> This is the original height of the image taking rotation into account. </summary>
        protected float scaledHeight;
    
        /**
        * The compression level of the content streams.
        * @since   2.1.3
        */
        protected int compressionLevel = PdfStream.DEFAULT_COMPRESSION;

        /// <summary> This is the rotation of the image. </summary>
        protected float rotationRadians;
    
        /// <summary> this is the colorspace of a jpeg-image. </summary>
        protected int colorspace = -1;

        protected int colortransform = 1;
    
        /// <summary> this is the bits per component of the raw image. It also flags a CCITT image.</summary>
        protected int bpc = 1;
    
        /// <summary> this is the transparency information of the raw image</summary>
        protected int[] transparency;
    
        // for the moment these variables are only used for Images in class Table
        // code contributed by Pelikan Stephan
        /** the indentation to the left. */
        protected float indentationLeft = 0;

        /** the indentation to the right. */
        protected float indentationRight = 0;
        // serial stamping
    
        protected long mySerialId = GetSerialId();
    
        static object serialId = 0L;

        protected PdfName role = PdfName.FIGURE;

        protected Dictionary<PdfName, PdfObject> accessibleAttributes = null;

        protected AccessibleElementId id = new AccessibleElementId();

        /// <summary> Holds value of property dpiX. </summary>
        protected int dpiX = 0;
    
        /// <summary> Holds value of property dpiY. </summary>
        protected int dpiY = 0;
    
        protected bool mask = false;
    
        protected Image imageMask;
    
        /// <summary> Holds value of property interpolation. </summary>
        protected bool interpolation;
    
        /// <summary> if the annotation is not null the image will be clickable. </summary>
        protected Annotation annotation = null;

        /// <summary> ICC Profile attached </summary>
        protected ICC_Profile profile = null;
    
        /** Holds value of property deflated. */
        protected bool deflated = false;

        private PdfDictionary additional = null;

        /** Holds value of property smask. */
        private bool smask;

        /** Holds value of property XYRatio. */
        private float xyRatio = 0;

        /** Holds value of property originalType. */
        protected int originalType = ORIGINAL_NONE;

        /** Holds value of property originalData. */
        protected byte[] originalData;

        /** The spacing before the image. */
        protected float spacingBefore;

        /** The spacing after the image. */
        protected float spacingAfter;

        protected float paddingTop;

        /**
        * Holds value of property widthPercentage.
        */
        private float widthPercentage = 100;

        protected IPdfOCG layer;

        /**
        * Holds value of property initialRotation.
        */
        private float initialRotation;
        
        private PdfIndirectReference directReference;

        // constructors
    
        /// <summary>
        /// Constructs an Image-object, using an url.
        /// </summary>
        /// <param name="url">the URL where the image can be found.</param>
        public Image(Uri url) : base(0, 0) {
            this.url = url;
            this.alignment = DEFAULT;
            rotationRadians = 0;
        }

        /// <summary>
        /// Constructs an Image object duplicate.
        /// </summary>
        /// <param name="image">another Image object.</param>
        public Image(Image image) : base(image) {
            this.type = image.type;
            this.url = image.url;
            this.rawData = image.rawData;
            this.bpc = image.bpc;
            this.template = image.template;
            this.alignment = image.alignment;
            this.alt = image.alt;
            this.absoluteX = image.absoluteX;
            this.absoluteY = image.absoluteY;
            this.plainWidth = image.plainWidth;
            this.plainHeight = image.plainHeight;
            this.scaledWidth = image.scaledWidth;
            this.scaledHeight = image.scaledHeight;
            this.mySerialId = image.mySerialId;

            this.directReference = image.directReference;

            this.rotationRadians = image.rotationRadians;
            this.initialRotation = image.initialRotation;
            this.indentationLeft = image.indentationLeft;
            this.indentationRight = image.indentationRight;
            this.spacingBefore = image.spacingBefore;
            this.spacingAfter = image.spacingAfter;

            this.widthPercentage = image.widthPercentage;
            this.scaleToFitLineWhenOverflow = image.scaleToFitLineWhenOverflow;
            this.scaleToFitHeight = image.scaleToFitHeight;
            this.annotation = image.annotation;
            this.layer = image.layer;
            this.interpolation = image.interpolation;
            this.originalType = image.originalType;
            this.originalData = image.originalData;
            this.deflated = image.deflated;
            this.dpiX = image.dpiX;
            this.dpiY = image.dpiY;
            this.XYRatio = image.XYRatio;

            this.colorspace = image.colorspace;
            this.invert = image.invert;
            this.profile = image.profile;
            this.additional = image.additional;
            this.mask = image.mask;
            this.imageMask = image.imageMask;
            this.smask = image.smask;
            this.transparency = image.transparency;
            this.role = image.role;
            if (image.accessibleAttributes != null)
                this.accessibleAttributes = new Dictionary<PdfName, PdfObject>(image.accessibleAttributes);
            ID = image.ID;
        }
    
        /// <summary>
        /// Gets an instance of an Image.
        /// </summary>
        /// <param name="image">an Image</param>
        /// <returns>an object of type Gif, Jpeg or Png</returns>
        public static Image GetInstance(Image image) {
            if (image == null)
                return null;
            return (Image)image.GetType().GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] {typeof(Image)}, null).Invoke(new object[] {image});
        }

        /// <summary>
        /// Gets an instance of an Image.
        /// </summary>
        /// <param name="url">an URL</param>
        /// <returns>an object of type Gif, Jpeg or Png</returns>
        public static Image GetInstance(Uri url) {
            return GetInstance(url, false);
        }

        /// <summary>
        /// Gets an instance of an Image.
        /// </summary>
        /// <param name="url">an URL</param>
        /// <returns>an object of type Gif, Jpeg or Png</returns>
        public static Image GetInstance(Uri url, bool recoverFromImageError) {
            Stream istr = null;
            RandomAccessSourceFactory randomAccessSourceFactory = new RandomAccessSourceFactory();
            try {
                WebRequest w = WebRequest.Create(url);
                w.Credentials = CredentialCache.DefaultCredentials;
                istr = w.GetResponse().GetResponseStream();
                int c1 = istr.ReadByte();
                int c2 = istr.ReadByte();
                int c3 = istr.ReadByte();
                int c4 = istr.ReadByte();
			    // jbig2
			    int c5 = istr.ReadByte();
			    int c6 = istr.ReadByte();
			    int c7 = istr.ReadByte();
			    int c8 = istr.ReadByte();
                istr.Close();

                istr = null;
                if (c1 == 'G' && c2 == 'I' && c3 == 'F') {
                    GifImage gif = new GifImage(url);
                    Image img = gif.GetImage(1);
                    return img;
                }
                if (c1 == 0xFF && c2 == 0xD8) {
                    return new Jpeg(url);
                }
			    if (c1 == 0x00 && c2 == 0x00 && c3 == 0x00 && c4 == 0x0c) {
				    return new Jpeg2000(url);
			    }
			    if (c1 == 0xff && c2 == 0x4f && c3 == 0xff && c4 == 0x51) {
				    return new Jpeg2000(url);
			    }
                if (c1 == PngImage.PNGID[0] && c2 == PngImage.PNGID[1]
                        && c3 == PngImage.PNGID[2] && c4 == PngImage.PNGID[3]) {
                    Image img = PngImage.GetImage(url);
                    return img;
                }
                if (c1 == 0xD7 && c2 == 0xCD) {
                    Image img = new ImgWMF(url);
                    return img;
                }
                if (c1 == 'B' && c2 == 'M') {
                    Image img = BmpImage.GetImage(url);
                    return img;
                }
                if ((c1 == 'M' && c2 == 'M' && c3 == 0 && c4 == 42)
                        || (c1 == 'I' && c2 == 'I' && c3 == 42 && c4 == 0)) {
                    RandomAccessFileOrArray ra = null;
                    try {
                        if (url.IsFile) {
                            String file = url.LocalPath;
                            ra = new RandomAccessFileOrArray(randomAccessSourceFactory.CreateBestSource(file));
                        } else
                            ra = new RandomAccessFileOrArray(randomAccessSourceFactory.CreateSource(url));
                        Image img = TiffImage.GetTiffImage(ra, 1);
                        img.url = url;
                        return img;
                    } catch (Exception e) {
                        if (recoverFromImageError) {
                            // reruns the getTiffImage() with several error recovering workarounds in place
                            // not guaranteed to work with every TIFF
                            Image img = TiffImage.GetTiffImage(ra, recoverFromImageError, 1);
                            img.url = url;
                            return img;
                        }
                        throw e;
                    }
                    finally {
                        if (ra != null)
                            ra.Close();
                    }

                }
                if ( c1 == 0x97 && c2 == 'J' && c3 == 'B' && c4 == '2' &&
                        c5 == '\r' && c6 == '\n' && c7 == 0x1a && c8 == '\n' ) {
                    RandomAccessFileOrArray ra = null;
                    try {
                        if (url.IsFile) {
                            String file = url.LocalPath;
                            ra = new RandomAccessFileOrArray(randomAccessSourceFactory.CreateBestSource(file));
                        } else
                            ra = new RandomAccessFileOrArray(randomAccessSourceFactory.CreateSource(url));
                        Image img = JBIG2Image.GetJbig2Image(ra, 1);
                        img.url = url;
                        return img;
                    } finally {
                        if (ra != null)
                            ra.Close();
                    }
                }
                throw new IOException(MessageLocalization.GetComposedMessage("unknown.image.format", url.ToString()));
            } finally {
                if (istr != null) {
                    istr.Close();
                }
            }
        }

        public static Image GetInstance(Stream s) {
            byte[] a = StreamUtil.InputStreamToArray(s);
            return GetInstance(a);
        }


        public static Image GetInstance(String filename, bool recoverFromImageError) {
            return GetInstance(Utilities.ToURL(filename), recoverFromImageError);
        }


        public static Image GetInstance(byte[] imgb) {
            return GetInstance(imgb, false);
        }

        
        /// <summary>
        /// Gets an instance of an Image.
        /// </summary>
        /// <param name="img">a byte array</param>
        /// <returns>an object of type Gif, Jpeg or Png</returns>
        public static Image GetInstance(byte[] imgb, bool recoverFromImageError) {
            RandomAccessSourceFactory randomAccessSourceFactory = new RandomAccessSourceFactory();
            int c1 = imgb[0];
            int c2 = imgb[1];
            int c3 = imgb[2];
            int c4 = imgb[3];

            if (c1 == 'G' && c2 == 'I' && c3 == 'F') {
                GifImage gif = new GifImage(imgb);
                return gif.GetImage(1);
            }
            if (c1 == 0xFF && c2 == 0xD8) {
                return new Jpeg(imgb);
            }
			if (c1 == 0x00 && c2 == 0x00 && c3 == 0x00 && c4 == 0x0c) {
				return new Jpeg2000(imgb);
			}
			if (c1 == 0xff && c2 == 0x4f && c3 == 0xff && c4 == 0x51) {
				return new Jpeg2000(imgb);
			}
            if (c1 == PngImage.PNGID[0] && c2 == PngImage.PNGID[1]
                    && c3 == PngImage.PNGID[2] && c4 == PngImage.PNGID[3]) {
                return PngImage.GetImage(imgb);
            }
            if (c1 == 0xD7 && c2 == 0xCD) {
                return new ImgWMF(imgb);
            }
            if (c1 == 'B' && c2 == 'M') {
                return BmpImage.GetImage(imgb);
            }
            if ((c1 == 'M' && c2 == 'M' && c3 == 0 && c4 == 42)
                    || (c1 == 'I' && c2 == 'I' && c3 == 42 && c4 == 0)) {
                RandomAccessFileOrArray ra = null;
                try {
                    ra = new RandomAccessFileOrArray(randomAccessSourceFactory.CreateSource(imgb));
                    Image img = TiffImage.GetTiffImage(ra, 1);
                    if (img.OriginalData == null)
                        img.OriginalData = imgb;

                    return img;
				} catch ( Exception e ) {
                    if ( recoverFromImageError ) {
                        // reruns the getTiffImage() with several error recovering workarounds in place
                        // not guaranteed to work with every TIFF
                        Image img = TiffImage.GetTiffImage(ra, recoverFromImageError, 1);
                        if (img.OriginalData == null)
                            img.OriginalData = imgb;
                        return img;
                    }
                    throw e;
                } finally {
                    if (ra != null)
                        ra.Close();
                }

            }
            if (c1 == 0x97 && c2 == 'J' && c3 == 'B' && c4 == '2') {
                int c5 = imgb[4];
                int c6 = imgb[5];
                int c7 = imgb[6];
                int c8 = imgb[7];
                if (c5 == '\r' && c6 == '\n' && c7 == 0x1a && c8 == '\n') {
                    // a jbig2 file with a file header.  the header is the only way we know here.
                    // embedded jbig2s don't have a header, have to create them by explicit use of Jbig2Image?
                    // nkerr, 2008-12-05  see also the getInstance(URL)
                    RandomAccessFileOrArray ra = null;
                    try {
                        ra = new RandomAccessFileOrArray(randomAccessSourceFactory.CreateSource(imgb));
                        Image img = JBIG2Image.GetJbig2Image(ra, 1);
                        if (img.OriginalData == null)
                            img.OriginalData = imgb;
                        return img;
                    }
                    finally {
                        if (ra != null)
                            ra.Close();
                    }
                }
            }
            throw new IOException(MessageLocalization.GetComposedMessage("the.byte.array.is.not.a.recognized.imageformat"));
        }
    
#if DRAWING
        /// <summary>
        /// Gets an instance of an Image from a System.Drwaing.Image.
        /// </summary>
        /// <param name="image">the System.Drawing.Image to convert</param>
        /// <param name="color">
        /// if different from null the transparency
        /// pixels are replaced by this color
        /// </param>
        /// <param name="forceBW">if true the image is treated as black and white</param>
        /// <returns>an object of type ImgRaw</returns>
        public static Image GetInstance(System.Drawing.Image image, BaseColor color, bool forceBW) {
            System.Drawing.Bitmap bm = (System.Drawing.Bitmap)image;
            int w = bm.Width;
            int h = bm.Height;
            int pxv = 0;
            if (forceBW) {
                int byteWidth = (w / 8) + ((w & 7) != 0 ? 1 : 0);
                byte[] pixelsByte = new byte[byteWidth * h];
            
                int index = 0;
                int transColor = 1;
                if (color != null) {
                    transColor = (color.R + color.G + color.B < 384) ? 0 : 1;
                }
                int[] transparency = null;
                int cbyte = 0x80;
                int wMarker = 0;
                int currByte = 0;
                if (color != null) {
                    for (int j = 0; j < h; j++) {
                        for (int i = 0; i < w; i++) {
                            int alpha = bm.GetPixel(i, j).A;
                            if (alpha < 250) {
                                if (transColor == 1)
                                    currByte |= cbyte;
                            }
                            else {
                                if ((bm.GetPixel(i, j).ToArgb() & 0x888) != 0)
                                    currByte |= cbyte;
                            }
                            cbyte >>= 1;
                            if (cbyte == 0 || wMarker + 1 >= w) {
                                pixelsByte[index++] = (byte)currByte;
                                cbyte = 0x80;
                                currByte = 0;
                            }
                            ++wMarker;
                            if (wMarker >= w)
                                wMarker = 0;
                        }
                    }
                }
                else {
                    for (int j = 0; j < h; j++) {
                        for (int i = 0; i < w; i++) {
                            if (transparency == null) {
                                int alpha = bm.GetPixel(i, j).A;
                                if (alpha == 0) {
                                    transparency = new int[2];
                                    transparency[0] = transparency[1] = ((bm.GetPixel(i, j).ToArgb() & 0x888) != 0) ? 1 : 0;
                                }
                            }
                            if ((bm.GetPixel(i, j).ToArgb() & 0x888) != 0)
                                currByte |= cbyte;
                            cbyte >>= 1;
                            if (cbyte == 0 || wMarker + 1 >= w) {
                                pixelsByte[index++] = (byte)currByte;
                                cbyte = 0x80;
                                currByte = 0;
                            }
                            ++wMarker;
                            if (wMarker >= w)
                                wMarker = 0;
                        }
                    }
                }
                return Image.GetInstance(w, h, 1, 1, pixelsByte, transparency);
            }
            else {
                byte[] pixelsByte = new byte[w * h * 3];
                byte[] smask = null;
            
                int index = 0;
                int red = 255;
                int green = 255;
                int blue = 255;
                if (color != null) {
                    red = color.R;
                    green = color.G;
                    blue = color.B;
                }
                int[] transparency = null;
                if (color != null) {
                    for (int j = 0; j < h; j++) {
                        for (int i = 0; i < w; i++) {
                            int alpha = (bm.GetPixel(i, j).ToArgb() >> 24) & 0xff;
                            if (alpha < 250) {
                                pixelsByte[index++] = (byte) red;
                                pixelsByte[index++] = (byte) green;
                                pixelsByte[index++] = (byte) blue;
                            }
                            else {
                                pxv = bm.GetPixel(i, j).ToArgb();
                                pixelsByte[index++] = (byte) ((pxv >> 16) & 0xff);
                                pixelsByte[index++] = (byte) ((pxv >> 8) & 0xff);
                                pixelsByte[index++] = (byte) ((pxv) & 0xff);
                            }
                        }
                    }
                }
                else {
                    int transparentPixel = 0;
                    smask = new byte[w * h];
                    bool shades = false;
                    int smaskPtr = 0;
                    for (int j = 0; j < h; j++) {
                        for (int i = 0; i < w; i++) {
                            pxv = bm.GetPixel(i, j).ToArgb();
                            byte alpha = smask[smaskPtr++] = (byte) ((pxv >> 24) & 0xff);
                            /* bugfix by Chris Nokleberg */
                            if (!shades) {
                                if (alpha != 0 && alpha != 255) {
                                    shades = true;
                                } else if (transparency == null) {
                                    if (alpha == 0) {
                                        transparentPixel = pxv & 0xffffff;
                                        transparency = new int[6];
                                        transparency[0] = transparency[1] = (transparentPixel >> 16) & 0xff;
                                        transparency[2] = transparency[3] = (transparentPixel >> 8) & 0xff;
                                        transparency[4] = transparency[5] = transparentPixel & 0xff;
                                    }
                                } else if ((pxv & 0xffffff) != transparentPixel) {
                                    shades = true;
                                }
                            }
                            pixelsByte[index++] = (byte) ((pxv >> 16) & 0xff);
                            pixelsByte[index++] = (byte) ((pxv >> 8) & 0xff);
                            pixelsByte[index++] = (byte) (pxv & 0xff);
                        }
                    }
                    if (shades)
                        transparency = null;
                    else
                        smask = null;
                }
                Image img = Image.GetInstance(w, h, 3, 8, pixelsByte, transparency);
                if (smask != null) {
                    Image sm = Image.GetInstance(w, h, 1, 8, smask);
                    sm.MakeMask();
                    img.ImageMask = sm;
                }
                return img;
            }
        }

        /// <summary>
        /// Converts a .NET image to a Native(PNG, JPG, GIF, WMF) image
        /// </summary>
        /// <param name="image"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public static Image GetInstance(System.Drawing.Image image, System.Drawing.Imaging.ImageFormat format) {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, format);
            return GetInstance(ms.ToArray());
        }
    
        /// <summary>
        /// Gets an instance of an Image from a System.Drawing.Image.
        /// </summary>
        /// <param name="image">the System.Drawing.Image to convert</param>
        /// <param name="color">
        /// if different from null the transparency
        /// pixels are replaced by this color
        /// </param>
        /// <returns>an object of type ImgRaw</returns>
        public static Image GetInstance(System.Drawing.Image image, BaseColor color) {
            return Image.GetInstance(image, color, false);
        }
#endif// DRAWING

        /// <summary>
        /// Gets an instance of an Image.
        /// </summary>
        /// <param name="filename">a filename</param>
        /// <returns>an object of type Gif, Jpeg or Png</returns>
        public static Image GetInstance(string filename) {
            return GetInstance(Utilities.ToURL(filename));
        }
    
        /// <summary>
        /// Gets an instance of an Image in raw mode.
        /// </summary>
        /// <param name="width">the width of the image in pixels</param>
        /// <param name="height">the height of the image in pixels</param>
        /// <param name="components">1,3 or 4 for GrayScale, RGB and CMYK</param>
        /// <param name="bpc">bits per component</param>
        /// <param name="data">the image data</param>
        /// <returns>an object of type ImgRaw</returns>
        public static Image GetInstance(int width, int height, int components, int bpc, byte[] data) {
            return Image.GetInstance(width, height, components, bpc, data, null);
        }

        /**
        * Creates a JBIG2 Image.
        * @param   width   the width of the image
        * @param   height  the height of the image
        * @param   data    the raw image data
        * @param   globals JBIG2 globals
        * @since   2.1.5
        */
        public static Image GetInstance(int width, int height, byte[] data, byte[] globals) {
            return new ImgJBIG2(width, height, data, globals);
        }
    
        /**
        * Reuses an existing image.
        * @param ref the reference to the image dictionary
        * @throws BadElementException on error
        * @return the image
        */    
        public static Image GetInstance(PRIndirectReference iref) {
            PdfDictionary dic = (PdfDictionary)PdfReader.GetPdfObjectRelease(iref);
            int width = ((PdfNumber)PdfReader.GetPdfObjectRelease(dic.Get(PdfName.WIDTH))).IntValue;
            int height = ((PdfNumber)PdfReader.GetPdfObjectRelease(dic.Get(PdfName.HEIGHT))).IntValue;
            Image imask = null;
            PdfObject obj = dic.Get(PdfName.SMASK);
            if (obj != null && obj.IsIndirect()) {
                imask = GetInstance((PRIndirectReference)obj);
            }
            else {
                obj = dic.Get(PdfName.MASK);
                if (obj != null && obj.IsIndirect()) {
                    PdfObject obj2 = PdfReader.GetPdfObjectRelease(obj);
                    if (obj2 is PdfDictionary)
                        imask = GetInstance((PRIndirectReference)obj);
                }
            }
            Image img = new ImgRaw(width, height, 1, 1, null);
            img.imageMask = imask;
            img.directReference = iref;
            return img;
        }
        
        /// <summary>
        /// Gets an instance of an Image in raw mode.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public static Image GetInstance(PdfTemplate template) {
            return new ImgTemplate(template);
        }
    
        /// <summary>
        /// Gets an instance of an Image in raw mode.
        /// </summary>
        /// <param name="width">the width of the image in pixels</param>
        /// <param name="height">the height of the image in pixels</param>
        /// <param name="reverseBits"></param>
        /// <param name="typeCCITT"></param>
        /// <param name="parameters"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Image GetInstance(int width, int height, bool reverseBits, int typeCCITT, int parameters, byte[] data) {
            return Image.GetInstance(width, height, reverseBits, typeCCITT, parameters, data, null);
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="reverseBits"></param>
        /// <param name="typeCCITT"></param>
        /// <param name="parameters"></param>
        /// <param name="data"></param>
        /// <param name="transparency"></param>
        /// <returns></returns>
        public static Image GetInstance(int width, int height, bool reverseBits, int typeCCITT, int parameters, byte[] data, int[] transparency) {
            if (transparency != null && transparency.Length != 2)
                throw new BadElementException(MessageLocalization.GetComposedMessage("transparency.length.must.be.equal.to.2.with.ccitt.images"));
            Image img = new ImgCCITT(width, height, reverseBits, typeCCITT, parameters, data);
            img.transparency = transparency;
            return img;
        }

        /// <summary>
        /// Gets an instance of an Image in raw mode.
        /// </summary>
        /// <param name="width">the width of the image in pixels</param>
        /// <param name="height">the height of the image in pixels</param>
        /// <param name="components">1,3 or 4 for GrayScale, RGB and CMYK</param>
        /// <param name="bpc">bits per component</param>
        /// <param name="data">the image data</param>
        /// <param name="transparency">
        /// transparency information in the Mask format of the
        /// image dictionary
        /// </param>
        /// <returns>an object of type ImgRaw</returns>
        public static Image GetInstance(int width, int height, int components, int bpc, byte[] data, int[] transparency) {
            if (transparency != null && transparency.Length != components * 2)
                throw new BadElementException(MessageLocalization.GetComposedMessage("transparency.length.must.be.equal.to.componentes.2"));
            if (components == 1 && bpc == 1) {
                byte[] g4 = CCITTG4Encoder.Compress(data, width, height);
                return Image.GetInstance(width, height, false, Element.CCITTG4, Element.CCITT_BLACKIS1, g4, transparency);
            }
            Image img = new ImgRaw(width, height, components, bpc, data);
            img.transparency = transparency;
            return img;
        }
    
        // methods to set information
    
        /// <summary>
        /// Sets the absolute position of the Image.
        /// </summary>
        /// <param name="absoluteX"></param>
        /// <param name="absoluteY"></param>
        virtual public void SetAbsolutePosition(float absoluteX, float absoluteY) {
            this.absoluteX = absoluteX;
            this.absoluteY = absoluteY;
        }
    
        /// <summary>
        /// Scale the image to the dimensions of the rectangle
        /// </summary>
        /// <param name="rectangle">dimensions to scale the Image</param>
        virtual public void ScaleAbsolute(Rectangle rectangle) {
            ScaleAbsolute(rectangle.Width, rectangle.Height);
        }

        /// <summary>
        /// Scale the image to an absolute width and an absolute height.
        /// </summary>
        /// <param name="newWidth">the new width</param>
        /// <param name="newHeight">the new height</param>
        virtual public void ScaleAbsolute(float newWidth, float newHeight) {
            plainWidth = newWidth;
            plainHeight = newHeight;
            float[] matrix = this.GetMatrix();
            scaledWidth = matrix[DX] - matrix[CX];
            scaledHeight = matrix[DY] - matrix[CY];
            WidthPercentage = 0;
        }
    
        /// <summary>
        /// Scale the image to an absolute width.
        /// </summary>
        /// <param name="newWidth">the new width</param>
        virtual public void ScaleAbsoluteWidth(float newWidth) {
            plainWidth = newWidth;
            float[] matrix = this.GetMatrix();
            scaledWidth = matrix[DX] - matrix[CX];
            scaledHeight = matrix[DY] - matrix[CY];
            WidthPercentage = 0;
        }
    
        /// <summary>
        /// Scale the image to an absolute height.
        /// </summary>
        /// <param name="newHeight">the new height</param>
        virtual public void ScaleAbsoluteHeight(float newHeight) {
            plainHeight = newHeight;
            float[] matrix = GetMatrix();
            scaledWidth = matrix[DX] - matrix[CX];
            scaledHeight = matrix[DY] - matrix[CY];
            WidthPercentage = 0;
        }
    
        /// <summary>
        /// Scale the image to a certain percentage.
        /// </summary>
        /// <param name="percent">the scaling percentage</param>
        virtual public void ScalePercent(float percent) {
            ScalePercent(percent, percent);
        }
    
        /// <summary>
        /// Scale the width and height of an image to a certain percentage.
        /// </summary>
        /// <param name="percentX">the scaling percentage of the width</param>
        /// <param name="percentY">the scaling percentage of the height</param>
        virtual public void ScalePercent(float percentX, float percentY) {
            plainWidth = (this.Width * percentX) / 100f;
            plainHeight = (this.Height * percentY) / 100f;
            float[] matrix = GetMatrix();
            scaledWidth = matrix[DX] - matrix[CX];
            scaledHeight = matrix[DY] - matrix[CY];
            WidthPercentage = 0;
        }
    
        /// <summary>
        /// Scales the images to the dimensions of the rectangle.
        /// </summary>
        /// <param name="rectangle">the dimensions to fit</param>
        virtual public void ScaleToFit(Rectangle rectangle) {
            ScaleToFit(rectangle.Width, rectangle.Height);
        }

        /// <summary>
        /// Scales the image so that it fits a certain width and height.
        /// </summary>
        /// <param name="fitWidth">the width to fit</param>
        /// <param name="fitHeight">the height to fit</param>
        virtual public void ScaleToFit(float fitWidth, float fitHeight) {
            ScalePercent(100);
            float percentX = (fitWidth * 100) / this.ScaledWidth;
            float percentY = (fitHeight * 100) / this.ScaledHeight;
            ScalePercent(percentX < percentY ? percentX : percentY);
            WidthPercentage = 0;
        }
    
        /**
        * Gets the current image rotation in radians.
        * @return the current image rotation in radians
        */
        virtual public float GetImageRotation() {
            float rot = (float) ((rotationRadians - initialRotation) % (2.0 * Math.PI));
            if (rot < 0) {
                rot += (float)(2.0 * Math.PI);
            }
            return rot;
        }

        /// <summary>
        /// Sets the rotation of the image in radians.
        /// </summary>
        /// <param name="r">rotation in radians</param>
        public new float Rotation {
            set
            {
                base.Rotation = (int)value;
                double d=Math.PI;                  //__IDS__
                rotationRadians = (float) ((value + initialRotation) % (2.0 * d)); //__IDS__
                if (rotationRadians < 0) {
                    rotationRadians += (float)(2.0 * d);           //__IDS__
                }
                float[] matrix = GetMatrix();
                scaledWidth = matrix[DX] - matrix[CX];
                scaledHeight = matrix[DY] - matrix[CY];
            }
        }

        /// <summary>
        /// Sets the rotation of the image in degrees.
        /// </summary>
        /// <param name="deg">rotation in degrees</param>
        virtual public float RotationDegrees {
            set {
                Rotation = (value / 180 * (float)Math.PI); //__IDS__
            }
        }
    
        /// <summary>
        /// Get/set the annotation.
        /// </summary>
        /// <value>the Annotation</value>
        virtual public Annotation Annotation {
            get {
                return annotation;
            }

            set {
                this.annotation = value;
            }
        }
    
        // methods to retrieve information
    
        /// <summary>
        /// Gets the bpc for the image.
        /// </summary>
        /// <remarks>
        /// this only makes sense for Images of the type RawImage.
        /// </remarks>
        /// <value>a bpc value</value>
        virtual public int Bpc {
            get {
                return bpc;
            }
        }
    
        /// <summary>
        /// Gets the raw data for the image.
        /// </summary>
        /// <remarks>
        /// this only makes sense for Images of the type RawImage.
        /// </remarks>
        /// <value>the raw data</value>
        virtual public byte[] RawData {
            get {
                return rawData;
            }
        }
    
        /// <summary>
        /// Get/set the template to be used as an image.
        /// </summary>
        /// <remarks>
        /// this only makes sense for Images of the type ImgTemplate.
        /// </remarks>
        /// <value>the template</value>
        virtual public PdfTemplate TemplateData {
            get {
                return template[0];
            }

            set {
                this.template[0] = value;
            }
        }
    
        /// <summary>
        /// Checks if the Images has to be added at an absolute position.
        /// </summary>
        /// <returns>a bool</returns>
        virtual public bool HasAbsolutePosition() {
            return !float.IsNaN(absoluteY);
        }
    
        /// <summary>
        /// Checks if the Images has to be added at an absolute X position.
        /// </summary>
        /// <returns>a bool</returns>
        virtual public bool HasAbsoluteX() {
            return !float.IsNaN(absoluteX);
        }
    
        /// <summary>
        /// Returns the absolute X position.
        /// </summary>
        /// <value>a position</value>
        virtual public float AbsoluteX {
            get {
                return absoluteX;
            }
        }
    
        /// <summary>
        /// Returns the absolute Y position.
        /// </summary>
        /// <value>a position</value>
        virtual public float AbsoluteY {
            get {
                return absoluteY;
            }
        }
    
        /// <summary>
        /// Returns the type.
        /// </summary>
        /// <value>a type</value>
        public override int Type {
            get {
                return type;
            }
        }
    
        /**
        * @see com.lowagie.text.Element#isNestable()
        * @since   iText 2.0.8
        */
        public override bool IsNestable() {
            return true;
        }

        /// <summary>
        /// Returns true if the image is a Jpeg-object.
        /// </summary>
        /// <returns>a bool</returns>
        virtual public bool IsJpeg() {
            return type == Element.JPEG;
        }
    
        /// <summary>
        /// Returns true if the image is a ImgRaw-object.
        /// </summary>
        /// <returns>a bool</returns>
        virtual public bool IsImgRaw() {
            return type == Element.IMGRAW;
        }

        /// <summary>
        /// Returns true if the image is an ImgTemplate-object.
        /// </summary>
        /// <returns>a bool</returns>
        virtual public bool IsImgTemplate() {
            return type == Element.IMGTEMPLATE;
        }
    
        /// <summary>
        /// Gets the string-representation of the reference to the image.
        /// </summary>
        /// <value>a string</value>
        virtual public Uri Url {
            get {
                return url;
            }
            set {
                url = value;
            }
        }
    
        /// <summary>
        /// Get/set the alignment for the image.
        /// </summary>
        /// <value>a value</value>
        virtual public int Alignment {
            get {
                return alignment;
            }

            set {
                this.alignment = value;
            }
        }
    
        /// <summary>
        /// Get/set the alternative text for the image.
        /// </summary>
        /// <value>a string</value>
        virtual public string Alt {
            get {
                return alt;
            }

            set {
                this.alt = value;
                SetAccessibleAttribute(PdfName.ALT, new PdfString(alt));
            }
        }
    
        /// <summary>
        /// Gets the scaled width of the image.
        /// </summary>
        /// <value>a value</value>
        virtual public float ScaledWidth {
            get {
                return scaledWidth;
            }
        }
    
        /// <summary>
        /// Gets the scaled height of the image.
        /// </summary>
        /// <value>a value</value>
        virtual public float ScaledHeight {
            get {
                return scaledHeight;
            }
        }
    
        /// <summary>
        /// Gets the colorspace for the image.
        /// </summary>
        /// <remarks>
        /// this only makes sense for Images of the type Jpeg.
        /// </remarks>
        /// <value>a colorspace value</value>
        virtual public int Colorspace {
            get {
                return colorspace;
            }
        }

        virtual public int ColorTransform
        {
            get { return colortransform; }
            set { colortransform = value; }
        }

        /// <summary>
        /// Returns the transformation matrix of the image.
        /// </summary>
        /// <value>an array [AX, AY, BX, BY, CX, CY, DX, DY]</value>
        virtual public float[] GetMatrix()
        {
            return GetMatrix(1);
        }

        /**
    	 * Returns the transformation matrix of the image.
    	 *
    	 * @return an array [AX, AY, BX, BY, CX, CY, DX, DY]
    	 */
        virtual public float[] GetMatrix(float scalePercentage)
        {
            float[] matrix = new float[8];
            float cosX = (float)Math.Cos(rotationRadians);
            float sinX = (float)Math.Sin(rotationRadians);
            matrix[AX] = plainWidth * cosX * scalePercentage;
            matrix[AY] = plainWidth * sinX * scalePercentage;
            matrix[BX] = (-plainHeight) * sinX * scalePercentage;
            matrix[BY] = plainHeight * cosX * scalePercentage;
            if (rotationRadians < Math.PI / 2f)
            {
                matrix[CX] = matrix[BX];
                matrix[CY] = 0;
                matrix[DX] = matrix[AX];
                matrix[DY] = matrix[AY] + matrix[BY];
            }
            else if (rotationRadians < Math.PI)
            {
                matrix[CX] = matrix[AX] + matrix[BX];
                matrix[CY] = matrix[BY];
                matrix[DX] = 0;
                matrix[DY] = matrix[AY];
            }
            else if (rotationRadians < Math.PI * 1.5f)
            {
                matrix[CX] = matrix[AX];
                matrix[CY] = matrix[AY] + matrix[BY];
                matrix[DX] = matrix[BX];
                matrix[DY] = 0;
            }
            else
            {
                matrix[CX] = 0;
                matrix[CY] = matrix[AY];
                matrix[DX] = matrix[AX] + matrix[BX];
                matrix[DY] = matrix[BY];
            }
            return matrix;
        }

        /// <summary>
        /// Returns the transparency.
        /// </summary>
        /// <value>the transparency</value>
        virtual public int[] Transparency {
            get {
                return transparency;
            }
            set {
                transparency = value;
            }
        }
    
        /// <summary>
        /// Gets the plain width of the image.
        /// </summary>
        /// <value>a value</value>
        virtual public float PlainWidth {
            get {
                return plainWidth;
            }
        }
    
        /// <summary>
        /// Gets the plain height of the image.
        /// </summary>
        /// <value>a value</value>
        virtual public float PlainHeight {
            get {
                return plainHeight;
            }
        }
    
        /// <summary>
        /// generates new serial id
        /// </summary>
        static protected long GetSerialId() {
            lock (serialId) {
                serialId = (long)serialId + 1L;
                return (long)serialId;
            }
        }
    
        /// <summary>
        /// returns serial id for this object
        /// </summary>
        virtual public long MySerialId {
            get {
                return mySerialId;
            }
        }
    
        /// <summary>
        /// Gets the dots-per-inch in the X direction. Returns 0 if not available.
        /// </summary>
        /// <value>the dots-per-inch in the X direction</value>
        virtual public int DpiX {
            get {
                return dpiX;
            }
        }
    
        /// <summary>
        /// Gets the dots-per-inch in the Y direction. Returns 0 if not available.
        /// </summary>
        /// <value>the dots-per-inch in the Y direction</value>
        virtual public int DpiY {
            get {
                return dpiY;
            }
        }
    
        /**
        * Sets the dots per inch value
        * 
        * @param dpiX
        *            dpi for x coordinates
        * @param dpiY
        *            dpi for y coordinates
        */
        virtual public void SetDpi(int dpiX, int dpiY) {
            this.dpiX = dpiX;
            this.dpiY = dpiY;
        }

        /// <summary>
        /// Returns true if this Image has the
        /// requisites to be a mask.
        /// </summary>
        /// <returns>true if this Image can be a mask</returns>
        virtual public bool IsMaskCandidate() {
            if (type == Element.IMGRAW) {
                if (bpc > 0xff)
                    return true;
            }
            return colorspace == 1;
        }
    
        /// <summary>
        /// Make this Image a mask.
        /// </summary>
        virtual public void MakeMask() {
            if (!IsMaskCandidate())
                throw new DocumentException(MessageLocalization.GetComposedMessage("this.image.can.not.be.an.image.mask"));
            mask = true;
        }
    
        /// <summary>
        /// Get/set the explicit masking.
        /// </summary>
        /// <value>the explicit masking</value>
        virtual public Image ImageMask {
            get {
                return imageMask;
            }

            set {
                if (this.mask)
                    throw new DocumentException(MessageLocalization.GetComposedMessage("an.image.mask.cannot.contain.another.image.mask"));
                if (!value.mask)
                    throw new DocumentException(MessageLocalization.GetComposedMessage("the.image.mask.is.not.a.mask.did.you.do.makemask"));
                imageMask = value;
                smask = (value.bpc > 1 && value.bpc <= 8);
            }
        }
    
        /// <summary>
        /// Returns true if this Image is a mask.
        /// </summary>
        /// <returns>true if this Image is a mask</returns>
        virtual public bool IsMask() {
            return mask;
        }
    
        /// <summary>
        /// Inverts the meaning of the bits of a mask.
        /// </summary>
        /// <value>true to invert the meaning of the bits of a mask</value>
        virtual public bool Inverted {
            set {
                this.invert = value;
            }
            get {
                return this.invert;
            }
        }
    
        /// <summary>
        /// Sets the image interpolation. Image interpolation attempts to
        /// produce a smooth transition between adjacent sample values.
        /// </summary>
        /// <value>New value of property interpolation.</value>
        virtual public bool Interpolation {
            set {
                this.interpolation = value;
            }
            get {
                return this.interpolation;
            }
        }
    
        /** Tags this image with an ICC profile.
         * @param profile the profile
         */    
        virtual public ICC_Profile TagICC {
            get {
                return profile;
            }
            set {
                this.profile = value;
            }
        }
    
        /** Checks is the image has an ICC profile.
         * @return the ICC profile or null
         */    
        virtual public bool HasICCProfile() {
            return (this.profile != null);
        }

        virtual public bool Deflated {
            get {
                return deflated;
            }
            set {
                deflated = value;
            }
        }

        virtual public PdfDictionary Additional {
            get {
                return additional;
            }
            set {
                additional = value;
            }
        }

        virtual public bool Smask {
            get {
                return smask;
            }
            set {
                smask = value;
            }
        }

        virtual public float XYRatio {
            get {
                return xyRatio;
            }
            set {
                xyRatio = value;
            }
        }

        virtual public float IndentationLeft {
            get {
                return indentationLeft;
            }
            set {
                indentationLeft = value;
            }
        }

        virtual public float IndentationRight {
            get {
                return indentationRight;
            }
            set {
                indentationRight = value;
            }
        }

        virtual public int OriginalType {
            get {
                return originalType;
            }
            set {
                originalType = value;
            }
        }

        virtual public byte[] OriginalData {
            get {
                return originalData;
            }
            set {
                originalData = value;
            }
        }

        virtual public float SpacingBefore {
            get {
                return spacingBefore;
            }
            set {
                spacingBefore = value;
            }
        }

        virtual public float SpacingAfter {
            get {
                return spacingAfter;
            }
            set {
                spacingAfter = value;
            }
        }

        virtual public float PaddingTop
        {
            get { return paddingTop; }
            set { paddingTop = value; }
        }

        virtual public float WidthPercentage {
            get {
                return widthPercentage;
            }
            set {
                widthPercentage = value;
            }
        }

        // scaling the image to the available width (or not)

        /**
         * Indicates if the image should be scaled to fit the line
         * when the image exceeds the available width.
         * @since iText 5.0.6
         */
        protected internal bool scaleToFitLineWhenOverflow;

        virtual public bool ScaleToFitLineWhenOverflow {
            get { return scaleToFitLineWhenOverflow; }
            set { scaleToFitLineWhenOverflow = value; }
        }

	    // scaling the image to the available height (or not)

	    /**
	     * Indicates if the image should be scaled to fit
	     * when the image exceeds the available height.
	     * @since iText 5.4.2
	     */
	    protected bool scaleToFitHeight = true;

	    /**
	     * Gets and sets the value of scaleToFitHeight.
	     * @return true if the image size has to scale to the available height
	     * @since iText 5.4.2
	     */
	    virtual public bool ScaleToFitHeight {
            get { return scaleToFitHeight; }
            set { scaleToFitHeight = value; }
	    }

        virtual public IPdfOCG Layer {
            get {
                return layer;
            }
            set {
                layer = value;
            }
        }

        private PdfObject SimplifyColorspace(PdfArray obj) {
            if (obj == null)
                return null;
            PdfObject first = obj.GetAsName(0);
            if (PdfName.CALGRAY.Equals(first))
                return PdfName.DEVICEGRAY;
            else if (PdfName.CALRGB.Equals(first))
                return PdfName.DEVICERGB;
            else
                return obj;
        }

        /**
        * Replaces CalRGB and CalGray colorspaces with DeviceRGB and DeviceGray.
        */    
        virtual public void SimplifyColorspace() {
            if (additional == null)
                return;
            PdfArray value = additional.GetAsArray(PdfName.COLORSPACE);
            if (value == null)
                return;
            PdfObject cs = SimplifyColorspace(value);
            PdfObject newValue;
            if (cs.IsName())
                newValue = cs;
            else {
                newValue = value;
                PdfName first = value.GetAsName(0);
                if (PdfName.INDEXED.Equals(first)) {
                    if (value.Size >= 2) {
                        PdfArray second = value.GetAsArray(1);
                        if (second != null) {
                            value[1] = SimplifyColorspace(second);
                        }
                    }
                }
            }
            additional.Put(PdfName.COLORSPACE, newValue);
        }
        
        /**
        * Some image formats, like TIFF may present the images rotated that have
        * to be compensated.
        */
        virtual public float InitialRotation {
            get {
                return initialRotation;
            }
            set {
                float old_rot = rotationRadians - this.initialRotation;
                this.initialRotation = value;
                Rotation = old_rot;
            }
        }

        virtual public PdfIndirectReference DirectReference {
            set {
                directReference = value;
            }
            get {
                return directReference;
            }
        }

        /**
        * Sets the compression level to be used if the image is written as a compressed stream.
        * @param compressionLevel a value between 0 (best speed) and 9 (best compression)
        * @since   2.1.3
        */
        virtual public int CompressionLevel {
            set {
                if (value < PdfStream.NO_COMPRESSION || value > PdfStream.BEST_COMPRESSION)
                    compressionLevel = PdfStream.DEFAULT_COMPRESSION;
                else
                    compressionLevel = value;
            }
            get {
                return compressionLevel;
            }
        }

        virtual public PdfObject GetAccessibleAttribute(PdfName key) {
            if (accessibleAttributes != null) {
                PdfObject value;
                accessibleAttributes.TryGetValue(key, out value);
                return value;
            } else
                return null;
        }

        virtual public void SetAccessibleAttribute(PdfName key, PdfObject value) {
            if (accessibleAttributes == null)
                accessibleAttributes = new Dictionary<PdfName, PdfObject>();
            accessibleAttributes[key] = value;
        }

       

        virtual public Dictionary<PdfName, PdfObject> GetAccessibleAttributes() {
            return accessibleAttributes;
        }

        virtual public PdfName Role {
            get { return role; }
            set { this.role = value; }
        }
        
        virtual public AccessibleElementId ID {
            get
            {
                if (id == null)
                    id = new AccessibleElementId();
                return id;
            }
            set { id = value; }
        }

        public virtual bool IsInline {
            get { return true; }
        }
    }
}
