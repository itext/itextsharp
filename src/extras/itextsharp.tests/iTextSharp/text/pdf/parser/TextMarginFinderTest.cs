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
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser
{
    class TextMarginFinderTest
    {
        [Test]
        virtual public void TestBasics()
        {
            Rectangle rToDraw = new Rectangle(1.42f * 72f, 2.42f * 72f, 7.42f * 72f, 10.42f * 72f);
            rToDraw.Border = Rectangle.BOX;
            rToDraw.BorderWidth = 1.0f;

            byte[] content = CreatePdf(rToDraw);
            //TestResourceUtils.openBytesAsPdf(content);

            TextMarginFinder finder = new TextMarginFinder();

            new PdfReaderContentParser(new PdfReader(content)).ProcessContent(1, finder);

            Assert.AreEqual(1.42f * 72f, finder.GetLlx(), 0.01f);
            Assert.AreEqual(7.42f * 72f, finder.GetUrx(), 0.01f);
            Assert.AreEqual(2.42f * 72f, finder.GetLly(), 0.01f);
            Assert.AreEqual(10.42f * 72f, finder.GetUry(), 0.01f);

        }

        private byte[] CreatePdf(Rectangle recToDraw)
        {
            MemoryStream baos = new MemoryStream();
            Document doc = new Document();
            PdfWriter writer = PdfWriter.GetInstance(doc, baos);
            writer.CompressionLevel = 0;
            doc.Open();


            PdfContentByte canvas = writer.DirectContent;
            canvas.BeginText();
            float fontsiz = 12;

            float llx = 1.42f * 72f;
            float lly = 2.42f * 72f;
            float urx = 7.42f * 72f;
            float ury = 10.42f * 72f;

            BaseFont font = BaseFont.CreateFont();
            canvas.SetFontAndSize(font, fontsiz);

            float ascent = font.GetFontDescriptor(BaseFont.ASCENT, fontsiz);
            float descent = font.GetFontDescriptor(BaseFont.DESCENT, fontsiz);

            canvas.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "LowerLeft", llx, lly - descent, 0.0f);
            canvas.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "LowerRight", urx, lly - descent, 0.0f);
            canvas.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "UpperLeft", llx, ury - ascent, 0.0f);
            canvas.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "UpperRight", urx, ury - ascent, 0.0f);
            canvas.EndText();

            if (recToDraw != null)
            {
                doc.Add(recToDraw);
            }

            doc.Close();

            return baos.ToArray();
        }
    }
}
