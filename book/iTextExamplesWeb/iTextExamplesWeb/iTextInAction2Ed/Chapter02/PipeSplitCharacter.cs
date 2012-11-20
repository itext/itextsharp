/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

/**
 * Writes a list of countries to a PDF file.
 */
namespace kuujinbo.iTextInAction2Ed.Chapter02 {
  public class PipeSplitCharacter : ISplitCharacter {
// ===========================================================================
    /**
     * @see com.itextpdf.text.SplitCharacter#isSplitCharacter(int, int, int, char[],
     *      com.itextpdf.text.pdf.PdfChunk[])
     */
    public bool IsSplitCharacter(
      int start, int current, int end, char[] cc, PdfChunk[] ck) 
    {
      char c = ck == null
        ? cc[current]
        : (char) ck[Math.Min(current, ck.Length - 1)]
            .GetUnicodeEquivalent(cc[current])
      ;
      return (c == '|' || c <= ' ' || c == '-');
    }
// ===========================================================================
  }
}