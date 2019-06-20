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
using System.Diagnostics;
using System.IO;
using iTextSharp.testutils;
using Microsoft.XmlDiffPatch;
using NUnit.Framework;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.xml;
using List = iTextSharp.text.List;

namespace itextsharp.tests.text.pdf {

    public class TaggedPdfTest {

        private const String text = "Lorem ipsum dolor sit amet," +
                                    "consectetur adipiscing elit." +
                                    "Pellentesque a lectus sit amet lectus accumsan aliquam." +
                                    "Quisque facilisis ullamcorper dolor, quis gravida leo faucibus in. Donec a dolor ligula, quis placerat nunc. Etiam enim velit, egestas in lacinia at, ultricies eu massa." +
                                    "Cras ornare felis id quam vehicula lobortis. Ut semper malesuada nulla, in vulputate dui eleifend at. Phasellus pulvinar nisl a lorem volutpat pellentesque. In vitae" +
                                    "ligula et quam vestibulum iaculis eget vitae massa. Fusce vitae leo ut diam suscipit dictum in id sapien. Praesent mi ligula, auctor vitae ultrices in, venenatis non" +
                                    "odio. Nullam sit amet velit pellentesque lectus consectetur lacinia nec quis mi. In hac habitasse platea dictumst." +
                                    "Quisque facilisis ullamcorper dolor, quis gravida leo faucibus in." +
                                    "Donec a dolor ligula, quis placerat nunc.\n" +
                                    "1. Etiam enim velit, egestas in lacinia at, ultricies eu massa. Cras ornare felis id quam vehicula lobortis. Ut semper malesuada nulla, in vulputate dui eleifend at." +
                                    "Phasellus pulvinar nisl a lorem volutpat pellentesque. In vitae ligula et quam vestibulum iaculis eget vitae massa. Fusce vitae leo ut diam suscipit dictum in id" +
                                    "sapien. Praesent mi ligula, auctor vitae ultrices in, venenatis non odio. Nullam sit amet velit pellentesque lectus consectetur lacinia nec quis mi. In hac" +
                                    "habitasse platea dictumst.\n" +
                                    "2. Morbi euismod, nunc quis malesuada feugiat, dui nibh rhoncus leo, quis cursus erat tellus vel tortor. Mauris nibh dolor, iaculis et pharetra pretium," +
                                    "pellentesque vitae erat. Aenean enim nisi, euismod quis ultricies vel, convallis nec nulla. Suspendisse nisl purus, molestie et egestas ac, cursus in mauris." +
                                    "Aliquam erat volutpat. Donec at nulla in elit faucibus mollis ac vel enim. Nullam dapibus dui sit amet sem consectetur ac vulputate est sagittis. Aliquam luctus" +
                                    "ornare nulla. Mauris adipiscing congue pharetra. Proin tempus, nibh sed pretium tempor, arcu est hendrerit est, et dignissim odio leo non purus." +
                                    "Suspendisse non elit massa. Vestibulum tincidunt ipsum vitae dui congue sagittis. Aenean porttitor tristique euismod. Nulla id justo in quam imperdiet" +
                                    "facilisis ut non turpis. Lorem ipsum dolor sit amet, consectetur adipiscing elit.\n" +
                                    "3. Aliquam non elit ligula, nec hendrerit urna. Mauris ut velit sapien. Sed in convallis diam. Nulla faucibus, purus a porttitor ultrices, est quam convallis magna," +
                                    "molestie aliquam sapien nulla eget metus. Integer nec enim mi, eu mattis massa. Integer quis sapien vel purus pretium ullamcorper ac id dui. Suspendisse" +
                                    "pellentesque tellus sit amet neque pulvinar egestas lacinia diam imperdiet.\n" +
                                    "4. Curabitur hendrerit, sem et facilisis vestibulum, massa felis vestibulum ligula, ut faucibus massa nisi in neque. Nulla facilisi. Etiam diam mauris, pellentesque" +
                                    "lacinia dapibus at, lobortis non quam. Nullam et neque quis diam vestibulum scelerisque ullamcorper non mauris. Cras massa enim, commodo malesuada" +
                                    "tincidunt ac, lobortis eu erat. Sed sed risus velit. Suspendisse tellus tortor, ullamcorper nec tristique ac, semper non nulla. Maecenas vitae diam orci, sed" +
                                    "fermentum enim. Curabitur a libero nisl, vel laoreet nulla. Integer id volutpat sem. Pellentesque blandit, tellus at consequat dictum, urna sem elementum nisi," +
                                    "a bibendum nisi ipsum sit amet felis. Donec mattis ipsum nec metus lobortis eget volutpat nisl volutpat.\n" +
                                    "5. Fusce in aliquet nibh. Etiam quis varius ipsum. Vivamus sit amet mauris a libero iaculis semper in a neque. Nam faucibus congue posuere. Cras vitae nibh" +
                                    "sed magna ultricies pretium. Proin eget lacus quis dui ullamcorper cursus commodo in lacus. Quisque et sem id leo venenatis dictum dignissim et felis." +
                                    "Vestibulum enim urna, vehicula vel dictum in, congue quis sapien. Quisque ac mauris tellus. Nulla cursus pellentesque mauris viverra bibendum. Fusce" +
                                    "molestie dui id sem blandit in convallis justo euismod. Curabitur velit nisi, adipiscing sed consequat et, dignissim eget dolor. Aenean malesuada quam id mi" +
                                    "vestibulum pulvinar. Nullam diam quam, lobortis sit amet semper vitae, tempus eget dolor.";

        public const String RESOURCES = @"..\..\resources\text\pdf\TaggedPdfTest\";
        public const String TARGET = "TaggedPdfTest\\";
        public const String OUT = TARGET + "pdf\\out";
        private Document document;
        private Paragraph h1;
        private string output;
        private PdfWriter writer;


        [SetUp]
        virtual public void Initialize() {
            Directory.CreateDirectory(TARGET + "pdf");
            Directory.CreateDirectory(TARGET + "xml");
            Document.Compress = false;
        }

        private void InitializeDocument(String name) {
            InitializeDocument(name, PdfWriter.VERSION_1_7);
        }

        private void InitializeDocument(String name, char pdfVersion) {
            output = OUT + name + ".pdf";
            document = new Document();
            writer = PdfWriter.GetInstance(document, new FileStream(output, FileMode.Create));
            writer.PdfVersion = pdfVersion;
            writer.SetTagged();
            document.Open();

            //Required for PDF/UA
            writer.ViewerPreferences = PdfWriter.DisplayDocTitle;
            document.AddLanguage("en-US");
            document.AddTitle("Some title");
            writer.CreateXmpMetadata();
            Chunk c = new Chunk("Document Header", new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD, BaseColor.BLUE));
            h1 = new Paragraph(c);
            h1.Role = PdfName.H1;
        }

        [Test]
        virtual public void CreateTaggedPdf0() {
            InitializeDocument("0");
            Paragraph paragraph = new Paragraph();
            Chunk c = new Chunk(" Hello ");
            paragraph.Add(c);
            c = new Chunk("  world\n\n");
            paragraph.Add(c);
            ColumnText columnText = new ColumnText(writer.DirectContent);
            columnText.SetSimpleColumn(36, 36, 250, 800);
            columnText.AddElement(paragraph);
            columnText.Go();
            document.Close();

            InitializeDocument("0");
            paragraph = new Paragraph();
            c = new Chunk("  ");
            paragraph.Add(c);
            columnText = new ColumnText(writer.DirectContent);
            columnText.SetSimpleColumn(36, 36, 250, 800);
            columnText.AddElement(paragraph);
            columnText.Go();
            document.Close();

            InitializeDocument("0");
            paragraph = new Paragraph();
            c = new Chunk("Hello World");
            paragraph.Add(c);
            columnText = new ColumnText(writer.DirectContent);
            columnText.SetSimpleColumn(36, 36, 250, 800);
            columnText.AddElement(paragraph);
            columnText.Go();
            document.Close();

            InitializeDocument("0");
            paragraph = new Paragraph();
            c = new Chunk("Hello World");
            paragraph.Add(c);
            document.Add(paragraph);
            document.Close();

            InitializeDocument("0");
            paragraph = new Paragraph();
            c = new Chunk(" Hello ");
            paragraph.Add(c);
            c = new Chunk("  world\n");
            paragraph.Add(c);
            paragraph.Font = new Font(Font.FontFamily.HELVETICA, 8, Font.NORMAL, BaseColor.RED);
            document.Add(paragraph);
            document.Close();
        }


        [Test]
        virtual public void CreateTaggedPdf1() {
            InitializeDocument("1");
            Paragraph paragraph = new Paragraph(text);
            paragraph.Font = new Font(Font.FontFamily.HELVETICA, 8, Font.NORMAL, BaseColor.RED);
            ColumnText columnText = new ColumnText(writer.DirectContent);
            columnText.SetSimpleColumn(36, 36, 250, 800);
            columnText.AddElement(h1);
            columnText.AddElement(paragraph);
            columnText.Go();
            columnText.SetSimpleColumn(300, 36, 500, 800);
            columnText.Go();
            document.Close();
            CompareResults("1");
        }

        [Test]
        virtual public void CreateTaggedPdf2() {
            InitializeDocument("2");
            Paragraph paragraph = new Paragraph(text);
            ColumnText columnText = new ColumnText(writer.DirectContent);

            columnText.SetSimpleColumn(36, 36, 400, 800);
            columnText.AddElement(h1);
            columnText.AddElement(paragraph);
            columnText.Go();
            document.NewPage();
            columnText.SetSimpleColumn(36, 36, 400, 800);
            columnText.Go();
            document.Close();
            CompareResults("2");
        }

        [Test]
        virtual public void CreateTaggedPdf3() {
            InitializeDocument("3");
            Paragraph paragraph = new Paragraph(text);
            document.Add(h1);
            document.Add(paragraph);
            document.Close();
            CompareResults("3");
        }

        [Test]
        virtual public void CreateTaggedPdf4() {
            InitializeDocument("4");
            Paragraph p = new Paragraph();
            PdfName nParagraph = new PdfName("Paragraph");
            p.Role = nParagraph;
            writer.StructureTreeRoot.MapRole(nParagraph, PdfName.P);

            try {
                Chunk c = new Chunk("Quick brown ");
                PdfName nTextBlock = new PdfName("TextBlock");
                c.Role = nTextBlock;
                writer.StructureTreeRoot.MapRole(nTextBlock, PdfName.SPAN);
                p.Add(c);
                Image i = Image.GetInstance(RESOURCES + "img\\fox.bmp");
                c = new Chunk(i, 0, 0);
                PdfName nImage = new PdfName("Image");
                c.Role = nImage;
                writer.StructureTreeRoot.MapRole(nImage, PdfName.FIGURE);
                c.SetAccessibleAttribute(PdfName.ALT, new PdfString("Fox image"));
                p.Add(c);
                p.Add(new Chunk(" jumped over a lazy "));
                i = Image.GetInstance(RESOURCES + "img\\dog.bmp");
                c = new Chunk(i, 0, 0);
                c.SetAccessibleAttribute(PdfName.ALT, new PdfString("Dog image"));
                p.Add(c);
            }
            catch (Exception) {
            }
            document.Add(h1);
            document.Add(p);
            document.Close();
            CompareResults("4");
        }

        [Test]
        virtual public void CreateTaggedPdf5() {
            InitializeDocument("5");
            List list = new List(true);
            try {
                list = new List(true);
                ListItem listItem =
                    new ListItem(
                        new Chunk(
                            "Quick brown fox jumped over a lazy dog. A very long line appears here because we need new line."));
                list.Add(listItem);
                Image i = Image.GetInstance(RESOURCES + "img\\fox.bmp");
                Chunk c = new Chunk(i, 0, 0);
                c.SetAccessibleAttribute(PdfName.ALT, new PdfString("Fox image"));
                listItem = new ListItem(c);
                list.Add(listItem);
                listItem = new ListItem(new Chunk("jumped over a lazy"));
                list.Add(listItem);
                i = Image.GetInstance(RESOURCES + "img\\dog.bmp");
                c = new Chunk(i, 0, 0);
                c.SetAccessibleAttribute(PdfName.ALT, new PdfString("Dog image"));
                listItem = new ListItem(c);
                list.Add(listItem);
            }
            catch (Exception) {
            }
            document.Add(h1);
            document.Add(list);
            document.Close();

            CompareResults("5");
        }

        [Test]
        virtual public void CreateTaggedPdf6() {
            InitializeDocument("6");

            ColumnText columnText = new ColumnText(writer.DirectContent);

            List list = new List(true);
            try {
                list = new List(true);
                ListItem listItem =
                    new ListItem(
                        new Chunk(
                            "Quick brown fox jumped over a lazy dog. A very long line appears here because we need new line."));
                list.Add(listItem);
                Image i = Image.GetInstance(RESOURCES + "img\\fox.bmp");
                Chunk c = new Chunk(i, 0, 0);
                c.SetAccessibleAttribute(PdfName.ALT, new PdfString("Fox image"));
                listItem = new ListItem(c);
                list.Add(listItem);
                listItem = new ListItem(new Chunk("jumped over a lazy"));
                list.Add(listItem);
                i = Image.GetInstance(RESOURCES + "img\\dog.bmp");
                c = new Chunk(i, 0, 0);
                c.SetAccessibleAttribute(PdfName.ALT, new PdfString("Dog image"));
                listItem = new ListItem(c);
                list.Add(listItem);
            }
            catch (Exception) {
            }
            columnText.SetSimpleColumn(36, 36, 400, 800);
            columnText.AddElement(h1);
            columnText.AddElement(list);
            columnText.Go();
            document.Close();
            CompareResults("6");
        }

        [Test]
        virtual public void CreateTaggedPdf7() {
            InitializeDocument("7");
            List list = new List(true);
            try {
                list = new List(true);
                ListItem listItem =
                    new ListItem(
                        new Chunk(
                            "Quick brown fox jumped over a lazy dog. A very long line appears here because we need new line."));
                list.Add(listItem);
                Image i = Image.GetInstance(RESOURCES + "img\\fox.bmp");
                Chunk c = new Chunk(i, 0, 0);
                c.SetAccessibleAttribute(PdfName.ALT, new PdfString("Fox image"));
                listItem = new ListItem(c);
                list.Add(listItem);
                listItem = new ListItem(new Chunk("jumped over a lazy"));
                list.Add(listItem);
                i = Image.GetInstance(RESOURCES + "img\\dog.bmp");
                c = new Chunk(i, 0, 0);
                c.SetAccessibleAttribute(PdfName.ALT, new PdfString("Dog image"));
                listItem = new ListItem(c);
                list.Add(listItem);
                listItem = new ListItem(new Paragraph(text));
                list.Add(listItem);
            }
            catch (Exception) {
            }
            document.Add(h1);
            document.Add(list);
            document.Close();

            CompareResults("7");
        }

        [Test]
        virtual public void CreateTaggedPdf8() {
            InitializeDocument("8");

            ColumnText columnText = new ColumnText(writer.DirectContent);

            List list = new List(true);
            try {
                list = new List(true);
                ListItem listItem =
                    new ListItem(
                        new Chunk(
                            "Quick brown fox jumped over a lazy dog. A very long line appears here because we need new line."));
                list.Add(listItem);
                Image i = Image.GetInstance(RESOURCES + "img\\fox.bmp");
                Chunk c = new Chunk(i, 0, 0);
                c.SetAccessibleAttribute(PdfName.ALT, new PdfString("Fox image"));
                listItem = new ListItem(c);
                list.Add(listItem);
                listItem = new ListItem(new Chunk("jumped over a lazy"));
                list.Add(listItem);
                i = Image.GetInstance(RESOURCES + "img\\dog.bmp");
                c = new Chunk(i, 0, 0);
                c.SetAccessibleAttribute(PdfName.ALT, new PdfString("Dog image"));
                listItem = new ListItem(c);
                list.Add(listItem);
                listItem = new ListItem(new Paragraph(text));
                list.Add(listItem);
            }
            catch (Exception) {
            }
            columnText.SetSimpleColumn(36, 36, 400, 800);
            columnText.AddElement(h1);
            columnText.AddElement(list);
            columnText.Go();
            document.NewPage();
            columnText.SetSimpleColumn(36, 36, 400, 800);
            columnText.Go();
            document.Close();

            CompareResults("8");
        }

        [Test]
        virtual public void CreateTaggedPdf9() {
            InitializeDocument("9");
            PdfPTable table = new PdfPTable(2);
            try {
                table.AddCell(
                    "Quick brown fox jumped over a lazy dog. A very long line appears here because we need new line.");
                Image i = Image.GetInstance(RESOURCES + "img\\fox.bmp");
                i.SetAccessibleAttribute(PdfName.ALT, new PdfString("Fox image"));
                table.AddCell(i);
                table.AddCell("jumped over a lazy");
                i = Image.GetInstance(RESOURCES + "img\\dog.bmp");
                i.SetAccessibleAttribute(PdfName.ALT, new PdfString("Dog image"));
                table.AddCell(i);
                table.AddCell("Hello World");
                Paragraph p = new Paragraph(text);
                table.AddCell(p);
            }
            catch (Exception) {
            }
            document.Add(h1);
            document.Add(table);
            document.Add(
                new Paragraph(
                    "Extra paragraph at the end of the document. Please make sure that this is really last portion of page content."));
            document.Close();
            CompareResults("9");
        }

        [Test]
        virtual public void CreateTaggedPdf10() {
            InitializeDocument("10");
            PdfPTable table = new PdfPTable(2);
            try {
                table.AddCell(
                    "Quick brown fox jumped over a lazy dog. A very long line appears here because we need new line.");
                Image i = Image.GetInstance(RESOURCES + "img\\fox.bmp");
                i.SetAccessibleAttribute(PdfName.ALT, new PdfString("Fox image"));
                table.AddCell(i);
                table.AddCell("jumped over a lazy");
                i = Image.GetInstance(RESOURCES + "img\\dog.bmp");
                i.SetAccessibleAttribute(PdfName.ALT, new PdfString("Dog image"));
                table.AddCell(i);

                PdfPTable t = new PdfPTable(2);
                t.AddCell(
                    "Quick brown fox jumped over a lazy dog. A very long line appears here because we need new line.");
                i = Image.GetInstance(RESOURCES + "img\\fox.bmp");
                i.SetAccessibleAttribute(PdfName.ALT, new PdfString("Fox image"));
                t.AddCell(i);
                t.AddCell("jumped over a lazy");
                i = Image.GetInstance(RESOURCES + "img\\dog.bmp");
                i.SetAccessibleAttribute(PdfName.ALT, new PdfString("Dog image"));
                t.AddCell(i);
                t.AddCell(text);
                t.AddCell("Hello World");
                table.AddCell(t);


                Paragraph p = new Paragraph(text);
                table.AddCell(p);
            }
            catch (Exception) {
            }
            document.Add(h1);
            document.Add(table);
            document.Close();

            CompareResults("10");
        }

        [Test]
        virtual public void CreateTaggedPdf11() {
            InitializeDocument("11");

            Chapter c =
                new Chapter(
                    new Paragraph("First chapter", new Font(Font.FontFamily.HELVETICA, 16, Font.BOLD, BaseColor.BLUE)),
                    1);
            c.TriggerNewPage = false;
            c.Indentation = 40;
            Section s1 =
                c.AddSection(new Paragraph("First section of a first chapter",
                                           new Font(Font.FontFamily.HELVETICA, 13, Font.BOLD, BaseColor.BLUE)));
            s1.Indentation = 20;
            Section s2 =
                s1.AddSection(new Paragraph("First subsection of a first section of a first chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Second subsection of a first section of a first chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Third subsection of a first section of a first chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s1 =
                c.AddSection(new Paragraph("Second section of a first chapter",
                                           new Font(Font.FontFamily.HELVETICA, 13, Font.BOLD, BaseColor.BLUE)));
            s1.Indentation = 20;
            s2 =
                s1.AddSection(new Paragraph("First subsection of a second section of a first chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Second subsection of a second section of a first chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Third subsection of a second section of a first chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s1 =
                c.AddSection(new Paragraph("Third section of a first chapter",
                                           new Font(Font.FontFamily.HELVETICA, 13, Font.BOLD, BaseColor.BLUE)));
            s1.Indentation = 20;
            s2 =
                s1.AddSection(new Paragraph("First subsection of a third section of a first chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Second subsection of a third section of a first chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Third subsection of a third section of a first chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            document.Add(c);

            c =
                new Chapter(
                    new Paragraph("Second chapter", new Font(Font.FontFamily.HELVETICA, 16, Font.BOLD, BaseColor.BLUE)),
                    2);
            c.TriggerNewPage = false;
            c.Indentation = 40;
            s1 =
                c.AddSection(new Paragraph("First section of a second chapter",
                                           new Font(Font.FontFamily.HELVETICA, 13, Font.BOLD, BaseColor.BLUE)));
            s1.Indentation = 20;
            s2 =
                s1.AddSection(new Paragraph("First subsection of a first section of a second chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Second subsection of a first section of a second chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Third subsection of a first section of a second chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s1 =
                c.AddSection(new Paragraph("Second section of a second chapter",
                                           new Font(Font.FontFamily.HELVETICA, 13, Font.BOLD, BaseColor.BLUE)));
            s1.Indentation = 20;
            s2 =
                s1.AddSection(new Paragraph("First subsection of a second section of a second chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Second subsection of a second section of a second chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Third subsection of a second section of a second chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s1 =
                c.AddSection(new Paragraph("Third section of a second chapter",
                                           new Font(Font.FontFamily.HELVETICA, 13, Font.BOLD, BaseColor.BLUE)));
            s1.Indentation = 20;
            s2 =
                s1.AddSection(new Paragraph("First subsection of a third section of a second chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Second subsection of a third section of a second chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Third subsection of a third section of a second chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            document.Add(c);

            c =
                new Chapter(
                    new Paragraph("Third chapter", new Font(Font.FontFamily.HELVETICA, 16, Font.BOLD, BaseColor.BLUE)),
                    3);
            c.TriggerNewPage = false;
            c.Indentation = 40;
            s1 =
                c.AddSection(new Paragraph("First section of a third chapter",
                                           new Font(Font.FontFamily.HELVETICA, 13, Font.BOLD, BaseColor.BLUE)));
            s1.Indentation = 20;
            s2 =
                s1.AddSection(new Paragraph("First subsection of a first section of a third chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Second subsection of a first section of a third chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Third subsection of a first section of a third chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s1 =
                c.AddSection(new Paragraph("Second section of a third chapter",
                                           new Font(Font.FontFamily.HELVETICA, 13, Font.BOLD, BaseColor.BLUE)));
            s1.Indentation = 20;
            s2 =
                s1.AddSection(new Paragraph("First subsection of a second section of a third chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Second subsection of a second section of a third chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Third subsection of a second section of a third chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s1 =
                c.AddSection(new Paragraph("Third section of a third chapter",
                                           new Font(Font.FontFamily.HELVETICA, 13, Font.BOLD, BaseColor.BLUE)));
            s1.Indentation = 20;
            s2 =
                s1.AddSection(new Paragraph("First subsection of a third section of a third chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Second subsection of a third section of a third chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            s2 =
                s1.AddSection(new Paragraph("Third subsection of a third section of a third chapter",
                                            new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.BLUE)));
            s2.Indentation = 10;
            s2.Add(new Paragraph("Some text..."));
            document.Add(c);

            c = new Chapter(4);
            c.TriggerNewPage = false;
            c.Indentation = 40;
            c.AddSection("First section of a fourths chapter, the chapter itself is invisible");
            document.Add(c);

            document.Close();

            CompareResults("11");
        }

        [Test]
        virtual public void CreateTaggedPdf12() {
            InitializeDocument("12");

            PdfPTable table = new PdfPTable(2);
            PdfPCell cell = new PdfPCell(new Paragraph("header 1"));
            cell.Colspan = 2;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("header 2"));
            cell.Colspan = 2;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("footer 1"));
            cell.Colspan = 2;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("footer 2"));
            cell.Colspan = 2;
            table.AddCell(cell);
            table.HeaderRows = 4;
            table.FooterRows = 2;
            try {
                for (int i = 1; i <= 50; i++) {
                    table.AddCell("row " + i + ", column 1");
                    table.AddCell("row " + i + ", column 2");
                }
            }
            catch (Exception) {
            }
            document.Add(table);
            document.Close();

            CompareResults("12");
        }

        [Test]
        virtual public void CreateTaggedPdf13() {
            InitializeDocument("13");

            Paragraph p = new Paragraph();
            Chunk chunk = new Chunk("Please visit ");
            p.Add(chunk);

            PdfAction action = new PdfAction("http://itextpdf.com");
            chunk = new Chunk("http://itextpdf.com",
                              new Font(Font.FontFamily.HELVETICA, Font.UNDEFINED, Font.UNDERLINE, BaseColor.BLUE));
            chunk.SetAction(action);
            p.Add(chunk);
            p.Add(new Chunk(" for more details."));
            document.Add(p);
            document.Close();
            CompareResults("13");
        }

        [Test]
        virtual public void CreateTaggedPdf14() {
            InitializeDocument("14");
            Paragraph paragraph = new Paragraph("Document MUST contain 1 page only!");
            document.NewPage();
            ColumnText columnText = new ColumnText(writer.DirectContent);
            columnText.SetSimpleColumn(36, 36, 250, 800);
            columnText.AddElement(paragraph);
            columnText.Go();
            document.Close();

            PdfReader reader = new PdfReader(OUT + "14.pdf");
            Assert.AreEqual(1, reader.NumberOfPages);
        }

        [Test]
        virtual public void CreateTaggedPdf15() {
            InitializeDocument("15");

            Paragraph p = new Paragraph();
            Chunk chunk = new Chunk("Hello tagged world!");
            chunk.SetBackground(new BaseColor(255, 0, 255));
            chunk.Font = FontFactory.GetFont("TimesNewRoman", 20, BaseColor.ORANGE);
            chunk.SetUnderline(BaseColor.PINK, 1.2f, 1, 1, 1, 0);
            p.Add(chunk);

            document.Add(p);
            document.Close();
            CompareResults("15");
        }

        [Test]
        virtual public void CreateTaggedPdf16() {
            InitializeDocument("16");

            Paragraph p = new Paragraph();
            Chunk chunk = new Chunk("Hello tagged world!");
            chunk.SetBackground(new BaseColor(255, 0, 255));
            chunk.Font = FontFactory.GetFont("TimesNewRoman", 20, BaseColor.ORANGE);
            chunk.SetUnderline(BaseColor.PINK, 1.2f, 1, 1, 1, 0);
            p.Add(chunk);
            PdfDiv div = new PdfDiv();
            div.AddElement(p);
            document.Add(div);

            document.Add(new Paragraph("This paragraph appears between 2 div blocks"));

            div = new PdfDiv();
            div.AddElement(new Paragraph(text));
            document.Add(div);


            document.Close();
            CompareResults("16");
        }

        [Test]
        virtual public void CreateTaggedPdf17() {
            InitializeDocument("17");

            PdfPTable table = new PdfPTable(2);
            PdfPCell cell = new PdfPCell(new Paragraph("h1"));
            cell.Colspan = 2;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("h2"));
            cell.Colspan = 2;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("footer 1"));
            cell.Colspan = 2;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("footer 2"));
            cell.Colspan = 2;
            table.AddCell(cell);
            table.HeaderRows = 4;
            table.FooterRows = 2;

            try {
                PdfPHeaderCell headerCell = new PdfPHeaderCell();
                headerCell.Scope = PdfPHeaderCell.ROW;
                headerCell.Phrase = new Phrase("header1");
                headerCell.Name = "header1";
                table.AddCell(headerCell);
                PdfPHeaderCell headerCell2 = new PdfPHeaderCell();
                headerCell2.Scope = PdfPHeaderCell.ROW;
                headerCell2.Phrase = new Phrase("header2");
                headerCell2.Name = "header2";
                table.AddCell(headerCell2);
                cell = new PdfPCell(new Phrase("row 2, column 1"));
                cell.AddHeader(headerCell);
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("row 2, column 2"));
                cell.AddHeader(headerCell2);
                cell.AddHeader(headerCell2);
                table.AddCell(cell);
            }
            catch (Exception e) {
                Trace.WriteLine(e.Message);
            }
            document.Add(table);
            document.Close();
            CompareResults("17");
        }

        [Test]
        virtual public void CreateTaggedPdf18() {
            InitializeDocument("18");

            PdfDiv div = new PdfDiv();

            Paragraph paragraph = new Paragraph(text);
            paragraph.Font = new Font(Font.FontFamily.HELVETICA, 8, Font.NORMAL, BaseColor.RED);

            div.BackgroundColor = BaseColor.MAGENTA;
            div.TextAlignment = Element.ALIGN_CENTER;
            div.AddElement(paragraph);
            document.Add(div);
            document.Close();
            CompareResults("18");
        }


        [Test]
        virtual public void CreateTaggedPdf19() {
            InitializeDocument("19");

            PdfDiv div = new PdfDiv();
            writer.DirectContent.OpenMCBlock(div);

            PdfArtifact artifact = new PdfArtifact();
            artifact.Type = new PdfString("Background");
            writer.DirectContent.OpenMCBlock(artifact);
            writer.DirectContent.SetColorFill(BaseColor.RED);
            writer.DirectContent.Rectangle(100, 100, 400, 400);
            writer.DirectContent.Fill();
            writer.DirectContent.CloseMCBlock(artifact);

            writer.DirectContent.CloseMCBlock(div);

            document.Close();
            CompareResults("19");
        }

        [Test]
        virtual public void CreateTaggedPdf20() {
            InitializeDocument("20");

            Paragraph paragraph = new Paragraph();
            paragraph.Font.Color = BaseColor.RED;
            Chunk c = new Chunk("Hello ");
            paragraph.Add(c);
            c = new Chunk("  world\n\n");
            paragraph.Add(c);

            ColumnText columnText = new ColumnText(writer.DirectContent);
            columnText.SetSimpleColumn(36, 36, 250, 800);
            columnText.AddElement(paragraph);
            columnText.Go();

            PdfTemplate template = writer.DirectContent.CreateTemplate(PageSize.A4.Width, PageSize.A4.Height);
            writer.DirectContent.AddTemplate(template, 0, 0, true);

            columnText = new ColumnText(template);
            columnText.SetSimpleColumn(36, 36, 250, 750);
            columnText.AddText(new Phrase(new Chunk("Hello word \n")));
            columnText.Go();

            document.NewPage();

            paragraph = new Paragraph();
            paragraph.Font.Color = BaseColor.RED;
            c = new Chunk("Hello ");
            paragraph.Add(c);
            c = new Chunk("  world\n");
            paragraph.Add(c);

            columnText = new ColumnText(template);
            columnText.SetSimpleColumn(36, 36, 250, 700);
            columnText.AddElement(paragraph);
            columnText.Go();

            template = writer.DirectContent.CreateTemplate(PageSize.A4.Width, PageSize.A4.Height);
            writer.DirectContent.AddTemplate(template, 0, 0, true);

            paragraph = new Paragraph();
            paragraph.Font.Color = BaseColor.GREEN;
            c = new Chunk("Hello ");
            paragraph.Add(c);
            c = new Chunk("  world\n");
            paragraph.Add(c);

            columnText = new ColumnText(template);
            columnText.SetSimpleColumn(36, 36, 250, 800);
            columnText.AddElement(paragraph);
            columnText.Go();

            paragraph = new Paragraph();
            paragraph.Font.Color = BaseColor.BLUE;
            c = new Chunk("Hello ");
            paragraph.Add(c);
            c = new Chunk("  world\n");
            paragraph.Add(c);

            template = writer.DirectContent.CreateTemplate(PageSize.A4.Width, PageSize.A4.Height);

            columnText = new ColumnText(template);
            columnText.SetSimpleColumn(36, 36, 250, 650);
            columnText.AddElement(paragraph);
            columnText.Go();

            writer.DirectContent.AddTemplate(template, 0, 100);

            writer.DirectContent.AddTemplate(template, 0, 50);

            writer.DirectContent.AddTemplate(template, 0, 0);

            document.Close();
            CompareResults("20");
        }

        [Test]
        virtual public void CreateTaggedPdf21() {
            try {
                InitializeDocument("21");

                PdfTemplate template = writer.DirectContent.CreateTemplate(PageSize.A4.Width, PageSize.A4.Height);

                writer.DirectContent.AddTemplate(template, 0, 0, true);

                ColumnText columnText = new ColumnText(template);
                columnText.SetSimpleColumn(36, 36, 250, 750);
                columnText.AddText(new Phrase("Hello word \n\n"));
                columnText.Go();

                document.NewPage();
                writer.DirectContent.AddTemplate(template, 0, 0);

                document.Close();
            }
            catch (Exception conformExc) {
                Assert.AreEqual("Template with tagged content could not be used more than once.", conformExc.Message);
                return;
            }
            finally {
                document.Close();
            }
            Assert.Fail("Expected error: 'Template with tagged content could not be used more than once.");
        }

        [Test]
        virtual public void CreateTaggedPdf22() {
            InitializeDocument("22", PdfWriter.VERSION_1_4);
            Paragraph p = new Paragraph();
            PdfName nParagraph = new PdfName("Paragraph");
            p.Role = nParagraph;
            writer.StructureTreeRoot.MapRole(nParagraph, PdfName.P);

            try {
                Chunk c = new Chunk("Quick brown ");
                PdfName nTextBlock = new PdfName("TextBlock");
                c.Role = nTextBlock;
                writer.StructureTreeRoot.MapRole(nTextBlock, PdfName.SPAN);
                p.Add(c);
                Image i = Image.GetInstance(RESOURCES + "img\\dog.bmp");
                c = new Chunk(i, 0, 0);
                PdfName nImage = new PdfName("Image");
                c.Role = nImage;
                writer.StructureTreeRoot.MapRole(nImage, PdfName.FIGURE);
                c.SetAccessibleAttribute(PdfName.ALT, new PdfString("Fox image"));
                p.Add(c);
                p.Add(new Chunk(" jumped over a lazy "));
                i = Image.GetInstance(RESOURCES + "img\\dog.bmp");
                c = new Chunk(i, 0, 0);
                c.SetAccessibleAttribute(PdfName.ALT, new PdfString("Dog image"));
                p.Add(c);
            }
            catch (Exception e) {
            }
            document.Add(h1);
            document.Add(p);
            document.Close();
            CompareResults("22");
        }

        [Test]
        virtual public void CreateTaggedPdf23() {
            InitializeDocument("23", PdfWriter.VERSION_1_4);

            PdfPTable table = new PdfPTable(2);
            PdfPCell cell = new PdfPCell(new Paragraph("header 1"));
            cell.Colspan = 2;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("header 2"));
            cell.Colspan = 2;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("footer 1"));
            cell.Colspan = 2;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("footer 2"));
            cell.Colspan = 2;
            table.AddCell(cell);
            table.HeaderRows = 4;
            table.FooterRows = 2;
            try {
                for (int i = 1; i <= 50; i++) {
                    table.AddCell("row " + i + ", column 1");
                    table.AddCell("row " + i + ", column 2");
                }
            }
            catch (Exception e) {
            }
            document.Add(table);
            document.Close();

            CompareResults("23");
        }

        [Test]
        public virtual void CreateTaggedPdf24() {
            Document document = new Document(PageSize.LETTER);

            MemoryStream baos = new MemoryStream();

            PdfWriter writer = PdfWriter.GetInstance(document, baos);

            writer.ViewerPreferences = PdfWriter.DisplayDocTitle;

//set more document properties

            writer.PdfVersion = PdfWriter.VERSION_1_7;
            writer.SetTagged(PdfWriter.markInlineElementsOnly);
            PdfDictionary info = writer.Info;
            info.Put(PdfName.TITLE, new PdfString("Testing"));

            writer.CreateXmpMetadata();


// step 3

            document.Open();
            document.AddLanguage("en_US");
            document.SetAccessibleAttribute(PdfName.LANG, new PdfString("en_US"));

// step 4

            Paragraph p = new Paragraph("Paragraph testing testing");
            p.SetAccessibleAttribute(PdfName.ACTUALTEXT, new PdfString("Paragraph ALT Text"));
            p.SetAccessibleAttribute(PdfName.ALT, new PdfString("Paragraph ALT Text"));
            document.Add(p);
            
            BaseFont bFont = BaseFont.CreateFont(RESOURCES + @"..\FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED);
            Font font = new Font(bFont, 12);
            Chunk ck = new Chunk("Span testing testing", font);
            ck.SetAccessibleAttribute(PdfName.ACTUALTEXT, new PdfString("Span ALT Text"));
            ck.SetAccessibleAttribute(PdfName.ALT, new PdfString("Span ALT Text"));
            p = new Paragraph(ck);

            document.Add(p);

// step 5
            document.Close();

            FileStream fos = new FileStream("TaggedPdfTest/pdf/out24.pdf", FileMode.Create);

            byte[] buff = baos.ToArray();
            fos.Write(buff, 0, buff.Length);

            fos.Flush();

            fos.Close();
            CompareResults("24");
        }

        [Test]
        public virtual void CreateTagedPdf25() {
            Document document = new Document();
            MemoryStream baos = new MemoryStream();

            PdfWriter writer = PdfWriter.GetInstance(document, baos);

            writer.ViewerPreferences = PdfWriter.DisplayDocTitle;

            writer.PdfVersion = PdfWriter.VERSION_1_7;
            writer.SetTagged();
            PdfDictionary info = writer.Info;
            info.Put(PdfName.TITLE, new PdfString("Testing"));
            writer.CreateXmpMetadata();

            document.Open();
            document.AddLanguage("en_US");
            document.SetAccessibleAttribute(PdfName.LANG, new PdfString("en_US"));

            string longParagraphString = "Long teeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeext Paragraph testing testing";
            BaseFont bFont = BaseFont.CreateFont(RESOURCES + @"..\FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED);
            Font font = new Font(bFont, 12);
            Paragraph p = new Paragraph(longParagraphString, font);
            document.Add(p);

            string longChunkString = "Long teeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeext Span testing testing";
            Chunk ck = new Chunk(longChunkString, font);
            p = new Paragraph(ck);
            document.Add(p);

            document.Close();
            FileStream fos = new FileStream("TaggedPdfTest/pdf/out25.pdf", FileMode.Create);
            byte[] buff = baos.ToArray();
            fos.Write(buff, 0, buff.Length);
            fos.Flush();
            fos.Close();
            CompareResults("25");

            CompareTool compareTool = new CompareTool();
            string cmpFile = RESOURCES + @"out25.pdf";
            string errorMessage = compareTool.CompareByContent("TaggedPdfTest/pdf/out25.pdf", cmpFile,
                "TaggedPdfTest/pdf/", "diff_");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }

        }

        [Test]
        public virtual void CreateTagedPdf26() {
            Document doc = new Document(PageSize.LETTER, 72, 72, 72, 72);
            MemoryStream baos = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(doc, baos);
            writer.SetTagged();

            doc.Open();

            ColumnText ct = new ColumnText(writer.DirectContent);
            ct.UseAscender = true;
            ct.AdjustFirstLine = true;
            ct.SetSimpleColumn(doc.Left, doc.Bottom, doc.Right, doc.Top);

            Paragraph p = new Paragraph("before");
            ct.AddElement(p);

            List list = new List(List.ORDERED, List.NUMERICAL);
            list.Autoindent = false;
            list.IndentationLeft = 14;
            list.SymbolIndent = 14;

            list.Add("Item 1");

            List nested = new List(List.ORDERED, List.NUMERICAL);
            nested.Autoindent = false;
            nested.IndentationLeft = 14;
            nested.SymbolIndent = 14;
            nested.Add("Nested 1");

            list.Add(nested);
            ct.AddElement(list);

            p = new Paragraph("after");
            ct.AddElement(p);

            ct.Go();

            doc.Close();

            File.WriteAllBytes("TaggedPdfTest/pdf/out26.pdf", baos.ToArray());
            CompareResults("26");
        }

        [Test]
        public void CreateTaggedPdf27() {
            Document doc = new Document();
            MemoryStream baos = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(doc, baos);
            writer.SetTagged();

            doc.Open();

            String imagePath = RESOURCES + "img\\Desert.jpg";
            Image img = Image.GetInstance(imagePath);
            img.SetAbsolutePosition(0, 0);
            img.SetAccessibleAttribute(PdfName.E, new PdfString("expansion"));
            img.SetAccessibleAttribute(PdfName.ALT, new PdfString("alt"));

            PdfTemplate template = writer.DirectContent.CreateTemplate(img.Width, img.Height);
            writer.DirectContent.AddTemplate(template, 100, 300, true);

            ColumnText ct = new ColumnText(template);
            ct.SetSimpleColumn(0, 0, 250, 300);
            ct.AddElement(img);
            ct.Go();

            doc.Close();



            File.WriteAllBytes("TaggedPdfTest/pdf/out27.pdf", baos.ToArray());
            CompareResults("27");
        }

        [TearDown]
        virtual public void Compress() {
            Document.Compress = true;
        }

        private void CompareResults(String name) {
            PdfReader reader = new PdfReader(OUT + name + ".pdf");
            string orig = RESOURCES + "xml\\test" + name + ".xml";
            string curr = TARGET + "xml\\test" + name + ".xml";
            FileStream xmlOut = new FileStream(curr, FileMode.Create);
            new MyTaggedPdfReaderTool().ConvertToXml(reader, xmlOut);
            xmlOut.Close();
            XmlDiff xmldiff = new XmlDiff(XmlDiffOptions.None);
            Assert.True(xmldiff.Compare(orig, curr, false));
        }

        #region Nested type: MyMarkedContentRenderFilter

        private class MyMarkedContentRenderFilter : MarkedContentRenderFilter {
            private readonly int mcid;

            public MyMarkedContentRenderFilter(int mcid)
                : base(mcid) {
                this.mcid = mcid;
            }

            public override bool AllowText(TextRenderInfo renderInfo) {
                return renderInfo.HasMcid(mcid, true);
            }
        }

        #endregion

        #region Nested type: MyTaggedPdfReaderTool

        private class MyTaggedPdfReaderTool : TaggedPdfReaderTool {
            public override void ParseTag(String tag, PdfObject obj, PdfDictionary page) {
                if (obj is PdfNumber) {
                    PdfNumber mcid = (PdfNumber) obj;
                    RenderFilter filter = new MyMarkedContentRenderFilter(mcid.IntValue);
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    FilteredTextRenderListener listener = new FilteredTextRenderListener(
                        strategy, filter);
                    PdfContentStreamProcessor processor = new PdfContentStreamProcessor(
                        listener);
                    processor.ProcessContent(PdfReader.GetPageContent(page), page
                                                                                 .GetAsDict(PdfName.RESOURCES));
                    outp.Write(XMLUtil.EscapeXML(listener.GetResultantText(), true));
                }
                else {
                    base.ParseTag(tag, obj, page);
                }
            }

            public override void InspectChildDictionary(PdfDictionary k) {
                InspectChildDictionary(k, true);
            }
        }

        #endregion
    }
}
