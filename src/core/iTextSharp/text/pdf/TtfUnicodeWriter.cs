using System;
using System.Collections.Generic;
using iTextSharp.text;

namespace iTextSharp.text.pdf{

    public class TtfUnicodeWriter
    {

        protected PdfWriter writer = null;

        public TtfUnicodeWriter(PdfWriter writer)
        {
            this.writer = writer;
        }

        internal protected virtual void WriteFont(TrueTypeFontUnicode font, PdfIndirectReference refer, Object[] parms, byte[] rotbits)
        {
            Dictionary<int, int[]> longTag = (Dictionary<int, int[]>)parms[0];
            font.AddRangeUni(longTag, true, font.Subset);
            int[][] metrics = new int[longTag.Count][];
            longTag.Values.CopyTo(metrics, 0);
            Array.Sort(metrics, font);
            PdfIndirectReference ind_font = null;
            PdfObject pobj = null;
            PdfIndirectObject obj = null;
            PdfIndirectReference cidset = null;
            // sivan: cff
            if (font.Cff)
            {
                byte[] b = font.ReadCffFont();
                if (font.Subset || font.SubsetRanges != null)
                {
                    CFFFontSubset cff = new CFFFontSubset(new RandomAccessFileOrArray(b), longTag);
                    b = cff.Process(cff.GetNames()[0]);
                }
                pobj = new BaseFont.StreamFont(b, "CIDFontType0C", font.CompressionLevel);
                obj = writer.AddToBody(pobj);
                ind_font = obj.IndirectReference;
            }
            else
            {
                byte[] b;
                if (font.Subset || font.DirectoryOffset != 0)
                {
                    b = font.GetSubSet(longTag, true);
                }
                else
                {
                    b = font.GetFullFont();
                }
                int[] lengths = new int[] { b.Length };
                pobj = new BaseFont.StreamFont(b, lengths, font.CompressionLevel);
                obj = writer.AddToBody(pobj);
                ind_font = obj.IndirectReference;
            }
            String subsetPrefix = "";
            if (font.Subset)
                subsetPrefix = TrueTypeFontUnicode.CreateSubsetPrefix();
            PdfDictionary dic = font.GetFontDescriptor(ind_font, subsetPrefix, cidset);
            obj = writer.AddToBody(dic);
            ind_font = obj.IndirectReference;

            pobj = font.GetCIDFontType2(ind_font, subsetPrefix, metrics);
            obj = writer.AddToBody(pobj);
            ind_font = obj.IndirectReference;

            pobj = font.GetToUnicode(metrics);
            PdfIndirectReference toUnicodeRef = null;

            if (pobj != null)
            {
                obj = writer.AddToBody(pobj);
                toUnicodeRef = obj.IndirectReference;
            }

            pobj = font.GetFontBaseType(ind_font, subsetPrefix, toUnicodeRef);
            writer.AddToBody(pobj, refer);
        }


    }
}