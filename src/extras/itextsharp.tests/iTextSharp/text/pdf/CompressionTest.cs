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
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    internal class CompressionTest
    {
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\CompressionTest\";
        private const string TARGET_PATH = @"CompressionTest\";

        [Test]
        public void decompressionBombInsideSingleStreamTest01() 
        {
            MemoryLimitsAwareHandler memoryHandler = new MemoryLimitsAwareHandler();
            memoryHandler.SetMaxSizeOfSingleDecompressedPdfStream(5000000);
            memoryHandler.SetMaxSizeOfDecompressedPdfStreamsSum(1000000000000000000l); // just to ensure that the single stream related exception is thrown

            ReaderProperties properties = new ReaderProperties();
            properties.SetMemoryLimitsAwareHandler(memoryHandler);

            PdfReader reader = new PdfReader(properties, TEST_RESOURCES_PATH + "acsploit_output.pdf");

            testDecompressionBomb(reader, MemoryLimitsAwareException.DuringDecompressionSingleStreamOccupiedMoreMemoryThanAllowed);
        }

        [Test]
        public void decompressionBombInsideMultipleStreamsTimingTest01() {
            MemoryLimitsAwareHandler memoryHandler = new MemoryLimitsAwareHandler();
            memoryHandler.SetMaxSizeOfSingleDecompressedPdfStream(int.MaxValue / 10 * 9); // just to ensure that the multiple streams related exception is thrown
            memoryHandler.SetMaxSizeOfDecompressedPdfStreamsSum(1000000);

            ReaderProperties properties = new ReaderProperties();
            properties.SetMemoryLimitsAwareHandler(memoryHandler);

            PdfReader reader = new PdfReader(properties, TEST_RESOURCES_PATH + "acsploit_timing.pdf");

            testDecompressionBomb(reader, MemoryLimitsAwareException.DuringDecompressionMultipleStreamsInSumOccupiedMoreMemoryThanAllowed);
        }

        [Test]
        public void decompressionBombInsideMultipleStreamsTimingTest02()
        {
            MemoryLimitsAwareHandler memoryHandler = new MemoryLimitsAwareHandler();
            memoryHandler.SetMaxSizeOfSingleDecompressedPdfStream(int.MaxValue / 10 * 9);  // just to ensure that the multiple streams related exception is thrown
            memoryHandler.SetMaxSizeOfDecompressedPdfStreamsSum(1000000);

            ReaderProperties properties = new ReaderProperties();
            properties.SetMemoryLimitsAwareHandler(memoryHandler);

            PdfReader reader = new PdfReader(properties, TEST_RESOURCES_PATH + "acsploit_timing2.pdf");

            testDecompressionBomb(reader, MemoryLimitsAwareException.DuringDecompressionMultipleStreamsInSumOccupiedMoreMemoryThanAllowed);
        }
        private static void testDecompressionBomb(PdfReader reader, String expectedExceptionMessage)
        {

            String thrownExceptionMessage = null;
            try {
                byte[] bytes = reader.GetPageContent(1);
            } catch (MemoryLimitsAwareException e) {
                thrownExceptionMessage = e.Message;
            } catch (OutOfMemoryException e) {
                Assert.IsTrue(false);
            }

            reader.Close();
            Assert.AreEqual(expectedExceptionMessage, thrownExceptionMessage);
        }

    }
}
