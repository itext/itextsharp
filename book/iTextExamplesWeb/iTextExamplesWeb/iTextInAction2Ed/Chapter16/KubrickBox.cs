/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.collection;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter16 {
  public class KubrickBox : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String RESULT = "kubrick_box.pdf";
    /** The path to an image used in the example. */
    public readonly String IMG_BOX =  Path.Combine(
      Utility.ResourceImage, "kubrick_box.jpg"
    );
    /** Path to the resources. */
    public readonly String RESOURCE =  Path.Combine(
      Utility.ResourcePosters, "{0}.jpg"
    );    
    /** The relative widths of the PdfPTable columns. */
    public readonly float[] WIDTHS = { 1 , 7 };
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        Image img = Image.GetInstance(IMG_BOX);
        document.Add(img);
        List list = new List(List.UNORDERED, 20);
        PdfDestination dest = new PdfDestination(PdfDestination.FIT);
        dest.AddFirst(new PdfNumber(1));
        IEnumerable<Movie> box = PojoFactory.GetMovies(1)
          .Concat(PojoFactory.GetMovies(4))
        ;
        foreach (Movie movie in box) {
          if (movie.Year > 1960) {
            PdfFileSpecification fs = PdfFileSpecification.FileEmbedded(
              writer, null,
              String.Format("kubrick_{0}.pdf", movie.Imdb),
              CreateMoviePage(movie)
            );
            fs.AddDescription(movie.Title, false);
            writer.AddFileAttachment(fs);
            ListItem item = new ListItem(movie.MovieTitle);
            PdfTargetDictionary target = new PdfTargetDictionary(true);
            target.EmbeddedFileName = movie.Title;
            PdfAction action = PdfAction.GotoEmbedded(null, target, dest, true);
            Chunk chunk = new Chunk(" (see info)");
            chunk.SetAction(action);
            item.Add(chunk);
            list.Add(item);
          }
        }
        document.Add(list);
      }
    }
// --------------------------------------------------------------------------- 
    /**
     * Creates the PDF.
     * @return the bytes of a PDF file.
     */
    public byte[] CreateMoviePage(Movie movie) {
      using (MemoryStream ms = new MemoryStream()) {
        // step 1
        using (Document document = new Document()) {
          // step 2
          PdfWriter.GetInstance(document, ms);
          // step 3
          document.Open();
          // step 4
          Paragraph p = new Paragraph(
            movie.MovieTitle,
            FontFactory.GetFont(
              BaseFont.HELVETICA, BaseFont.WINANSI, BaseFont.NOT_EMBEDDED, 16
            )
          );
          document.Add(p);
          document.Add(Chunk.NEWLINE);
          PdfPTable table = new PdfPTable(WIDTHS);
          table.AddCell(Image.GetInstance(
            String.Format(RESOURCE, movie.Imdb)
          ));
          PdfPCell cell = new PdfPCell();
          cell.AddElement(new Paragraph("Year: " + movie.Year.ToString()));
          cell.AddElement(new Paragraph("Duration: " + movie.Duration.ToString()));
          table.AddCell(cell);
          document.Add(table);
          PdfDestination dest = new PdfDestination(PdfDestination.FIT);
          dest.AddFirst(new PdfNumber(1));
          PdfTargetDictionary target = new PdfTargetDictionary(false);
          Chunk chunk = new Chunk("Go to original document");
          PdfAction action = PdfAction.GotoEmbedded(null, target, dest, false);
          chunk.SetAction(action);
          document.Add(chunk);
        }
        return ms.ToArray();
      }
    }
    
// ===========================================================================
  }
}