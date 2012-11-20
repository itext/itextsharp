/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter03 {
  public class FestivalOpening : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      string RESOURCE = Utility.ResourcePosters;
      // step 1
      using (Document document = new Document(PageSize.POSTCARD, 30, 30, 30, 30)) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        // Create and add a Paragraph
        Paragraph p = new Paragraph(
          "Foobar Film Festival", 
          new Font(Font.FontFamily.HELVETICA, 22)
        );
        p.Alignment = Element.ALIGN_CENTER;
        document.Add(p);
        // Create and add an Image
        Image img = Image.GetInstance(Path.Combine(
          Utility.ResourceImage, "loa.jpg"
        ));
        img.SetAbsolutePosition(
          (PageSize.POSTCARD.Width - img.ScaledWidth) / 2,
          (PageSize.POSTCARD.Height - img.ScaledHeight) / 2
        );
        document.Add(img);
        // Now we go to the next page
        document.NewPage();
        document.Add(p);
        document.Add(img);
        // Add text on top of the image
        PdfContentByte over = writer.DirectContent;
        over.SaveState();
        float sinus = (float)Math.Sin(Math.PI / 60);
        float cosinus = (float)Math.Cos(Math.PI / 60);
        BaseFont bf = BaseFont.CreateFont();
        over.BeginText();
        over.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE);
        over.SetLineWidth(1.5f);
        over.SetRGBColorStroke(0xFF, 0x00, 0x00);
        over.SetRGBColorFill(0xFF, 0xFF, 0xFF);
        over.SetFontAndSize(bf, 36);
        over.SetTextMatrix(cosinus, sinus, -sinus, cosinus, 50, 324);
        over.ShowText("SOLD OUT");
        over.EndText();
        over.RestoreState();
        // Add a rectangle under the image
        PdfContentByte under = writer.DirectContentUnder;
        under.SaveState();
        under.SetRGBColorFill(0xFF, 0xD7, 0x00);
        under.Rectangle(5, 5,
          PageSize.POSTCARD.Width - 10, 
          PageSize.POSTCARD.Height - 10
        );
        under.Fill();
        under.RestoreState();        
      }
    }
// ===========================================================================
  }
}