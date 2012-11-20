/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Data;
using System.Data.Common;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

/**
 * Writes a list of countries to a PDF file.
 */
namespace kuujinbo.iTextInAction2Ed.Chapter02 {
  public class CountryChunks : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream).InitialLeading = 16;
        // step 3
        document.Open();
        // add the ID in another font
        Font font = new Font(Font.FontFamily.HELVETICA, 6, Font.BOLD, BaseColor.WHITE);
        // step 4
        using (var c =  AdoDB.Provider.CreateConnection()) {
          c.ConnectionString = AdoDB.CS;
          using (DbCommand cmd = c.CreateCommand()) {
            cmd.CommandText = 
              "SELECT country,id FROM film_country ORDER BY country";
            c.Open();
            using (var r = cmd.ExecuteReader()) {
              while (r.Read()) {
                var country = r.GetString(0);
                var ID = r.GetString(1);
                document.Add(new Chunk(country));
                document.Add(new Chunk(" "));
                Chunk id = new Chunk(ID, font);
                // with a background color
                id.SetBackground(BaseColor.BLACK, 1f, 0.5f, 1f, 1.5f);
                // and a text rise
                id.SetTextRise(6);
                document.Add(id);
                document.Add(Chunk.NEWLINE);
              }
            }
          }
        }
      }
    }
// ===========================================================================
  }
}