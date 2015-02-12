using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup;
using NUnit.Framework;
using Org.BouncyCastle.Utilities;

namespace itextsharp.tests.iTextSharp.text.pdf.pdfcleanup {
    class PdfCleanUpProcessorTest {

        private static readonly string INPUT_PATH = @"..\..\resources\text\pdf\pdfcleanup\PdfCleanUpProcessorTest\";
        private static readonly string OUTPUT_PATH = @"PdfCleanUpProcessorTest\";

        [Test, TestCaseSourceAttribute("TestData")]
        public virtual void CleanUp(string input, string output, string cmp, IList<PdfCleanUpLocation> cleanUpLocations) {
            string completeInputPath = INPUT_PATH + input;
            string completeCmpPath = INPUT_PATH + cmp;
            string completeOutputPath = OUTPUT_PATH + output;

            CleanUp(completeInputPath, completeOutputPath, cleanUpLocations);
            CompareByContent(completeCmpPath, completeOutputPath, OUTPUT_PATH, "diff");
        }

        private static object[] TestData() {
            IList<PdfCleanUpLocation> cleanUpLocations1 = new List<PdfCleanUpLocation>(); 
            cleanUpLocations1.Add(new PdfCleanUpLocation(1, new Rectangle(240.0f, 602.3f, 275.7f, 614.8f), BaseColor.GRAY));
            cleanUpLocations1.Add(new PdfCleanUpLocation(1, new Rectangle(171.3f, 550.3f, 208.4f, 562.8f), BaseColor.GRAY));
            cleanUpLocations1.Add(new PdfCleanUpLocation(1, new Rectangle(270.7f, 459.2f, 313.1f, 471.7f), BaseColor.GRAY));
            cleanUpLocations1.Add(new PdfCleanUpLocation(1, new Rectangle(249.9f, 329.3f, 279.6f, 341.8f), BaseColor.GRAY));
            cleanUpLocations1.Add(new PdfCleanUpLocation(1, new Rectangle(216.2f, 303.3f, 273.0f, 315.8f), BaseColor.GRAY));

            IList<PdfCleanUpLocation> cleanUpLocations2 = new List<PdfCleanUpLocation>();
            cleanUpLocations2.Add(new PdfCleanUpLocation(1, new Rectangle(97f, 405f, 480f, 445f), BaseColor.GRAY));

            IList<PdfCleanUpLocation> cleanUpLocations3 = new List<PdfCleanUpLocation>();
            cleanUpLocations3.Add(new PdfCleanUpLocation(1, new Rectangle(97f, 605f, 480f, 645f), BaseColor.GRAY));

            return new object[] {
                new object[] {"page229.pdf", "page229_01.pdf", "cmp_page229_01.pdf", cleanUpLocations1},
                new object[] {"page229-modified-Tc-Tw.pdf", "page229-modified-Tc-Tw.pdf", "cmp_page229-modified-Tc-Tw.pdf", cleanUpLocations1},
                new object[] {"page166_03.pdf", "page166_03.pdf", "cmp_page166_03.pdf", null},
                new object[] {"page166_04.pdf", "page166_04.pdf", "cmp_page166_04.pdf", null},
                new object[] {"hello_05.pdf", "hello_05.pdf", "cmp_hello_05.pdf", null},
                new object[] {"BigImage-jpg.pdf", "BigImage-jpg.pdf", "cmp_BigImage-jpg.pdf", null},
                new object[] {"BigImage-png.pdf", "BigImage-png.pdf", "cmp_BigImage-png.pdf", null},
                new object[] {"BigImage-tif.pdf", "BigImage-tif.pdf", "cmp_BigImage-tif.pdf", null},
                new object[] {"BigImage-tif-lzw.pdf", "BigImage-tif-lzw.pdf", "cmp_BigImage-tif-lzw.pdf", null},
                new object[] {"simpleImmediate.pdf", "simpleImmediate.pdf", "cmp_simpleImmediate.pdf", cleanUpLocations2},
                new object[] {"simpleImmediate-tm.pdf", "simpleImmediate-tm.pdf", "cmp_simpleImmediate-tm.pdf", cleanUpLocations2},
                new object[] {"multiUseIndirect.pdf", "multiUseIndirect.pdf", "cmp_multiUseIndirect.pdf", cleanUpLocations3},
                new object[] {"multiUseImage.pdf", "multiUseImage.pdf", "cmp_multiUseImage.pdf", cleanUpLocations2},
                new object[] {"smaskImage.pdf", "smaskImage.pdf", "cmp_smaskImage.pdf", cleanUpLocations2}
            };
        }

        private void CleanUp(String input, String output, IList<PdfCleanUpLocation> cleanUpLocations) {
            DirectoryInfo outDir = new DirectoryInfo(OUTPUT_PATH);

            if (!outDir.Exists) {
                outDir.Create();
            }

            PdfReader reader = new PdfReader(input);
            Stream fos = new FileStream(output, FileMode.Create);
            PdfStamper stamper = new PdfStamper(reader, fos);

            PdfCleanUpProcessor cleaner = (cleanUpLocations == null)? new PdfCleanUpProcessor(stamper) : new PdfCleanUpProcessor(cleanUpLocations, stamper);
            cleaner.CleanUp();

            stamper.Close();
            fos.Close();
            reader.Close();
        }

        private void CompareByContent(String cmp, String output, String targetDir, String operation) {
            CompareTool cmpTool = new CompareTool();
            cmpTool.SetCompareByContentErrorsLimit(10);

            String errorMessage = cmpTool.CompareByContent(output, cmp, targetDir, operation);

            if (errorMessage != null) {
                Assert.Fail(errorMessage);
            }
        }
    }
}
