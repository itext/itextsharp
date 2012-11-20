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
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.xml.xmp;

namespace kuujinbo.iTextInAction2Ed.Chapter12 {
  public class MetadataXmp : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const string RESULT1 = "xmp_metadata.pdf";
    /** The resulting PDF file. */
    public const string RESULT2 = "xmp_metadata_automatic.pdf";
    /** The resulting PDF file. */
    public const string RESULT3 = "xmp_metadata_added.pdf";
    /** The resulting PDF file. */
    public const string RESULT4 = "xmp.xml";
// ---------------------------------------------------------------------------        
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        MetadataXmp metadata = new MetadataXmp();
        byte[] pdf = metadata.CreatePdf();
        zip.AddEntry(RESULT1, pdf);
        zip.AddEntry(RESULT2, metadata.CreatePdfAutomatic());
        byte[] manipulated = metadata.ManipulatePdf(
          new MetadataPdf().CreatePdf()
        );
        zip.AddEntry(RESULT3, manipulated );
        zip.AddEntry(RESULT4, metadata.ReadXmpMetadata(manipulated));
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
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          using (MemoryStream msXmp = new MemoryStream()) {
            XmpWriter xmp = new XmpWriter(msXmp);
            XmpSchema dc = new DublinCoreSchema();
            XmpArray subject = new XmpArray(XmpArray.UNORDERED);
            subject.Add("Hello World");
            subject.Add("XMP & Metadata");
            subject.Add("Metadata");
            dc.SetProperty(DublinCoreSchema.SUBJECT, subject);
            xmp.AddRdfDescription(dc);
            PdfSchema pdf = new PdfSchema();
/*
 *  iTextSharp uses Item property instead of Java setProperty() method
 * 
 *      pdf.SetProperty(PdfSchema.KEYWORDS, "Hello World, XMP, Metadata");
 *      pdf.SetProperty(PdfSchema.VERSION, "1.4");
 */
            pdf[PdfSchema.KEYWORDS] = "Hello World, XMP, Metadata";
            pdf[PdfSchema.VERSION] = "1.4";
            xmp.AddRdfDescription(pdf);
            xmp.Close();
            writer.XmpMetadata = ms.ToArray();
          }
          // step 3
          document.Open();
          // step 4
          document.Add(new Paragraph("Hello World"));
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------  
    /**
     * Creates a PDF document.
     */
    public byte[] CreatePdfAutomatic() {
      using (MemoryStream ms = new MemoryStream()) {
        // step 1
        using (Document document = new Document()) {
          // step 2
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          document.AddTitle("Hello World example");
          document.AddSubject("This example shows how to add metadata & XMP");
          document.AddKeywords("Metadata, iText, step 3");
          document.AddCreator("My program using 'iText'");
          document.AddAuthor("Bruno Lowagie & Paulo Soares");
          writer.CreateXmpMetadata();
          // step 3
          document.Open();
          // step 4
          document.Add(new Paragraph("Hello World"));
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
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          Dictionary<String, String> info = reader.Info;
          using (MemoryStream msXmp = new MemoryStream()) {
            XmpWriter xmp = new XmpWriter(msXmp, info);
            xmp.Close();
            stamper.XmpMetadata = msXmp.ToArray();      
          }
        }
        return ms.ToArray();
      }
    }      
// ---------------------------------------------------------------------------
    /**
     * Reads the XML stream inside a PDF file into an XML file.
     * @param src  A PDF file containing XMP data
     */
    public string ReadXmpMetadata(byte[] src) {
      PdfReader reader = new PdfReader(src);
      byte[] b = reader.Metadata;
      return Encoding.UTF8.GetString(b, 0, b.Length);
    }  
// ===========================================================================
  }
}