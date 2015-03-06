using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace iTextSharp.sandbox.tables
{

    [WrapToTest]
    public class SimpleRowColspan
    {
        public static readonly String DEST = "results/tables/simple_rowspan_colspan.pdf";

        public static void Main(String[] args)
        {
            DirectoryInfo dir = new FileInfo(DEST).Directory;
            if (dir != null)
                dir.Create();

            new SimpleRowColspan().CreatePdf(DEST);
        }

        public void CreatePdf(String dest)
        {
            Document document = new Document();
            PdfWriter.GetInstance(document, new FileStream(dest, FileMode.Create));
            document.Open();
            PdfPTable table = new PdfPTable(5);
            table.SetWidths(new int[] {1, 2, 2, 2, 1});
            PdfPCell cell;
            cell = new PdfPCell(new Phrase("S/N"));
            cell.Rowspan = 2;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("Name"));
            cell.Colspan = 3;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("Age"));
            cell.Rowspan = 2;
            table.AddCell(cell);
            table.AddCell("SURNAME");
            table.AddCell("FIRST NAME");
            table.AddCell("MIDDLE NAME");
            table.AddCell("1");
            table.AddCell("James");
            table.AddCell("Fish");
            table.AddCell("Stone");
            table.AddCell("17");
            document.Add(table);
            document.Close();
        }
    }
}
