using System;
using System.IO;
using System.Net;
using System.util;
using iTextSharp.text.pdf;
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

namespace iTextSharp.text {
    /// <summary>
    /// An Jpeg is the representation of a graphic element (JPEG)
    /// that has to be inserted into the document
    /// </summary>
    /// <seealso cref="T:iTextSharp.text.Element"/>
    /// <seealso cref="T:iTextSharp.text.Image"/>
    /// <seealso cref="T:iTextSharp.text.Gif"/>
    /// <seealso cref="T:iTextSharp.text.Png"/>
    public class Jpeg : Image {
    
        // public static membervariables
    
        /// <summary> This is a type of marker. </summary>
        public const int NOT_A_MARKER = -1;
    
        /// <summary> This is a type of marker. </summary>
        public const int VALID_MARKER = 0;
    
        /// <summary> Acceptable Jpeg markers. </summary>
        public static int[] VALID_MARKERS = {0xC0, 0xC1, 0xC2};
    
        /// <summary> This is a type of marker. </summary>
        public const int UNSUPPORTED_MARKER = 1;
    
        /// <summary> Unsupported Jpeg markers. </summary>
        public static int[] UNSUPPORTED_MARKERS = {0xC3, 0xC5, 0xC6, 0xC7, 0xC8, 0xC9, 0xCA, 0xCB, 0xCD, 0xCE, 0xCF};
    
        /// <summary> This is a type of marker. </summary>
        public const int NOPARAM_MARKER = 2;
    
        /// <summary> Jpeg markers without additional parameters. </summary>
        public static int[] NOPARAM_MARKERS = {0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0x01};
    
        public const int M_APP0 = 0xE0;
        public const int M_APP2 = 0xE2;
        public const int M_APPE = 0xEE;
        /** Marker value for Photoshop IRB */
        public const int M_APPD = 0xED;
    
        public static byte[] JFIF_ID = {0x4A, 0x46, 0x49, 0x46, 0x00};
        /** sequence preceding Photoshop resolution data */
        public static readonly byte[] PS_8BIM_RESO = {0x38, 0x42, 0x49, 0x4d, 0x03, (byte) 0xed};

        private byte[][] icc;

        // Constructors
    
        /// <summary>
        /// Construct a Jpeg-object, using a Image
        /// </summary>
        /// <param name="image">a Image</param>
        public Jpeg(Image image) : base(image) {}

        /// <summary>
        /// Constructs a Jpeg-object, using an Uri.
        /// </summary>
        /// <remarks>
        /// Deprecated, use Image.GetInstance(...) to create an Image
        /// </remarks>
        /// <param name="Uri">the Uri where the image can be found</param>
        public Jpeg(Uri Uri) : base(Uri) {
            ProcessParameters();
        }
    
        /// <summary>
        /// Constructs a Jpeg-object from memory.
        /// </summary>
        /// <param name="img">the memory image</param>
        public Jpeg(byte[] img) : base((Uri)null) {
            rawData = img;
            originalData = img;
            ProcessParameters();
        }

        /// <summary>
        /// Constructs a Jpeg-object from memory.
        /// </summary>
        /// <param name="img">the memory image.</param>
        /// <param name="width">the width you want the image to have</param>
        /// <param name="height">the height you want the image to have</param>
        public Jpeg(byte[] img, float width, float height) : this(img) {
            scaledWidth = width;
            scaledHeight = height;
        }
    
        // private static methods
    
        /// <summary>
        /// Reads a short from the Stream.
        /// </summary>
        /// <param name="istr">the Stream</param>
        /// <returns>an int</returns>
        private static int GetShort(Stream istr) {
            return (istr.ReadByte() << 8) + istr.ReadByte();
        }
    
        /// <summary>
        /// Reads an inverted short from the Stream.
        /// </summary>
        /// <param name="istr">the Stream</param>
        /// <returns>an int</returns>
        private static int GetShortInverted(Stream istr) {
            return (istr.ReadByte() + istr.ReadByte() << 8);
        }

        /// <summary>
        /// Returns a type of marker.
        /// </summary>
        /// <param name="marker">an int</param>
        /// <returns>a type: VALID_MARKER, UNSUPPORTED_MARKER or NOPARAM_MARKER</returns>
        private static int MarkerType(int marker) {
            for (int i = 0; i < VALID_MARKERS.Length; i++) {
                if (marker == VALID_MARKERS[i]) {
                    return VALID_MARKER;
                }
            }
            for (int i = 0; i < NOPARAM_MARKERS.Length; i++) {
                if (marker == NOPARAM_MARKERS[i]) {
                    return NOPARAM_MARKER;
                }
            }
            for (int i = 0; i < UNSUPPORTED_MARKERS.Length; i++) {
                if (marker == UNSUPPORTED_MARKERS[i]) {
                    return UNSUPPORTED_MARKER;
                }
            }
            return NOT_A_MARKER;
        }
    
        // private methods
    
        /// <summary>
        /// This method checks if the image is a valid JPEG and processes some parameters.
        /// </summary>
        private void ProcessParameters() {
            type = Element.JPEG;
            originalType = ORIGINAL_JPEG;
            Stream istr = null;
            try {
                string errorID;
                if (rawData == null){
                    WebRequest w = WebRequest.Create(url);
                    w.Credentials = CredentialCache.DefaultCredentials;
                    istr = w.GetResponse().GetResponseStream();
                    errorID = url.ToString();
                }
                else{
                    istr = new MemoryStream(rawData);
                    errorID = "Byte array";
                }
                if (istr.ReadByte() != 0xFF || istr.ReadByte() != 0xD8)    {
                    throw new BadElementException(MessageLocalization.GetComposedMessage("1.is.not.a.valid.jpeg.file", errorID));
                }
                bool firstPass = true;
                int len;
                while (true) {
                    int v = istr.ReadByte();
                    if (v < 0)
                        throw new IOException(MessageLocalization.GetComposedMessage("premature.eof.while.reading.jpg"));
                    if (v == 0xFF) {
                        int marker = istr.ReadByte();
                        if (firstPass && marker == M_APP0) {
                            firstPass = false;
                            len = GetShort(istr);
                            if (len < 16) {
                                Utilities.Skip(istr, len - 2);
                                continue;
                            }
                            byte[] bcomp = new byte[JFIF_ID.Length];
                            int r = istr.Read(bcomp, 0, bcomp.Length);
                            if (r != bcomp.Length)
                                throw new BadElementException(MessageLocalization.GetComposedMessage("1.corrupted.jfif.marker", errorID));
                            bool found = true;
                            for (int k = 0; k < bcomp.Length; ++k) {
                                if (bcomp[k] != JFIF_ID[k]) {
                                    found = false;
                                    break;
                                }
                            }
                            if (!found) {
                                Utilities.Skip(istr, len - 2 - bcomp.Length);
                                continue;
                            }
                            Utilities.Skip(istr, 2);
                            int units = istr.ReadByte();
                            int dx = GetShort(istr);
                            int dy = GetShort(istr);
                            if (units == 1) {
                                dpiX = dx;
                                dpiY = dy;
                            }
                            else if (units == 2) {
                                dpiX = (int)((float)dx * 2.54f + 0.5f);
                                dpiY = (int)((float)dy * 2.54f + 0.5f);
                            }
                            Utilities.Skip(istr, len - 2 - bcomp.Length - 7);
                            continue;
                        }
                        if (marker == M_APPE) {
                            len = GetShort(istr) - 2;
                            byte[] byteappe = new byte[len];
                            for (int k = 0; k < len; ++k) {
                                byteappe[k] = (byte)istr.ReadByte();
                            }
                            if (byteappe.Length >= 12) {
                                string appe = System.Text.ASCIIEncoding.ASCII.GetString(byteappe,0,5);
                                if (Util.EqualsIgnoreCase(appe, "adobe")) {
                                    invert = true;
                                }
                            }
                            continue;
                        }
                        if (marker == M_APP2) {
                            len = GetShort(istr) - 2;
                            byte[] byteapp2 = new byte[len];
                            for (int k = 0; k < len; ++k) {
                                byteapp2[k] = (byte)istr.ReadByte();
                            }
                            if (byteapp2.Length >= 14) {
                                String app2 = System.Text.ASCIIEncoding.ASCII.GetString(byteapp2, 0, 11);
                                if (app2.Equals("ICC_PROFILE")) {
                                    int order = byteapp2[12] & 0xff;
                                    int count = byteapp2[13] & 0xff;
                                    // some jpeg producers don't know how to count to 1
                                    if (order < 1)
                                        order = 1;
                                    if (count < 1)
                                        count = 1;
                                    if (icc == null)
                                        icc = new byte[count][];
                                    icc[order - 1] = byteapp2;
                                }
                            }
                            continue;
                        }
                        if (marker == M_APPD) {
                            len = GetShort(istr) - 2;
                            byte[] byteappd = new byte[len];
                            for (int k = 0; k < len; k++) {
                                byteappd[k] = (byte)istr.ReadByte();
                            }
                            // search for '8BIM Resolution' marker
                            int j = 0;
                            for (j = 0; j < len-PS_8BIM_RESO.Length; j++) {
                                bool found = true;
                                for (int i = 0; i < PS_8BIM_RESO.Length; i++) {
                                    if (byteappd[j+i] != PS_8BIM_RESO[i]) {
                                        found = false;
                                        break;
                                    }
                                }
                                if (found)
                                    break;
                            }

                            j+=PS_8BIM_RESO.Length;
                            if (j < len-PS_8BIM_RESO.Length) {
                                // "PASCAL String" for name, i.e. string prefix with length byte
                                // padded to be even length; 2 null bytes if empty
                                byte namelength = byteappd[j];
                                // add length byte
                                namelength++;
                                // add padding
                                if (namelength % 2 == 1)
                                    namelength++;
                                // just skip name
                                j += namelength;
                                // size of the resolution data
                                int resosize = (byteappd[j] << 24) + (byteappd[j+1] << 16) + (byteappd[j+2] << 8) + byteappd[j+3];
                                // should be 16
                                if (resosize != 16) {
                                    // fail silently, for now
                                    //System.err.println("DEBUG: unsupported resolution IRB size");
                                    continue;
                                }
                                j+=4;
                                int dx = (byteappd[j] << 8) + (byteappd[j+1] & 0xff);
                                j+=2;
                                // skip 2 unknown bytes
                                j+=2;
                                int unitsx = (byteappd[j] << 8) + (byteappd[j + 1] & 0xff);
                                j+=2;
                                // skip 2 unknown bytes
                                j+=2;
                                int dy = (byteappd[j] << 8) + (byteappd[j + 1] & 0xff);
                                j+=2;
                                // skip 2 unknown bytes
                                j+=2;
                                int unitsy = (byteappd[j] << 8) + (byteappd[j + 1] & 0xff);
                                
                                if (unitsx == 1 || unitsx == 2) {
                                    dx = (unitsx == 2 ? (int)(dx * 2.54f + 0.5f) : dx);
                                    // make sure this is consistent with JFIF data
                                    if (dpiX != 0 && dpiX != dx) {
                                        //System.err.println("DEBUG: inconsistent metadata (dpiX: " + dpiX + " vs " + dx + ")");
                                    }
                                    else
                                        dpiX = dx;
                                }
                                if (unitsy == 1 || unitsy == 2) {
                                    dy = (unitsy == 2 ? (int)(dy * 2.54f + 0.5f) : dy);
                                    // make sure this is consistent with JFIF data
                                    if (dpiY != 0 && dpiY != dy) {
                                        //System.err.println("DEBUG: inconsistent metadata (dpiY: " + dpiY + " vs " + dy + ")");
                                    }
                                    else
                                        dpiY = dy;
                                }
                            }
                            continue;
                        }
                        firstPass = false;
                        int markertype = MarkerType(marker);
                        if (markertype == VALID_MARKER) {
                            Utilities.Skip(istr, 2);
                            if (istr.ReadByte() != 0x08) {
                                throw new BadElementException(MessageLocalization.GetComposedMessage("1.must.have.8.bits.per.component", errorID));
                            }
                            scaledHeight = GetShort(istr);
                            Top = scaledHeight;
                            scaledWidth = GetShort(istr);
                            Right = scaledWidth;
                             colorspace = istr.ReadByte();
                            bpc = 8;
                            break;
                        }
                        else if (markertype == UNSUPPORTED_MARKER) {
                            throw new BadElementException(MessageLocalization.GetComposedMessage("1.unsupported.jpeg.marker.2", errorID, marker));
                        }
                        else if (markertype != NOPARAM_MARKER) {
                            Utilities.Skip(istr, GetShort(istr) - 2);
                        }
                    }
                }
            }
            finally {
                if (istr != null) {
                    istr.Close();
                }
            }
            plainWidth = this.Width;
            plainHeight = this.Height;
            if (icc != null) {
                int total = 0;
                for (int k = 0; k < icc.Length; ++k) {
                    if (icc[k] == null) {
                        icc = null;
                        return;
                    }
                    total += icc[k].Length - 14;
                }
                byte[] ficc = new byte[total];
                total = 0;
                for (int k = 0; k < icc.Length; ++k) {
                    System.Array.Copy(icc[k], 14, ficc, total, icc[k].Length - 14);
                    total += icc[k].Length - 14;
                }
                try {
                    ICC_Profile icc_prof = ICC_Profile.GetInstance(ficc, colorspace);
                    TagICC = icc_prof;
                }
                catch {}
                icc = null;
            }
        }
    }
}
