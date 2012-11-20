/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace kuujinbo.iTextInAction2Ed.Chapter16 {
  public class ResizeImage : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const String RESULT = "resized_image.pdf";
    /** The multiplication factor for the image. */
    public const float FACTOR = 0.5f;
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        SpecialId s = new SpecialId();
        byte[] pdf = s.CreatePdf();
        zip.AddEntry(Utility.ResultFileName(s.ToString() + ".pdf"), pdf);       
        zip.AddEntry(RESULT, new ResizeImage().ManipulatePdf(pdf));       
        zip.Save(stream);             
      }
    } 
// ---------------------------------------------------------------------------
    /**
     * Manipulates a PDF file src with the byte array as result
     */    
    public byte[] ManipulatePdf(byte[] pdf) {
      PdfName key = new PdfName("ITXT_SpecialId");
      PdfName value = new PdfName("123456789");
      // Read the file
      PdfReader reader = new PdfReader(pdf);
      int n = reader.XrefSize;
      PdfObject pdfObject;
      PRStream prStream;
      // Look for image and manipulate image prStream
      for (int i = 0; i < n; i++) {
        pdfObject = reader.GetPdfObject(i);
        if (pdfObject == null || !pdfObject.IsStream()) continue;
        
        prStream = (PRStream) pdfObject;
        byte[] imageBytes;
        if (value.Equals(prStream.Get(key))) {
          PdfImageObject image = new PdfImageObject(prStream);
          using (System.Drawing.Image original = image.GetDrawingImage()) {
            if (original == null) continue;
            int width = (int)(original.Width * FACTOR);
            int height = (int)(original.Height * FACTOR);
            
            using (System.Drawing.Image thumb = new Bitmap(width, height) ) {
              using ( Graphics graphic = Graphics.FromImage(thumb) ) {
                graphic.DrawImage(original, 0, 0, width, height);
                using (MemoryStream ms = new MemoryStream()) {
                  thumb.Save(ms, ImageFormat.Jpeg);
                  imageBytes = ms.ToArray();
                }
              }
            }
            prStream.Clear();
            prStream.SetData(imageBytes, false, PRStream.NO_COMPRESSION);
            prStream.Put(PdfName.TYPE, PdfName.XOBJECT);
            prStream.Put(PdfName.SUBTYPE, PdfName.IMAGE);
            prStream.Put(key, value);
            prStream.Put(PdfName.FILTER, PdfName.DCTDECODE);
            prStream.Put(PdfName.WIDTH, new PdfNumber(width));
            prStream.Put(PdfName.HEIGHT, new PdfNumber(height));
            prStream.Put(PdfName.BITSPERCOMPONENT, new PdfNumber(8));
            prStream.Put(PdfName.COLORSPACE, PdfName.DEVICERGB);            
          }
        }
      }
      // Save altered PDF
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}