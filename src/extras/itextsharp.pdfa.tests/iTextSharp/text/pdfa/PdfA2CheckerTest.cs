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
using NUnit.Framework;
using iTextSharp.text.pdf;

namespace iTextSharp.text.pdfa {
    [TestFixture]
    public class PdfA2CheckerTest {
        public const String RESOURCES = @"..\..\resources\text\pdfa\";
        public const String TARGET = "PdfA2CheckerTest\\";
        public const String OUT = TARGET + "pdf\\out";


        [SetUp]
        virtual public void Initialize() {
            Directory.CreateDirectory(TARGET + "pdf");
            Directory.CreateDirectory(TARGET + "xml");
        }

        [Test]
        virtual public void MetadaCheckTest() {
            FileStream fos = new FileStream(OUT + "metadaPDFA2CheckTest1.pdf", FileMode.Create);
            Document document = new Document();
            PdfWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_2B);
            document.Open();
            PdfContentByte canvas = writer.DirectContent;

            canvas.SetColorFill(BaseColor.LIGHT_GRAY);
            canvas.MoveTo(writer.PageSize.Left, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Top);
            canvas.LineTo(writer.PageSize.Left, writer.PageSize.Bottom);
            canvas.Fill();

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException exc) {
                exceptionThrown = true;
            }

            if (!exceptionThrown)
                Assert.Fail("PdfAConformance exception should be thrown on unknown blend mode.");
        }

        [Test]
        virtual public void TransparencyCheckTest1() {
            string filename = OUT + "pdfa2TransparencyCheckTest1.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_2B);
            document.Open();

            PdfContentByte canvas = writer.DirectContent;

            canvas.SaveState();
            PdfGState gs = new PdfGState();
            gs.BlendMode = PdfGState.BM_DARKEN;
            canvas.SetGState(gs);
            canvas.Rectangle(100, 100, 100, 100);
            canvas.Fill();
            canvas.RestoreState();

            canvas.SaveState();
            gs = new PdfGState();
            gs.BlendMode = new PdfName("Lighten");
            canvas.SetGState(gs);
            canvas.Rectangle(200, 200, 100, 100);
            canvas.Fill();
            canvas.RestoreState();

            bool conformanceExceptionThrown = false;
            try {
                canvas.SaveState();
                gs = new PdfGState();
                gs.BlendMode = new PdfName("UnknownBM");
                canvas.SetGState(gs);
                canvas.Rectangle(300, 300, 100, 100);
                canvas.Fill();
                canvas.RestoreState();

                document.Close();
            }
            catch (PdfAConformanceException pdface) {
                conformanceExceptionThrown = true;
            }

            if (!conformanceExceptionThrown)
                Assert.Fail("PdfAConformance exception should be thrown on unknown blend mode.");
        }

        [Test]
        virtual public void TransparencyCheckTest2() {
            Document document = new Document();
            try {
                // step 2
                PdfAWriter writer = PdfAWriter.GetInstance(document,
                    new FileStream(OUT + "pdfa2TransperancyCheckTest2.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
                writer.CreateXmpMetadata();
                // step 3
                document.Open();
                PdfDictionary sec = new PdfDictionary();
                sec.Put(PdfName.GAMMA, new PdfArray(new float[] {2.2f, 2.2f, 2.2f}));
                sec.Put(PdfName.MATRIX,
                    new PdfArray(new float[]
                    {0.4124f, 0.2126f, 0.0193f, 0.3576f, 0.7152f, 0.1192f, 0.1805f, 0.0722f, 0.9505f}));
                sec.Put(PdfName.WHITEPOINT, new PdfArray(new float[] {0.9505f, 1f, 1.089f}));
                PdfArray arr = new PdfArray(PdfName.CALRGB);
                arr.Add(sec);
                writer.SetDefaultColorspace(PdfName.DEFAULTRGB, writer.AddToBody(arr).IndirectReference);
                // step 4
                PdfContentByte cb = writer.DirectContent;
                float gap = (document.PageSize.Width - 400)/3;

                PictureBackdrop(gap, 500f, cb);
                PictureBackdrop(200 + 2*gap, 500, cb);
                PictureBackdrop(gap, 500 - 200 - gap, cb);
                PictureBackdrop(200 + 2*gap, 500 - 200 - gap, cb);

                PictureCircles(gap, 500, cb);
                cb.SaveState();
                PdfGState gs1 = new PdfGState();
                gs1.FillOpacity = 0.5f;
                cb.SetGState(gs1);
                PictureCircles(200 + 2*gap, 500, cb);
                cb.RestoreState();

                cb.SaveState();
                PdfTemplate tp = cb.CreateTemplate(200, 200);
                PdfTransparencyGroup group = new PdfTransparencyGroup();
                tp.Group = group;
                PictureCircles(0, 0, tp);
                cb.SetGState(gs1);
                cb.AddTemplate(tp, gap, 500 - 200 - gap);
                cb.RestoreState();

                cb.SaveState();
                tp = cb.CreateTemplate(200, 200);
                tp.Group = group;
                PdfGState gs2 = new PdfGState();
                gs2.FillOpacity = 0.5f;
                gs2.BlendMode = PdfGState.BM_HARDLIGHT;
                tp.SetGState(gs2);
                PictureCircles(0, 0, tp);
                cb.AddTemplate(tp, 200 + 2*gap, 500 - 200 - gap);
                cb.RestoreState();

                Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf",
                    BaseFont.WINANSI, true);
                font.Color = BaseColor.BLACK;
                cb.ResetRGBColorFill();
                ColumnText ct = new ColumnText(cb);
                Phrase ph = new Phrase("Ungrouped objects\nObject opacity = 1.0", font);
                ct.SetSimpleColumn(ph, gap, 0, gap + 200, 500, 18, Element.ALIGN_CENTER);
                ct.Go();

                ph = new Phrase("Ungrouped objects\nObject opacity = 0.5", font);
                ct.SetSimpleColumn(ph, 200 + 2*gap, 0, 200 + 2*gap + 200, 500,
                    18, Element.ALIGN_CENTER);
                ct.Go();

                ph = new Phrase("Transparency group\nObject opacity = 1.0\nGroup opacity = 0.5\nBlend mode = Normal",
                    font);
                ct.SetSimpleColumn(ph, gap, 0, gap + 200, 500 - 200 - gap, 18, Element.ALIGN_CENTER);
                ct.Go();

                ph = new Phrase(
                    "Transparency group\nObject opacity = 0.5\nGroup opacity = 1.0\nBlend mode = HardLight", font);
                ct.SetSimpleColumn(ph, 200 + 2*gap, 0, 200 + 2*gap + 200, 500 - 200 - gap,
                    18, Element.ALIGN_CENTER);
                ct.Go();
                //ICC_Profile icc = ICC_Profile.GetInstance(File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read));
                //writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            }
            catch (DocumentException de) {
                Console.Error.WriteLine(de.Message);
            }
            catch (IOException ioe) {
                Console.Error.WriteLine(ioe.Message);
            }

            bool conformanceExceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException pdface) {
                conformanceExceptionThrown = true;
            }

            if (!conformanceExceptionThrown)
                Assert.Fail("PdfAConformance exception should be thrown on unknown blend mode.");
        }

        [Test]
        virtual public void TransparencyCheckTest3() {
            Document document = new Document();
            try {
                // step 2
                PdfAWriter writer = PdfAWriter.GetInstance(
                    document,
                    new FileStream(OUT + "pdfa2TransperancyCheckTest3.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
                writer.CreateXmpMetadata();
                // step 3
                document.Open();
                PdfDictionary sec = new PdfDictionary();
                sec.Put(PdfName.GAMMA, new PdfArray(new float[] {2.2f, 2.2f, 2.2f}));
                sec.Put(PdfName.MATRIX,
                    new PdfArray(new float[]
                    {0.4124f, 0.2126f, 0.0193f, 0.3576f, 0.7152f, 0.1192f, 0.1805f, 0.0722f, 0.9505f}));
                sec.Put(PdfName.WHITEPOINT, new PdfArray(new float[] {0.9505f, 1f, 1.089f}));
                PdfArray arr = new PdfArray(PdfName.CALRGB);
                arr.Add(sec);
                writer.SetDefaultColorspace(PdfName.DEFAULTRGB, writer.AddToBody(arr).IndirectReference);

                // step 4
                PdfContentByte cb = writer.DirectContent;
                float gap = (document.PageSize.Width - 400)/3;

                PictureBackdrop(gap, 500, cb, writer);
                PictureBackdrop(200 + 2*gap, 500, cb, writer);
                PictureBackdrop(gap, 500 - 200 - gap, cb, writer);
                PictureBackdrop(200 + 2*gap, 500 - 200 - gap, cb, writer);
                PdfTemplate tp;
                PdfTransparencyGroup group;

                tp = cb.CreateTemplate(200, 200);
                PictureCircles(0, 0, tp, writer);
                group = new PdfTransparencyGroup();
                group.Isolated = true;
                group.Knockout = true;
                tp.Group = group;
                cb.AddTemplate(tp, gap, 500);

                tp = cb.CreateTemplate(200, 200);
                PictureCircles(0, 0, tp, writer);
                group = new PdfTransparencyGroup();
                group.Isolated = true;
                group.Knockout = false;
                tp.Group = group;
                cb.AddTemplate(tp, 200 + 2*gap, 500);

                tp = cb.CreateTemplate(200, 200);
                PictureCircles(0, 0, tp, writer);
                group = new PdfTransparencyGroup();
                group.Isolated = false;
                group.Knockout = true;
                tp.Group = group;
                cb.AddTemplate(tp, gap, 500 - 200 - gap);

                tp = cb.CreateTemplate(200, 200);
                PictureCircles(0, 0, tp, writer);
                group = new PdfTransparencyGroup();
                group.Isolated = false;
                group.Knockout = false;
                tp.Group = group;
                cb.AddTemplate(tp, 200 + 2*gap, 500 - 200 - gap);
            }
            catch (DocumentException de) {
                Console.Error.WriteLine(de.Message);
            }
            catch (IOException ioe) {
                Console.Error.WriteLine(ioe.Message);
            }

            bool conformanceException = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException pdface) {
                conformanceException = true;
            }

            if (!conformanceException)
                Assert.Fail("PdfAConformance exception should be thrown on unknown blend mode.");
        }

        [Test]
        virtual public void TransparencyCheckTest4() {
            // step 1
            Document document = new Document(new Rectangle(850, 600));
            // step 2
            PdfAWriter writer
                = PdfAWriter.GetInstance(document, new FileStream(OUT + "pdfa2TransperancyCheckTest4.pdf", FileMode.Create),
                    PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            // step 3
            document.Open();
            // step 4
            PdfContentByte canvas = writer.DirectContent;

            // add the clipped image
            Image img = Image.GetInstance(RESOURCES + "img/bruno_ingeborg.jpg");
            float w = img.ScaledWidth;
            float h = img.ScaledHeight;
            canvas.Ellipse(1, 1, 848, 598);
            canvas.Clip();
            canvas.NewPath();
            canvas.AddImage(img, w, 0, 0, h, 0, -600);

            // Create a transparent PdfTemplate
            PdfTemplate t2 = writer.DirectContent.CreateTemplate(850, 600);
            PdfTransparencyGroup transGroup = new PdfTransparencyGroup();
            transGroup.Put(PdfName.CS, PdfName.DEVICEGRAY);
            transGroup.Isolated = true;
            transGroup.Knockout = false;
            t2.Group = transGroup;

            // Add transparent ellipses to the template
            int gradationStep = 30;
            float[] gradationRatioList = new float[gradationStep];
            for (int i = 0; i < gradationStep; i++) {
                gradationRatioList[i] = 1 - (float) Math.Sin(Math.PI/180*90.0f/gradationStep*(i + 1));
            }
            for (int i = 1; i < gradationStep + 1; i++) {
                t2.SetLineWidth(5*(gradationStep + 1 - i));
                t2.SetGrayStroke(gradationRatioList[gradationStep - i]);
                t2.Ellipse(0, 0, 850, 600);
                t2.Stroke();
            }

            // Create an image mask for the direct content
            PdfDictionary maskDict = new PdfDictionary();
            maskDict.Put(PdfName.TYPE, PdfName.MASK);
            maskDict.Put(PdfName.S, new PdfName("Luminosity"));
            maskDict.Put(new PdfName("G"), t2.IndirectReference);
            PdfGState gState = new PdfGState();
            gState.Put(PdfName.SMASK, maskDict);
            canvas.SetGState(gState);

            canvas.AddTemplate(t2, 0, 0);

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            // step 5
            document.Close();
        }

        [Test]
        virtual public void ImageCheckTest1() {
            string filename = OUT + "ImageCheckTest1.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);
            Document document = new Document();
            PdfWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            String[] pdfaErrors = new String[9];
            for (int i = 1; i <= 9; i++) {
                try {
                    Image img = Image.GetInstance(String.Format("{0}jpeg2000\\file{1}.jp2", RESOURCES, i));
                    document.Add(img);
                    document.NewPage();
                }
                catch (Exception e) {
                    pdfaErrors[i - 1] = e.Message;
                }
            }

            Assert.AreEqual(null, pdfaErrors[0]);
            Assert.AreEqual(null, pdfaErrors[1]);
            Assert.AreEqual(null, pdfaErrors[2]);
            Assert.AreEqual(null, pdfaErrors[3]);
            Assert.AreEqual(true, pdfaErrors[4].Contains("0x01"));
            Assert.AreEqual(null, pdfaErrors[5]);
            Assert.AreEqual(true, pdfaErrors[6].Contains("0x01"));
            Assert.AreEqual(null, pdfaErrors[7]);
            Assert.AreEqual(null, pdfaErrors[8]);

            document.Close();
        }

        [Test]
        virtual public void ImageCheckTest2() {
            string filename = OUT + "ImageCheckTest2.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);
            Document document = new Document();
            PdfWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            List<String> pdfaErrors = new List<String>();
            try {
                Image img = Image.GetInstance(RESOURCES + @"jpeg2000\p0_01.j2k");
                document.Add(img);
                document.NewPage();
            }
            catch (Exception e) {
                pdfaErrors.Add(e.Message);
            }

            try {
                Image img = Image.GetInstance(RESOURCES + @"jpeg2000\p0_02.j2k");
                document.Add(img);
            }
            catch (Exception e) {
                pdfaErrors.Add(e.Message);
            }

            try {
                Image img = Image.GetInstance(RESOURCES + @"jpeg2000\p1_01.j2k");
                document.Add(img);
            }
            catch (Exception e) {
                pdfaErrors.Add(e.Message);
            }

            try {
                Image img = Image.GetInstance(RESOURCES + @"jpeg2000\p1_02.j2k");
                document.Add(img);
            }
            catch (Exception e) {
                pdfaErrors.Add(e.Message);
            }

            Assert.AreEqual(4, pdfaErrors.Count);
            for (int i = 0; i < 4; i++) {
                Assert.AreEqual(true, pdfaErrors[i].Contains("JPX"));
            }

            document.Close();
        }

        [Test]
        virtual public void LayerCheckTest1() {
            string filename = OUT + "LayerCheckTest1.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);
            Document document = new Document();
            PdfWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            writer.ViewerPreferences = PdfWriter.PageModeUseOC;
            writer.PdfVersion = PdfWriter.VERSION_1_5;
            document.Open();
            PdfLayer layer = new PdfLayer("Do you see me?", writer);
            layer.On = true;
            BaseFont bf = BaseFont.CreateFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, true);
            PdfContentByte cb = writer.DirectContent;
            cb.BeginText();
            cb.SetFontAndSize(bf, 18);
            cb.ShowTextAligned(Element.ALIGN_LEFT, "Do you see me?", 50, 790, 0);
            cb.BeginLayer(layer);
            cb.ShowTextAligned(Element.ALIGN_LEFT, "Peek-a-Boo!!!", 50, 766, 0);
            cb.EndLayer();
            cb.EndText();

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            document.Close();
        }

        [Test]
        virtual public void LayerCheckTest2() {
            string filename = OUT + "LayerCheckTest2.pdf";
            FileStream fos = new FileStream(filename, FileMode.Create);
            Document document = new Document();
            PdfWriter writer = PdfAWriter.GetInstance(document, fos, PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            writer.ViewerPreferences = PdfWriter.PageModeUseOC;
            writer.PdfVersion = PdfWriter.VERSION_1_5;
            document.Open();
            PdfContentByte cb = writer.DirectContent;
            PdfLayer nested = new PdfLayer("Nested layers", writer);
            PdfLayer nested_1 = new PdfLayer("Nested layer 1", writer);
            PdfLayer nested_2 = new PdfLayer("Nested layer 2", writer);
            nested.AddChild(nested_1);
            nested.AddChild(nested_2);
            writer.LockLayer(nested_2);
            cb.BeginLayer(nested);

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, true);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("nested layers", font), 50, 775, 0);
            cb.EndLayer();
            cb.BeginLayer(nested_1);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("nested layer 1", font), 100, 800, 0);
            cb.EndLayer();
            cb.BeginLayer(nested_2);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_LEFT, new Phrase("nested layer 2", font), 100, 750, 0);
            cb.EndLayer();

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            document.Close();
        }

        [Test]
        virtual public void EgsCheckTest1() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document,
                new FileStream(OUT + "pdfa2egsCheckTest1.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2A);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            PdfContentByte canvas = writer.DirectContent;
            PdfGState gs = new PdfGState();
            gs.Put(PdfName.TR, new PdfName("Test"));
            gs.Put(PdfName.HTP, new PdfName("Test"));
            canvas.SaveState();
            canvas.SetGState(gs);
            canvas.RestoreState();
            canvas.MoveTo(writer.PageSize.Left, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Top);
            canvas.Fill();

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject() == gs) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void EgsCheckTest2() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document,
                new FileStream(OUT + "EgsCheckTest2.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2A);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            PdfContentByte canvas = writer.DirectContent;
            PdfGState gs = new PdfGState();
            PdfDictionary dict = new PdfDictionary();
            dict.Put(PdfName.HALFTONETYPE, new PdfNumber(6));
            gs.Put(PdfName.HT, dict);
            canvas.SetGState(gs);

            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                exceptionThrown = true;
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void EgsCheckTest3() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document,
                new FileStream(OUT + "pdfa2EgsCheckTest3.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2A);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            PdfContentByte canvas = writer.DirectContent;
            PdfGState gs = new PdfGState();
            PdfDictionary dict = new PdfDictionary();
            dict.Put(PdfName.HALFTONETYPE, new PdfNumber(5));
            dict.Put(PdfName.HALFTONENAME, new PdfName("Test"));
            gs.Put(PdfName.HT, dict);
            canvas.SetGState(gs);

            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                exceptionThrown = true;
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void EgsCheckTest4() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "pdfa2egsCheckTest4.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            PdfContentByte canvas = writer.DirectContent;
            PdfGState gs = new PdfGState();
            gs.Put(PdfName.TR2, new PdfName("Test"));
            gs.Put(PdfName.HTP, new PdfName("Test"));
            canvas.SaveState();
            canvas.SetGState(gs);
            canvas.RestoreState();
            canvas.MoveTo(writer.PageSize.Left, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Top);
            canvas.Fill();

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject() == gs) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void CanvasCheckTest1() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "canvasCheckTest1.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_1B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfContentByte canvas = writer.DirectContent;
            bool exceptionThrown = false;
            try {
                for (int i = 0; i < 29; i++) {
                    canvas.SaveState();
                }
            }
            catch (PdfAConformanceException e) {
                if ("q".Equals(e.GetObject())) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
            for (int i = 0; i < 28; i++) {
                canvas.RestoreState();
            }

            document.Close();
        }

        [Test]
        virtual public void CanvasCheckTest2() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "canvasCheckTestt2.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_1B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfContentByte canvas = writer.DirectContent;
            for (int i = 0; i < 28; i++) {
                canvas.SaveState();
            }
            for (int i = 0; i < 28; i++) {
                canvas.RestoreState();
            }
            document.Close();
        }

        [Test]
        public void PdfObjectCheckTest() {
            PdfA1CheckerTest.PdfObjectCheck(OUT + "pdfObjectCheckTest.pdf", PdfAConformanceLevel.PDF_A_2B, false);
        }

        [Test]
        virtual public void AnnotationCheckTest1() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "annotationCheckTest1.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, new PdfName("Movie"));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject() == annot && e.Message.Equals("Annotation type /Movie not allowed.")) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest2() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "annotationCheckTest2.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject() == annot && e.Message.Equals("Annotation type null not allowed.")) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest2_1() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "annotationCheckTest2_1.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.POPUP);
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            document.Close();
        }

        [Test]
        virtual public void AnnotationCheckTest2_2() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "annotationCheckTest2_2.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 200, 100, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.WIDGET);
            annot.Put(PdfName.CONTENTS, new PdfDictionary());
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            document.Close();
        }

        [Test]
        virtual public void AnnotationCheckTest2_3() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "annotationCheckTest2_3.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.WIDGET);
            annot.Put(PdfName.CONTENTS, new PdfDictionary());
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject() == annot && e.Message.Equals("Every annotation shall have at least one appearance dictionary")) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest3() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "annotationCheckTest3.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.TEXT);
            annot.Put(PdfName.F, new PdfNumber(0));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject() == annot && e.Message
                    .Equals("The F key's Print flag bit shall be set to 1 and its Hidden, Invisible, NoView and ToggleNoView flag bits shall be set to 0.")) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest4() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "annotationCheckTest4.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.TEXT);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT & PdfAnnotation.FLAGS_INVISIBLE));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject() == annot && e.Message
                    .Equals("The F key's Print flag bit shall be set to 1 and its Hidden, Invisible, NoView and ToggleNoView flag bits shall be set to 0.")) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest5() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "annotationCheckTest5.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            annot.Put(PdfName.SUBTYPE, PdfName.WIDGET);
            annot.Put(PdfName.CONTENTS, new PdfDictionary());
            PdfDictionary ap = new PdfDictionary();
            PdfStream s = new PdfStream(Encoding.Default.GetBytes("Hello World"));
            ap.Put(PdfName.D, new PdfDictionary());
            ap.Put(PdfName.N, writer.AddToBody(s).IndirectReference);
            annot.Put(PdfName.AP, ap);
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject() == annot && e.Message
                    .Equals("Appearance dictionary shall contain only the N key with stream value.")) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest6() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "annotationCheckTest6.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            annot.Put(PdfName.SUBTYPE, PdfName.WIDGET);
            annot.Put(PdfName.CONTENTS, new PdfDictionary());
            annot.Put(PdfName.FT, new PdfName("Btn"));
            PdfDictionary ap = new PdfDictionary();
            PdfStream s = new PdfStream(Encoding.Default.GetBytes("Hello World"));
            //PdfDictionary s = new PdfDictionary();
            ap.Put(PdfName.N, writer.AddToBody(s).IndirectReference);
            annot.Put(PdfName.AP, ap);
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject() == annot && e.Message
                    .Equals("Appearance dictionary of Widget subtype and Btn field type shall contain only the n key with dictionary value")) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest7() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "annotationCheckTest7.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            annot.Put(PdfName.SUBTYPE, PdfName.WIDGET);
            annot.Put(PdfName.CONTENTS, new PdfDictionary());
            PdfDictionary ap = new PdfDictionary();
            //PdfStream s = new PdfStream(Encoding.Default.GetBytes("Hello World"));
            PdfDictionary s = new PdfDictionary();
            ap.Put(PdfName.N, writer.AddToBody(s).IndirectReference);
            annot.Put(PdfName.AP, ap);
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject() == annot && e.Message
                    .Equals("Appearance dictionary shall contain only the N key with stream value.")) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest8() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "annotationCheckTest8.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            annot.Put(PdfName.SUBTYPE, PdfName.WIDGET);
            annot.Put(PdfName.CONTENTS, new PdfDictionary());
            PdfDictionary ap = new PdfDictionary();
            PdfStream s = new PdfStream(Encoding.Default.GetBytes("Hello World"));
            ap.Put(PdfName.N, writer.AddToBody(s).IndirectReference);
            annot.Put(PdfName.AP, ap);
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            document.Close();
        }

        [Test]
        virtual public void AnnotationCheckTest9() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "annotationCheckTest9.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.TEXT);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT | PdfAnnotation.FLAGS_NOZOOM | PdfAnnotation.FLAGS_NOROTATE));
            PdfDictionary ap = new PdfDictionary();
            PdfStream s = new PdfStream(Encoding.Default.GetBytes("Hello World"));
            ap.Put(PdfName.N, writer.AddToBody(s).IndirectReference);
            annot.Put(PdfName.AP, ap);
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            document.Close();
        }

        [Test]
        virtual public void AnnotationCheckTest10() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "annotationCheckTest10.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2A);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.STAMP);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            bool exceptionThrown = false;
            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject() == annot && e.Message
                    .Equals("Annotation of type /Stamp should have Contents key.")) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void AnnotationCheckTest11() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "annotationCheckTest11.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2A);
            writer.CreateXmpMetadata();
            writer.SetTagged();
            document.Open();
            document.AddLanguage("en-US");

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(100, 100, 200, 200));
            annot.Put(PdfName.SUBTYPE, PdfName.STAMP);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            annot.Put(PdfName.CONTENTS, new PdfString("Hello World"));
            PdfDictionary ap = new PdfDictionary();
            PdfStream s = new PdfStream(Encoding.Default.GetBytes("Hello World"));
            ap.Put(PdfName.N, writer.AddToBody(s).IndirectReference);
            annot.Put(PdfName.AP, ap);
            PdfContentByte canvas = writer.DirectContent;
            canvas.AddAnnotation(annot);
            document.Close();
        }

        [Test]
        virtual public void AnnotationCheckTest12()
        {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "annotationCheckTest12.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2A);
            writer.CreateXmpMetadata();
            writer.SetTagged();
            document.Open();
            document.AddLanguage("en-US");

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            PdfDictionary ap = new PdfDictionary();
            PdfStream s = new PdfStream(Encoding.Default.GetBytes("Hello World"));
            ap.Put(PdfName.N, writer.AddToBody(s).IndirectReference);
            PdfContentByte canvas = writer.DirectContent;

            PdfAnnotation annot = new PdfAnnotation(writer, new Rectangle(220, 220, 240, 240));
            annot.Put(PdfName.SUBTYPE, PdfName.POLYGON);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            annot.Put(PdfName.AP, ap);
            canvas.AddAnnotation(annot);
            annot = new PdfAnnotation(writer, new Rectangle(100, 100, 120, 120));
            annot.Put(PdfName.SUBTYPE, PdfName.POLYLINE);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            annot.Put(PdfName.AP, ap);
            canvas.AddAnnotation(annot);
            annot = new PdfAnnotation(writer, new Rectangle(130, 130, 150, 150));
            annot.Put(PdfName.SUBTYPE, PdfName.CARET);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            annot.Put(PdfName.AP, ap);
            canvas.AddAnnotation(annot);
            annot = new PdfAnnotation(writer, new Rectangle(160, 160, 180, 180));
            annot.Put(PdfName.SUBTYPE, PdfName.WATERMARK);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            annot.Put(PdfName.AP, ap);
            canvas.AddAnnotation(annot);
            annot = new PdfAnnotation(writer, new Rectangle(190, 190, 210, 210));
            annot.Put(PdfName.SUBTYPE, PdfName.FILEATTACHMENT);
            annot.Put(PdfName.F, new PdfNumber(PdfAnnotation.FLAGS_PRINT));
            annot.Put(PdfName.AP, ap);
            document.Close();
        }

        [Test]
        virtual public void ColorCheckTest1() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "pdfa2ColorCheckTest1.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();
            PdfDictionary sec = new PdfDictionary();
            sec.Put(PdfName.GAMMA, new PdfArray(new float[] {2.2f, 2.2f, 2.2f}));
            sec.Put(PdfName.MATRIX, new PdfArray(new float[] {0.4124f, 0.2126f, 0.0193f, 0.3576f, 0.7152f, 0.1192f, 0.1805f, 0.0722f, 0.9505f}));
            sec.Put(PdfName.WHITEPOINT, new PdfArray(new float[] {0.9505f, 1f, 1.089f}));
            PdfArray arr = new PdfArray(PdfName.CALRGB);
            arr.Add(sec);
            writer.SetDefaultColorspace(PdfName.DEFAULTCMYK, writer.AddToBody(arr).IndirectReference);

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            font.Color = GrayColor.GRAYBLACK;
            document.Add(new Paragraph("Hello World", font));
            font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            font.Color = new CMYKColor(0, 100, 0, 0);
            document.Add(new Paragraph("Hello World", font));
            font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            font.Color = new BaseColor(0, 255, 0);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfContentByte canvas = writer.DirectContent;
            canvas.SetColorFill(new CMYKColor(0.1f, 0.1f, 0.1f, 0.1f));
            canvas.MoveTo(writer.PageSize.Left, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Top);
            canvas.Fill();

            document.Close();
        }

        [Test]
        virtual public void ColorCheckTest2() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "pdfa2ColorCheckTest2.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12, Font.NORMAL, BaseColor.RED);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            PdfContentByte canvas = writer.DirectContent;
            bool exceptionThrown = false;
            canvas.SetColorFill(new CMYKColor(0.1f, 0.1f, 0.1f, 0.1f));
            canvas.MoveTo(writer.PageSize.Left, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Top);
            canvas.Fill();

            try {
                document.Close();
            }
            catch (PdfAConformanceException e) {
                exceptionThrown = true;
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException should be thrown.");
        }

        [Test]
        virtual public void ColorCheckTest3() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "pdfa2ColorCheckTest3.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();
            PdfDictionary sec = new PdfDictionary();
            sec.Put(PdfName.GAMMA, new PdfArray(new float[] {2.2f, 2.2f, 2.2f}));
            sec.Put(PdfName.MATRIX, new PdfArray(new float[] {0.4124f, 0.2126f, 0.0193f, 0.3576f, 0.7152f, 0.1192f, 0.1805f, 0.0722f, 0.9505f}));
            sec.Put(PdfName.WHITEPOINT, new PdfArray(new float[] {0.9505f, 1f, 1.089f}));
            PdfArray arr = new PdfArray(PdfName.CALRGB);
            arr.Add(sec);
            writer.SetDefaultColorspace(PdfName.DEFAULTRGB, writer.AddToBody(arr).IndirectReference);
            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));
            document.Close();

            PdfReader reader = new PdfReader(OUT + "pdfa2ColorCheckTest3.pdf");
            PdfAStamper stamper = new PdfAStamper(reader, new FileStream(OUT + "pdfa2ColorCheckTest3_updating_failed.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            bool exceptionThrown = false;
            try {
                font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
                font.Color = BaseColor.RED;
                PdfContentByte canvas = stamper.GetOverContent(1);
                canvas.SetFontAndSize(font.BaseFont, 12);
                canvas.SetColorFill(BaseColor.RED);
                ColumnText.ShowTextAligned(canvas, Element.ALIGN_LEFT, new Paragraph("Hello World", font), 36, 775, 0);
                stamper.Close();
            }
            catch (PdfAConformanceException) {
                exceptionThrown = true;
            }
            reader.Close();

            if (!exceptionThrown)
                Assert.Fail("PdfAConformance exception should be thrown");

            reader = new PdfReader(OUT + "pdfa2ColorCheckTest3.pdf");
            stamper = new PdfAStamper(reader, new FileStream(OUT + "pdfa2ColorCheckTest3_updating_ok.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            stamper.Writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            font.Color = BaseColor.RED;
            PdfContentByte canvas1 = stamper.GetOverContent(1);
            canvas1.SetFontAndSize(font.BaseFont, 12);
            canvas1.SetColorFill(BaseColor.RED);
            ColumnText.ShowTextAligned(canvas1,
                Element.ALIGN_LEFT, new Paragraph("Hello World", font), 36, 775, 0);
            stamper.Close();
            reader.Close();
        }

        [Test]
        virtual public void ColorCheckTest4() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "pdfa2ColorCheckTest4.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));
            document.Close();

            PdfReader reader = new PdfReader(OUT + "pdfa2ColorCheckTest4.pdf");
            PdfAStamper stamper = new PdfAStamper(reader, new FileStream(OUT + "pdfa2ColorCheckTest4_updating_failed.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            bool exceptionThrown = false;
            try {
                iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read,
                    FileShare.Read);
                icc = ICC_Profile.GetInstance(iccProfileFileStream);
                iccProfileFileStream.Close();

                stamper.Writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
                font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
                font.Color = BaseColor.RED;
                PdfContentByte canvas = stamper.GetOverContent(1);
                canvas.SetFontAndSize(font.BaseFont, 12);
                canvas.SetColorFill(BaseColor.RED);
                ColumnText.ShowTextAligned(canvas,
                    Element.ALIGN_LEFT, new Paragraph("Hello World", font), 36, 775, 760);
                stamper.Close();
            }
            catch (PdfAConformanceException) {
                exceptionThrown = true;
            }
            reader.Close();

            if (!exceptionThrown)
                Assert.Fail("PdfAConformance exception should be thrown");
        }

        [Test]
        virtual public void ColorCheckTest5() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "pdfa2ColorCheckTest5.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();
            bool exceptionThrown = false;
            try {
                Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
                document.Add(new Paragraph("Hello World", font));

                PdfContentByte canvas = writer.DirectContent;

                canvas.SetColorFill(BaseColor.LIGHT_GRAY);
                canvas.MoveTo(writer.PageSize.Left, writer.PageSize.Bottom);
                canvas.LineTo(writer.PageSize.Right, writer.PageSize.Bottom);
                canvas.LineTo(writer.PageSize.Right, writer.PageSize.Top);
                canvas.LineTo(writer.PageSize.Left, writer.PageSize.Bottom);
                canvas.Fill();


                canvas.SetFontAndSize(font.BaseFont, 20);
                canvas.SetColorStroke(new CMYKColor(0, 0, 0, 1f));
                canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_STROKE);
                canvas.SaveState();
                canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_CLIP);
                canvas.RestoreState();
                canvas.BeginText();
                canvas.ShowTextAligned(Element.ALIGN_LEFT, "Hello World", 36, 770, 0);
                canvas.EndText();

                FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
                ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
                iccProfileFileStream.Close();

                writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

                document.Close();
            }
            catch (PdfAConformanceException) {
                exceptionThrown = true;
            }

            if (!exceptionThrown)
                Assert.Fail("PdfAConformance exception should be thrown");
        }

        [Test]
        virtual public void ColorCheckTest6() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "pdfa2ColorCheckTest6.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            PdfContentByte canvas = writer.DirectContent;

            canvas.SetColorFill(BaseColor.LIGHT_GRAY);
            canvas.MoveTo(writer.PageSize.Left, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Bottom);
            canvas.LineTo(writer.PageSize.Right, writer.PageSize.Top);
            canvas.LineTo(writer.PageSize.Left, writer.PageSize.Bottom);
            canvas.Fill();


            canvas.SetFontAndSize(font.BaseFont, 20);
            canvas.SetColorStroke(new CMYKColor(0, 0, 0, 1f));
            canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_INVISIBLE);
            canvas.BeginText();
            canvas.ShowTextAligned(Element.ALIGN_LEFT, "Hello World", 36, 770, 0);
            canvas.EndText();
            canvas.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL);
            canvas.BeginText();
            canvas.ShowTextAligned(Element.ALIGN_LEFT, "Hello World", 36, 750, 0);
            canvas.EndText();

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            document.Close();
        }

        [Test]
        virtual public void FontCheckTest1() {
            bool exceptionThrown = false;
            try {
                Document document = new Document();
                PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "pdfa2FontCheckTest1.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2A);
                writer.CreateXmpMetadata();
                writer.SetTagged();
                document.Open();
                document.AddLanguage("en-US");

                Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, false /*BaseFont.EMBEDDED*/, 12);
                document.Add(new Paragraph("Hello World", font));

                FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
                ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
                iccProfileFileStream.Close();

                writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

                document.Close();
            }
            catch (DocumentException) {
                exceptionThrown = true;
            }
            catch (PdfAConformanceException) {
                exceptionThrown = true;
            }

            if (!exceptionThrown)
                Assert.Fail("PdfAConformance exception should be thrown");
        }

        /**
         * Prints a square and fills half of it with a gray rectangle.
         *
         * @param x
         * @param y
         * @param cb
         * @throws Exception
         */
        private void PictureBackdrop(float x, float y, PdfContentByte cb) {
            cb.SetColorStroke(BaseColor.BLACK);
            cb.SetColorFill(BaseColor.LIGHT_GRAY);
            cb.Rectangle(x, y, 100, 200);
            cb.Fill();
            cb.SetLineWidth(2);
            cb.Rectangle(x, y, 200, 200);
            cb.Stroke();
        }

        /**
         * Prints 3 circles in different colors that intersect with eachother.
         *
         * @param x
         * @param y
         * @param cb
         * @throws Exception
         */
        private void PictureCircles(float x, float y, PdfContentByte cb) {
            cb.SetColorFill(BaseColor.RED);
            cb.Circle(x + 70, y + 70, 50);
            cb.Fill();
            cb.SetColorFill(BaseColor.YELLOW);
            cb.Circle(x + 100, y + 130, 50);
            cb.Fill();
            cb.SetColorFill(BaseColor.BLUE);
            cb.Circle(x + 130, y + 70, 50);
            cb.Fill();
        }

        /**
         * Prints a square and fills half of it with a gray rectangle.
         *
         * @param x
         * @param y
         * @param cb
         * @throws Exception
         */
        private void PictureBackdrop(float x, float y, PdfContentByte cb,
            PdfWriter writer) {
            PdfShading axial = PdfShading.SimpleAxial(writer, x, y, x + 200, y,
                BaseColor.YELLOW, BaseColor.RED);
            PdfShadingPattern axialPattern = new PdfShadingPattern(axial);
            cb.SetShadingFill(axialPattern);
            cb.SetColorStroke(BaseColor.BLACK);
            cb.SetLineWidth(2);
            cb.Rectangle(x, y, 200, 200);
            cb.FillStroke();
        }

        /**
         * Prints 3 circles in different colors that intersect with eachother.
         *
         * @param x
         * @param y
         * @param cb
         * @throws Exception
         */
        private void PictureCircles(float x, float y, PdfContentByte cb, PdfWriter writer) {
            PdfGState gs = new PdfGState();
            gs.BlendMode = PdfGState.BM_MULTIPLY;
            gs.FillOpacity = 1f;
            cb.SetGState(gs);
            cb.SetColorFill(BaseColor.LIGHT_GRAY);
            cb.Circle(x + 75, y + 75, 70);
            cb.Fill();
            cb.Circle(x + 75, y + 125, 70);
            cb.Fill();
            cb.Circle(x + 125, y + 75, 70);
            cb.Fill();
            cb.Circle(x + 125, y + 125, 70);
            cb.Fill();
        }

        [Test]
        virtual public void FileSpecCheckTest1() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "fileSpecCheckTest1.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            byte[] somePdf = new byte[25];
            writer.AddFileAttachment("some pdf file", somePdf, "foo.pdf", "foo.pdf", PdfAWriter.MimeTypePdf,
                AFRelationshipValue.Data);

            document.Close();
        }

        [Test]
        public virtual void CidFontCheckTest1() {
            String outPdf = TARGET + "cidFontCheckTest1.pdf";
            String resourceDir = @"..\..\resources\text\pdfa\";
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(outPdf, FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(resourceDir + "FreeMonoBold.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));
            ICC_Profile icc = ICC_Profile.GetInstance(new FileStream(resourceDir + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read));
            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            document.Close();

            Assert.Null(new CompareTool().CompareByContent(outPdf, resourceDir + "cidset/cmp_cidFontCheckTest1.pdf", TARGET, "diff_"));
        }

        [Test]
        public virtual void CidFontCheckTest2() {
            String outPdf = TARGET + "cidFontCheckTest2.pdf";
            String resourceDir = @"..\..\resources\text\pdfa\";
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(outPdf, FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(resourceDir + "Puritan2.otf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));
            ICC_Profile icc = ICC_Profile.GetInstance(new FileStream(resourceDir + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read));
            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            document.Close();

            Assert.Null(new CompareTool().CompareByContent(outPdf, resourceDir + "cidset/cmp_cidFontCheckTest2.pdf", TARGET, "diff_"));
        }

        [Test]
        public virtual void CidFontCheckTest3()
        {
            String outPdf = TARGET + "cidFontCheckTest3.pdf";
            String resourceDir = @"..\..\resources\text\pdfa\";
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(outPdf, FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(resourceDir + "NotoSansCJKjp-Bold.otf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));
            ICC_Profile icc = ICC_Profile.GetInstance(new FileStream(resourceDir + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read));
            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            document.Close();

            Assert.Null(new CompareTool().CompareByContent(outPdf, resourceDir + "cidset/cmp_cidFontCheckTest3.pdf", TARGET, "diff_"));
        }

        [Test]
        virtual public void FileSpecCheckTest2() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "fileSpecCheckTest2.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            FileStream iss = File.OpenRead(RESOURCES + "pdfa.pdf");
            MemoryStream os = new MemoryStream();
            byte[] buffer = new byte[1024];
            int length;
            while ((length = iss.Read(buffer, 0, 1024)) > 0) {
                os.Write(buffer, 0, length);
            }
            writer.AddPdfAttachment("some pdf file", os.ToArray(), "foo.pdf", "foo.pdf");

            document.Close();
        }

        [Test]
        virtual public void FileSpecCheckTest3() {
            Document document = new Document();
            PdfAWriter writer = PdfAWriter.GetInstance(document, new FileStream(OUT + "fileSpecCheckTest3.pdf", FileMode.Create), PdfAConformanceLevel.PDF_A_2B);
            writer.CreateXmpMetadata();
            document.Open();

            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            document.Add(new Paragraph("Hello World", font));

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read, FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            writer.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);

            MemoryStream txt = new MemoryStream();
            TextWriter outp = new StreamWriter(txt);
            outp.Write("<foo><foo2>Hello world</foo2></foo>");
            outp.Close();

            bool exceptionThrown = false;
            try {
                writer.AddFileAttachment("foo file", txt.ToArray(), "foo.xml", "foo.xml", "application/xml",
                    AFRelationshipValue.Source);
            }
            catch (PdfAConformanceException e) {
                if (e.GetObject() != null && e.Message.Equals("Embedded file shall contain correct pdf mime type.")) {
                    exceptionThrown = true;
                }
            }
            if (!exceptionThrown)
                Assert.Fail("PdfAConformanceException with correct message should be thrown.");
        }

        [Test]
        public virtual void TextFieldTest() {
            Document d = new Document();
            PdfWriter w = PdfAWriter.GetInstance(d, new FileStream(OUT + "textField.pdf", FileMode.Create),
                PdfAConformanceLevel.PDF_A_2B);
            w.CreateXmpMetadata();
            d.Open();

            FileStream iccProfileFileStream = File.Open(RESOURCES + "sRGB Color Space Profile.icm", FileMode.Open, FileAccess.Read,
                FileShare.Read);
            ICC_Profile icc = ICC_Profile.GetInstance(iccProfileFileStream);
            iccProfileFileStream.Close();

            w.SetOutputIntents("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", icc);
            TextField text = new TextField(w, new Rectangle(50, 700, 150, 750), "text1");
            Font font = FontFactory.GetFont(RESOURCES + "FreeMonoBold.ttf", BaseFont.WINANSI, BaseFont.EMBEDDED, 12);
            text.Font = font.BaseFont;
            text.Text = "test";
            PdfFormField field = text.GetTextField();
            w.AddAnnotation(field);
            d.Close();
        }
    }
}
