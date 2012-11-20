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
using kuujinbo.iTextInAction2Ed.Intro_1_2;
using kuujinbo.iTextInAction2Ed.Chapter02;

namespace kuujinbo.iTextInAction2Ed.Chapter13 {
  public class PageLayoutExample : MovieParagraphs1 {
// ===========================================================================
    /** The resulting PDF file. */
    public const string RESULT1 = "page_layout_single.pdf";
    /** The resulting PDF file. */
    public const string RESULT2 = "page_layout_column.pdf";
    /** The resulting PDF file. */
    public const string RESULT3 = "page_layout_columns_l.pdf";
    /** The resulting PDF file. */
    public const string RESULT4 = "page_layout_columns_r.pdf";
    /** The resulting PDF file. */
    public const string RESULT5 = "page_layout_pages_l.pdf";
    /** The resulting PDF file. */
    public const string RESULT6 = "page_layout_pages_r.pdf";
// --------------------------------------------------------------------------- 
    public override void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        PageLayoutExample example = new PageLayoutExample();
        zip.AddEntry(RESULT1, example.CreatePdf(PdfWriter.PageLayoutSinglePage));
        zip.AddEntry(RESULT2, example.CreatePdf(PdfWriter.PageLayoutOneColumn));
        zip.AddEntry(RESULT3, example.CreatePdf(PdfWriter.PageLayoutTwoColumnLeft));
        zip.AddEntry(RESULT4, example.CreatePdf(PdfWriter.PageLayoutTwoColumnRight));
        zip.AddEntry(RESULT5, example.CreatePdf(PdfWriter.PageLayoutTwoPageLeft));
        zip.AddEntry(RESULT6, example.CreatePdf(PdfWriter.PageLayoutTwoPageRight));      
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------        
    /**
     * Creates a PDF with information about the movies
     * @param pagelayout the value for the viewerpreferences
     */
    public byte[] CreatePdf(int viewerpreference) {
      using (MemoryStream ms = new MemoryStream()) {
        // step 1
        using (Document document = new Document()) {
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          writer.PdfVersion = PdfWriter.VERSION_1_5;
          writer.ViewerPreferences = viewerpreference;
          // step 3
          document.Open();
          // step 4
          foreach (Movie movie in PojoFactory.GetMovies()) {
            Paragraph p = CreateMovieInformation(movie);
            p.Alignment = Element.ALIGN_JUSTIFIED;
            p.IndentationLeft = 18;
            p.FirstLineIndent = -18;
            document.Add(p);
          }
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}