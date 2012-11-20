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
  public class ChoiceFields : IWriter, IPdfPCellEvent {
// ===========================================================================
    public const string RESULT1 = "choice_fields.pdf";
    /** The resulting PDF. */
    public const String RESULT2 = "choice_filled.pdf";    
    protected int cf;
    public readonly string[] LANGUAGES = {
      "English", "German", "French", "Spanish", "Dutch"
    };
    public readonly string[] EXPORTVALUES = {
      "EN", "DE", "FR", "ES", "NL"
    };    
// --------------------------------------------------------------------------- 
    public ChoiceFields() {}
    public ChoiceFields(int cf) {
      this.cf = cf;
    }
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        ChoiceFields fields = new ChoiceFields(0);
        byte[] pdf = fields.CreatePdf();
        zip.AddEntry(RESULT1, pdf);       
        zip.AddEntry(RESULT2, fields.ManipulatePdf(pdf));       
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
          form.SetField("choice_1", "NL");
          form.SetListSelection("choice_2", new String[]{"German", "Spanish"});
          String[] languages = form.GetListOptionDisplay("choice_3");
          String[] exportvalues = form.GetListOptionExport("choice_3");
          int n = languages.Length;
          String[] new_languages = new String[n + 2];
          String[] new_exportvalues = new String[n + 2];
          for (int i = 0; i < n; i++) {
              new_languages[i] = languages[i];
              new_exportvalues[i] = exportvalues[i];
          }
          new_languages[n] = "Chinese";
          new_exportvalues[n] = "CN";
          new_languages[n + 1] = "Japanese";
          new_exportvalues[n + 1] = "JP";
          form.SetListOption("choice_3", new_exportvalues, new_languages);
          form.SetField("choice_3", "CN");
          form.SetField("choice_4", "Japanese");
        }
        return ms.ToArray();
      }
    }  
// ---------------------------------------------------------------------------
    /**
     * Creates a PDF document.
     * @param filename the path to the new PDF document
     */
    public byte[] CreatePdf() {
      using (MemoryStream ms = new MemoryStream()) {
        using (Document document = new Document()) {
          PdfWriter.GetInstance(document, ms);
          document.Open();

          PdfPCell cell;
          PdfPTable table = new PdfPTable(2);

          table.DefaultCell.Border = Rectangle.NO_BORDER;
          table.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
          table.AddCell("Language of the movie:");
          cell = new PdfPCell();
          cell.CellEvent = new ChoiceFields(1);
          table.AddCell(cell);
          table.AddCell("Subtitle languages:");
          cell = new PdfPCell();
          cell.CellEvent = new ChoiceFields(2);
          cell.FixedHeight = 70;
          table.AddCell(cell);
          table.AddCell("Select preferred language:");
          cell = new PdfPCell();
          cell.CellEvent = new ChoiceFields(3);
          table.AddCell(cell);
          table.AddCell("Language of the director:");
          cell = new PdfPCell();
          cell.CellEvent = new ChoiceFields(4);
          table.AddCell(cell);
          
          document.Add(table);
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------
    public void CellLayout(PdfPCell cell, Rectangle rectangle, 
        PdfContentByte[] canvases) 
    {
      PdfWriter writer = canvases[0].PdfWriter;
      TextField text = new TextField(
        writer, rectangle, string.Format("choice_{0}", cf)
      );
      text.BackgroundColor = new GrayColor(0.75f);
      switch(cf) {
        case 1:
          text.Choices = LANGUAGES;
          text.ChoiceExports = EXPORTVALUES;
          text.ChoiceSelection = 2;
          writer.AddAnnotation(text.GetListField());
          break;
        case 2:
          text.Choices = LANGUAGES;
          text.BorderColor = BaseColor.GREEN;
          text.BorderStyle = PdfBorderDictionary.STYLE_DASHED;
          text.Options = TextField.MULTISELECT;
          List<int> selections = new List<int>();
          selections.Add(0);
          selections.Add(2);
          text.ChoiceSelections = selections;
          PdfFormField field = text.GetListField();
          writer.AddAnnotation(field);
          break;
        case 3:
          text.BorderColor = BaseColor.RED;
          text.BackgroundColor = BaseColor.GRAY;
          text.Choices = LANGUAGES;
          text.ChoiceExports = EXPORTVALUES;
          text.ChoiceSelection = 4;
          writer.AddAnnotation(text.GetComboField());
          break;
        case 4:
          text.Choices = LANGUAGES;
          text.Options = TextField.EDIT;
          writer.AddAnnotation(text.GetComboField());
          break;
      }
    }
// ===========================================================================
  }
}