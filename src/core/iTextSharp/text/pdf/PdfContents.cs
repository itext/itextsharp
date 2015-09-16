using System;
using System.IO;

using iTextSharp.text;

using System.util.zlib;

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
     * <CODE>PdfContents</CODE> is a <CODE>PdfStream</CODE> containing the contents (text + graphics) of a <CODE>PdfPage</CODE>.
     */

    public class PdfContents : PdfStream {
    
        internal static byte[] SAVESTATE = DocWriter.GetISOBytes("q\n");
        internal static byte[] RESTORESTATE = DocWriter.GetISOBytes("Q\n");
        internal static byte[] ROTATE90 = DocWriter.GetISOBytes("0 1 -1 0 ");
        internal static byte[] ROTATE180 = DocWriter.GetISOBytes("-1 0 0 -1 ");
        internal static byte[] ROTATE270 = DocWriter.GetISOBytes("0 -1 1 0 ");
        internal static byte[] ROTATEFINAL = DocWriter.GetISOBytes(" cm\n");
        // constructor
    
        /**
         * Constructs a <CODE>PdfContents</CODE>-object, containing text and general graphics.
         *
         * @param under the direct content that is under all others
         * @param content the graphics in a page
         * @param text the text in a page
         * @param secondContent the direct content that is over all others
         * @throws BadPdfFormatException on error
         */
    
        internal PdfContents(PdfContentByte under, PdfContentByte content, PdfContentByte text, PdfContentByte secondContent, Rectangle page) : base() {
            Stream ostr = null;
            streamBytes = new MemoryStream();
            if (Document.Compress) {
                compressed = true;
                int compresLevel;
                if (text != null)
                    compresLevel = text.PdfWriter.CompressionLevel;
                else
                    compresLevel = content.PdfWriter.CompressionLevel;
                ostr = new ZDeflaterOutputStream(streamBytes, compresLevel);
            }
            else
                ostr = streamBytes;
            int rotation = page.Rotation;
            byte[] tmp;
            switch (rotation) {
                case 90:
                    ostr.Write(ROTATE90, 0, ROTATE90.Length);
                    tmp = DocWriter.GetISOBytes(ByteBuffer.FormatDouble(page.Top));
                    ostr.Write(tmp, 0, tmp.Length);
                    ostr.WriteByte((byte)' ');
                    ostr.WriteByte((byte)'0');
                    ostr.Write(ROTATEFINAL, 0, ROTATEFINAL.Length);
                    break;
                case 180:
                    ostr.Write(ROTATE180, 0, ROTATE180.Length);
                    tmp = DocWriter.GetISOBytes(ByteBuffer.FormatDouble(page.Right));
                    ostr.Write(tmp, 0, tmp.Length);
                    ostr.WriteByte((byte)' ');
                    tmp = DocWriter.GetISOBytes(ByteBuffer.FormatDouble(page.Top));
                    ostr.Write(tmp, 0, tmp.Length);
                    ostr.Write(ROTATEFINAL, 0, ROTATEFINAL.Length);
                    break;
                case 270:
                    ostr.Write(ROTATE270, 0, ROTATE270.Length);
                    ostr.WriteByte((byte)'0');
                    ostr.WriteByte((byte)' ');
                    tmp = DocWriter.GetISOBytes(ByteBuffer.FormatDouble(page.Right));
                    ostr.Write(tmp, 0, tmp.Length);
                    ostr.Write(ROTATEFINAL, 0, ROTATEFINAL.Length);
                    break;
            }
            if (under.Size > 0) {
                ostr.Write(SAVESTATE, 0, SAVESTATE.Length);
                under.InternalBuffer.WriteTo(ostr);
                ostr.Write(RESTORESTATE, 0, RESTORESTATE.Length);
            }
            if (content.Size > 0) {
                ostr.Write(SAVESTATE, 0, SAVESTATE.Length);
                content.InternalBuffer.WriteTo(ostr);
                ostr.Write(RESTORESTATE, 0, RESTORESTATE.Length);
            }
            if (text != null) {
                ostr.Write(SAVESTATE, 0, SAVESTATE.Length);
                text.InternalBuffer.WriteTo(ostr);
                ostr.Write(RESTORESTATE, 0, RESTORESTATE.Length);
            }
            if (secondContent.Size > 0) {
                secondContent.InternalBuffer.WriteTo(ostr);
            }

            if (ostr is ZDeflaterOutputStream)
                ((ZDeflaterOutputStream)ostr).Finish();
            Put(PdfName.LENGTH, new PdfNumber(streamBytes.Length));
            if (compressed)
                Put(PdfName.FILTER, PdfName.FLATEDECODE);
        }
    }
}
