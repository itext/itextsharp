/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
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
using System.Collections.Generic;
using System.IO;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.exceptions;
using iTextSharp.text.io;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class PdfReaderTest
    {
        [SetUp]
        virtual public void SetUp()
        {
            TestResourceUtils.PurgeTempFiles();
        }

        [TearDown]
        virtual public void TearDown()
        {
            TestResourceUtils.PurgeTempFiles();
        }

        string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\PdfReaderTest\";

        [Test, Ignore("validity of test needs to be resolved")]
        virtual public void TestGetLink()
        {
            string testFile = TestResourceUtils.GetResourceAsTempFile(TEST_RESOURCES_PATH, "getLinkTest1.pdf");
            PdfReader currentReader = new PdfReader(testFile);
            Document document = new Document(PageSize.A4, 0, 0, 0, 0);
            PdfWriter writer = PdfWriter.GetInstance(document, new MemoryStream());
            document.Open();
            document.NewPage();
            List<PdfAnnotation.PdfImportedLink> links = currentReader.GetLinks(1);
            PdfAnnotation.PdfImportedLink link = links[0];
            writer.AddAnnotation(link.CreateAnnotation(writer));
            document.Close();

            currentReader.Close();
        }

        [Test]
        virtual public void testGetLink2()
        {
            string testFile = TestResourceUtils.GetResourceAsTempFile(TEST_RESOURCES_PATH, "getLinkTest2.pdf");
            string filename = testFile;
            PdfReader rdr = new PdfReader(new RandomAccessFileOrArray(filename), new byte[0]);
            // this one works: PdfReader rdr = new PdfReader(filename);
            rdr.ConsolidateNamedDestinations(); // does not help
            rdr.GetLinks(1);

            rdr.Close();
        }

        [Test]
        virtual public void testPageResources()
        {
            string testFile = TestResourceUtils.GetResourceAsTempFile(TEST_RESOURCES_PATH, "getLinkTest2.pdf");
            String filename = testFile;
            PdfReader rdr = new PdfReader(new RandomAccessFileOrArray(filename), new byte[0]);

            PdfDictionary pageResFromNum = rdr.GetPageResources(1);
            PdfDictionary pageResFromDict = rdr.GetPageResources(rdr.GetPageN(1));
            // same size & keys
            Assert.IsTrue(pageResFromNum.Keys.Equals(pageResFromDict.Keys));

            rdr.Close();
        }

        //Check for crash
        [Test]
        public virtual void ReadCompressedPdfTest1() {
            string filename = TestResourceUtils.GetResourceAsTempFile(TEST_RESOURCES_PATH, "readCompressedPdfTest1.pdf");
            PdfReader rdr = new PdfReader(filename);
            for (int i = 1; i <= rdr.NumberOfPages; i++) {
                PdfDictionary p = rdr.GetPageNRelease(i);
                Assert.AreEqual(PdfName.PAGE.ToString(), p.GetAsName(PdfName.TYPE).ToString());
            }
            rdr.Close();
        }

        [Test]
        public void PartialReadFromByteArrayTest() {
            byte[] pdfFile = TestResourceUtils.GetResourceAsByteArray(TEST_RESOURCES_PATH, "iphone_user_guide.pdf");
            PdfReader reader = new PdfReader(new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource(pdfFile)), null, true);

            int pagesNum = 0;
            PdfDictionary pagesObj = reader.Catalog.GetAsDict(PdfName.PAGES);
            Stack<PdfDictionary> pages = new Stack<PdfDictionary>();
            pages.Push(pagesObj);
            while(pages.Count > 0) {
                PdfDictionary page = pages.Pop();
                PdfArray kids = page.GetAsArray(PdfName.KIDS);
                if (kids != null) {
                    for (int i = 0; i < kids.Size; ++i) {
                        pages.Push(kids.GetAsDict(i));
                    }
                } else {
                    ++pagesNum;
                }
            }

            Assert.True(pagesNum == 130, String.Format("There is 130 pages in document, but iText counted {0}", pagesNum));
        }

         
        [Test]
        public void CircularReferencesInResources()  {
            string testFile = TestResourceUtils.GetResourceAsTempFile(TEST_RESOURCES_PATH, "circularReferencesInResources.pdf");
            PdfReader rdr = new PdfReader(testFile);
            rdr.Close();
            Assert.Throws(typeof(InvalidPdfException), delegate { BaseFont.GetDocumentFonts(rdr); });
        }

        
        [Test]
        public void CircularReferencesInPageTree()  {
            string testFile = TestResourceUtils.GetResourceAsTempFile(TEST_RESOURCES_PATH, "circularReferencesInPageTree.pdf");
            Assert.Throws(typeof(InvalidPdfException), delegate { new PdfReader(testFile); });
        }

    }
}
