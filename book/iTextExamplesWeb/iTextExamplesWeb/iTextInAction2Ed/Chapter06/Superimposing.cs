/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.Com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class Superimposing : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const string SOURCE = "opening.pdf";
    /** The resulting PDF. */
    public const string RESULT = "festival_opening.pdf";    
// ---------------------------------------------------------------------------        
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) { 
        byte[] pdf = new Superimposing().CreatePdf();
        // Create a reader
        PdfReader reader = new PdfReader(pdf);
        using (MemoryStream ms = new MemoryStream()) {     
          // step 1
          using (Document document = new Document(PageSize.POSTCARD)) {
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            // step 3
            document.Open();
            // step 4
            PdfContentByte canvas = writer.DirectContent;
            PdfImportedPage page;
            for (int i = 1; i <= reader.NumberOfPages; i++) {
              page = writer.GetImportedPage(reader, i);
              canvas.AddTemplate(page, 1f, 0, 0, 1, 0, 0);
            }
          } 
          zip.AddEntry(RESULT, ms.ToArray());
        }
        zip.AddEntry(SOURCE, pdf);
        zip.Save(stream);            
      }        
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a PDF document for PdfReader.
     */
    public byte[] CreatePdf() {
      using (MemoryStream ms = new MemoryStream()) {
        // step 1
        using (Document document = new Document(PageSize.POSTCARD, 30, 30, 30, 30)) {
          // step 2
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          // step 3
          document.Open();
          // step 4
          PdfContentByte under = writer.DirectContentUnder;
          // page 1: rectangle
          under.SetRGBColorFill(0xFF, 0xD7, 0x00);
          under.Rectangle(5, 5,
              PageSize.POSTCARD.Width - 10, PageSize.POSTCARD.Height - 10);
          under.Fill();
          document.NewPage();
          // page 2: image
          Image img = Image.GetInstance(Path.Combine(
            Utility.ResourceImage, "loa.jpg"
          ));
          img.SetAbsolutePosition(
            (PageSize.POSTCARD.Width - img.ScaledWidth) / 2,
            (PageSize.POSTCARD.Height - img.ScaledHeight) / 2
          );
          document.Add(img);
          document.NewPage();
          // page 3: the words "Foobar Film Festival"
          Paragraph p = new Paragraph(
            "Foobar Film Festival", new Font(Font.FontFamily.HELVETICA, 22)
          );
          p.Alignment = Element.ALIGN_CENTER;
          document.Add(p);
          document.NewPage();
          // page 4: the words "SOLD OUT"
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
          over.SetTextMatrix(0, 0);
          over.EndText();
          over.RestoreState();
        }
        return ms.ToArray();
      }
    }  
// ===========================================================================
  }
}