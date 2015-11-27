using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.fonts
{
    class EncodingTest {

    private  static String sourceFolder = @"..\..\resources\text\pdf\fonts\EncodingTest/";
    private static String outFolder = "fonts/EncodingTest";

    [SetUp]
    public static void SetUp()  {
        Directory.CreateDirectory("fonts/EncodingTest");
    }

    [Test]
    public void ExoticCharsTimesRomanTest()  {
        String filename = "exoticCharsTimesRomanTest.pdf";
        BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.WINANSI, true);
        Document doc = new Document();

        PdfWriter.GetInstance(doc, new FileStream(outFolder + filename,FileMode.Create));
        doc.Open();
        doc.NewPage();
        Paragraph p = new Paragraph();
        p.Font = new Font(bf);
        p.Add("\u0188");
        doc.Add(p);
        doc.Close();

        new CompareTool().CompareByContent(outFolder + filename, sourceFolder + "cmp_" + filename, outFolder, "diff");
    }

    [Test]
    public void ExoticCharsWithDifferencesTimesRomanTest()  {
        String filename = "exoticCharsWithDifferencesTimesRomanTest.pdf";
        BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, "# simple 32 0020 0188", true);
        Document doc = new Document();

        PdfWriter.GetInstance(doc, new FileStream(outFolder + filename, FileMode.Create));
        doc.Open();
        doc.NewPage();
        Paragraph p = new Paragraph();
        p.Font = new Font(bf);
        p.Add("\u0188");
        doc.Add(p);
        doc.Close();

        new CompareTool().CompareByContent(outFolder + filename, sourceFolder + "cmp_" + filename, outFolder, "diff");
    }

    [Test]
    public void ExoticCharsCourierTest()  {
        String filename = "exoticCharsCourierTest.pdf";
        BaseFont bf = BaseFont.CreateFont(BaseFont.COURIER, BaseFont.WINANSI, true);
        Document doc = new Document();

        PdfWriter.GetInstance(doc, new FileStream(outFolder + filename,FileMode.Create));
        doc.Open();
        doc.NewPage();
        Paragraph p = new Paragraph();
        p.Font = new Font(bf);
        p.Add("\u0188");
        doc.Add(p);
        doc.Close();

        new CompareTool().CompareByContent(outFolder + filename, sourceFolder + "cmp_" + filename, outFolder, "diff");
    }

    [Test]
    public void ExoticCharsWithDifferencesCourierTest()  {
        String filename = "exoticCharsWithDifferencesCourierTest.pdf";
        BaseFont bf = BaseFont.CreateFont(BaseFont.COURIER, "# simple 32 0020 0188", true);
        Document doc = new Document();

        PdfWriter.GetInstance(doc, new FileStream(outFolder + filename,FileMode.Create));
        doc.Open();
        doc.NewPage();
        Paragraph p = new Paragraph();
        p.Font = new Font(bf);
        p.Add("\u0188");
        doc.Add(p);
        doc.Close();

        new CompareTool().CompareByContent(outFolder + filename, sourceFolder + "cmp_" + filename, outFolder, "diff");
    }

    [Test]
    public void SurrogatePairTest()  {
        String filename = "surrogatePairTest.pdf";
        BaseFont bf = BaseFont.CreateFont(sourceFolder + "DejaVuSans.ttf", BaseFont.IDENTITY_H, true);
        Document doc = new Document();

        PdfWriter.GetInstance(doc, new FileStream(outFolder + filename,FileMode.Create));
        doc.Open();
        doc.NewPage();
        Paragraph p = new Paragraph();
        p.Font = new Font(bf);
        p.Add("\uD800\uDF1D");
        doc.Add(p);
        doc.Close();

        new CompareTool().CompareByContent(outFolder + filename, sourceFolder + "cmp_" + filename, outFolder, "diff");
    }

    [Test]
    public void ExoticCharsFreeSansTest()  {
        String filename = "exoticCharsFreeSansTest.pdf";
        BaseFont bf = BaseFont.CreateFont(sourceFolder + "FreeSans.ttf", BaseFont.WINANSI, true);
        Document doc = new Document();

        PdfWriter.GetInstance(doc, new FileStream(outFolder + filename,FileMode.Create));
        doc.Open();
        doc.NewPage();
        Paragraph p = new Paragraph();
        p.Font = new Font(bf);
        p.Add("\u0188");
        doc.Add(p);
        doc.Close();

        new CompareTool().CompareByContent(outFolder + filename, sourceFolder + "cmp_" + filename, outFolder, "diff");
    }

    [Test]
    public void ExoticCharsFreeSansWithDifferencesTest()  {
        String filename = "exoticCharsFreeSansWithDifferencesTest.pdf";
        BaseFont bf = BaseFont.CreateFont(sourceFolder + "FreeSans.ttf", "# simple 32 0020 1031D", true);
        Document doc = new Document();

        PdfWriter.GetInstance(doc, new FileStream(outFolder + filename,FileMode.Create));
        doc.Open();
        doc.NewPage();
        Paragraph p = new Paragraph();
        p.Font = new Font(bf);
        p.Add("\u0188");
        doc.Add(p);
        doc.Close();

        new CompareTool().CompareByContent(outFolder + filename, sourceFolder + "cmp_" + filename, outFolder, "diff");
    }

    [Test]
    public void SymbolDefaultFontTest()  {
        String filename = "symbolDefaultFontTest.pdf";
        BaseFont bf = BaseFont.CreateFont(BaseFont.SYMBOL, BaseFont.WINANSI, true);
        Document doc = new Document();

        PdfWriter.GetInstance(doc, new FileStream(outFolder + filename,FileMode.Create));
        doc.Open();
        doc.NewPage();
        Paragraph p = new Paragraph();
        p.Font = new Font(bf);
        String str = "";
        for (int i = 32; i <=254; i++) {
            str+= (char)i;
        }
        p.Add(str);
        doc.Add(p);
        doc.Close();

        new CompareTool().CompareByContent(outFolder + filename, sourceFolder + "cmp_" + filename, outFolder, "diff");
    }

    [Test]
    public void SymbolFontWinansiTest()  {
        String filename = "symbolFontWinansiTest.pdf";
        BaseFont bf = BaseFont.CreateFont(sourceFolder + "Symbols1.ttf", BaseFont.WINANSI, true);
        Document doc = new Document();

        PdfWriter.GetInstance(doc, new FileStream(outFolder + filename,FileMode.Create));
        doc.Open();
        doc.NewPage();
        Paragraph p = new Paragraph();
        p.Font = new Font(bf);
        String str = "";
        for (int i = 32; i <=254; i++) {
            str+= (char)i;
        }
        p.Add(str);
        doc.Add(p);
        doc.Close();

        new CompareTool().CompareByContent(outFolder + filename, sourceFolder + "cmp_" + filename, outFolder, "diff");
    }

    [Test]
    public void SymbolFontIdentityHTest()  {
        String filename = "symbolFontIdentityHTest.pdf";
        BaseFont bf = BaseFont.CreateFont(sourceFolder + "Symbols1.ttf", BaseFont.IDENTITY_H, true);
        Document doc = new Document();

        PdfWriter.GetInstance(doc, new FileStream(outFolder + filename,FileMode.Create));
        doc.Open();
        doc.NewPage();
        Paragraph p = new Paragraph();
        p.Font = new Font(bf);
        String str = "";
        for (int i = 32; i <=254; i++) {
            str+= (char)i;
        }
        p.Add(str);
        doc.Add(p);
        doc.Close();

        new CompareTool().CompareByContent(outFolder + filename, sourceFolder + "cmp_" + filename, outFolder, "diff");
    }

    }
}
