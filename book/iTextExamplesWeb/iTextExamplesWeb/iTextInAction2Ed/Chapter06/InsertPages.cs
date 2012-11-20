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
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class InsertPages : IWriter {
// ===========================================================================
    public const string RESULT1 = "inserted_pages.pdf";
    public const string RESULT2 = "reordered.pdf";
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        Stationery s = new Stationery();
        StampStationery ss = new StampStationery();
        byte[] stationery  = s.CreateStationary();
        byte[] sStationery = ss.ManipulatePdf(
          ss.CreatePdf(), stationery
        );
        byte[] insertPages = ManipulatePdf(sStationery, stationery);
        zip.AddEntry(RESULT1, insertPages); 
        // reorder the pages in the PDF
        PdfReader reader = new PdfReader(insertPages);
        reader.SelectPages("3-41,1-2");
        using (MemoryStream ms = new MemoryStream()) {
          using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          }
          zip.AddEntry(RESULT2, ms.ToArray());
        }
        zip.AddEntry(Utility.ResultFileName(s.ToString() + ".pdf"), stationery);
        zip.AddEntry(Utility.ResultFileName(ss.ToString() + ".pdf"), sStationery);
        zip.Save(stream);          
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Manipulates a PDF file src
     * @param src the original PDF
     * @param stationery the resulting PDF
     */
    public byte[] ManipulatePdf(byte[] src, byte[] stationery) {
      ColumnText ct = new ColumnText(null);
      string SQL = 
@"SELECT country, id FROM film_country 
ORDER BY country
";        
      using (var c =  AdoDB.Provider.CreateConnection()) {
        c.ConnectionString = AdoDB.CS;
        using (DbCommand cmd = c.CreateCommand()) {
          cmd.CommandText = SQL;
          c.Open();
          using (var r = cmd.ExecuteReader()) {
            while (r.Read()) {
              ct.AddElement(new Paragraph(
                24, new Chunk(r["country"].ToString())
              ));
            }
          }
        }
      }
      // Create a reader for the original document and for the stationery
      PdfReader reader = new PdfReader(src);
      PdfReader rStationery = new PdfReader(stationery);
      using (MemoryStream ms = new MemoryStream()) {
        // Create a stamper
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          // Create an imported page for the stationery
          PdfImportedPage page = stamper.GetImportedPage(rStationery, 1);
          int i = 0;
          // Add the content of the ColumnText object 
          while(true) {
          // Add a new page
            stamper.InsertPage(++i, reader.GetPageSize(1));
            // Add the stationary to the new page
            stamper.GetUnderContent(i).AddTemplate(page, 0, 0);
            // Add as much content of the column as possible
            ct.Canvas = stamper.GetOverContent(i);
            ct.SetSimpleColumn(36, 36, 559, 770);
            if (!ColumnText.HasMoreText(ct.Go()))
                break;
          }
        }
        return ms.ToArray();     
      }
    }    
// ===========================================================================
  }
}