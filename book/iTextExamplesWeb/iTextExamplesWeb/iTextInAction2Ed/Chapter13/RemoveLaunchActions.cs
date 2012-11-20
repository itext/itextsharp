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
using kuujinbo.iTextInAction2Ed.Chapter07;

namespace kuujinbo.iTextInAction2Ed.Chapter13 {
  public class RemoveLaunchActions : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String RESULT = "launch_removed.pdf";
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        LaunchAction l = new LaunchAction();
        byte[] pdf = Utility.PdfBytes(l);
        zip.AddEntry(Utility.ResultFileName(l.ToString() + ".pdf"), pdf); 
        RemoveLaunchActions r = new RemoveLaunchActions();      
        zip.AddEntry(RESULT, r.ManipulatePdf(pdf));       
        zip.Save(stream);             
      }
    }    
// --------------------------------------------------------------------------- 
    /**
     * Manipulates a PDF file src
     * @param src the original PDF
     */
    public byte[] ManipulatePdf(byte[] src) {
      PdfReader reader = new PdfReader(src);
      PdfObject obj;
      PdfDictionary action;
      for (int i = 1; i < reader.XrefSize; i++) {
      	obj = reader.GetPdfObject(i);
      	if (obj is PdfDictionary) {
      		action = ((PdfDictionary)obj).GetAsDict(PdfName.A);
      		if (action == null) continue;
      		if (PdfName.LAUNCH.Equals(action.GetAsName(PdfName.S))) {
      			action.Remove(PdfName.F);
      			action.Remove(PdfName.WIN);
      			action.Put(PdfName.S, PdfName.JAVASCRIPT);
      			action.Put(PdfName.JS, new PdfString(
      			  "app.alert('Launch Application Action removed by iText');\r"
      			));
      		}
      	}
      }        
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}