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
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    class AcroFieldsTest {
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\AcroFieldsTest\";
        private const string OUT_FOLDER = @"AcroFieldsTest\";

        [SetUp]
        public virtual void SetUp() {
            TestResourceUtils.PurgeTempFiles();
            Directory.CreateDirectory(OUT_FOLDER);
        }

        [TearDown]
        public virtual void TearDown() {
            TestResourceUtils.PurgeTempFiles();
        }

        [Test]
        public virtual void TestSetFields() {
            SingleTest("register.xfdf");
        }

        [Test]
        public virtual void TestListInSetFields() {
            SingleTest("list_register.xfdf");
        }

        private void SingleTest(string xfdfResourceName) {
            // merging the FDF file
            PdfReader pdfreader = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, "SimpleRegistrationForm.pdf");
            PdfStamper stamp = new PdfStamper(pdfreader, new MemoryStream());
            string xfdfFile = TestResourceUtils.GetResourceAsTempFile(TEST_RESOURCES_PATH, xfdfResourceName);
            XfdfReader fdfreader = new XfdfReader(xfdfFile);
            AcroFields form = stamp.AcroFields;
            form.SetFields(fdfreader);
            stamp.Close();
        }


        private readonly string PDF_COMBO = TEST_RESOURCES_PATH + "choice_field_order.pdf";
        private readonly string PDF_COMBO_EXPORT = TEST_RESOURCES_PATH + "choice_field_order_export.pdf";

        private readonly string PDF_COMBO_FIELD_NAME = "choice_field";

        private readonly string[] PDF_COMBO_VALUES = {
            "Option 1",
            "Option 2",
            "Option 3"
        };

        private readonly string[] PDF_COMBO_EXPORT_VALUES = {
            "Export 1",
            "Export 2",
            "Export 3"
        };

        [Test]
        public virtual void TestComboboxAppearanceStateOrder() {
            try {
                CheckOrderOfAppearanceStates(PDF_COMBO, PDF_COMBO_FIELD_NAME, PDF_COMBO_VALUES);
            } catch (IOException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public virtual void TestComboboxDisplayValues() {
            try {
                PdfReader reader = new PdfReader(PDF_COMBO);
                AcroFields acroFields = reader.AcroFields;
                string[] actual = acroFields.GetListOptionDisplay(PDF_COMBO_FIELD_NAME);

                Assert.AreEqual(PDF_COMBO_VALUES.Length, actual.Length);

                for (int i = 0; i < PDF_COMBO_VALUES.Length; i++) {
                    Assert.AreEqual(PDF_COMBO_VALUES[i], actual[i]);
                }
            } catch (IOException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public virtual void TestComboboxExportValues() {
            try {
                PdfReader reader = new PdfReader(PDF_COMBO_EXPORT);
                AcroFields acroFields = reader.AcroFields;
                string[] actual = acroFields.GetListOptionExport(PDF_COMBO_FIELD_NAME);

                Assert.AreEqual(PDF_COMBO_EXPORT_VALUES.Length, actual.Length);

                for (int i = 0; i < PDF_COMBO_EXPORT_VALUES.Length; i++) {
                    Assert.AreEqual(PDF_COMBO_EXPORT_VALUES[i], actual[i]);
                }
            } catch (IOException e) {
                Assert.Fail(e.Message);
            }
        }

        private void CheckOrderOfAppearanceStates(string pdf, string fieldName, string[] expected) {
            PdfReader reader = new PdfReader(pdf);
            AcroFields acroFields = reader.AcroFields;
            string[] actual = acroFields.GetAppearanceStates(fieldName);

            Assert.AreEqual(expected.Length, actual.Length);

            for (int i = 0; i < expected.Length; i++) {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        [Test]
        public virtual void FdfTest() {
            String acroform_pdf = TEST_RESOURCES_PATH + "acroform.pdf";
            String barcode_jpg = TEST_RESOURCES_PATH + "barcode.jpg";
            String signature_pdf = TEST_RESOURCES_PATH + "signature.pdf";
            String outFdf = OUT_FOLDER + "acroform_fields.fdf";

            FileStream fos = new FileStream(outFdf, FileMode.Create);
            FdfWriter fdfWriter = new FdfWriter(fos);
            fdfWriter.File = Path.GetFullPath(acroform_pdf);

            fdfWriter.SetFieldAsString("FirstName", "Alexander");
            fdfWriter.SetFieldAsString("LastName", "Chingarev");

            //Add signature from external PDF.
            PdfReader signatureReader = new PdfReader(signature_pdf);
            fdfWriter.SetFieldAsTemplate("Signature", fdfWriter.GetImportedPage(signatureReader, 1));

            //Add barcode image
            Image img = Image.GetInstance(barcode_jpg);
            fdfWriter.SetFieldAsImage("Barcode", img);

            fdfWriter.Write();

            //Close signature PDF reader.
            signatureReader.Close();


            FdfReader fdfReader = new FdfReader(outFdf);
            Dictionary<String, PdfDictionary> fields = fdfReader.Fields;
            PdfDictionary barcode = fields["Barcode"];
            PdfStream n = barcode.GetAsDict(PdfName.AP).GetAsStream(PdfName.N);
            Assert.NotNull(n);
            byte[] b = FdfReader.GetStreamBytes((PRStream)n);
            Assert.AreEqual(51, b.Length);
            PdfStream img0 = n.GetAsDict(PdfName.RESOURCES).GetAsDict(PdfName.XOBJECT).GetAsStream(new PdfName("img0"));
            Assert.NotNull(img0);
            PdfDictionary signature = fields["Signature"];
            n = signature.GetAsDict(PdfName.AP).GetAsStream(PdfName.N);
            Assert.NotNull(n);
            b = FdfReader.GetStreamBytes((PRStream)n);
            Assert.AreEqual(24410, b.Length);
            fdfReader.Close();
        }

        [Test]
        public void IcelandicLettersInAcroFieldTest()  {

            String outFile = OUT_FOLDER+"icelandicLettersInAcroFieldTest.pdf";
            FileStream file = new FileStream(outFile,FileMode.Create);

            PdfReader reader = new PdfReader(new FileStream(TEST_RESOURCES_PATH + "HelveticaFont.pdf",FileMode.Open));

            PdfStamper stamper = new PdfStamper(reader, file);

            AcroFields fields = stamper.AcroFields;

            fields.SetField("Mitarbeiter", "ÁÁÁÁ ÓÓÓÓ Testð");

            stamper.Close();

            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(outFile, TEST_RESOURCES_PATH + "cmp_icelandicLettersInAcroFieldTest.pdf", OUT_FOLDER, "diff_");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public void SpecialCharactersInAcroFieldTest()  {

            String outFile = OUT_FOLDER+"specialCharactersInAcroFieldTest.pdf";
            FileStream file = new FileStream(outFile,FileMode.Create);

            PdfReader reader = new PdfReader(new FileStream(TEST_RESOURCES_PATH+"HelveticaFont.pdf",FileMode.Open));

            PdfStamper stamper = new PdfStamper(reader, file);
            AcroFields acroFields = stamper.AcroFields;
            acroFields.SetField("Mitarbeiter", "öäüß€@");
            stamper.Close();

            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(outFile, TEST_RESOURCES_PATH + "cmp_specialCharactersInAcroFieldTest.pdf", OUT_FOLDER, "diff_");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public void FlatteningRadioButtonFieldsTest()
        {

            String outFile = OUT_FOLDER + "flatteningRadioButtonFieldsTest.pdf";
            FileStream file = new FileStream(outFile, FileMode.Create);

            PdfReader reader = new PdfReader(new FileStream(TEST_RESOURCES_PATH + "radios_src.pdf", FileMode.Open));

            PdfStamper stamper = new PdfStamper(reader, file);
            AcroFields acroFields = stamper.AcroFields;
            acroFields.SetField("radiogroup", "1");
            stamper.Close();

            CompareTool compareTool = new CompareTool();
            String errorMessage = compareTool.CompareByContent(outFile, TEST_RESOURCES_PATH + "cmp_flatteningRadioButtonFieldsTest.pdf", OUT_FOLDER, "diff_");
            if (errorMessage != null)
            {
                Assert.Fail(errorMessage);
            }
        }
    }
}
