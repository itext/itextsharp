using iTextSharp.text;
using iTextSharp.text.log;

namespace itextsharp.xmlworker.tests.examples {
    /**
 * @author Balder Van Camp
 *
 */

    internal class Setup {
        static Setup() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger());
            FontFactory.RegisterDirectories();
        }
    }
}
