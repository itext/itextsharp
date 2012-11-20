/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Globalization;
using System.util;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;

namespace kuujinbo.iTextInAction2Ed.Chapter04 {
  public class PdfCalendar : IWriter {
// ===========================================================================
    /** The year for which we want to create a calendar */
    public const int YEAR = 2011;
    /** The language code for the calendar
     * change this to set culture-specific info
    */
    public const string LANGUAGE = "en";
    /** Path to the resources. */
    public readonly string RESOURCE = Utility.ResourceCalendar;
    /** resources file. */
    public const string CONTENT = "content.txt";

    public Properties specialDays = new Properties();
    /** Collection with the description of the images */
    public Properties content = new Properties();

    /** A font that is used in the calendar */
    protected Font normal;
    /** A font that is used in the calendar */
    protected Font small;
    /** A font that is used in the calendar */
    protected Font bold;
// ---------------------------------------------------------------------------
    /**
     * Initializes the fonts and collections.
     */
    public PdfCalendar() {
      // fonts
    BaseFont bf_normal = BaseFont.CreateFont(
      "c://windows/fonts/arial.ttf",
        BaseFont.WINANSI, 
        BaseFont.EMBEDDED
      );
      normal = new Font(bf_normal, 16);
      small = new Font(bf_normal, 8);
      BaseFont bf_bold = BaseFont.CreateFont(
        "c://windows/fonts/arialbd.ttf",
        BaseFont.WINANSI,
        BaseFont.EMBEDDED
      );
      bold = new Font(bf_bold, 14);
      // collections
      string year_file = Path.Combine(
        Utility.ResourceCalendar, string.Format("{0}.txt", YEAR)
      );
      if (!File.Exists(year_file)) { 
        throw new ArgumentException(year_file + " NOT FOUND!");
      }
      string content_file = Path.Combine(Utility.ResourceCalendar, CONTENT);
      if (!File.Exists(content_file)) {
        throw new ArgumentException(year_file + " NOT FOUND!");
      }
      using (FileStream fs = new FileStream(
        year_file, FileMode.Open, FileAccess.Read
      ) ) {
        specialDays.Load(fs);
      }
      using (FileStream fs = new FileStream(
        content_file, FileMode.Open, FileAccess.Read
      ) ) {
        content.Load(fs);
      }      
    }
// ---------------------------------------------------------------------------
    /**
     * Creates a PDF document.
     */
    public virtual void Write(Stream stream) {
      // step 1
      using (Document document = new Document(PageSize.A4.Rotate())) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        // "en" => System.Globalization.GregorianCalendar 
        Calendar calendar = new CultureInfo(LANGUAGE, false).Calendar;
        PdfContentByte canvas = writer.DirectContent;
        // Loop over the months
        for (int month = 0; month < 12; month++) {
          int current_month = month + 1;
          DateTime dt = new DateTime(YEAR, current_month, 1, calendar);
          // draw the background
          DrawImageAndText(canvas, dt);
          // create a table with 7 columns
          PdfPTable table = new PdfPTable(7);
          table.TotalWidth = 504;
          // add the name of the month
          table.DefaultCell.BackgroundColor = BaseColor.WHITE;
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
          table.WriteSelectedRows(0, -1, 169, table.TotalHeight + 18, canvas);
          document.NewPage();
        }
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Draws the image of the month to the calendar.
     * @param canvas the direct content layer
     * @param dt the DateTime (to know which picture to use)
     */
    public void DrawImageAndText(PdfContentByte canvas, DateTime dt) {
      string MM = dt.ToString("MM");
      // get the image
      Image img = Image.GetInstance(Path.Combine(
        RESOURCE, MM + ".jpg"
      ));
      img.ScaleToFit(PageSize.A4.Height, PageSize.A4.Width);
      img.SetAbsolutePosition(
        (PageSize.A4.Height - img.ScaledWidth) / 2,
        (PageSize.A4.Width - img.ScaledHeight) / 2
      );
      canvas.AddImage(img);
      // add metadata
      canvas.SaveState();
      canvas.SetCMYKColorFill(0x00, 0x00, 0x00, 0x80);
      Phrase p = new Phrase(string.Format(
          "{0} - \u00a9 Katharine Osborne",
          content[string.Format("{0}.jpg", dt.ToString("MM"))]
        ),
        small
      );
      ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, p, 5, 5, 0);
      p = new Phrase(
        "Calendar generated using iText - example for the book iText in Action 2nd Edition", 
        small
      );
      ColumnText.ShowTextAligned(canvas, Element.ALIGN_RIGHT, p, 839, 5, 0);
      canvas.RestoreState();
    }
// ---------------------------------------------------------------------------   
    /**
     * Creates a PdfPCell with the name of the month
     * @param dt a DateTime
     * @return a PdfPCell with rowspan 7, containing the name of the month
     */
    public PdfPCell GetMonthCell(DateTime dt) {
      PdfPCell cell = new PdfPCell();
      cell.Colspan = 7;
      cell.BackgroundColor = BaseColor.WHITE;
      cell.UseDescender = true;
      Paragraph p = new Paragraph(dt.ToString("MMMM yyyy"), bold);
      p.Alignment = Element.ALIGN_CENTER;
      cell.AddElement(p);
      return cell;
    }
//// ---------------------------------------------------------------------------   
//    /**
//     * Creates a PdfPCell for a specific day
//     * @param dt a DateTime
//     * @return a PdfPCell
//     */
    public PdfPCell GetDayCell(DateTime dt) {
      PdfPCell cell = new PdfPCell();
      cell.Padding = 3;
      // set the background color, based on the type of day
      cell.BackgroundColor = IsSunday(dt)
        ? BaseColor.GRAY
        : IsSpecialDay(dt)
          ? BaseColor.LIGHT_GRAY
          : BaseColor.WHITE
      ;
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
// ---------------------------------------------------------------------------       
    /**
     * Returns true for Sundays.
     * @param dt a DateTime
     * @return true for Sundays
     */
    public bool IsSunday(DateTime dt) {
      return dt.DayOfWeek == DayOfWeek.Sunday ? true : false;
    }
//// ---------------------------------------------------------------------------       
//    /**
//     * Returns true if the date was found in a list with special days (holidays).
//     * @param calendar a date
//     * @return true for holidays
//     */
    public bool IsSpecialDay(DateTime dt) {
      return dt.DayOfWeek == DayOfWeek.Saturday
        ? true
        : specialDays.ContainsKey(dt.ToString("MMdd"))
          ? true
          : false
      ;
    }    
// ===========================================================================
  }
}