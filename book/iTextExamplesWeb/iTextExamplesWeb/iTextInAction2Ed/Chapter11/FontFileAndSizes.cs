/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using System.Collections.Generic;

using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter11 {
  public class FontFileAndSizes : IWriter {
// ===========================================================================
    /** The names of the resulting PDF files. */
    public string[] RESULT {
      get { return new string[] {
          "font_not_embedded.pdf",
          "font_embedded.pdf",
          "font_embedded_less_glyphs.pdf",
          "font_compressed.pdf",
          "font_full.pdf"
        };
      }
    }
    /** The path to the font. */
    public const string FONT = "c:/windows/fonts/arial.ttf";
    /** Some text. */
    public const string TEXT
        = "quick brown fox jumps over the lazy dog";
    /** Some text. */
    public const string OOOO
        = "ooooo ooooo ooo ooooo oooo ooo oooo ooo";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        FontFileAndSizes ffs = new FontFileAndSizes();
        BaseFont bf;
        bf = BaseFont.CreateFont(FONT, BaseFont.WINANSI, BaseFont.NOT_EMBEDDED);
        zip.AddEntry(RESULT[0], ffs.CreatePdf(bf, TEXT));
        bf = BaseFont.CreateFont(FONT, BaseFont.WINANSI, BaseFont.EMBEDDED);
        zip.AddEntry(RESULT[1], ffs.CreatePdf(bf, TEXT));
        zip.AddEntry(RESULT[2], ffs.CreatePdf(bf, OOOO));
        bf = BaseFont.CreateFont(FONT, BaseFont.WINANSI, BaseFont.EMBEDDED);
        bf.CompressionLevel = 9;
        zip.AddEntry(RESULT[3], ffs.CreatePdf(bf, TEXT));
        bf = BaseFont.CreateFont(FONT, BaseFont.WINANSI, BaseFont.EMBEDDED);
        bf.Subset = false;
        zip.AddEntry(RESULT[4], ffs.CreatePdf(bf, TEXT)); 
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a PDF document.
     */
    public byte[] CreatePdf(BaseFont bf, String text) {
      using (MemoryStream ms = new MemoryStream()) {
        using (Document document = new Document()) {
          PdfWriter.GetInstance(document, ms);
          document.Open();
          document.Add(new Paragraph(text, new Font(bf, 12)));
        }
        return ms.ToArray();
      }
    }    
// ===========================================================================
  }
}