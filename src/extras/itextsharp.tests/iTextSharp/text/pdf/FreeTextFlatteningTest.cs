using System;
using System.IO;
using iTextSharp.testutils;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    [TestFixture]
    public class FreeTextFlatteningTest {
        private const String FOLDER = @"..\..\resources\text\pdf\FreeTextFlatteningTest\";
        private const String TARGET = @"FreeTextFlattening\";

        [TestFixtureSetUp]
        public static void setUp() {
            Directory.CreateDirectory(TARGET);
        }
        
        [Test]
        public void FlattenCorrectlyTest() {
            String outputFile = TARGET + "freetext-flattened.pdf";

            FlattenFreeText(FOLDER + "freetext.pdf", outputFile);
            CheckAnnotationSize(outputFile, 0);

            String errorMessage = new CompareTool().CompareByContent(outputFile, FOLDER + "flattened.pdf", TARGET, "diff");
            if ( errorMessage != null ) {
                Assert.Fail(errorMessage);
            }
        }
        
        [Test]
        public void CheckPageContentTest() {
            CheckPageContent(FOLDER + "flattened.pdf");
        }

        [Test]
        public void FlattenWithoutDA() {
            String outputFile = TARGET + "freetext-flattened-no-da.pdf";

            FlattenFreeText(FOLDER + "freetext-no-da.pdf", outputFile);
            CheckAnnotationSize(outputFile, 1);
        }

        [Test]
        public void FlattenAndCheckCourier() {
            String inputFile = FOLDER + "freetext-courier.pdf";
            String outputFile = TARGET + "freetext-courier-flattened.pdf";

            FlattenFreeText(inputFile, outputFile);
            CheckPageContent(outputFile);
        }

        [Test]
        public void FlattenAndCheckShortFontName() {
            String inputFile = FOLDER + "freetext-times-short.pdf";
            String outputFile = TARGET + "freetext-times-short-flattened.pdf";

            FlattenFreeText(inputFile, outputFile);
            CheckPageContent(outputFile);
            
            String errorMessage = new CompareTool().CompareByContent(outputFile, FOLDER + "cmp_freetext-times-short-flattened.pdf", TARGET, "diff_short");
            if ( errorMessage != null ) {
                Assert.Fail(errorMessage);
            }
        }
        
        private void CheckAnnotationSize(String path, int expectedAnnotationsSize) {
            FileStream fin = File.OpenRead(path);
            CheckAnnotationSize(fin, expectedAnnotationsSize);
            fin.Close();
        }

        private void CheckAnnotationSize(Stream inputStream, int expectedAnnotationsSize) {
            PdfReader reader = new PdfReader(inputStream);
            PdfDictionary pageDictionary = reader.GetPageN(1);
            if ( pageDictionary.Contains(PdfName.ANNOTS )) {
                PdfArray annotations = pageDictionary.GetAsArray(PdfName.ANNOTS);
                Assert.True(annotations.Size == expectedAnnotationsSize);
            }
        }

        private void FlattenFreeText(String inputPath, String outputPath) {
            FileStream fin = File.OpenRead(inputPath);
            FileStream fout = File.Create(outputPath);
            FlattenFreeText(fin, fout);
            fin.Close();
            fout.Close();
        }

        private void FlattenFreeText(Stream inputStream, Stream outputStream) {
            PdfReader reader = new PdfReader(inputStream);
            PdfStamper stamper = new PdfStamper(reader, outputStream);

            stamper.FormFlattening = true;
            stamper.FreeTextFlattening = true;
            stamper.AnnotationFlattening = true;

            stamper.Close();
        }

        private void CheckPageContent(String path) {
            PdfReader pdfReader = new PdfReader(path);
            PdfDictionary pageDic = pdfReader.GetPageN(1);

            IRenderListener dummy = new DummyRenderListner();
            PdfContentStreamProcessor processor = new PdfContentStreamProcessor(dummy);

            PdfDictionary resourcesDic = pageDic.GetAsDict(PdfName.RESOURCES);
            processor.ProcessContent(ContentByteUtils.GetContentBytesForPage(pdfReader, 1), resourcesDic);
            pdfReader.Close();
        }
        
        private class DummyRenderListner : IRenderListener {
            public void BeginTextBlock() {
            }

            public void RenderText(TextRenderInfo renderInfo) {
            }

            public void EndTextBlock() {
            }

            public void RenderImage(ImageRenderInfo renderInfo) {
            }
        }
    }
}