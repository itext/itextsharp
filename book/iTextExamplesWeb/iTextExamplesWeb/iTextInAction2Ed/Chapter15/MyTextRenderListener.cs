/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace kuujinbo.iTextInAction2Ed.Chapter15 {
  public class MyTextRenderListener : IRenderListener {
// ===========================================================================
    public StringBuilder Text { get; set; }
// ---------------------------------------------------------------------------
    /**
     * Creates a RenderListener that will look for text.
     */
    public MyTextRenderListener() {
      Text = new StringBuilder();
    }
    
    /**
     * @see com.itextpdf.text.pdf.parser.RenderListener#beginTextBlock()
     */
    public void BeginTextBlock() {
      Text.Append("<");
    }

    /**
     * @see com.itextpdf.text.pdf.parser.RenderListener#endTextBlock()
     */
    public void EndTextBlock() {
      Text.AppendLine(">");
    }

    /**
     * @see com.itextpdf.text.pdf.parser.RenderListener#renderImage(
     *     com.itextpdf.text.pdf.parser.ImageRenderInfo)
     */
    public void RenderImage(ImageRenderInfo renderInfo) {
    }

    /**
     * @see com.itextpdf.text.pdf.parser.RenderListener#renderText(
     *     com.itextpdf.text.pdf.parser.TextRenderInfo)
     */
    public void RenderText(TextRenderInfo renderInfo) {
      Text.Append("<");
      Text.Append(renderInfo.GetText());
      Text.Append(">");
    }    
// ===========================================================================
	}
}