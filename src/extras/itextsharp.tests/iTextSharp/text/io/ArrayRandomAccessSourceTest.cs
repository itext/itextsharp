using System;
using System.IO;
using NUnit.Framework;
using iTextSharp.text.io;

namespace itextsharp.tests.iTextSharp.text.io
{
    public class ArrayRandomAccessSourceTest
    {
        byte[] data;

        [SetUp]
        virtual public void Initialize()
        {
            Random r = new Random(42);

            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buf = new byte[1];
                for (int i = 0; i < (1 << 10); i++)
                {
                    r.NextBytes(buf);
                    ms.Write(buf, 0, 1);
                }

                data = ms.ToArray();
            }
        }

        [Test]
        virtual public void TestGet()
        {
            ArrayRandomAccessSource s = new ArrayRandomAccessSource(data);
            try
            {
                for (int i = 0; i < data.Length; i++)
                {
                    int ch = s.Get(i);
                    Assert.IsFalse(ch == -1);
                    Assert.AreEqual(data[i], (byte)ch, "Position " + i);
                }
                Assert.AreEqual(-1, s.Get(data.Length));
            }
            finally
            {
                s.Close();
            }
        }

        private void AssertArrayEqual(byte[] a, int offa, byte[] b, int offb, int len)
        {
            for (int i = 0; i < len; i++)
            {
                if (a[i + offa] != b[i + offb])
                {
                    throw new AssertionException("Differ at index " + (i + offa) + " and " + (i + offb));
                }

            }
        }

        [Test]
        virtual public void TestGetArray()
        {
            byte[] chunk = new byte[257];
            ArrayRandomAccessSource s = new ArrayRandomAccessSource(data);
            try
            {
                int pos = 0;
                int count = s.Get(pos, chunk, 0, chunk.Length);
                while (count != -1)
                {
                    AssertArrayEqual(data, pos, chunk, 0, count);
                    pos += count;
                    count = s.Get(pos, chunk, 0, chunk.Length);
                }

                Assert.AreEqual(-1, s.Get(pos, chunk, 0, chunk.Length));
            }
            finally
            {
                s.Close();
            }
        }
    }
}
