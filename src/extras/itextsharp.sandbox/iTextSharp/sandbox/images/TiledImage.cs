using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace iTextSharp.sandbox.images
{
    [WrapToTest]
    public class TiledImage
    {
        public static readonly string IMAGE = "../../resources/images/bruno_ingeborg.jpg";
        public static readonly string DEST = "results/images/tiled_image.pdf";

        public static void Main(string[] args)
        {
            DirectoryInfo dir = new FileInfo(DEST).Directory;
            if (dir != null)
                dir.Create();
            new TiledImage().CreatePdf(DEST);
        }

        public void CreatePdf(string dest)
        {
            Image image = Image.GetInstance(IMAGE);
            float width = image.ScaledWidth;
            float height = image.ScaledHeight;
            Rectangle page = new Rectangle(width/2, height/2);
            Document document = new Document(page);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(dest, FileMode.Create));
            document.Open();
            PdfContentByte canvas = writer.DirectContentUnder;
            canvas.AddImage(image, width, 0, 0, height, 0, -height/2);
            document.NewPage();
            canvas.AddImage(image, width, 0, 0, height, 0, 0);
            document.NewPage();
            canvas.AddImage(image, width, 0, 0, height, -width/2, - height/2);
            document.NewPage();
            canvas.AddImage(image, width, 0, 0, height, -width/2, 0);
            document.Close();
        }
    }
}
