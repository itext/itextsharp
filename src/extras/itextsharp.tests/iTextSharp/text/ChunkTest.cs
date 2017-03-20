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
/**
 * 
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using iTextSharp.testutils;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;
using iTextSharp.text.io;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;

namespace iTextSharp.text {

    public class ChunkTest {
        private const String _4SPACES = "    4spaces    ";
        private const String _TAB = "\t4spaces    ";

        private const String RESOURCES = @"..\..\resources\text\ChunkTest\";
        private const String TARGET = @"ChunkTest\";

        private const String SOURCE11 = RESOURCES + "source11.pdf";
        private const String SOURCE12 = RESOURCES + "source12.pdf";
        private const String SOURCE13 = RESOURCES + "source13.pdf";
        private const String SOURCE14 = RESOURCES + "source14.pdf";
        private const String SOURCE15 = RESOURCES + "source15.pdf";
        private const String SOURCE16 = RESOURCES + "source16.pdf";
        private const String SOURCE17 = RESOURCES + "source17.pdf";
        private const String OUTTABSPACED = TARGET + "/tabspaceDocument.pdf";
        private const String OUTABSPACEC = TARGET + "/tabspaceColumnText.pdf";
        private const String OUTTABD = TARGET + "/tabDocument.pdf";
        private const String OUTABC = TARGET + "/tabColumnText.pdf";
        private const String OUTABSTOPSC = TARGET + "/tabstopsColumnText.pdf";
        private const String OUTSPTRIMDOC = TARGET + "/spaceTrimDoc.pdf";
        private const String OUTSPTRIMCT = TARGET + "/spaceTrimColumnText.pdf";

        [SetUp]
        virtual public void Init() {
            Directory.CreateDirectory(TARGET);
        }

        [Test]
        virtual public void PrependingWhitspaces() {
            Chunk c = new Chunk(_4SPACES);
            Assert.AreEqual(_4SPACES, c.Content, "difference in string");
        }

        [Test]
        virtual public void PrependingTab() {
            Chunk c = new Chunk(_TAB);
            Assert.AreEqual("4spaces    ", c.Content, "difference in string");
        }

        [Test]
        virtual public void TabspaceDocumentTest() {
            Font f = FontFactory.GetFont(FontFactory.COURIER, 11);
            FileStream fs = new FileStream(OUTTABSPACED, FileMode.Create);
            Document doc = new Document();
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);
            Paragraph p;
            writer.CompressionLevel = 0;
            doc.Open();

            p = new Paragraph(new Chunk("Hello world", f));
            AddTabspaces(p, f, 0);
            doc.Add(p);

            p = new Paragraph(new Chunk("Hello World!!!"));
            AddTabspaces(p, f, 0);
            doc.Add(p);

            f.Size = 16;
            p = new Paragraph(new Chunk("Hello world", f));
            AddTabspaces(p, f, 0);
            doc.Add(p);

            f = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 12);
            p = new Paragraph(new Chunk("Hello world", f));
            AddTabspaces(p, f, 0);
            doc.Add(p);

            f.Size = 20;
            p = new Paragraph(new Chunk("Hello world", f));
            AddTabspaces(p, f, 0);
            doc.Add(p);

            doc.Close();
            fs.Close();
            Assert.IsTrue(CompareInnerText(SOURCE11, OUTTABSPACED));
        }

        [Test]
        virtual public void TabspaceColumnTextTest() {
            Font f = FontFactory.GetFont(FontFactory.COURIER, 11);
            Document doc = new Document();
            Paragraph p;
            FileStream fs = new FileStream(OUTABSPACEC, FileMode.Create);
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);
            writer.CompressionLevel = 0;
            doc.Open();
            ColumnText ct = new ColumnText(writer.DirectContent);
            ct.SetSimpleColumn(36, 36, 436, 800);
            p = new Paragraph(new Chunk("Hello world", f));
            AddTabspaces(p, f, 0);
            ct.AddElement(p);

            p = new Paragraph(new Chunk("Hello World!!!"));
            AddTabspaces(p, f, 0);
            ct.AddElement(p);

            f.Size = 16;
            p = new Paragraph(new Chunk("Hello world", f));
            AddTabspaces(p, f, 0);
            ct.AddElement(p);

            f = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 12);
            p = new Paragraph(new Chunk("Hello world", f));
            AddTabspaces(p, f, 0);
            ct.AddElement(p);

            f.Size = 20;
            p = new Paragraph(new Chunk("Hello world", f));
            AddTabspaces(p, f, 0);
            ct.AddElement(p);
            ct.Go();
            doc.Close();
            fs.Close();
            Assert.IsTrue(CompareInnerText(SOURCE12, OUTABSPACEC));
        }

        [Test]
        virtual public void TabDocumentTest() {
            Font f = FontFactory.GetFont(FontFactory.COURIER, 11);
            FileStream fs = new FileStream(OUTTABD, FileMode.Create);
            Document doc = new Document();
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);
            Paragraph p;
            List<TabStop> tabStopsList = new List<TabStop>();
            tabStopsList.Add(new TabStop(100, new DottedLineSeparator()));
            tabStopsList.Add(new TabStop(200, new LineSeparator(), TabStop.Alignment.CENTER));
            tabStopsList.Add(new TabStop(300, new DottedLineSeparator(), TabStop.Alignment.RIGHT));
            writer.CompressionLevel = 0;
            doc.Open();

            p = new Paragraph(new Chunk("Hello world", f));
            AddTabs(p, f, 0, "la|la");
            p.TabSettings = new TabSettings(tabStopsList, 50);
            doc.Add(p);

            tabStopsList = new List<TabStop>();
            tabStopsList.Add(new TabStop(100, new DottedLineSeparator()));
            tabStopsList.Add(new TabStop(200, new LineSeparator(), TabStop.Alignment.ANCHOR));
            tabStopsList.Add(new TabStop(300, new DottedLineSeparator(), TabStop.Alignment.ANCHOR));
            p = new Paragraph(new Chunk("Hello World!!!"));
            p.TabSettings = new TabSettings(tabStopsList, 50);
            AddTabs(p, f, 12, "l.aal");
            AddTabs(p, f, 12, "laa.l");
            AddTabs(p, f, 12, "la.al");
            doc.Add(p);

            f.Size = 16;
            tabStopsList = new List<TabStop>();
            tabStopsList.Add(new TabStop(100, new DottedLineSeparator()));
            tabStopsList.Add(new TabStop(200, new LineSeparator(), TabStop.Alignment.ANCHOR, ','));
            tabStopsList.Add(new TabStop(300, new DottedLineSeparator(), TabStop.Alignment.ANCHOR));
            p = new Paragraph(new Chunk("Hello world", f));
            p.TabSettings = new TabSettings(tabStopsList, 50);
            AddTabs(p, f, 13, "l.aal");
            AddTabs(p, f, 11, "l,aal");
            AddTabs(p, f, 11, "laa.l");
            AddTabs(p, f, 11, "laa,l");
            doc.Add(p);

            f = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 12);
            p = new Paragraph(new Chunk("Hello world", f));
            p.TabSettings = new TabSettings(38);
            AddTabs(p, f, 0);
            doc.Add(p);

            f.Size = 20;
            p = new Paragraph(new Chunk("Hello world", f));
            p.TabSettings = new TabSettings(38);
            AddTabs(p, f, 0);
            doc.Add(p);

            doc.Close();
            fs.Close();
            Assert.IsTrue(CompareInnerText(SOURCE13, OUTTABD));
        }

        [Test]
        virtual public void TabColumnTextTest() {
            Font f = FontFactory.GetFont(FontFactory.COURIER, 11);
            Document doc = new Document();
            Paragraph p;
            FileStream fs = new FileStream(OUTABC, FileMode.Create);
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);
            writer.CompressionLevel = 0;
            doc.Open();
            ColumnText ct = new ColumnText(writer.DirectContent);
            ct.SetSimpleColumn(36, 36, 436, 800);
            List<TabStop> tabStopsList = new List<TabStop>();
            tabStopsList.Add(new TabStop(100, new DottedLineSeparator()));
            tabStopsList.Add(new TabStop(200, new LineSeparator(), TabStop.Alignment.CENTER));
            tabStopsList.Add(new TabStop(300, new DottedLineSeparator(), TabStop.Alignment.RIGHT));
            p = new Paragraph(new Chunk("Hello world", f));
            p.TabSettings = new TabSettings(tabStopsList, 50);
            AddTabs(p, f, 0, "la|la");
            ct.AddElement(p);

            tabStopsList = new List<TabStop>();
            tabStopsList.Add(new TabStop(100, new DottedLineSeparator()));
            tabStopsList.Add(new TabStop(200, new LineSeparator(), TabStop.Alignment.ANCHOR));
            tabStopsList.Add(new TabStop(300, new DottedLineSeparator(), TabStop.Alignment.ANCHOR));
            p = new Paragraph(new Chunk("Hello World!!!"));
            p.TabSettings = new TabSettings(tabStopsList, 50);
            AddTabs(p, f, 12, "l.aal");
            AddTabs(p, f, 12, "laa.l");
            AddTabs(p, f, 12, "la.al");
            ct.AddElement(p);

            f.Size = 16;
            p = new Paragraph(new Chunk("Hello world", f));
            tabStopsList = new List<TabStop>();
            tabStopsList.Add(new TabStop(100, new DottedLineSeparator()));
            tabStopsList.Add(new TabStop(200, new LineSeparator(), TabStop.Alignment.ANCHOR, ','));
            tabStopsList.Add(new TabStop(300, new DottedLineSeparator(), TabStop.Alignment.ANCHOR));
            p.TabSettings = new TabSettings(tabStopsList, 50);
            AddTabs(p, f, 15, "l.aal");
            AddTabs(p, f, 13, "laa,l");
            AddTabs(p, f, 13, "laa.l");
            AddTabs(p, f, 13, "l,aal");
            ct.AddElement(p);

            f = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 12);
            p = new Paragraph(new Chunk("Hello world", f));
            p.TabSettings = new TabSettings(38);
            AddTabs(p, f, 0);
            ct.AddElement(p);

            f.Size = 20;
            p = new Paragraph(new Chunk("Hello world", f));
            p.TabSettings = new TabSettings(38);
            AddTabs(p, f, 0);
            ct.AddElement(p);
            ct.Go();
            doc.Close();
            fs.Close();
            Assert.IsTrue(CompareInnerText(SOURCE14, OUTABC));

        }

        [Test]
        virtual public void TabStopsColumnText() {
            Document document = new Document(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(OUTABSTOPSC, FileMode.Create));
            document.Open();

            Font oFont1 = FontFactory.GetFont(BaseFont.ZAPFDINGBATS, 15, Font.UNDEFINED);
            Font oFont2 = FontFactory.GetFont(BaseFont.COURIER, 15, Font.UNDEFINED);
            Font oFont3 = FontFactory.GetFont(BaseFont.TIMES_ROMAN, 15, Font.UNDEFINED);
            Font oFont4 = FontFactory.GetFont(BaseFont.HELVETICA, 15, Font.UNDEFINED);
            Image oImg = Image.GetInstance(RESOURCES + "logo.gif");
            PdfContentByte canvas = writer.DirectContentUnder;

            List<TabStop> tabStops = new List<TabStop>();
            //tabStops.Add(new TabStop(100, new DottedLineSeparator()));
            //tabStops.Add(new TabStop(200, new DottedLineSeparator()));
            tabStops.Add(new TabStop(200, new DottedLineSeparator()));
            //tabStops.Add(new TabStop(300, new DottedLineSeparator()));
            tabStops.Add(new TabStop(400, new DottedLineSeparator()));
            //tabStops.Add(new TabStop(500, new DottedLineSeparator()));
            //tabStops.Add(new TabStop(550, new DottedLineSeparator()));

            Paragraph oPara = new Paragraph("Hello World! ", oFont1);
            oPara.TabSettings = new TabSettings(tabStops);
            oPara.Add(new Chunk("iText ® is a library that allows you to create and manipulate PDF documents.", oFont2));
            oPara.Add(
                new Chunk("It enables developers looking to enhance web- and other applications with dynamic PDF docu",
                          oFont3));
            oPara.Add(Chunk.TABBING);
            oPara.Add(new Chunk("ment generation and/or manipulation.", oFont3));
            oPara.Add(new Chunk(oImg, 0, 0, true));
            //oPara.Add(new Chunk(new TestVerticalPositionMark()));
            oPara.Add(Chunk.TABBING);
            oPara.Add(new Chunk("Developers can use iText to:", oFont4));

            ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, new Phrase("|100"), 100, 500, 0);
            ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, new Phrase("|200"), 200, 500, 0);
            ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, new Phrase("|250"), 250, 500, 0);
            ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, new Phrase("|300"), 300, 500, 0);
            ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, new Phrase("|400"), 400, 500, 0);
            ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, new Phrase("|500"), 500, 500, 0);
            ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, new Phrase("|550"), 550, 500, 0);
            ColumnText oColTxt1 = new ColumnText(canvas);

            oColTxt1.SetSimpleColumn(0, 400, 595, 500);
            oColTxt1.AddElement(oPara);
            oColTxt1.Go();

            document.Close();
            Assert.IsTrue(CompareInnerText(SOURCE15, OUTABSTOPSC));
        }

        [Test]
        public virtual void SpaceTrimPdfDocumentTest() {
            Document doc = new Document(PageSize.A4, 50, 30, 50, 30);
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(OUTSPTRIMDOC, FileMode.Create));
            doc.Open();

            Phrase under = new Phrase();
            under.Font = new Font(Font.FontFamily.TIMES_ROMAN, 12, Font.UNDERLINE);
            under.Add(new Chunk(" 1                                                      1                                                                                                                             9      "));

            doc.Add(under);

            doc.Close();
            writer.Close();

            PdfReader reader = new PdfReader(OUTSPTRIMDOC);
            MyTextRenderListener listener = new MyTextRenderListener();
            PdfContentStreamProcessor processor = new PdfContentStreamProcessor(listener);
            PdfDictionary pageDic = reader.GetPageN(1);
            PdfDictionary resourcesDic = pageDic.GetAsDict(PdfName.RESOURCES);
            processor.ProcessContent(ContentByteUtils.GetContentBytesForPage(reader, 1), resourcesDic);
            //should be 60, as in @spaceTrimColumnTextTest
            //Assert.assertTrue("Unexpected text length", listener.getText().length() == 60);
            Assert.IsTrue(listener.GetText().Length == 77, "Unexpected text length");
        }

        [Test]
        public virtual void SpaceTrimColumnTextTest() {
            Document doc = new Document(PageSize.A4, 50, 30, 50, 30);
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(OUTSPTRIMCT, FileMode.Create));
            doc.Open();

            Phrase under = new Phrase();
            under.Font = new Font(Font.FontFamily.TIMES_ROMAN, 12, Font.UNDERLINE);
            under.Add(new Chunk(" 1                                                      1                                                                                                                             9      "));

            Paragraph underlineTest = new Paragraph(under);
            underlineTest.KeepTogether = true;
            doc.Add(underlineTest);

            doc.Close();
            writer.Close();

            PdfReader reader = new PdfReader(OUTSPTRIMCT);
            MyTextRenderListener listener = new MyTextRenderListener();
            PdfContentStreamProcessor processor = new PdfContentStreamProcessor(listener);
            PdfDictionary pageDic = reader.GetPageN(1);
            PdfDictionary resourcesDic = pageDic.GetAsDict(PdfName.RESOURCES);
            processor.ProcessContent(ContentByteUtils.GetContentBytesForPage(reader, 1), resourcesDic);
            Assert.IsTrue(listener.GetText().Length == 60, "Unexpected text length");
        }

        [Test]
        public virtual void TabStopOutOfPageBoundDocumentTest() {
            Document doc = new Document(PageSize.A4, 36, 36, 0, 30);
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(TARGET + "/tabStopOutDocument.pdf", FileMode.Create));
            doc.Open();
            Font f = FontFactory.GetFont(FontFactory.COURIER, 11);
            f.Size = 16;
            Paragraph p = new Paragraph(Chunk.TABBING);
            p.Add(new Chunk("Hello world", f));
            List<TabStop> tabStopsList = new List<TabStop>();
            tabStopsList.Add(new TabStop(1000, new DottedLineSeparator()));
            tabStopsList.Add(new TabStop(1050, new LineSeparator(), TabStop.Alignment.ANCHOR, ','));
            tabStopsList.Add(new TabStop(1100, new DottedLineSeparator(), TabStop.Alignment.ANCHOR));
            p.TabSettings = new TabSettings(tabStopsList, 50);
            AddTabs(p, f, 15, "l.aal");
            AddTabs(p, f, 13, "laa,l");
            AddTabs(p, f, 13, "laa.l");
            AddTabs(p, f, 13, "l,aal");

            doc.Add(p);
            doc.Close();
            writer.Close();
            Assert.IsTrue(CompareInnerText(SOURCE16, TARGET + "/tabStopOutDocument.pdf"));
        }

        [Test]
        public void TabStopOutOfPageBoundColumnTextTest() {
            Font f = FontFactory.GetFont(FontFactory.COURIER, 11);
            Document doc = new Document();
            Paragraph p;
            FileStream fs = new FileStream(TARGET + "/tabStopOutColumnText.pdf", FileMode.Create);
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);
            writer.CompressionLevel = 0;
            doc.Open();
            ColumnText ct = new ColumnText(writer.DirectContent);
            ct.SetSimpleColumn(36, 0, 436, 836);
            f.Size = 16;
            p = new Paragraph(Chunk.TABBING);
            p.Add(new Chunk("Hello world", f));
            List<TabStop> tabStopsList = new List<TabStop>();
            tabStopsList.Add(new TabStop(1000, new DottedLineSeparator()));
            tabStopsList.Add(new TabStop(1050, new LineSeparator(), TabStop.Alignment.ANCHOR, ','));
            tabStopsList.Add(new TabStop(1100, new DottedLineSeparator(), TabStop.Alignment.ANCHOR));
            p.TabSettings = new TabSettings(tabStopsList, 50);
            AddTabs(p, f, 15, "l.aal");
            AddTabs(p, f, 13, "laa,l");
            AddTabs(p, f, 13, "laa.l");
            AddTabs(p, f, 13, "l,aal");
            ct.AddElement(p);
            ct.Go();
            doc.Close();
            writer.Close();
            Assert.IsTrue(CompareInnerText(SOURCE17, TARGET + "/tabStopOutColumnText.pdf"));
        }

        private class MyTextRenderListener : IRenderListener {
            protected StringBuilder buffer = new StringBuilder();

            public void BeginTextBlock() {
            }

            public void EndTextBlock() {
            }

            public void RenderImage(ImageRenderInfo renderInfo) {
            }

            public void RenderText(TextRenderInfo renderInfo) {
                buffer.Append(renderInfo.GetText());
                buffer.Append("\n");
            }

            public String GetText() {
                return buffer.ToString();
            }
        }

        virtual public void AddTabspaces(Paragraph p, Font f, int count) {
            p.Add(Chunk.CreateTabspace());
            p.Add(new Chunk("|", f));
            if (count == 16)
                return;
            else
                AddTabspaces(p, f, count + 1);
        }

        virtual public void AddTabs(Paragraph p, Font f, int count, String text) {
            p.Add(Chunk.TABBING);
            p.Add(new Chunk(text, f));
            if (count == 17)
                return;
            else
                AddTabs(p, f, count + 1, text);
        }

        virtual public void AddTabs(Paragraph p, Font f, int count) {
            p.Add(Chunk.TABBING);
            p.Add(new Chunk("|", f));
            if (count == 17)
                return;
            else
                AddTabs(p, f, count + 1);
        }

        virtual public bool CompareInnerText(String path1, String path2) {
            PdfReader reader1 = new PdfReader(path1);
            byte[] streamBytes1 = reader1.GetPageContent(1);
            PRTokeniser tokenizer1 =
                new PRTokeniser(new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource(streamBytes1)));



            PdfReader reader2 = new PdfReader(path2);
            byte[] streamBytes2 = reader2.GetPageContent(1);
            PRTokeniser tokenizer2 =
                new PRTokeniser(new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource(streamBytes2)));

            try {
                while (tokenizer1.NextToken()) {
                    if (!tokenizer2.NextToken())
                        return false;
                    else {
                        if (tokenizer1.TokenType != tokenizer2.TokenType)
                            return false;
                        else {
                            if (tokenizer1.TokenType == tokenizer2.TokenType && tokenizer2.TokenType == PRTokeniser.TokType.NUMBER) {
                                if (Math.Abs(float.Parse(tokenizer1.StringValue, CultureInfo.InvariantCulture)
                                             - float.Parse(tokenizer2.StringValue, CultureInfo.InvariantCulture)) > 0.001)
                                    return false;
                            } else if (!tokenizer1.StringValue.Equals(tokenizer2.StringValue))
                                return false;
                        }

                    }
                }
                return true;
            }
            finally {
                reader1.Close();
                reader2.Close();
            }
        }

        [Test]
        public void TestImageChunkOnGenericTag() {
            String fileName = "testImageChunkOnGenericTag.pdf";
            FileStream fos = new FileStream(TARGET + fileName, FileMode.Create);
            Document doc = new Document(PageSize.LETTER);
            PdfWriter writer = PdfWriter.GetInstance(doc, fos);
            writer.PageEvent = new EventHandler();
            doc.Open();

            Image img = Image.GetInstance(writer.DirectContent.CreateTemplate(100f, 25f));
            Console.WriteLine(img.Height.ToString("F1", CultureInfo.InvariantCulture));
            Chunk c = new Chunk(img, 0, 0);
            c.SetGenericTag("foobar");

            doc.Add(c);
            doc.Close();

            CompareTool compareTool = new CompareTool();
            String error = compareTool.CompareByContent(TARGET + fileName, RESOURCES + "cmp_" + fileName, TARGET, "diff_");
            if (error != null) {
                Assert.Fail(error);
            }
        }

        private class EventHandler : PdfPageEventHelper {
            public override void OnGenericTag(PdfWriter writer, Document document, Rectangle rect, String text) {
                PdfContentByte cb = writer.DirectContent;
                cb.SaveState();
                cb.SetColorStroke(BaseColor.BLACK);
                cb.Rectangle(rect.Left, rect.Bottom, rect.Width, rect.Height);
                cb.Stroke();
                cb.RestoreState();

                writer.DirectContent.BeginText();
                Font f = FontFactory.GetFont(BaseFont.COURIER, 8f);
                writer.DirectContent.SetFontAndSize(f.BaseFont, 8f);
                writer.DirectContent.ShowTextAligned(Element.ALIGN_LEFT,
                    rect.Height.ToString("F1", CultureInfo.InvariantCulture), rect.Left, rect.Bottom, 0);
                writer.DirectContent.EndText();
            }
        }
    }
}

