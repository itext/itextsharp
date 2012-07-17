using System;
using System.Collections.Generic;
using iTextSharp.text;

namespace iTextSharp.text.pdf{

    public class TtfUnicodeWriter {

        protected PdfWriter writer = null;

        public TtfUnicodeWriter(PdfWriter writer) {
            this.writer = writer;
        }

        public virtual void WriteFont(TrueTypeFontUnicode font, PdfIndirectReference refer, Object[] parms, byte[] rotbits)
        {
            Dictionary<int, int[]> longTag = (Dictionary<int, int[]>)parms[0];
            font.AddRangeUni(longTag, true, font.subset);
            int[][] metrics = new int[0][];
            longTag.Values.CopyTo(metrics,0);
            Array.Sort(metrics, font);
            PdfIndirectReference ind_font = null;
            PdfObject pobj = null;
            PdfIndirectObject obj = null;
            PdfIndirectReference cidset = null;
            // sivan: cff
            if (font.cff) {
                byte[] b = font.ReadCffFont();
                if (font.subset || font.subsetRanges != null) {
                    CFFFontSubset cff = new CFFFontSubset(new RandomAccessFileOrArray(b),longTag);
                    b = cff.Process(cff.GetNames()[0]);
                }
                pobj = new BaseFont.StreamFont(b, "CIDFontType0C", font.compressionLevel);
                obj = writer.AddToBody(pobj);
                ind_font = obj.IndirectReference;
            } else {
                byte[] b;
                if (font.subset || font.directoryOffset != 0) {
                    TrueTypeFontSubSet sb = new TrueTypeFontSubSet(font.fileName, new RandomAccessFileOrArray(font.rf), longTag, font.directoryOffset, false, false);
                    b = sb.Process();
                }
                else {
                    b = font.GetFullFont();
                }
                int [] lengths = new int[]{b.Length};
                pobj = new BaseFont.StreamFont(b, lengths, font.compressionLevel);
                obj = writer.AddToBody(pobj);
                ind_font = obj.IndirectReference;
            }
            String subsetPrefix = "";
            if (font.subset)
                subsetPrefix = TrueTypeFontUnicode.CreateSubsetPrefix();
            PdfDictionary dic = font.GetFontDescriptor(ind_font, subsetPrefix, cidset);
            obj = writer.AddToBody(dic);
            ind_font = obj.IndirectReference;

            pobj = font.GetCIDFontType2(ind_font, subsetPrefix, metrics);
            obj = writer.AddToBody(pobj);
            ind_font = obj.IndirectReference;

            pobj = font.GetToUnicode(metrics);
            PdfIndirectReference toUnicodeRef = null;

            if (pobj != null) {
                obj = writer.AddToBody(pobj);
                toUnicodeRef = obj.IndirectReference;
            }

            pobj = font.GetFontBaseType(ind_font, subsetPrefix, toUnicodeRef);
            writer.AddToBody(pobj, refer);
        }
    }
}