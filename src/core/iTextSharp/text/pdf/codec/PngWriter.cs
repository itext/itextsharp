using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using iTextSharp.text;
using System.util.zlib;

namespace iTextSharp.text.pdf.codec {
    public class PngWriter {
        private static readonly byte[] PNG_SIGNTURE = {(byte)137, 80, 78, 71, 13, 10, 26, 10};

        private static readonly byte[] IHDR = DocWriter.GetISOBytes("IHDR");
        private static readonly byte[] PLTE = DocWriter.GetISOBytes("PLTE");
        private static readonly byte[] IDAT = DocWriter.GetISOBytes("IDAT");
        private static readonly byte[] IEND = DocWriter.GetISOBytes("IEND");
        private static readonly byte[] iCCP = DocWriter.GetISOBytes("iCCP");

        private static uint[] crc_table;

        private Stream outp;

        public PngWriter(Stream outp) {
            this.outp = outp;
            outp.Write(PNG_SIGNTURE, 0, PNG_SIGNTURE.Length);
        }

        public void WriteHeader(int width, int height, int bitDepth, int colorType) {
            MemoryStream ms = new MemoryStream();
            OutputInt(width, ms);
            OutputInt(height, ms);
            ms.WriteByte((byte)bitDepth);
            ms.WriteByte((byte)colorType);
            ms.WriteByte(0);
            ms.WriteByte(0);
            ms.WriteByte(0);
            WriteChunk(IHDR, ms.ToArray());
        }

        public void WriteEnd() {
            WriteChunk(IEND, new byte[0]);
        }

        public void WriteData(byte[] data, int stride) {
            MemoryStream stream = new MemoryStream();
            ZDeflaterOutputStream zip = new ZDeflaterOutputStream(stream, 5);
            for (int k = 0; k < data.Length; k += stride) {
                zip.WriteByte(0);
                zip.Write(data, k, stride);
            }
            zip.Finish();
            WriteChunk(IDAT, stream.ToArray());
        }

        public void WritePalette(byte[] data) {
            WriteChunk(PLTE, data);
        }

        public void WriteIccProfile(byte[] data) {
            MemoryStream stream = new MemoryStream();
            stream.WriteByte((byte)'I');
            stream.WriteByte((byte)'C');
            stream.WriteByte((byte)'C');
            stream.WriteByte(0);
            stream.WriteByte(0);
            ZDeflaterOutputStream zip = new ZDeflaterOutputStream(stream, 5);
            zip.Write(data, 0, data.Length);
            zip.Finish();
            WriteChunk(iCCP, stream.ToArray());
        }

        private static void make_crc_table() {
            if (crc_table != null)
                return;
            uint[] crc2 = new uint[256];
            for (uint n = 0; n < 256; n++) {
                uint c = n;
                for (int k = 0; k < 8; k++) {
                    if ((c & 1) != 0)
                        c = 0xedb88320U ^ (c >> 1);
                    else
                        c = c >> 1;
                }
                crc2[n] = c;
            }
            crc_table = crc2;
        }

        private static uint update_crc(uint crc, byte[] buf, int offset, int len) {
            uint c = crc;

            if (crc_table == null)
                make_crc_table();
            for (int n = 0; n < len; n++) {
                c = crc_table[(c ^ buf[n + offset]) & 0xff] ^ (c >> 8);
            }
            return c;
        }

        private static uint crc(byte[] buf, int offset, int len) {
            return update_crc(0xffffffffU, buf, offset, len) ^ 0xffffffffU;
        }

        private static uint crc(byte[] buf) {
            return update_crc(0xffffffffU, buf, 0, buf.Length) ^ 0xffffffffU;
        }

        public void OutputInt(int n) {
            OutputInt(n, outp);
        }

        public static void OutputInt(int n, Stream s) {
            s.WriteByte((byte)(n >> 24));
            s.WriteByte((byte)(n >> 16));
            s.WriteByte((byte)(n >> 8));
            s.WriteByte((byte)n);
        }

        public void WriteChunk(byte[] chunkType, byte[] data) {
            OutputInt(data.Length);
            outp.Write(chunkType, 0, 4);
            outp.Write(data, 0, data.Length);
            OutputInt((int)crc(data));
        }
    }
}
