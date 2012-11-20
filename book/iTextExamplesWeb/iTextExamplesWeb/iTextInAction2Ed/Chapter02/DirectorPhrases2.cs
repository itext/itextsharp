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

/**
 * Writes a list of directors to a PDF file.
 */
namespace kuujinbo.iTextInAction2Ed.Chapter02 {
  public class DirectorPhrases2 : DirectorPhrases1 {
// ===========================================================================
    /** A font that will be used in our PDF. */
    public Font BOLD;
    /** A font that will be used in our PDF. */
    public new Font NORMAL;
// ---------------------------------------------------------------------------
    public DirectorPhrases2() {
      BaseFont timesbd = null;
      BaseFont times = null;
    // create a font that will be embedded
      timesbd = BaseFont.CreateFont(
        "c:/windows/fonts/timesbd.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED
      );
      // create a font that will be embedded
      times = BaseFont.CreateFont(
        "c:/windows/fonts/times.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED
      );
      BOLD = new Font(timesbd, 12);
      NORMAL = new Font(times, 12);
    }
// ---------------------------------------------------------------------------
    /**
     * Creates a Phrase with the name and given name of a director using different fonts.
     * @param    rs    the ResultSet containing director records.
     */
    protected override Phrase CreateDirectorPhrase(DbDataReader r) {
      Phrase director = new Phrase();
      Chunk name = new Chunk(r["name"].ToString(), BOLD);
      name.SetUnderline(0.2f, -2f);
      director.Add(name);
      director.Add(new Chunk(",", BOLD));
      director.Add(new Chunk(" ", NORMAL));
      director.Add(
        new Chunk(r["given_name"].ToString(), NORMAL)
      );
      director.Leading = 24;
      return director;
    }
// ---------------------------------------------------------------------------
    public override void Write(Stream stream) {
      base.Write(stream);
    }
// ===========================================================================
  }
}