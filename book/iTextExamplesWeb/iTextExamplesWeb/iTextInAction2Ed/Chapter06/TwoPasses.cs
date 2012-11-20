/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using System.Data;
using System.Data.Common;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class TwoPasses : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const String RESULT = "page_x_of_y.pdf";
// ---------------------------------------------------------------------------         
    public void Write(Stream stream) {
      string SQL = "SELECT country, id FROM film_country ORDER BY country";
      using (ZipFile zip = new ZipFile()) {
        using (var ms = new MemoryStream()) {
          // FIRST PASS, CREATE THE PDF WITHOUT HEADER
          // step 1
          using (Document document = new Document(PageSize.A4, 36, 36, 54, 36)) {
            // step 2
            PdfWriter.GetInstance(document, ms);
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
                    document.Add(new Paragraph(
                      r["country"].ToString(), FilmFonts.BOLD
                    ));
                    document.Add(Chunk.NEWLINE);          
                    string id = r["id"].ToString();
                    foreach (Movie movie in PojoFactory.GetMovies(id, true)) {
                      document.Add(new Paragraph(
                        movie.MovieTitle, FilmFonts.BOLD
                      ));
                      if (!string.IsNullOrEmpty(movie.OriginalTitle)) {
                        document.Add(new Paragraph(
                          movie.OriginalTitle, FilmFonts.ITALIC
                        ));
                      }
                      document.Add(new Paragraph(string.Format(
                          "Year: {0}; run length: {0} minutes",
                          movie.Year, movie.Duration
                        ), 
                        FilmFonts.NORMAL
                      ));
                      document.Add(PojoToElementFactory.GetDirectorList(movie));
                    }
                    document.NewPage();              
                  }
                }
              }
            }
          }
          byte[] firstPass = ms.ToArray();
          zip.AddEntry("first-pass.pdf", firstPass);

          // SECOND PASS, ADD THE HEADER
          // Create a reader
          PdfReader reader = new PdfReader(firstPass);
          using (MemoryStream ms2 = new MemoryStream()) {
            // Create a stamper
            using (PdfStamper stamper = new PdfStamper(reader, ms2)) {
              // Loop over the pages and add a header to each page
              int n = reader.NumberOfPages;
              for (int i = 1; i <= n; i++) {
                GetHeaderTable(i, n).WriteSelectedRows(
                  0, -1, 34, 803, stamper.GetOverContent(i)
                );
              }
            }
            zip.AddEntry(RESULT, ms2.ToArray());
          }
        }
        zip.Save(stream);
      }        
    }
// ---------------------------------------------------------------------------    
    /**
     * Create a header table with page X of Y
     * @param x the page number
     * @param y the total number of pages
     * @return a table that can be used as header
     */
    public static PdfPTable GetHeaderTable(int x, int y) {
      PdfPTable table = new PdfPTable(2);
      table.TotalWidth = 527;
      table.LockedWidth = true;
      table.DefaultCell.FixedHeight = 20;
      table.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
      table.AddCell("FOOBAR FILMFESTIVAL");
      table.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
      table.AddCell(string.Format("Page {0} of {1}", x, y));
      return table;
    }    
// ===========================================================================
  }
}