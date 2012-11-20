/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO; 
using System.Globalization; 
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;

namespace kuujinbo.iTextInAction2Ed.Chapter05 {
  public class PdfCalendar : kuujinbo.iTextInAction2Ed.Chapter04.PdfCalendar {
// ===========================================================================
    /** A table event. */
    public IPdfPTableEvent tableBackground;
    /** A cell event. */
    public IPdfPCellEvent cellBackground;
    /** A cell event. */
    public IPdfPCellEvent roundRectangle;
    /** A cell event. */
    public IPdfPCellEvent whiteRectangle;
    /**
     * Inner class with a table event that draws a background with rounded corners.
     */
    class TableBackground : IPdfPTableEvent {
      public void TableLayout(
        PdfPTable table, float[][] width, float[] height,
        int headerRows, int rowStart, PdfContentByte[] canvas
      ) {
        PdfContentByte background = canvas[PdfPTable.BASECANVAS];
        background.SaveState();
        background.SetCMYKColorFill(0x00, 0x00, 0xFF, 0x0F);
        background.RoundRectangle(
          width[0][0], height[height.Length - 1] - 2,
          width[0][1] - width[0][0] + 6, 
          height[0] - height[height.Length - 1] - 4, 4
        );
        background.Fill();
        background.RestoreState();
      }
    }
/*
 * end inner class
*/     
    /**
     * Inner class with a cell event that draws a background with rounded corners.
     */
    class CellBackground : IPdfPCellEvent {
      public void CellLayout(
        PdfPCell cell, Rectangle rect, PdfContentByte[] canvas
    ) {
        PdfContentByte cb = canvas[PdfPTable.BACKGROUNDCANVAS];
        cb.RoundRectangle(
          rect.Left + 1.5f, 
          rect.Bottom + 1.5f, 
          rect.Width - 3,
          rect.Height - 3, 4
        );
        cb.SetCMYKColorFill(0x00, 0x00, 0x00, 0x00);
        cb.Fill();
      }
    }
/*
 * end inner class
*/ 
    /**
     * Inner class with a cell event that draws a border with rounded corners.
     */
    class RoundRectangle : IPdfPCellEvent {
      /** the border color described as CMYK values. */
      protected int[] color;
      /** Constructs the event using a certain color. */
      public RoundRectangle(int[] color) {
        this.color = color;
      }
      
      public void CellLayout(
        PdfPCell cell, Rectangle rect, PdfContentByte[] canvas
      ) {
        PdfContentByte cb = canvas[PdfPTable.LINECANVAS];
        cb.RoundRectangle(
          rect.Left + 1.5f, 
          rect.Bottom + 1.5f, 
          rect.Width - 3,
          rect.Height - 3, 4
        );
        cb.SetLineWidth(1.5f);
        cb.SetCMYKColorStrokeF(color[0], color[1], color[2], color[3]);
        cb.Stroke();
      }
    }
/*
 * end inner class
*/ 
// ---------------------------------------------------------------------------
    /**
     * Initializes the fonts and collections.
     */
    public PdfCalendar() : base() {
      tableBackground = new TableBackground();
      cellBackground = new CellBackground();
      roundRectangle = new RoundRectangle(new int[]{ 0xFF, 0x00, 0xFF, 0x00 });
      whiteRectangle = new RoundRectangle(new int[]{ 0x00, 0x00, 0x00, 0x00 });
    }

    public override void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4.Rotate())) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        // "en" => System.Globalization.GregorianCalendar 
        Calendar calendar = new CultureInfo(LANGUAGE, false).Calendar;
        PdfPTable table;
        PdfContentByte canvas = writer.DirectContent;
        // Loop over the months
        for (int month = 0; month < 12; month++) {
          int current_month = month + 1;
          DateTime dt = new DateTime(YEAR, current_month, 1, calendar);
          // draw the background
          DrawImageAndText(canvas, dt);
          // create a table with 7 columns
          table = new PdfPTable(7);
          table.TableEvent = tableBackground;
          table.TotalWidth = 504;
          // add the name of the month
          table.DefaultCell.Border  = PdfPCell.NO_BORDER;
          table.DefaultCell.CellEvent = whiteRectangle;
          table.AddCell(GetMonthCell(dt));
          int daysInMonth = DateTime.DaysInMonth(YEAR, dt.Month);
          int day = 1;
          // add empty cells
          // SUNDAY; Java => 1, .NET => 0
          int position = 0;
          while (position++ != (int) dt.DayOfWeek) {
            table.AddCell("");
          }
          // add cells for each day
          while (day <= daysInMonth) {
            dt = new DateTime(YEAR, current_month, day++, calendar);
            table.AddCell(GetDayCell(dt));
          }          
          // complete the table
          table.CompleteRow();
          // write the table to an absolute position
          table.WriteSelectedRows(0, -1, 169, table.TotalHeight + 20, canvas);
          document.NewPage();
        }
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Creates a PdfPCell with the name of the month
     * @param dt a DateTime
     * @return a PdfPCell with rowspan 7, containing the name of the month
     */
    public new PdfPCell GetMonthCell(DateTime dt) {
      PdfPCell cell = new PdfPCell();
      cell.Colspan = 7;
      cell.Border = PdfPCell.NO_BORDER;
      cell.UseDescender = true;
      Paragraph p = new Paragraph(dt.ToString("MMMM yyyy"), bold);
      p.Alignment = Element.ALIGN_CENTER;
      cell.AddElement(p);
      return cell;
    }

    /**
     * Creates a PdfPCell for a specific day
     * @param dt a DateTime
     * @return a PdfPCell
     */
    public new PdfPCell GetDayCell(DateTime dt) {
      PdfPCell cell = new PdfPCell();
      // set the event to draw the background
      cell.CellEvent = cellBackground;
      // set the event to draw a special border
      if (IsSunday(dt) || IsSpecialDay(dt))
          cell.CellEvent = roundRectangle;
      cell.Padding = 3;
      cell.Border = PdfPCell.NO_BORDER;
      // set the content in the language of the locale
      Chunk chunk = new Chunk(dt.ToString("ddd"), small);
      chunk.SetTextRise(8);
      // a paragraph with the day
      Paragraph p = new Paragraph(chunk);
      // a separator
      p.Add(new Chunk(new VerticalPositionMark()));
      // and the number of the day
      p.Add(new Chunk(dt.ToString("%d"), normal));
      cell.AddElement(p);
      return cell;
    }
    
    /**
     * Returns true if the date was found in a list with special days (holidays).
     * @param dt a DateTime
     * @return true for holidays
     */
    public new bool IsSpecialDay(DateTime dt) {
      return specialDays.ContainsKey(dt.ToString("MMdd"))
          ? true: false;
    }    
// ===========================================================================
  }
}