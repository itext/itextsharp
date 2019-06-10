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
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser
{
    class FilteredTextRenderListenerTest
    {

        [Test]
        virtual public void TestRegion()
        {
            byte[] pdf = CreatePdfWithCornerText();

            PdfReader reader = new PdfReader(pdf);
            float pageHeight = reader.GetPageSize(1).Height;
            Rectangle upperLeft = new Rectangle(0, (int)pageHeight - 30, 250, (int)pageHeight);

            Assert.IsTrue(TextIsInRectangle(reader, "Upper Left", upperLeft));
            Assert.IsFalse(TextIsInRectangle(reader, "Upper Right", upperLeft));
        }

        private bool TextIsInRectangle(PdfReader reader, String text, Rectangle rect)
        {

            FilteredTextRenderListener filterListener = new FilteredTextRenderListener(new LocationTextExtractionStrategy(), new RegionTextRenderFilter(rect));

            String extractedText = PdfTextExtractor.GetTextFromPage(reader, 1, filterListener);

            return extractedText.Equals(text);
        }

        private byte[] CreatePdfWithCornerText()
        {
            MemoryStream byteStream = new MemoryStream();

            Document document = new Document(PageSize.LETTER);
            PdfWriter writer = PdfWriter.GetInstance(document, byteStream);
            document.Open();

            PdfContentByte canvas = writer.DirectContent;
            canvas.BeginText();

            canvas.SetFontAndSize(BaseFont.CreateFont(), 12);

            canvas.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Upper Left", 10, document.PageSize.Height - 10 - 12, 0);
            canvas.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Upper Right", document.PageSize.Width - 10, document.PageSize.Height - 10 - 12, 0);
            canvas.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Lower Left", 10, 10, 0);
            canvas.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Lower Right", document.PageSize.Width - 10, 10, 0);

            canvas.EndText();

            document.Close();

            byte[] pdfBytes = byteStream.ToArray();

            return pdfBytes;

        }
    }
}
