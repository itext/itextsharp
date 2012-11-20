/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Chapter01;
using kuujinbo.iTextInAction2Ed.Chapter05;

namespace kuujinbo.iTextInAction2Ed.Chapter14 {
  public class GetContentStream : IWriter {
// ===========================================================================
    /** The content stream of a first PDF. */
    public const String RESULT1 = "contentstream1.txt";
    /** The content stream of a second PDF. */
    public const String RESULT2 = "contentstream2.txt";
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {
      HelloWorld hello = new HelloWorld();
      Hero1 hero = new Hero1();
      using (ZipFile zip = new ZipFile()) {
        byte[] pdfHello = Utility.PdfBytes(hello);
        byte[] pdfHero = Utility.PdfBytes(hero);
        zip.AddEntry(Utility.ResultFileName(
          hello.ToString() + ".pdf"), pdfHello
        );       
        zip.AddEntry(Utility.ResultFileName(
          hero.ToString() + ".pdf"), pdfHero
        );
        GetContentStream example = new GetContentStream();
        zip.AddEntry(RESULT1, example.ReadContent(pdfHello));
        zip.AddEntry(RESULT2, example.ReadContent(pdfHero));
        zip.Save(stream);             
      }    
    }
// ---------------------------------------------------------------------------    
    /**
     * Reads the content stream of the first page of a PDF into a text file.
     * @param src the PDF file
     */
    public string ReadContent(byte[] src) {
      PdfReader reader = new PdfReader(src);
      byte[] pc = reader.GetPageContent(1);
      return Encoding.UTF8.GetString(pc, 0, pc.Length);
    }
// ===========================================================================
  }
}