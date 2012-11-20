/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace kuujinbo.iTextInAction2Ed.Chapter15 {
  public class ParseTaggedPdf : IWriter {
// ===========================================================================
    /** The resulting XML file. */
    public const String RESULT = "moby_extracted.xml";
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        StructuredContent s = new StructuredContent();
        byte[] pdf = s.CreatePdf();
        zip.AddEntry(Utility.ResultFileName(s.ToString() + ".pdf"), pdf); 
        TaggedPdfReaderTool reader = new TaggedPdfReaderTool();
        using (MemoryStream ms = new MemoryStream()) {
          reader.ConvertToXml(new PdfReader(pdf), ms);
          StringBuilder sb =  new StringBuilder();
          foreach (byte b in ms.ToArray()) {
            sb.Append((char) b);
          }
          zip.AddEntry(RESULT, sb.ToString());
        }
        zip.Save(stream);
      }
    }    
// ===========================================================================
	}
}