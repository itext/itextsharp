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
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

/**
 * We'll test our SQLite database with this example
 */
namespace kuujinbo.iTextInAction2Ed.Chapter02 {
  public class DatabaseTest : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        using (var c =  AdoDB.Provider.CreateConnection()) {
          c.ConnectionString = AdoDB.CS;
          using (DbCommand cmd = c.CreateCommand()) {
            cmd.CommandText = 
              "SELECT country FROM film_country ORDER BY country";
            c.Open();            
            using (var r = cmd.ExecuteReader()) {
              while (r.Read()) {
                document.Add(new Paragraph( r.GetString(0) ));
              }
            }
          }
        }
      }
    }
// ===========================================================================
  }
}