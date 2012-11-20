/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.Com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Data;
using System.Data.Common;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class Stationery : IWriter {
// ===========================================================================
    /** The original PDF. */
    public const string STATIONERY = "stationery.pdf";
    /** The resulting PDF. */
    public const string RESULT = "text_on_stationery.pdf";
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
// create first PDF        
        byte[] stationary  = CreateStationary();
        zip.AddEntry(STATIONERY, stationary);
        zip.AddEntry(RESULT, CreatePdf(stationary));
        zip.Save(stream);
      }
    }
// --------------------------------------------------------------------------- 
    /**
     * Creates a PDF document.
     * @param stationary byte array of the new PDF document
     */   
    public byte[] CreatePdf(byte[] stationary) {
      using (MemoryStream ms = new MemoryStream()) {
        // step 1
        using (Document document = new Document(PageSize.A4, 36, 36, 72, 36)) {
          // step 2
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          //writer.CloseStream = false;
          UseStationary(writer, stationary);
          // step 3
          document.Open();
          // step 4
          string SQL = "SELECT country, id FROM film_country ORDER BY country";
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
                    document.Add(new Paragraph(
                      string.Format(
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
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------    
    /** Imported page with the stationery. */
    private PdfImportedPage page;

    /**
     * Initialize the imported page.
     * @param writer The PdfWriter
     */
    public void UseStationary(PdfWriter writer, byte[] stationary) {
      writer.PageEvent = new TemplateHelper(this);
      PdfReader reader = new PdfReader(stationary);
      page = writer.GetImportedPage(reader, 1);
    }
/*
 * ###########################################################################
 * Inner class to add template
 * ###########################################################################
*/
    class TemplateHelper : PdfPageEventHelper {
      private Stationery instance;
      public TemplateHelper() { }
      public TemplateHelper(Stationery instance) { 
        this.instance = instance;
      }
      /**
       * @see com.itextpdf.text.pdf.PdfPageEventHelper#onEndPage(
       *      com.itextpdf.text.pdf.PdfWriter, com.itextpdf.text.Document)
       */
      public override void OnEndPage(PdfWriter writer, Document document) {
        writer.DirectContentUnder.AddTemplate(instance.page, 0, 0);
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a PDF document.
     */
    public byte[] CreateStationary() {
      using (MemoryStream ms = new MemoryStream()) {
        // step 1
        using (Document document = new Document()) {
          // step 2
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          writer.CloseStream = false;
          // step 3
          document.Open();
          // step 4
          PdfPTable table = new PdfPTable(1);
          table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
          table.AddCell(new Phrase("FOOBAR FILM FESTIVAL", FilmFonts.BOLD));
          document.Add(table);
          Font font = new Font(
            Font.FontFamily.HELVETICA, 52, Font.BOLD, new GrayColor(0.75f)
          );
          ColumnText.ShowTextAligned(
            writer.DirectContentUnder,
            Element.ALIGN_CENTER, new Phrase("FOOBAR FILM FESTIVAL", font),
            297.5f, 421, 45
          );
        }
        return ms.ToArray();
      }      
    }    
// ===========================================================================
  }
}