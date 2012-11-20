/*
 * This class is part of the book "iText in Action - 2nd Edition"
 * written by Bruno Lowagie (ISBN: 9781935182610)
 * For more info, go to: http://itextpdf.com/examples/
 * This example only works with the AGPL version of iText.
 */
using System;
using System.IO;
using Ionic.Zip;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace kuujinbo.iTextInAction2Ed.Chapter08 {
  public class XfaMovie : IWriter {
// ===========================================================================
    public readonly string RESOURCE = Path.Combine(
      Utility.ResourcePdf, "xfa_movie.pdf"
    );
    public readonly string RESOURCEXFA = Path.Combine(
      Utility.ResourceXml, "xfa.xml"
    );
    public const string RESULTTXT1 = "movie_xfa.txt";
    public const string RESULTTXT2 = "movie_acroform.txt";
    public const string RESULTXML = "movie_xfa.xml";
    public const string RESULTXMLFILLED = "movie_filled.xml";
    public const string RESULTDATA = "movie.xml";
    public const string RESULT1 = "xfa_filled_1.pdf";
    public const string RESULT2 = "xfa_filled_2.pdf";
    public const string RESULT3 = "xfa_filled_3.pdf";
// ---------------------------------------------------------------------------
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        XfaMovie xfa = new XfaMovie();
        zip.AddEntry(RESULTTXT1, xfa.ReadFieldnames(new PdfReader(RESOURCE)));
        zip.AddEntry(RESULTXML, xfa.ReadXfa(new PdfReader(RESOURCE)));
        byte[] pdf = xfa.FillData1(RESOURCE);
        zip.AddEntry(RESULT1, pdf);
        zip.AddEntry(RESULTXMLFILLED, xfa.ReadXfa(new PdfReader(pdf)));
        pdf = xfa.FillData2(RESOURCE, RESOURCEXFA);
        zip.AddEntry(RESULT2, pdf);
        zip.AddEntry(RESULTDATA, xfa.ReadData(pdf));
        pdf = xfa.FillData3(RESOURCE);
        zip.AddEntry(RESULT3, pdf);
        zip.AddEntry(RESULTTXT2, xfa.ReadFieldnames(new PdfReader(pdf)));
        
        zip.AddFile(RESOURCE, "");
        zip.AddFile(RESOURCEXFA, "");
        zip.Save(stream);             
      }    
    }
// ---------------------------------------------------------------------------
    /**
     * Checks if a PDF containing an interactive form uses
     * AcroForm technology, XFA technology, or both.
     * Also lists the field names.
     * @param reader the original PDF
     */
    public string ReadFieldnames(PdfReader reader) {
      AcroFields form = reader.AcroFields;
      XfaForm xfa = form.Xfa;
      StringBuilder sb = new StringBuilder();
      sb.Append(xfa.XfaPresent ? "XFA form" : "AcroForm");
      sb.Append(Environment.NewLine);
      foreach (string key in form.Fields.Keys) {
        sb.Append(key);
        sb.Append(Environment.NewLine);
      }
      return sb.ToString();
    }    
// ---------------------------------------------------------------------------
    /**
     * Reads the XML that makes up an XFA form.
     * @param reader the original PDF file
     * @param dest the resulting XML file
     */
    public string ReadXfa(PdfReader reader) {
      XfaForm xfa = new XfaForm(reader);
      XmlDocument doc = xfa.DomDocument;
      reader.Close();
      
      // remove the namespace for pretty-formatted XML;
      // .NET DOM API doesn't support modifying namespace
      // it looks so easy in Java...
      if (!string.IsNullOrEmpty(doc.DocumentElement.NamespaceURI)) {
        doc.DocumentElement.SetAttribute("xmlns", "");
        // so we need to create a new XmlDocument...
        XmlDocument new_doc = new XmlDocument();
        new_doc.LoadXml(doc.OuterXml);
        doc = new_doc;
      }

      var sb = new StringBuilder(4000);
      var Xsettings = new XmlWriterSettings() {Indent = true};
      using (var writer = XmlWriter.Create(sb, Xsettings)) {
        doc.WriteTo(writer);
      }
      return sb.ToString();    
    }
// ---------------------------------------------------------------------------
    /**
     * Fill out a form the "traditional way".
     * Note that not all fields are correctly filled in because of the way the form was created.
     * @param src the original PDF
     */
    public byte[] FillData1(String src) {
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          AcroFields form = stamper.AcroFields;
          form.SetField("movies[0].movie[0].imdb[0]", "1075110");
          form.SetField("movies[0].movie[0].duration[0]", "108");
          form.SetField("movies[0].movie[0].title[0]", "The Misfortunates");
          form.SetField("movies[0].movie[0].original[0]", "De helaasheid der dingen");
          form.SetField("movies[0].movie[0].year[0]", "2009");
        }
        return ms.ToArray();
      }
    }   
// --------------------------------------------------------------------------- 
    /**
     * Fills out a form by replacing the XFA stream.
     * @param src the original PDF
     * @param xml the XML making up the new form
     */   
    public byte[] FillData2(String src, String xml) {
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          XfaForm xfa = new XfaForm(reader);
          XmlDocument doc = new XmlDocument();
          doc.Load(xml);
          xfa.DomDocument = doc;
          xfa.Changed = true;
          XfaForm.SetXfa(xfa, stamper.Reader, stamper.Writer);
        }
        return ms.ToArray();
      }
    }
// ---------------------------------------------------------------------------
    /**
     * Reads the data from a PDF containing an XFA form.
     * @param src the original PDF
     */
    public string ReadData(byte[] src) {
      PdfReader reader = new PdfReader(src);
      XfaForm xfa = new XfaForm(reader);
      XmlNode node = xfa.DatasetsNode;
      XmlNodeList list = node.ChildNodes;
      for (int i = 0; i < list.Count; i++) {
        if ( "data".Equals(list.Item(i).LocalName) ) {
          node = list.Item(i);
          break;        
        }
      }
      list = node.ChildNodes;
      for (int i = 0; i < list.Count; i++) {
        if("movies".Equals(list.Item(i).LocalName)) {
          node = list.Item(i);
          break;
        }
      }
      reader.Close();
      
      var sb = new StringBuilder(4000);
      var Xsettings = new XmlWriterSettings() {Indent = true};
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(node.OuterXml);      
      using (var writer = XmlWriter.Create(sb, Xsettings)) {
        doc.WriteTo(writer);
      }
      return sb.ToString();
    }
// ---------------------------------------------------------------------------
    /**
     * Fills out a PDF form, removing the XFA.
     * @param src
     */  
    public byte[] FillData3(String src) {
      PdfReader reader = new PdfReader(src);
      using (MemoryStream ms = new MemoryStream()) {
        using (PdfStamper stamper = new PdfStamper(reader, ms)) {
          AcroFields form = stamper.AcroFields;
          form.RemoveXfa();
          form.SetField("movies[0].movie[0].imdb[0]", "1075110");
          form.SetField("movies[0].movie[0].duration[0]", "108");
          form.SetField("movies[0].movie[0].title[0]", "The Misfortunates");
          form.SetField("movies[0].movie[0].original[0]", "De helaasheid der dingen");
          form.SetField("movies[0].movie[0].year[0]", "2009");
        }
        return ms.ToArray();
      }
    }    
// ===========================================================================
  }
}