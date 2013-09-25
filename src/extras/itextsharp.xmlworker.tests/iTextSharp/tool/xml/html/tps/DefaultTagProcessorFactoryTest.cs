using iTextSharp.text.log;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html.tps {
    /**
 *
 * @author redlab_b
 *
 */

    internal class DefaultTagProcessorFactoryTest {
        /**
	 * @author itextpdf.com
	 *
	 */

        private class TagProcessorImplementation : AbstractTagProcessor {
        }

        private ITagProcessorFactory tp;
        private TagProcessorImplementation tpi;

        [SetUp]
        public void SetUp() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            tp = Tags.GetHtmlTagProcessorFactory();
            tpi = new TagProcessorImplementation();
        }

        [Test]
        public void TestLoadDefaults() {
            Assert.IsTrue(tp.GetProcessor("a", "") is Anchor);
        }

        [Test]
        [ExpectedException(typeof (NoTagProcessorException))]
        public void LoadFail() {
            tp.GetProcessor("unknown", "");
        }

        [Test]
        public void AddTagProcessor() {
            tp.AddProcessor(tpi, new string[] {"addatag"});
            ITagProcessor processor = tp.GetProcessor("addatag", "");
            Assert.AreEqual(tpi, processor);
        }

        [Test]
        [ExpectedException(typeof (NoTagProcessorException))]
        public void RemoveTagProcessor() {
            tp.AddProcessor(tpi, new string[] {"addatag"});
            tp.RemoveProcessor("addatag");
            tp.GetProcessor("addatag", "");
        }
    }
}