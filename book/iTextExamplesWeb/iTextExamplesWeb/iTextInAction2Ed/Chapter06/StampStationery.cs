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
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class StampStationery : IWriter {
// ===========================================================================
    /** The original PDF file. */
    public const string ORIGINAL = "original.pdf";
    /** The resulting PDF. */
    public const string RESULT = "stamped_stationary.pdf";    
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        // previous example
        Stationery s = new Stationery();
        StampStationery ss = new StampStationery();
        byte[] stationery  = s.CreateStationary();
        byte[] sStationery = ss.CreatePdf();
        
        zip.AddEntry(RESULT, ManipulatePdf(sStationery, stationery));
        zip.AddEntry(ORIGINAL, sStationery); 
        zip.AddEntry(Utility.ResultFileName(s.ToString() + ".pdf"), stationery); 
        zip.Save(stream);
      }      
    }
// ---------------------------------------------------------------------------
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     * @param stationery a PDF that will be added as background
     */
    public byte[] ManipulatePdf(byte[] src, byte[] stationery) {
      // Create readers
      PdfReader reader = new PdfReader(src);
      PdfReader s_reader = new PdfReader(stationery);
      using (MemoryStream ms = new MemoryStream()) {
        // Create the stamper
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          // Add the stationery to each page
          PdfImportedPage page = stamper.GetImportedPage(s_reader, 1);
          int n = reader.NumberOfPages;
          PdfContentByte background;
          for (int i = 1; i <= n; i++) {
            background = stamper.GetUnderContent(i);
            background.AddTemplate(page, 0, 0);
          }
        } 
        return ms.ToArray();   
      }
    }  
// ---------------------------------------------------------------------------
    /**
     * Creates a PDF document.
     */
    public byte[] CreatePdf() {
      string SQL = 
@"SELECT country, id FROM film_country 
ORDER BY country
";     
      using (MemoryStream ms = new MemoryStream()) {     
        // step 1
        using (Document document = new Document(PageSize.A4, 36, 36, 72, 36)) {
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
                  foreach (Movie movie in 
                      PojoFactory.GetMovies(r["id"].ToString(), true)) 
                  {
                    document.Add(new Paragraph(
                      movie.MovieTitle, FilmFonts.BOLD
                    ));
                    if (!string.IsNullOrEmpty(movie.OriginalTitle))
                        document.Add(
                          new Paragraph(movie.OriginalTitle, FilmFonts.ITALIC
                        ));
                    document.Add(new Paragraph(
                        String.Format(
                          "Year: {0}; run length: {1} minutes",
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
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}
