using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.javaone.edition14.part4.helper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace iTextSharp.javaone.edition14
{
    /// <summary>
    /// In this first example that parses a PDF to discover its structure,
    /// we'll highlight all the different text items and images that are
    /// encountered during the parsing process.
    /// </summary>
    public class S08_FindItems
    {
        /// <summary>
        /// The source file that is going to be parsed.
        /// </summary>
        public static readonly String SRC = "../../resources/pdfs/page229.pdf";
    
        /// <summary>
        /// The resulting PDF after parsing for structure.
        /// </summary>
        public static readonly String DEST = "results/javaone/edition2014/08_page229_items.pdf";

        /// <summary>
        /// Reads the first page of a document of which the top margin is 48pt heigh
        /// and highlights text items and images.
        /// </summary>
        /// <param name="args">No arguments needed</param>
        public static void Main(String[] args)
        {
            DirectoryInfo dir = new FileInfo(DEST).Directory;
            if (dir != null)
                dir.Create();

            S08_FindItems app = new S08_FindItems();
            PdfReader reader = new PdfReader(SRC);
            List<MyItem> items = app.GetContentItems(reader, 1, 48);
            app.Highlight(items, reader, 1, DEST);
        }

        /// <summary>
        /// Parses a page of a PDF file resulting in a list of
        /// TextItem and ImageItem objects.
        /// </summary>
        /// <param name="reader">a PdfReader</param>
        /// <param name="page">the page number of the page that needs to be parsed</param>
        /// <param name="header_height">header_height the height of the top margin</param>
        /// <returns>a list of TextItem and ImageItem objects</returns>
        public List<MyItem> GetContentItems(PdfReader reader, int page, float header_height)
        {
            PdfReaderContentParser parser = new PdfReaderContentParser(reader);
            Rectangle pageSize = reader.GetPageSize(page);
            MyRenderListener myRenderListener = new MyRenderListener(pageSize.Top - header_height);
            parser.ProcessContent(page, myRenderListener);
            return myRenderListener.Items;
        }

        /// <summary>
        /// Accepts a list of MyItem objects and draws a colored rectangle for each
        /// item in the list.
        /// </summary>
        /// <param name="items">The list of items</param>
        /// <param name="reader">The reader instance that has access to the PDF file</param>
        /// <param name="pageNum">The page number of the page that needs to be parsed</param>
        /// <param name="destination">The path for the altered PDF file</param>
        public void Highlight(List<MyItem> items, PdfReader reader, int pageNum, String destination)
        {
            PdfStamper stamper = new PdfStamper(reader, new FileStream(destination, FileMode.Create));
            PdfContentByte over = stamper.GetOverContent(pageNum);
            foreach (MyItem item in items)
            {
                if (item.Color == null)
                    continue;
                over.SaveState();
                over.SetColorStroke(item.Color);
                over.SetLineWidth(2);
                Rectangle r = item.Rectangle;
                over.Rectangle(r.Left, r.Bottom, r.Width, r.Height);
                over.Stroke();
                over.RestoreState();
            }
            stamper.Close();
        }
    }
}
