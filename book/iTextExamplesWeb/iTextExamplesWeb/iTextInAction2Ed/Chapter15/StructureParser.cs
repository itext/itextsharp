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
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
/* 
 * Java book example uses SAX to parse XML. .NET uses XmlReader class instead of 
 * SAX, so this class is unnecessary.
 * couple of good explanations of the differences between Java and C#:
 * http://www.xml.com/pub/a/2002/03/06/csharpxml.html
 * http://msdn.microsoft.com/en-us/library/aa478996.aspx
*/
namespace kuujinbo.iTextInAction2Ed.Chapter15 {
  public class StructureParser : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      throw new NotImplementedException(
        ".NET uses XmlReader class instead of SAX"
      );
    }
// ===========================================================================
  }
}