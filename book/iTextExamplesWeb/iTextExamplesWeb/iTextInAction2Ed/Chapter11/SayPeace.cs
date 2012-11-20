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
using System.Xml;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter11 {
  public class SayPeace : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const String RESULT = "say_peace.pdf";
    /** The XML file with the text. */
    public static string RESOURCE = Path.Combine(
      Utility.ResourceXml, "say_peace.xml"
    );
    /** The font that is used for the peace message. */
    public Font f;
    /** The document to which we are going to add our message. */
    protected Document document;

    /** The StringBuilder that holds the characters. */
    protected StringBuilder buf = new StringBuilder();
    
    /** The table that holds the text. */
    protected PdfPTable table;
    
    /** The current cell. */
    protected PdfPCell cell; 
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        zip.AddFile(RESOURCE, "");
        
        using (MemoryStream ms = new MemoryStream()) {
          // step 1
          using (document = new Document()) {
            // step 2
            PdfWriter.GetInstance(document, ms);
            // step 3
            document.Open();
            // step 4
            f = new Font(BaseFont.CreateFont("c:/windows/fonts/arialuni.ttf",
              BaseFont.IDENTITY_H, BaseFont.EMBEDDED
            ));
            using (XmlReader xr = XmlReader.Create(RESOURCE)) {
              table = new PdfPTable(1);
              xr.MoveToContent();
              while (xr.Read()) {
                switch (xr.NodeType) {
                  case XmlNodeType.Element:
                    StartElement(xr);
                    break;
                  case XmlNodeType.Text:
                    buf.Append(xr.Value.Trim());
                    break;
                  case XmlNodeType.EndElement:
                    EndElement(xr.Name);
                    break;
                }
              }
            }
          }
          zip.AddEntry(RESULT, ms.ToArray()); 
        }
        zip.Save(stream);             
      }
    }       
// ---------------------------------------------------------------------------
    public void StartElement(XmlReader xr) {
      string name =  xr.Name;
      if ("message".Equals(name)) {
        buf = new StringBuilder();
        cell = new PdfPCell();
        cell.Border = PdfPCell.NO_BORDER;
        string direction = xr.GetAttribute("direction") != null
          ? xr.GetAttribute("direction")
          : ""
        ;
        if ("RTL".Equals(direction)) {
          cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
        }
      }
      else if ("pace".Equals(name)) {
        table = new PdfPTable(1);
        table.WidthPercentage = 100;
      }
    }
// ---------------------------------------------------------------------------
    public void EndElement(string name) {
      if ("big".Equals(name)) {
        Chunk bold = new Chunk(Strip(buf), f);
        bold.SetTextRenderMode(
          PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE, 0.5f,
          GrayColor.GRAYBLACK
        );
        Paragraph p = new Paragraph(bold);
        p.Alignment = Element.ALIGN_LEFT;
        cell.AddElement(p);
        buf = new StringBuilder();
      }
      else if ("message".Equals(name)) {
        Paragraph p = new Paragraph(Strip(buf), f);
        p.Alignment = Element.ALIGN_LEFT;
        cell.AddElement(p);
        table.AddCell(cell);
        buf = new StringBuilder();
      }
      else if ("pace".Equals(name)) {
        document.Add(table);
      }
    }
// ---------------------------------------------------------------------------    
  /**
   * Replaces all the newline characters by a space.
   * 
   * @param buf the original StringBuffer
   * @return a String without newlines
   */
    protected string Strip(StringBuilder buf) {
      buf.Replace("\n", " ");
      return buf.ToString();
    }    
// ===========================================================================
  }
}