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
using System.Text;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    class PdfStamperTest {
        static  string DestFolder = "com/itextpdf/test/pdf/PdfStamperTest/";
        private string TestResourcesPath = @"..\..\resources\text\pdf\PdfStamperTest\";

        [SetUp]
        public void SetUp() {
            Directory.CreateDirectory(DestFolder);
        }

        [Test]
        public void SetPageContentTest01()  {
            String outPdf = DestFolder + "out1.pdf";
            PdfReader reader =
                new PdfReader(TestResourceUtils.GetResourceAsStream(TestResourcesPath, "in.pdf"));
            PdfStamper stamper = new PdfStamper(reader, new FileStream(outPdf, FileMode.Create));
            reader.EliminateSharedStreams();
            int total = reader.NumberOfPages + 1;
            for (int i = 1; i < total; i++) {
                byte[] bb = reader.GetPageContent(i);
                reader.SetPageContent(i, bb);
            }
            stamper.Close();

            Assert.Null(new CompareTool().CompareByContent(outPdf, TestResourceUtils.GetResourceAsTempFile(TestResourcesPath, "cmp_out1.pdf"), DestFolder, "diff_"));
        }

        [Test]
        public void SetPageContentTest02()  {
            String outPdf = DestFolder + "out2.pdf";
            PdfReader reader = new PdfReader(TestResourceUtils.GetResourceAsStream(TestResourcesPath, "in.pdf"));
            PdfStamper stamper = new PdfStamper(reader, new FileStream(outPdf, FileMode.Create));
            int total = reader.NumberOfPages + 1;
            for (int i = 1; i < total; i++) {
                byte[] bb = reader.GetPageContent(i);
                reader.SetPageContent(i, bb);
            }
            reader.RemoveUnusedObjects();
            stamper.Close();

            String s = new CompareTool().CompareByContent(outPdf, TestResourceUtils.GetResourceAsTempFile(TestResourcesPath, "cmp_out2.pdf"), DestFolder, "diff_");
            Assert.Null(s);
        }

        [Test]
        public void LayerStampingTest() {
            String outPdf = DestFolder + "out3.pdf";
            PdfReader reader =
                new PdfReader(TestResourceUtils.GetResourceAsStream(TestResourcesPath, "House_Plan_Final.pdf"));
            PdfStamper stamper = new PdfStamper(reader, File.Create(outPdf));

            PdfLayer logoLayer = new PdfLayer("Logos", stamper.Writer);
            PdfContentByte cb = stamper.GetUnderContent(1);
            cb.BeginLayer(logoLayer);

            Image iImage = Image.GetInstance(TestResourceUtils.GetResourceAsStream(TestResourcesPath, "Willi-1.jpg"));
            iImage.ScalePercent(24f);
            iImage.SetAbsolutePosition(100, 100);
            cb.AddImage(iImage);

            cb.EndLayer();
            stamper.Close();

            Assert.Null(new CompareTool().CompareByContent(outPdf, TestResourceUtils.GetResourceAsTempFile(TestResourcesPath, "cmp_House_Plan_Final.pdf"), DestFolder, "diff_"));
        }


    }
}
