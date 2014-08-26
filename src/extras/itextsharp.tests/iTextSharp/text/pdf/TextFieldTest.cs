using System;
using System.IO;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.events;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf {
    public class TextFieldTest {
        private const string CMP_FOLDER = @"..\..\resources\text\pdf\TextFieldTest\";
        private const string OUTPUT_FOLDER = @"TextFieldTest\";

        [TestFixtureSetUp]
        public static void Init() {
            Directory.CreateDirectory(OUTPUT_FOLDER);
        }

        [Test]
        public virtual void TestVisibleTopChoice() {
            int[] testValues = new int[] {-3, 0, 2, 3};
            int[] expectedValues = new int[] {-1, 0, 2, -1};

            for (int i = 0; i < testValues.Length; i++) {
                VisibleTopChoiceHelper(testValues[i], expectedValues[i], "textfield-top-visible-" + i + ".pdf");
            }
        }


        private void VisibleTopChoiceHelper(int topVisible, int expected, String file) {
            Document document = new Document();
            FileStream fos = new FileStream(OUTPUT_FOLDER + file, FileMode.Create);
            PdfWriter writer = PdfWriter.GetInstance(document, fos);
            document.Open();

            TextField textField = new TextField(writer, new Rectangle(380, 560, 500, 610), "testListBox");

            textField.Visibility = BaseField.VISIBLE;
            textField.Rotation = 0;

            textField.FontSize = 14f;
            textField.TextColor = BaseColor.MAGENTA;

            textField.BorderColor = BaseColor.BLACK;
            textField.BorderStyle = PdfBorderDictionary.STYLE_SOLID;

            textField.Font = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.WINANSI, BaseFont.EMBEDDED);
            textField.BorderWidth = BaseField.BORDER_WIDTH_THIN;

            writer.RunDirection = PdfWriter.RUN_DIRECTION_LTR;

            textField.Choices = new String[] {"one", "two", "three"};
            textField.ChoiceExports = new String[] {"1", "2", "3"};

            //choose the second item
            textField.ChoiceSelection = 1;
            //set the first item as the visible one
            textField.VisibleTopChoice = topVisible;

            Assert.AreEqual(expected, textField.VisibleTopChoice);

            PdfFormField field = textField.GetListField();

            writer.AddAnnotation(field);

            document.Close();

            // compare
            CompareTool compareTool = new CompareTool(OUTPUT_FOLDER + file, CMP_FOLDER + file);
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

        [Test]
        public void TestRegeneratingFields() {
            String file = "regenerateField.pdf";

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
            PdfReader reader = new PdfReader(fs);
            PdfStamper stamper = new PdfStamper(reader, new FileStream(OUTPUT_FOLDER + file.Clone(), FileMode.Create));
            stamper.FormFlattening = true;
            stamper.Close();
            reader.Close();

            // compare
            CompareTool compareTool = new CompareTool(OUTPUT_FOLDER + file, CMP_FOLDER + file);
            String errorMessage = compareTool.CompareByContent(OUTPUT_FOLDER, "diff");
            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }

    }
}
