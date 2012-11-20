/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter14 {
  public class PathConstructionAndPainting : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfContentByte canvas = writer.DirectContent;
        // draw squares
        CreateSquares(canvas, 50, 720, 80, 20);
        ColumnText.ShowTextAligned(
          canvas, Element.ALIGN_LEFT,
          new Phrase(
            "Methods MoveTo(), LineTo(), stroke(), closePathStroke(), Fill(), and closePathFill()"
          ),
          50, 700, 0
        );
        // draw Bezier curves
        createBezierCurves(canvas, 70, 600, 80, 670, 140, 690, 160, 630, 160);
        ColumnText.ShowTextAligned(
          canvas, Element.ALIGN_LEFT,
          new Phrase("Different CurveTo() methods, followed by stroke()"),
          50, 580, 0
        );
        // draw stars and circles
        CreateStarsAndCircles(canvas, 50, 470, 40, 20);
        ColumnText.ShowTextAligned(
          canvas, Element.ALIGN_LEFT,
          new Phrase(
            "Methods Fill(), eoFill(), NewPath(), FillStroke(), and EoFillStroke()"
          ),
          50, 450, 0
        );
        // draw different shapes using convenience methods
        canvas.SaveState();
        canvas.SetColorStroke(new GrayColor(0.2f));
        canvas.SetColorFill(new GrayColor(0.9f));
        canvas.Arc(50, 270, 150, 330, 45, 270);
        canvas.Ellipse(170, 270, 270, 330);
        canvas.Circle(320, 300, 30);
        canvas.RoundRectangle(370, 270, 80, 60, 20);
        canvas.FillStroke();
        canvas.RestoreState();
        Rectangle rect = new Rectangle(470, 270, 550, 330);
        rect.BorderWidthBottom = 10;
        rect.BorderColorBottom = new GrayColor(0f);
        rect.BorderWidthLeft = 4;
        rect.BorderColorLeft = new GrayColor(0.9f);
        rect.BackgroundColor = new GrayColor(0.4f);
        canvas.Rectangle(rect);
        ColumnText.ShowTextAligned(
          canvas, Element.ALIGN_LEFT,
          new Phrase("Convenience methods"), 50, 250, 0
        );
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Draws a row of squares.
     * @param canvas the canvas to which the squares have to be drawn
     * @param x      X coordinate to position the row
     * @param y      Y coordinate to position the row
     * @param side   the side of the square
     * @param gutter the space between the squares
     */
    public void CreateSquares(PdfContentByte canvas,
      float x, float y, float side, float gutter) {
      canvas.SaveState();
      canvas.SetColorStroke(new GrayColor(0.2f));
      canvas.SetColorFill(new GrayColor(0.9f));
      canvas.MoveTo(x, y);
      canvas.LineTo(x + side, y);
      canvas.LineTo(x + side, y + side);
      canvas.LineTo(x, y + side);
      canvas.Stroke();
      x = x + side + gutter;
      canvas.MoveTo(x, y);
      canvas.LineTo(x + side, y);
      canvas.LineTo(x + side, y + side);
      canvas.LineTo(x, y + side);
      canvas.ClosePathStroke();
      x = x + side + gutter;
      canvas.MoveTo(x, y);
      canvas.LineTo(x + side, y);
      canvas.LineTo(x + side, y + side);
      canvas.LineTo(x, y + side);
      canvas.Fill();
      x = x + side + gutter;
      canvas.MoveTo(x, y);
      canvas.LineTo(x + side, y);
      canvas.LineTo(x + side, y + side);
      canvas.LineTo(x, y + side);
      canvas.FillStroke();
      x = x + side + gutter;
      canvas.MoveTo(x, y);
      canvas.LineTo(x + side, y);
      canvas.LineTo(x + side, y + side);
      canvas.LineTo(x, y + side);
      canvas.ClosePathFillStroke();
      canvas.RestoreState();
    }
// ---------------------------------------------------------------------------    
    /**
     * Draws a series of Bezier curves
     * @param cb the canvas to which the curves have to be drawn
     * @param x0 X coordinate of the start point
     * @param y0 Y coordinate of the start point
     * @param x1 X coordinate of the first control point
     * @param y1 Y coordinate of the first control point
     * @param x2 X coordinate of the second control point
     * @param y2 Y coordinate of the second control point
     * @param x3 X coordinate of the end point
     * @param y3 Y coordinate of the end point
     * @param distance the distance between the curves
     */
    public void createBezierCurves(PdfContentByte cb, float x0, float y0,
        float x1, float y1, float x2, float y2, float x3, 
        float y3, float distance) 
    {
      cb.MoveTo(x0, y0);
      cb.LineTo(x1, y1);
      cb.MoveTo(x2, y2);
      cb.LineTo(x3, y3);
      cb.MoveTo(x0, y0);
      cb.CurveTo(x1, y1, x2, y2, x3, y3);
      x0 += distance;
      x1 += distance;
      x2 += distance;
      x3 += distance;
      cb.MoveTo(x2, y2);
      cb.LineTo(x3, y3);
      cb.MoveTo(x0, y0);
      cb.CurveTo(x2, y2, x3, y3);
      x0 += distance;
      x1 += distance;
      x2 += distance;
      x3 += distance;
      cb.MoveTo(x0, y0);
      cb.LineTo(x1, y1);
      cb.MoveTo(x0, y0);
      cb.CurveTo(x1, y1, x3, y3);
      cb.Stroke();
    }
// ---------------------------------------------------------------------------    
    /**
     * Draws a row of stars and circles.
     * @param canvas the canvas to which the shapes have to be drawn
     * @param x      X coordinate to position the row
     * @param y      Y coordinate to position the row
     * @param radius the radius of the circles
     * @param gutter the space between the shapes
     */
    public static void CreateStarsAndCircles(PdfContentByte canvas,
        float x, float y, float radius, float gutter) 
    {
      canvas.SaveState();
      canvas.SetColorStroke(new GrayColor(0.2f));
      canvas.SetColorFill(new GrayColor(0.9f));
      CreateStar(canvas, x, y);
      CreateCircle(canvas, x + radius, y - 70, radius, true);
      CreateCircle(canvas, x + radius, y - 70, radius / 2, true);
      canvas.Fill();
      x += 2 * radius + gutter;
      CreateStar(canvas, x, y);
      CreateCircle(canvas, x + radius, y - 70, radius, true);
      CreateCircle(canvas, x + radius, y - 70, radius / 2, true);
      canvas.EoFill();
      x += 2 * radius + gutter;
      CreateStar(canvas, x, y);
      canvas.NewPath();
      CreateCircle(canvas, x + radius, y - 70, radius, true);
      CreateCircle(canvas, x + radius, y - 70, radius / 2, true);
      x += 2 * radius + gutter;
      CreateStar(canvas, x, y);
      CreateCircle(canvas, x + radius, y - 70, radius, true);
      CreateCircle(canvas, x + radius, y - 70, radius / 2, false);
      canvas.FillStroke();
      x += 2 * radius + gutter;
      CreateStar(canvas, x, y);
      CreateCircle(canvas, x + radius, y - 70, radius, true);
      CreateCircle(canvas, x + radius, y - 70, radius / 2, true);
      canvas.EoFillStroke();
      canvas.RestoreState();
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a path for a five pointed star.
     * This method doesn't fill or stroke the star!
     * @param canvas the canvas for which the star is constructed
     * @param x      the X coordinate of the center of the star
     * @param y      the Y coordinate of the center of the star
     */
    public static void CreateStar(PdfContentByte canvas, float x, float y) {
      canvas.MoveTo(x + 10, y);
      canvas.LineTo(x + 80, y + 60);
      canvas.LineTo(x, y + 60);
      canvas.LineTo(x + 70, y);
      canvas.LineTo(x + 40, y + 90);
      canvas.ClosePath();
    }
// --------------------------------------------------------------------------- 
    /**
     * Creates a path for circle using Bezier curvers.
     * The path can be constructed clockwise or counter-clockwise.
     * This method doesn't Fill or stroke the circle!
     * @param canvas    the canvas for which the path is constructed 
     * @param x         the X coordinate of the center of the circle
     * @param y         the Y coordinate of the center of the circle
     * @param r         the radius
     * @param clockwise true if the circle has to be constructed clockwise
     */
    public static void CreateCircle(PdfContentByte canvas, float x, float y,
      float r, bool clockwise) 
    {
      float b = 0.5523f;
      if (clockwise) {
        canvas.MoveTo(x + r, y);
        canvas.CurveTo(x + r, y - r * b, x + r * b, y - r, x, y - r);
        canvas.CurveTo(x - r * b, y - r, x - r, y - r * b, x - r, y);
        canvas.CurveTo(x - r, y + r * b, x - r * b, y + r, x, y + r);
        canvas.CurveTo(x + r * b, y + r, x + r, y + r * b, x + r, y);
      } else {
        canvas.MoveTo(x + r, y);
        canvas.CurveTo(x + r, y + r * b, x + r * b, y + r, x, y + r);
        canvas.CurveTo(x - r * b, y + r, x - r, y + r * b, x - r, y);
        canvas.CurveTo(x - r, y - r * b, x - r * b, y - r, x, y - r);
        canvas.CurveTo(x + r * b, y - r, x + r, y - r * b, x + r, y);
      }
    }    
// ===========================================================================
  }
}