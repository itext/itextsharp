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
  public class FestivalCalendar1 : IWriter {
// ===========================================================================
    /** The path to a Flash application. */
    public readonly String RESOURCE = Path.Combine(
      Utility.ResourceSwf, "FestivalCalendar1.swf"
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
        // we prepare a RichMediaAnnotation
        RichMediaAnnotation richMedia = new RichMediaAnnotation(
          writer, new Rectangle(36, 400, 559,806)
        );
        // we embed the swf file
        PdfFileSpecification fs = PdfFileSpecification.FileEmbedded(
          writer, RESOURCE, "FestivalCalendar1.swf", null
        );
        // we declare the swf file as an asset
        PdfIndirectReference asset = richMedia.AddAsset(
          "FestivalCalendar1.swf", fs
        );
        // we create a configuration
        RichMediaConfiguration configuration = new RichMediaConfiguration(
          PdfName.FLASH
        );
        RichMediaInstance instance = new RichMediaInstance(PdfName.FLASH);
        RichMediaParams flashVars = new RichMediaParams();
        String vars = "&day=2011-10-13";
        flashVars.FlashVars = vars;
        instance.Params = flashVars;
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
      }
    }
// ===========================================================================
	}
}