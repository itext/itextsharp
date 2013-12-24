using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using iTextSharp.text.error_messages;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.error_messages {
    internal class ErrorMessageExistenceTest {
        private static String[] LANGUAGES = {"nl", "en"};
        private static String SOURCE_FILES_EXTENSION = ".cs";
        private static String SOURCE_CODE_ROOT_PATH = @"..\..\..\..\";
        private List<string> sourceFiles;
        private List<String> nonLozalizedMessageErrors;
        private Regex pattern;

        [SetUp]
        virtual public void SetUp() {
            pattern = new Regex("MessageLocalization.GetComposedMessage\\(\"([^\"]*)\"");
            sourceFiles = new List<string>();
            nonLozalizedMessageErrors = new List<String>();

            AddSourceFilesRecursively(SOURCE_CODE_ROOT_PATH);
        }

        [Test]
        virtual public void Test() {
            foreach (String language in LANGUAGES) {
                TestSingleLanguageLocalization(language);
            }

            Assert.IsTrue(nonLozalizedMessageErrors.Count == 0, "There exist messages without localization.");
        }

        private void AddSourceFilesRecursively(string folder) {
            foreach (string subfolder in Directory.GetDirectories(folder)) {
                AddSourceFilesRecursively(subfolder);
            }

            foreach (string file in Directory.GetFiles(folder)) {
                if (file.EndsWith(SOURCE_FILES_EXTENSION)) {
                    sourceFiles.Add(file);
                }
            }
        }

        private void TestSingleLanguageLocalization(String language) {
            MessageLocalization.SetLanguage(language, null);

            foreach (string file in sourceFiles) {
                TestOneSourceFile(file, language);
            }
        }

        private void TestOneSourceFile(string file, String language) {
            string fileContents = File.ReadAllText(file);

            MatchCollection matcher = pattern.Matches(fileContents);
            foreach (Match match in matcher) {
                String key = match.Groups[1].Value;
                String assertMessage = language + " localization for " + key + " message was not found. File " + file;

                if (MessageLocalization.GetMessage(key, false).StartsWith("No message found for")) {
                    Console.WriteLine(assertMessage);
                    nonLozalizedMessageErrors.Add(assertMessage);
                }

                //Assert.IsFalse(MessageLocalization.GetMessage(key, false).StartsWith("No message found for"), assertMessage);
            }
        }
    }
}
