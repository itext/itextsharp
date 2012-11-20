/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter16 {
  public class KubrickDvds : IWriter {
// ===========================================================================
    /** The filename of the resulting PDF. */
    public const String RESULT = "kubrick_dvds.pdf";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        KubrickDvds kubrick = new KubrickDvds();
        byte[] ePdf = kubrick.CreatePdf();
        zip.AddEntry(RESULT, ePdf);
        kubrick.ExtractAttachments(ePdf, zip);
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * Extracts attachments from an existing PDF.
     * @param src the path to the existing PDF
     * @param zip the ZipFile object to add the extracted images
     */
    public void ExtractAttachments(byte[] src, ZipFile zip) {
      PdfReader reader = new PdfReader(src);
      for (int i = 1; i <= reader.NumberOfPages; i++) {
        PdfArray array = reader.GetPageN(i).GetAsArray(PdfName.ANNOTS);
        if (array == null) continue;
        for (int j = 0; j < array.Size; j++) {
          PdfDictionary annot = array.GetAsDict(j);
          if (PdfName.FILEATTACHMENT.Equals(
              annot.GetAsName(PdfName.SUBTYPE)))
          {
            PdfDictionary fs = annot.GetAsDict(PdfName.FS);
            PdfDictionary refs = fs.GetAsDict(PdfName.EF);
            foreach (PdfName name in refs.Keys) {
              zip.AddEntry(
                fs.GetAsString(name).ToString(), 
                PdfReader.GetStreamBytes((PRStream)refs.GetAsStream(name))
              );
            }
          }
        }
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Creates the PDF.
     * @return the bytes of a PDF file.
     */
    public byte[] CreatePdf() {
      using (MemoryStream ms = new MemoryStream()) {
        using (Document document = new Document()) {
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          document.Open();
          document.Add(new Paragraph(
            "This is a list of Kubrick movies available in DVD stores."
          ));
          IEnumerable<Movie> movies = PojoFactory.GetMovies(1)
            .Concat(PojoFactory.GetMovies(4))
          ;          
          List list = new List();
          string RESOURCE = Utility.ResourcePosters;
          foreach (Movie movie in movies) {
            PdfAnnotation annot = PdfAnnotation.CreateFileAttachment(
              writer, null, 
              movie.GetMovieTitle(false), null,
              Path.Combine(RESOURCE, movie.Imdb + ".jpg"),
              string.Format("{0}.jpg", movie.Imdb)              
            );
            ListItem item = new ListItem(movie.GetMovieTitle(false));
            item.Add("\u00a0\u00a0");
            Chunk chunk = new Chunk("\u00a0\u00a0\u00a0\u00a0");
            chunk.SetAnnotation(annot);
            item.Add(chunk);
            list.Add(item);
          }
          document.Add(list);
        }
        return ms.ToArray();
      }
    }    
// ===========================================================================
	}
}