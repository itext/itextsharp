using System;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;

namespace html2pdf {
    class UnembedFontProvider : FontFactoryImp {
        private String _baseFontEncoding = BaseFont.CP1252;

        public UnembedFontProvider() : this(null, null) { }
        public UnembedFontProvider(String fontsPath) : this(fontsPath, null) { }
        public UnembedFontProvider(String fontsPath, String fontEncoding) {
            if (fontEncoding != null) {
                _baseFontEncoding = fontEncoding;
            }
            if (string.IsNullOrEmpty(fontsPath)) {
                base.RegisterDirectories();
            } else if (!fontsPath.Equals(XMLWorkerFontProvider.DONTLOOKFORFONTS)) {
                RegisterDirectory(fontsPath, true);
            }
        }

        public override Font GetFont(String fontname, String encoding, bool embedded, float size, int style, BaseColor color) {
            return base.GetFont(fontname, _baseFontEncoding, false, size, style, color);
        }
    }

    class Html2Pdf {
        static void Main(string[] args) {
            if (args.Length < 2) {
                Console.WriteLine("Invalid number of arguments.");
                Console.WriteLine("Usage: html2Pdf.exe [input html file] [default css file]");
                return;
            }

            Document doc = new Document(PageSize.LETTER);
            String path = Path.GetDirectoryName(Path.GetFullPath(args[0])) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(args[0]) + ".pdf";
            PdfWriter pdfWriter = PdfWriter.GetInstance(doc, new FileStream(path, FileMode.Create));

            doc.Open();
            XMLWorkerHelper.GetInstance()
                .ParseXHtml(pdfWriter, doc, new FileStream(args[0], FileMode.Open), new FileStream(args[1], FileMode.Open), new UnembedFontProvider());
            doc.Close();
        }
    }
}
