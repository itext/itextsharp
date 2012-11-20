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
  public class Buttons : IWriter {
// ===========================================================================
    public const string RESULT1 = "buttons.pdf";
    /** The resulting PDF. */
    public const String RESULT2 = "buttons_filled.pdf";    
    /** Path to the resources. */
    public const string RESOURCE = "buttons.js";
    public const string IMAGE = "info.png";
    public readonly string[] LANGUAGES = { 
      "English", "German", "French", "Spanish", "Dutch" 
    };
    protected string jsString;
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        Buttons buttons = new Buttons();
        byte[] r1 = buttons.CreatePdf();
        zip.AddEntry(RESULT1, r1);
        zip.AddEntry(RESULT2, buttons.ManipulatePdf(r1));
        zip.AddEntry(RESOURCE, buttons.jsString);                
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
          String[] radiostates = form.GetAppearanceStates("language");
          form.SetField("language", radiostates[4]);
          for (int i = 0; i < LANGUAGES.Length; i++) {
            String[] checkboxstates = form.GetAppearanceStates("English");
            form.SetField(LANGUAGES[i], checkboxstates[i % 2 == 0 ? 1 : 0]);
          }
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
        using (Document document = new Document() ) {
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          document.Open();
          jsString = File.ReadAllText(
            Path.Combine(Utility.ResourceJavaScript, RESOURCE)
          );
          writer.AddJavaScript(jsString);
          
          PdfContentByte canvas = writer.DirectContent;
          Font font = new Font(Font.FontFamily.HELVETICA, 18);
          Rectangle rect;
          PdfFormField field;
          PdfFormField radiogroup = PdfFormField.CreateRadioButton(writer, true);
          radiogroup.FieldName = "language";
          RadioCheckField radio;
          for (int i = 0; i < LANGUAGES.Length; i++) {
            rect = new Rectangle(40, 806 - i * 40, 60, 788 - i * 40);
            radio = new RadioCheckField(writer, rect, null, LANGUAGES[i]);
            radio.BorderColor = GrayColor.GRAYBLACK;
            radio.BackgroundColor = GrayColor.GRAYWHITE;
            radio.CheckType = RadioCheckField.TYPE_CIRCLE;
            field = radio.RadioField;
            radiogroup.AddKid(field);
            ColumnText.ShowTextAligned(
              canvas, Element.ALIGN_LEFT, 
              new Phrase(LANGUAGES[i], font), 70, 790 - i * 40, 0
            );
          }
          writer.AddAnnotation(radiogroup);

          PdfAppearance[] onOff = new PdfAppearance[2];
          onOff[0] = canvas.CreateAppearance(20, 20);
          onOff[0].Rectangle(1, 1, 18, 18);
          onOff[0].Stroke();
          onOff[1] = canvas.CreateAppearance(20, 20);
          onOff[1].SetRGBColorFill(255, 128, 128);
          onOff[1].Rectangle(1, 1, 18, 18);
          onOff[1].FillStroke();
          onOff[1].MoveTo(1, 1);
          onOff[1].LineTo(19, 19);
          onOff[1].MoveTo(1, 19);
          onOff[1].LineTo(19, 1);
          onOff[1].Stroke();
          RadioCheckField checkbox;
          for (int i = 0; i < LANGUAGES.Length; i++) {
            rect = new Rectangle(180, 806 - i * 40, 200, 788 - i * 40);
            checkbox = new RadioCheckField(writer, rect, LANGUAGES[i], "on");
            // checkbox.CheckType = RadioCheckField.TYPE_CHECK;
            field = checkbox.CheckField;
            field.SetAppearance(
              PdfAnnotation.APPEARANCE_NORMAL, "Off", onOff[0]
            );
            field.SetAppearance(
              PdfAnnotation.APPEARANCE_NORMAL, "On", onOff[1]
            );
            writer.AddAnnotation(field);
            ColumnText.ShowTextAligned(
              canvas, Element.ALIGN_LEFT, 
              new Phrase(LANGUAGES[i], font), 210,
              790 - i * 40, 0
            );
          }
          rect = new Rectangle(300, 806, 370, 788);
          PushbuttonField button = new PushbuttonField(writer, rect, "Buttons");
          button.BackgroundColor = new GrayColor(0.75f);
          button.BorderColor = GrayColor.GRAYBLACK;
          button.BorderWidth = 1;
          button.BorderStyle = PdfBorderDictionary.STYLE_BEVELED;
          button.TextColor = GrayColor.GRAYBLACK ;
          button.FontSize = 12;
          button.Text = "Push me";
          button.Layout = PushbuttonField.LAYOUT_ICON_LEFT_LABEL_RIGHT;
          button.ScaleIcon = PushbuttonField.SCALE_ICON_ALWAYS;
          button.ProportionalIcon = true;
          button.IconHorizontalAdjustment = 0;
          button.Image = Image.GetInstance(
            Path.Combine(Utility.ResourceImage, IMAGE)
          );
          field = button.Field;
          field.Action = PdfAction.JavaScript("this.showButtonState()", writer);
          writer.AddAnnotation(field);
          
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}