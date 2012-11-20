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
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.collection;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter16 {
  public class KubrickMovies : IWriter {
// ===========================================================================
    /** Path to the resources. */
    public readonly String RESOURCE =  Path.Combine(
      Utility.ResourcePosters, "{0}.jpg"
    );    
    /** The relative widths of the PdfPTable columns. */
    public readonly float[] WIDTHS = { 1 , 7 };
    /** The filename of the PDF */
    public const String FILENAME = "kubrick_movies.pdf";    
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        document.Add(new Paragraph(
          "This document contains a collection of PDFs,"
          + " one per Stanley Kubrick movie."
        ));
        
        PdfCollection collection = new PdfCollection(PdfCollection.DETAILS);
        PdfCollectionSchema schema = _collectionSchema(); 
        collection.Schema = schema;
        PdfCollectionSort sort = new PdfCollectionSort("YEAR");
        sort.SetSortOrder(false);
        collection.Sort = sort;
        collection.InitialDocument = "Eyes Wide Shut";
        writer.Collection = collection;
        
        PdfCollectionItem item;
        IEnumerable<Movie> movies = PojoFactory.GetMovies(1);
        foreach (Movie movie in movies) {
          PdfFileSpecification fs = PdfFileSpecification.FileEmbedded(
            writer, null,
            String.Format("kubrick_{0}.pdf", movie.Imdb),
            CreateMoviePage(movie)
          );
          fs.AddDescription(movie.Title, false);

          item = new PdfCollectionItem(schema);
          item.AddItem("TITLE", movie.GetMovieTitle(false));
          if (movie.GetMovieTitle(true) != null) {
            item.SetPrefix("TITLE", movie.GetMovieTitle(true));
          }
          item.AddItem("DURATION", movie.Duration.ToString());
          item.AddItem("YEAR", movie.Year.ToString());
          fs.AddCollectionItem(item);
          writer.AddFileAttachment(fs);
        }
      }
    }
// ---------------------------------------------------------------------------     
    /**
     * Creates a Collection schema that can be used to organize the movies 
     * of Stanley Kubrick in a collection: year, title, duration, DVD 
     * acquisition, filesize (filename is present, but hidden).
     * @return a collection schema
     */
    private PdfCollectionSchema _collectionSchema() {
      PdfCollectionSchema schema = new PdfCollectionSchema();
      PdfCollectionField size = new PdfCollectionField(
        "File size", PdfCollectionField.SIZE
      );
      
      size.Order = 4;
      schema.AddField("SIZE", size);
      
      PdfCollectionField filename = new PdfCollectionField(
        "File name", PdfCollectionField.FILENAME
      );
      filename.Visible = false;
      schema.AddField("FILE", filename);
      
      PdfCollectionField title = new PdfCollectionField(
        "Movie title", PdfCollectionField.TEXT
      );
      title.Order = 1;
      schema.AddField("TITLE", title);
      
      PdfCollectionField duration = new PdfCollectionField(
        "Duration", PdfCollectionField.NUMBER
      );
      duration.Order = 2;
      schema.AddField("DURATION", duration);
      
      PdfCollectionField year = new PdfCollectionField(
        "Year", PdfCollectionField.NUMBER
      );
      year.Order = 0;
      schema.AddField("YEAR", year);
      
      return schema;
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
          
          PdfTargetDictionary target = new PdfTargetDictionary(false);
          target.AdditionalPath = new PdfTargetDictionary(false);
          Chunk chunk = new Chunk("Go to original document");
          PdfAction action = PdfAction.GotoEmbedded(
            null, target, new PdfString("movies"), false
          );
          chunk.SetAction(action);
          document.Add(chunk);
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}