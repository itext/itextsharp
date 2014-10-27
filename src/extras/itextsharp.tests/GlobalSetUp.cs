using iTextSharp.text.io;
using NUnit.Framework;

namespace itextsharp.tests {
    [SetUpFixture]
    public class GlobalSetUp {
        [SetUp]
        public virtual void SetUp() {
            StreamUtil.AddToResourceSearch(@"iTextAsian.dll");
        }
    }
}
