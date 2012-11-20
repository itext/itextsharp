/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter16 {
  public class EmbedFontPostFacto : IWriter {
// ===========================================================================
    /** The first resulting PDF file. */
    public const String RESULT1 = "without_font.pdf";
    /** The second resulting PDF file. */
    public const String RESULT2 = "with_font.pdf";
    /** A special font. */
    public readonly String FONT = Path.Combine(
      Utility.ResourceFonts, "wds011402.ttf"
    );     
    /** The name of the special font. */
    public const String FONTNAME = "WaltDisneyScriptv4.1";
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        EmbedFontPostFacto example = new EmbedFontPostFacto();
        byte[] pdf = example.CreatePdf();
        zip.AddEntry(RESULT1, pdf);       
        zip.AddEntry(RESULT2, example.ManipulatePdf(pdf));       
        zip.Save(stream);             
      }
    }     
// --------------------------------------------------------------------------- 
    /**
     * Creates a PDF document.
     */
    public byte[] CreatePdf() {
      using (MemoryStream ms = new MemoryStream()) {
        // step 1
        using (Document document = new Document()) {    
          // step 2
          PdfWriter.GetInstance(document, ms);
          // step 3
          document.Open();
          // step 4:
          Font font = new Font(
            BaseFont.CreateFont(FONT, "", BaseFont.NOT_EMBEDDED), 60
          );
          document.Add(new Paragraph("iText in Action", font));
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------     
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] ManipulatePdf(byte[] src) {
      // the font file
      byte[] fontfile = null;
      using (FileStream fs = new FileStream(
        FONT, FileMode.Open, FileAccess.Read)) 
      {
        fontfile = new byte[fs.Length];
        fs.Read(fontfile, 0, (int) fs.Length);
      }
      // create a new stream for the font file
      PdfStream stream = new PdfStream(fontfile);
      stream.FlateCompress();
      stream.Put(PdfName.LENGTH1, new PdfNumber(fontfile.Length));
      // create a reader object
      PdfReader reader = new PdfReader(src);
      int n = reader.XrefSize;
      PdfDictionary font;
      using (MemoryStream ms = new MemoryStream()) {      
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          PdfName fontname = new PdfName(FONTNAME);
          for (int i = 0; i < n; i++) {
            PdfObject objectPdf = reader.GetPdfObject(i);
            if (objectPdf == null || !objectPdf.IsDictionary()) {
              continue;
            }
            font = (PdfDictionary)objectPdf;
            if (PdfName.FONTDESCRIPTOR.Equals(font.Get(PdfName.TYPE))
                && fontname.Equals(font.Get(PdfName.FONTNAME))) 
            {
              PdfIndirectObject objref = stamper.Writer.AddToBody(stream);
              font.Put(PdfName.FONTFILE2, objref.IndirectReference);
            }
          }
        }
        return ms.ToArray();
      }
    } 
// ===========================================================================    
  }
}