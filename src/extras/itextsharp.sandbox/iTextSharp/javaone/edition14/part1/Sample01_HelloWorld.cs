using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace iTextSharp.javaone.edition14.part1
{
    public class Sample01_HelloWorld
    {
        public static readonly string DEST = "results/javaone/edition2014/part1/hello.pdf";
        static void Main(string[] args)
        {
            DirectoryInfo dir = new FileInfo(DEST).Directory;
            if (dir != null)
                dir.Create();
            new Sample01_HelloWorld().CreatePdf(DEST);
        }

        public void CreatePdf(string dest)
        {
            // step 1
            Document document = new Document();
            // step 2
            PdfWriter.GetInstance(document, new FileStream(dest, FileMode.Create));
            // step 3
            document.Open();
            // step 4
            document.Add(new Paragraph("Hello World!"));
            // step 5
            document.Close();
        }
    }
}
