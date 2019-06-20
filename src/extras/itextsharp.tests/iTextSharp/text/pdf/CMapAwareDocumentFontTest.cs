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
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    internal class CMapAwareDocumentFontTest {
        private string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\CMapAwareDocumentFontTest\";

        [Test]
        virtual public void TestWidths() {
            PdfReader pdfReader = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, "fontwithwidthissue.pdf");

            try {
                PdfDictionary fontsDic = pdfReader.GetPageN(1).GetAsDict(PdfName.RESOURCES).GetAsDict(PdfName.FONT);
                PRIndirectReference fontDicIndirect = (PRIndirectReference) fontsDic.Get(new PdfName("F1"));

                CMapAwareDocumentFont f = new CMapAwareDocumentFont(fontDicIndirect);
                Assert.IsTrue(f.GetWidth('h') != 0, "Width should not be 0");
            }
            finally {
                pdfReader.Close();
            }
        }
		
        [Test]
		virtual public void IllegalDifference() {
			PdfReader reader = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, "illegalDifference.pdf");
			// this call will throw an exception and make the test fail if we remove the error-catching code
			PdfTextExtractor.GetTextFromPage(reader, 1);
		}

        [Test]
        virtual public void WeirdHyphensTest() {
            PdfReader reader = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, "WeirdHyphens.pdf");
            List<String> textChunks = new List<String>();
            IRenderListener listener = new MyTextRenderListener(textChunks);
            PdfContentStreamProcessor processor = new PdfContentStreamProcessor(listener);
            PdfDictionary pageDic = reader.GetPageN(1);
            PdfDictionary resourcesDic = pageDic.GetAsDict(PdfName.RESOURCES);
            processor.ProcessContent(ContentByteUtils.GetContentBytesForPage(reader, 1), resourcesDic);
            /**
             * This assertion makes sure that encoding has been read properly from FontDescriptor.
             * If not the vallue will be "\u0000 14".
             */
            Assert.AreEqual("\u0096 14", textChunks[18]);
            reader.Close();
        }

        private class MyTextRenderListener : IRenderListener {
            private List<String> textChunks;

            public MyTextRenderListener(List<String> textChunks) {
                this.textChunks = textChunks;
            }

            virtual public void BeginTextBlock() {
            }

            virtual public void EndTextBlock() {
            }

            virtual public void RenderImage(ImageRenderInfo renderInfo) {
            }

            virtual public void RenderText(TextRenderInfo renderInfo) {
                textChunks.Add(renderInfo.GetText());
            }
        }
    }
}
