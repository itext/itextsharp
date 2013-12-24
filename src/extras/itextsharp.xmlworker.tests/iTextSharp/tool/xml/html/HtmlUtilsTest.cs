using iTextSharp.text.log;
using iTextSharp.tool.xml.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html {
    /**
 * @author redlab_b
 *
 */

    internal class HtmlUtilsTest {
        [SetUp]
        virtual public void SetUp() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
        }

//	[Test]
//	public void testSpaceLeading() {
//		Assert.AreEqual("leading space removed", util.Sanitize(" leading space removed"));
//	}
//
//	[Test]
//	public void testSpaceTrailing() {
//		Assert.AreEqual("trailing space removed", util.Sanitize("trailing space removed "));
//	}

        [Test]
        virtual public void TestRTN() {
            Assert.AreEqual(" ", HTMLUtils.Sanitize("\r\n\t", false)[0].ToString());
        }

        [Test]
        virtual public void TestRTNinline() {
            Assert.AreEqual(" ", HTMLUtils.SanitizeInline("\r\n\t", false)[0].ToString());
        }
    }
}
