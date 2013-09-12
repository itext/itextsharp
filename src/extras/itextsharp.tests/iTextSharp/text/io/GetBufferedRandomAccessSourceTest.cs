using iTextSharp.text.io;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.io
{
    class GetBufferedRandomAccessSourceTest
    {
        [Test]
        public void TestSmallSizedFile()
        {
            // we had a problem if source was less than 4 characters in length - would result in array index out of bounds problems on get()
            byte[] data = new byte[] { 42 };
            ArrayRandomAccessSource arrayRAS = new ArrayRandomAccessSource(data);
            GetBufferedRandomAccessSource bufferedRAS = new GetBufferedRandomAccessSource(arrayRAS);
            Assert.AreEqual(42, bufferedRAS.Get(0));
        }
    }
}
