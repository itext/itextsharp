/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter05 {
  public class RunLengthEvent : IWriter {
// ===========================================================================
    /** Inner class to draw a bar inside a cell. */
    class RunLength : IPdfPCellEvent {
      public int duration;
      
      public RunLength(int duration) {
        this.duration = duration;
      }
      
      /**
       * @see com.lowagie.text.pdf.PdfPCellEvent#cellLayout(
       *      com.lowagie.text.pdf.PdfPCell, com.lowagie.text.Rectangle,
       *      com.lowagie.text.pdf.PdfContentByte[])
       */
      public void CellLayout(
        PdfPCell cell, Rectangle rect, PdfContentByte[] canvas
      ) {
        PdfContentByte cb = canvas[PdfPTable.BACKGROUNDCANVAS];
        cb.SaveState();
        if (duration < 90) {
          cb.SetRGBColorFill(0x7C, 0xFC, 0x00);
        }
        else if (duration > 120) {
          cb.SetRGBColorFill(0x8B, 0x00, 0x00);
        } 
        else {
          cb.SetRGBColorFill(0xFF, 0xA5, 0x00);
        }
        cb.Rectangle(
          rect.Left, rect.Bottom, 
          rect.Width * duration / 240, rect.Height
        );
        cb.Fill();
        cb.RestoreState();
      }
    }
/*
 * end inner class
*/ 
    /** Inner class to add the words PRESS PREVIEW to a cell. */
    class PressPreview : IPdfPCellEvent {
      public BaseFont bf;
      public PressPreview() {
        bf = BaseFont.CreateFont();
      }
      
      /**
       * @see com.lowagie.text.pdf.PdfPCellEvent#cellLayout(com.lowagie.text.pdf.PdfPCell,
       *      com.lowagie.text.Rectangle,
       *      com.lowagie.text.pdf.PdfContentByte[])
       */
      public void CellLayout(
        PdfPCell cell, Rectangle rect, PdfContentByte[] canvas
      ) {
        PdfContentByte cb = canvas[PdfPTable.TEXTCANVAS];
        cb.BeginText();
        cb.SetFontAndSize(bf, 12);
        cb.ShowTextAligned(
          Element.ALIGN_RIGHT, "PRESS PREVIEW",
          rect.Right - 3,
          rect.Bottom + 4.5f, 0
        );
        cb.EndText();
      }
    }
/*
 * end inner class
*/  
// ---------------------------------------------------------------------------
    /** The press cell event. */
    public IPdfPCellEvent press;
    public void Write(Stream stream) {
      press = new PressPreview();
      // step 1
      using (Document document = new Document(PageSize.A4.Rotate())) {
        // step 2
        PdfWriter.GetInstance(document, stream);      
        // step 3
        document.Open();
        // step 4
        List<string> days = PojoFactory.GetDays();
        foreach (string day in days) {
          document.Add(GetTable(day));
          document.NewPage();
        }        
      }
    }
// ---------------------------------------------------------------------------
    /**
     * @param connection
     * @param day
     * @return
     * @throws SQLException
     * @throws DocumentException
     * @throws IOException
     */
    public PdfPTable GetTable(string day) {
      PdfPTable table = new PdfPTable(new float[] { 2, 1, 2, 5, 1 });
      table.WidthPercentage = 100f;
      table.DefaultCell.Padding = 3;
      table.DefaultCell.UseAscender = true;
      table.DefaultCell.UseDescender = true;
      table.DefaultCell.Colspan = 5;
      table.DefaultCell.BackgroundColor = BaseColor.RED;
      table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
      table.AddCell(day);
      table.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;
      table.DefaultCell.Colspan = 1;
      table.DefaultCell.BackgroundColor = BaseColor.YELLOW;
      for (int i = 0; i < 2; i++) {
        table.AddCell("Location");
        table.AddCell("Time");
        table.AddCell("Run Length");
        table.AddCell("Title");
        table.AddCell("Year");
      }
      table.DefaultCell.BackgroundColor = null;
      table.HeaderRows = 3;
      table.FooterRows = 1;
      List<Screening> screenings = PojoFactory.GetScreenings(day);
      Movie movie;
      PdfPCell runLength;
      foreach (Screening screening in screenings) {
        movie = screening.movie;
        table.AddCell(screening.Location);
        table.AddCell(screening.Time.Substring(0, 5));
        runLength = new PdfPCell(table.DefaultCell);
        runLength.Phrase = new Phrase(String.Format(
          "{0} '", movie.Duration
        ));
        runLength.CellEvent = new RunLength(movie.Duration);
        if (screening.Press) {
          runLength.CellEvent = press;
        }
        table.AddCell(runLength);
        table.AddCell(movie.MovieTitle);
        table.AddCell(movie.Year.ToString());
      }
      return table;
    }
// ===========================================================================
  }
}