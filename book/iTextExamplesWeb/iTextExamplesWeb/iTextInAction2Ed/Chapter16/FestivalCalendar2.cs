/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.richmedia;

namespace kuujinbo.iTextInAction2Ed.Chapter16 {
  public class FestivalCalendar2 : IWriter {
// ===========================================================================
    /** The path to a Flash application. */
    public readonly String RESOURCE = Path.Combine(
      Utility.ResourceSwf, "FestivalCalendar2.swf"
    );
    /** The path to a JavaScript file. */
    public readonly String JS = Path.Combine(
      Utility.ResourceJavaScript, "show_date.js"
    );
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        writer.SetPdfVersion(PdfWriter.PDF_VERSION_1_7);
        writer.AddDeveloperExtension(
          PdfDeveloperExtension.ADOBE_1_7_EXTENSIONLEVEL3
        );
        // step 3
        document.Open();
        // step 4
        writer.AddJavaScript(File.ReadAllText(JS));

        // we prepare a RichMediaAnnotation
        RichMediaAnnotation richMedia = new RichMediaAnnotation(
          writer, new Rectangle(36, 560, 561, 760)
        );
        // we embed the swf file
        PdfFileSpecification fs = PdfFileSpecification.FileEmbedded(
          writer, RESOURCE, "FestivalCalendar2.swf", null
        );
        // we declare the swf file as an asset
        PdfIndirectReference asset = richMedia.AddAsset(
          "FestivalCalendar2.swf", fs
        );
        // we create a configuration
        RichMediaConfiguration configuration = new RichMediaConfiguration(
          PdfName.FLASH
        );
        RichMediaInstance instance = new RichMediaInstance(PdfName.FLASH);
        instance.Asset = asset;
        configuration.AddInstance(instance);
        // we add the configuration to the annotation
        PdfIndirectReference configurationRef = richMedia.AddConfiguration(
          configuration
        );
        // activation of the rich media
        RichMediaActivation activation = new RichMediaActivation();
        activation.Configuration = configurationRef;
        richMedia.Activation = activation;
        // we add the annotation
        PdfAnnotation richMediaAnnotation = richMedia.CreateAnnotation();
        richMediaAnnotation.Flags = PdfAnnotation.FLAGS_PRINT;
        writer.AddAnnotation(richMediaAnnotation);
        
        String[] days = new String[] {
          "2011-10-12", "2011-10-13", "2011-10-14", "2011-10-15",
          "2011-10-16", "2011-10-17", "2011-10-18", "2011-10-19"
        };
        for (int i = 0; i < days.Length; i++) {
          Rectangle rect = new Rectangle(36 + (65 * i), 765, 100 + (65 * i), 780);
          PushbuttonField button = new PushbuttonField(writer, rect, "button" + i);
          button.BackgroundColor = new GrayColor(0.75f);
          button.BorderStyle = PdfBorderDictionary.STYLE_BEVELED;
          button.TextColor = GrayColor.GRAYBLACK;
          button.FontSize = 12;
          button.Text = days[i];
          button.Layout = PushbuttonField.LAYOUT_ICON_LEFT_LABEL_RIGHT;
          button.ScaleIcon = PushbuttonField.SCALE_ICON_ALWAYS;
          button.ProportionalIcon = true;
          button.IconHorizontalAdjustment = 0;
          PdfFormField field = button.Field;
          RichMediaCommand command = new RichMediaCommand(
            new PdfString("getDateInfo")
          );
          command.Arguments = new PdfString(days[i]);
          RichMediaExecuteAction action = new RichMediaExecuteAction(
            richMediaAnnotation.IndirectReference, command
          );
          field.Action = action;
          writer.AddAnnotation(field);
        }
        TextField text = new TextField(
          writer, new Rectangle(36, 785, 559, 806), "date"
        );
        text.Options = TextField.READ_ONLY;
        writer.AddAnnotation(text.GetTextField());
      }
    }
// ===========================================================================
	}
}