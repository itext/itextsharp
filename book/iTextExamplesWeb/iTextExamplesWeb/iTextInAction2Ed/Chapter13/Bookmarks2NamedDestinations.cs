/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;
using kuujinbo.iTextInAction2Ed.Chapter07;

namespace kuujinbo.iTextInAction2Ed.Chapter13 {
  public class Bookmarks2NamedDestinations : IWriter {
// ===========================================================================
    /** The resulting PDF file. */
    public const string RESULT1 = "bookmarks.pdf";
    /** The resulting PDF file. */
    public const string RESULT2 = "named_destinations.pdf";
    /** The resulting PDF file. */
    public const string RESULT3 = "named_destinations.xml";
    
    /** The different epochs. */
    public readonly string[] EPOCH = {
      "Forties", "Fifties", "Sixties", "Seventies", "Eighties",
      "Nineties", "Twenty-first Century"
    };
    /** The fonts for the title. */
    public readonly Font[] FONT = {
      new Font(Font.FontFamily.HELVETICA, 24),
      new Font(Font.FontFamily.HELVETICA, 18),
      new Font(Font.FontFamily.HELVETICA, 14),
      new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD)
    };
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        Bookmarks2NamedDestinations example = new Bookmarks2NamedDestinations();
        byte[] pdf = example.CreatePdf();
        zip.AddEntry(RESULT1, pdf);
        pdf = example.ManipulatePdf(pdf);
        zip.AddEntry(RESULT2, pdf);
        zip.AddEntry(RESULT3, new LinkActions().CreateXml(pdf)); 
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
          PdfWriter.GetInstance(document, ms);
          // step 3
          document.Open();
          // step 4
          int epoch = -1;
          int currentYear = 0;
          Paragraph title = null;
          Chapter chapter = null;
          Section section = null;
          bool sortByYear = true;
          foreach (Movie movie in PojoFactory.GetMovies(sortByYear)) {
            // add the chapter if we're in a new epoch
            if (epoch < (movie.Year - 1940) / 10) {
              epoch = (movie.Year - 1940) / 10;
              if (chapter != null) {
                document.Add(chapter);
              }
              title = new Paragraph(EPOCH[epoch], FONT[0]);
              chapter = new Chapter(title, epoch + 1);
              chapter.BookmarkTitle = EPOCH[epoch];
            }
            // switch to a new year
            if (currentYear < movie.Year) {
              currentYear = movie.Year;
              title = new Paragraph(
                String.Format("The year {0}", movie.Year), FONT[1]
              );
              section = chapter.AddSection(title);
              section.BookmarkTitle = movie.Year.ToString();
              section.Indentation = 30;
              section.BookmarkOpen = false;
              section.NumberStyle = Section.NUMBERSTYLE_DOTTED_WITHOUT_FINAL_DOT;
              section.Add(new Paragraph(
                String.Format("Movies from the year {0}:", movie.Year)
              ));
            }
            title = new Paragraph(movie.MovieTitle, FONT[2]);
            section.Add(title);
            section.Add(new Paragraph(
              "Duration: " + movie.Duration.ToString(), FONT[3]
            ));
            section.Add(new Paragraph("Director(s):", FONT[3]));
            section.Add(PojoToElementFactory.GetDirectorList(movie));
            section.Add(new Paragraph("Countries:", FONT[3]));
            section.Add(PojoToElementFactory.GetCountryList(movie));
          }
          document.Add(chapter);
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
      PdfDictionary root = reader.Catalog;
      PdfDictionary outlines = root.GetAsDict(PdfName.OUTLINES);
      if (outlines == null) return null;
      
      PdfArray dests = new PdfArray();
      AddKids(dests, outlines.GetAsDict(PdfName.FIRST));
      if (dests.Size == 0) return null;
      
      PdfIndirectReference pir = reader.AddPdfObject(dests);
      PdfDictionary nametree = new PdfDictionary();
      nametree.Put(PdfName.NAMES, pir);
      PdfDictionary names = new PdfDictionary();
      names.Put(PdfName.DESTS, nametree);
      root.Put(PdfName.NAMES, names);
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------    
    public void AddKids(PdfArray dests, PdfDictionary outline) {
      while (outline != null) {
        dests.Add(outline.GetAsString(PdfName.TITLE));
        dests.Add(outline.GetAsArray(PdfName.DEST));
        AddKids(dests, outline.GetAsDict(PdfName.FIRST));
        outline = outline.GetAsDict(PdfName.NEXT);
      }
    }    
// ===========================================================================
  }
}