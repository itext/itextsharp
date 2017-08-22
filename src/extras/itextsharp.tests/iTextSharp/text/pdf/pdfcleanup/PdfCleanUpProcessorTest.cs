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

            List<PdfCleanUpLocation> cleanUpLocations4 = new List<PdfCleanUpLocation>();
            cleanUpLocations4.Add(new PdfCleanUpLocation(1, new Rectangle(212, 394, 212 + 186, 394 + 170), null));

            List<PdfCleanUpLocation> cleanUpLocations5 = new List<PdfCleanUpLocation>();
            cleanUpLocations5.Add(new PdfCleanUpLocation(1, new Rectangle(0f, 0f, 595f, 680f), BaseColor.GRAY));

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
                new object[] {"smaskImage.pdf", "smaskImage.pdf", "cmp_smaskImage.pdf", cleanUpLocations2},
                new object[] {"rotatedImg.pdf", "rotatedImg.pdf", "cmp_rotatedImg.pdf", cleanUpLocations2},
                new object[] {"lineArtsCompletely.pdf", "lineArtsCompletely.pdf", "cmp_LineArtsCompletely.pdf", null},
                new object[] {"lineArtsPartially.pdf", "lineArtsPartially.pdf", "cmp_lineArtsPartially.pdf", null},
                new object[] {"dashedStyledClosedBezier.pdf", "dashedStyledClosedBezier.pdf", "cmp_dashedStyledClosedBezier.pdf", null},
                new object[] {"styledLineArts.pdf", "styledLineArts.pdf", "cmp_styledLineArts.pdf", null},
                new object[] {"dashedBezier.pdf", "dashedBezier.pdf", "cmp_dashedBezier.pdf", null},
                new object[] {"closedBezier.pdf", "closedBezier.pdf", "cmp_closedBezier.pdf", null},
                new object[] {"clippingNWRule.pdf", "clippingNWRule.pdf", "cmp_clippingNWRule.pdf", null},
                new object[] {"dashedClosedRotatedTriangles.pdf", "dashedClosedRotatedTriangles.pdf", "cmp_dashedClosedRotatedTriangles.pdf", null},
                new object[] {"miterTest.pdf", "miterTest.pdf", "cmp_miterTest.pdf", null},
                new object[] {"degenerateCases.pdf", "degenerateCases.pdf", "cmp_degenerateCases.pdf", null},
                new object[] {"absentICentry.pdf", "absentICentry.pdf", "cmp_absentICentry.pdf", null},
                new object[] {"lotOfDashes.pdf", "lotOfDashes.pdf", "cmp_lotOfDashes.pdf", null},
                new object[] {"clipPathReduction.pdf", "clipPathReduction.pdf", "cmp_clipPathReduction.pdf", cleanUpLocations4},
                new object[] {"helloHelvetica.pdf", "helloHelvetica.pdf", "cmp_helloHelvetica.pdf", cleanUpLocations5},

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
