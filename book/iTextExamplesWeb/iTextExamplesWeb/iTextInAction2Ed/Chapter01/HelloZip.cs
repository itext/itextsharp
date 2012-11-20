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
/*
 * word of warning - .NET ZIP data compression/archive 
 * format implementation is **severely** lacking:
 * [1] only supported in .NET 3.0 and above
 * [2] compression non-existent; see 'Remarks' section when 
 *     setting 'CompressionOption' parameter of Package.CreatePart()
 *     http://msdn.microsoft.com/en-us/library/ms568067.aspx
 * 
 * so instead we use the excellent DotNetZip library:
 * 
 * http://dotnetzip.codeplex.com/
*/
namespace kuujinbo.iTextInAction2Ed.Chapter01 {
  public class HelloZip : IWriter {
// ===========================================================================
/*
 * Creates a zip file with three PDF documents:
 * hello1.pdf to hello3.pdf
 */
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        for (int i = 1; i <= 3; i++) {
          using (MemoryStream ms = new MemoryStream()) {
            // step 1
            using (Document document = new Document()) {
              // step 2
              PdfWriter writer = PdfWriter.GetInstance(document, ms);
              // step 3
              document.Open();
              // step 4
              document.Add(new Paragraph(string.Format("Hello {0}", i)));
            }
            string fileName = string.Format("hello_{0}.pdf", i);
            ZipEntry e = zip.AddEntry(fileName, ms.ToArray());
          }
        }
        zip.Save(stream);
      }
    }
// ===========================================================================
  }
}