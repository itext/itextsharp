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
using iTextSharp.text.pdf.parser;
using kuujinbo.iTextInAction2Ed.Chapter03;

namespace kuujinbo.iTextInAction2Ed.Chapter15 {
  public class InspectPageContent : IWriter {
// ===========================================================================
    /** Text file containing information about a PDF file. */
    public const String RESULT = "calendar_info.txt";
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        MovieTemplates m = new MovieTemplates();
        byte[] mPdf = Utility.PdfBytes(m);
        zip.AddEntry(Utility.ResultFileName(m.ToString() + ".pdf"), mPdf);
        InspectPageContent i = new InspectPageContent();
        zip.AddEntry(RESULT, i.InspectPdf(mPdf));
        zip.Save(stream);             
      }
    }    
// --------------------------------------------------------------------------- 
    /**
     * Parses object and content information of a PDF into a text file.
     * @param pdf the original PDF
     * 
     * this method uses code from; 
     * PdfContentReaderTool.ListContentStreamForPage()
     * so i can pass in a byte array instead of file path
     * 
     */
    public string InspectPdf(byte[] pdf) {
      PdfReader reader = new PdfReader(pdf);
      int maxPageNum = reader.NumberOfPages;
      StringBuilder sb = new StringBuilder();
      for (int pageNum = 1; pageNum <= maxPageNum; pageNum++){
        sb.AppendLine("==============Page " + pageNum + "====================");
        sb.AppendLine("- - - - - Dictionary - - - - - -");
        PdfDictionary pageDictionary = reader.GetPageN(pageNum);
        sb.AppendLine(
          PdfContentReaderTool.GetDictionaryDetail(pageDictionary)
        );

        sb.AppendLine("- - - - - XObject Summary - - - - - -");
        sb.AppendLine(PdfContentReaderTool.GetXObjectDetail(
          pageDictionary.GetAsDict(PdfName.RESOURCES))
        );
        
        sb.AppendLine("- - - - - Content Stream - - - - - -");
        RandomAccessFileOrArray f = reader.SafeFile;

        byte[] contentBytes = reader.GetPageContent(pageNum, f);
        f.Close();

        foreach (byte b in contentBytes) {
          sb.Append((char)b);
        }
        
        sb.AppendLine("- - - - - Text Extraction - - - - - -");
        String extractedText = PdfTextExtractor.GetTextFromPage(
          reader, pageNum, new LocationTextExtractionStrategy()
        );
        if (extractedText.Length != 0) {
          sb.AppendLine(extractedText);
        }
        else {
          sb.AppendLine("No text found on page " + pageNum);
        }
        sb.AppendLine();      
      }   
      return sb.ToString();   
    }    
// ===========================================================================
	}
}
