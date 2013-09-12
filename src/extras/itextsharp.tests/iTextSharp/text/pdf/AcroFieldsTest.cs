using System.IO;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class AcroFieldsTest
    {
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\AcroFieldsTest\";

        [SetUp]
        public void SetUp()
        {
            TestResourceUtils.PurgeTempFiles();
        }

        [TearDown]
        public void TearDown()
        {
            TestResourceUtils.PurgeTempFiles();
        }

        [Test]
        public void TestSetFields()
        {
            SingleTest("register.xfdf");
        }

        [Test]
        public void TestListInSetFields()
        {
            SingleTest("list_register.xfdf");
        }

        private void SingleTest(string xfdfResourceName)
        {
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
        public void TestComboboxAppearanceStateOrder()
        {
            try
            {
                CheckOrderOfAppearanceStates(PDF_COMBO, PDF_COMBO_FIELD_NAME, PDF_COMBO_VALUES);
            }
            catch (IOException e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestComboboxDisplayValues()
        {
            try
            {
                PdfReader reader = new PdfReader(PDF_COMBO);
                AcroFields acroFields = reader.AcroFields;
                string[] actual = acroFields.GetListOptionDisplay(PDF_COMBO_FIELD_NAME);

                Assert.AreEqual(PDF_COMBO_VALUES.Length, actual.Length);

                for (int i = 0; i < PDF_COMBO_VALUES.Length; i++)
                {
                    Assert.AreEqual(PDF_COMBO_VALUES[i], actual[i]);
                }
            }
            catch (IOException e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestComboboxExportValues()
        {
            try
            {
                PdfReader reader = new PdfReader(PDF_COMBO_EXPORT);
                AcroFields acroFields = reader.AcroFields;
                string[] actual = acroFields.GetListOptionExport(PDF_COMBO_FIELD_NAME);

                Assert.AreEqual(PDF_COMBO_EXPORT_VALUES.Length, actual.Length);

                for (int i = 0; i < PDF_COMBO_EXPORT_VALUES.Length; i++)
                {
                    Assert.AreEqual(PDF_COMBO_EXPORT_VALUES[i], actual[i]);
                }
            }
            catch (IOException e)
            {
                Assert.Fail(e.Message);
            }
        }

        private void CheckOrderOfAppearanceStates(string pdf, string fieldName, string[] expected)
        {
            PdfReader reader = new PdfReader(pdf);
            AcroFields acroFields = reader.AcroFields;
            string[] actual = acroFields.GetAppearanceStates(fieldName);

            Assert.AreEqual(expected.Length, actual.Length);

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i]);
            }
        }
    }
}
