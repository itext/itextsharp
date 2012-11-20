/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter16 {
  public class Pdf3D : IWriter {
// ===========================================================================
    public readonly String RESOURCE = Path.Combine(
      Utility.ResourceImage, "teapot.u3d"
    );
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document()) {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        Rectangle rect = new Rectangle(100, 400, 500, 800);
        rect.Border = Rectangle.BOX;
        rect.BorderWidth = 0.5f;
        rect.BorderColor = new BaseColor(0xFF, 0x00, 0x00);
        document.Add(rect);

        PdfIndirectObject streamObject = null;
        using (FileStream fs = 
          new FileStream(RESOURCE, FileMode.Open, FileAccess.Read))
        {
          PdfStream stream3D = new PdfStream(fs, writer);
          
          stream3D.Put(PdfName.TYPE, new PdfName("3D"));
          stream3D.Put(PdfName.SUBTYPE, new PdfName("U3D"));
          stream3D.FlateCompress();
          streamObject = writer.AddToBody(stream3D);
          stream3D.WriteLength();
        }
            
        PdfDictionary dict3D = new PdfDictionary();
        dict3D.Put(PdfName.TYPE, new PdfName("3DView"));
        dict3D.Put(new PdfName("XN"), new PdfString("Default"));
        dict3D.Put(new PdfName("IN"), new PdfString("Unnamed"));
        dict3D.Put(new PdfName("MS"), PdfName.M);
        dict3D.Put(
          new PdfName("C2W"),
          new PdfArray(
            new float[] { 1, 0, 0, 0, 0, -1, 0, 1, 0, 3, -235, 28 }
          )
        );
        dict3D.Put(PdfName.CO, new PdfNumber(235));

        PdfIndirectObject dictObject = writer.AddToBody(dict3D); 
            
        PdfAnnotation annot = new PdfAnnotation(writer, rect);
        annot.Put(PdfName.CONTENTS, new PdfString("3D Model"));
        annot.Put(PdfName.SUBTYPE, new PdfName("3D"));
        annot.Put(PdfName.TYPE, PdfName.ANNOT);
        annot.Put(new PdfName("3DD"), streamObject.IndirectReference);
        annot.Put(new PdfName("3DV"), dictObject.IndirectReference);
        PdfAppearance ap = writer.DirectContent.CreateAppearance(
          rect.Width, rect.Height
        );
        annot.SetAppearance(PdfAnnotation.APPEARANCE_NORMAL, ap);
        annot.SetPage();

        writer.AddAnnotation(annot);      
      }
    }
// ===========================================================================
	}
}