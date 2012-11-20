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
using System.Text;
using System.Xml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Intro_1_2;

namespace kuujinbo.iTextInAction2Ed.Chapter08 {
  public class XfaMovies : IWriter {
// ---------------------------------------------------------------------------
    public const String RESULTTXT = "movies_xfa.txt";
    public const String XMLDATA = "movies.xml";
    public const String RESULTXFA = "movies_xfa.xml";
    public const String RESULT = "xfa_filled_in.pdf";
    public readonly string RESOURCE = Path.Combine(
      Utility.ResourcePdf, "xfa_movies.pdf"
    );
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        XfaMovie x = new XfaMovie();
        zip.AddEntry(RESULTTXT, x.ReadFieldnames(new PdfReader(RESOURCE)));
        XfaMovies xfa = new XfaMovies();
        string xml = xfa.CreateXML();
        zip.AddEntry(XMLDATA, xml);
        zip.AddEntry(RESULT, xfa.ManipulatePdf(RESOURCE, xml));
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------
    public string CreateXML() {
      StringBuilder SB = new StringBuilder();
      SB.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>\n");
      SB.Append("<movies>\n");
      foreach (Movie movie in PojoFactory.GetMovies()) {
        SB.Append(GetXml(movie));
      }
      SB.Append("</movies>");
      return SB.ToString();
    }
// ---------------------------------------------------------------------------
    public byte[] ManipulatePdf(String src, String xml) {
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          AcroFields form = stamper.AcroFields;
          XfaForm xfa = form.Xfa;
          xfa.FillXfaForm(XmlReader.Create(new StringReader(xml)));
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------    
    public String GetXml(Movie movie) {
      StringBuilder buf = new StringBuilder();
      buf.Append("<movie duration=\"");
      buf.Append(movie.Duration.ToString());
      buf.Append("\" imdb=\"");
      buf.Append(movie.Imdb);
      buf.Append("\" year=\"");
      buf.Append(movie.Year.ToString());
      buf.Append("\">");
      buf.Append("<title>");
      buf.Append(movie.MovieTitle);
      buf.Append("</title>");
      if (!string.IsNullOrEmpty(movie.OriginalTitle)) {
        buf.Append("<original>");
        buf.Append(movie.OriginalTitle);
        buf.Append("</original>");
      }
      buf.Append("<directors>");
      foreach (Director director in movie.Directors) {
        buf.Append("<director>");
        buf.Append(director.Name);
        buf.Append(", ");
        buf.Append(director.GivenName);
        buf.Append("</director>");
      }
      buf.Append("</directors>");
      buf.Append("<countries>");
      foreach (Country country in movie.Countries) {
        buf.Append("<country>");
        buf.Append(country.Name);
        buf.Append("</country>");
      }
      buf.Append("</countries>");
      buf.Append("</movie>\n");
      return buf.ToString();
    }    
// ===========================================================================
  }
}