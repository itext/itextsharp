/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General Public License version 3 as published by the Free Software Foundation with the addition of the following permission added to Section 15 as permitted in Section 7(a): FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY iText Group NV, iText Group NV DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License along with this program; if not, see http://www.gnu.org/licenses or write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA, 02110-1301 USA, or download the license from the following URL:

http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions of this program must display Appropriate Legal Notices, as required under Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License, a covered work must retain the producer line in every PDF that is created or manipulated using iText.

You can be released from the requirements of the license by purchasing a commercial license. Buying such a license is mandatory as soon as you develop commercial activities involving the iText software without disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP, serving PDFs on the fly in a web application, shipping iText with a closed source product.

For more information, please contact iText Software Corp. at this address: sales@itextpdf.com */
using System;
using System.IO;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    public class BarcodeMacroPDF417Test : ITextTest {
        private const String CMP_DIR = @"..\..\resources\text\pdf\BarcodeMacroPDF417Test\";
        private const String OUT_DIR = "com/itextpdf/test/pdf/BarcodeMacroPDF417Test/";


        [SetUp]
        public void SetUp() {
            Directory.CreateDirectory(OUT_DIR);
        }


        protected override void MakePdf(String outPdf) {
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outPdf, FileMode.Create));
            document.Open();
            PdfContentByte cb = writer.DirectContent;
            Image img = CreateBarcode(cb, "This is PDF417 segment 0", 1, 1, 0);
            document.Add(new Paragraph("This is PDF417 segment 0"));
            document.Add(img);

            document.Add(new Paragraph("\u00a0"));
            document.Add(new Paragraph("\u00a0"));
            document.Add(new Paragraph("\u00a0"));
            document.Add(new Paragraph("\u00a0"));
            document.Add(new Paragraph("\u00a0"));
            document.Add(new Paragraph("\u00a0"));
            document.Add(new Paragraph("\u00a0"));
            document.Add(new Paragraph("\u00a0"));

            img = CreateBarcode(cb, "This is PDF417 segment 1", 1, 1, 1);
            document.Add(new Paragraph("This is PDF417 segment 1"));
            document.Add(img);
            document.Close();
        }

        public Image CreateBarcode(PdfContentByte cb, String text, float mh, float mw, int segmentId) {
            BarcodePDF417 pf = new BarcodePDF417();

            // MacroPDF417 setup
            pf.Options = BarcodePDF417.PDF417_USE_MACRO;
            pf.MacroFileId = "12";
            pf.MacroSegmentCount = 2;
            pf.MacroSegmentId = segmentId;

            pf.SetText(text);
            Rectangle size = pf.GetBarcodeSize();
            PdfTemplate template = cb.CreateTemplate(mw * size.Width, mh * size.Height);
            pf.PlaceBarcode(template, BaseColor.BLACK, mh, mw);
            return Image.GetInstance(template);
        }

        [Test]
        public void Test() {
            RunTest();
        }

        protected override void ComparePdf(String outPdf, String cmpPdf) {
            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(outPdf, cmpPdf, OUT_DIR, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        protected override String GetOutPdf() {
            return OUT_DIR + "barcode_macro_pdf417.pdf";
        }


        protected override String GetCmpPdf() {
            return CMP_DIR + "cmp_barcode_macro_pdf417.pdf";
        }
    }
}
