/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
using iTextSharp.text;
using iTextSharp.text.pdf.parser;

namespace iTextSharp.javaone.edition14.part4.helper
{
    /// <summary>
    /// Subclass of the MyItem class that is used to store the coordinates
    /// of an image.
    /// </summary>
    public class ImageItem : MyItem
    {
        /// <summary>
        /// Images will be marked in this color.
        /// </summary>
        public static readonly BaseColor IMG_COLOR = BaseColor.RED;

        /// <summary>
        /// Creates an ImageItem based on an ImageRenderInfo object.
        /// </summary>
        /// <param name="imageRenderInfo">Object containing info about an image</param>
        public ImageItem(ImageRenderInfo imageRenderInfo) : base()
        {
            rectangle = GetRectangle(imageRenderInfo);
            color = IMG_COLOR;
        }

        /// <summary>
        /// Converts the Matrix containing the coordinates of an image as stored
        /// in an ImageRenderInfo object into a Rectangle.
        /// </summary>
        /// <param name="imageRenderInfo">Object that contains info about an image</param>
        /// <returns>coordinates in the form of a Rectangle object</returns>
        private static Rectangle GetRectangle(ImageRenderInfo imageRenderInfo)
        {
            Matrix ctm = imageRenderInfo.GetImageCTM();
            return new Rectangle(ctm[6], ctm[7], ctm[6] + ctm[0], ctm[7] + ctm[4]);
        }
    }
}
