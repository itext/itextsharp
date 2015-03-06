using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace iTextSharp.sandbox.fonts
{

    [WrapToTest]
    public class UnembedFont
    {

        public static readonly String SRC = "results/fonts/withSerifFont.pdf";
        public static readonly String DEST = "results/fonts/withoutSerifFont.pdf";

        public static void Main(String[] args)
        {
            UnembedFont app = new UnembedFont();
            app.ManipulatePdf(SRC, DEST);
        }

        /**
         * Creates a PDF with an embedded font.
         */
        public void CreatePdf(String file)
        {
            // step 1
            Document document = new Document();
            // step 2
            Stream os = new FileStream(file, FileMode.Create);
            PdfWriter.GetInstance(document, os);
            // step 3
            document.Open();
            // step 4
            BaseFont bf = BaseFont.CreateFont("../../resources/fonts/PT_Serif-Web-Regular.ttf", BaseFont.WINANSI,
                BaseFont.EMBEDDED);
            Font f = new Font(bf, 12);
            document.Add(new Paragraph("This is a test with Times New Roman.", f));
            // step 5
            document.Close();
        }

        /**
         * Removes the embedded font
         */
        public void ManipulatePdf(String src, String dest)
        {
            CreatePdf(src);
            // we create a reader instance
            PdfReader reader = new PdfReader(src);
            // we loop over all objects
            PdfObject obj;
            for (int i = 1; i < reader.XrefSize; i++)
            {
                obj = reader.GetPdfObject(i);
                // we skip all objects that aren't a dictionary
                if (obj == null || !obj.IsDictionary())
                    continue;
                // we process all dictionaries
                UnembedTTF((PdfDictionary) obj);
            }
            // removing unused objects will remove unused font file streams
            reader.RemoveUnusedObjects();
            // we persist the altered document
            Stream os = new FileStream(dest, FileMode.Create);
            PdfStamper stamper = new PdfStamper(reader, os);
            stamper.Close();
        }

        /**
        * Processes a dictionary.
        * In case of font dictionaries, the dictionary is processed.
        */
        public void UnembedTTF(PdfDictionary dict)
        {
            // we ignore all dictionaries that aren't font dictionaries
            if (!dict.IsFont())
                return;
            // we only remove TTF fonts
            if (dict.GetAsDict(PdfName.FONTFILE2) != null)
            {
                return;
            }
            // check if a subset was used (in which case we remove the prefix)
            PdfName baseFont = dict.GetAsName(PdfName.BASEFONT);
            if (baseFont.GetBytes()[7] == '+')
            {
                baseFont = new PdfName(baseFont.ToString().Substring(8));
                dict.Put(PdfName.BASEFONT, baseFont);
            }
            // we check if there's a font descriptor
            PdfDictionary fontDescriptor = dict.GetAsDict(PdfName.FONTDESCRIPTOR);
            if (fontDescriptor == null)
                return;
            // is there is, we replace the fontname and remove the font file
            fontDescriptor.Put(PdfName.FONTNAME, baseFont);
            fontDescriptor.Remove(PdfName.FONTFILE2);
        }
    }
}
