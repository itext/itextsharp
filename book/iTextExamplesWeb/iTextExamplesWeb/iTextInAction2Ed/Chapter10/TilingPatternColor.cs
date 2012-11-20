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
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter10 {
  public class TilingPatternColor : DeviceColor {
// ===========================================================================
    /** The resulting PDF file. */
    public const string RESULT = "tiling_pattern.pdf";
    /** An image that will be used for a pattern color. */
    public static string RESOURCE = Path.Combine(Utility.ResourceImage, "info.png");
// --------------------------------------------------------------------------- 
    public override void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        TilingPatternColor t = new TilingPatternColor();
        zip.AddEntry(RESULT, t.CreatePdf());
        zip.AddFile(RESOURCE, "");
        zip.Save(stream);             
      }
    }
// --------------------------------------------------------------------------- 
    /**
     * Creates a PDF document.
     */
    public byte[] CreatePdf()  {
      using (MemoryStream ms = new MemoryStream()) {
        // step 1
        using (Document document = new Document()) {
          // step 2
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          // step 3
          document.Open();
          // step 4
          PdfContentByte canvas = writer.DirectContent;
          PdfPatternPainter square
              = canvas.CreatePattern(15, 15);
          square.SetColorFill(new BaseColor(0xFF, 0xFF, 0x00));
          square.SetColorStroke(new BaseColor(0xFF, 0x00, 0x00));
          square.Rectangle(5, 5, 5, 5);
          square.FillStroke();
          
          PdfPatternPainter ellipse
              = canvas.CreatePattern(15, 10, 20, 25);
          ellipse.SetColorFill(new BaseColor(0xFF, 0xFF, 0x00));
          ellipse.SetColorStroke(new BaseColor(0xFF, 0x00, 0x00));
          ellipse.Ellipse(2f, 2f, 13f, 8f);
          ellipse.FillStroke();
          
          PdfPatternPainter circle
              = canvas.CreatePattern(15, 15, 10, 20, BaseColor.BLUE);
          circle.Circle(7.5f, 7.5f, 2.5f);
          circle.Fill();
          
          PdfPatternPainter line
              = canvas.CreatePattern(5, 10, null);
          line.SetLineWidth(1);
          line.MoveTo(3, -1);
          line.LineTo(3, 11);
          line.Stroke();
          
          Image img = Image.GetInstance(RESOURCE);
          img.ScaleAbsolute(20, 20);
          img.SetAbsolutePosition(0, 0);
          PdfPatternPainter img_pattern
              = canvas.CreatePattern(20, 20, 20, 20);
          img_pattern.AddImage(img);
          img_pattern.SetPatternMatrix(-0.5f, 0f, 0f, 0.5f, 0f, 0f);
          
          ColorRectangle(canvas, new PatternColor(square), 36, 696, 126, 126);
          ColorRectangle(canvas, new PatternColor(ellipse), 180, 696, 126, 126);
          ColorRectangle(canvas, new PatternColor(circle), 324, 696, 126, 126);
          ColorRectangle(canvas, new PatternColor(line), 36, 552, 126, 126);
          ColorRectangle(canvas, new PatternColor(img_pattern), 36, 408, 126, 126);

          canvas.SetPatternFill(line, BaseColor.RED);
          canvas.Ellipse(180, 552, 306, 678);
          canvas.FillStroke();
          canvas.SetPatternFill(circle, BaseColor.GREEN);
          canvas.Ellipse(324, 552, 450, 678);
          canvas.FillStroke();
          
          canvas.SetPatternFill(img_pattern);
          canvas.Ellipse(180, 408, 450, 534);
          canvas.FillStroke();
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}