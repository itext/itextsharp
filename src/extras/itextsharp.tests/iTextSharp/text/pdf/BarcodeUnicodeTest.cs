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
using System.IO;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.qrcode;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    public class BarcodeUnicodeTest : ITextTest {
        private const String OUT_DIR = "com/itextpdf/test/pdf/BarcodeUnicodeTest/";

        [SetUp]
        public void SetUp() {
            Directory.CreateDirectory(OUT_DIR);
        }

        protected override void MakePdf(string outPdf) {
            // step 1
            Document document = new Document(new Rectangle(340, 842));
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outPdf, FileMode.Create));
            // step 3
            document.Open();
            // step 4
            PdfContentByte cb = writer.DirectContent;

            String str = "\u6D4B";

            document.Add(new Paragraph("QR code unicode"));
            IDictionary<EncodeHintType, Object> hints = new Dictionary<EncodeHintType, object>();
            hints[EncodeHintType.CHARACTER_SET] = "UTF-8";
            BarcodeQRCode q = new BarcodeQRCode(str, 100, 100, hints);
            document.Add(q.GetImage());

            // step 5
            document.Close();
        }

        [Test]
        public virtual void Test() {
            RunTest();
        }

        protected override string GetOutPdf() {
            return OUT_DIR + "barcode.pdf";
        }
    }
}
