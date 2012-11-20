/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;
using kuujinbo.iTextInAction2Ed.Chapter04;

namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class FindDirectors : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const String RESULT = "find_directors.pdf";
    /** Path to a resource. */
    public const string RESOURCE = "find_director.js";
    protected string jsContents;
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) { 
      NestedTables n = new NestedTables();
      byte[] pdf = Utility.PdfBytes(n);        
      using (ZipFile zip = new ZipFile()) {
        FindDirectors f =  new FindDirectors();
        zip.AddEntry(RESULT, f.CreatePdf(pdf));
        zip.AddEntry(RESOURCE, f.jsContents);
        zip.AddEntry(Utility.ResultFileName(n.ToString() + ".pdf"), pdf);
        zip.Save(stream);             
      }    
    }
// ---------------------------------------------------------------------------
    /**
     * Creates a PDF file with director names.
     * @param pdf the PDF file to be used as a reader
     */
    public byte[] CreatePdf(byte[] pdf) {
      byte[] tmpDoc = null;
      using ( MemoryStream ms = new MemoryStream() ) {
        using (Document tmp = new Document()) {
          PdfWriter writer = PdfWriter.GetInstance(tmp, ms);
          // step 3
          tmp.Open();
          // step 4
          var SQL = 
  @"SELECT name, given_name 
  FROM film_director 
  ORDER BY name, given_name";
        using (var c =  AdoDB.Provider.CreateConnection()) {
          c.ConnectionString = AdoDB.CS;
          using (DbCommand cmd = c.CreateCommand()) {
            cmd.CommandText = SQL;            
              c.Open();            
              using (var r = cmd.ExecuteReader()) {
                while ( r.Read() ) {
                  tmp.Add(CreateDirectorParagraph(writer, r));
                }
              }
            }
          }
        }
        tmpDoc = ms.ToArray();
      }

      jsContents = File.ReadAllText(
        Path.Combine(Utility.ResourceJavaScript, RESOURCE)
      );
      List<byte[]> readers = new List<byte[]>() {tmpDoc, pdf};
      using (MemoryStream ms = new MemoryStream()) {
        // step 1
        using (Document document = new Document()) {
          // step 2
          using (PdfCopy copy = new PdfCopy(document, ms)) {
            // step 3
            document.Open();
            // step 4
            copy.AddJavaScript(jsContents);
            for (int i = 0; i < readers.Count; ++i) {
              PdfReader reader = new PdfReader(readers[i]);
              int n = reader.NumberOfPages;
              for (int page = 0; page < n; ) {
                copy.AddPage(copy.GetImportedPage(reader, ++page));
              }
            }
          }
        } 
        return ms.ToArray();     
      }
    } 
// ---------------------------------------------------------------------------
    /**
     * Creates a Phrase with the name and given name of a director
     * using different fonts.
     * @param r the DbDataReader containing director records.
     */
    public Paragraph CreateDirectorParagraph(PdfWriter writer, DbDataReader r) {
      string n = r["name"].ToString();
      Chunk name = new Chunk(n);
      name.SetAction(PdfAction.JavaScript(
        string.Format("findDirector('{0}');", n), 
        writer
      ) );
      name.Append(", ");
      name.Append(r["given_name"].ToString());
      return new Paragraph(name);
    }
// ===========================================================================
  }
}