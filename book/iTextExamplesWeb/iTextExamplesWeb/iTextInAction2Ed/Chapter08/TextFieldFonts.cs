/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using iTextSharp.text;
using iTextSharp.text.pdf;
/*
 * as stated in the book, this won't work on all systems: the PDF viewer needs
 * to have access to a font with the Chinese/Korean glyphs.
 * 
 * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
 * the geniuses at micro$oft decided __NOT__ to include arialuni.ttf in some
 * versions of window$ 7. to add insult to injury they expect you to pay
 * for the damn font:
 * 
 * http://www.microsoft.com/typography/fonts/family.aspx?FID=24
 * 
 * mileage will vary with this example and __ALL__ others using arialuni.ttf
 * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
 * 
*/
namespace kuujinbo.iTextInAction2Ed.Chapter08 {
  public class TextFieldFonts : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String RESULT1 = "unicode_field_1.pdf";
    /** The resulting PDF. */
    public const String RESULT2 = "unicode_field_2.pdf";
    /** The resulting PDF. */
    public const String RESULT3 = "unicode_field_3.pdf";
    /** The resulting PDF. */
    public const String RESULT4 = "unicode_field_4.pdf";
    /** The resulting PDF. */
    public const String RESULT5 = "unicode_field_5.pdf";
    /** The resulting PDF. */
    public const String RESULT6 = "unicode_field_6.pdf";
    /** The resulting PDF. */
    public const String RESULT7 = "unicode_field_7.pdf";
    /** The resulting PDF. */
    public const String RESULT8 = "unicode_field_8.pdf";

    public const string TEXT = 
      "These are the protagonists in 'Hero', a movie by Zhang Yimou:\n"
      + "\u7121\u540d (Nameless), \u6b98\u528d (Broken Sword), "
      + "\u98db\u96ea (Flying Snow), \u5982\u6708 (Moon), "
      + "\u79e6\u738b (the King), and \u9577\u7a7a (Sky).";
    public const string BINJIP =
      "The Korean title of the movie 3-Iron is \ube48\uc9d1 (Bin-Jip)";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        TextFieldFonts example = new TextFieldFonts();
        byte[] pdf1 = example.CreatePdf(false, false);
        byte[] pdf2 = example.CreatePdf(true, false);
        byte[] pdf3 = example.CreatePdf(false, true);
        zip.AddEntry(RESULT1, pdf1);
        zip.AddEntry(RESULT2, pdf2);
        zip.AddEntry(RESULT3, pdf3); 
        zip.AddEntry(RESULT4, example.ManipulatePdf(pdf1));
        zip.AddEntry(RESULT5, example.ManipulatePdf(pdf2));
        zip.AddEntry(RESULT6, example.ManipulatePdf(pdf3));      
        zip.AddEntry(RESULT7, example.ManipulatePdfFont1(pdf3));      
        zip.AddEntry(RESULT8, example.ManipulatePdfFont2(pdf3));      
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Creates a page with specified font and appearance.
     * @param appearances sets the need appearances flag if true
     * @param font adds a substitution font if true
     */   
    public byte[] CreatePdf(bool appearances, bool font) {
      using (MemoryStream ms = new MemoryStream()) {
        using (Document document = new Document()) {
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          document.Open();
          writer.AcroForm.NeedAppearances = appearances;
          TextField text = new TextField(
            writer, new Rectangle(36, 806, 559, 780), "description"
          );
          text.Options = TextField.MULTILINE;
          if (font) {
            BaseFont unicode = BaseFont.CreateFont(
              "c:/windows/fonts/arialuni.ttf", 
              BaseFont.IDENTITY_H, BaseFont.EMBEDDED
            );
            text.ExtensionFont = BaseFont.CreateFont();
            List<BaseFont> list = new List<BaseFont>();
            list.Add(unicode);
            text.SubstitutionFonts = list;
          }
          text.Text = TEXT;
          writer.AddAnnotation(text.GetTextField());
        }
        return ms.ToArray();
      }
    }  
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] ManipulatePdf(byte[] src) {
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          AcroFields form = stamper.AcroFields;
          form.SetField("description", BINJIP);
        }
        return ms.ToArray();
      }
    }
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] ManipulatePdfFont1(byte[] src) {
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          AcroFields form = stamper.AcroFields;
          BaseFont unicode = BaseFont.CreateFont(
            "HYSMyeongJoStd-Medium", "UniKS-UCS2-H", BaseFont.NOT_EMBEDDED
          );
          form.SetFieldProperty("description", "textfont", unicode, null);
          form.SetField("description", BINJIP);
        }
        return ms.ToArray();
      }
    }
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] ManipulatePdfFont2(byte[] src) {
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          AcroFields form = stamper.AcroFields;
          BaseFont unicode = BaseFont.CreateFont(
            "c:/windows/fonts/arialuni.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED
          );
          form.AddSubstitutionFont(unicode);
          form.SetField("description", BINJIP);
        }
        return ms.ToArray();
      }
    }    
// ===========================================================================
  }
}