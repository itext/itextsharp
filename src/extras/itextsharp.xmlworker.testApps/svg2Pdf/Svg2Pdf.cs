using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml.svg;

namespace svg2Pdf {
    class Svg2Pdf {
        static void Main(string[] args) {
            if (args.Length < 1) {
                Console.WriteLine("Invalid number of arguments.");
                Console.WriteLine("Usage: svg2Pdf.exe [input svg file]");
                return;
            }

            Document doc = new Document();
            String path = Path.GetDirectoryName(Path.GetFullPath(args[0])) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(args[0]) + ".pdf";
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(path, FileMode.Create));

            doc.Open();
            PdfTemplate template = XMLHelperForSVG.GetInstance().ParseToTemplate(writer.DirectContent, new StreamReader(args[0]));
            Image img = Image.GetInstance(template);
            img.Border = Image.BOX;
            img.BorderWidth = 1;
            doc.Add(img);
            doc.Close();
        }
    }
}
