using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace iTextSharp.sandbox.images
{
    [WrapToTest]
    public class RepeatImage
    {
        public static readonly string SRC = "../../resources/pdfs/chinese.pdf";
        public static readonly string DEST = "results/images/chinese.pdf";

        public static void Main(string[] args)
        {
            DirectoryInfo dir = new FileInfo(DEST).Directory;
            if (dir != null)
                dir.Create();
            new RepeatImage().ManipulatePdf(SRC, DEST);
        }

        public void ManipulatePdf(string src, string dest)
        {
            PdfReader reader = new PdfReader(src);
            // We assume that there's a single large picture on the first page
            PdfDictionary page = reader.GetPageN(1);
            PdfDictionary resources = page.GetAsDict(PdfName.RESOURCES);
            PdfDictionary xobjects = resources.GetAsDict(PdfName.XOBJECT);
            Dictionary<PdfName, PdfObject>.KeyCollection.Enumerator enumerator = xobjects.Keys.GetEnumerator();
            enumerator.MoveNext();
            PdfName imgName = enumerator.Current;
            Image img = Image.GetInstance((PRIndirectReference) xobjects.GetAsIndirectObject(imgName));
            img.SetAbsolutePosition(0, 0);
            img.ScaleAbsolute(reader.GetPageSize(1));
            PdfStamper stamper = new PdfStamper(reader, new FileStream(dest,FileMode.Create));
            stamper.GetOverContent(1).AddImage(img);
            stamper.Close();
            reader.Close();
        }
    }
}
