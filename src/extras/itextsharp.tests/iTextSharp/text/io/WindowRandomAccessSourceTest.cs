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
