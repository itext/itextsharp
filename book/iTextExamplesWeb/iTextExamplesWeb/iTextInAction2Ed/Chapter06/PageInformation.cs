/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Ionic.Zip;
using kuujinbo.iTextInAction2Ed.Chapter01;
using kuujinbo.iTextInAction2Ed.Chapter03;
using kuujinbo.iTextInAction2Ed.Chapter05;

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class PageInformation : IWriter {
// ===========================================================================
    public const string RESULT = "page_info.txt";
    public void Write(Stream stream) {
      IWriter[] iw = {
        new HelloWorldLandscape1(), new HelloWorldLandscape2(),
        new MovieTemplates(), new Hero1()
      }; 
      StringBuilder sb = new StringBuilder();       
      using (ZipFile zip = new ZipFile()) {
        foreach (IWriter w in iw) {
          // Create a reader
          byte[] pdf = Utility.PdfBytes(w);
          string fileName = Utility.ResultFileName(w.ToString() + ".pdf");
          Inspect(sb, pdf, fileName); 
          zip.AddEntry(fileName, pdf);  
        }
        zip.AddEntry(RESULT, sb.ToString());
        zip.Save(stream);
      }          
    }   
// ---------------------------------------------------------------------------
    /**
     * Inspect a PDF file and write the info to a txt file
     * @param writer StringBuilder
     * @param pdf PDF file bytes
     * @param fileName PDF filename
     */
    public static void Inspect(StringBuilder sb, byte[] pdf, string fileName) {
      PdfReader reader = new PdfReader(pdf);
      sb.Append(fileName);
      sb.Append(Environment.NewLine);
      sb.Append("Number of pages: ");
      sb.Append(reader.NumberOfPages);
      sb.Append(Environment.NewLine);
      Rectangle mediabox = reader.GetPageSize(1);
      sb.Append("Size of page 1: [");
      sb.Append(mediabox.Left);
      sb.Append(',');
      sb.Append(mediabox.Bottom);
      sb.Append(',');
      sb.Append(mediabox.Right);
      sb.Append(',');
      sb.Append(mediabox.Top);
      sb.Append("]");
      sb.Append(Environment.NewLine);
      sb.Append("Rotation of page 1: ");
      sb.Append(reader.GetPageRotation(1));
      sb.Append(Environment.NewLine);
      sb.Append("Page size with rotation of page 1: ");
      sb.Append(reader.GetPageSizeWithRotation(1));
      sb.Append(Environment.NewLine);
      sb.Append("Is rebuilt? ");
      sb.Append(reader.IsRebuilt().ToString());
      sb.Append(Environment.NewLine);
      sb.Append("Is encrypted? ");
      sb.Append(reader.IsEncrypted().ToString());
      sb.Append(Environment.NewLine);
      sb.Append(Environment.NewLine);
    }    
// ===========================================================================
  }
}