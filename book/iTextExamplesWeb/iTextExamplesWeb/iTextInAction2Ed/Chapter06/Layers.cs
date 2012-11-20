/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class Layers : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const string SOURCE = "layers_orig.pdf";
    /** The resulting PDF. */
    public const string RESULT = "layers.pdf";    
// ---------------------------------------------------------------------------        
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) { 
        byte[] pdf = new Layers().CreatePdf();        

        // Create a reader
        PdfReader reader = new PdfReader(pdf);
        // step 1
        using (MemoryStream ms = new MemoryStream()) { 
          using (Document document = new Document(PageSize.A5.Rotate())) {
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            // step 3
            document.Open();
            // step 4
            PdfContentByte canvas = writer.DirectContent;
            PdfImportedPage page;
            BaseFont bf = BaseFont.CreateFont(
              BaseFont.ZAPFDINGBATS, "", BaseFont.EMBEDDED
            );
            for (int i = 0; i < reader.NumberOfPages; ) {
              page = writer.GetImportedPage(reader, ++i);
              canvas.AddTemplate(page, 1f, 0, 0.4f, 0.4f, 72, 50 * i);
              canvas.BeginText();
              canvas.SetFontAndSize(bf, 20);
              canvas.ShowTextAligned(
                Element.ALIGN_CENTER,
                ((char)(181 + i)).ToString(),
                496, 150 + 50 * i, 0
              );
              canvas.EndText();
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
      // step 1
      using (MemoryStream ms = new MemoryStream()) {
        using (Document document = new Document(PageSize.POSTCARD, 30, 30, 30, 30)) {
        // step 2
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          // step 3
          document.Open();
          // step 4
          PdfContentByte under = writer.DirectContentUnder;
          // Page 1: a rectangle
          DrawRectangle(
            under, PageSize.POSTCARD.Width, PageSize.POSTCARD.Height
          );
          under.SetRGBColorFill(0xFF, 0xD7, 0x00);
          under.Rectangle(
            5, 5, PageSize.POSTCARD.Width - 10, PageSize.POSTCARD.Height - 10
          );
          under.Fill();
          document.NewPage();
          // Page 2: an image
          DrawRectangle(
            under, PageSize.POSTCARD.Width, PageSize.POSTCARD.Height
          );
          Image img = Image.GetInstance(Path.Combine(
            Utility.ResourceImage, "loa.jpg"
          ));
          img.SetAbsolutePosition(
            (PageSize.POSTCARD.Width - img.ScaledWidth) / 2,
            (PageSize.POSTCARD.Height - img.ScaledHeight) / 2
          );
          document.Add(img);
          document.NewPage();
          // Page 3: the words "Foobar Film Festival"
          DrawRectangle(
            under, PageSize.POSTCARD.Width, PageSize.POSTCARD.Height
          );
          Paragraph p = new Paragraph(
            "Foobar Film Festival", new Font(Font.FontFamily.HELVETICA, 22)
          );
          p.Alignment = Element.ALIGN_CENTER;
          document.Add(p);
          document.NewPage();
          // Page 4: the words "SOLD OUT"
          DrawRectangle(under, PageSize.POSTCARD.Width, PageSize.POSTCARD.Height);
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
// ---------------------------------------------------------------------------    
    /**
     * Draws a rectangle
     * @param content the direct content layer
     * @param width the width of the rectangle
     * @param height the height of the rectangle
     */
    public static void DrawRectangle(
      PdfContentByte content, float width, float height) 
    {
      content.SaveState();
      PdfGState state = new PdfGState();
      state.FillOpacity = 0.6f;
      content.SetGState(state);
      content.SetRGBColorFill(0xFF, 0xFF, 0xFF);
      content.SetLineWidth(3);
      content.Rectangle(0, 0, width, height);
      content.FillStroke();
      content.RestoreState();
    }   
// ===========================================================================
  }
}