/* 
 * output files only sent if running on localhost!
*/
<%@ WebHandler Language="C#" Class="kuujinbo.iTextInAction2Ed.WebHandler" %>
using System;
using System.Collections.Generic;
using System.Web;

namespace kuujinbo.iTextInAction2Ed {
  public class WebHandler : IHttpHandler {
  // =========================================================================
    public void ProcessRequest (HttpContext context) {
      HttpRequest Request = context.Request;
      if (Request.IsLocal) {
        Chapters c =  new Chapters() {
          ChapterName = Request.QueryString[Chapters.QS_CHAPTER], 
          ExampleName = Request.QueryString[Chapters.QS_CLASS]
        };
        if ( c.IsValidChapterExample && c.HasResult ) {
          c.SendOutput();
        }
      }
      else {
        Chapters c =  new Chapters() {
          ChapterName = Request.QueryString[Chapters.QS_CHAPTER], 
          ExampleName = Request.QueryString[Chapters.QS_CLASS]
        };
        if (c.IsValidChapterExample) {
          if (c.IsPdfResult) {
            context.Response.Redirect(string.Format(
              "/iTextInAction2Ed/results/{0}/{1}.pdf",
              c.ChapterName, c.ExampleName
            ));
          }
          else if (c.IsZipResult) {
            context.Response.Redirect(string.Format(
              "/iTextInAction2Ed/results/{0}/{1}.zip",
              c.ChapterName, c.ExampleName
            ));
          }
        }
      }
    }
    public bool IsReusable {
      get { return false; }
    }
  // =========================================================================
  }
}