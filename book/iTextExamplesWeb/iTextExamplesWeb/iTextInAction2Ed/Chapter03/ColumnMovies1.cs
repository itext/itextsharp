/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter03 {
  public class ColumnMovies1 : IWriter {
// ===========================================================================
    public readonly string RESOURCE = Utility.ResourcePosters;
    /** Definition of two columns */
    public readonly float[,] COLUMNS = new float[,]  {
      { 36, 36, 224, 579 } , { 230, 36, 418, 579 },
      { 424, 36, 612, 579 } , { 618, 36, 806, 579 }
    };
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4.Rotate())) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        ColumnText ct = new ColumnText(writer.DirectContent);
        int column = 0;
        ct.SetSimpleColumn(
          COLUMNS[column,0], COLUMNS[column,1],
          COLUMNS[column,2], COLUMNS[column,3]
        );
        // iText-ONLY, 'Initial value of the status' => 0
        // iTextSharp **DOES NOT** include this member variable
        // int status = ColumnText.START_COLUMN;
        int status = 0;
        float y;
        Image img;
        IEnumerable<Movie> movies = PojoFactory.GetMovies();
        foreach (Movie movie in movies) {
          y = ct.YLine;
          img = Image.GetInstance(
            Path.Combine(RESOURCE, movie.Imdb + ".jpg")
          );
          img.ScaleToFit(80, 1000);
          AddContent(ct, movie, img);
          status = ct.Go(true);
          if (ColumnText.HasMoreText(status)) {
            column = (column + 1) % 4;
            if (column == 0) {
              document.NewPage();
            }
            ct.SetSimpleColumn(
              COLUMNS[column, 0], COLUMNS[column, 1],
              COLUMNS[column, 2], COLUMNS[column, 3]
            );
            y = COLUMNS[column, 3];
          }
          ct.YLine = y;
          ct.SetText(null);
          AddContent(ct, movie, img);
          status = ct.Go();
        }
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Add content to a ColumnText object.
     * @param ct the ColumnText object
     * @param movie a Movie object
     * @param img the poster of the image
     */
    public void AddContent(ColumnText ct, Movie movie, Image img) {
      ct.AddElement(img);
      ct.AddElement(new Paragraph(movie.Title, FilmFonts.BOLD));
      if (!string.IsNullOrEmpty(movie.OriginalTitle)) {
        ct.AddElement(new Paragraph(movie.OriginalTitle, FilmFonts.ITALIC));
      }
      ct.AddElement(PojoToElementFactory.GetDirectorList(movie));
      ct.AddElement(PojoToElementFactory.GetYearPhrase(movie));
      ct.AddElement(PojoToElementFactory.GetDurationPhrase(movie));
      ct.AddElement(PojoToElementFactory.GetCountryList(movie));
      ct.AddElement(Chunk.NEWLINE);
    }    
// ===========================================================================
  }
}