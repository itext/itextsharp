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
using kuujinbo.iTextInAction2Ed.Chapter08;

namespace kuujinbo.iTextInAction2Ed.Chapter13 {
  public class ReplaceURL : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const string RESULT1 = "submit1.pdf";
    /** The resulting PDF file. */
    public const string RESULT2 = "submit2.pdf";
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        ReplaceURL form = new ReplaceURL(); 
    	  byte[] pdf = form.CreatePdf();
  	    zip.AddEntry(RESULT1, pdf);
  	    zip.AddEntry(RESULT2, form.ManipulatePdf(pdf));         
        zip.Save(stream);             
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
          
          PushbuttonField button1 = new PushbuttonField(
              writer, new Rectangle(90, 660, 140, 690), "post");
          button1.Text = "POST";
          button1.BackgroundColor = new GrayColor(0.7f);
          button1.Visibility = PushbuttonField.VISIBLE_BUT_DOES_NOT_PRINT;
          PdfFormField submit1 = button1.Field;
          submit1.Action = PdfAction.CreateSubmitForm(
            "/book/request", null,
            PdfAction.SUBMIT_HTML_FORMAT | PdfAction.SUBMIT_COORDINATES
          );
          writer.AddAnnotation(submit1);
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] ManipulatePdf(byte[] src) {
    	PdfReader reader = new PdfReader(src);
    	AcroFields form = reader.AcroFields;
    	AcroFields.Item item = form.GetFieldItem("post");
    	PdfDictionary field = item.GetMerged(0);
    	PdfDictionary action = field.GetAsDict(PdfName.A);
    	PdfDictionary f = action.GetAsDict(PdfName.F);
    	f.Put(PdfName.F, new PdfString("http://NON-EXISTENT-DOMAIN.invalid/"));
		  using (MemoryStream ms = new MemoryStream()) {
    	  using (PdfStamper stamper = new PdfStamper(reader, ms)) {
    	  }
    	  return ms.ToArray();
      }	
    }
// ===========================================================================
  }
}