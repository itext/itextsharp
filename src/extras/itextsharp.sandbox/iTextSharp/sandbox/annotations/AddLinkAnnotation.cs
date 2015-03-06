using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace iTextSharp.sandbox.annotations
{
    [WrapToTest]
    public class AddLinkAnnotation
    {
        public static readonly String SRC = "../../resources/pdfs/primes.pdf";
        public static readonly String DEST = "results/annotations/link_annotation.pdf";

        public static void Main(string[] args)
        {
            DirectoryInfo dir = new FileInfo(DEST).Directory;
            if (dir != null)
                dir.Create();
            new AddLinkAnnotation().ManipulatePdf(SRC, DEST);
        }

        public void ManipulatePdf(String src, String dest)
        {
            PdfReader reader = new PdfReader(src);
            PdfStamper stamper = new PdfStamper(reader, new FileStream(dest, FileMode.Create));
            Rectangle linkLocation = new Rectangle(523, 770, 559, 806);
            PdfDestination destination = new PdfDestination(PdfDestination.FIT);
            PdfAnnotation link = PdfAnnotation.CreateLink(stamper.Writer,
                linkLocation, PdfAnnotation.HIGHLIGHT_INVERT,
                3, destination);
            stamper.AddAnnotation(link, 1);
            stamper.Close();
        }
    }
}
