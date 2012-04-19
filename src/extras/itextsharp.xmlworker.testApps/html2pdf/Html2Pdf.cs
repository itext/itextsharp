using System;
using System.IO;
using System.Collections.Generic;
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
                Console.WriteLine("Usage: html2Pdf.exe [input html_file/directory] [default css file]");
                return;
            }

            List<FileStream> fileList = new List<FileStream>();
            if (File.Exists(args[0])) {
                fileList.Add(new FileStream(args[0], FileMode.Open));
            } else if (Directory.Exists(args[0])) {
                DirectoryInfo directory = new DirectoryInfo(args[0]);
                foreach(FileInfo fi in directory.GetFileSystemInfos()) {
                    if (fi.Exists && fi.Extension.ToLower().Equals(".html")) {
                        fileList.Add(fi.Open(FileMode.Open));    
                    }    
                }
            }

            if (fileList.Count == 0) {
                Console.WriteLine("Invalid html_file/directory");
                Console.WriteLine("Usage: html2Pdf.exe [input html_file/directory] [default css file]");
                return;    
            }

            foreach (FileStream fileStream in fileList)
            {
                Document doc = new Document(PageSize.LETTER);
                doc.SetMargins(doc.LeftMargin, doc.RightMargin, 30, 30);
                String path = Path.GetDirectoryName(Path.GetFullPath(fileStream.Name)) + Path.DirectorySeparatorChar +
                              Path.GetFileNameWithoutExtension(fileStream.Name) + ".pdf";
                PdfWriter pdfWriter = PdfWriter.GetInstance(doc, new FileStream(path, FileMode.Create));

                doc.Open();
                XMLWorkerHelper.GetInstance()
                    .ParseXHtml(pdfWriter, doc, fileStream,
                                new FileStream(args[1], FileMode.Open), new UnembedFontProvider());
                doc.Close();
            }
            
        }
    }
}
