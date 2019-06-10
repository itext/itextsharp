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
using iTextSharp.text.pdf;

namespace itextsharp.tests.iTextSharp.testutils
{

    internal static class TestResourceUtils
    {
        private const string TESTPREFIX = "itexttest_";

        public static void PurgeTempFiles()
        {
            string[] itextTempFiles = Directory.GetFiles(Path.GetTempPath(), TESTPREFIX + "*");

            foreach (string filePath in itextTempFiles)
                File.Delete(filePath);
        }

        public static string GetFullyQualifiedResourceName(string testResourcesPath, string resourceName)
        {
            return testResourcesPath + resourceName;
        }

        public static FileStream GetResourceAsStream(string testResourcesPath, string resourceName)
        {
            return new FileStream(testResourcesPath + resourceName, FileMode.Open);
        }

        public static PdfReader GetResourceAsPdfReader(string testResourcesPath, string resourceName)
        {
            return new PdfReader(GetResourceAsStream(testResourcesPath, resourceName));
        }

        public static string GetResourceAsTempFile(string testResourcesPath, string resourceName)
        {
            FileStream istream = GetResourceAsStream(testResourcesPath, resourceName);
            string tempStream = WriteStreamToTempFile(resourceName, istream);

            return tempStream;
        }

        private static string WriteStreamToTempFile(string id, Stream istream)
        {
            string filename = Path.GetTempPath() + TESTPREFIX + id + "-" + ".pdf";
            if (istream == null) throw new NullReferenceException("Input stream is null");
            FileStream fs = File.Create(filename);

            WriteInputToOutput(istream, fs);
            istream.Close();
            fs.Close();

            return filename;
        }

        private static void WriteInputToOutput(Stream input, Stream output)
        {
            int bytesRead;
            byte[] buffer = new byte[1024 * 16];

            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                output.Write(buffer, 0, bytesRead);
        }

        public static byte[] GetResourceAsByteArray(string testResourcesPath, string resourceName)
        {
            FileStream inputStream = GetResourceAsStream(testResourcesPath, resourceName);

            MemoryStream ms = new MemoryStream();

            try
            {
                WriteInputToOutput(inputStream, ms);
            }
            finally
            {
                inputStream.Close();
            }

            return ms.ToArray();

        }

        /**
         * Used for testing only if we need to open the PDF itself
         * @param bytes
         * @param file
         * @throws Exception
         */
        public static void SaveBytesToFile(byte[] bytes, string filepath)
        {
            FileStream outputStream = new FileStream(filepath, FileMode.Create);
            outputStream.Write(bytes, 0, bytes.Length);
            outputStream.Close();
            Console.WriteLine("PDF dumped to " + filepath + " by the following calls:");
            Console.WriteLine("StackTrace: '{0}'", Environment.StackTrace);
        }
    }
}
