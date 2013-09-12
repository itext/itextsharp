using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using iTextSharp.text.io;

namespace itextsharp.tests.iTextSharp.text.io
{
    class GroupedRandomAccessSourceTest
    {
        byte[] data;

        [SetUp]
        public void SetUp()
        {

            using (MemoryStream ms = new MemoryStream())
            {
                for (int i = 0; i < 100; i++)
                {
                    ms.WriteByte((byte)i);
                }

                data = ms.ToArray();
            }
        }


        [Test]
        public void TestGet()
        {
            ArrayRandomAccessSource source1 = new ArrayRandomAccessSource(data);
            ArrayRandomAccessSource source2 = new ArrayRandomAccessSource(data);
            ArrayRandomAccessSource source3 = new ArrayRandomAccessSource(data);

            IRandomAccessSource[] inputs = { source1, source2, source3 };

            GroupedRandomAccessSource grouped = new GroupedRandomAccessSource(inputs);

            Assert.AreEqual(source1.Length + source2.Length + source3.Length, grouped.Length);

            Assert.AreEqual(source1.Get(99), grouped.Get(99));
            Assert.AreEqual(source2.Get(0), grouped.Get(100));
            Assert.AreEqual(source2.Get(1), grouped.Get(101));
            Assert.AreEqual(source1.Get(99), grouped.Get(99));
            Assert.AreEqual(source3.Get(99), grouped.Get(299));

            Assert.AreEqual(-1, grouped.Get(300));
        }

        private byte[] RangeArray(int start, int count)
        {
            byte[] rslt = new byte[count];
            for (int i = 0; i < count; i++)
            {
                rslt[i] = (byte)(i + start);
            }
            return rslt;
        }

        private void AssertArrayEqual(byte[] a, int offa, byte[] b, int offb, int len)
        {
            for (int i = 0; i < len; i++)
            {
                if (a[i + offa] != b[i + offb])
                {
                    throw new AssertionException("Differ at index " + (i + offa) + " and " + (i + offb) + " -> " + a[i + offa] + " != " + b[i + offb]);
                }

            }
        }

        [Test]
        public void TestGetArray()
        {
            ArrayRandomAccessSource source1 = new ArrayRandomAccessSource(data); // 0 - 99
            ArrayRandomAccessSource source2 = new ArrayRandomAccessSource(data); // 100 - 199
            ArrayRandomAccessSource source3 = new ArrayRandomAccessSource(data); // 200 - 299

            IRandomAccessSource[] inputs = new IRandomAccessSource[] { source1, source2, source3 };

            GroupedRandomAccessSource grouped = new GroupedRandomAccessSource(inputs);

            byte[] output = new byte[500];

            Assert.AreEqual(300, grouped.Get(0, output, 0, 300));
            AssertArrayEqual(RangeArray(0, 100), 0, output, 0, 100);
            AssertArrayEqual(RangeArray(0, 100), 0, output, 100, 100);
            AssertArrayEqual(RangeArray(0, 100), 0, output, 200, 100);

            Assert.AreEqual(300, grouped.Get(0, output, 0, 301));
            AssertArrayEqual(RangeArray(0, 100), 0, output, 0, 100);
            AssertArrayEqual(RangeArray(0, 100), 0, output, 100, 100);
            AssertArrayEqual(RangeArray(0, 100), 0, output, 200, 100);

            Assert.AreEqual(100, grouped.Get(150, output, 0, 100));
            AssertArrayEqual(RangeArray(50, 50), 0, output, 0, 50);
            AssertArrayEqual(RangeArray(0, 50), 0, output, 50, 50);
        }

        private class GroupedRandomAccessSourceTestClass : GroupedRandomAccessSource
        {
            public readonly IRandomAccessSource[] current = new IRandomAccessSource[] { null };
            public readonly int[] openCount = new int[] { 0 };

            public GroupedRandomAccessSourceTestClass(ICollection<IRandomAccessSource> sources) : base(sources)
            {
            }

            protected internal override void SourceReleased(IRandomAccessSource source)
            {
                openCount[0]--;
                if (current[0] != source)
                    throw new AssertionException("Released source isn't the current source");
                current[0] = null;
            }

            protected internal override void SourceInUse(IRandomAccessSource source)
            {
                if (current[0] != null)
                    throw new AssertionException("Current source wasn't released properly");
                openCount[0]++;
                current[0] = source;
            }

        };

        [Test]
        public void TestRelease()
        {

            ArrayRandomAccessSource source1 = new ArrayRandomAccessSource(data); // 0 - 99
            ArrayRandomAccessSource source2 = new ArrayRandomAccessSource(data); // 100 - 199
            ArrayRandomAccessSource source3 = new ArrayRandomAccessSource(data); // 200 - 299

            IRandomAccessSource[] sources = new IRandomAccessSource[]{
                    source1, source2, source3
            };

            
            GroupedRandomAccessSourceTestClass grouped = new GroupedRandomAccessSourceTestClass(sources);

            grouped.Get(250);
            grouped.Get(251);
            Assert.AreEqual(1, grouped.openCount[0]);
            grouped.Get(150);
            grouped.Get(151);
            Assert.AreEqual(1, grouped.openCount[0]);
            grouped.Get(50);
            grouped.Get(51);
            Assert.AreEqual(1, grouped.openCount[0]);
            grouped.Get(150);
            grouped.Get(151);
            Assert.AreEqual(1, grouped.openCount[0]);
            grouped.Get(250);
            grouped.Get(251);
            Assert.AreEqual(1, grouped.openCount[0]);

            grouped.Close();
        }
    }
}
