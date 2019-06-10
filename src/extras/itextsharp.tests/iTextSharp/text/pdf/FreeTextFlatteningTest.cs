/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General Public License version 3 as published by the Free Software Foundation with the addition of the following permission added to Section 15 as permitted in Section 7(a): FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY iText Group NV, iText Group NV DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License along with this program; if not, see http://www.gnu.org/licenses or write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA, 02110-1301 USA, or download the license from the following URL:

http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions of this program must display Appropriate Legal Notices, as required under Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License, a covered work must retain the producer line in every PDF that is created or manipulated using iText.

You can be released from the requirements of the license by purchasing a commercial license. Buying such a license is mandatory as soon as you develop commercial activities involving the iText software without disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP, serving PDFs on the fly in a web application, shipping iText with a closed source product.

For more information, please contact iText Software Corp. at this address: sales@itextpdf.com */
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