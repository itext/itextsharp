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
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    public class DocumentLayoutTest {
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\DocumentLayoutTest\";
        private const string OUTPUT_FOLDER = @"DocumentLayoutTest\";

        [SetUp]
        public static void Init() {
            Directory.CreateDirectory(OUTPUT_FOLDER);
        }

        [Test]
        public void TextLeadingTest() {
            String file = "phrases.pdf";

            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, File.Create(OUTPUT_FOLDER + file));
            document.Open();

            Phrase p1 = new Phrase("first, leading of 150");
            p1.Leading = 150;
            document.Add(p1);
            document.Add(Chunk.NEWLINE);

            Phrase p2 = new Phrase("second, leading of 500");
            p2.Leading = 500;
            document.Add(p2);
            document.Add(Chunk.NEWLINE);

            Phrase p3 = new Phrase();
            p3.Add(new Chunk("third, leading of 20"));
            p3.Leading = 20;
            document.Add(p3);

            document.Close();

            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER + file, TEST_RESOURCES_PATH + file, OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }
    
        [Test]
        public void WaitingImageTest() {
            String file = "waitingImage.pdf";

            Document document = new Document();
            PdfWriter.GetInstance(document, File.Create(OUTPUT_FOLDER + file));
            document.Open();

            String longTextString = "asdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddas" +
                                    "asdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddas" +
                                    "asdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddas" +
                                    "asdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddas" +
                                    "asdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddas" +
                                    "asdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddas" +
                                    "asdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddasasdsaddsdadasddas";
            String extraLongTextString = longTextString + longTextString;
            document.Add(new Paragraph(extraLongTextString));
            String imageFile = "Desert.jpg";
            document.Add(Image.GetInstance(TEST_RESOURCES_PATH + imageFile));

            document.Close();

            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER + file, TEST_RESOURCES_PATH + file, OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }
    }
}
