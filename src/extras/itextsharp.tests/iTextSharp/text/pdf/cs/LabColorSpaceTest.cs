using System;
using System.IO;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.cs {
    public class LabColorSpaceTest {
        public const String DEST_FOLDER = @"cs\LabColorSpaceTest\";
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\cs\LabColorSpaceTest\";

        [SetUp]
        public virtual void Init() {
            if (Directory.Exists(DEST_FOLDER)) {
                foreach (String file in Directory.GetFiles(DEST_FOLDER)) {
                    File.Delete(file);
                }
            } else {
                Directory.CreateDirectory(DEST_FOLDER);
            }
        }

        [Test]
        public virtual void LabSpotBasedGradient() {
            // step 1
            Document document = new Document(PageSize.A3);
            // step 2
            String dest_file = DEST_FOLDER + "/lab_spot_based_gradient.pdf";
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(dest_file, FileMode.Create));
            // step 3
            document.Open();
            // step 4
            PdfContentByte canvas = writer.DirectContent;
            PdfLabColor pdfLabColor = new PdfLabColor(new float[] {0.9505f, 1.0f, 1.0890f}, new float[] {0f, 0.5f, 1.5f},
                new float[] {-10, 100, -10, 100});
            PdfDeviceNColor pdfDeviceNNChannelColor =
                new PdfDeviceNColor(new PdfSpotColor[] {
                    new PdfSpotColor("lab3", pdfLabColor.Rgb2lab(new BaseColor(0, 217, 83))),
                    new PdfSpotColor("labBlue", pdfLabColor.Rgb2lab(new BaseColor(0, 0, 255)))
                });
            PdfSpotColor psc_lab3 = new PdfSpotColor("lab3", pdfLabColor.Rgb2lab(new BaseColor(0, 217, 83)));
            PdfSpotColor psc_lab2 = new PdfSpotColor("lab2", pdfLabColor.Rgb2lab(new BaseColor(70, 138, 96)));
            PdfSpotColor psc_lab1 = new PdfSpotColor("lab1", pdfLabColor.Rgb2lab(new BaseColor(255, 0, 0)));
            PdfSpotColor psc_lab_blue = new PdfSpotColor("labBlue", new BaseColor(0, 0, 100));

            PdfDeviceNColor pdfDeviceNNChannelColor2 = new PdfDeviceNColor(new PdfSpotColor[]{psc_lab_blue, psc_lab2, psc_lab1});

            ColorRectangle(canvas, pdfLabColor.Rgb2lab(new BaseColor(0, 0, 255)), 36, 878, 36, 36);
            ColorRectangle(canvas, pdfLabColor.Rgb2lab(new BaseColor(70, 138, 96)), 90, 878, 36, 36);
            ColorRectangle(canvas, pdfLabColor.Rgb2lab(new BaseColor(0, 217, 83)), 144, 878, 36, 36);
            ColorRectangle(canvas, pdfLabColor.Rgb2lab(new BaseColor(0, 50, 50)), 198, 878, 36, 36);

            ColorRectangle(canvas, pdfLabColor.Rgb2lab(new BaseColor(0, 0, 255)).ToRgb(), 36, 824, 36, 36);
            ColorRectangle(canvas, pdfLabColor.Rgb2lab(new BaseColor(70, 138, 96)).ToRgb(), 90, 824, 36, 36);
            ColorRectangle(canvas, pdfLabColor.Rgb2lab(new BaseColor(0, 217, 83)).ToRgb(), 144, 824, 36, 36);
            ColorRectangle(canvas, pdfLabColor.Rgb2lab(new BaseColor(0, 50, 50)).ToRgb(), 198, 824, 36, 36);


            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("lab1", pdfLabColor.Rgb2lab(new BaseColor(255, 0, 0))), 1f), 36, 986, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("lab1", pdfLabColor.Rgb2lab(new BaseColor(255, 0, 0))), 0.8f), 90, 986, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("lab1", pdfLabColor.Rgb2lab(new BaseColor(255, 0, 0))), 0.6f), 144, 986, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("lab1", pdfLabColor.Rgb2lab(new BaseColor(255, 0, 0))), 0.2f), 198, 986, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("lab2", pdfLabColor.Rgb2lab(new BaseColor(70, 138, 96))), 1f), 252, 986, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("lab2", pdfLabColor.Rgb2lab(new BaseColor(70, 138, 96))), 0.8f), 306, 986, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("lab2", pdfLabColor.Rgb2lab(new BaseColor(70, 138, 96))), 0.6f), 360, 986, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("lab2", pdfLabColor.Rgb2lab(new BaseColor(70, 138, 96))), 0.2f), 416, 986, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("lab3", pdfLabColor.Rgb2lab(new BaseColor(0, 217, 83))), 1.0f), 470, 986, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("lab3", pdfLabColor.Rgb2lab(new BaseColor(0, 217, 83))), 0.8f), 524, 986, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("lab3", pdfLabColor.Rgb2lab(new BaseColor(0, 217, 83))), 0.6f), 578, 986, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("lab3", pdfLabColor.Rgb2lab(new BaseColor(0, 217, 83))), 0.2f), 634, 986, 36, 36);

            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("rgb1", new BaseColor(255, 0, 0)), 1f), 36, 932, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("rgb1", new BaseColor(255, 0, 0)), 0.8f), 90, 932, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("rgb1", new BaseColor(255, 0, 0)), 0.6f), 144, 932, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("rgb1", new BaseColor(255, 0, 0)), 0.2f), 198, 932, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("rgb2", new BaseColor(70, 138, 96)), 1f), 252, 932, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("rgb2", new BaseColor(70, 138, 96)), 0.8f), 306, 932, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("rgb2", new BaseColor(70, 138, 96)), 0.6f), 360, 932, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("rgb2", new BaseColor(70, 138, 96)), 0.2f), 416, 932, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("rgb3", new BaseColor(0, 217, 83)), 1.0f), 470, 932, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("rgb3", new BaseColor(0, 217, 83)), 0.8f), 524, 932, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("rgb3", new BaseColor(0, 217, 83)), 0.6f), 578, 932, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("rgb3", new BaseColor(0, 217, 83)), 0.2f), 634, 932, 36, 36);

            ColorRectangle(canvas, new DeviceNColor(new PdfDeviceNColor(new PdfSpotColor[]{psc_lab2, psc_lab_blue, psc_lab1}), new float[]{0, 0.0f, 1}), 36, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(pdfDeviceNNChannelColor, new float[]{0.1f, 0.1f}), 90, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(pdfDeviceNNChannelColor, new float[]{0.2f, 0.2f}), 144, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(pdfDeviceNNChannelColor, new float[]{0.3f, 0.3f}), 198, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(pdfDeviceNNChannelColor, new float[]{0.4f, 0.4f}), 252, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(new PdfDeviceNColor(new PdfSpotColor[]{psc_lab2, psc_lab_blue, psc_lab1, psc_lab3}), new float[]{0.5f, 0.5f, 1, 0.5f}), 306, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(pdfDeviceNNChannelColor2, new float[]{0.6f, 0.1f, 0.5f}), 360, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(pdfDeviceNNChannelColor, new float[]{0.7f, 0.7f}), 416, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(pdfDeviceNNChannelColor, new float[]{0.8f, 0.8f}), 470, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(new PdfDeviceNColor(new PdfSpotColor[]{psc_lab2, psc_lab_blue, psc_lab1}), new float[]{0.9f, 0.9f, 1}), 524, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(pdfDeviceNNChannelColor, new float[]{1, 1}), 578, 770, 36, 36);

            canvas.SaveState();
            canvas.Rectangle(418, 412, -329, 189);
            canvas.Clip();
            canvas.NewPath();
            canvas.SaveState();
            canvas.ConcatCTM(329f, 0f, 0f, -329f, 89f, 506.5f);
            canvas.PaintShading(PdfShading.SimpleAxial(writer, 0, 0, 1, 0, new DeviceNColor(pdfDeviceNNChannelColor, new float[]{1, 0}), new DeviceNColor(pdfDeviceNNChannelColor, new float[]{ 0, 1})));
            canvas.RestoreState();
            canvas.RestoreState();
            canvas.SetColorStroke(new DeviceNColor(pdfDeviceNNChannelColor, new float[]{1, 1}));
            canvas.Rectangle(418, 412, -329, 189);
            canvas.Stroke();

            // step 5
            document.Close();

            CompareTool compareTool = new CompareTool(dest_file, TEST_RESOURCES_PATH +  "cmp_lab_spot_based_gradient.pdf");
            String error = compareTool.CompareByContent(DEST_FOLDER, "diff");
            if (error != null) {
                Assert.Fail(error);
            }
        }

        public virtual void ColorRectangle(PdfContentByte canvas,
            BaseColor color, float x, float y, float width, float height) {
            canvas.SaveState();
            canvas.SetColorFill(color);
            canvas.Rectangle(x, y, width, height);
            canvas.FillStroke();
            canvas.RestoreState();
        }
    }
}
