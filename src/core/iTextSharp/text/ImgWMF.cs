using System;
using System.IO;
using System.Net;
using iTextSharp.text.error_messages;

using iTextSharp.text.pdf;
using iTextSharp.text.pdf.codec.wmf;

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
    /**
     * An ImgWMF is the representation of a windows metafile
     * that has to be inserted into the document
     *
     * @see        Element
     * @see        Image
     * @see        Gif
     * @see        Png
     */
    /// <summary>
    /// An ImgWMF is the representation of a windows metafile
    /// that has to be inserted into the document
    /// </summary>
    public class ImgWMF : Image {
    
        // Constructors
        /// <summary>
        /// Constructs an ImgWMF-object
        /// </summary>
        /// <param name="image">a Image</param>
        public ImgWMF(Image image) : base(image) {}
    
        /// <summary>
        /// Constructs an ImgWMF-object, using an url.
        /// </summary>
        /// <param name="url">the URL where the image can be found</param>
        public ImgWMF(Uri url) : base(url) {
            ProcessParameters();
        }

        /// <summary>
        /// Constructs an ImgWMF-object, using a filename.
        /// </summary>
        /// <param name="filename">a string-representation of the file that contains the image.</param>
        public ImgWMF(string filename) : this(Utilities.ToURL(filename)) {}
    
        /// <summary>
        /// Constructs an ImgWMF-object from memory.
        /// </summary>
        /// <param name="img">the memory image</param>
        public ImgWMF(byte[] img) : base((Uri)null) {
            rawData = img;
            originalData = img;
            ProcessParameters();
        }
    
        /// <summary>
        /// This method checks if the image is a valid WMF and processes some parameters.
        /// </summary>
        private void ProcessParameters() {
            type = Element.IMGTEMPLATE;
            originalType = ORIGINAL_WMF;
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
                InputMeta im = new InputMeta(istr);
                if (im.ReadInt() != unchecked((int)0x9AC6CDD7))    {
                    throw new BadElementException(MessageLocalization.GetComposedMessage("1.is.not.a.valid.placeable.windows.metafile", errorID));
                }
                im.ReadWord();
                int left = im.ReadShort();
                int top = im.ReadShort();
                int right = im.ReadShort();
                int bottom = im.ReadShort();
                int inch = im.ReadWord();
                dpiX = 72;
                dpiY = 72;
                scaledHeight = (float)(bottom - top) / inch * 72f;
                this.Top =scaledHeight;
                scaledWidth = (float)(right - left) / inch * 72f;
                this.Right = scaledWidth;
            }
            finally {
                if (istr != null) {
                    istr.Close();
                }
                plainWidth = this.Width;
                plainHeight = this.Height;
            }
        }
    
        /// <summary>
        /// Reads the WMF into a template.
        /// </summary>
        /// <param name="template">the template to read to</param>
        virtual public void ReadWMF(PdfTemplate template) {
            TemplateData = template;
            template.Width = this.Width;
            template.Height = this.Height;
            Stream istr = null;
            try {
                if (rawData == null){
                    WebRequest w = WebRequest.Create(url);
                    w.Credentials = CredentialCache.DefaultCredentials;
                    istr = w.GetResponse().GetResponseStream();
                }
                else{
                    istr = new MemoryStream(rawData);
                }
                MetaDo meta = new MetaDo(istr, template);
                meta.ReadAll();
            }
            finally {
                if (istr != null) {
                    istr.Close();
                }
            }
        }
    }
}
