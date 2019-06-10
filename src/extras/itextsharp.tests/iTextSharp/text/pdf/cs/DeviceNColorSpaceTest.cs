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
using System.IO;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.cs {
    public class DeviceNColorSpaceTest {

        public const String DEST_FOLDER = @"cs\DeviceNColorSpaceTest\";
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\cs\DeviceNColorSpaceTest\";

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
        public virtual void DeviceNSpotBasedGradient() {
            // step 1
            Document document = new Document(PageSize.A3);
            // step 2
            String dest_file = DEST_FOLDER + "/device_n_gradient_base.pdf";
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(dest_file, FileMode.Create));
            // step 3
            document.Open();
            // step 4
            PdfContentByte canvas = writer.DirectContent;
            PdfSpotColor psc_gray = new PdfSpotColor("iTextGray", new GrayColor(0f));
            PdfSpotColor psc_cmyk_yell = new PdfSpotColor("iTextYellow", new CMYKColor(0f, 0f, 1f, 0f));
            PdfSpotColor psc_cmyk_magen = new PdfSpotColor("iTextMagenta", new CMYKColor(0f, 1f, 0f, 0f));
            PdfSpotColor psc_rgb_blue = new PdfSpotColor("iTextBlue", new BaseColor(0, 0, 255));

            PdfDeviceNColor pdfDeviceNNChannelColor =
                new PdfDeviceNColor(new PdfSpotColor[] {psc_cmyk_yell, psc_cmyk_magen, psc_rgb_blue});
            PdfDeviceNColor pdfDeviceNNChannelColor2 =
                new PdfDeviceNColor(new PdfSpotColor[] {psc_cmyk_magen, psc_cmyk_yell, psc_rgb_blue});

            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("iTextGray", new GrayColor(0f)), 0.8f), 36, 824, 36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("iTextYellow", new CMYKColor(0f, 0f, 1f, 0f)), 0.8f), 90, 824,
                36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("iTextMagenta", new CMYKColor(0f, 1f, 0f, 0f)), 0.4f), 144, 824,
                36, 36);
            ColorRectangle(canvas, new SpotColor(new PdfSpotColor("iTextBlue", new BaseColor(0, 0, 255)), 0.7f), 198, 824, 36, 36);

            ColorRectangle(canvas,
                new DeviceNColor(new PdfDeviceNColor(new PdfSpotColor[] {psc_cmyk_yell, psc_cmyk_magen, psc_rgb_blue}),
                    new float[] {0, 0.0f, 1}), 36, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(pdfDeviceNNChannelColor, new float[] {0.1f, 0.1f, 1}), 90, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(pdfDeviceNNChannelColor, new float[] {0.2f, 0.2f, 1}), 144, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(pdfDeviceNNChannelColor, new float[] {0.3f, 0.3f, 1}), 198, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(pdfDeviceNNChannelColor, new float[] {0.4f, 0.4f, 1}), 252, 770, 36, 36);
            ColorRectangle(canvas,
                new DeviceNColor(new PdfDeviceNColor(new PdfSpotColor[] {psc_cmyk_yell, psc_cmyk_magen, psc_rgb_blue, psc_gray}),
                    new float[] {0.5f, 0.5f, 1, 0.5f}), 306, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(pdfDeviceNNChannelColor2, new float[] {0.6f, 0.1f, 1}), 360, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(pdfDeviceNNChannelColor, new float[] {0.7f, 0.7f, 1}), 416, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(pdfDeviceNNChannelColor, new float[] {0.8f, 0.8f, 1}), 470, 770, 36, 36);
            ColorRectangle(canvas,
                new DeviceNColor(new PdfDeviceNColor(new PdfSpotColor[] {psc_cmyk_yell, psc_cmyk_magen, psc_rgb_blue}),
                    new float[] {0.9f, 0.9f, 1}), 524, 770, 36, 36);
            ColorRectangle(canvas, new DeviceNColor(pdfDeviceNNChannelColor, new float[] {1, 1, 1}), 578, 770, 36, 36);

            PdfDeviceNColor pdfDeviceNColor = new PdfDeviceNColor(new PdfSpotColor[] {psc_cmyk_yell, psc_cmyk_magen, psc_rgb_blue});
            canvas.SetColorFill(new DeviceNColor(pdfDeviceNColor, new float[] {0, 0, 1}));
            canvas.Rectangle(36, 716, 36, 36);
            canvas.FillStroke();
            canvas.SetColorFill(new DeviceNColor(pdfDeviceNColor, new float[] {0.1f, 0.1f, 1}));
            canvas.Rectangle(90, 716, 36, 36);
            canvas.FillStroke();
            canvas.SetColorFill(new DeviceNColor(pdfDeviceNColor, new float[] {0.2f, 0.2f, 1}));
            canvas.Rectangle(144, 716, 36, 36);
            canvas.FillStroke();
            canvas.SetColorFill(new DeviceNColor(pdfDeviceNColor, new float[] {0.3f, 0.3f, 1}));
            canvas.Rectangle(198, 716, 36, 36);
            canvas.FillStroke();
            canvas.SetColorFill(new DeviceNColor(pdfDeviceNColor, new float[] {0.4f, 0.4f, 1}));
            canvas.Rectangle(252, 716, 36, 36);
            canvas.FillStroke();
            canvas.SetColorFill(new DeviceNColor(pdfDeviceNColor, new float[] {0.5f, 0.5f, 1}));
            canvas.Rectangle(306, 716, 36, 36);
            canvas.FillStroke();
            canvas.SetColorFill(new DeviceNColor(pdfDeviceNColor, new float[] {0.6f, 0.1f, 1}));
            canvas.Rectangle(360, 716, 36, 36);
            canvas.FillStroke();
            canvas.SetColorFill(new DeviceNColor(pdfDeviceNColor, new float[] {0.7f, 0.7f, 1}));
            canvas.Rectangle(416, 716, 36, 36);
            canvas.FillStroke();
            canvas.SetColorFill(new DeviceNColor(pdfDeviceNColor, new float[] {0.8f, 0.8f, 1}));
            canvas.Rectangle(470, 716, 36, 36);
            canvas.FillStroke();
            canvas.SetColorFill(new DeviceNColor(pdfDeviceNColor, new float[] {0.9f, 0.9f, 1}));
            canvas.Rectangle(524, 716, 36, 36);
            canvas.FillStroke();
            canvas.SetColorFill(new DeviceNColor(pdfDeviceNColor, new float[] {1, 1, 1}));
            canvas.Rectangle(578, 716, 36, 36);
            canvas.FillStroke();

            canvas.SaveState();
            canvas.Rectangle(418, 412, -329, 189);
            canvas.Clip();
            canvas.NewPath();
            canvas.SaveState();
            canvas.ConcatCTM(329f, 0f, 0f, -329f, 89f, 506.5f);
            canvas.PaintShading(PdfShading.SimpleAxial(writer, 0, 0, 1, 0,
                new DeviceNColor(pdfDeviceNNChannelColor, new float[] {1, 1, 0}),
                new DeviceNColor(pdfDeviceNNChannelColor, new float[] {0, 0, 1})));
            canvas.RestoreState();
            canvas.RestoreState();
            canvas.SetColorStroke(new DeviceNColor(pdfDeviceNNChannelColor, new float[] {1, 1, 1}));
            canvas.Rectangle(418, 412, -329, 189);
            canvas.Stroke();

            // step 5
            document.Close();

            CompareTool compareTool = new CompareTool();
            String error = compareTool.Compare(dest_file, TEST_RESOURCES_PATH + "cmp_device_n_gradient_base.pdf", DEST_FOLDER, "diff_");
            if (error != null) {
                Assert.Fail(error);
            }
        }

        [Test]
        public virtual void DeviceNCmykRedRgbBlueGradient() {
            Document document = new Document();
            // step 2
            String dest_file = DEST_FOLDER + "/device_n_gradient_CmykRedRgbBlue.pdf";
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(dest_file, FileMode.Create));
            // step 3
            document.Open();
            // step 4
            PdfContentByte canvas = writer.DirectContent;
            PdfSpotColor psc_red = new PdfSpotColor("Spot Red", new CMYKColor(0f, 1f, 1f, 0f));
            PdfSpotColor psc_blue = new PdfSpotColor("Spot Blue", new BaseColor(0, 0, 255));
            PdfDeviceNColor deviceNColor = new PdfDeviceNColor(new PdfSpotColor[] {psc_red, psc_blue});

            canvas.SaveState();
            canvas.Rectangle(418, 412, -329, 189);
            canvas.Clip();
            canvas.NewPath();
            canvas.SaveState();
            canvas.ConcatCTM(329f, 0f, 0f, -329f, 89f, 506.5f);
            canvas.PaintShading(PdfShading.SimpleAxial(writer, 0, 0, 1, 0, new DeviceNColor(deviceNColor, new float[] {1, 0}),
                new DeviceNColor(deviceNColor, new float[] {0, 1})));
            canvas.RestoreState();
            canvas.RestoreState();
            canvas.SetCMYKColorStroke(0, 0, 0, 0xFF);
            canvas.Rectangle(418, 412, -329, 189);
            canvas.Stroke();
            document.Close();

            CompareTool compareTool = new CompareTool();
            String error = compareTool.Compare(dest_file, TEST_RESOURCES_PATH + "cmp_device_n_gradient_CmykRedRgbBlue.pdf", DEST_FOLDER, "diff_");
            if (error != null) {
                Assert.Fail(error);
            }
        }

        public virtual void ColorRectangle(PdfContentByte canvas, BaseColor color, float x, float y, float width, float height) {
            canvas.SaveState();
            canvas.SetColorFill(color);
            canvas.Rectangle(x, y, width, height);
            canvas.FillStroke();
            canvas.RestoreState();
        }

    }
}
