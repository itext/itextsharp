using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml {
    internal class BugRunnerTest {
        public const String RESOURCES = @"..\..\resources\";
        public const String TARGET = @"BugRunnerTest\";
        private List<String> list = new List<String>();

        static BugRunnerTest() {
            // FontFactory.registerDirectories();
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
        }

        [SetUp]
        public void SetUp() {
            list.Add("3353957.html");
            list.Add("ol-test.html");
            list.Add("processing-instructions.html");

            Directory.CreateDirectory(TARGET + "bugs/");
        }

        [Test]
        public void RunBugSamples() {
            bool success = true;
            foreach (String str in list) {
                try {
                    Console.WriteLine(str);
                    Document doc = new Document();
                    PdfWriter writer = null;
                    try {
                        writer = PdfWriter.GetInstance(doc, new FileStream(TARGET + "/bugs/" + str
                                                                           + ".pdf", FileMode.Create));
                    }
                    catch (DocumentException e) {
                        Console.WriteLine(e);
                    }
                    doc.Open();
                    XMLWorkerHelper helper = XMLWorkerHelper.GetInstance();
                    helper.ParseXHtml(writer, doc, File.OpenText(RESOURCES + "/bugs/" + str));
                    doc.Close();
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    Console.WriteLine(e);
                    success = false;
                }
            }
            if (!success) {
                Assert.Fail();
            }
        }
    }
}