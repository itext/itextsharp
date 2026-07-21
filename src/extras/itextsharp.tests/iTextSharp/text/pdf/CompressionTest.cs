/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2026 iText Group NV
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
using System.Text;
using NUnit.Framework;
using iTextSharp.text;
using iTextSharp.text.exceptions;
using iTextSharp.text.io;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    internal class CompressionTest
    {
        private const string TEST_RESOURCES_PATH = @"..\..\..\resources\text\pdf\CompressionTest\";
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

        [Test]
        public void flateBombTest()
        {
            PdfReader reader = new PdfReader(TEST_RESOURCES_PATH + "pageStreamFlateBomb.pdf");

            testDecompressionBomb(reader, MemoryLimitsAwareException.DuringDecompressionSingleStreamOccupiedMoreMemoryThanAllowed);
        }

        [Test]
        public void pngDecodeStreamTest()
        {
            // This test demonstrates a possible false positive
            PdfReader reader = new PdfReader(TEST_RESOURCES_PATH + "png5000x5000.pdf");
            PdfDictionary resources = reader.GetPageResources(1);
            PdfDictionary xobjects = resources.GetAsDict(PdfName.XOBJECT);
            PdfIndirectReference objRef = xobjects.GetAsIndirectObject(new PdfName("Im0"));
            PRStream stream = (PRStream) PdfReader.GetPdfObject(objRef);
            try {
                PdfImageObject img = new PdfImageObject(stream);
            } catch (MemoryLimitsAwareException e) {
                Assert.AreEqual(
                        MemoryLimitsAwareException.DuringDecompressionSingleStreamOccupiedMoreMemoryThanAllowed,
                        e.Message);
                return;
            }

            Assert.Fail("Expected MemoryLimitsAwareException was not thrown");
        }
        
        [Test]
        public void streamWithoutEndstreamKeywordTest() {
            PdfReader reader = new PdfReader(TEST_RESOURCES_PATH + "NoEndstreamKeyword.pdf");
            PdfStream xmpMetadataStream = reader.catalog.GetAsStream(PdfName.METADATA);
            int xmpMetadataStreamLength = xmpMetadataStream.GetAsNumber(PdfName.LENGTH).IntValue;
            Assert.AreEqual(27599, xmpMetadataStreamLength);

            Exception e = Assert.Throws(typeof(EndOfStreamException), delegate { byte[] mtd = reader.Metadata; });
        }

        [Test]
        public void endDicInsteadOfArrayClosingBracketTest() {
            Exception e = Assert.Throws(typeof(InvalidPdfException), delegate {
                new PdfReader(TEST_RESOURCES_PATH + "invalidArrayEndDictToken.pdf");
            });
            Assert.AreEqual("Rebuild failed: Unexpected '>>' at file pointer 532; Original message: Unexpected '>>'"
                    + " at file pointer 532", e.Message);
        }
        
        [Test]
        public void endArrayClosingBracketInsteadOfEndDicTest() {
            Exception e = Assert.Throws(typeof(InvalidPdfException), delegate {
                new PdfReader(TEST_RESOURCES_PATH + "endArrayClosingBracketInsteadOfEndDic.pdf");
            });
            Assert.AreEqual("Rebuild failed: Unexpected ']' at file pointer 221; Original message: Unexpected ']'"
                            + " at file pointer 221", e.Message);
        }
        
        [Test]
        public void endDicClosingBracketInsideTheDicTest() {
            Exception e = Assert.Throws(typeof(InvalidPdfException), delegate {
                new PdfReader(TEST_RESOURCES_PATH + "endDicClosingBracketInsideTheDic.pdf");
            });
            Assert.AreEqual("Rebuild failed: Unexpected '>>' at file pointer 221; Original message: Unexpected '>>'"
                            + " at file pointer 221", e.Message);
        }
        
        [Test]
        public void eofInsteadOfArrayClosingBracketTest() {
            Exception e = Assert.Throws(typeof(InvalidPdfException), delegate {
                new PdfReader(TEST_RESOURCES_PATH + "invalidArrayEOFToken.pdf");
            });
            Assert.IsTrue(e.Message.Contains("Rebuild failed:  is not a valid number - System.FormatException: Input string was not in a correct format."));
        }
        
        [Test]
        public void endObjInsteadOfArrayClosingBracketTest() {
            Exception e = Assert.Throws(typeof(InvalidPdfException), delegate {
                new PdfReader(TEST_RESOURCES_PATH + "invalidArrayEndObjToken.pdf");
            });
            Assert.IsTrue(e.Message.Contains("Rebuild failed:  is not a valid number - System.FormatException: Input string was not in a correct format."));
        }
        
        [Test]
        public void nameInsteadOfArrayClosingBracketTest() {
            Exception e = Assert.Throws(typeof(InvalidPdfException), delegate {
                new PdfReader(TEST_RESOURCES_PATH + "invalidArrayNameToken.pdf");
            });
            
            Assert.IsTrue(e.Message.Contains(" is not a valid number - System.FormatException: Input string was not in a correct format."));
        }
        
        [Test]
        public void objInsteadOfArrayClosingBracketTest() {
            Exception e = Assert.Throws(typeof(InvalidPdfException), delegate {
                new PdfReader(TEST_RESOURCES_PATH + "invalidArrayObjToken.pdf");
            });
            Assert.IsTrue(e.Message.Contains("Rebuild failed:  is not a valid number - System.FormatException: Input string was not in a correct format."));
        }
        
        [Test]
        public void refInsteadOfArrayClosingBracketTest() {
            Exception e = Assert.Throws(typeof(InvalidPdfException), delegate {
                new PdfReader(TEST_RESOURCES_PATH + "invalidArrayRefToken.pdf");
            });
            Assert.IsTrue(e.Message.Contains("Rebuild failed:  is not a valid number - System.FormatException: Input string was not in a correct format."));
        }
        
        [Test]
        public void startArrayInsteadOfArrayClosingBracketTest() {
            Exception e = Assert.Throws(typeof(InvalidPdfException), delegate {
                new PdfReader(TEST_RESOURCES_PATH + "invalidArrayStartArrayToken.pdf");
            });
            Assert.IsTrue(e.Message.Contains("Rebuild failed:  is not a valid number - System.FormatException: Input string was not in a correct format."));
        }
        
        [Test]
        public void stringInsteadOfArrayClosingBracketTest() {
            PdfReader reader = new PdfReader(TEST_RESOURCES_PATH + "invalidArrayStringToken.pdf");
            PdfArray actual = (PdfArray) reader.GetPdfObject(4);
            PdfArray expected = new PdfArray(new float[]{5, 10, 15, 20});
            for (int i = 0; i < expected.Size; i++) {
                Assert.AreEqual(expected.GetAsNumber(i).IntValue, actual.GetAsNumber(i).IntValue);
            }
        }
        
        [Test]
        public void closingArrayBracketMissingConservativeTest() {
            Exception e = Assert.Throws(typeof(InvalidPdfException), delegate {
                new PdfReader(TEST_RESOURCES_PATH + "invalidArrayObjToken.pdf");
            });
            Assert.IsTrue(e.Message.Contains("Rebuild failed:  is not a valid number - System.FormatException: Input string was not in a correct format."));
        }
        
        [Test]
        public void parseArrayTest() {
            PdfReader reader = new PdfReader(TEST_RESOURCES_PATH + "innerArraysInContentStreamWithEndDictToken.pdf");
            PRTokeniser cmpTokeniser = new PRTokeniser(new RandomAccessFileOrArray(
                    new RandomAccessSourceFactory().CreateSource(reader.GetPageContent(1))));
            PdfContentParser parser = new PdfContentParser(cmpTokeniser);
            Exception e = Assert.Throws(typeof(IOException), delegate {
                parseContentStream(parser);
            });
            Assert.AreEqual("Unexpected '>>'", e.Message);
        }
        
        private static void parseContentStream(PdfContentParser parser) {
            List<PdfObject> operands = new List<PdfObject>();
            while (parser.Parse(operands).Count != 0) {
                // do nothign with operands
            }
        }

        private static void testDecompressionBomb(PdfReader reader, String expectedExceptionMessage)
        {
            String thrownExceptionMessage = null;
            try {
                byte[] bytes = reader.GetPageContent(1);
            } catch (MemoryLimitsAwareException e) {
                thrownExceptionMessage = e.Message;
            } catch (OutOfMemoryException e) {
                Assert.Fail("Expected MemoryLimitsAwareException was not thrown");
            }

            reader.Close();
            Assert.AreEqual(expectedExceptionMessage, thrownExceptionMessage);
        }

    }
}
