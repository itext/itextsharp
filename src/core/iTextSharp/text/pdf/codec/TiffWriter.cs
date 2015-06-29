using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
/*
 * $Id: TiffWriter.java 4509 2010-05-13 23:06:03Z psoares33 $
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
namespace iTextSharp.text.pdf.codec {
    /**
     *
     * @author psoares
     */
    public class TiffWriter {
        private SortedDictionary<int,FieldBase> ifd = new SortedDictionary<int,FieldBase>();

        virtual public void AddField(FieldBase field) {
            ifd[field.GetTag()] = field;
        }

        virtual public int GetIfdSize() {
            return 6 + ifd.Count * 12;
        }

        virtual public void WriteFile(Stream stream) {
            stream.WriteByte(0x4d);
            stream.WriteByte(0x4d);
            stream.WriteByte(0);
            stream.WriteByte(42);
            WriteLong(8, stream);
            WriteShort(ifd.Count, stream);
            int offset = 8 + GetIfdSize();
            foreach (FieldBase field in ifd.Values) {
                int size = field.GetValueSize();
                if (size > 4) {
                    field.SetOffset(offset);
                    offset += size;
                }
                field.WriteField(stream);
            }
            WriteLong(0, stream);
            foreach (FieldBase field in ifd.Values) {
                field.WriteValue(stream);
            }
        }

        public abstract class FieldBase {
            private int tag;
            private int fieldType;
            private int count;
            protected byte[] data;
            private int offset;

            protected FieldBase(int tag, int fieldType, int count) {
                this.tag = tag;
                this.fieldType = fieldType;
                this.count = count;
            }

            virtual public int GetValueSize() {
                return (int)((data.Length + 1) & 0xfffffffe);
            }

            virtual public int GetTag() {
                return tag;
            }

            virtual public void SetOffset(int offset) {
                this.offset = offset;
            }

            virtual public void WriteField(Stream stream) {
                WriteShort(tag, stream);
                WriteShort(fieldType, stream);
                WriteLong(count, stream);
                if (data.Length <= 4) {
                    stream.Write(data, 0, data.Length);
                    for (int k = data.Length; k < 4; ++k) {
                        stream.WriteByte(0);
                    }
                }
                else {
                    WriteLong(offset, stream);
                }
            }

            virtual public void WriteValue(Stream stream) {
                if (data.Length <= 4)
                    return;
                stream.Write(data, 0, data.Length);
                if ((data.Length & 1) == 1)
                    stream.WriteByte(0);
            }
        }

        public class FieldShort : FieldBase {
            public FieldShort(int tag, int value) : base(tag, 3, 1) {
                data = new byte[2];
                data[0] = (byte)(value >> 8);
                data[1] = (byte)value;
            }

            public FieldShort(int tag, int[] values) : base(tag, 3, values.Length) {
                data = new byte[values.Length * 2];
                int ptr = 0;
                foreach (int value in values) {
                    data[ptr++] = (byte)(value >> 8);
                    data[ptr++] = (byte)value;
                }
            }
        }

        public class FieldLong : FieldBase {
            public FieldLong(int tag, int value) : base(tag, 4, 1) {
                data = new byte[4];
                data[0] = (byte)(value >> 24);
                data[1] = (byte)(value >> 16);
                data[2] = (byte)(value >> 8);
                data[3] = (byte)value;
            }

            public FieldLong(int tag, int[] values) : base(tag, 4, values.Length) {
                data = new byte[values.Length * 4];
                int ptr = 0;
                foreach (int value in values) {
                    data[ptr++] = (byte)(value >> 24);
                    data[ptr++] = (byte)(value >> 16);
                    data[ptr++] = (byte)(value >> 8);
                    data[ptr++] = (byte)value;
                }
            }
        }

        public class FieldRational : FieldBase {
            public FieldRational(int tag, int[] value) : this(tag, new int[][]{value}) {
            }

            public FieldRational(int tag, int[][] values) : base(tag, 5, values.Length) {
                data = new byte[values.Length * 8];
                int ptr = 0;
                foreach (int[] value in values) {
                    data[ptr++] = (byte)(value[0] >> 24);
                    data[ptr++] = (byte)(value[0] >> 16);
                    data[ptr++] = (byte)(value[0] >> 8);
                    data[ptr++] = (byte)value[0];
                    data[ptr++] = (byte)(value[1] >> 24);
                    data[ptr++] = (byte)(value[1] >> 16);
                    data[ptr++] = (byte)(value[1] >> 8);
                    data[ptr++] = (byte)value[1];
                }
            }
        }

        public class FieldByte : FieldBase {
            public FieldByte(int tag, byte[] values) : base(tag, 1, values.Length) {
                data = values;
            }
        }

        public class FieldUndefined : FieldBase {
            public FieldUndefined(int tag, byte[] values) : base(tag, 7, values.Length) {
                data = values;
            }
        }

        public class FieldImage : FieldBase {
            public FieldImage(byte[] values) : base(TIFFConstants.TIFFTAG_STRIPOFFSETS, 4, 1) {
                data = values;
            }
        }

        public class FieldAscii : FieldBase {
            public FieldAscii(int tag, String values) : base(tag, 2, Encoding.ASCII.GetBytes(values).Length + 1) {
                byte[] b = Encoding.ASCII.GetBytes(values);
                data = new byte[b.Length + 1];
                System.Array.Copy(b, 0, data, 0, b.Length);
            }
        }

        public static void WriteShort(int v, Stream stream) {
            stream.WriteByte((byte)((v >> 8) & 0xff));
            stream.WriteByte((byte)(v & 0xff));
        }

        public static void WriteLong(int v, Stream stream) {
            stream.WriteByte((byte)((v >> 24) & 0xff));
            stream.WriteByte((byte)((v >> 16) & 0xff));
            stream.WriteByte((byte)((v >> 8) & 0xff));
            stream.WriteByte((byte)(v & 0xff));
        }

        public static void CompressLZW(Stream stream, int predictor, byte[] b, int height, int samplesPerPixel, int stride) {

            LZWCompressor lzwCompressor = new LZWCompressor(stream, 8, true);
            bool usePredictor = predictor == TIFFConstants.PREDICTOR_HORIZONTAL_DIFFERENCING;

            if (!usePredictor) {
                lzwCompressor.Compress(b, 0, b.Length);
            } else {
                int off = 0;
                byte[] rowBuf = usePredictor ? new byte[stride] : null;
                for (int i = 0; i < height; i++) {
                    System.Array.Copy(b, off, rowBuf, 0, stride);
                    for (int j = stride - 1; j >= samplesPerPixel; j--) {
                        rowBuf[j] -= rowBuf[j - samplesPerPixel];
                    }
                    lzwCompressor.Compress(rowBuf, 0, stride);
                    off += stride;
                }
            }

            lzwCompressor.Flush();
        }
    }
}
