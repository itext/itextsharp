using System;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;

namespace html2pdf {
    class Html2Pdf {
        static void Main(string[] args) {
            if (args.Length < 2) {
                Console.WriteLine("Invalid number of arguments.");
                Console.WriteLine("Usage: html2Pdf.exe [input html file] [default css file]");
                return;
            }

            Document doc = new Document(PageSize.LETTER);
            String path = Path.GetDirectoryName(Path.GetFullPath(args[0])) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(args[0]) + ".pdf";
            PdfWriter instance = PdfWriter.GetInstance(doc, new FileStream(path, FileMode.Create));

            doc.Open();
            XMLWorkerHelper.GetInstance()
                .ParseXHtml(instance, doc, new FileStream(args[0], FileMode.Open), new FileStream(args[1], FileMode.Open), null);
            doc.Close();
        }
    }
}
