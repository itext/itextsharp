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
using List = iTextSharp.text.List;

namespace itextsharp.tests.iTextSharp.text.pdf {
public class NestedListInColumnTextTest {
    public const String DEST_FOLDER = @"NestedListInColumnTextTest\";
    public const String SOURCE_FOLDER = @"..\..\resources\text\pdf\NestedListInColumnTextTest\"; 

    [SetUp]
    public void Init() {
        if (Directory.Exists(DEST_FOLDER))
            Directory.Delete(DEST_FOLDER, true);

        Directory.CreateDirectory(DEST_FOLDER);
    }

    //SUP-879 Nested List items not displaying properly
    [Test]
    public void NestedListAtTheEndOfAnotherNestedList() {
        String pdfFile = "nestedListAtTheEndOfAnotherNestedList.pdf";
        // step 1
        Document document = new Document();
        // step 2
        PdfWriter.GetInstance(document, File.Create(DEST_FOLDER + pdfFile));
        // step 3
        document.Open();
        // step 4
        PdfPTable table = new PdfPTable(1);
        PdfPCell cell = new PdfPCell();
        cell.BackgroundColor = BaseColor.ORANGE;

        RomanList romanlist = new RomanList(true, 20);
        romanlist.IndentationLeft = 10f;
        romanlist.Add("One");
        romanlist.Add("Two");
        romanlist.Add("Three");

        RomanList romanlist2 = new RomanList(true, 20);
        romanlist2.IndentationLeft = 10f;
        romanlist2.Add("One");
        romanlist2.Add("Two");
        romanlist2.Add("Three");

        romanlist.Add(romanlist2);
        //romanlist.add("Four");

        List list = new List(List.ORDERED, 20f);
        list.SetListSymbol("\u2022");
        list.IndentationLeft = 20f;
        list.Add("One");
        list.Add("Two");
        list.Add("Three");
        list.Add("Four");
        list.Add("Roman List");
        list.Add(romanlist);

        list.Add("Five");
        list.Add("Six");

        cell.AddElement(list);
        table.AddCell(cell);
        document.Add(table);
        // step 5
        document.Close();

        CompareTool compareTool = new CompareTool();
        String error = compareTool.CompareByContent(DEST_FOLDER + pdfFile, SOURCE_FOLDER + pdfFile, DEST_FOLDER, "diff_");
        if (error != null) {
            Assert.Fail(error);
        }
    }

}
}
