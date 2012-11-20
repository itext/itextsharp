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

namespace kuujinbo.iTextInAction2Ed.Chapter12 {
  public class EncryptionPdf : IWriter {
// ===========================================================================
    /** User password. */
    public readonly byte[] USER = ASCIIEncoding.UTF8.GetBytes("Hello");
    /** Owner password. */
    public readonly byte[] OWNER = ASCIIEncoding.UTF8.GetBytes("World");

    /** The resulting PDF file. */
    public const string RESULT1 = "encryption.pdf";
    /** The resulting PDF file. */
    public const string RESULT2 = "encryption_decrypted.pdf";
    /** The resulting PDF file. */
    public const string RESULT3 = "encryption_encrypted.pdf";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        EncryptionPdf metadata = new EncryptionPdf();
        byte[] enc1 = metadata.CreatePdf();        
        zip.AddEntry(RESULT1,enc1);
        byte[] enc2 = metadata.DecryptPdf(enc1);
        zip.AddEntry(RESULT2, enc2);
        zip.AddEntry(RESULT3, metadata.EncryptPdf(enc2));
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
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          writer.SetEncryption(
            USER, OWNER, 
            PdfWriter.ALLOW_PRINTING, 
            PdfWriter.STANDARD_ENCRYPTION_128
          );
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
    public byte[] DecryptPdf(byte[] src) {
      PdfReader reader = new PdfReader(src, OWNER);
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Manipulates a PDF file src with the file dest as result
     * @param src the original PDF
     */
    public byte[] EncryptPdf(byte[] src) {
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          stamper.SetEncryption(
            USER, OWNER, 
            PdfWriter.ALLOW_PRINTING, 
            PdfWriter.ENCRYPTION_AES_128 | PdfWriter.DO_NOT_ENCRYPT_METADATA
          );
        }
        return ms.ToArray();
      }
    }    
// ===========================================================================
  }
}