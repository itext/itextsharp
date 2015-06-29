using System;
using System.IO;
using iTextSharp.text.pdf;
using System.util;
/*
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Kevin Day, Bruno Lowagie, Paulo Soares, et al.
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
     * @author kevin
     * @since 5.0.1
     */
    public class ContentByteUtils {
        private ContentByteUtils() {
        }

        /**
         * Gets the content bytes from a content object, which may be a reference
         * a stream or an array.
         * @param contentObject the object to read bytes from
         * @return the content bytes
         * @throws IOException
         */
        public static byte[] GetContentBytesFromContentObject(PdfObject contentObject) {
            byte[] result;
            switch (contentObject.Type)
            {
                case PdfObject.INDIRECT:
                    PRIndirectReference refi = (PRIndirectReference) contentObject;
                    PdfObject directObject = PdfReader.GetPdfObjectRelease(refi);
                    result = GetContentBytesFromContentObject(directObject);
                    break;
                case PdfObject.STREAM:
                    PRStream stream = (PRStream) PdfReader.GetPdfObjectRelease(contentObject);
                    result = PdfReader.GetStreamBytes(stream);
                    break;
                case PdfObject.ARRAY:
                    // Stitch together all content before calling ProcessContent(), because
                    // ProcessContent() resets state.
                    MemoryStream allBytes = new MemoryStream();
                    PdfArray contentArray = (PdfArray) contentObject;
                    ListIterator<PdfObject> iter = contentArray.GetListIterator();
                    while (iter.HasNext()) {
                        PdfObject element = iter.Next();
                        byte[] b;
                        allBytes.Write(b = GetContentBytesFromContentObject(element), 0, b.Length);
                        allBytes.WriteByte((byte)' ');
                    }
                    result = allBytes.ToArray();
                    break;
                default:
                    String msg = "Unable to handle Content of type " + contentObject.GetType();
                throw new InvalidOperationException(msg);
            }
            return result;
        }

        /**
         * Gets the content bytes of a page from a reader
         * @param reader  the reader to get content bytes from
         * @param pageNum   the page number of page you want get the content stream from
         * @return  a byte array with the effective content stream of a page
         * @throws IOException
         * @since 5.0.1
         */
        public static byte[] GetContentBytesForPage(PdfReader reader, int pageNum) {
            PdfDictionary pageDictionary = reader.GetPageN(pageNum);
            PdfObject contentObject = pageDictionary.Get(PdfName.CONTENTS);
            if (contentObject == null)
                return new byte[0];
            
            byte[] contentBytes = ContentByteUtils.GetContentBytesFromContentObject(contentObject);
            return contentBytes;
        }
    }
}
