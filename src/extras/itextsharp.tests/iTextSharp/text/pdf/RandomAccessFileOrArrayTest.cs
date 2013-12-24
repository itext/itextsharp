using System.IO;
using iTextSharp.text.pdf;
using NUnit.Framework;
using iTextSharp.text.io;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class RandomAccessFileOrArrayTest
    {
        byte[] data;
        RandomAccessFileOrArray rafoa;

        [SetUp]
        virtual public void SetUp()
        {
            MemoryStream os = new MemoryStream();
            for (int i = 0; i < 10000; i++)
            {
                os.WriteByte((byte)i);
            }
            data = os.ToArray();
            rafoa = new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource(data));
        }


        [Test]
        virtual public void TestPushback_byteByByte()
        {

            Assert.AreEqual(data[0], (byte)rafoa.Read());
            Assert.AreEqual(data[1], (byte)rafoa.Read());
            byte pushBackVal = (byte)(data[1] + 42);
            rafoa.PushBack(pushBackVal);
            Assert.AreEqual(pushBackVal, (byte)rafoa.Read());
            Assert.AreEqual(data[2], (byte)rafoa.Read());
            Assert.AreEqual(data[3], (byte)rafoa.Read());

        }

        [Test]
        virtual public void TestSimple()
        {
            for (int i = 0; i < data.Length; i++)
            {
                Assert.AreEqual(data[i], (byte)rafoa.Read());
            }
        }

        [Test]
        virtual public void TestSeek()
        {
            RandomAccessFileOrArray rafoa = new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource(data));
            rafoa.Seek(72);
            for (int i = 72; i < data.Length; i++)
            {
                Assert.AreEqual(data[i], (byte)rafoa.Read());
            }
        }

        [Test]
        virtual public void TestFilePositionWithPushback()
        {
            RandomAccessFileOrArray rafoa = new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource(data));
            long offset = 72;
            rafoa.Seek(offset);
            Assert.AreEqual(offset, rafoa.FilePointer);
            byte pushbackVal = 42;
            rafoa.PushBack(pushbackVal);
            Assert.AreEqual(offset - 1, rafoa.FilePointer);
        }
    }
}
