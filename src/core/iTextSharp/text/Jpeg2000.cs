using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
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

namespace iTextSharp.text
{

    /**
    * An <CODE>Jpeg2000</CODE> is the representation of a graphic element (JPEG)
    * that has to be inserted into the document
    *
    * @see		Element
    * @see		Image
    */

    public class Jpeg2000 : Image
    {
        // public static final membervariables

        public const int JP2_JP = 0x6a502020;
        public const int JP2_IHDR = 0x69686472;
        public const int JPIP_JPIP = 0x6a706970;

        public const int JP2_FTYP = 0x66747970;
        public const int JP2_JP2H = 0x6a703268;
        public const int JP2_COLR = 0x636f6c72;
        public const int JP2_JP2C = 0x6a703263;
        public const int JP2_URL = 0x75726c20;
        public const int JP2_DBTL = 0x6474626c;
        public const int JP2_BPCC = 0x62706363;
        public const int JP2_JP2 = 0x6a703220;

        private Stream inp;
        private int boxLength;
        private int boxType;
        private int numOfComps;
        private List<ColorSpecBox> colorSpecBoxes = null;
        private bool isJp2 = false;
        private byte[] bpcBoxData;

        private const int ZERO_BOX_SIZE = 2003;
        // Constructors

        public Jpeg2000(Image image) : base(image)
        {
            if (image is Jpeg2000)
            {
                Jpeg2000 jpeg2000 = (Jpeg2000) image;
                numOfComps = jpeg2000.numOfComps;
                if (colorSpecBoxes != null)
                    colorSpecBoxes = new List<ColorSpecBox>(jpeg2000.colorSpecBoxes);
                isJp2 = jpeg2000.isJp2;
                if (bpcBoxData != null)
                    bpcBoxData = (byte[])jpeg2000.bpcBoxData.Clone();
            }
        }

        /**
        * Constructs a <CODE>Jpeg2000</CODE>-object, using an <VAR>url</VAR>.
        *
        * @param		url			the <CODE>URL</CODE> where the image can be found
        * @throws BadElementException
        * @throws IOException
        */

        public Jpeg2000(Uri url) : base(url)
        {
            ProcessParameters();
        }

        /**
        * Constructs a <CODE>Jpeg2000</CODE>-object from memory.
        *
        * @param		img		the memory image
        * @throws BadElementException
        * @throws IOException
        */

        public Jpeg2000(byte[] img) : base((Uri) null)
        {
            rawData = img;
            originalData = img;
            ProcessParameters();
        }

        /**
        * Constructs a <CODE>Jpeg2000</CODE>-object from memory.
        *
        * @param		img			the memory image.
        * @param		width		the width you want the image to have
        * @param		height		the height you want the image to have
        * @throws BadElementException
        * @throws IOException
        */

        public Jpeg2000(byte[] img, float width, float height) : this(img)
        {
            scaledWidth = width;
            scaledHeight = height;
        }

        private int Cio_read(int n)
        {
            int v = 0;
            for (int i = n - 1; i >= 0; i--)
            {
                v += inp.ReadByte() << (i << 3);
            }
            return v;
        }

        virtual public void Jp2_read_boxhdr()
        {
            boxLength = Cio_read(4);
            boxType = Cio_read(4);
            if (boxLength == 1)
            {
                if (Cio_read(4) != 0)
                {
                    throw new IOException(
                        MessageLocalization.GetComposedMessage("cannot.handle.box.sizes.higher.than.2.32"));
                }
                boxLength = Cio_read(4);
                if (boxLength == 0)
                    throw new ZeroBoxSiteException(MessageLocalization.GetComposedMessage("unsupported.box.size.eq.eq.0"));
            }
            else if (boxLength == 0)
            {
                throw new IOException(MessageLocalization.GetComposedMessage("unsupported.box.size.eq.eq.0"));
            }
        }

        /**
        * This method checks if the image is a valid JPEG and processes some parameters.
        * @throws BadElementException
        * @throws IOException
        */

        private void ProcessParameters()
        {
            type = JPEG2000;
            originalType = ORIGINAL_JPEG2000;
            inp = null;
            try
            {
                if (rawData == null)
                {
                    WebRequest w = WebRequest.Create(url);
                    w.Credentials = CredentialCache.DefaultCredentials;
                    inp = w.GetResponse().GetResponseStream();
                }
                else
                {
                    inp = new MemoryStream(rawData);
                }
                boxLength = Cio_read(4);
                if (boxLength == 0x0000000c) {
                    isJp2 = true;
                    boxType = Cio_read(4);
                    if (JP2_JP != boxType)
                    {
                        throw new IOException(MessageLocalization.GetComposedMessage("expected.jp.marker"));
                    }
                    if (0x0d0a870a != Cio_read(4))
                    {
                        throw new IOException(MessageLocalization.GetComposedMessage("error.with.jp.marker"));
                    }

                    Jp2_read_boxhdr();
                    if (JP2_FTYP != boxType)
                    {
                        throw new IOException(MessageLocalization.GetComposedMessage("expected.ftyp.marker"));
                    }
                    Utilities.Skip(inp, boxLength - 8);
                    Jp2_read_boxhdr();
                    do
                    {
                        if (JP2_JP2H != boxType)
                        {
                            if (boxType == JP2_JP2C)
                            {
                                throw new IOException(MessageLocalization.GetComposedMessage("expected.jp2h.marker"));
                            }
                            Utilities.Skip(inp, boxLength - 8);
                            Jp2_read_boxhdr();
                        }
                    } while (JP2_JP2H != boxType);
                    Jp2_read_boxhdr();
                    if (JP2_IHDR != boxType)
                    {
                        throw new IOException(MessageLocalization.GetComposedMessage("expected.ihdr.marker"));
                    }
                    scaledHeight = Cio_read(4);
                    Top = scaledHeight;
                    scaledWidth = Cio_read(4);
                    Right = scaledWidth;
                    numOfComps = Cio_read(2);
                    bpc = -1;
                    bpc = Cio_read(1);

                    Utilities.Skip(inp, 3);

                    Jp2_read_boxhdr();
                    if (boxType == JP2_BPCC) {
                        bpcBoxData = new byte[boxLength - 8];
                        inp.Read(bpcBoxData, 0, boxLength - 8);
                    } else if (boxType == JP2_COLR) {
                        do {
                            if (colorSpecBoxes == null)
                                colorSpecBoxes = new List<ColorSpecBox>();
                            colorSpecBoxes.Add(Jp2_read_colr());
                            try {
                                Jp2_read_boxhdr();
                            }
                            catch (ZeroBoxSiteException e) {
                                //Probably we have reached the contiguous codestream box which is the last in jpeg2000 and has no length.
                            }
                        } while (JP2_COLR == boxType);
                    }
                }
                else if ((uint) boxLength == 0xff4fff51)
                {
                    Utilities.Skip(inp, 4);
                    int x1 = Cio_read(4);
                    int y1 = Cio_read(4);
                    int x0 = Cio_read(4);
                    int y0 = Cio_read(4);
                    Utilities.Skip(inp, 16);
                    colorspace = Cio_read(2);
                    bpc = 8;
                    scaledHeight = y1 - y0;
                    Top = scaledHeight;
                    scaledWidth = x1 - x0;
                    Right = scaledWidth;
                }
                else
                {
                    throw new IOException(MessageLocalization.GetComposedMessage("not.a.valid.jpeg2000.file"));
                }
            }
            finally
            {
                if (inp != null) {
                    try {
                        inp.Close();
                    }
                    catch { }
                    inp = null;
                }
            }
            plainWidth = this.Width;
            plainHeight = this.Height;
        }

        private ColorSpecBox Jp2_read_colr()
        {
            int readBytes = 8;
            ColorSpecBox colr = new ColorSpecBox();
            for (int i = 0; i < 3; i++)
            {
                colr.Add(Cio_read(1));
                readBytes++;
            }
            if (colr.GetMeth() == 1)
            {
                colr.Add(Cio_read(4));
                readBytes += 4;
            }
            else
            {
                colr.Add(0);
            }

            if (boxLength - readBytes > 0)
            {
                byte[] colorProfile = new byte[boxLength - readBytes];
                inp.Read(colorProfile, 0, boxLength - readBytes);
                colr.SetColorProfile(colorProfile);
            }
            return colr;
        }

        virtual public int GetNumOfComps()
        {
            return numOfComps;
        }

        virtual public byte[] GetBpcBoxData()
        {
            return bpcBoxData;
        }

        virtual public List<ColorSpecBox> GetColorSpecBoxes()
        {
            return colorSpecBoxes;
        }

        /**
         * @return <code>true</code> if the image is JP2, <code>false</code> if a codestream.
         */
        virtual public bool IsJp2()
        {
            return isJp2;
        }

        public class ColorSpecBox : List<int?>
        {
            private byte[] colorProfile;

            virtual public int? GetMeth()
            {
                return this.Count > 0 ? this[0] : null;
            }

            virtual public int? GetPrec() {
                return this.Count > 1 ? this[1] : null;
            }

            virtual public int? GetApprox() {
                return this.Count > 2 ? this[2] : null;
            }

            virtual public int? GetEnumCs() {
                return this.Count > 3 ? this[3] : null;
            }

            virtual public byte[] GetColorProfile() {
                return colorProfile;
            }

            internal void SetColorProfile(byte[] colorProfile) {
                this.colorProfile = colorProfile;
            }
        }

        private class ZeroBoxSiteException : IOException 
        {
            public ZeroBoxSiteException() { }

            public ZeroBoxSiteException(string message) : base(message) { }

            public ZeroBoxSiteException(string message, int hresult) : base(message, hresult) { }

            public ZeroBoxSiteException(string message, Exception innerException) : base(message, innerException) { }

            protected ZeroBoxSiteException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }



    }
}
