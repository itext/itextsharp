using System;
using System.IO;
using System.Collections;
using System.util.zlib;
/*
 * $Id$
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
    * Extends PdfStream and should be used to create Streams for Embedded Files
    * (file attachments).
    * @since	2.1.3
    */

    public class PdfEFStream : PdfStream {

	    /**
	    * Creates a Stream object using an InputStream and a PdfWriter object
	    * @param	in	the InputStream that will be read to get the Stream object
	    * @param	writer	the writer to which the stream will be added
	    */
	    public PdfEFStream(Stream inp, PdfWriter writer) : base (inp, writer) {
	    }

	    /**
	    * Creates a Stream object using a byte array
	    * @param	fileStore	the bytes for the stream
	    */
	    public PdfEFStream(byte[] fileStore) : base(fileStore) {
	    }

        /**
        * @see com.lowagie.text.pdf.PdfDictionary#toPdf(com.lowagie.text.pdf.PdfWriter, java.io.OutputStream)
        */
        public override void ToPdf(PdfWriter writer, Stream os) {
            if (inputStream != null && compressed)
                Put(PdfName.FILTER, PdfName.FLATEDECODE);
            PdfEncryption crypto = null;
            if (writer != null)
                crypto = writer.Encryption;
            if (crypto != null) {
                PdfObject filter = Get(PdfName.FILTER);
                if (filter != null) {
                    if (PdfName.CRYPT.Equals(filter))
                        crypto = null;
                    else if (filter.IsArray()) {
                        PdfArray a = (PdfArray)filter;
                        if (!a.IsEmpty() && PdfName.CRYPT.Equals(a[0]))
                            crypto = null;
                    }
                }
            }
    	    if (crypto != null && crypto.IsEmbeddedFilesOnly()) {
    		    PdfArray filter = new PdfArray();
    		    PdfArray decodeparms = new PdfArray();
    		    PdfDictionary crypt = new PdfDictionary();
    		    crypt.Put(PdfName.NAME, PdfName.STDCF);
    		    filter.Add(PdfName.CRYPT);
    		    decodeparms.Add(crypt);
    		    if (compressed) {
    			    filter.Add(PdfName.FLATEDECODE);
    			    decodeparms.Add(new PdfNull());
    		    }
    		    Put(PdfName.FILTER, filter);
    		    Put(PdfName.DECODEPARMS, decodeparms);
    	    }
            PdfObject nn = Get(PdfName.LENGTH);
            if (crypto != null && nn != null && nn.IsNumber()) {
                int sz = ((PdfNumber)nn).IntValue;
                Put(PdfName.LENGTH, new PdfNumber(crypto.CalculateStreamSize(sz)));
                SuperToPdf(writer, os);
                Put(PdfName.LENGTH, nn);
            }
            else
                SuperToPdf(writer, os);

            os.Write(STARTSTREAM, 0, STARTSTREAM.Length);
            if (inputStream != null) {
                rawLength = 0;
                ZDeflaterOutputStream def = null;
                OutputStreamCounter osc = new OutputStreamCounter(os);
                OutputStreamEncryption ose = null;
                Stream fout = osc;
                if (crypto != null)
                    fout = ose = crypto.GetEncryptionStream(fout);
                if (compressed)    
                    fout = def = new ZDeflaterOutputStream(fout, compressionLevel);
                
                byte[] buf = new byte[4192];
                while (true) {
                    int n = inputStream.Read(buf, 0, buf.Length);
                    if (n <= 0)
                        break;
                    fout.Write(buf, 0, n);
                    rawLength += n;
                }
                if (def != null)
                    def.Finish();
                if (ose != null)
                    ose.Finish();
                inputStreamLength = (int)osc.Counter;
            }
            else {
                if (crypto == null) {
                    if (streamBytes != null)
                        streamBytes.WriteTo(os);
                    else
                        os.Write(bytes, 0, bytes.Length);
                }
                else {
                    byte[] b;
                    if (streamBytes != null) {
                        b = crypto.EncryptByteArray(streamBytes.ToArray());
                    }
                    else {
                        b = crypto.EncryptByteArray(bytes);
                    }
                    os.Write(b, 0, b.Length);
                }
            }
            os.Write(ENDSTREAM, 0, ENDSTREAM.Length);
        }
    }
}
