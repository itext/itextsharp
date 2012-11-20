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
using iTextSharp.text.pdf.draw;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter03 {
  public class MovieColumns4 : MovieColumns1 {
// ===========================================================================
    public override void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        PdfContentByte canvas = writer.DirectContent;
        DrawRectangles(canvas);
        ColumnText ct = new ColumnText(canvas);
        ct.Alignment = Element.ALIGN_JUSTIFIED;
        ct.Leading = 14;
        int column = 0;
        ct.SetColumns(
          new float[] { 36,806, 36,670, 108,670, 108,596, 36,596, 36,36 }, 
          new float[] { 296,806, 296,484, 259,484, 259,410, 296,410, 296,36 }
        ); 
        ct.SetColumns(LEFT[column], RIGHT[column] );
        // ct.SetColumns(LEFT[column], RIGHT[column]);
        // iText-ONLY, 'Initial value of the status' => 0
        // iTextSharp **DOES NOT** include this member variable
        // int status = ColumnText.START_COLUMN;
        int status = 0;
        Phrase p;
        float y;
        IEnumerable<Movie> movies = PojoFactory.GetMovies();
        foreach (Movie movie in movies) {
          y = ct.YLine;
          p = CreateMovieInformation(movie);
          ct.AddText(p);
          status = ct.Go(true);
          if (ColumnText.HasMoreText(status)) {
            column = Math.Abs(column - 1);
            if (column == 0) {
              document.NewPage();
              DrawRectangles(canvas);
            }
            ct.SetColumns(LEFT[column], RIGHT[column]);
            y = 806;
          }
          ct.YLine = y;
          ct.SetText(p);
          status = ct.Go();
        }        
      }
    }
// ---------------------------------------------------------------------------    
    public readonly float[][] LEFT = { 
      new float[] { 36,806, 36,670, 108,670, 108,596, 36,596, 36,36 }, 
      new float[] { 299,806, 299,484, 336,484, 336,410, 299,410, 299,36 } 
    };
    public readonly float[][] RIGHT = { 
      new float[] { 296,806, 296,484, 259,484, 259,410, 296,410, 296,36 },
      new float[] { 559,806, 559,246, 487,246, 487,172, 559,172, 559,36 } 
    };
// ---------------------------------------------------------------------------    
    /**
     * Draws three rectangles
     * @param canvas
     */
    public void DrawRectangles(PdfContentByte canvas) {
      canvas.SaveState();
      canvas.SetGrayFill(0.9f);
      canvas.Rectangle(33, 592, 72, 72);
      canvas.Rectangle(263, 406, 72, 72);
      canvas.Rectangle(491, 168, 72, 72);
      canvas.FillStroke();
      canvas.RestoreState();
    }    
// ===========================================================================
  }
}