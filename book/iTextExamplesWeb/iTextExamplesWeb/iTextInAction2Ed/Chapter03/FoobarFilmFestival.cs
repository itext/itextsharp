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
  public class FoobarFilmFestival : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      string RESOURCE = Utility.ResourcePosters;
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        Chunk c;
        String foobar = "Foobar Film Festival";
        // Measuring a String in Helvetica
        Font helvetica = new Font(Font.FontFamily.HELVETICA, 12);
        BaseFont bf_helv = helvetica.GetCalculatedBaseFont(false);
        float width_helv = bf_helv.GetWidthPoint(foobar, 12);
        c = new Chunk(foobar + ": " + width_helv, helvetica);
        document.Add(new Paragraph(c));
        document.Add(new Paragraph(string.Format(
          "Chunk width: {0}", c.GetWidthPoint()
        )));
        // Measuring a String in Times
        BaseFont bf_times = BaseFont.CreateFont(
          "c:/windows/fonts/times.ttf", 
          BaseFont.WINANSI, BaseFont.EMBEDDED
        );
        Font times = new Font(bf_times, 12);
        float width_times = bf_times.GetWidthPoint(foobar, 12);
        c = new Chunk(foobar + ": " + width_times, times);
        document.Add(new Paragraph(c));
        document.Add(new Paragraph(String.Format(
          "Chunk width: {0}", c.GetWidthPoint()
        )));
        document.Add(Chunk.NEWLINE);
        // Ascent and descent of the String
        document.Add(new Paragraph(
          "Ascent Helvetica: " + bf_helv.GetAscentPoint(foobar, 12)
        ));
        document.Add(new Paragraph(
          "Ascent Times: " + bf_times.GetAscentPoint(foobar, 12)
        ));
        document.Add(new Paragraph(
          "Descent Helvetica: " + bf_helv.GetDescentPoint(foobar, 12)
        ));
        document.Add(new Paragraph(
          "Descent Times: " + bf_times.GetDescentPoint(foobar, 12)
        ));
        document.Add(Chunk.NEWLINE);
        // Kerned text
        width_helv = bf_helv.GetWidthPointKerned(foobar, 12);
        c = new Chunk(foobar + ": " + width_helv, helvetica);
        document.Add(new Paragraph(c));
        // Drawing lines to see where the text is added
        PdfContentByte canvas = writer.DirectContent;
        canvas.SaveState();
        canvas.SetLineWidth(0.05f);
        canvas.MoveTo(400, 806);
        canvas.LineTo(400, 626);
        canvas.MoveTo(508.7f, 806);
        canvas.LineTo(508.7f, 626);
        canvas.MoveTo(280, 788);
        canvas.LineTo(520, 788);
        canvas.MoveTo(280, 752);
        canvas.LineTo(520, 752);
        canvas.MoveTo(280, 716);
        canvas.LineTo(520, 716);
        canvas.MoveTo(280, 680);
        canvas.LineTo(520, 680);
        canvas.MoveTo(280, 644);
        canvas.LineTo(520, 644);
        canvas.Stroke();
        canvas.RestoreState();
        // Adding text with PdfContentByte.ShowTextAligned()
        canvas.BeginText();
        canvas.SetFontAndSize(bf_helv, 12);
        canvas.ShowTextAligned(Element.ALIGN_LEFT, foobar, 400, 788, 0);
        canvas.ShowTextAligned(Element.ALIGN_RIGHT, foobar, 400, 752, 0);
        canvas.ShowTextAligned(Element.ALIGN_CENTER, foobar, 400, 716, 0);
        canvas.ShowTextAligned(Element.ALIGN_CENTER, foobar, 400, 680, 30);
        canvas.ShowTextAlignedKerned(Element.ALIGN_LEFT, foobar, 400, 644, 0);
        canvas.EndText();
        // More lines to see where the text is added
        canvas.SaveState();
        canvas.SetLineWidth(0.05f);
        canvas.MoveTo(200, 590);
        canvas.LineTo(200, 410);
        canvas.MoveTo(400, 590);
        canvas.LineTo(400, 410);
        canvas.MoveTo(80, 572);
        canvas.LineTo(520, 572);
        canvas.MoveTo(80, 536);
        canvas.LineTo(520, 536);
        canvas.MoveTo(80, 500);
        canvas.LineTo(520, 500);
        canvas.MoveTo(80, 464);
        canvas.LineTo(520, 464);
        canvas.MoveTo(80, 428);
        canvas.LineTo(520, 428);
        canvas.Stroke();
        canvas.RestoreState();
        // Adding text with ColumnText.ShowTextAligned()
        Phrase phrase = new Phrase(foobar, times);
        ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, phrase, 200, 572, 0);
        ColumnText.ShowTextAligned(canvas, Element.ALIGN_RIGHT, phrase, 200, 536, 0);
        ColumnText.ShowTextAligned(canvas, Element.ALIGN_CENTER, phrase, 200, 500, 0);
        ColumnText.ShowTextAligned(canvas, Element.ALIGN_CENTER, phrase, 200, 464, 30);
        ColumnText.ShowTextAligned(canvas, Element.ALIGN_CENTER, phrase, 200, 428, -30);
        // Chunk attributes
        c = new Chunk(foobar, times);
        c.SetHorizontalScaling(0.5f);
        phrase = new Phrase(c);
        ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, phrase, 400, 572, 0);
        c = new Chunk(foobar, times);
        c.SetSkew(15, 15);
        phrase = new Phrase(c);
        ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, phrase, 400, 536, 0);
        c = new Chunk(foobar, times);
        c.SetSkew(0, 25);
        phrase = new Phrase(c);
        ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, phrase, 400, 500, 0);
        c = new Chunk(foobar, times);
        c.SetTextRenderMode(PdfContentByte.TEXT_RENDER_MODE_STROKE, 0.1f, BaseColor.RED);
        phrase = new Phrase(c);
        ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, phrase, 400, 464, 0);
        c = new Chunk(foobar, times);
        c.SetTextRenderMode(PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE, 1, null);
        phrase = new Phrase(c);
        ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, phrase, 400, 428, -0);        
      }
    }
// ===========================================================================
  }
}