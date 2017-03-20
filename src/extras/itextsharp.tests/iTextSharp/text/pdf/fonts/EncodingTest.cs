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
