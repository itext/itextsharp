using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.pipeline.html;

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
            Font font = base.GetFont(fontname != null && fontSubstitutionMap.TryGetValue(fontname, out substFontName) ? substFontName : fontname, _baseFontEncoding, false, size, style, color);
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

    class ImageProvider : AbstractImageProvider {
        String imageRootPath;

        public ImageProvider(String inputHtmlFilePath) {
            imageRootPath = File.Exists(inputHtmlFilePath) ? Path.GetDirectoryName(inputHtmlFilePath) : inputHtmlFilePath + Path.DirectorySeparatorChar;
        }

        public override String GetImageRootPath() {
            return imageRootPath;
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
                CollectHtmlFiles(fileList, args[0]);
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
                CssFilesImpl cssFiles = new CssFilesImpl();
                cssFiles.Add(XMLWorkerHelper.GetCSS(new FileStream(args[1], FileMode.Open)));
                StyleAttrCSSResolver cssResolver = new StyleAttrCSSResolver(cssFiles);
                HtmlPipelineContext hpc = new HtmlPipelineContext(new CssAppliersImpl(new UnembedFontProvider(XMLWorkerFontProvider.DONTLOOKFORFONTS, substFonts)));
                hpc.SetImageProvider(new ImageProvider(args[0]));
                hpc.SetAcceptUnknown(true).AutoBookmark(true).SetTagFactory(Tags.GetHtmlTagProcessorFactory());
                HtmlPipeline htmlPipeline = new HtmlPipeline(hpc, new PdfWriterPipeline(doc, pdfWriter));
                IPipeline pipeline = new CssResolverPipeline(cssResolver, htmlPipeline);
                XMLWorker worker = new XMLWorker(pipeline, true);
                XMLParser xmlParse = new XMLParser(true, worker, null);
		        xmlParse.Parse(fileStream);
                doc.Close();

                String cmpPath = Path.GetDirectoryName(Path.GetFullPath(fileStream.Name)) + Path.DirectorySeparatorChar +
                                 "cmp_" + Path.GetFileNameWithoutExtension(fileStream.Name) + ".pdf";
                if (File.Exists(cmpPath)) {
                    CompareTool ct = new CompareTool(path, cmpPath);
                    String outImage = "<testName>-%03d.png".Replace("<testName>", Path.GetFileNameWithoutExtension(fileStream.Name) );
                    String cmpImage = "cmp_<testName>-%03d.png".Replace("<testName>", Path.GetFileNameWithoutExtension(fileStream.Name) );
                    String diffPath = Path.GetDirectoryName(Path.GetFullPath(fileStream.Name)) +
                                      Path.DirectorySeparatorChar + "diff_" + Path.GetFileNameWithoutExtension(fileStream.Name);
                    String errorMessage = ct.Compare(Path.GetDirectoryName(Path.GetFullPath(fileStream.Name)) + Path.DirectorySeparatorChar + "compare" + Path.DirectorySeparatorChar, diffPath);
                    if (errorMessage != null) {
                        Console.WriteLine(errorMessage);
                    }
                }
            }
        }

        static protected void CollectHtmlFiles(List<FileStream> fileList, String directoryPath) {
            DirectoryInfo directory = new DirectoryInfo(directoryPath);
            try {
                foreach (FileSystemInfo fi in directory.GetFileSystemInfos()) {
                    if (fi is FileInfo && fi.Extension.ToLower().Equals(".html")) {
                        fileList.Add(((FileInfo)fi).Open(FileMode.Open));
                    } else {
                        CollectHtmlFiles(fileList, fi.FullName);
                    }
                }
            } catch {}
        }
    }
}
