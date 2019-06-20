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
using System.IO;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class PdfReaderSelectPagesTest
    {
        byte[] data;
        string dataFile;
        string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\PdfReaderSelectPagesTest\";



        [SetUp]
        virtual public void SetUp()
        {
            TestResourceUtils.PurgeTempFiles();
            dataFile = TestResourceUtils.GetResourceAsTempFile(TEST_RESOURCES_PATH, "RomeoJuliet.pdf");
            data = TestResourceUtils.GetResourceAsByteArray(TEST_RESOURCES_PATH, "RomeoJuliet.pdf");
        }

        [TearDown]
        virtual public void TearDown()
        {
            TestResourceUtils.PurgeTempFiles();
        }

        [Test]
        virtual public void Test()
        {
            PdfReader reader = new PdfReader(dataFile);
            try
            {
                reader.SelectPages("4-8");
                ManipulateWithStamper(reader);
                ManipulateWithCopy(reader);
            }
            finally
            {
                reader.Close();
            }
        }


        /**
         * Creates a new PDF based on the one in the reader
         * @param reader a reader with a PDF file
         * @throws IOException
         * @throws DocumentException
         */
        private void ManipulateWithStamper(PdfReader reader)
        {
            PdfStamper stamper = new PdfStamper(reader, new MemoryStream());
            stamper.Close();
        }

        /**
         * Creates a new PDF based on the one in the reader
         * @param reader a reader with a PDF file
         * @throws IOException
         * @throws DocumentException
         */
        private void ManipulateWithCopy(PdfReader reader)
        {
            int n = reader.NumberOfPages;
            Document document = new Document();
            PdfCopy copy = new PdfCopy(document, new MemoryStream());
            document.Open();
            for (int i = 0; i < n; )
            {
                copy.AddPage(copy.GetImportedPage(reader, ++i));
            }
            document.Close();
        }

    }
}
