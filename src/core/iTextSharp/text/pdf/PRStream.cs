using System;
using System.IO;

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

public class PRStream : PdfStream {
    
    protected PdfReader reader;
	protected long offset;
    protected int length;
    
    //added by ujihara for decryption
    protected int objNum = 0;
    protected int objGen = 0;
    
    public PRStream(PRStream stream, PdfDictionary newDic) {
        reader = stream.reader;
        offset = stream.offset;
        length = stream.Length;
        compressed = stream.compressed;
        compressionLevel = stream.compressionLevel;
        streamBytes = stream.streamBytes;
        bytes = stream.bytes;
        objNum = stream.objNum;
        objGen = stream.objGen;
        if (newDic != null)
            Merge(newDic);
        else
            Merge(stream);
    }

    public PRStream(PRStream stream, PdfDictionary newDic, PdfReader reader) : this(stream, newDic) {
        this.reader = reader;
    }

	public PRStream (PdfReader reader, long offset) {
        this.reader = reader;
        this.offset = offset;
    }
    
    public PRStream(PdfReader reader, byte[] conts) : this(reader, conts, DEFAULT_COMPRESSION) {
    }

    /**
     * Creates a new PDF stream object that will replace a stream
     * in a existing PDF file.
     * @param   reader  the reader that holds the existing PDF
     * @param   conts   the new content
     * @param   compressionLevel    the compression level for the content
     * @since   2.1.3 (replacing the existing constructor without param compressionLevel)
     */
    public PRStream(PdfReader reader, byte[] conts, int compressionLevel) {
        this.reader = reader;
        this.offset = -1;
        if (Document.Compress) {
            MemoryStream stream = new MemoryStream();
            ZDeflaterOutputStream zip = new ZDeflaterOutputStream(stream, compressionLevel);
            zip.Write(conts, 0, conts.Length);
            zip.Close();
            bytes = stream.ToArray();
            Put(PdfName.FILTER, PdfName.FLATEDECODE);
        }
        else
            bytes = conts;
        Length = bytes.Length;
    }
    
    /**
     * Sets the data associated with the stream, either compressed or
     * uncompressed. Note that the data will never be compressed if
     * Document.compress is set to false.
     * 
     * @param data raw data, decrypted and uncompressed.
     * @param compress true if you want the stream to be compresssed.
     * @since   iText 2.1.1
     */
    virtual public void SetData(byte[] data, bool compress) {
        SetData(data, compress, DEFAULT_COMPRESSION);
    }
    
    /**
     * Sets the data associated with the stream, either compressed or
     * uncompressed. Note that the data will never be compressed if
     * Document.compress is set to false.
     * 
     * @param data raw data, decrypted and uncompressed.
     * @param compress true if you want the stream to be compresssed.
     * @param compressionLevel  a value between -1 and 9 (ignored if compress == false)
     * @since   iText 2.1.3
     */
    virtual public void SetData(byte[] data, bool compress, int compressionLevel) {
        Remove(PdfName.FILTER);
        this.offset = -1;
        if (Document.Compress && compress) {
            MemoryStream stream = new MemoryStream();
            ZDeflaterOutputStream zip = new ZDeflaterOutputStream(stream, compressionLevel);
            zip.Write(data, 0, data.Length);
            zip.Close();
            bytes = stream.ToArray();
            this.compressionLevel = compressionLevel;
            Put(PdfName.FILTER, PdfName.FLATEDECODE);
        }
        else
            bytes = data;
        Length = bytes.Length;
    }

    /**
     * Sets the data associated with the stream, as-is.  This method will not
     * remove or change any existing filter: the data has to match an existing
     * filter or an appropriate filter has to be set.
     *
     * @param data data, possibly encrypted and/or compressed
     * @since 5.5.0
     */
    virtual public void SetDataRaw(byte[] data)
    {
        this.offset = -1;
        bytes = data;
        Length = bytes.Length;
    }


    /**Sets the data associated with the stream
     * @param data raw data, decrypted and uncompressed.
     */
    virtual public void SetData(byte[] data) {
        SetData(data, true);
    }

    public new int Length {
        set {
            length = value;
            Put(PdfName.LENGTH, new PdfNumber(length));
        }
        get {
            return length;
        }
    }
    
	virtual public long Offset {
        get {
            return offset;
        }
    }
    
    virtual public PdfReader Reader {
        get {
            return reader;
        }
    }
    
    public new byte[] GetBytes() {
        return bytes;
    }
    
    virtual public int ObjNum {
        get {
            return objNum;
        }
        set {
            objNum = value;
        }
    }
    
    virtual public int ObjGen {
        get {
            return objGen;
        }
        set {
            objGen = value;
        }
    }
    
    public override void ToPdf(PdfWriter writer, Stream os) {
        byte[] b = PdfReader.GetStreamBytesRaw(this);
        PdfEncryption crypto = null;
        if (writer != null)
            crypto = writer.Encryption;
        PdfObject objLen = Get(PdfName.LENGTH);
        int nn = b.Length;
        if (crypto != null)
            nn = crypto.CalculateStreamSize(nn);
        Put(PdfName.LENGTH, new PdfNumber(nn));
        SuperToPdf(writer, os);
        Put(PdfName.LENGTH, objLen);
        os.Write(STARTSTREAM, 0, STARTSTREAM.Length);
        if (length > 0) {
            if (crypto != null && !crypto.IsEmbeddedFilesOnly())
                b = crypto.EncryptByteArray(b);
            os.Write(b, 0, b.Length);
        }
        os.Write(ENDSTREAM, 0, ENDSTREAM.Length);
    }
}
}
