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
using System.Text.RegularExpressions;
using Ionic.Zip;
using System.Xml;
using iTextSharp.text;
using iTextSharp.text.pdf;
/* 
 * Java book example uses SAX to parse XML. .NET uses XmlReader class instead of 
 * SAX; i.e. ContentParser & StructureParser classes from book not needed and 
 * are implemented in __THIS__ class
 * 
 * couple of good explanations of the differences between Java and C#:
 * http://www.xml.com/pub/a/2002/03/06/csharpxml.html
 * http://msdn.microsoft.com/en-us/library/aa478996.aspx
 * 
*/
namespace kuujinbo.iTextInAction2Ed.Chapter15 {
  public class StructuredContent : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String RESULT = "moby.pdf";
    /** An XML file that will be converted to PDF. */
    public readonly String RESOURCE = Path.Combine(
      Utility.ResourceXml, "moby.xml"
    );
    /** The StringBuffer that holds the characters. */
    protected StringBuilder buf = new StringBuilder();

    /** The document to which content parsed form XML will be added. */
    protected Document document;
    /** The writer to which PDF syntax will be written. */
    protected PdfWriter writer;
    /** The canvas to which content will be written. */
    protected PdfContentByte canvas;

    /** The current structure element during the parsing process. */
    protected PdfStructureElement current;
    /** The column to which content will be added. */
    protected ColumnText column;
    /** The font used when content elements are created. */
    protected Font font;    
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        zip.AddFile(RESOURCE, ""); 
        StructuredContent s = new StructuredContent();      
        zip.AddEntry(RESULT, s.CreatePdf());       
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------    
    public byte[] CreatePdf() {
      using (MemoryStream ms = new MemoryStream()) {
        using (document = new Document(PageSize.A5)) {
          // step 2
          writer = PdfWriter.GetInstance(document, ms);
          writer.SetTagged();
          // step 3
          document.Open();
          // step 4
          PdfStructureTreeRoot root = writer.StructureTreeRoot;
          root.MapRole(new PdfName("chapter"), PdfName.SECT);
          root.MapRole(new PdfName("title"), PdfName.H);
          root.MapRole(new PdfName("para"), PdfName.P);
          top = new PdfStructureElement(
            root, new PdfName("chapter")
          );
          
          canvas = writer.DirectContent;
          column = new ColumnText(canvas);
          column.SetSimpleColumn(36, 36, 384, 569);
          font = new Font( 
            BaseFont.CreateFont(
              "c:/windows/fonts/arial.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED
            ), 12
          );          
          
          using (XmlReader xr = XmlReader.Create(RESOURCE)) {
            xr.MoveToContent();
            while (xr.Read()) {
              switch (xr.NodeType) {
                case XmlNodeType.Element:
                  StartElement(xr.Name);
                  break;
                case XmlNodeType.Text:
                  buf.Append(Regex.Replace(xr.Value.Trim(), "\n", " "));
                  break;
                case XmlNodeType.EndElement:
                  EndElement(xr.Name);
                  break;
              }
            }
          }
        }
        return ms.ToArray();
      }
    }
    
    PdfStructureElement top;
// ---------------------------------------------------------------------------        
// replace [StructureParser|ContentParser].startElement() from Java example
    public void StartElement(string name) {
      switch (name) {
        case "chapter":
          break;
        default:
          current = new PdfStructureElement(top, new PdfName(name));
          canvas.BeginMarkedContentSequence(current);
          break;
      }
    }    
// ---------------------------------------------------------------------------        
// replace ContentParser.endElement() from Java example
    public void EndElement(string name) {   
      if ("chapter".Equals(name)) return;
      
      String s = buf.ToString().Trim();
      buf = new StringBuilder();
      if (s.Length > 0) {
        Paragraph p = new Paragraph(s, font);
        p.Alignment = Element.ALIGN_JUSTIFIED;
        column.AddElement(p);
        int status = column.Go();
        while (ColumnText.HasMoreText(status)) {
          canvas.EndMarkedContentSequence();
          document.NewPage();
          canvas.BeginMarkedContentSequence(current);
          column.SetSimpleColumn(36, 36, 384, 569);
          status = column.Go();
        }
      }
      canvas.EndMarkedContentSequence();    
    }
// ===========================================================================
  }
}