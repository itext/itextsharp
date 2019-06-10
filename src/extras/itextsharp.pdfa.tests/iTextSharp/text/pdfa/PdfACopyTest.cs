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
using System.IO;
using itextsharp.pdfa.iTextSharp.text.pdf;
using iTextSharp.text;
using iTextSharp.text.error_messages;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.pdfa.tests.iTextSharp.text.pdfa {
    public class PdfACopyTest {
        protected const String outputDir = "copy\\";
        public const String RESOURCES = @"..\..\resources\text\pdfa\";

        static PdfACopyTest() {
            Directory.CreateDirectory(outputDir);
            try {
                MessageLocalization.SetLanguage("en", "US");
            } catch (IOException e) {
            }
        }

        [Test]
        public virtual void TestCreatePdfA_1() {
            String f1 = RESOURCES + "copy\\pdfa-1a.pdf";
            String testName = "testCreatePdfA_1.pdf";

            FileStream outputPdfStream = new FileStream(outputDir + testName, FileMode.Create);
            Document document = new Document();
            PdfACopy copy = new PdfACopy(document, outputPdfStream, PdfAConformanceLevel.PDF_A_1B);
            copy.CreateXmpMetadata();
            document.Open();
            document.AddLanguage("en-US");
            PdfReader reader = new PdfReader(f1);

            PdfImportedPage page = copy.GetImportedPage(reader, 1);
            PdfCopy.PageStamp stamp = copy.CreatePageStamp(page);
            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 24);
            ColumnText.ShowTextAligned(stamp.GetUnderContent(), Element.ALIGN_CENTER, new Phrase("Hello world!", font), 100, 500, 0);
            stamp.AlterContents();
            copy.AddPage(page);

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read,
                FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            copy.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            copy.Close();
        }

        [Test]
        public virtual void TestMergeFields1() {
            String f1 = RESOURCES + "copy/pdfa-1a.pdf";
            String f2 = RESOURCES + "copy/pdfa-1a-2.pdf";
            String testName = "testMergeFields1.pdf";

            FileStream outputPdfStream = new FileStream(outputDir + testName, FileMode.Create);
            Document document = new Document();
            PdfACopy copy = new PdfACopy(document, outputPdfStream, PdfAConformanceLevel.PDF_A_1A);
            copy.SetMergeFields();
            copy.CreateXmpMetadata();
            copy.SetTagged();
            document.Open();
            document.AddLanguage("en-US");
            foreach (String f in new String[] {f1, f2}) {
                PdfReader reader = new PdfReader(f);
                copy.AddDocument(reader);
            }

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read,
                FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            copy.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            copy.Close();
        }

        [Test]
        public virtual void TestMergeFields2() {
            String f1 = RESOURCES + "copy/pdfa-1a.pdf";
            String f2 = RESOURCES + "copy/pdfa-1a-2.pdf";
            String f3 = RESOURCES + "copy/pdfa-1b.pdf";

            Stream outputPdfStream = new MemoryStream();
            Document document = new Document();
            PdfACopy copy = new PdfACopy(document, outputPdfStream, PdfAConformanceLevel.PDF_A_1A);
            copy.SetMergeFields();
            copy.CreateXmpMetadata();
            copy.SetTagged();
            document.Open();
            document.AddLanguage("en-US");

            bool exceptionThrown = false;
            try {
                foreach (String f in new String[] {f1, f2, f3}) {
                    PdfReader reader = new PdfReader(f);
                    copy.AddDocument(reader);
                }
            } catch (PdfAConformanceException e) {
                if (e.Message.Contains("Incompatible PDF/A conformance level"))
                    exceptionThrown = true;
            }

            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        public virtual void TestMergeFields3() {
            String f1 = RESOURCES + "copy/pdfa-1a.pdf";
            String f2 = RESOURCES + "copy/pdfa-2a.pdf";

            Stream outputPdfStream = new MemoryStream();
            Document document = new Document();
            PdfACopy copy = new PdfACopy(document, outputPdfStream, PdfAConformanceLevel.PDF_A_1A);
            copy.SetMergeFields();
            copy.CreateXmpMetadata();
            copy.SetTagged();
            document.Open();
            document.AddLanguage("en-US");

            bool exceptionThrown = false;
            try {
                foreach (String f in new String[] {f1, f2}) {
                    PdfReader reader = new PdfReader(f);
                    copy.AddDocument(reader);
                }
            } catch (PdfAConformanceException e) {
                if (e.Message.Contains("Different PDF/A version"))
                    exceptionThrown = true;
            }

            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        public virtual void TestMergeFields4() {
            String f1 = RESOURCES + "copy/pdfa-1a.pdf";
            String f2 = RESOURCES + "copy/source16.pdf";

            Stream outputPdfStream = new MemoryStream();
            Document document = new Document();
            PdfACopy copy = new PdfACopy(document, outputPdfStream, PdfAConformanceLevel.PDF_A_1B);
            copy.SetMergeFields();
            copy.CreateXmpMetadata();
            copy.SetTagged();
            document.Open();
            document.AddLanguage("en-US");
            bool exceptionThrown = false;
            try {
                foreach (String f in new String[] {f1, f2}) {
                    PdfReader reader = new PdfReader(f);
                    copy.AddDocument(reader);
                }
            } catch (PdfAConformanceException e) {
                if (e.Message.Contains("Only PDF/A documents can be added in PdfACopy"))
                    exceptionThrown = true;
            }

            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }


    [Test]
    public virtual void TestImportedPage1() {
        String f1 = RESOURCES + "copy/pdfa-1a.pdf";
        String f2 = RESOURCES + "copy/pdfa-1a-2.pdf";
        String testName = "testImportedPage1.pdf";

        FileStream outputPdfStream = new FileStream(outputDir + testName, FileMode.Create);
        Document document = new Document();
        PdfACopy copy = new PdfACopy(document, outputPdfStream, PdfAConformanceLevel.PDF_A_1A);
        copy.CreateXmpMetadata();
        copy.SetTagged();
        document.Open();
        document.AddLanguage("en-US");
        foreach (String f in new String[] {f1, f2}) {
            PdfReader reader = new PdfReader(f);
            for (int i = 1; i <= reader.NumberOfPages; i++) {
                copy.AddPage(copy.GetImportedPage(reader, i, true));
            }
        }

        FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read,
            FileShare.Read);
        ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
        iccProfileFileStream.Close();

        copy.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
        copy.Close();
    }

        [Test]
        public virtual void TestImportedPage2() {
            String f1 = RESOURCES + "copy/pdfa-1a.pdf";
            String f2 = RESOURCES + "copy/pdfa-1a-2.pdf";
            String f3 = RESOURCES + "copy/pdfa-2a.pdf";

            Stream outputPdfStream = new MemoryStream();
            Document document = new Document();
            PdfACopy copy = new PdfACopy(document, outputPdfStream, PdfAConformanceLevel.PDF_A_1A);
            copy.CreateXmpMetadata();
            copy.SetTagged();
            document.Open();
            document.AddLanguage("en-US");

            bool exceptionThrown = false;
            try {
                foreach (String f in new String[] {f1, f2, f3}) {
                    PdfReader reader = new PdfReader(f);
                    for (int i = 1; i <= reader.NumberOfPages; i++) {
                        copy.AddPage(copy.GetImportedPage(reader, i, true));
                    }
                }
            } catch (PdfAConformanceException e) {
                if (e.Message.Contains("Different PDF/A version"))
                    exceptionThrown = true;
            }

            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        public virtual void TestImportedPage3() {
            String f1 = RESOURCES + "copy/pdfa-1a.pdf";
            String f2 = RESOURCES + "copy/pdfa-1b-2.pdf";
            String testName = "testImportedPage3.pdf";

            Stream outputPdfStream = new FileStream(outputDir + testName, FileMode.Create);
            Document document = new Document();
            PdfACopy copy = new PdfACopy(document, outputPdfStream, PdfAConformanceLevel.PDF_A_1B);
            copy.CreateXmpMetadata();
            document.Open();
            foreach (String f in new String[] {f1, f2}) {
                PdfReader reader = new PdfReader(f);
                for (int i = 1; i <= reader.NumberOfPages; i++) {
                    copy.AddPage(copy.GetImportedPage(reader, i));
                }
            }

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read,
                FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            copy.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            copy.Close();
        }

        [Test]
        public virtual void TestImportedPage4() {
            String f1 = RESOURCES + "copy/pdfa-2a.pdf";
            String f2 = RESOURCES + "copy/pdfa-2u.pdf";
            String f3 = RESOURCES + "copy/pdfa-2b.pdf";
            String testName = "testImportedPage4.pdf";

            Stream outputPdfStream = new FileStream(outputDir + testName, FileMode.Create);
            Document document = new Document();
            PdfACopy copy = new PdfACopy(document, outputPdfStream, PdfAConformanceLevel.PDF_A_2B);
            copy.CreateXmpMetadata();
            document.Open();
            foreach (String f in new String[] {f1, f2, f3}) {
                PdfReader reader = new PdfReader(f);
                for (int i = 1; i <= reader.NumberOfPages; i++) {
                    copy.AddPage(copy.GetImportedPage(reader, i));
                }
            }
        
            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read,
                FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            copy.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            copy.Close();
        }

        [Test]
        public virtual void TestImportedPage5() {
            String f1 = RESOURCES + "copy/pdfa-3a.pdf";
            String f2 = RESOURCES + "copy/pdfa-3u.pdf";
            String testName = "testImportedPage5.pdf";

            Stream outputPdfStream = new FileStream(outputDir + testName, FileMode.Create);
            Document document = new Document();
            PdfACopy copy = new PdfACopy(document, outputPdfStream, PdfAConformanceLevel.PDF_A_3U);
            copy.CreateXmpMetadata();
            document.Open();

            foreach (String f in new String[] {f1, f2}) {
                PdfReader reader = new PdfReader(f);
                for (int i = 1; i <= reader.NumberOfPages; i++) {
                    copy.AddPage(copy.GetImportedPage(reader, i));
                }
            }

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read,
                FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            copy.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            copy.Close();
        }

        [Test]
        public void TestImportedPage6() {
            String f1 = RESOURCES + "copy/pdfa-3a.pdf";
            String f2 = RESOURCES + "copy/pdfa-3u.pdf";
            String f3 = RESOURCES + "copy/pdfa-3b.pdf";
            String testName = "testImportedPage5.pdf";

            Stream outputPdfStream = new MemoryStream();
            Document document = new Document();
            PdfACopy copy = new PdfACopy(document, outputPdfStream, PdfAConformanceLevel.PDF_A_3U);
            copy.CreateXmpMetadata();
            document.Open();

            bool exceptionThrown = false;
            try {
                foreach (String f in new String[] {f1, f2, f3}) {
                    PdfReader reader = new PdfReader(f);
                    for (int i = 1; i <= reader.NumberOfPages; i++) {
                        copy.AddPage(copy.GetImportedPage(reader, i));
                    }
                }
            } catch (PdfAConformanceException e) {
                if (e.Message.Contains("Incompatible PDF/A conformance level"))
                    exceptionThrown = true;
            }

            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        public void TestSmartCopyCreatePdfA_1() {
            String fileName = RESOURCES + "copy/pdfa-1a.pdf";
            String testName = "testSmartCopyPdfA_1.pdf";

            FileStream outputPdfStream = File.Create(outputDir + testName);
            Document document = new Document();
            PdfCopy copy = new PdfASmartCopy(document, outputPdfStream, PdfAConformanceLevel.PDF_A_1B);
            copy.CreateXmpMetadata();
            document.Open();
            document.AddLanguage("en-US");
            PdfReader reader = new PdfReader(fileName);
            PdfImportedPage page = copy.GetImportedPage(reader, 1);
            copy.AddPage(page);
            copy.Close();
        }

    }
}
