/*
 * This class is __NOT__ part of the book "iText in Action - 2nd Edition".
 * it's a helper class to build the examples using VS2008 or higher
 * on your local machine, either in a web context or command line
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;

namespace kuujinbo.iTextInAction2Ed {
  public static class Utility  {
/* ###########################################################################
 * directories for resource/result files; path depends on running context
 * ###########################################################################   
*/
// base directory for running context
    public static readonly string BaseDirectory;
// base resource directory for running context
    public static readonly string ResourceDirectory;
// result files created here; only has meaning when running from command line
    public static readonly string ResultDirectory;
   
// resource files
    public static string ResourceCalendar {
      get { return Path.Combine(ResourceDirectory, "calendar"); }
    }
    public static string ResourceEncryption {
      get { return Path.Combine(ResourceDirectory, "encryption"); }
    }    
    public static string ResourceFonts {
      get { return Path.Combine(ResourceDirectory, "fonts"); }
    }
    public static string ResourceHtml {
      get { return Path.Combine(ResourceDirectory, "html"); }
    }   
    public static string ResourceImage {
      get { return Path.Combine(ResourceDirectory, "img"); }
    }
    public static string ResourceJavaScript {
      get { return Path.Combine(ResourceDirectory, "js"); }
    }
    public static string ResourcePdf {
      get { return Path.Combine(ResourceDirectory, "pdf"); }
    }
    public static string ResourcePosters {
      get { return Path.Combine(ResourceDirectory, "posters"); }
    }
    public static string ResourceSwf {
      get { return Path.Combine(ResourceDirectory, "swf"); }
    }    
    public static string ResourceText {
      get { return Path.Combine(ResourceDirectory, "txt"); }
    }
    public static string ResourceXml {
      get { return Path.Combine(ResourceDirectory, "xml"); }
    }
/* ---------------------------------------------------------------------------
 * static contructor => initialize member(s)
*/
    static Utility() {
      HttpContext hc = HttpContext.Current;
      if (hc != null) {
          BaseDirectory = hc.Server.MapPath("~/iTextInAction2Ed/");
          ResourceDirectory = Path.Combine(BaseDirectory, "resources");
      } else
      {
          BaseDirectory = Path.GetDirectoryName(
              Assembly.GetEntryAssembly().Location
              );
          ResultDirectory = new Uri(
              new Uri(BaseDirectory), "results"
              ).LocalPath;
          ResourceDirectory = new Uri(
              new Uri(BaseDirectory), "iTextInAction2Ed/resources"
              ).LocalPath;
          BaseFont.AddToResourceSearch(Path.Combine(
              BaseDirectory, "iTextAsian.dll"
                                           ));
      }
    }
/* ###########################################################################
 * helpers for both contexts
 * ###########################################################################   
*/
/* ---------------------------------------------------------------------------
 * get **FULL* output file path;
 * output files are prefixed with the example chapter name dot class name:
 * ChapterXX.CLASSNAME
*/
    public static string ResultFileName(string fileName) {
      return ResultFileName(fileName, true);
    }
    public static string ResultFileName(string fileName, bool isWebRequest) {
      fileName = Regex.Replace(
        fileName,
        // remove common namespace string
        typeof(Utility).Namespace,
        ""
      ) // remove leading dot
      .Substring(1);
      return isWebRequest
        ? fileName
        : Path.Combine(ResultDirectory, fileName)
      ;
    }
/* ---------------------------------------------------------------------------
 * get PDF in-memory bytes
 */
    public static byte[] PdfBytes(IWriter w) {
      using (MemoryStream ms = new MemoryStream()) {
        w.Write(ms);
        return ms.ToArray();
      }    
    }     
/* ###########################################################################
 * run under web context
 * ###########################################################################   
*/    
// check if HTTP "POST" method
    public static bool IsHttpPost() {
      return HttpContext.Current.Request.HttpMethod == "POST";
    }    
/* ---------------------------------------------------------------------------
 * get base URL of current server
 */
    public static string GetServerBaseUrl() {
      return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
    }
/* ---------------------------------------------------------------------------
 * send HTTP headers so client recognizes response as a PDF
 */
    public static void SendPdfHeaders(HttpResponse Response, string name) {
      Response.ContentType = "application/pdf";
      Response.AddHeader(
        "content-disposition", 
        string.Format("attachment; filename={0}", name)
      );   
    }
/* ---------------------------------------------------------------------------
 * send HTTP headers so client recognizes response as a PDF
 */
    public static void SendZipHeaders(HttpResponse Response, string name) {
      Response.ContentType = "application/zip";
      Response.AddHeader(
        "content-disposition", 
        string.Format("attachment; filename={0}", name)
      );   
    }    
/* ###########################################################################
 * miscellaneous helpers
 * ###########################################################################   
*/
// get milliseconds for stringified time => 'hh:mm:ss'
    public static long GetMilliseconds(string hhmmss) {
      return (long) (
        DateTime.Parse("1970-01-01 " + hhmmss) 
        //DateTime.Parse("1970-01-01 09:30:00")
        - DateTime.Parse("1970-01-01")
      ).TotalMilliseconds;
    }    
// ===========================================================================  
  }
}