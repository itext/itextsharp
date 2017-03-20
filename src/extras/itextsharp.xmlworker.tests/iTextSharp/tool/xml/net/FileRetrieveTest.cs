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
