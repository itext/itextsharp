using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    public class DefaultSplitCharacterProfilingTest
    {
        private const string INPUT_DIR = @"..\..\..\resources\text\pdf\DefaultSplitCharacterProfilingTest\";

        private const string CHECK_DATE_PATTERN_FAIL_MESSAGE =
            "The test verifies the optimization of the checkDatePattern method. This failure indicates that the optimization was broken.";

        private const int TIME_LIMIT = 5000;

        [Test, Timeout(30000)]
        public void CheckDatePatternProfilingTest()
        {
            string testFile = INPUT_DIR + "profilingText.txt";
            string str = ReadFile(testFile);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < 10000; i++)
            {
                IsSplitCharacter(str);
            }

            stopwatch.Stop();
            Console.WriteLine("Test run time: " + stopwatch.ElapsedMilliseconds);
            Assert.True(stopwatch.ElapsedMilliseconds < TIME_LIMIT, CHECK_DATE_PATTERN_FAIL_MESSAGE);
        }

        private static void IsSplitCharacter(string text)
        {
            new DefaultSplitCharacter().IsSplitCharacter(0, 0, text.Length + 1, text.ToCharArray(), null);
        }

        private static string ReadFile(string fileName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            using (StreamReader file = new StreamReader(fileName))
            {
                string ln;

                while ((ln = file.ReadLine()) != null)
                {
                    stringBuilder.Append(ln);
                }

                file.Close();
            }

            return stringBuilder.ToString();
        }
    }
}