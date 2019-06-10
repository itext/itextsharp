/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using itextsharp.tests.iTextSharp.testutils;
using itextsharp.tests.iTextSharp.text.error_messages;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class PdfCopyTest {
        private const string RESOURCES = @"..\..\resources\text\pdf\PdfCopyTest\";

        [SetUp]
        virtual public void SetUp() {
            TestResourceUtils.PurgeTempFiles();
        }

        [TearDown]
        virtual public void TearDown() {
            TestResourceUtils.PurgeTempFiles();
        }
        
        /**
         * Test to demonstrate issue https://sourceforge.net/tracker/?func=detail&aid=3013642&group_id=15255&atid=115255
         */
        [Test]
#if DRAWING
        [Ignore("ignore")]
#endif// !NO_DRAWING
        virtual public void TestExtraXObjects()
        {
#if DRAWING
            PdfReader sourceR = new PdfReader(CreateImagePdf());
            try
            {
                int sourceXRefCount = sourceR.XrefSize;

                Document document = new Document();
                MemoryStream outStream = new MemoryStream();
                PdfCopy copy = new PdfCopy(document, outStream);
                document.Open();
                PdfImportedPage importedPage = copy.GetImportedPage(sourceR, 1);
                copy.AddPage(importedPage);
                document.Close();

                PdfReader targetR = new PdfReader(outStream.ToArray());
                int destinationXRefCount = targetR.XrefSize;

                //        TestResourceUtils.saveBytesToFile(createImagePdf(), new File("./source.pdf"));
                //        TestResourceUtils.saveBytesToFile(out.toByteArray(), new File("./result.pdf"));

                Assert.AreEqual(sourceXRefCount, destinationXRefCount);
            }
            finally
            {
                sourceR.Close();
            }
#endif// DRAWING
        }

#if DRAWING
        private static byte[] CreateImagePdf()
        {
            MemoryStream byteStream = new MemoryStream();

            Document document = new Document();
            document.SetPageSize(PageSize.LETTER);

            document.Open();

            System.Drawing.Bitmap awtImg = new System.Drawing.Bitmap(100, 100, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(awtImg);
            g.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.Green), 10, 10, 80, 80);
            g.Save();
            Image itextImg = Image.GetInstance(awtImg, (BaseColor)null);
            document.Add(itextImg);

            document.Close();

            byte[] pdfBytes = byteStream.ToArray();

            return pdfBytes;
        }
#endif// DRAWING




        [Test]
        /**
         * Test to make sure that the following issue is fixed: http://sourceforge.net/mailarchive/message.php?msg_id=30891213
         */
        virtual public void TestDecodeParmsArrayWithNullItems() {
            Document document = new Document();
            MemoryStream byteStream = new MemoryStream();
            PdfSmartCopy pdfSmartCopy = new PdfSmartCopy(document, byteStream);
            document.Open();

            PdfReader reader = TestResourceUtils.GetResourceAsPdfReader(RESOURCES, "imgWithDecodeParms.pdf");
            pdfSmartCopy.AddPage(pdfSmartCopy.GetImportedPage(reader, 1));

            document.Close();
            reader.Close();

            reader = new PdfReader(byteStream.ToArray());
            PdfDictionary page = reader.GetPageN(1);
            PdfDictionary resources = page.GetAsDict(PdfName.RESOURCES);
            PdfDictionary xObject = resources.GetAsDict(PdfName.XOBJECT);
            PdfStream img = xObject.GetAsStream(new PdfName("Im0"));
            PdfArray decodeParms = img.GetAsArray(PdfName.DECODEPARMS);
            Assert.AreEqual(2, decodeParms.Size);
            Assert.IsTrue(decodeParms[0] is PdfNull);

            reader.Close();
        }

        [Test]
        public virtual void TestNeedAppearances() {
            String f1 = RESOURCES + "appearances1.pdf";
            String f2 = RESOURCES + "appearances2.pdf";
            String f3 = RESOURCES + "appearances3.pdf";
            String f4 = RESOURCES + "appearances4.pdf";

            Directory.CreateDirectory("PdfCopyTest/");
            FileStream outputPdfStream = new FileStream("PdfCopyTest/appearances.pdf", FileMode.Create);
            Document document = new Document();
            PdfCopy copy = new PdfCopy(document, outputPdfStream);
            copy.SetMergeFields();
            document.Open();
            foreach (String f in new String[] {f1, f2, f3, f4}) {
                PdfReader r = new PdfReader(f);
                copy.AddDocument(r);
            }
            copy.Close();
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent("PdfCopyTest/appearances.pdf", RESOURCES + "cmp_appearances.pdf", "PdfCopyTest/", "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public virtual void TestNeedAppearancesFalse() {
            String f1 = RESOURCES + "appearances1(needAppearancesFalse).pdf";
            String f2 = RESOURCES + "appearances2(needAppearancesFalse).pdf";
            String f3 = RESOURCES + "appearances3(needAppearancesFalse).pdf";
            String f4 = RESOURCES + "appearances4(needAppearancesFalse).pdf";

            Directory.CreateDirectory("PdfCopyTest/");
            FileStream outputPdfStream =
                new FileStream("PdfCopyTest/appearances(needAppearancesFalse).pdf", FileMode.Create);
            Document document = new Document();
            PdfCopy copy = new PdfCopy(document, outputPdfStream);
            copy.SetMergeFields();
            document.Open();
            foreach (String f in new String[] {f1, f2, f3, f4}) {
                PdfReader r = new PdfReader(f);
                copy.AddDocument(r);
            }
            copy.Close();
            CompareTool compareTool =
                new CompareTool();
            String errorMessage = compareTool.CompareByContent("PdfCopyTest/appearances(needAppearancesFalse).pdf", RESOURCES + "cmp_appearances(needAppearancesFalse).pdf", "PdfCopyTest/", "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public virtual void TestNeedAppearancesFalseWithStreams() {
            String f1 = RESOURCES + "appearances1(needAppearancesFalseWithStreams).pdf";
            String f2 = RESOURCES + "appearances2(needAppearancesFalseWithStreams).pdf";
            String f3 = RESOURCES + "appearances3(needAppearancesFalseWithStreams).pdf";
            String f4 = RESOURCES + "appearances4(needAppearancesFalseWithStreams).pdf";

            Directory.CreateDirectory("PdfCopyTest/");
            FileStream outputPdfStream =
                new FileStream("PdfCopyTest/appearances(needAppearancesFalseWithStreams).pdf", FileMode.Create);
            Document document = new Document();
            PdfCopy copy = new PdfCopy(document, outputPdfStream);
            copy.SetMergeFields();
            document.Open();
            foreach (String f in new String[] {f1, f2, f3, f4}) {
                PdfReader r = new PdfReader(f);
                copy.AddDocument(r);
            }
            copy.Close();
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent("PdfCopyTest/appearances(needAppearancesFalseWithStreams).pdf", RESOURCES + "cmp_appearances(needAppearancesFalseWithStreams).pdf", "PdfCopyTest/", "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public virtual void TestNeedAppearancesMixed() {
            String f1 = RESOURCES + "appearances1.pdf";
            String f2 = RESOURCES + "appearances2(needAppearancesFalse).pdf";
            String f3 = RESOURCES + "appearances3(needAppearancesFalseWithStreams).pdf";
            String f4 = RESOURCES + "appearances4.pdf";

            Directory.CreateDirectory("PdfCopyTest/");
            FileStream outputPdfStream =
                new FileStream("PdfCopyTest/appearances(mixed).pdf", FileMode.Create);
            Document document = new Document();
            PdfCopy copy = new PdfCopy(document, outputPdfStream);
            copy.SetMergeFields();
            document.Open();
            foreach (String f in new String[] {f1, f2, f3, f4}) {
                PdfReader r = new PdfReader(f);
                copy.AddDocument(r);
            }
            copy.Close();
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent("PdfCopyTest/appearances(mixed).pdf", RESOURCES + "cmp_appearances(mixed).pdf", "PdfCopyTest/", "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }


        [Test]
        public virtual void TestFullCompression1() {
            Directory.CreateDirectory("PdfCopyTest/");
            String outfile = "PdfCopyTest/out-noforms.pdf";
            String first = RESOURCES + "hello.pdf";
            String second = RESOURCES + "hello_memory.pdf";
            FileStream out_ = new FileStream(outfile, FileMode.Create);
            PdfReader reader = new PdfReader(first);
            PdfReader reader2 = new PdfReader(second);
            Document pdfDocument = new Document();
            PdfCopy pdfCopy = new PdfCopy(pdfDocument, out_);
            pdfCopy.SetMergeFields();
            pdfCopy.SetFullCompression();
            pdfCopy.CompressionLevel = PdfStream.BEST_COMPRESSION;
            pdfDocument.Open();
            pdfCopy.AddDocument(reader);
            pdfCopy.AddDocument(reader2);
            pdfCopy.Close();
            reader.Close();
            reader2.Close();

            reader = new PdfReader("PdfCopyTest/out-noforms.pdf");
            Assert.NotNull(reader.GetPageN(1));
            reader.Close();
        }

        [Test]
        public virtual void TestFullCompression2() {
            Directory.CreateDirectory("PdfCopyTest/");
            String outfile = "PdfCopyTest/out-forms.pdf";
            String first = RESOURCES + "subscribe.pdf";
            String second = RESOURCES + "filled_form_1.pdf";
            FileStream out_ = new FileStream(outfile, FileMode.Create);
            PdfReader reader = new PdfReader(first);
            PdfReader reader2 = new PdfReader(second);
            Document pdfDocument = new Document();
            PdfCopy pdfCopy = new PdfCopy(pdfDocument, out_);
            pdfCopy.SetMergeFields();
            pdfCopy.SetFullCompression();
            pdfCopy.CompressionLevel = PdfStream.BEST_COMPRESSION;
            pdfDocument.Open();
            pdfCopy.AddDocument(reader);
            pdfCopy.AddDocument(reader2);
            pdfCopy.Close();
            reader.Close();
            reader2.Close();

            reader = new PdfReader("PdfCopyTest/out-forms.pdf");
            Assert.NotNull(reader.GetPageN(1));
            reader.Close();
        }

        [Test]
        public virtual void CopyFields1Test() {
            Document pdfDocument = new Document();
            Directory.CreateDirectory("PdfCopyTest/");
            PdfCopy copier = new PdfCopy(pdfDocument, new FileStream("PdfCopyTest/copyFields.pdf", FileMode.Create));
            copier.SetMergeFields();

            pdfDocument.Open();

            PdfReader readerMain = new PdfReader(RESOURCES + "fieldsOn3-sPage.pdf");
            PdfReader secondSourceReader = new PdfReader(RESOURCES + "fieldsOn2-sPage.pdf");
            PdfReader thirdReader = new PdfReader(RESOURCES + "appearances1.pdf");

            copier.AddDocument(readerMain);
            copier.CopyDocumentFields(secondSourceReader);
            copier.AddDocument(thirdReader);

            copier.Close();
            readerMain.Close();
            secondSourceReader.Close();
            thirdReader.Close();
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent("PdfCopyTest/copyFields.pdf", RESOURCES + "cmp_copyFields.pdf", "PdfCopyTest/", "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public virtual void CopyFields2Test() {
            Document pdfDocument = new Document();
            Directory.CreateDirectory("PdfCopyTest/");
            PdfCopy copier = new PdfCopy(pdfDocument, new FileStream("PdfCopyTest/copyFields2.pdf", FileMode.Create));
            copier.SetMergeFields();
            pdfDocument.Open();

            PdfReader reader = new PdfReader(RESOURCES + "hello_with_comments.pdf");
            copier.AddDocument(reader);
            copier.Close();
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent("PdfCopyTest/copyFields2.pdf", RESOURCES + "cmp_copyFields2.pdf", "PdfCopyTest/", "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public virtual void CopyFields3Test() {
            Document pdfDocument = new Document();
            Directory.CreateDirectory("PdfCopyTest/");
            PdfCopy copier = new PdfCopy(pdfDocument, new FileStream("PdfCopyTest/copyFields3.pdf", FileMode.Create));
            copier.SetMergeFields();
            pdfDocument.Open();

            PdfReader reader = new PdfReader(RESOURCES + "hello2_with_comments.pdf");
            copier.AddDocument(reader);
            copier.Close();
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent("PdfCopyTest/copyFields3.pdf", RESOURCES + "cmp_copyFields3.pdf", "PdfCopyTest/", "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public virtual void CopyFields4Test() {
            string target = "PdfCopyTest/";
            Directory.CreateDirectory(target);
            const string outfile = "copyFields4.pdf";
            const string inputFile = "link.pdf";

            Document document = new Document();
            MemoryStream stream = new MemoryStream();
            PdfWriter.GetInstance(document, stream);
            Font font = new Font(BaseFont.CreateFont(RESOURCES + "fonts/georgia.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED), 9);
            document.Open();
            document.Add(new Phrase("text", font));
            document.Close();

            Document pdfDocument = new Document();
            PdfCopy copier = new PdfCopy(pdfDocument, new FileStream(target + outfile, FileMode.Create));
            copier.SetMergeFields();
            pdfDocument.Open();

            PdfReader reader1 = new PdfReader(RESOURCES + inputFile);
            PdfReader reader2 = new PdfReader(stream.ToArray());

            copier.AddDocument(reader1);
            copier.AddDocument(reader2);
            copier.Close();
            CompareTool cmpTool = new CompareTool();
            string errorMessage = cmpTool.CompareByContent(target + outfile, RESOURCES + "cmp_" + outfile, target, "diff");
            if (errorMessage != null)
                Assert.Fail(errorMessage);
        }

        [Test]
        [Timeout(60000)]
        public virtual void LargeFilePerformanceTest() {
            const string target = "PdfCopyTest/";
            const string output = "copyLargeFile.pdf";
            const string cmp = "cmp_copyLargeFile.pdf";
            
            Directory.CreateDirectory(target);

            Stopwatch timer = new Stopwatch();
            timer.Start();

            PdfReader firstSourceReader = new PdfReader(RESOURCES + "frontpage.pdf");
            PdfReader secondSourceReader = new PdfReader(RESOURCES + "large_pdf.pdf");

            Document document = new Document();

            PdfCopy copy = new PdfCopy(document, File.Create(target + output));
            copy.SetMergeFields();

            document.Open();
            copy.AddDocument(firstSourceReader);
            copy.AddDocument(secondSourceReader);

            copy.Close();
            document.Close();

            timer.Stop();
            Console.WriteLine(timer.ElapsedMilliseconds);

            CompareTool cmpTool = new CompareTool();
            String errorMessage = cmpTool.CompareByContent(target + output, RESOURCES + cmp, target, "diff");

            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public void MergeNamedDestinationsTest()  {
            string outputFolder = "PdfCopyTest/";
            string outputFile = "namedDestinations.pdf";
            Directory.CreateDirectory(outputFolder);

            // Create simple document
            MemoryStream main = new MemoryStream();
            Document doc = new Document(new Rectangle(612f,792f),54f,54f,36f,36f);
            PdfWriter pdfwrite = PdfWriter.GetInstance(doc, main);
            doc.Open();
            doc.Add(new Paragraph("Testing Page"));
            doc.Close();

            // Create TOC document
            MemoryStream two = new MemoryStream();
            Document doc2 = new Document(new Rectangle(612f,792f),54f,54f,36f,36f);
            PdfWriter pdfwrite2 = PdfWriter.GetInstance(doc2, two);
            doc2.Open();
            Chunk chn = new Chunk("<<-- Link To Testing Page -->>");
            chn.SetRemoteGoto("DUMMY.PDF","page-num-1");
            doc2.Add(new Paragraph(chn));
            doc2.Close();

            // Merge documents
            MemoryStream three = new MemoryStream();
            PdfReader reader1 = new PdfReader(main.ToArray());
            PdfReader reader2 = new PdfReader(two.ToArray());
            Document doc3 = new Document();
            PdfCopy DocCopy = new PdfCopy(doc3,three);
            doc3.Open();
            DocCopy.AddPage(DocCopy.GetImportedPage(reader2,1));
            DocCopy.AddPage(DocCopy.GetImportedPage(reader1,1));
            DocCopy.AddNamedDestination("page-num-1",2,new PdfDestination(PdfDestination.FIT));
            doc3.Close();

            // Fix references and write to file
            PdfReader finalReader = new PdfReader(three.ToArray());
            finalReader.MakeRemoteNamedDestinationsLocal();
            PdfStamper stamper = new PdfStamper(finalReader,new  FileStream(outputFolder + outputFile, FileMode.Create));
            stamper.Close();

           
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(outputFolder + outputFile, RESOURCES + "cmp_" + outputFile, outputFolder, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }


        [Test]
        public void RecursiveSmartMergeTest() {
            string pathPrefix = @"PdfCopyTest/";
            string inputDocPath = @"recursiveSmartMerge.pdf";

            byte[]  part1 = ExtractPages(Path.Combine(RESOURCES, inputDocPath), 1, 2);
            string outputPath1 = Path.Combine(pathPrefix, "part1_c.pdf");
            File.WriteAllBytes(outputPath1, part1);

            byte[] part2 = ExtractPages(Path.Combine(RESOURCES, inputDocPath), 3,7);
            string outputPath2 = Path.Combine(pathPrefix, "part2_c.pdf");
            File.WriteAllBytes(outputPath2, part2);

            byte[] merged = Merge(new string[] {outputPath1, outputPath2});

            string mergedPath = Path.Combine(pathPrefix, "output_c.pdf");
            File.WriteAllBytes(mergedPath, merged);

            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(mergedPath, RESOURCES + "cmp_" + inputDocPath, "PdfCopyTest/" , "diff");
            if (errorMessage != null)
            {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public void CopySignedDocuments()
        {
            string file = RESOURCES + "hello_signed1.pdf";
            Directory.CreateDirectory("PdfCopyTest/");
            Document pdfDocument = new Document();
            PdfCopy copier = new PdfCopy(pdfDocument, new FileStream("PdfCopyTest/CopySignedDocuments.pdf", FileMode.Create));
            pdfDocument.Open();

            PdfReader reader1 = new PdfReader(file);
            copier.AddPage(copier.GetImportedPage(reader1, 1));
            copier.FreeReader(reader1);

            reader1 = new PdfReader(file);
            copier.AddPage(copier.GetImportedPage(reader1, 1));
            copier.FreeReader(reader1);

            pdfDocument.Close();

            PdfReader reader = new PdfReader("PdfCopyTest/CopySignedDocuments.pdf");
            PdfDictionary sig = (PdfDictionary)reader.GetPdfObject(9);
            PdfDictionary sigRef = sig.GetAsArray(PdfName.REFERENCE).GetAsDict(0);
            Assert.True(PdfName.SIGREF.Equals(sigRef.GetAsName(PdfName.TYPE)));
            Assert.False(sigRef.Contains(PdfName.DATA));
            sig = (PdfDictionary)reader.GetPdfObject(21);
            sigRef = sig.GetAsArray(PdfName.REFERENCE).GetAsDict(0);
            Assert.True(PdfName.SIGREF.Equals(sigRef.GetAsName(PdfName.TYPE)));
            Assert.False(sigRef.Contains(PdfName.DATA));

        }

        [Test]
        public void SmartCopySignedDocuments()
        {
            string file = RESOURCES + "hello_signed1.pdf";
            Directory.CreateDirectory("PdfCopyTest/");
            Document pdfDocument = new Document();
            PdfSmartCopy copier = new PdfSmartCopy(pdfDocument, new FileStream("PdfCopyTest/SmartCopySignedDocuments.pdf", FileMode.Create));
            pdfDocument.Open();

            PdfReader reader1 = new PdfReader(file);
            copier.AddPage(copier.GetImportedPage(reader1, 1));
            copier.FreeReader(reader1);

            reader1 = new PdfReader(file);
            copier.AddPage(copier.GetImportedPage(reader1, 1));
            copier.FreeReader(reader1);

            pdfDocument.Close();

            PdfReader reader = new PdfReader("PdfCopyTest/SmartCopySignedDocuments.pdf");
            PdfDictionary sig = (PdfDictionary)reader.GetPdfObject(8);
            PdfDictionary sigRef = sig.GetAsArray(PdfName.REFERENCE).GetAsDict(0);
            Assert.True(PdfName.SIGREF.Equals(sigRef.GetAsName(PdfName.TYPE)));
            Assert.False(sigRef.Contains(PdfName.DATA));
        }

        public static byte[] Merge(string[] documentPaths) {
            byte[] mergedDocument;

            using (MemoryStream memoryStream = new MemoryStream())
            using (Document document = new Document())
            {
                PdfSmartCopy pdfSmartCopy = new PdfSmartCopy(document, memoryStream);
                document.Open();

                foreach (string docPath in documentPaths)
                {
                    PdfReader reader = new PdfReader(docPath);
                    try
                    {
                        reader.ConsolidateNamedDestinations();
                        int numberOfPages = reader.NumberOfPages;
                        for (int page = 0; page < numberOfPages; )
                        {
                            PdfImportedPage pdfImportedPage = pdfSmartCopy.GetImportedPage(reader, ++page);
                            pdfSmartCopy.AddPage(pdfImportedPage);
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }

                document.Close();
                mergedDocument = memoryStream.ToArray();
            }

            return mergedDocument;
        }


        public static byte[] ExtractPages(string pdfDocument, int startPage, int endPage ) {
            PdfReader reader = new PdfReader(pdfDocument);
            int numberOfPages = reader.NumberOfPages;
            int endPageResolved = endPage;
            if (startPage > numberOfPages || endPageResolved > numberOfPages)
                return null;

            byte[] outputDocument;
            using (Document doc = new Document())
            using (MemoryStream msOut = new MemoryStream())
            {
                PdfCopy pdfCopyProvider = new PdfCopy(doc, msOut);
                doc.Open();
                for (int i = startPage; i <= endPageResolved; i++)
                {
                    PdfImportedPage page = pdfCopyProvider.GetImportedPage(reader, i);
                    pdfCopyProvider.AddPage(page);
                }
                doc.Close();
                reader.Close();
                outputDocument = msOut.ToArray();
            }

            return outputDocument;
        }
    }
}
