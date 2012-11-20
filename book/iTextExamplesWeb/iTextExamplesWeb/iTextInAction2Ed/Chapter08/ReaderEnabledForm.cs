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
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter08 {
  public class ReaderEnabledForm : IWriter {
// ===========================================================================
    public readonly string RESOURCE = 
        Path.Combine(Utility.ResourcePdf, "xfa_enabled.pdf");
    public readonly string RESULT1 = "xfa_broken.pdf";
    public readonly string RESULT2 = "xfa_removed.pdf";
    public readonly string RESULT3 = "xfa_preserved.pdf";         
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        ReaderEnabledForm form = new ReaderEnabledForm();
        zip.AddEntry(RESULT1, form.ManipulatePdf(RESOURCE, false, false));
        zip.AddEntry(RESULT2, form.ManipulatePdf(RESOURCE, true, false));
        zip.AddEntry(RESULT3, form.ManipulatePdf(RESOURCE, false, true));
        zip.AddFile(RESOURCE, "");
        zip.Save(stream);             
      }
    }  
// ---------------------------------------------------------------------------    
    public void SetFields(PdfStamper stamper) {
      AcroFields form = stamper.AcroFields;
      form.SetField("movie[0].#subform[0].title[0]", "The Misfortunates");
      form.SetField("movie[0].#subform[0].original[0]", "De helaasheid der dingen");
      form.SetField("movie[0].#subform[0].duration[0]", "108");
      form.SetField("movie[0].#subform[0].year[0]", "2009");    
    }
// ---------------------------------------------------------------------------
    public byte[] ManipulatePdf(String src, bool remove, bool preserve) {
      PdfReader reader = new PdfReader(src);
      if (remove) reader.RemoveUsageRights();
      using (MemoryStream ms = new MemoryStream()) {
        if (preserve) {
          using (PdfStamper stamper = new PdfStamper(reader, ms, '\0', true)) {
            SetFields(stamper);
          }
        } 
        else {
          using (PdfStamper stamper = new PdfStamper(reader, ms)) {
            SetFields(stamper);
          }
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}