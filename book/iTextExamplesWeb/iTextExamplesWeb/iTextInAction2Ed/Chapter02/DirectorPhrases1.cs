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
 * Writes a list of directors to a PDF file.
 */
namespace kuujinbo.iTextInAction2Ed.Chapter02 {
  public class DirectorPhrases1 : IWriter {
// ===========================================================================
    /** A font that will be used in our PDF. */
    public readonly Font BOLD_UNDERLINED =
        new Font(Font.FontFamily.TIMES_ROMAN, 12, Font.BOLD | Font.UNDERLINE);
    /** A font that will be used in our PDF. */
    public readonly Font NORMAL = new Font(Font.FontFamily.TIMES_ROMAN, 12);
// ---------------------------------------------------------------------------
    /**
     * Creates a Phrase with the name and given name of a director using different fonts.
     * @param r the DbDataReader containing director records.
     */
    protected virtual Phrase CreateDirectorPhrase(DbDataReader r) {
      Phrase director = new Phrase();
      director.Add(
        new Chunk(r["name"].ToString(), BOLD_UNDERLINED)
      );
      director.Add(new Chunk(",", BOLD_UNDERLINED));
      director.Add(new Chunk(" ", NORMAL));
      director.Add(
        new Chunk(r["given_name"].ToString(), NORMAL)
      );
      return director;
    }
// ---------------------------------------------------------------------------
    public virtual void Write(Stream stream) {
      var SQL = 
@"SELECT name, given_name 
FROM film_director 
ORDER BY name, given_name";     
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
            cmd.CommandText = SQL;        
            c.Open();            
            using (var r = cmd.ExecuteReader()) {
              while (r.Read()) {
                document.Add(CreateDirectorPhrase(r));
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