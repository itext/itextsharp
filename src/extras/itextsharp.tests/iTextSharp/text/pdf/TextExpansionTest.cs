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
using System.Text;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class TextExpansionTest
    {
        private const string SOURCE_FOLDER = @"..\..\resources\text\pdf\TextExpansionTest\";
        private const string DEST_FOLDER = @"TextExpansionTest\";

        [SetUp]
        public virtual void SetUp()
        {
            TestResourceUtils.PurgeTempFiles();
            Directory.CreateDirectory(DEST_FOLDER);
        }

        [Test]
        public void ImageTaggingExpansionTest() {
            String filename = "TextExpansionTest.pdf";
            Document doc = new Document(PageSize.LETTER, 72, 72, 72, 72);
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(DEST_FOLDER + filename, FileMode.Create));
            writer.SetTagged();
            doc.Open();

            Chunk c1 = new Chunk("ABC");
            c1.SetTextExpansion("the alphabet");
            Paragraph p1 = new Paragraph();
            p1.Add(c1);
            doc.Add(p1);

            PdfTemplate t = writer.DirectContent.CreateTemplate(6, 6);
            t.SetLineWidth(1f);
            t.Circle(3f, 3f, 1.5f);
            t.SetGrayFill(0);
            t.FillStroke();
            Image i = Image.GetInstance(t);
            Chunk c2 = new Chunk(i, 100, -100);
            doc.Add(c2);

            Chunk c3 = new Chunk("foobar");
            c3.SetTextExpansion("bar bar bar");
            Paragraph p3 = new Paragraph();
            p3.Add(c3);
            doc.Add(p3);

            doc.Close();


            CompareTool compareTool = new CompareTool();
            String error = compareTool.CompareByContent(DEST_FOLDER + filename, SOURCE_FOLDER + "cmp_" + filename, DEST_FOLDER, "diff_");
            if (error != null) {
                Assert.Fail(error);
            }
        }
    }
}
