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
