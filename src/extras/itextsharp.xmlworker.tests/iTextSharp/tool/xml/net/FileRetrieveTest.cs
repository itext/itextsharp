using System;
using System.IO;
using System.Text;
using iTextSharp.text.log;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.net;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.net {
    internal class FileRetrieveTest {
        static FileRetrieveTest() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
        }

        private static ILogger LOG = LoggerFactory.GetLogger(typeof (FileRetrieveTest));
        private FileRetrieveImpl retriever;
        private string expected;
        private string actual;
        private const string RESOURCES = @"..\..\resources\";
        private const string TARGET = @"FileRetrieveTest\";

        private static FileStream output;

        [SetUp]
        virtual public void SetUp() {
            retriever = new FileRetrieveImpl();
            actual = TARGET + "css/actual.css";
            expected = RESOURCES + "css/test.css";

            Directory.CreateDirectory(TARGET + @"css\");
        }

        [TearDown]
        virtual public void TearDown() {
            output = null;

            try {
                File.Delete(actual);
            }
            catch (Exception) {
            }
        }

        private class CustomReadingProcessor : IReadingProcessor {
            virtual public void Process(int inbit) {
                try {
                    byte[] bytes = Encoding.Default.GetBytes(new char[] {(char) inbit});
                    output.Write(bytes, 0, bytes.Length);
                }
                catch (IOException e) {
                    throw new RuntimeWorkerException(e);
                }
            }
        }

        [Test]
        virtual public void RetrieveURL() {
            if (File.Exists("http://itextsupport.com/files/testresources/css/test.css")) {
                output = new FileStream(actual, FileMode.Create);
                retriever.ProcessFromHref("http://itextsupport.com/files/testresources/css/test.css",
                    new CustomReadingProcessor());
                output.Close();
            }
        }

        private static bool FilesEqual(string path1, string path2) {
            byte[] file1 = File.ReadAllBytes(path1);
            byte[] file2 = File.ReadAllBytes(path2);
            if (file1.Length == file2.Length) {
                for (int i = 0; i < file1.Length; i++) {
                    if (file1[i] != file2[i]) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        [Test]
        virtual public void RetrieveStreamFromFile() {
            output = new FileStream(actual, FileMode.Create);
            FileStream css = File.OpenRead(RESOURCES + @"\css\test.css");
            retriever.ProcessFromStream(css, new CustomReadingProcessor());
            css.Close();
            output.Close();
            Assert.IsTrue(FilesEqual(expected, actual));
        }

        [Test]
        virtual public void RetrieveFile() {
            output = new FileStream(actual, FileMode.Create);
            retriever.AddRootDir(RESOURCES);
            retriever.ProcessFromHref(@"css\test.css", new CustomReadingProcessor());
            output.Close();
            Assert.IsTrue(FilesEqual(expected, actual));
        }
    }
}
