/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.Data;
using System.Data.Common;
using System.IO;
using Ionic.Zip;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter13 {
  public class PageLabelExample : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const string RESULT = "page_labels.pdf";
    /** The resulting PDF file. */
    public const string RESULT2 = "page_labels_changed.pdf";
    /** A text file containing page numbers and labels. */
    public const string LABELS = "page_labels.txt";
    /** SQL statements */
    public readonly string[] SQL = {
      "SELECT country FROM film_country ORDER BY country",
      "SELECT name FROM film_director ORDER BY name",
      "SELECT title FROM film_movietitle ORDER BY title"
    };
    /** SQL statements */
    public readonly string[] FIELD = { "country", "name", "title" };
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        PageLabelExample labels = new PageLabelExample();
        byte[] pdf = labels.CreatePdf();
        zip.AddEntry(RESULT, pdf);           
        zip.AddEntry(LABELS, labels.ListPageLabels(pdf));           
        zip.AddEntry(RESULT2, labels.ManipulatePageLabel(pdf));           
        zip.Save(stream);             
      }
    }        
// ---------------------------------------------------------------------------    
    /**
     * Creates a PDF document.
     */
    public byte[] CreatePdf() {
      using (MemoryStream ms = new MemoryStream()) { 
        using (var c =  AdoDB.Provider.CreateConnection()) {
          c.ConnectionString = AdoDB.CS;
          c.Open();
          // step 1
          using (Document document = new Document(PageSize.A5)) {
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            // step 3
            document.Open();
            // step 4
            int[] start = new int[3];
            for (int i = 0; i < 3; i++) {
              start[i] = writer.PageNumber;
              AddParagraphs(document, c, SQL[i], FIELD[i]);
              document.NewPage();
            }
            PdfPageLabels labels = new PdfPageLabels();
            labels.AddPageLabel(start[0], PdfPageLabels.UPPERCASE_LETTERS);
            labels.AddPageLabel(start[1], PdfPageLabels.DECIMAL_ARABIC_NUMERALS);
            labels.AddPageLabel(
              start[2], PdfPageLabels.DECIMAL_ARABIC_NUMERALS, 
              "Movies-", start[2] - start[1] + 1
            );
            writer.PageLabels = labels;
          }
          return ms.ToArray();
        }
      }
    }
// ---------------------------------------------------------------------------    
    public void AddParagraphs(
        Document document, DbConnection connection, String sql, String field) 
    {
      using (DbCommand cmd = connection.CreateCommand()) {
        cmd.CommandText = sql;  
        using (var r = cmd.ExecuteReader()) {
          while (r.Read()) {
            document.Add(new Paragraph(r[field].ToString()));
          }
        }
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Reads the page labels from an existing PDF
     * @param src the existing PDF
     */   
    public string ListPageLabels(byte[] src) {
      StringBuilder sb = new StringBuilder();
      String[] labels = PdfPageLabels.GetPageLabels(new PdfReader(src));
      for (int i = 0; i < labels.Length; i++) {
        sb.Append(labels[i]);
        sb.AppendLine();
      }
      return sb.ToString();
    }
// --------------------------------------------------------------------------- 
    /**
     * Manipulates the page labels at the lowest PDF level.
     * @param src  the source file
     */   
    public byte[] ManipulatePageLabel(byte[] src) {
      PdfReader reader = new PdfReader(src);
      PdfDictionary root = reader.Catalog;
      PdfDictionary labels = root.GetAsDict(PdfName.PAGELABELS);
      PdfArray nums = labels.GetAsArray(PdfName.NUMS);
      int n;
      PdfDictionary pagelabel;
      for (int i = 0; i < nums.Size; i++) {
        n = nums.GetAsNumber(i).IntValue;
        i++;
        if (n == 5) {
          pagelabel = nums.GetAsDict(i);
          pagelabel.Remove(PdfName.ST);
          pagelabel.Put(PdfName.P, new PdfString("Film-"));
        }
      }
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
        }
        return ms.ToArray();
      }
    }    
// ===========================================================================
  }
}