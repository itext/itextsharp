using System;
using iTextSharp.text.pdf.codec;
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
    /**
     * CCITT Image data that has to be inserted into the document
     *
     * @see        Element
     * @see        Image
     *
     * @author  Paulo Soares
     */
    /// <summary>
    /// CCITT Image data that has to be inserted into the document
    /// </summary>
    /// <seealso cref="T:iTextSharp.text.Element"/>
    /// <seealso cref="T:iTextSharp.text.Image"/>
    public class ImgCCITT : Image {
        public ImgCCITT(Image image) : base(image) {}

        /// <summary>
        /// Creats an Image in CCITT mode.
        /// </summary>
        /// <param name="width">the exact width of the image</param>
        /// <param name="height">the exact height of the image</param>
        /// <param name="reverseBits">
        /// reverses the bits in data.
        /// Bit 0 is swapped with bit 7 and so on
        /// </param>
        /// <param name="typeCCITT">
        /// the type of compression in data. It can be
        /// CCITTG4, CCITTG31D, CCITTG32D
        /// </param>
        /// <param name="parameters">
        /// parameters associated with this stream. Possible values are
        /// CCITT_BLACKIS1, CCITT_ENCODEDBYTEALIGN, CCITT_ENDOFLINE and CCITT_ENDOFBLOCK or a
        /// combination of them
        /// </param>
        /// <param name="data">the image data</param>
        public ImgCCITT(int width, int height, bool reverseBits, int typeCCITT, int parameters, byte[] data) : base((Uri)null) {
            if (typeCCITT != Element.CCITTG4 && typeCCITT != Element.CCITTG3_1D && typeCCITT != Element.CCITTG3_2D)
                throw new BadElementException(MessageLocalization.GetComposedMessage("the.ccitt.compression.type.must.be.ccittg4.ccittg3.1d.or.ccittg3.2d"));
            if (reverseBits)
                TIFFFaxDecoder.ReverseBits(data);
            type = Element.IMGRAW;
            scaledHeight = height;
            this.Top = scaledHeight;
            scaledWidth = width;
            this.Right = scaledWidth;
            colorspace = parameters;
            bpc = typeCCITT;
            rawData = data;
            plainWidth = this.Width;
            plainHeight = this.Height;
        }
    }
}
