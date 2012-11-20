/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using kuujinbo.iTextInAction2Ed.Chapter11;

namespace kuujinbo.iTextInAction2Ed.Chapter16 {
  public class ListUsedFonts : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const String RESULT = "fonts.txt";
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        FontTypes f = new FontTypes();
        byte[] pdf = Utility.PdfBytes(f);
        zip.AddEntry(Utility.ResultFileName(f.ToString() + ".pdf"), pdf);
        IEnumerable<String> set1 = from m 
          in new ListUsedFonts().ListFonts(pdf)
          orderby m select m;
        StringBuilder sb = new StringBuilder();
/*
 * i don't have c:/windows/fonts/ARBLI__.TTF on my system;
 * if you compare to Java example __THIS__ result file will have
 * one __LESS__ font listed!
 */
        foreach (String fontname in set1) {
          sb.AppendLine(fontname);
        }        
        zip.AddEntry(RESULT, sb.ToString());       
        zip.Save(stream);
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Creates a HashSet containing information about the fonts in the src PDF file.
     * @param src the PDF file
     * 
     * HashSet only available in .NET >= 3.5
     */
    public HashSet<String> ListFonts(byte[] src) {
      HashSet<String> set = new HashSet<String>();
      PdfReader reader = new PdfReader(src);
      PdfDictionary resources;
      for (int k = 1; k <= reader.NumberOfPages; ++k) {
        resources = reader.GetPageN(k).GetAsDict(PdfName.RESOURCES);
        ProcessResource(set, resources);
      }
      return set;
    }
// ---------------------------------------------------------------------------    
    /**
     * Extracts the font names from page or XObject resources.
     * @param set the HashSet with the font names
     * @param resources the resources dictionary
     */
    public void ProcessResource(HashSet<String> set, PdfDictionary resource) {
        if (resource == null) return;
        
        PdfDictionary xobjects = resource.GetAsDict(PdfName.XOBJECT);
        if (xobjects != null) {
          foreach (PdfName key in xobjects.Keys) {
            ProcessResource(set, xobjects.GetAsDict(key));
          }
        }
        PdfDictionary fonts = resource.GetAsDict(PdfName.FONT);
        if (fonts == null) return;
        
        PdfDictionary font;
        foreach (PdfName key in fonts.Keys) {
          font = fonts.GetAsDict(key);
          String name = font.GetAsName(PdfName.BASEFONT).ToString();
          if (name.Length > 8 && name.Substring(7, 1) == "+") {
            name = String.Format(
              "{0} subset ({1})", 
              name.Substring(8), name.Substring(1, 7)
            );
          }
          else {
              name = name.Substring(1);
              PdfDictionary desc = font.GetAsDict(PdfName.FONTDESCRIPTOR);
              if (desc == null) {
                name += " nofontdescriptor";
              }
              else if (desc.Get(PdfName.FONTFILE) != null) {
                name += " (Type 1) embedded";
              }
              else if (desc.Get(PdfName.FONTFILE2) != null) {
                name += " (TrueType) embedded";
              }
              else if (desc.Get(PdfName.FONTFILE3) != null) {
                name += " (" + font.GetAsName(PdfName.SUBTYPE).ToString().Substring(1) + ") embedded";
              }
          }
          set.Add(name);
        }
    }
// ===========================================================================  
  }
}