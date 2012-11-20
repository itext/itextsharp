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
using iTextSharp.text.xml;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter16 {
  public class KubrickDocumentary : IWriter {
// ===========================================================================
    /** The filename of the resulting PDF. */
    public const String RESULT = "kubrick_documentary.pdf";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        KubrickDocumentary kubrick = new KubrickDocumentary();
        byte[] ePdf = kubrick.CreatePdf();
        zip.AddEntry(RESULT, ePdf);
        kubrick.ExtractDocLevelAttachments(ePdf, zip);
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Creates the PDF.
     * @return the bytes of a PDF file.
     */
    public byte[] CreatePdf() {
      IEnumerable<Movie> movies = PojoFactory.GetMovies(1);
      using (MemoryStream ms = new MemoryStream()) {
        using (Document document = new Document()) {
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          document.Open();
          document.Add(new Paragraph(
            "'Stanley Kubrick: A Life in Pictures'"
            + " is a documentary about Stanley Kubrick and his films:"
          ));
          StringBuilder sb = new StringBuilder();
          sb.AppendLine("<movies>");
          List list = new List(List.UNORDERED, 20);
          foreach (Movie movie in movies) {
            sb.AppendLine("<movie>");
            sb.AppendLine(String.Format(
              "<title>{0}</title>",
              XMLUtil.EscapeXML(movie.MovieTitle, true)
            ));
            sb.AppendLine(String.Format("<year>{0}</year>", movie.Year));
            sb.AppendLine(String.Format(
              "<duration>{0}</duration>", movie.Duration
            ));
            sb.AppendLine("</movie>");
            ListItem item = new ListItem(movie.MovieTitle);
            list.Add(item);
          }
          document.Add(list);
          sb.Append("</movies>");
          PdfFileSpecification fs = PdfFileSpecification.FileEmbedded(
            writer, null, "kubrick.xml", Encoding.UTF8.GetBytes(sb.ToString())
            //txt.toByteArray()
          );
          writer.AddFileAttachment(fs);
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Extracts document level attachments
     * @param PDF from which document level attachments will be extracted
     * @param zip the ZipFile object to add the extracted images
     */
    public void ExtractDocLevelAttachments(byte[] pdf, ZipFile zip) {
      PdfReader reader = new PdfReader(pdf);
      PdfDictionary root = reader.Catalog;
      PdfDictionary documentnames = root.GetAsDict(PdfName.NAMES);
      PdfDictionary embeddedfiles = 
          documentnames.GetAsDict(PdfName.EMBEDDEDFILES);
      PdfArray filespecs = embeddedfiles.GetAsArray(PdfName.NAMES);
      for (int i = 0; i < filespecs.Size; ) {
        filespecs.GetAsString(i++);
        PdfDictionary filespec = filespecs.GetAsDict(i++);
        PdfDictionary refs = filespec.GetAsDict(PdfName.EF);
        foreach (PdfName key in refs.Keys) {
          PRStream stream = (PRStream) PdfReader.GetPdfObject(
            refs.GetAsIndirectObject(key)
          );
          zip.AddEntry(
            filespec.GetAsString(key).ToString(), 
            PdfReader.GetStreamBytes(stream)
          );
        }
      }
    }
// ===========================================================================
	}
}