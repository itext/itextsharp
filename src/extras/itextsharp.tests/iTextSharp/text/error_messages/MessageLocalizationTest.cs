using System;
using iTextSharp.text.error_messages;
using NUnit.Framework;

namespace itextsharp.tests.text.error_messages
{
    public class MessageLocalizationTest
    {
        [Test]
        public void TestBackslashes()
        {
            String testPath = "C:\\test\\file.txt";
            String rslt = MessageLocalization.GetComposedMessage("1.not.found.as.file.or.resource", testPath);
            Assert.IsTrue(rslt.Contains(testPath), "Result doesn't contain the test path");
        }
    }
}
