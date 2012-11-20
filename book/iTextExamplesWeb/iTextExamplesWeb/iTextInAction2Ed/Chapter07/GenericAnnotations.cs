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
using iTextSharp.text.pdf.draw;
/*
 * book example inherits directly from PdfPageEventHelper class, we use
 * nested inner class.
*/
namespace kuujinbo.iTextInAction2Ed.Chapter07 {
  public class GenericAnnotations : IWriter {
// ===========================================================================
    /** Possible icons. */
    public static readonly string[] ICONS = {
      "Comment", "Key", "Note", "Help", "NewParagraph", "Paragraph", "Insert"
    };
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        writer.PageEvent = new AnnotationHelper();
        // step 3
        document.Open();
        // step 4
        Paragraph p = new Paragraph();
        Chunk chunk;
        Chunk tab = new Chunk(new VerticalPositionMark());
        for (int i = 0; i < ICONS.Length; i++) {
          chunk = new Chunk(ICONS[i]);
          chunk.SetGenericTag(ICONS[i]);
          p.Add(chunk);
          p.Add(tab);
        }        
        document.Add(p); 
      }
    }
/*
 * ###########################################################################
 * Inner class to write annotations
 * ###########################################################################
*/
    class AnnotationHelper : PdfPageEventHelper {
    /**
     * @see com.itextpdf.text.pdf.PdfPageEventHelper#onGenericTag(
     *      com.itextpdf.text.pdf.PdfWriter,
     *      com.itextpdf.text.Document,
     *      com.itextpdf.text.Rectangle, java.lang.String)
     */
      public override void OnGenericTag(PdfWriter writer,
        Document document, Rectangle rect, string text) 
      {
        PdfAnnotation annotation = new PdfAnnotation(writer,
          new Rectangle(
            rect.Right+ 10, rect.Bottom,
            rect.Right+ 30, rect.Top
          )
        );
        annotation.Title = "Text annotation";
        annotation.Put(PdfName.SUBTYPE, PdfName.TEXT);
        annotation.Put(PdfName.OPEN, PdfBoolean.PDFFALSE);
        annotation.Put(PdfName.CONTENTS,
          new PdfString(string.Format("Icon: {0}", text))
        );
        annotation.Put(PdfName.NAME, new PdfName(text));
        writer.AddAnnotation(annotation);
      }        
    }
// ===========================================================================
  }
}