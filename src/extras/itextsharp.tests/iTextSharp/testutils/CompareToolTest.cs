using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.testutils;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.testutils {
    class CompareToolTest {

        private const string TEST_RESOURCES_PATH = @"..\..\resources\testutils\CompareToolTest\";
        private const string OUT_FOLDER = @"CompareToolTest\";

        [SetUp]
        public void SetUp() {
            new DirectoryInfo(OUT_FOLDER).Create();
        }

        [Test]
        public void Test1() {
            CompareTool compareTool = new CompareTool();
            compareTool.SetCompareByContentErrorsLimit(10);
            compareTool.SetGenerateCompareByContentXmlReport(true);

            String outPdf = TEST_RESOURCES_PATH + "simple_pdf.pdf";
            String cmpPdf = TEST_RESOURCES_PATH + "cmp_simple_pdf.pdf";
            String result = compareTool.CompareByContent(outPdf, cmpPdf, OUT_FOLDER, "difference");

            Assert.NotNull(result);
            Console.WriteLine(result);
        }

        [Test]
        public void Test2() {
            CompareTool compareTool = new CompareTool();
            compareTool.SetCompareByContentErrorsLimit(10);
            compareTool.SetGenerateCompareByContentXmlReport(true);

            String outPdf = TEST_RESOURCES_PATH + "tagged_pdf.pdf";
            String cmpPdf = TEST_RESOURCES_PATH + "cmp_tagged_pdf.pdf";
            String result = compareTool.CompareByContent(outPdf, cmpPdf, OUT_FOLDER, "difference");

            Assert.NotNull(result);
            Console.WriteLine(result);
        }
    }
}
