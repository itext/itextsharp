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
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter02 {
  public class RiverPhoenix : IWriter {
// ===========================================================================
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
        document.Add(new Paragraph(
          "Movies featuring River Phoenix", FilmFonts.BOLD
        ));
        document.Add(CreateParagraph(
          "My favorite movie featuring River Phoenix was ", "0092005"
        ));
        document.Add(CreateParagraph(
          "River Phoenix was nominated for an academy award for his role in ", "0096018"
        ));
        document.Add(CreateParagraph(
          "River Phoenix played the young Indiana Jones in ", "0097576"
        ));
        document.Add(CreateParagraph(
          "His best role was probably in ", "0102494"
        ));
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Creates a paragraph with some text about a movie with River Phoenix,
     * and a poster of the corresponding movie.
     * @param text the text about the movie
     * @param imdb the IMDB code referring to the poster
     */
    public Paragraph CreateParagraph(String text, String imdb) {
      Paragraph p = new Paragraph(text);
      Image img = Image.GetInstance(Path.Combine(RESOURCE, imdb + ".jpg"));
      img.ScaleToFit(1000, 72);
      img.RotationDegrees = -30;
      p.Add(new Chunk(img, 0, -15, true));
      return p;
    }
// ===========================================================================
  }
}