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
using System.Web;
using System.Xml;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using kuujinbo.iTextInAction2Ed.Chapter08;
/* 
 * Java book example uses SAX to parse XML. .NET uses XmlReader class instead of 
 * SAX, so instead of implementing an interface like the book (XmlHandler.java)
 * we instantiate a concrete XmlReader in this class.
 * 
 * differences between Java and C# XML parsing:
 * http://www.xml.com/pub/a/2002/03/06/csharpxml.html
 * http://msdn.microsoft.com/en-us/library/aa478996.aspx
 * http://msdn.microsoft.com/en-us/library/sbw89de7.aspx
*/
namespace kuujinbo.iTextInAction2Ed.Chapter09 {
  public class MovieServlet : IWriter {
// ===========================================================================
    public string XMLDATA;
    protected Document document;
// --------------------------------------------------------------------------- 
    public void Write(Stream stream) {
      using (ZipFile zip = new ZipFile()) {
        XfaMovies xfa = new XfaMovies();
        XMLDATA = xfa.CreateXML();
        zip.AddEntry(Utility.ResultFileName(xfa.ToString() + ".xml"), XMLDATA);        
        using (MemoryStream ms = new MemoryStream()) {
          using (document = new Document()) {
            PdfWriter.GetInstance(document, ms);
            document.Open();
            Parse(document); 
          } 
          zip.AddEntry(Utility.ResultFileName(
            this.ToString() + ".pdf"), ms.ToArray()
          );
        }
        zip.Save(stream);             
      }
    }
// ---------------------------------------------------------------------------   
/*
 * .NET SAX replacement
*/
// ---------------------------------------------------------------------------       
/*
 * This is a <CODE>Stack</CODE> of objects, waiting to be added to the document.
*/
    Stack<ITextElementArray> stack = new Stack<ITextElementArray>();
    /** This is the current chunk to which characters can be added. */
    Chunk CurrentChunk = null;

    String year = null;
    String duration = null;
    String imdb = null;
// ---------------------------------------------------------------------------    
    public void Parse(Document document) {
      using (StringReader sr = new StringReader(XMLDATA)) {        
        using (XmlReader xr = XmlReader.Create(sr)) {
          xr.MoveToContent();
          while (xr.Read()) {
            switch (xr.NodeType) {
              case XmlNodeType.Element:
                StartElement(xr);
                break;
              case XmlNodeType.Text:
                Characters(xr.Value.Trim());
                break;
              case XmlNodeType.EndElement:
                UpdateStack();
                EndElement(xr.Name);
                break;
            }
          }
        }
      }
    }    
// ---------------------------------------------------------------------------        
// replace XmlHandler.characters() from Java example
    public void Characters(string content) {
      if (content.Length > 0) {
        if (CurrentChunk == null) {
          CurrentChunk = new Chunk(content);
        }
        else {
          CurrentChunk.Append(" ");
          CurrentChunk.Append(content);
        }               
      }
    }
// ---------------------------------------------------------------------------        
// replace XmlHandler.startElement() from Java example
    public void StartElement(XmlReader xr) {
      string name =  xr.Name;
      switch (name) {
        case "directors":
        case "countries":
          stack.Push(new List(List.UNORDERED));
          break;
        case "director":
        case "country":
          stack.Push(new ListItem());
          break;  
        case "movie":
          FlushStack();
          Paragraph p = new Paragraph();
          p.Font = new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD);
          stack.Push(p);
          year = xr.GetAttribute("year");
          duration = xr.GetAttribute("duration");
          imdb = xr.GetAttribute("imdb");
          break; 
        case "original":
          string text = xr.Value.Trim();
          stack.Push(new Paragraph("Original title: "));
          break;                                                    
      }       
    }
// ---------------------------------------------------------------------------        
// replace XmlHandler.endElement() from Java example
    public void EndElement(string name) {
      switch (name) {
        case "directors":
          FlushStack();
          Paragraph p = new Paragraph(
            String.Format("Year: {0}; duration: {0}; ", year, duration)
          );
          Anchor link = new Anchor("link to IMDB");
          link.Reference = 
            String.Format("http://www.imdb.com/title/tt{0}/", imdb);
          p.Add(link);
          stack.Push(p);                
          break;
        case "countries":
        case "title":
          FlushStack();
          break;  
        case "original":
        case "movie":
          CurrentChunk = Chunk.NEWLINE;
          UpdateStack();
          break;  
        case "director":
        case "country":
          ListItem listItem = (ListItem) stack.Pop();
          List list = (List) stack.Pop();
          list.Add(listItem);
          stack.Push(list);
          break;                                                     
      }
    }
// ---------------------------------------------------------------------------        
    /**
     * replace XmlHandler.updateStack() from Java example
     * If the CurrentChunk is not null, it is forwarded to the stack and made
     * null.
     */
    private void UpdateStack() {
      if (CurrentChunk != null) {
        ITextElementArray current;
        if (stack.Count > 0) {
          current = stack.Pop();
          Paragraph p = current as Paragraph;
          if (p == null || !p.IsEmpty()) {
            current.Add(new Chunk(" "));
          }
        } 
        else {
          current = new Paragraph();
        }        
        current.Add(CurrentChunk);
        stack.Push(current);
        CurrentChunk = null;
      }
    }
// ---------------------------------------------------------------------------    
    /**
     * replace XmlHandler.flushStack() from Java example
     * Flushes the stack, adding al objects in it to the document.
     */
    private void FlushStack() {
      while (stack.Count > 0) {
        IElement element = (IElement) stack.Pop();
        if (stack.Count > 0) {
          ITextElementArray previous = (ITextElementArray) stack.Pop();
          previous.Add(element);
          stack.Push(previous);        
        }
        else {
          document.Add(element);
        }
      }
    }
// ===========================================================================
  }
}