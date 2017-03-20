/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
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
