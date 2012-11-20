using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using kuujinbo.iTextInAction2Ed;

namespace iTextExamplesWeb
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack) {
                DropDownListChapters.DataSource = Chapters.Examples.Keys;
                DropDownListChapters.DataBind();
                ChapterSelected(sender, e);
            }
        }

        protected void ChapterSelected(object sender, EventArgs e) 
        {
            DropDownListExamples.DataSource = Chapters.Examples[DropDownListChapters.Text].Keys;
            DropDownListExamples.DataBind();
        }
        
        protected void Submit(object sender, EventArgs e)
        {
            Chapters c = new Chapters() {
                ChapterName = DropDownListChapters.Text, ExampleName = DropDownListExamples.Text
            };
            if (c.IsPdfResult || c.IsZipResult || c.IsOtherResult) {

                try {
                    c.SendOutput();
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            } else {
                //MessageBox.Show()
            }
        }
    }

}