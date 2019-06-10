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
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.events;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    /**
     * @author Michael Demey
     */

    public class FlatteningTest {
        private const string RESOURCES_FOLDER = @"..\..\resources\text\pdf\FlatteningTest\";
        private const string OUTPUT_FOLDER = "FlatteningTest/";

        [SetUp]
        public void SetUp() {
            Directory.CreateDirectory(OUTPUT_FOLDER);
        }

        [Test]
        public virtual void TestFlatteningNewAppearances() {
            String OUT = "tpl3_flattened.pdf";

            PdfReader reader = new PdfReader(RESOURCES_FOLDER + "tpl3.pdf");
            AcroFields fields = reader.AcroFields;
            if (fields != null && fields.Fields != null && fields.Fields.Count > 0) {
                FileStream @out = null;
                @out = new FileStream(OUTPUT_FOLDER + OUT, FileMode.Create);
                PdfStamper stamp = new PdfStamper(reader, @out);
                stamp.FormFlattening = true;
                AcroFields form = stamp.AcroFields;

                foreach (KeyValuePair<String, AcroFields.Item> e in form.Fields) {
                    form.SetField(e.Key, e.Key);
                }

                stamp.Close();
                @out.Close();
            }
            reader.Close();

            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.Compare(OUTPUT_FOLDER + OUT, RESOURCES_FOLDER + "cmp_" + OUT, OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public virtual void FlattenGenerateAppearanceFalse01() {
            String outFile = OUTPUT_FOLDER + "flattenGenerateAppearanceFalse01.pdf";
            FileStream @out = new FileStream(outFile, FileMode.Create);
            PdfReader reader = new PdfReader(RESOURCES_FOLDER + "not_filled_form.pdf");
            PdfStamper stamper = new PdfStamper(reader, @out);

            AcroFields form = stamper.AcroFields;

            // Fill out the form with arbitrary data
            int x = 1;
            foreach (KeyValuePair<String, AcroFields.Item> e in form.Fields) {
                form.SetField(e.Key, "Test " + x++);
            }

            form.GenerateAppearances = false;
            stamper.FormFlattening = true;
            stamper.Close();

            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(outFile, RESOURCES_FOLDER + "cmp_flattenGenerateAppearanceFalse01.pdf", OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public virtual void TestFlattening() {
            const string INPUT_FOLDER = RESOURCES_FOLDER + "input/";
            const string CMP_FOLDER = RESOURCES_FOLDER + "cmp/";
            if (File.Exists(INPUT_FOLDER))
                Assert.Fail("Input folder can't be found (" + INPUT_FOLDER + ")");

            String[] files = Directory.GetFiles(INPUT_FOLDER, "*.pdf");

            foreach (String file in files) {
                // flatten fields
                PdfReader reader = new PdfReader(file);
                PdfStamper stamper = new PdfStamper(reader,
                    new FileStream(OUTPUT_FOLDER + Path.GetFileName(file), FileMode.Create));
                stamper.FormFlattening = true;
                stamper.Close();

                // compare
                CompareTool compareTool = new CompareTool();
                String errorMessage = compareTool.Compare(OUTPUT_FOLDER + Path.GetFileName(file),
                    CMP_FOLDER + Path.GetFileName(file), OUTPUT_FOLDER, "diff");
                if (errorMessage != null) {
                    Assert.Fail(errorMessage);
                }
            }
        }


        [Test]
        public virtual void TestFlatteningGenerateAppearances1() {
            const string OUT = "noappearances-needapp-false_override-false.pdf";
            TestFlatteningGenerateAppearance(RESOURCES_FOLDER + "noappearances-needapp-false.pdf", OUTPUT_FOLDER + OUT,
                false);

            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.Compare(OUTPUT_FOLDER + OUT, RESOURCES_FOLDER + "cmp_" + OUT,
                OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public virtual void TestFlatteningGenerateAppearances2() {
            const string OUT = "noappearances-needapp-false_override-true.pdf";
            TestFlatteningGenerateAppearance(RESOURCES_FOLDER + "noappearances-needapp-false.pdf", OUTPUT_FOLDER + OUT,
                true);

            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.Compare(OUTPUT_FOLDER + OUT, RESOURCES_FOLDER + "cmp_" + OUT,
                OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public virtual void TestFlatteningGenerateAppearances3() {
            const string OUT = "noappearances-needapp-false_override-none.pdf";
            TestFlatteningGenerateAppearance(RESOURCES_FOLDER + "noappearances-needapp-false.pdf", OUTPUT_FOLDER + OUT,
                null);

            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.Compare(OUTPUT_FOLDER + OUT, RESOURCES_FOLDER + "cmp_" + OUT,
                OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public virtual void TestFlatteningGenerateAppearances4() {
            const string OUT = "noappearances-needapp-true_override-false.pdf";
            TestFlatteningGenerateAppearance(RESOURCES_FOLDER + "noappearances-needapp-true.pdf", OUTPUT_FOLDER + OUT,
                false);

            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.Compare(OUTPUT_FOLDER + OUT, RESOURCES_FOLDER + "cmp_" + OUT,
                OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public virtual void TestFlatteningGenerateAppearances5() {
            const string OUT = "noappearances-needapp-true_override-true.pdf";
            TestFlatteningGenerateAppearance(RESOURCES_FOLDER + "noappearances-needapp-true.pdf", OUTPUT_FOLDER + OUT,
                true);

            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.Compare(OUTPUT_FOLDER + OUT, RESOURCES_FOLDER + "cmp_" + OUT,
                OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public virtual void TestFlatteningGenerateAppearances6() {
            const string OUT = "noappearances-needapp-true_override-none.pdf";
            TestFlatteningGenerateAppearance(RESOURCES_FOLDER + "noappearances-needapp-true.pdf", OUTPUT_FOLDER + OUT,
                null);

            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.Compare(OUTPUT_FOLDER + OUT, RESOURCES_FOLDER + "cmp_" + OUT,
                OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }
        
        [Test]
        public virtual void TestFlatteningGenerateAppearances7() {
            const string OUT = "test01.pdf";
            TestFlatteningGenerateAppearance(RESOURCES_FOLDER + "test01.pdf", OUTPUT_FOLDER + OUT,
                true);
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.Compare(OUTPUT_FOLDER + OUT, RESOURCES_FOLDER + "cmp_" + OUT,
                OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }


        public virtual void TestFlatteningGenerateAppearance(string input, string output, bool? gen) {
            PdfReader reader = new PdfReader(input);
            PdfStamper stamper = new PdfStamper(reader, new FileStream(output, FileMode.Create));
            if (gen != null)
                stamper.AcroFields.GenerateAppearances = (bool) gen;
            stamper.FormFlattening = true;
            stamper.Close();
        }

        [Test]
        public void TestRegeneratingFieldsTrue() {
            String file = "regenerateFieldTrue.pdf";

            Document doc = new Document(PageSize.A4);
            MemoryStream fs = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);
            writer.CloseStream = false;

            doc.Open();
            PdfPTable myTable = new PdfPTable(1);
            myTable.TotalWidth = 300f;
            myTable.LockedWidth = true;
            myTable.HorizontalAlignment = 0;

            //Create the textfield that will sit on a cell in the table
            TextField tf = new TextField(writer, new Rectangle(0, 0, 70, 200), "cellTextBox");
            tf.Text = "text field";
            //Create the table cell
            PdfPCell tbCell = new PdfPCell(new Phrase(" "));
            FieldPositioningEvents events = new FieldPositioningEvents(writer, tf.GetTextField());
            tbCell.CellEvent = events;
            myTable.AddCell(tbCell);
            PdfContentByte cb = writer.DirectContent;
            //Write out the table to the middle of the document
            myTable.WriteSelectedRows(0, -1, 0, -1, 20, 700, cb);
            doc.Close();

            fs.Position = 0;
            MemoryStream fs2 = new MemoryStream();
            PdfReader reader2 = new PdfReader(fs);
            PdfStamper stamper2 = new PdfStamper(reader2, fs2);
            stamper2.AcroFields.GenerateAppearances = true;
            stamper2.Close();
            reader2.Close();

            PdfReader reader = new PdfReader(new MemoryStream(fs2.ToArray()));
            PdfStamper stamper = new PdfStamper(reader, new FileStream(OUTPUT_FOLDER + file.Clone(), FileMode.Create));
            stamper.FormFlattening = true;
            stamper.Close();
            reader.Close();

            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER + file, RESOURCES_FOLDER + file,
                OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public void TestRegeneratingFieldsFalse() {
            String file = "regenerateFieldFalse.pdf";

            Document doc = new Document(PageSize.A4);
            MemoryStream fs = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);
            writer.CloseStream = false;

            doc.Open();
            PdfPTable myTable = new PdfPTable(1);
            myTable.TotalWidth = 300f;
            myTable.LockedWidth = true;
            myTable.HorizontalAlignment = 0;

            //Create the textfield that will sit on a cell in the table
            TextField tf = new TextField(writer, new Rectangle(0, 0, 70, 200), "cellTextBox");
            tf.Text = "text field";
            //Create the table cell
            PdfPCell tbCell = new PdfPCell(new Phrase(" "));
            FieldPositioningEvents events = new FieldPositioningEvents(writer, tf.GetTextField());
            tbCell.CellEvent = events;
            myTable.AddCell(tbCell);
            PdfContentByte cb = writer.DirectContent;
            //Write out the table to the middle of the document
            myTable.WriteSelectedRows(0, -1, 0, -1, 20, 700, cb);
            doc.Close();

            fs.Position = 0;
            MemoryStream fs2 = new MemoryStream();
            PdfReader reader2 = new PdfReader(fs);
            PdfStamper stamper2 = new PdfStamper(reader2, fs2);
            stamper2.AcroFields.GenerateAppearances = false;
            stamper2.Close();
            reader2.Close();

            PdfReader reader = new PdfReader(new MemoryStream(fs2.ToArray()));
            PdfStamper stamper = new PdfStamper(reader, new FileStream(OUTPUT_FOLDER + file.Clone(), FileMode.Create));
            stamper.FormFlattening = true;
            stamper.Close();
            reader.Close();

            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER + file, RESOURCES_FOLDER + file,
                OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public void TestAnnotationFlatteningWithSkewAndRotation() {
            String file = "annotationWithTransformMatrix.pdf";
            PdfReader reader = new PdfReader(RESOURCES_FOLDER + file);
            PdfStamper stamper = new PdfStamper(reader, new FileStream(OUTPUT_FOLDER + file, FileMode.Create));
            stamper.Writer.CompressionLevel = 0;
            stamper.AnnotationFlattening = true;
            stamper.Close();
            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER + file, RESOURCES_FOLDER + "cmp_" + file,
                OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public void TestRotatedFilledField() {
            String file = "rotatedField.pdf";

            PdfReader pdfReader = new PdfReader(RESOURCES_FOLDER + file);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(OUTPUT_FOLDER + file, FileMode.Create));

            AcroFields fields = pdfStamper.AcroFields;
            fields.SetField("Text1", "TEST");
            fields.GenerateAppearances = true;

            pdfStamper.FormFlattening = true;
            pdfStamper.Close();
            pdfReader.Close();
            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER + file, RESOURCES_FOLDER + "cmp_" + file,
                OUTPUT_FOLDER, "diff");
            if (errorMessage != null)
            {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public void TestFreeTextRecangleBBoxInteraction()
        {
            String file = "freeTextRectangleBBoxInteraction.pdf";

            PdfReader pdfReader = new PdfReader(RESOURCES_FOLDER + file);
            PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(OUTPUT_FOLDER + file, FileMode.Create));

            pdfStamper.FormFlattening = true;
            pdfStamper.FreeTextFlattening = true;
            pdfStamper.Close();
            pdfReader.Close();
            // compare
            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER + file, RESOURCES_FOLDER + "cmp_" + file,
                OUTPUT_FOLDER, "diff");
            if (errorMessage != null)
            {
                Assert.Fail(errorMessage);
            }
        }
    }
}
