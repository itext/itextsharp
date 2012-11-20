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
using System.Linq; 
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter08 {
  public class TextFields : IWriter, IPdfPCellEvent {
// ===========================================================================
    public const string RESULT1 = "text_fields.pdf";
    public const String RESULT2 = "text_filled.pdf";
    protected int tf;
// --------------------------------------------------------------------------- 
    public TextFields() {}
    public TextFields(int tf) {
      this.tf = tf;
    }
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        TextFields example = new TextFields(0);
        byte[] pdf = example.CreatePdf();
        zip.AddEntry(RESULT1, pdf);       
        zip.AddEntry(RESULT2, example.ManipulatePdf(pdf));       
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] ManipulatePdf(byte[] src) {
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          AcroFields form = stamper.AcroFields;
          form.SetField("text_1", "Bruno Lowagie");
          form.SetFieldProperty("text_2", "fflags", 0, null);
          form.SetFieldProperty("text_2", "bordercolor", BaseColor.RED, null);
          form.SetField("text_2", "bruno");
          form.SetFieldProperty("text_3", "clrfflags", TextField.PASSWORD, null);
          form.SetFieldProperty("text_3", "setflags", PdfAnnotation.FLAGS_PRINT, null);
          form.SetField("text_3", "12345678", "xxxxxxxx");
          form.SetFieldProperty("text_4", "textsize", 12f, null);
          form.RegenerateField("text_4");
        }
        return ms.ToArray();
      }
    }  
// ---------------------------------------------------------------------------
    /**
     * Creates a PDF document.
     */
    public byte[] CreatePdf() {
      using (MemoryStream ms = new MemoryStream()) {
        using (Document document = new Document()) {
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          document.Open();

          PdfPCell cell;
          PdfPTable table = new PdfPTable(2);
          table.SetWidths(new int[]{ 1, 2 });

          table.AddCell("Name:");
          cell = new PdfPCell();
          cell.CellEvent = new TextFields(1);
          table.AddCell(cell);
          
          table.AddCell("Loginname:");
          cell = new PdfPCell();
          cell.CellEvent = new TextFields(2);
          table.AddCell(cell);
          
          table.AddCell("Password:");
          cell = new PdfPCell();
          cell.CellEvent = new TextFields(3);
          table.AddCell(cell);
          
          table.AddCell("Reason:");
          cell = new PdfPCell();
          cell.CellEvent = new TextFields(4);
          cell.FixedHeight = 60;
          table.AddCell(cell);

          document.Add(table);
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------
    public void CellLayout(PdfPCell cell, Rectangle rectangle, PdfContentByte[] canvases) {
      PdfWriter writer = canvases[0].PdfWriter;
      TextField text = new TextField(
        writer, rectangle, string.Format("text_{0}", tf)
      );
      text.BackgroundColor = new GrayColor(0.75f);
      switch(tf) {
        case 1:
          text.BorderStyle = PdfBorderDictionary.STYLE_BEVELED;
          text.Alignment = Element.ALIGN_RIGHT;
          text.Text = "Enter your name here...";
          text.FontSize = 0;
          text.Alignment = Element.ALIGN_CENTER;
          text.Options = TextField.REQUIRED;
          break;
        case 2:
          text.MaxCharacterLength = 8;
          text.Options = TextField.COMB;
          text.BorderStyle = PdfBorderDictionary.STYLE_SOLID;
          text.BorderColor = BaseColor.BLUE;
          text.BorderWidth = 2;
          break;
        case 3:
          text.BorderStyle = PdfBorderDictionary.STYLE_INSET;
          text.Options = TextField.PASSWORD;
          text.Visibility = TextField.VISIBLE_BUT_DOES_NOT_PRINT;
          break;
        case 4:
          text.BorderStyle = PdfBorderDictionary.STYLE_DASHED;
          text.BorderColor = BaseColor.RED;
          text.BorderWidth = 2;
          text.FontSize = 8;
          text.Text = "Enter the reason why you want to win "
              + "a free accreditation for the Foobar Film Festival";
          text.Options = TextField.MULTILINE | TextField.REQUIRED;
          break;
      }
      PdfFormField field = text.GetTextField();
      if (tf == 3) {
        field.UserName = "Choose a password";
      }
      writer.AddAnnotation(field);
    }
// ===========================================================================
  }
}