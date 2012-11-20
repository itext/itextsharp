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
  public class TextStateOperators : IWriter {
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
        String text = "AWAY again";
        BaseFont bf = BaseFont.CreateFont();
        canvas.BeginText();
        // line 1
        canvas.SetFontAndSize(bf, 16);
        canvas.MoveText(36, 806);
        canvas.MoveTextWithLeading(0, -24);
        canvas.ShowText(text);
        // line 2
        canvas.SetWordSpacing(20);
        canvas.NewlineShowText(text);
        // line 3
        canvas.SetCharacterSpacing(10);
        canvas.NewlineShowText(text);
        canvas.SetWordSpacing(0);
        canvas.SetCharacterSpacing(0);
        // line 4
        canvas.SetHorizontalScaling(50);
        canvas.NewlineShowText(text);
        canvas.SetHorizontalScaling(100);
        // line 5
        canvas.NewlineShowText(text);
        canvas.SetTextRise(15);
        canvas.SetFontAndSize(bf, 12);
        canvas.SetColorFill(BaseColor.RED);
        canvas.ShowText("2");
        canvas.SetColorFill(GrayColor.GRAYBLACK);
        // line 6
        canvas.SetLeading(56);
        canvas.NewlineShowText("Changing the leading: " + text);
        canvas.SetLeading(24);
        canvas.NewlineText();
        // line 7
        PdfTextArray array = new PdfTextArray("A");
        array.Add(120);
        array.Add("W");
        array.Add(120);
        array.Add("A");
        array.Add(95);
        array.Add("Y again");
        canvas.ShowText(array);
        canvas.EndText();

        canvas.SetColorFill(BaseColor.BLUE);
        canvas.BeginText();
        canvas.SetTextMatrix(360, 770);
        canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL);
        canvas.SetFontAndSize(bf, 24);
        canvas.ShowText(text);
        canvas.EndText();

        canvas.BeginText();
        canvas.SetTextMatrix(360, 730);
        canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_STROKE);
        canvas.SetFontAndSize(bf, 24);
        canvas.ShowText(text);
        canvas.EndText();

        canvas.BeginText();
        canvas.SetTextMatrix(360, 690);
        canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE);
        canvas.SetFontAndSize(bf, 24);
        canvas.ShowText(text);
        canvas.EndText();

        canvas.BeginText();
        canvas.SetTextMatrix(360, 650);
        canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_INVISIBLE);
        canvas.SetFontAndSize(bf, 24);
        canvas.ShowText(text);
        canvas.EndText();

        PdfTemplate template = canvas.CreateTemplate(200, 36);
        template.SetLineWidth(2);
        for (int i = 0; i < 6; i++) {
            template.MoveTo(0, i * 6);
            template.LineTo(200, i * 6);
        }
        template.Stroke();
        
        canvas.SaveState();
        canvas.BeginText();
        canvas.SetTextMatrix(360, 610);
        canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL_CLIP);
        canvas.SetFontAndSize(bf, 24);
        canvas.ShowText(text);
        canvas.EndText();
        canvas.AddTemplate(template, 360, 610);
        canvas.RestoreState();

        canvas.SaveState();
        canvas.BeginText();
        canvas.SetTextMatrix(360, 570);
        canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_STROKE_CLIP);
        canvas.SetFontAndSize(bf, 24);
        canvas.ShowText(text);
        canvas.EndText();
        canvas.AddTemplate(template, 360, 570);
        canvas.RestoreState();

        canvas.SaveState();
        canvas.BeginText();
        canvas.SetTextMatrix(360, 530);
        canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE_CLIP);
        canvas.SetFontAndSize(bf, 24);
        canvas.ShowText(text);
        canvas.EndText();
        canvas.AddTemplate(template, 360, 530);
        canvas.RestoreState();
        
        canvas.SaveState();
        canvas.BeginText();
        canvas.SetTextMatrix(360, 490);
        canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_CLIP);
        canvas.SetFontAndSize(bf, 24);
        canvas.ShowText(text);
        canvas.EndText();
        canvas.AddTemplate(template, 360, 490);
        canvas.RestoreState();

      }
    }
// ===========================================================================
  }
}