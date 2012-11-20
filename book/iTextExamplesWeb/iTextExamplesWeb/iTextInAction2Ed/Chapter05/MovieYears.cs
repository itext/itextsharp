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
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter05 {
  public class MovieYears : IWriter {
// ===========================================================================
    /**
     * Inner class to add functionality to a Chunk in a generic way.
     */
    class GenericTags : PdfPageEventHelper {
      public override void OnGenericTag(
        PdfWriter writer, Document pdfDocument, Rectangle rect, String text
    ) {
        if ("strip".Equals(text)) {
          Strip(writer.DirectContent, rect);
        }
        else if ("ellipse".Equals(text)) {
          Ellipse(writer.DirectContentUnder, rect);
        }
        else {
          CountYear(text);
        }
      }

      public void Strip(PdfContentByte content, Rectangle rect) {
        content.Rectangle(
          rect.Left - 1, rect.Bottom - 5f,
          rect.Width, rect.Height + 8
        );
        content.Rectangle(
          rect.Left, rect.Bottom - 2,
          rect.Width - 2, rect.Height + 2
        );
        float y1 = rect.Top + 0.5f;
        float y2 = rect.Bottom - 4;
        for (float f = rect.Left; f < rect.Right - 4; f += 5) {
          content.Rectangle(f, y1, 4f, 1.5f);
          content.Rectangle(f, y2, 4f, 1.5f);
        }
        content.EoFill();
      }
      
      public void Ellipse(PdfContentByte content, Rectangle rect) {
        content.SaveState();
        content.SetRGBColorFill(0x00, 0x00, 0xFF);
        content.Ellipse(
          rect.Left - 3f, rect.Bottom - 5f,
          rect.Right + 3f, rect.Top + 3f
        );
        content.Fill();
        content.RestoreState();
      }

      internal SortedDictionary<string, int> years = new SortedDictionary<string, int>();
      
      public void CountYear(String text) {
        if ( years.ContainsKey(text) ) {
          ++years[text];
        }
        else {
          years.Add(text, 1);
        }
      }
    }
/*
 * end inner class
*/    
    /**
     * Inner class to add lines when a paragraph begins and ends.
     */
    class ParagraphPositions : PdfPageEventHelper {
      public override void OnParagraph(
          PdfWriter writer, Document pdfDocument, float paragraphPosition
      ) {
        DrawLine(
          writer.DirectContent, pdfDocument.Left, 
          pdfDocument.Right, paragraphPosition - 8
        );
      }

      public override void OnParagraphEnd(
          PdfWriter writer, Document pdfDocument, float paragraphPosition
      ) {
          DrawLine(
            writer.DirectContent, pdfDocument.Left, 
            pdfDocument.Right, paragraphPosition - 5
          );
      }
      
      public void DrawLine(PdfContentByte cb, float x1, float x2, float y) {
        cb.MoveTo(x1, y);
        cb.LineTo(x2, y);
        cb.Stroke();
      }
    }
/*
 * end inner class
*/
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
      // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        GenericTags gevent = new GenericTags();
        writer.PageEvent = gevent;
        writer.PageEvent = new ParagraphPositions();       
        // step 3
        document.Open();
        // step 4
        Font bold = new Font(Font.FontFamily.HELVETICA, 11, Font.BOLD);
        Font italic = new Font(Font.FontFamily.HELVETICA, 11, Font.ITALIC);
        Font white = new Font(
          Font.FontFamily.HELVETICA, 12, 
          Font.BOLD | Font.ITALIC, 
          BaseColor.WHITE
        );
        Paragraph p;
        Chunk c;        
        foreach (Movie movie in PojoFactory.GetMovies(true)) {
            p = new Paragraph(22);
            c = new Chunk(String.Format("{0} ", movie.Year), bold);
            c.SetGenericTag("strip");
            p.Add(c);
            c = new Chunk(movie.MovieTitle);
            c.SetGenericTag(movie.Year.ToString());
            p.Add(c);
            c = new Chunk(
              String.Format(" ({0} minutes)  ", movie.Duration), 
              italic
            );
            p.Add(c);
            c = new Chunk("IMDB", white);
            c.SetAnchor("http://www.imdb.com/title/tt" + movie.Imdb);
            c.SetGenericTag("ellipse");
            p.Add(c);
            document.Add(p);
        }
        document.NewPage();
        writer.PageEvent = null;
        foreach (string entry in gevent.years.Keys) {
          p = new Paragraph(String.Format(
            "{0}: {1} movie(s)", entry, gevent.years[entry]
          ));
          document.Add(p);
        }
      }
    }
// ===========================================================================
  }
}