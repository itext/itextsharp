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
  public class TransparentOverlay : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const String RESULT = "transparent_overlay.pdf";
    /** One of the resources. */
    public static string RESOURCE = Path.Combine(
      Utility.ResourceImage, "bruno_ingeborg.jpg"
    );
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        TransparentOverlay t = new TransparentOverlay();
        zip.AddEntry(RESULT, t.CreatePdf());
        zip.AddFile(RESOURCE, "");
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------
    public byte[] CreatePdf() {
      using (MemoryStream ms = new MemoryStream()) {    
        // step 1
        using (Document document = new Document(new Rectangle(850, 600))) {
          // step 2
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          // step 3
          document.Open();
          // step 4
          PdfContentByte canvas = writer.DirectContent;
          // add the clipped image
          Image img = Image.GetInstance(
            Path.Combine(Utility.ResourceImage, RESOURCE)
          );
          float w = img.ScaledWidth;
          float h = img.ScaledHeight;
          canvas.Ellipse(1, 1, 848, 598);
          canvas.Clip();
          canvas.NewPath();
          canvas.AddImage(img, w, 0, 0, h, 0, -600);

          // Create a transparent PdfTemplate
          PdfTemplate t2 = writer.DirectContent.CreateTemplate(850, 600);
          PdfTransparencyGroup transGroup = new PdfTransparencyGroup();
          transGroup.Put( PdfName.CS, PdfName.DEVICEGRAY);
          transGroup.Isolated = true;
          transGroup.Knockout = false;
          t2.Group = transGroup;

          // Add transparent ellipses to the template
          int gradationStep = 30;
          float[] gradationRatioList = new float[gradationStep];
          for(int i = 0; i < gradationStep; i++) {
/*
* gotta love .NET, guess they forgot to copy java.lang.Math.toRadians
*/
            double radians = (Math.PI / 180) * 90.0f / gradationStep * (i + 1);
            gradationRatioList[i] = 1 - (float) Math.Sin(radians);
          }
          for(int i = 1; i < gradationStep + 1; i++) {
              t2.SetLineWidth(5 * (gradationStep + 1 - i));
              t2.SetGrayStroke(gradationRatioList[gradationStep - i]);
              t2.Ellipse(0, 0, 850, 600);
              t2.Stroke();
          }
          
          // Create an image mask for the direct content
          PdfDictionary maskDict = new PdfDictionary();
          maskDict.Put(PdfName.TYPE, PdfName.MASK);
          maskDict.Put(PdfName.S, new PdfName("Luminosity"));
          maskDict.Put(new PdfName("G"), t2.IndirectReference);
          PdfGState gState = new PdfGState();
          gState.Put(PdfName.SMASK, maskDict );
          canvas.SetGState(gState);
          
          canvas.AddTemplate(t2, 0, 0);        
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}