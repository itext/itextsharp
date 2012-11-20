/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter05 {
  public class MovieCountries1 : IWriter {
// ===========================================================================
    /**
     * Inner class to add a table as header.
     */
    protected class TableHeader : PdfPageEventHelper {
      /** The template with the total number of pages. */
      PdfTemplate total;
      
      /** The header text. */
      public string Header { get; set; }

      /**
       * Creates the PdfTemplate that will hold the total number of pages.
       * @see com.itextpdf.text.pdf.PdfPageEventHelper#onOpenDocument(
       *      com.itextpdf.text.pdf.PdfWriter, com.itextpdf.text.Document)
       */
      public override void OnOpenDocument(PdfWriter writer, Document document) {
        total = writer.DirectContent.CreateTemplate(30, 16);
      }
      
      /**
       * Adds a header to every page
       * @see com.itextpdf.text.pdf.PdfPageEventHelper#onEndPage(
       *      com.itextpdf.text.pdf.PdfWriter, com.itextpdf.text.Document)
       */
      public override void OnEndPage(PdfWriter writer, Document document) {
        PdfPTable table = new PdfPTable(3);
        try {
          table.SetWidths(new int[]{24, 24, 2});
          table.TotalWidth = 527;
          table.LockedWidth = true;
          table.DefaultCell.FixedHeight = 20;
          table.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
          table.AddCell(Header);
          table.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
          table.AddCell(string.Format("Page {0} of", writer.PageNumber));
          PdfPCell cell = new PdfPCell(Image.GetInstance(total));
          cell.Border = Rectangle.BOTTOM_BORDER;
          table.AddCell(cell);
          table.WriteSelectedRows(0, -1, 34, 803, writer.DirectContent);
        }
        catch(DocumentException de) {
          throw de;
        }
      }
      
      /**
       * Fills out the total number of pages before the document is closed.
       * @see com.itextpdf.text.pdf.PdfPageEventHelper#onCloseDocument(
       *      com.itextpdf.text.pdf.PdfWriter, com.itextpdf.text.Document)
       */
      public override void OnCloseDocument(PdfWriter writer, Document document) {
        ColumnText.ShowTextAligned(
          total, Element.ALIGN_LEFT,
/*
 * NewPage() already called when closing the document; subtract 1
*/
          new Phrase((writer.PageNumber - 1).ToString()),
          2, 2, 0
        );
      }
    }
// ---------------------------------------------------------------------------
    protected readonly string SQL = 
@"SELECT country, id FROM film_country 
ORDER BY country
"; 
    public virtual void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4, 36, 36, 54, 36)) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        TableHeader tevent = new TableHeader();
        writer.PageEvent = tevent;
        // step 3
        document.Open();
        // step 4       
        using (var c =  AdoDB.Provider.CreateConnection()) {
          c.ConnectionString = AdoDB.CS;
          using (DbCommand cmd = c.CreateCommand()) {
            cmd.CommandText = SQL;        
            c.Open();
            using (var r = cmd.ExecuteReader()) {
              while (r.Read()) {
                tevent.Header = r["country"].ToString();
                // LINQ allows us to sort on any movie object property inline;
                // let's sort by Movie.Year, Movie.Title
                var by_year = from m in PojoFactory.GetMovies(
                  r["id"].ToString()
                )
                  orderby m.Year, m.Title
                  select m
                ;                
                foreach (Movie movie in by_year) {
                  document.Add(new Paragraph(
                    movie.MovieTitle, FilmFonts.BOLD
                  ));
                  if (!string.IsNullOrEmpty(movie.OriginalTitle)) document.Add(
                    new Paragraph(movie.OriginalTitle, FilmFonts.ITALIC)
                  );
                  document.Add(new Paragraph(
                    String.Format("Year: {0}; run length: {1} minutes",
                      movie.Year, movie.Duration
                    ), FilmFonts.NORMAL
                  ));
                  document.Add(PojoToElementFactory.GetDirectorList(movie));
                }
                document.NewPage();              
              }      
            }
          }
        }
      }
    }    
// ===========================================================================
  }
}