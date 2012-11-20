/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter04 {
  public class NestedTables : IWriter {
// ===========================================================================
    /** Collection containing all the Images */
    public Dictionary<String, Image> images = new Dictionary<String, Image>();
    /** Path to the resources. */
    public readonly string RESOURCE = Utility.ResourcePosters;
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        List<string> days = PojoFactory.GetDays();
        foreach (string day in days) {
          document.Add(GetTable(day));
          document.NewPage();
        }
      }
    }
// ---------------------------------------------------------------------------   
    /**
     * Creates a table with all the screenings of a specific day.
     * The table is composed of nested tables.
     * @param day a film festival day
     * @return a table
     */
    public PdfPTable GetTable(string day) {
      // Create a table with only one column
      PdfPTable table = new PdfPTable(1);
      table.WidthPercentage = 100f;
      // add the cell with the date
      Font f = new Font();
      f.Color = BaseColor.WHITE;
      PdfPCell cell = new PdfPCell(new Phrase(day, f));
      cell.BackgroundColor = BaseColor.BLACK;
      cell.HorizontalAlignment = Element.ALIGN_CENTER;
      table.AddCell(cell);
      // add the movies as nested tables
      List<Screening> screenings = PojoFactory.GetScreenings(day);
      foreach (Screening screening in screenings) {
        table.AddCell(GetTable(screening));
      }
      return table;
    }
// ---------------------------------------------------------------------------   
    /**
     * Create a table with information about a movie.
     * @param screening a Screening
     * @return a table
     */
    private PdfPTable GetTable(Screening screening) {
      // Create a table with 4 columns
      PdfPTable table = new PdfPTable(4);
      table.SetWidths(new int[]{1, 5, 10, 10});
      // Get the movie
      Movie movie = screening.movie;
      // A cell with the title as a nested table spanning the complete row
      PdfPCell cell = new PdfPCell();
      // nesting is done with addElement() in this example
      cell.AddElement(FullTitle(screening));
      cell.Colspan = 4;
      cell.Border = PdfPCell.NO_BORDER;
      BaseColor color = WebColors.GetRGBColor(
        "#" + movie.entry.category.color
      );
      cell.BackgroundColor = color;
      table.AddCell(cell);
      // empty cell
      cell = new PdfPCell();
      cell.Border = PdfPCell.NO_BORDER;
      cell.UseAscender = true;
      cell.UseDescender = true;
      table.AddCell(cell);
      // cell with the movie poster
      cell = new PdfPCell(GetImage(movie.Imdb));
      cell.Border = PdfPCell.NO_BORDER;
      table.AddCell(cell);
      // cell with the list of directors
      cell = new PdfPCell();
      cell.AddElement(PojoToElementFactory.GetDirectorList(movie));
      cell.Border = PdfPCell.NO_BORDER;
      cell.UseAscender = true;
      cell.UseDescender = true;
      table.AddCell(cell);
      // cell with the list of countries
      cell = new PdfPCell();
      cell.AddElement(PojoToElementFactory.GetCountryList(movie));
      cell.Border = PdfPCell.NO_BORDER;
      cell.UseAscender =  true;
      cell.UseDescender = true;
      table.AddCell(cell);
      return table;
    }
// ---------------------------------------------------------------------------       
    /**
     * Create a table with the full movie title
     * @param screening a Screening object
     * @return a table
     */
    private static PdfPTable FullTitle(Screening screening) {
      PdfPTable table = new PdfPTable(3);
      table.SetWidths(new int[]{3, 15, 2});
      table.WidthPercentage = 100;
      // cell 1: location and time
      PdfPCell cell = new PdfPCell();
      cell.Border = PdfPCell.NO_BORDER;
      cell.BackgroundColor = BaseColor.WHITE;
      cell.UseAscender = true;
      cell.UseDescender = true;
      String s = string.Format(
        // "{0} \u2013 %2$tH:%2$tM",
        "{0} \u2013 {1}",
        screening.Location, 
        screening.Time.Substring(0, 5)
        // screening.Time().getTime()
      );
      cell.AddElement(new Paragraph(s));
      table.AddCell(cell);
      // cell 2: English and original title 
      Movie movie = screening.movie;
      Paragraph p = new Paragraph();
      p.Add(new Phrase(movie.MovieTitle, FilmFonts.BOLD));
      p.Leading = 16;
      if (!string.IsNullOrEmpty(movie.OriginalTitle)) {
        p.Add(new Phrase(" (" + movie.OriginalTitle + ")"));
      }
      cell = new PdfPCell();
      cell.AddElement(p);
      cell.Border = PdfPCell.NO_BORDER;
      cell.UseAscender = true;
      cell.UseDescender = true;
      table.AddCell(cell);
      // cell 3 duration
      cell = new PdfPCell();
      cell.Border = PdfPCell.NO_BORDER;
      cell.BackgroundColor = BaseColor.WHITE;
      cell.UseAscender = true;
      cell.UseDescender = true;
      // p = new Paragraph(String.format("%d'", movie.getDuration()));
      p = new Paragraph(movie.Duration.ToString() + "'");
      p.Alignment = Element.ALIGN_CENTER;
      cell.AddElement(p);
      table.AddCell(cell);
      return table;
    }
// ---------------------------------------------------------------------------       
    /**
     * Create an image with a movie poster.
     * @param imdb an Internet Movie Database id
     * @return an Image
     * @throws DocumentException
     * @throws IOException
     */
    public Image GetImage(String imdb) {
      if ( images.ContainsKey(imdb) ) {
        return images[imdb];
      }
      else {
        Image img = Image.GetInstance(Path.Combine(RESOURCE, imdb + ".jpg"));
        img.ScaleToFit(80, 72);
        images.Add(imdb, img);
        return img;
      }
    } 
// ===========================================================================
  }
}