using System;
using System.IO;
using System.Net;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.error_messages;
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
    /**
    * <CODE>PdfImage</CODE> is a <CODE>PdfStream</CODE> containing an image-<CODE>Dictionary</CODE> and -stream.
    */

    public class PdfImage : PdfStream {
        
        internal const int TRANSFERSIZE = 4096;
        // membervariables
        
        /** This is the <CODE>PdfName</CODE> of the image. */
        protected PdfName name = null;

        protected Image image = null;
        
        // constructor
        
        /**
        * Constructs a <CODE>PdfImage</CODE>-object.
        *
        * @param image the <CODE>Image</CODE>-object
        * @param name the <CODE>PdfName</CODE> for this image
        * @throws BadPdfFormatException on error
        */
        
        public PdfImage(Image image, String name, PdfIndirectReference maskRef) : base() {
            this.image = image;
            if (name == null) 
        	    GenerateImgResName(image);
            else
                this.name = new PdfName(name);
            Put(PdfName.TYPE, PdfName.XOBJECT);
            Put(PdfName.SUBTYPE, PdfName.IMAGE);
            Put(PdfName.WIDTH, new PdfNumber(image.Width));
            Put(PdfName.HEIGHT, new PdfNumber(image.Height));
            if (image.Layer != null)
                Put(PdfName.OC, image.Layer.Ref);
            if (image.IsMask() && (image.Bpc == 1 || image.Bpc > 0xff))
                Put(PdfName.IMAGEMASK, PdfBoolean.PDFTRUE);
            if (maskRef != null) {
                if (image.Smask)
                    Put(PdfName.SMASK, maskRef);
                else
                    Put(PdfName.MASK, maskRef);
            }
            if (image.IsMask() && image.Inverted)
                Put(PdfName.DECODE, new PdfLiteral("[1 0]"));
            if (image.Interpolation)
                Put(PdfName.INTERPOLATE, PdfBoolean.PDFTRUE);
            Stream isp = null;
            try {
        	    // deal with transparency
                int[] transparency = image.Transparency;
                if(transparency != null && !image.IsMask() && maskRef == null) {
                    StringBuilder s = new StringBuilder("[");
                    for(int k = 0; k < transparency.Length; ++k)
                        s.Append(transparency[k]).Append(' ');
                    s.Append(']');
                    Put(PdfName.MASK, new PdfLiteral(s.ToString()));
                }
                // Raw Image data
                if (image.IsImgRaw()) {
                    // will also have the CCITT parameters
                    int colorspace = image.Colorspace;
                    bytes = image.RawData;
                    Put(PdfName.LENGTH, new PdfNumber(bytes.Length));
                    int bpc = image.Bpc;
                    if (bpc > 0xff) {
                        if (!image.IsMask())
                            Put(PdfName.COLORSPACE, PdfName.DEVICEGRAY);
                        Put(PdfName.BITSPERCOMPONENT, new PdfNumber(1));
                        Put(PdfName.FILTER, PdfName.CCITTFAXDECODE);
                        int k = bpc - Image.CCITTG3_1D;
                        PdfDictionary decodeparms = new PdfDictionary();
                        if (k != 0)
                            decodeparms.Put(PdfName.K, new PdfNumber(k));
                        if ((colorspace & Image.CCITT_BLACKIS1) != 0)
                            decodeparms.Put(PdfName.BLACKIS1, PdfBoolean.PDFTRUE);
                        if ((colorspace & Image.CCITT_ENCODEDBYTEALIGN) != 0)
                            decodeparms.Put(PdfName.ENCODEDBYTEALIGN, PdfBoolean.PDFTRUE);
                        if ((colorspace & Image.CCITT_ENDOFLINE) != 0)
                            decodeparms.Put(PdfName.ENDOFLINE, PdfBoolean.PDFTRUE);
                        if ((colorspace & Image.CCITT_ENDOFBLOCK) != 0)
                            decodeparms.Put(PdfName.ENDOFBLOCK, PdfBoolean.PDFFALSE);
                        decodeparms.Put(PdfName.COLUMNS, new PdfNumber(image.Width));
                        decodeparms.Put(PdfName.ROWS, new PdfNumber(image.Height));
                        Put(PdfName.DECODEPARMS, decodeparms);
                    }
                    else {
                        switch (colorspace) {
                            case 1:
                                Put(PdfName.COLORSPACE, PdfName.DEVICEGRAY);
                                if (image.Inverted)
                                    Put(PdfName.DECODE, new PdfLiteral("[1 0]"));
                                break;
                            case 3:
                                Put(PdfName.COLORSPACE, PdfName.DEVICERGB);
                                if (image.Inverted)
                                    Put(PdfName.DECODE, new PdfLiteral("[1 0 1 0 1 0]"));
                                break;
                            case 4:
                            default:
                                Put(PdfName.COLORSPACE, PdfName.DEVICECMYK);
                                if (image.Inverted)
                                    Put(PdfName.DECODE, new PdfLiteral("[1 0 1 0 1 0 1 0]"));
                                break;
                        }
                        PdfDictionary additional = image.Additional;
                        if (additional != null)
                            Merge(additional);
                        if (image.IsMask() && (image.Bpc == 1 || image.Bpc > 8))
                            Remove(PdfName.COLORSPACE);
                        Put(PdfName.BITSPERCOMPONENT, new PdfNumber(image.Bpc));
                        if (image.Deflated)
                            Put(PdfName.FILTER, PdfName.FLATEDECODE);
                        else {
                            FlateCompress(image.CompressionLevel);
                        }
                    }
                    return;
                }
                
                // GIF, JPEG or PNG
                String errorID;
                if (image.RawData == null){
                    WebRequest wr = WebRequest.Create(image.Url);
                    wr.Credentials = CredentialCache.DefaultCredentials;
                    isp = wr.GetResponse().GetResponseStream();
                    errorID = image.Url.ToString();
                }
                else{
                    isp = new MemoryStream(image.RawData);
                    errorID = "Byte array";
                }
                switch (image.Type) {
                    case Image.JPEG:
                        Put(PdfName.FILTER, PdfName.DCTDECODE);
                        if(image.ColorTransform == 0) {
                            PdfDictionary decodeparms = new PdfDictionary();
                            decodeparms.Put(PdfName.COLORTRANSFORM, new PdfNumber(0));
                            Put(PdfName.DECODEPARMS, decodeparms);
                        }
                        switch (image.Colorspace) {
                            case 1:
                                Put(PdfName.COLORSPACE, PdfName.DEVICEGRAY);
                                break;
                            case 3:
                                Put(PdfName.COLORSPACE, PdfName.DEVICERGB);
                                break;
                            default:
                                Put(PdfName.COLORSPACE, PdfName.DEVICECMYK);
                                if (image.Inverted) {
                                    Put(PdfName.DECODE, new PdfLiteral("[1 0 1 0 1 0 1 0]"));
                                }
                                break;
                        }
                        Put(PdfName.BITSPERCOMPONENT, new PdfNumber(8));
                        if (image.RawData != null){
                            bytes = image.RawData;
                            Put(PdfName.LENGTH, new PdfNumber(bytes.Length));
                            return;
                        }
                        streamBytes = new MemoryStream();
                        TransferBytes(isp, streamBytes, -1);
                        break;
                    case Image.JPEG2000:
                        Put(PdfName.FILTER, PdfName.JPXDECODE);
                        if (image.Colorspace > 0) {
                            switch (image.Colorspace) {
                                case 1:
                                    Put(PdfName.COLORSPACE, PdfName.DEVICEGRAY);
                                    break;
                                case 3:
                                    Put(PdfName.COLORSPACE, PdfName.DEVICERGB);
                                    break;
                                default:
                                    Put(PdfName.COLORSPACE, PdfName.DEVICECMYK);
                                    break;
                            }
                            Put(PdfName.BITSPERCOMPONENT, new PdfNumber(image.Bpc));
                        }
                        if (image.RawData != null){
                            bytes = image.RawData;
                            Put(PdfName.LENGTH, new PdfNumber(bytes.Length));
                            return;
                        }
                        streamBytes = new MemoryStream();
                        TransferBytes(isp, streamBytes, -1);
                        break;
                    case Image.JBIG2:
                        Put(PdfName.FILTER, PdfName.JBIG2DECODE);
                        Put(PdfName.COLORSPACE, PdfName.DEVICEGRAY);
                        Put(PdfName.BITSPERCOMPONENT, new PdfNumber(1));
                        if (image.RawData != null){
                            bytes = image.RawData;
                            Put(PdfName.LENGTH, new PdfNumber(bytes.Length));
                            return;
                        }
                        streamBytes = new MemoryStream();
                        TransferBytes(isp, streamBytes, -1);
                        break;
                    default:
                        throw new IOException(MessageLocalization.GetComposedMessage("1.is.an.unknown.image.format", errorID));
                }
                if (image.CompressionLevel > NO_COMPRESSION)
                    FlateCompress(image.CompressionLevel);
                Put(PdfName.LENGTH, new PdfNumber(streamBytes.Length));
            }
            finally {
                if (isp != null) {
                    try{
                        isp.Close();
                    }
                    catch  {
                        // empty on purpose
                    }
                }
            }
        }
        
        /**
        * Returns the <CODE>PdfName</CODE> of the image.
        *
        * @return        the name
        */
        
        virtual public PdfName Name {
            get {
                return name;
            }
        }

        virtual public Image Image
        {
            get { return image; }
        }

        internal static void TransferBytes(Stream inp, Stream outp, int len) {
            byte[] buffer = new byte[TRANSFERSIZE];
            if (len < 0)
                len = 0x7fff0000;
            int size;
            while (len != 0) {
                size = inp.Read(buffer, 0, Math.Min(len, TRANSFERSIZE));
                if (size <= 0)
                    return;
                outp.Write(buffer, 0, size);
                len -= size;
            }
        }
        
        virtual protected void ImportAll(PdfImage dup) {
            name = dup.name;
            compressed = dup.compressed;
            compressionLevel = dup.compressionLevel;
            streamBytes = dup.streamBytes;
            bytes = dup.bytes;
            hashMap = dup.hashMap;
        }

        /**
         * Called when no resource name is provided in our constructor.  This generates a 
         * name that is required to be unique within a given resource dictionary.
         * @since 5.0.1
         */
        private void GenerateImgResName( Image img ) {
    	    name = new PdfName("img" + img.MySerialId.ToString("X"));
        }
    }
}
