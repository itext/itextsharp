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
using iTextSharp.text.pdf.draw;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter03 {
  public class MovieColumns1 : IWriter {
// ===========================================================================
    public virtual void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        IEnumerable<Movie> movies = PojoFactory.GetMovies();
        ColumnText ct = new ColumnText(writer.DirectContent);
        foreach (Movie movie in movies) {
          ct.AddText(CreateMovieInformation(movie));
          ct.AddText(Chunk.NEWLINE);
        }
        ct.Alignment = Element.ALIGN_JUSTIFIED;
        ct.ExtraParagraphSpace = 6;
        ct.SetLeading(0, 1.2f);
        ct.FollowingIndent = 27;
        int linesWritten = 0;
        int column = 0;
        // iText-ONLY, 'Initial value of the status' => 0
        // iTextSharp **DOES NOT** include this member variable
        // int status = ColumnText.START_COLUMN;
        int status = 0;
        while (ColumnText.HasMoreText(status)) {
          ct.SetSimpleColumn(
            COLUMNS[column][0], COLUMNS[column][1],
            COLUMNS[column][2], COLUMNS[column][3]
          );
          ct.YLine = COLUMNS[column][3];
          status = ct.Go();
          linesWritten += ct.LinesWritten;
          column = Math.Abs(column - 1);
          if (column == 0) document.NewPage();
        }
        
        ct.AddText(new Phrase("Lines written: " + linesWritten));
        ct.Go();
      }
    }
// ---------------------------------------------------------------------------    
    /** Definition of two columns */
    public readonly float[][] COLUMNS = {
      new float[] { 36, 36, 296, 806 },
      new float[] { 299, 36, 559, 806 }
    };
// ---------------------------------------------------------------------------    
    /**
     * Creates a Phrase containing information about a movie.
     * @param    movie    the movie for which you want to create a Paragraph
     */
    public Phrase CreateMovieInformation(Movie movie) {
      Phrase p = new Phrase();
      p.Font = FilmFonts.NORMAL;
      p.Add(new Phrase("Title: ", FilmFonts.BOLDITALIC));
      p.Add(PojoToElementFactory.GetMovieTitlePhrase(movie));
      p.Add(" ");
      if (!string.IsNullOrEmpty(movie.OriginalTitle)) {
        p.Add(new Phrase("Original title: ", FilmFonts.BOLDITALIC));
        p.Add(PojoToElementFactory.GetOriginalTitlePhrase(movie));
        p.Add(" ");
      }
      p.Add(new Phrase("Country: ", FilmFonts.BOLDITALIC));
      foreach (Country country in movie.Countries) {
        p.Add(PojoToElementFactory.GetCountryPhrase(country));
        p.Add(" ");
      }
      p.Add(new Phrase("Director: ", FilmFonts.BOLDITALIC));
      foreach (Director director in movie.Directors) {
        p.Add(PojoToElementFactory.GetDirectorPhrase(director));
        p.Add(" ");
      }
      p.Add(new Chunk("Year: ", FilmFonts.BOLDITALIC));
      p.Add(new Chunk(movie.Year.ToString(), FilmFonts.NORMAL));
      p.Add(new Chunk(" Duration: ", FilmFonts.BOLDITALIC));
      p.Add(new Chunk(movie.Duration.ToString(), FilmFonts.NORMAL));
      p.Add(new Chunk(" minutes", FilmFonts.NORMAL));
      p.Add(new LineSeparator(0.3f, 100, null, Element.ALIGN_CENTER, -2));
      return p;
    }    
// ===========================================================================
  }
}