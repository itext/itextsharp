using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace iTextSharp.javaone.edition14
{
    /// <summary>
    ///  Example that shows how to watermark a less simple PDF.
    /// </summary>
    public class S04_CorrectedWatermark
    {
        /// <summary>
        /// The original PDF file. 
        /// </summary>
        public static readonly string SRC = "../../resources/pdfs/superman.pdf";

        /// <summary>
        /// The resulting PDF file.
        /// </summary>
        public static readonly string DEST = "results/javaone/edition2014/04_correct_watermark.pdf";

        /// <summary>
        /// Fills out an interactive form.
        /// </summary>
        /// <param name="args">no arguments needed</param>
        public static void Main(string[] args)
        {
            DirectoryInfo dir = new FileInfo(DEST).Directory;
            if (dir != null)
                dir.Create();

            new S04_CorrectedWatermark().ManipulatePdf(SRC, DEST);
        }

        /// <summary>
        /// Fills out and flattens a form with the name, company and country.
        /// </summary>
        /// <param name="src"> the path to the original form </param>
        /// <param name="dest"> the path to the filled out form </param>
        public void ManipulatePdf(String src, String dest)
        {
            PdfReader reader = new PdfReader(src);
            PdfStamper stamper = new PdfStamper(reader, new FileStream(dest, FileMode.Create));
            int n = reader.NumberOfPages;
            Rectangle pagesize;
            for (int i = 1; i <= n; i++)
            {
                PdfContentByte over = stamper.GetOverContent(i);
                pagesize = reader.GetPageSize(i);
                float x = (pagesize.Left + pagesize.Right) / 2;
                float y = (pagesize.Bottom + pagesize.Top) / 2;
                PdfGState gs = new PdfGState();
                gs.FillOpacity = 0.3f;
                over.SaveState();
                over.SetGState(gs);
                over.SetRGBColorFill(200, 200, 0);
                ColumnText.ShowTextAligned(over, Element.ALIGN_CENTER,
                    new Phrase("Watermark", new Font(Font.FontFamily.HELVETICA, 120)),
                    x, y, 45);
                over.RestoreState();
            }
            stamper.Close();
            reader.Close();
        }
    }
}
