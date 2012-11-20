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
using kuujinbo.iTextInAction2Ed.Chapter01;

namespace kuujinbo.iTextInAction2Ed.Chapter15 {
  public class ParsingHelloWorld : IWriter {
// ===========================================================================
    /** The resulting PDF. */
    public const String PDF = "hello_reverse.pdf";
    /** A possible resulting after parsing the PDF. */
    public const String TEXT1 = "result1.txt";
    /** A possible resulting after parsing the PDF. */
    public const String TEXT2 = "result2.txt";
    /** A possible resulting after parsing the PDF. */
    public const String TEXT3 = "result3.txt";
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        ParsingHelloWorld example = new ParsingHelloWorld();
        byte[] ePdf = example.CreatePdf();
        zip.AddEntry(PDF, ePdf);
        HelloWorld hello = new HelloWorld();
        byte[] hPdf = Utility.PdfBytes(hello);
        zip.AddEntry(Utility.ResultFileName(hello.ToString() + ".pdf"), hPdf);       
        zip.AddEntry(TEXT1, example.ParsePdf(hPdf));
        zip.AddEntry(TEXT2, example.ParsePdf(ePdf));
        zip.AddEntry(TEXT3, example.ExtractText(ePdf));
        zip.Save(stream);             
      }
    }
// --------------------------------------------------------------------------- 
    /**
     * Generates a PDF file with the text 'Hello World'
     */    
    public byte[] CreatePdf() {
      using (MemoryStream ms = new MemoryStream()) {
        using (Document document = new Document()) {
          // step 2
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          // step 3
          document.Open();
          // step 4
          // we add the text to the direct content, but not in the right order
          PdfContentByte cb = writer.DirectContent;
          BaseFont bf = BaseFont.CreateFont();
          cb.BeginText();
          cb.SetFontAndSize(bf, 12);
          cb.MoveText(88.66f, 367); 
          cb.ShowText("ld");
          cb.MoveText(-22f, 0); 
          cb.ShowText("Wor");
          cb.MoveText(-15.33f, 0); 
          cb.ShowText("llo");
          cb.MoveText(-15.33f, 0); 
          cb.ShowText("He");
          cb.EndText();
          // we also add text in a form XObject
          PdfTemplate tmp = cb.CreateTemplate(250, 25);
          tmp.BeginText();
          tmp.SetFontAndSize(bf, 12);
          tmp.MoveText(0, 7);
          tmp.ShowText("Hello People");
          tmp.EndText();
          cb.AddTemplate(tmp, 36, 343);
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Parses the PDF using PRTokeniser
     * @param src the ]original PDF file
]     */
    public string ParsePdf(byte[] src) {
      PdfReader reader = new PdfReader(src);
      // we can inspect the syntax of the imported page
      byte[] streamBytes = reader.GetPageContent(1);
      StringBuilder sb = new StringBuilder();
      PRTokeniser tokenizer = new PRTokeniser(streamBytes);
      while (tokenizer.NextToken()) {
        if (tokenizer.TokenType == PRTokeniser.TokType.STRING) {
          sb.AppendLine(tokenizer.StringValue);
        }
      }
      return sb.ToString();
    }
// ---------------------------------------------------------------------------    
    /**
     * Extracts text from a PDF document.
     * @param src the original PDF document
     */
    public string ExtractText(byte[] src) {
      PdfReader reader = new PdfReader(src);
      MyTextRenderListener listener = new MyTextRenderListener();
      PdfContentStreamProcessor processor = new PdfContentStreamProcessor(listener);
      PdfDictionary pageDic = reader.GetPageN(1);
      PdfDictionary resourcesDic = pageDic.GetAsDict(PdfName.RESOURCES);
      processor.ProcessContent(
        ContentByteUtils.GetContentBytesForPage(reader, 1), 
        resourcesDic
      );
      return listener.Text.ToString();
    }    
// ===========================================================================
  }
}