/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using System.Collections; 
using System.Collections.Generic; 
using System.Linq; 
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter08 {
  public class Subscribe : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String FORM = "subscribe.pdf";
    /** The resulting PDFs. */
    public const String RESULT = "filled_form_{0}.pdf";    
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        Subscribe subscribe = new Subscribe(); 
        byte[] pdf = subscribe.CreatePdf();
        zip.AddEntry(FORM, pdf);
        Dictionary<string, TextField> fieldCache = 
            new Dictionary<string, TextField>();        
        zip.AddEntry(string.Format(RESULT, 1), 
          subscribe.ManipulatePdf(pdf, fieldCache, "Bruno Lowagie", "blowagie"
        ));
        zip.AddEntry(string.Format(RESULT, 2), 
          subscribe.ManipulatePdf(pdf, fieldCache, "Paulo Soares", "psoares"
        ));
        zip.AddEntry(string.Format(RESULT, 3), 
          subscribe.ManipulatePdf(pdf, fieldCache, "Mark Storer", "mstorer"
        ));        
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------
    public byte[] ManipulatePdf(byte[] src, Dictionary<string, TextField> cache,
        string name, string login) 
    {
      using (MemoryStream ms = new MemoryStream()) {
        PdfReader reader = new PdfReader(src);
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          AcroFields form = stamper.AcroFields;
          form.FieldCache = cache;
          form.SetExtraMargin(2, 0);
          form.RemoveField("personal.password");
          form.SetField("personal.name", name);        
          form.SetField("personal.loginname", login);
          form.RenameField("personal.reason", "personal.motivation");
          form.SetFieldProperty(
            "personal.loginname", "setfflags", TextField.READ_ONLY, null
          );        
          stamper.FormFlattening = true;
          stamper.PartialFormFlattening("personal.name");
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
          PdfFormField personal = PdfFormField.CreateEmpty(writer);
          personal.FieldName = "personal";
          PdfPTable table = new PdfPTable(3);
          PdfPCell cell;        
          
          table.AddCell("Your name:");
          cell = new PdfPCell();
          cell.Colspan = 2;
          TextField field = new TextField(writer, new Rectangle(0, 0), "name");
          field.FontSize = 12;
          cell.CellEvent = new ChildFieldEvent(
            personal, field.GetTextField(), 1
          );
          table.AddCell(cell);
          table.AddCell("Login:");
          cell = new PdfPCell();
          field = new TextField(writer, new Rectangle(0, 0), "loginname");
          field.FontSize = 12;
          cell.CellEvent = new ChildFieldEvent(
            personal, field.GetTextField(), 1
          );
          table.AddCell(cell);
          cell = new PdfPCell();
          field = new TextField(writer, new Rectangle(0, 0), "password");
          field.Options = TextField.PASSWORD;
          field.FontSize = 12;
          cell.CellEvent = new ChildFieldEvent(
            personal, field.GetTextField(), 1
          );
          table.AddCell(cell);
          table.AddCell("Your motivation:");
          cell = new PdfPCell();
          cell.Colspan = 2;
          cell.FixedHeight = 60;
          field = new TextField(writer, new Rectangle(0, 0), "reason");
          field.Options = TextField.MULTILINE;
          field.FontSize = 12;
          cell.CellEvent = new ChildFieldEvent(
            personal, field.GetTextField(), 1
          );
          table.AddCell(cell);
          document.Add(table);
          writer.AddAnnotation(personal);
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}