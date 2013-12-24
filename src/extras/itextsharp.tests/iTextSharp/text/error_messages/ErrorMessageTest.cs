using iTextSharp.text.error_messages;
using NUnit.Framework;

namespace itextsharp.tests.text.error_messages
{
    public class ErrorMessageTest
    {
        /*
         * using 1.unsupported.jpeg.marker.2={1}: unsupported JPEG marker: {2}
         */
        [Test]
        virtual public void VerifyParamReplacementNoParam()
        {
            Assert.AreEqual("{1}: unsupported JPEG marker: {2}",
                    MessageLocalization
                            .GetComposedMessage("1.unsupported.jpeg.marker.2"));
        }

        [Test]
        virtual public void VerifyParamReplacement1Param()
        {
            Assert.AreEqual("one: unsupported JPEG marker: {2}",
                    MessageLocalization.GetComposedMessage(
                            "1.unsupported.jpeg.marker.2", "one"));
        }

        [Test]
        virtual public void VerifyParamReplacement2Param()
        {
            Assert.AreEqual("one: unsupported JPEG marker: two",
                    MessageLocalization.GetComposedMessage(
                            "1.unsupported.jpeg.marker.2", "one", "two"));
        }

        [Test]
        virtual public void VerifyParamReplacementXParam()
        {
            Assert.AreEqual("one: unsupported JPEG marker: two",
                    MessageLocalization.GetComposedMessage(
                            "1.unsupported.jpeg.marker.2", "one", "two", "three",
                            "four", "five"));
        }
    }
}
