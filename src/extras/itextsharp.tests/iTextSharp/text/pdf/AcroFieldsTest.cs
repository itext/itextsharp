using System;
using System.Collections.Generic;
using System.IO;
using itextsharp.tests.iTextSharp.testutils;
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
    }
}
