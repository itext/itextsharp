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
  public class Peace : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const String RESULT = "peace.pdf";
    /** The XML file with the text. */
    public static string RESOURCE = Path.Combine(
      Utility.ResourceXml, "peace.xml"
    );
    public readonly string FONT_PATH = Utility.ResourceFonts;
    /** Paths to and encodings of fonts we're going to use in this example */
    public string[][] FONTS { 
      get { return new string[][] {
          new string[] {
            "c:/windows/fonts/arialuni.ttf", BaseFont.IDENTITY_H
          },
          new string[] {
            string.Format(FONT_PATH, "abserif4_5.ttf"), BaseFont.IDENTITY_H
          },
          new string[] {
            string.Format(FONT_PATH, "damase.ttf"), BaseFont.IDENTITY_H
          },
          new string[] {
            string.Format(FONT_PATH, "fsex2p00_public.ttf"), BaseFont.IDENTITY_H
          }
        };
      }
    }
    /** Holds he fonts that can be used for the peace message. */
    public FontSelector fs;

    /** The columns that contains the message. */
    protected PdfPTable table;

    /** The language. */
    protected String language;

    /** The countries. */
    protected String countries;

    /** Indicates when the text should be written from right to left. */
    protected bool rtl; 
    
    /** The StringBuilder that holds the characters. */
    protected StringBuilder buf = new StringBuilder();
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        zip.AddFile(RESOURCE, "");       
        using (MemoryStream ms = new MemoryStream()) {
          using (Document document = new Document(PageSize.A4.Rotate())) {
            // step 2
            PdfWriter.GetInstance(document, ms);
            // step 3
            document.Open();
            // step 4
            fs = new FontSelector();
            for (int i = 0; i < FONTS.Length; i++) {
              fs.AddFont(FontFactory.GetFont(
                FONTS[i][0], FONTS[i][1], BaseFont.EMBEDDED
              ));
            }
            table = new PdfPTable(3);
            table.DefaultCell.Padding = 3;
            table.DefaultCell.UseAscender = true;
            table.DefaultCell.UseDescender = true;
            
            using (XmlReader xr = XmlReader.Create(RESOURCE)) {
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
            document.Add(table); 
          }
          zip.AddEntry(RESULT, ms.ToArray());
        }
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------
    public void StartElement(XmlReader xr) {
      string name =  xr.Name;
      if ("pace".Equals(name)) {
        buf = new StringBuilder();
        language = xr.GetAttribute("language");
        countries = xr.GetAttribute("countries");
        rtl = "RTL".Equals(xr.GetAttribute("direction")) ? true : false;
      }
    }
// ---------------------------------------------------------------------------
    public void EndElement(string name) {
      if ("pace".Equals(name)) {
        PdfPCell cell = new PdfPCell();
        cell.AddElement(fs.Process(buf.ToString()));
        cell.Padding = 3;
        cell.UseAscender = true;
        cell.UseDescender = true;
        if (rtl) {
          cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
        }
        table.AddCell(language);
        table.AddCell(cell);
        table.AddCell(countries);
      }    
    }    
// ===========================================================================
  }
}