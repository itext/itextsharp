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
  public class DeviceColor : IWriter {
// ===========================================================================
    public virtual void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfContentByte canvas = writer.DirectContent;
        // RGB Colors
        ColorRectangle(canvas, new BaseColor(0x00, 0x00, 0x00), 
          36, 770, 36, 36
        );
        ColorRectangle(canvas, new BaseColor(0x00, 0x00, 0xFF), 
          90, 770, 36, 36
        );
        ColorRectangle(canvas, new BaseColor(0x00, 0xFF, 0x00), 
          144, 770, 36, 36
        );
        ColorRectangle(canvas, new BaseColor(0xFF, 0x00, 0x00), 
          198, 770, 36, 36
        );
        ColorRectangle(canvas, new BaseColor(0f, 1f, 1f), 252, 770, 36, 36);
        ColorRectangle(canvas, new BaseColor(1f, 0f, 1f), 306, 770, 36, 36);
        ColorRectangle(canvas, new BaseColor(1f, 1f, 0f), 360, 770, 36, 36);
        ColorRectangle(canvas, BaseColor.BLACK, 416, 770, 36, 36);
        ColorRectangle(canvas, BaseColor.LIGHT_GRAY, 470, 770, 36, 36);
        // CMYK Colors
        ColorRectangle(canvas, new CMYKColor(0x00, 0x00, 0x00, 0x00), 
          36, 716, 36, 36
        );
        ColorRectangle(canvas, new CMYKColor(0x00, 0x00, 0xFF, 0x00), 
          90, 716, 36, 36
        );
        ColorRectangle(canvas, new CMYKColor(0x00, 0x00, 0xFF, 0xFF), 
          144, 716, 36, 36
        );
        ColorRectangle(canvas, new CMYKColor(0x00, 0xFF, 0x00, 0x00), 
          198, 716, 36, 36
        );
        ColorRectangle(canvas, new CMYKColor(0f, 1f, 0f, 0.5f), 
          252, 716, 36, 36
        );
        ColorRectangle(canvas, new CMYKColor(1f, 0f, 0f, 0f), 
          306, 716, 36, 36
        );
        ColorRectangle(canvas, new CMYKColor(1f, 0f, 0f, 0.5f), 
          360, 716, 36, 36
        );
        ColorRectangle(canvas, new CMYKColor(1f, 0f, 0f, 1f), 
          416, 716, 36, 36
        );
        ColorRectangle(canvas, new CMYKColor(0f, 0f, 0f, 1f), 
          470, 716, 36, 36
        );
        // Gray color
        ColorRectangle(canvas, new GrayColor(0x20), 36, 662, 36, 36);
        ColorRectangle(canvas, new GrayColor(0x40), 90, 662, 36, 36);
        ColorRectangle(canvas, new GrayColor(0x60), 144, 662, 36, 36);
        ColorRectangle(canvas, new GrayColor(0.5f), 198, 662, 36, 36);
        ColorRectangle(canvas, new GrayColor(0.625f), 252, 662, 36, 36);
        ColorRectangle(canvas, new GrayColor(0.75f), 306, 662, 36, 36);
        ColorRectangle(canvas, new GrayColor(0.825f), 360, 662, 36, 36);
        ColorRectangle(canvas, GrayColor.GRAYBLACK, 416, 662, 36, 36);
        ColorRectangle(canvas, GrayColor.GRAYWHITE, 470, 662, 36, 36);
        // Alternative ways to color the rectangle
        canvas.SetRGBColorFill(0x00, 0x80, 0x80);
        canvas.Rectangle(36, 608, 36, 36);
        canvas.FillStroke();
        canvas.SetRGBColorFillF(0.5f, 0.25f, 0.60f);
        canvas.Rectangle(90, 608, 36, 36);
        canvas.FillStroke();
        canvas.SetGrayFill(0.5f);
        canvas.Rectangle(144, 608, 36, 36);
        canvas.FillStroke();
        canvas.SetCMYKColorFill(0xFF, 0xFF, 0x00, 0x80);
        canvas.Rectangle(198, 608, 36, 36);
        canvas.FillStroke();
        canvas.SetCMYKColorFillF(0f, 1f, 1f, 0.5f);
        canvas.Rectangle(252, 608, 36, 36);
        canvas.FillStroke();
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Draws a colored rectangle.
     * @param canvas the canvas to draw on
     * @param color the Color
     * @param x the X coordinate
     * @param y the Y coordinate
     * @param width the width of the rectangle
     * @param height the height of the rectangle
     */
    public void ColorRectangle(PdfContentByte canvas, BaseColor color, 
        float x, float y, float width, float height) 
    {
      canvas.SaveState();
      canvas.SetColorFill(color);
      canvas.Rectangle(x, y, width, height);
      canvas.FillStroke();
      canvas.RestoreState();
    }
// ===========================================================================
  }
}