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
using kuujinbo.iTextInAction2Ed.Chapter04;
using kuujinbo.iTextInAction2Ed.Chapter07;

namespace kuujinbo.iTextInAction2Ed.Chapter08 {
  public class ReplaceIcon : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String RESULT = "advertisement2.pdf";
    public static string RESOURCE = Path.Combine(Utility.ResourceImage, "iia2.jpg");
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        NestedTables n = new NestedTables(); 
        byte[] ntPdf = Utility.PdfBytes(n);      
        Advertisement a = new Advertisement();
        byte[] aPdf = a.ManipulatePdf(ntPdf);

        PdfReader reader = new PdfReader(aPdf);
        using (MemoryStream ms = new MemoryStream()) {
          using (PdfStamper stamper = new PdfStamper(reader, ms)) {
            AcroFields form = stamper.AcroFields;
            PushbuttonField ad = form.GetNewPushbuttonFromField("advertisement");
            ad.Layout = PushbuttonField.LAYOUT_ICON_ONLY;
            ad.ProportionalIcon = true;
            ad.Image = Image.GetInstance(RESOURCE);
            form.ReplacePushbuttonField("advertisement", ad.Field);
          }
          zip.AddEntry(RESULT, ms.ToArray());
        }
        zip.AddFile(RESOURCE, "");
        zip.AddEntry(Utility.ResultFileName(a.ToString() + ".pdf"), aPdf);                
        zip.Save(stream);             
      }
    }         
// ===========================================================================
  }
}