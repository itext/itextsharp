/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
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
        }


        [Test]
        virtual public void TestGet()
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
        virtual public void TestGetArray()
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

        }

        [Test]
        virtual public void TestRelease()
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
