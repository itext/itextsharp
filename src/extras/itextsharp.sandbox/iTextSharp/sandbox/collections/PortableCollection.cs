using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.collection;

namespace iTextSharp.sandbox.collections
{
    [WrapToTest]
    public class PortableCollection
    {

        public static readonly String DEST = "results/collections/portable_collection.pdf";
        public static readonly String DATA = "../../resources/data/united_states.csv";
        public static readonly String HELLO = "../../resources/pdfs/hello.pdf";
        public static readonly String IMG = "../../resources/images/berlin2013.jpg";

        public static void Main(String[] args)
        {
            new PortableCollection().CreatePdf(DEST);
        }


        public void CreatePdf(String dest)
        {
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(dest, FileMode.Create));
            document.Open();
            document.Add(new Paragraph("Portable collection"));
            PdfCollection collection = new PdfCollection(PdfCollection.TILE);
            writer.Collection = collection;
            PdfFileSpecification fileSpec = PdfFileSpecification.FileEmbedded(writer, DATA, "united_states.csv", null);
            writer.AddFileAttachment("united_states.csv", fileSpec);
            fileSpec = PdfFileSpecification.FileEmbedded(writer, HELLO, "hello.pdf", null);
            writer.AddFileAttachment("hello.pdf", fileSpec);
            fileSpec = PdfFileSpecification.FileEmbedded(writer, IMG, "berlin2013.jpg", null);
            writer.AddFileAttachment("berlin2013.jpg", fileSpec);
            document.Close();
        }
    }
}
