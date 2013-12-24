using System.IO;
using iTextSharp.text.io;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.io
{
    class WindowRandomAccessSourceTest
    {
        ArrayRandomAccessSource source;
        byte[] data;

        [SetUp]
        virtual public void SetUp()
        {

            using (MemoryStream ms = new MemoryStream())
            {
                for (int i = 0; i < 100; i++)
                {
                    ms.WriteByte((byte)i);
                }

                data = ms.ToArray();
            }

            source = new ArrayRandomAccessSource(data);
        }

        [Test]
        virtual public void TestBasics()
        {
            WindowRandomAccessSource window = new WindowRandomAccessSource(source, 7, 17);

            Assert.AreEqual(17, window.Length);
            byte[] output = new byte[45];
            Assert.AreEqual(17, window.Get(0, output, 0, 17));

            Assert.AreEqual(7, window.Get(0));
            Assert.AreEqual(17, window.Get(10));
            Assert.AreEqual(-1, window.Get(17));

            Assert.AreEqual(17, window.Get(0, output, 0, 45));
            for (int i = 0; i < 17; i++)
            {
                Assert.AreEqual(data[i + 7], output[i]);
            }
        }
    }
}
