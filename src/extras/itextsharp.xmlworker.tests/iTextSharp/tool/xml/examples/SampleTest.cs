using System;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.examples {
    internal class SampleTest {
        protected String outPath;
        protected String outPdf;
        protected String inputHtml;
        private String cmpPdf;
        private String differenceImage;
        private CompareTool compareTool;
        protected static String testPath;
        protected static String testName;

        protected static string RESOURCES = @"..\..\resources\com\itextpdf\";

        [SetUp]
        public void SetUp() {
            testPath = this.GetType().FullName.Replace("itextsharp.xmlworker.tests.iTextSharp.", "");
            testPath = testPath.Replace(".", Path.DirectorySeparatorChar.ToString());
            testPath = testPath.Substring(0, testPath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            testName = GetTestName();
            if (testName.Length > 0) {
                if (testName.Contains(Path.DirectorySeparatorChar.ToString())) {
                    int startIndex = testName.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                    testName = testName.Substring(startIndex, testName.Length - startIndex);
                }
                outPath = "target" + Path.DirectorySeparatorChar + testPath +
                          testName + Path.DirectorySeparatorChar;
                String inputPath = RESOURCES + Path.DirectorySeparatorChar + testPath + Path.DirectorySeparatorChar +
                                   testName + Path.DirectorySeparatorChar;
                differenceImage = outPath + "difference";
                outPdf = outPath + testName + ".pdf";
                inputHtml = inputPath + "<testName>.html".Replace("<testName>", testName);
                cmpPdf = inputPath + "<testName>.pdf".Replace("<testName>", testName);
                compareTool = new CompareTool(outPdf, cmpPdf);

                if (Directory.Exists(outPath)) {
                    DeleteDirectory(outPath);
                }
                Directory.CreateDirectory(outPath);
            }
        }

        [Test, Timeout(120000)]
        public void Test() {
            String testName = GetTestName();
            if (this.GetType() != typeof (SampleTest) && (testName.Length > 0)) {
                TransformHtml2Pdf();
                if (DetectCrashesAndHangUpsOnly() == false) {
                    String errorMessage = compareTool.Compare(outPath, differenceImage);
                    if (errorMessage != null) {
                        Assert.Fail(errorMessage);
                    }
                }
            }
        }

        protected virtual String GetTestName() {
            return "";
        }

        protected bool DetectCrashesAndHangUpsOnly() {
            return false;
        }

        protected class SampleTestImageProvider : AbstractImageProvider {
            private String imageRootPath;

            public SampleTestImageProvider() {
                imageRootPath = RESOURCES + Path.DirectorySeparatorChar + testPath + testName +
                                Path.DirectorySeparatorChar;
            }

            public override String GetImageRootPath() {
                return imageRootPath;
            }
        }

        protected virtual void TransformHtml2Pdf() {
            Document doc = new Document(PageSize.A4);
            PdfWriter pdfWriter = PdfWriter.GetInstance(doc, new FileStream(outPdf, FileMode.Create));
            doc.Open();
            TransformHtml2Pdf(doc, pdfWriter, new SampleTestImageProvider(),
                new XMLWorkerFontProvider(RESOURCES + @"\tool\xml\examples\fonts"),
                File.OpenRead(RESOURCES + @"\tool\xml\examples\" + "sampleTest.css"));
            doc.Close();
        }

        protected void TransformHtml2Pdf(Document doc, PdfWriter pdfWriter, IImageProvider imageProvider,
            IFontProvider fontProvider, Stream cssFile) {
            CssFilesImpl cssFiles = new CssFilesImpl();
            if (cssFile == null)
                cssFile =
                    typeof (XMLWorker).Assembly.GetManifestResourceStream("iTextSharp.tool.xml.css.default.css");
            cssFiles.Add(XMLWorkerHelper.GetCSS(cssFile));
            StyleAttrCSSResolver cssResolver = new StyleAttrCSSResolver(cssFiles);
            HtmlPipelineContext hpc;

            if (fontProvider != null)
                hpc = new HtmlPipelineContext(new CssAppliersImpl(fontProvider));
            else
                hpc = new HtmlPipelineContext(null);

            hpc.SetImageProvider(imageProvider);
            hpc.SetAcceptUnknown(true).AutoBookmark(true).SetTagFactory(Tags.GetHtmlTagProcessorFactory());
            HtmlPipeline htmlPipeline = new HtmlPipeline(hpc, new PdfWriterPipeline(doc, pdfWriter));
            IPipeline pipeline = new CssResolverPipeline(cssResolver, htmlPipeline);
            XMLWorker worker = new XMLWorker(pipeline, true);
            XMLParser xmlParse = new XMLParser(true, worker, Encoding.GetEncoding("UTF-8"));
            xmlParse.Parse(File.OpenRead(inputHtml), Encoding.GetEncoding("UTF-8"));
        }

        private void DeleteDirectory(string path) {
            if (path == null)
                return;

            if (Directory.Exists(path)) {
                foreach (string subfolder in Directory.GetDirectories(path))
                    DeleteDirectory(subfolder);
                foreach (string file in Directory.GetFiles(path))
                    File.Delete(file);
                Directory.Delete(path);
            }
        }
    }
}