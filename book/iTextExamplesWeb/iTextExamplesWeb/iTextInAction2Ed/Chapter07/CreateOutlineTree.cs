/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using System.Collections;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class CreateOutlineTree : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String RESULT = "outline_tree.pdf";
    /** An XML representing the outline tree */
    public const string RESULTXML = "outline_tree.xml";
    /** Pattern of the IMDB urls */
    public const String RESOURCE = "http://imdb.com/title/tt{0}/";    
    /** JavaScript snippet. */
    public const string INFO
        = "app.alert('Movie produced in {0}; run length: {1}');";    
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        CreateOutlineTree example = new CreateOutlineTree();
        byte[] pdf = example.CreatePdf();
        zip.AddEntry(RESULT, pdf);
        zip.AddEntry(RESULTXML, example.CreateXml(pdf));
        zip.Save(stream);             
      }   
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a PDF document.
     */
    public byte[] CreatePdf() {
      using (MemoryStream ms = new MemoryStream()) {
        // step 1
        using (Document document = new Document()) {
          // step 2
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          // step 3
          document.Open();
          // step 4
          PdfOutline root = writer.RootOutline;
          PdfOutline movieBookmark;
          PdfOutline link;
          String title;
          IEnumerable<Movie> movies = PojoFactory.GetMovies();
          foreach (Movie movie in movies) {
            title = movie.MovieTitle;
            if ("3-Iron".Equals(title))
                title = "\ube48\uc9d1";
            movieBookmark = new PdfOutline(root, 
              new PdfDestination(
                PdfDestination.FITH, writer.GetVerticalPosition(true)
              ),
              title, true
            );
            movieBookmark.Style = Font.BOLD;
            link = new PdfOutline(movieBookmark,
              new PdfAction(String.Format(RESOURCE, movie.Imdb)),
              "link to IMDB"
            );
            link.Color = BaseColor.BLUE;
            new PdfOutline(movieBookmark,
              PdfAction.JavaScript(
                String.Format(INFO, movie.Year, movie.Duration),
                writer
              ),
              "instant info"
            );
            document.Add(new Paragraph(movie.MovieTitle));
            document.Add(PojoToElementFactory.GetDirectorList(movie));
            document.Add(PojoToElementFactory.GetCountryList(movie));
          }
        }
        return ms.ToArray();
      }
    }   
// ---------------------------------------------------------------------------    
    /**
     * Creates an XML file with the bookmarks of a PDF file
     * @param pdfIn the byte array with the document 
     */
    public string CreateXml(byte[] pdfIn) {
      PdfReader reader = new PdfReader(pdfIn);
      var list = SimpleBookmark.GetBookmark(reader);
      using (MemoryStream ms = new MemoryStream()) {
        SimpleBookmark.ExportToXML(list, ms, "ISO8859-1", true); 
        ms.Position = 0;
        using (StreamReader sr =  new StreamReader(ms)) {
          return sr.ReadToEnd();
        }              
      }       
    }
// ===========================================================================
  }
}