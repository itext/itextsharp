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
using System.Data;
using System.Data.Common;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;
using kuujinbo.iTextInAction2Ed.Chapter02;

namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class LinkActions : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const string RESULT1 = "movie_links_1.pdf";
     /** The resulting PDF file. */
    public const string RESULT2 = "movie_links_2.pdf"; 
    /** The resulting XML file. */
    public const string RESULT3 = "destinations.xml";    
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        MovieLinks1 m = new MovieLinks1(); 
        byte[] pdf = Utility.PdfBytes(m);
        LinkActions actions = new LinkActions();
        zip.AddEntry(RESULT2, actions.CreatePdf());
        zip.AddEntry(RESULT3, actions.CreateXml(pdf));
        zip.AddEntry(RESULT1, pdf);
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
          PdfWriter.GetInstance(document, ms);
          // step 3
          document.Open();
          // step 4
          // Add text with a local destination
          Paragraph p = new Paragraph();
          Chunk top = new Chunk("Country List", FilmFonts.BOLD);
          top.SetLocalDestination("top");
          p.Add(top);
          document.Add(p);
          // Add text with a link to an external URL
          Chunk imdb = new Chunk("Internet Movie Database", FilmFonts.ITALIC);
          imdb.SetAction(new PdfAction(new Uri("http://www.imdb.com/")));
          p = new Paragraph(
            @"Click on a country, and you'll get a list of movies, 
            containing links to the "
          );
          p.Add(imdb);
          p.Add(".");
          document.Add(p);
          // Add text with a remote goto
          p = new Paragraph("This list can be found in a ");
          Chunk page1 = new Chunk("separate document");
          page1.SetAction(new PdfAction(RESULT1, 1));
          
          p.Add(page1);
          p.Add(".");
          document.Add(p);
          document.Add(Chunk.NEWLINE); 
          // Get a list with countries from the database      
          var SQL = 
@"SELECT DISTINCT mc.country_id, c.country, count(*) AS c 
FROM film_country c, film_movie_country mc 
WHERE c.id = mc.country_id
GROUP BY mc.country_id, country 
ORDER BY c DESC";
          using (var c =  AdoDB.Provider.CreateConnection()) {
            c.ConnectionString = AdoDB.CS;
            using (DbCommand cmd = c.CreateCommand()) {
              cmd.CommandText = SQL; 
              c.Open();            
              using (var r = cmd.ExecuteReader()) {
                while (r.Read()) {
                  Paragraph country = new Paragraph(r["country"].ToString());
                  country.Add(": ");
                  Chunk link = new Chunk(string.Format(
                    "{0} movies", r["c"].ToString()
                  ));
                  link.SetAction(PdfAction.GotoRemotePage(
                    RESULT1, 
                    r["country_id"].ToString(),
                    false, 
                    true
                  ));
                  country.Add(link);
                  document.Add(country);                
                }         
              }
            }
          }
          document.Add(Chunk.NEWLINE);
          // Add text with a local goto
          p = new Paragraph("Go to ");
          top = new Chunk("top");
          top.SetAction(PdfAction.GotoLocalPage("top", false));
          p.Add(top);
          p.Add(".");
          document.Add(p);        
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Create an XML file with named destinations
     * @param src the PDF with the destinations
     */
    public string CreateXml(byte[] src) {
      PdfReader reader = new PdfReader(src);
      Dictionary<string,string> map = SimpleNamedDestination
          .GetNamedDestination(reader, false);
      using (MemoryStream ms = new MemoryStream()) {
        SimpleNamedDestination.ExportToXML(map, ms, "ISO8859-1", true);
        ms.Position = 0;
        using (StreamReader sr =  new StreamReader(ms)) {
          return sr.ReadToEnd();
        }
      }
    }    
// ===========================================================================
  }
}