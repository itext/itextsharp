using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;

namespace html2pdf {
    class UnembedFontProvider : XMLWorkerFontProvider {
        private String _baseFontEncoding = BaseFont.CP1252;

        public UnembedFontProvider(String fontsPath, Dictionary<String, String> fontSubstitutionMap) : base(fontsPath, fontSubstitutionMap){}

        public UnembedFontProvider(String fontsPath, Dictionary<String, String> fontSubstitutionMap, String fontEncoding) : base(fontsPath, fontSubstitutionMap) {
            if (fontEncoding != null) {
                _baseFontEncoding = fontEncoding;
            }        
        }

        public override Font GetFont(String fontname, String encoding, bool embedded, float size, int style, BaseColor color) {
            String substFontName = null;
            Font font = base.GetFont(fontSubstitutionMap.TryGetValue(fontname, out substFontName) ? substFontName : fontname, _baseFontEncoding, false, size, style, color);
            if (font.BaseFont != null) {
                float ascent = Math.Max(font.BaseFont.GetFontDescriptor(BaseFont.ASCENT, 1000f), font.BaseFont.GetFontDescriptor(BaseFont.BBOXURY, 1000f));
                float descent = Math.Min(font.BaseFont.GetFontDescriptor(BaseFont.DESCENT, 1000f), font.BaseFont.GetFontDescriptor(BaseFont.BBOXLLY, 1000f));
                font.BaseFont.SetFontDescriptor(BaseFont.ASCENT, ascent);
                font.BaseFont.SetFontDescriptor(BaseFont.DESCENT, descent);
            }
            return font;
        }

        public override bool IsRegistered(String fontName) {
            String substFontName;
            return base.IsRegistered(fontSubstitutionMap.TryGetValue(fontName, out substFontName) ? substFontName : fontName);
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
                doc.SetMargins(doc.LeftMargin, doc.RightMargin, 35, 0);
                String path = Path.GetDirectoryName(Path.GetFullPath(fileStream.Name)) + Path.DirectorySeparatorChar +
                              Path.GetFileNameWithoutExtension(fileStream.Name) + ".pdf";
                PdfWriter pdfWriter = PdfWriter.GetInstance(doc, new FileStream(path, FileMode.Create));

                doc.Open();
                Dictionary<String, String> substFonts = new Dictionary<String, String>();
                substFonts["Arial Unicode MS"] = "Helvetica";
                XMLWorkerHelper.GetInstance()
                    .ParseXHtml(pdfWriter, doc, fileStream,
                                new FileStream(args[1], FileMode.Open), new UnembedFontProvider(XMLWorkerFontProvider.DONTLOOKFORFONTS, substFonts));
                doc.Close();
            }
        }
    }
}
