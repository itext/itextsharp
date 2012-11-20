/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter08 {
  public class MovieAds : IWriter {
// ===========================================================================
    public const String RESULT = "festival.pdf";
    public readonly string TEMPLATE = "template.pdf";
    public readonly string RESOURCE = Path.Combine(
      Utility.ResourcePdf, "movie_overview.pdf"
    );
    public readonly string IMAGE = Utility.ResourcePosters;
    public const string POSTER = "poster";
    public const string TEXT = "text";
    public const string YEAR = "year";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        MovieAds movieAds = new MovieAds();
        byte[] pdf = movieAds.CreateTemplate();
        zip.AddEntry(TEMPLATE, pdf);       
        
        using (MemoryStream msDoc = new MemoryStream()) {
          using (Document document = new Document()) {
            using (PdfSmartCopy copy = new PdfSmartCopy(document, msDoc)) {
              document.Open();
              PdfReader reader;
              PdfStamper stamper = null;
              AcroFields form = null;
              int count = 0;
              MemoryStream ms = null;
              using (ms) {
                foreach (Movie movie in PojoFactory.GetMovies()) {
                  if (count == 0) {
                    ms = new MemoryStream();
                    reader = new PdfReader(RESOURCE);
                    stamper = new PdfStamper(reader, ms);
                    stamper.FormFlattening = true;
                    form = stamper.AcroFields;
                  }
                  count++;
                  PdfReader ad = new PdfReader(
                    movieAds.FillTemplate(pdf, movie)
                  );
                  PdfImportedPage page = stamper.GetImportedPage(ad, 1);
                  PushbuttonField bt = form.GetNewPushbuttonFromField(
                    "movie_" + count
                  );
                  bt.Layout = PushbuttonField.LAYOUT_ICON_ONLY;
                  bt.ProportionalIcon = true;
                  bt.Template = page;
                  form.ReplacePushbuttonField("movie_" + count, bt.Field);
                  if (count == 16) {
                    stamper.Close();
                    reader = new PdfReader(ms.ToArray());
                    copy.AddPage(copy.GetImportedPage(reader, 1));
                    count = 0;
                  }
                }
                if (count > 0) {
                  stamper.Close();
                  reader = new PdfReader(ms.ToArray());
                  copy.AddPage(copy.GetImportedPage(reader, 1));
                }
              }
            }
          }
          zip.AddEntry(RESULT, msDoc.ToArray());
        }

        zip.AddFile(RESOURCE, "");
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------
    public byte[] FillTemplate(byte[] pdf, Movie movie) {
      using (MemoryStream ms = new MemoryStream()) {
        PdfReader reader = new PdfReader(pdf);
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          AcroFields form = stamper.AcroFields;
          BaseColor color = WebColors.GetRGBColor(
            "#" + movie.entry.category.color
          );
          PushbuttonField bt = form.GetNewPushbuttonFromField(POSTER);
          bt.Layout = PushbuttonField.LAYOUT_ICON_ONLY;
          bt.ProportionalIcon = true;
          bt.Image = Image.GetInstance(Path.Combine(IMAGE, movie.Imdb + ".jpg"));
          bt.BackgroundColor = color;
          form.ReplacePushbuttonField(POSTER, bt.Field);
          
          PdfContentByte canvas = stamper.GetOverContent(1);
          float size = 12;
          AcroFields.FieldPosition f = form.GetFieldPositions(TEXT)[0];
          while (AddParagraph(CreateMovieParagraph(movie, size),
              canvas, f, true) && size > 6) 
          {
              size -= 0.2f;
          }
          AddParagraph(CreateMovieParagraph(movie, size), canvas, f, false);
          
          form.SetField(YEAR, movie.Year.ToString());
          form.SetFieldProperty(YEAR, "bgcolor", color, null);
          form.SetField(YEAR, movie.Year.ToString());
          stamper.FormFlattening = true;
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------
    public bool AddParagraph(Paragraph p, PdfContentByte canvas, 
        AcroFields.FieldPosition f, bool simulate) 
    {
      ColumnText ct = new ColumnText(canvas);
      ct.SetSimpleColumn(
        f.position.Left, f.position.GetBottom(2),
        f.position.GetRight(2), f.position.Top
      );
      ct.AddElement(p);
      return ColumnText.HasMoreText(ct.Go(simulate));
    }
// ---------------------------------------------------------------------------
    public Paragraph CreateMovieParagraph(Movie movie, float fontsize) {
      Font normal = new Font(Font.FontFamily.HELVETICA, fontsize);
      Font bold = new Font(Font.FontFamily.HELVETICA, fontsize, Font.BOLD);
      Font italic = new Font(Font.FontFamily.HELVETICA, fontsize, Font.ITALIC);
      Paragraph p = new Paragraph(fontsize * 1.2f);
      p.Font = normal;
      p.Alignment = Element.ALIGN_JUSTIFIED;
      p.Add(new Chunk(movie.MovieTitle, bold));
      if (!string.IsNullOrEmpty(movie.OriginalTitle)) {
        p.Add(" ");
        p.Add(new Chunk(movie.OriginalTitle, italic));
      }
      p.Add(new Chunk(string.Format(
        "; run length: {0}", movie.Duration), normal
      ));
      p.Add(new Chunk("; directed by:", normal));
      foreach (Director director in movie.Directors) {
        p.Add(" ");
        p.Add(director.GivenName);
        p.Add(", ");
        p.Add(director.Name);
      }
      return p;
    }
// ---------------------------------------------------------------------------
    public byte[] CreateTemplate() {
      using (MemoryStream ms = new MemoryStream()) {
        using (Document document = new Document(new Rectangle(
          Utilities.MillimetersToPoints(35), Utilities.MillimetersToPoints(50)
        )))
        {
          PdfWriter writer = PdfWriter.GetInstance(document, ms);
          writer.ViewerPreferences = PdfWriter.PageLayoutSinglePage;
          document.Open();
          PushbuttonField poster = new PushbuttonField(
            writer,
            new Rectangle(
              Utilities.MillimetersToPoints(0), 
              Utilities.MillimetersToPoints(25),
              Utilities.MillimetersToPoints(35), 
              Utilities.MillimetersToPoints(50)
            ),
            POSTER
          );
          poster.BackgroundColor = new GrayColor(0.4f);
          writer.AddAnnotation(poster.Field);
          TextField movie = new TextField(
            writer, 
            new Rectangle(
              Utilities.MillimetersToPoints(0), 
              Utilities.MillimetersToPoints(7),
              Utilities.MillimetersToPoints(35), 
              Utilities.MillimetersToPoints(25)
            ),
            TEXT
          );
          movie.Options = TextField.MULTILINE;
          writer.AddAnnotation(movie.GetTextField());
          TextField screening = new TextField(
            writer, 
            new Rectangle(
              Utilities.MillimetersToPoints(0), 
              Utilities.MillimetersToPoints(0),
              Utilities.MillimetersToPoints(35), 
              Utilities.MillimetersToPoints(7)
            ),
            YEAR
          );
          screening.Alignment = Element.ALIGN_CENTER;
          screening.BackgroundColor = new GrayColor(0.4f);
          screening.TextColor = GrayColor.GRAYWHITE;
          writer.AddAnnotation(screening.GetTextField());
        }
        return ms.ToArray();
      }
    }
// ===========================================================================
  }
}