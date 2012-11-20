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
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Chapter08;

namespace kuujinbo.iTextInAction2Ed.Chapter13 {
  public class InspectForm : IWriter {
// ===========================================================================
    /** A text file containing information about a form. */
    public const String RESULTTXT = "fieldflags.txt";
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        Subscribe s = new Subscribe();
        byte[] pdf = s.CreatePdf();
        zip.AddEntry(Utility.ResultFileName(s.ToString() + ".pdf"), pdf); 
        InspectForm i = new InspectForm();
        zip.AddEntry(RESULTTXT, i.InspectPdf(pdf));             
        zip.Save(stream);             
      }
    }    
// --------------------------------------------------------------------------- 
    /**
     * Inspects a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public string InspectPdf(byte[] src) {
  	  PdfReader reader = new PdfReader(src);
  	  AcroFields form = reader.AcroFields;
  	  IDictionary<String,AcroFields.Item> fields = form.Fields;
  	  AcroFields.Item item;
  	  PdfDictionary dict;
  	  int flags;
  	  StringBuilder sb = new StringBuilder();
  	  foreach (string key in fields.Keys) {
  		  sb.Append(key);
  		  item = fields[key];
  		  dict = item.GetMerged(0);
  		  flags = dict.GetAsNumber(PdfName.FF).IntValue;
  		  if ((flags & BaseField.PASSWORD) > 0)
  			  sb.Append(" -> password");
  		  if ((flags & BaseField.MULTILINE) > 0)
  			  sb.Append(" -> multiline");
  		  sb.Append(Environment.NewLine);
  	  }        
      return sb.ToString();
    }
// ===========================================================================
  }
}