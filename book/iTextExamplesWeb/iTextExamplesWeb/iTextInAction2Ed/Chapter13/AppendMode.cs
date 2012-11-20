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
using kuujinbo.iTextInAction2Ed.Chapter01;

namespace kuujinbo.iTextInAction2Ed.Chapter13 {
  public class AppendMode : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String RESULT = "appended.pdf";
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        HelloWorld h = new HelloWorld();
        byte[] pdf = Utility.PdfBytes(h);
        zip.AddEntry(Utility.ResultFileName(h.ToString() + ".pdf"), pdf);       
        AppendMode a = new AppendMode();      
        zip.AddEntry(RESULT, a.ManipulatePdf(pdf));       
        zip.Save(stream);             
      }
    }
// --------------------------------------------------------------------------- 
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     * @throws DocumentException
     */
    public byte[] ManipulatePdf(byte[] src) {
	    PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
	      using (PdfStamper stamper = new PdfStamper(reader, ms, '\0', true)) {
	        PdfContentByte cb = stamper.GetUnderContent(1);
	        cb.BeginText();
	        cb.SetFontAndSize(BaseFont.CreateFont(), 12);
	        cb.ShowTextAligned(Element.ALIGN_LEFT, "Hello People!", 36, 770, 0);
	        cb.EndText();
  	    }
  	    return ms.ToArray();
	    }
    }
// ===========================================================================
  }
}