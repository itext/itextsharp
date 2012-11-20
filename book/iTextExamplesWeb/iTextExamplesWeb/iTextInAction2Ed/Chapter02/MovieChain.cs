/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter02 {
  public class MovieChain : IWriter {
// ===========================================================================
    public void Write(Stream stream) {
      // step 1
      using (Document document = new Document(
        new Rectangle(240, 240), 10, 10, 10, 10
      ))
      {
        // step 2
        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        // step 3
        document.Open();
        // step 4
        // create a long Stringbuffer with movie titles
        StringBuilder sb = new StringBuilder();
        IEnumerable<Movie> movies = PojoFactory.GetMovies(1);
        foreach ( Movie movie in movies  ) {
        // replace spaces with non-breaking spaces
          sb.Append(movie.MovieTitle.Replace(' ', '\u00a0'));
          // use pipe as separator
          sb.Append('|');
        }
        // Create a first chunk
        Chunk chunk1 = new Chunk(sb.ToString());
        // wrap the chunk in a paragraph and add it to the document
        Paragraph paragraph = new Paragraph("A:\u00a0");
        paragraph.Add(chunk1);
        paragraph.Alignment = Element.ALIGN_JUSTIFIED;
        document.Add(paragraph);
        document.Add(Chunk.NEWLINE);
        // define the pipe character as split character
        chunk1.SetSplitCharacter(new PipeSplitCharacter());
        // wrap the chunk in a second paragraph and add it
        paragraph = new Paragraph("B:\u00a0");
        paragraph.Add(chunk1);
        paragraph.Alignment = Element.ALIGN_JUSTIFIED;
        document.Add(paragraph);
        document.Add(Chunk.NEWLINE);

        // create a new StringBuffer with movie titles
        sb = new StringBuilder();
        foreach ( Movie movie in movies ) {
            sb.Append(movie.MovieTitle);
            sb.Append('|');
        }
        // Create a second chunk 
        Chunk chunk2 = new Chunk(sb.ToString());
        // wrap the chunk in a paragraph and add it to the document
        paragraph = new Paragraph("C:\u00a0");
        paragraph.Add(chunk2);
        paragraph.Alignment = Element.ALIGN_JUSTIFIED;
        document.Add(paragraph);
        document.NewPage();
        // define hyphenation for the chunk
        chunk2.SetHyphenation(new HyphenationAuto("en", "US", 2, 2));
        // wrap the second chunk in a second paragraph and add it
        paragraph = new Paragraph("D:\u00a0");
        paragraph.Add(chunk2);
        paragraph.Alignment = Element.ALIGN_JUSTIFIED;
        document.Add(paragraph);
        
        // go to a new page
        document.NewPage();
        // define a new space/char ratio
        writer.SpaceCharRatio = PdfWriter.NO_SPACE_CHAR_RATIO;
        // wrap the second chunk in a third paragraph and add it
        paragraph = new Paragraph("E:\u00a0");
        paragraph.Add(chunk2);
        paragraph.Alignment = Element.ALIGN_JUSTIFIED;
        document.Add(paragraph);
      }
    }
// ===========================================================================
  }
}