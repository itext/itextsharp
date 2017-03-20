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
using System.IO;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    public class PageEventTest {
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\PageEventTest\";
        private const string OUTPUT_FOLDER = @"PageEventTest\";

        [SetUp]
        public static void Init() {
            Directory.CreateDirectory(OUTPUT_FOLDER);
        }

        [Test]
        public void PageEventTest01() {
            String fileName = "pageEventTest01.pdf";

            MemoryStream baos = new MemoryStream();
            Document doc = new Document(PageSize.LETTER, 144, 144, 144, 144);
            PdfWriter writer = PdfWriter.GetInstance(doc, baos);
            writer.PageEvent = new MyEventHandler();

            writer.SetTagged();
            doc.Open();

            Chunk c = new Chunk("This is page 1");
            doc.Add(c);

            doc.Close();

            File.WriteAllBytes(OUTPUT_FOLDER + fileName, baos.ToArray());
            baos.Close();

            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER + fileName, TEST_RESOURCES_PATH + fileName, OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        private class MyEventHandler : PdfPageEventHelper {
            private PdfPTable _header;
            private PdfPTable _footer;

            public MyEventHandler() {
                _header = new PdfPTable(1);
                PdfPCell hCell = new PdfPCell(new Phrase("HEADER"));
                hCell.Border = Rectangle.NO_BORDER;
                _header.AddCell(hCell);
                _header.SetTotalWidth(new float[]{300f});

                _footer = new PdfPTable(1);
                PdfPCell fCell = new PdfPCell(new Phrase("FOOTER"));
                fCell.Border = Rectangle.NO_BORDER;
                _footer.AddCell(fCell);
                _footer.SetTotalWidth(new float[]{300f});
            }

            public override void OnStartPage(PdfWriter writer, Document document) {
                base.OnStartPage(writer, document);
                WriteHeader(writer);
            }

            public override void OnEndPage(PdfWriter writer, Document document) {
                base.OnEndPage(writer, document);
                WriteFooter(writer);
            }

            private void WriteHeader(PdfWriter writer) {
                writer.DirectContent.SaveState();
                _header.WriteSelectedRows(0, _header.Rows.Count, 72, writer.PageSize.Height - 72, writer.DirectContent);
                writer.DirectContent.RestoreState();
            }

            private void WriteFooter(PdfWriter writer) {
                writer.DirectContent.SaveState();
                _footer.WriteSelectedRows(0, _footer.Rows.Count, 72, 72, writer.DirectContent);
                writer.DirectContent.RestoreState();
            }
        }
    }
}
