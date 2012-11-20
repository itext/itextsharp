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
using System.Data;
using System.Data.Common;
using System.Linq; 
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using kuujinbo.iTextInAction2Ed.Chapter04;

namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class Advertisement : IWriter {
// ===========================================================================
    /** Path to a resource. */
    public static String RESOURCE = Path.Combine(Utility.ResourcePdf, "hero.pdf");
    /** Path to a resource. */
    /** The resulting PDF file. */
    public const String RESULT = "advertisement.pdf";
    /** Path to a resource. */
    public const string IMAGE = "close.png";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        NestedTables n = new NestedTables(); 
        byte[] pdf = Utility.PdfBytes(n);
        Advertisement a = new Advertisement();
        zip.AddEntry(RESULT, a.ManipulatePdf(pdf));       
        zip.AddEntry(Utility.ResultFileName(n.ToString() + ".pdf"), pdf);
        zip.AddFile(RESOURCE, "");
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] ManipulatePdf(byte[] src) {
      // Create a reader for the original document
      PdfReader reader = new PdfReader(src);
      // Create a reader for the advertisement resource
      PdfReader ad = new PdfReader(RESOURCE); 
      using (MemoryStream ms = new MemoryStream()) {
        // Create a stamper
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {           
          // Create the advertisement annotation for the menubar
          Rectangle rect = new Rectangle(400, 772, 545, 792);
          PushbuttonField button = new PushbuttonField(
            stamper.Writer, rect, "click"
          );
          button.BackgroundColor = BaseColor.RED;
          button.BorderColor = BaseColor.RED;
          button.FontSize = 10;
          button.Text = "Close this advertisement";
          button.Image = Image.GetInstance(
            Path.Combine(Utility.ResourceImage, IMAGE)
          );
          button.Layout = PushbuttonField.LAYOUT_LABEL_LEFT_ICON_RIGHT;
          button.IconHorizontalAdjustment = 1;
          PdfFormField menubar = button.Field;
          String js = "var f1 = getField('click'); f1.display = display.hidden;"
            + "var f2 = getField('advertisement'); f2.display = display.hidden;"
          ;
          menubar.Action = PdfAction.JavaScript(js, stamper.Writer);
          // Add the annotation
          stamper.AddAnnotation(menubar, 1);
          // Create the advertisement annotation for the content
          rect = new Rectangle(400, 550, 545, 772);
          button = new PushbuttonField(
            stamper.Writer, rect, "advertisement"
          );
          button.BackgroundColor = BaseColor.WHITE;
          button.BorderColor = BaseColor.RED;
          button.Text = "Buy the book iText in Action 2nd edition";
          button.Template = stamper.GetImportedPage(ad, 1);
          button.Layout = PushbuttonField.LAYOUT_ICON_TOP_LABEL_BOTTOM;
          PdfFormField advertisement = button.Field;
          advertisement.Action = new PdfAction(
            "http://www.1t3xt.com/docs/book.php"
          );
          // Add the annotation
          stamper.AddAnnotation(advertisement, 1);      
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}