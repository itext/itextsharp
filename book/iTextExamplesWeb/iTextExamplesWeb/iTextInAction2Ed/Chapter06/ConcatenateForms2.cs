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

namespace kuujinbo.iTextInAction2Ed.Chapter06 {
  public class ConcatenateForms2 : IWriter {
// ===========================================================================
    public const string RESULT = "concatenated_forms2.pdf";
/** The original PDF file. */
    public const string copyName = "datasheet.pdf";
    public readonly string DATASHEET = Path.Combine(
      Utility.ResourcePdf, copyName
    );
// ---------------------------------------------------------------------------    
    public void Write(Stream stream) {   
      using (ZipFile zip = new ZipFile()) {
        using (MemoryStream ms = new MemoryStream()) {
          // Create a PdfCopyFields object
          PdfCopyFields copy = new PdfCopyFields(ms);
          // add a document
          copy.AddDocument(new PdfReader(RenameFieldsIn(
            DATASHEET, 1
          )));
          // add a document
          copy.AddDocument(new PdfReader(RenameFieldsIn(
            DATASHEET, 2
          )));
          // close the PdfCopyFields object
          copy.Close();
          zip.AddEntry(RESULT, ms.ToArray());            
        }
        zip.AddFile(DATASHEET, "");
        zip.Save(stream);        
      }     
    }
// ---------------------------------------------------------------------------    
    /**
     * Renames the fields in an interactive form.
     * @param datasheet the path to the original form
     * @param i a number that needs to be appended to the field names
     * @return a byte[] containing an altered PDF file
     */
    private static byte[] RenameFieldsIn(string datasheet, int i) {
      List<string> form_keys = new List<string>();
      using (var ms = new MemoryStream()) {
        // Create the stamper
        using (PdfStamper stamper = new PdfStamper(
          new PdfReader(datasheet), ms)) 
        {
          // Get the fields
          AcroFields form = stamper.AcroFields;
          // so we aren't hit with 'Collection was modified' exception
          foreach (string k in stamper.AcroFields.Fields.Keys) {
            form_keys.Add(k);
          }
          // Loop over the fields
          foreach (string key in form_keys) {
            // rename the fields
            form.RenameField(key, string.Format("{0}_{1}", key, i));
          }
        }
        return ms.ToArray();
      }
    }    
// ===========================================================================
  }
}