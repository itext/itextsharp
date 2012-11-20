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

namespace kuujinbo.iTextInAction2Ed.Chapter13 {
  public class FixBrokenForm : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public readonly string ORIGINAL = Path.Combine(
      Utility.ResourcePdf, "broken_form.pdf"
    );
    /** The resulting PDF file. */
    public const string FIXED = "fixed_form.pdf";
    /* NAME DIFFERENT FROM THE EXAMPLE FOR IMPROVED CLARITY */
    public const string RESULT1 = "broken_form_not_filled.pdf";
    public const string RESULT2 = "filled_form.pdf";
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        zip.AddFile(ORIGINAL, "");
        FixBrokenForm example = new FixBrokenForm();
        byte[] pdf = example.ManipulatePdf(ORIGINAL);
        zip.AddEntry(FIXED, pdf);
        zip.AddEntry(RESULT1, example.FillData(new PdfReader(ORIGINAL)));
        zip.AddEntry(RESULT2, example.FillData(new PdfReader(pdf)));
        zip.Save(stream);             
      }
    }    
// ---------------------------------------------------------------------------
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] ManipulatePdf(string src) {
      PdfReader reader = new PdfReader(src);
      PdfDictionary root = reader.Catalog;
      PdfDictionary form = root.GetAsDict(PdfName.ACROFORM);
      PdfArray fields = form.GetAsArray(PdfName.FIELDS);
      PdfDictionary page;
      PdfArray annots;
      for (int i = 1; i <= reader.NumberOfPages; i++) {
        page = reader.GetPageN(i);
        annots = page.GetAsArray(PdfName.ANNOTS);
        for (int j = 0; j < annots.Size; j++) {
          fields.Add(annots.GetAsIndirectObject(j));
        }
      }
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------
    public byte[] FillData(PdfReader reader) {
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          AcroFields form = stamper.AcroFields;
          form.SetField("title", "The Misfortunates");
          form.SetField("director", "Felix Van Groeningen");
          form.SetField("year", "2009");
          form.SetField("duration", "108");
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}