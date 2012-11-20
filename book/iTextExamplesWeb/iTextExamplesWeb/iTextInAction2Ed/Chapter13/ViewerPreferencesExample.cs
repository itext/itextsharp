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
  public class ViewerPreferencesExample : PageLayoutExample {
// ===========================================================================
    /** The resulting PDF file. */
    public new const string RESULT1 = "viewerpreferences1.pdf";
    /** The resulting PDF file. */
    public new const string RESULT2 = "viewerpreferences2.pdf";
    /** The resulting PDF file. */
    public new const string RESULT3 = "viewerpreferences3.pdf";
    /** The resulting PDF file. */
    public new const string RESULT4 = "viewerpreferences4.pdf";
    /** The resulting PDF file. */
    public new const string RESULT5 = "viewerpreferences5.pdf";
    /** The resulting PDF file. */
    public new const string RESULT6 = "viewerpreferences6.pdf";
// --------------------------------------------------------------------------- 
    public override void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        ViewerPreferencesExample example = new ViewerPreferencesExample();
        zip.AddEntry(RESULT1, example.CreatePdf(PdfWriter.PageModeFullScreen));
        zip.AddEntry(RESULT2, example.CreatePdf(PdfWriter.PageModeUseThumbs));
        zip.AddEntry(RESULT3, example.CreatePdf(
          PdfWriter.PageLayoutTwoColumnRight | PdfWriter.PageModeUseThumbs
        ));
        zip.AddEntry(RESULT4, example.CreatePdf(
          PdfWriter.PageModeFullScreen | PdfWriter.NonFullScreenPageModeUseOutlines
        ));
        zip.AddEntry(RESULT5, example.CreatePdf(
          PdfWriter.FitWindow | PdfWriter.HideToolbar
        ));
        zip.AddEntry(RESULT6, example.CreatePdf(PdfWriter.HideWindowUI));      
        zip.Save(stream);             
      }
    } 
// ===========================================================================
  }
}