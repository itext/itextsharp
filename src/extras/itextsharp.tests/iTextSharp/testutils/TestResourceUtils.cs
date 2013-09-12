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
