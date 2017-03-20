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
using iTextSharp.text;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline.html;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.pipeline {
    internal class StackKeeperTest {
        private Chunk a;
        private Chunk b;
        private Chunk c;
        private StackKeeper sk;
        private Tag t;

        [SetUp]
        virtual public void SetUp() {
            t = new Tag("root");
            sk = new StackKeeper(t);
            a = new Chunk("a");
            sk.Add(a);
            b = new Chunk("b");
            sk.Add(b);
            c = new Chunk("c");
            sk.Add(c);
        }

        [Test]
        virtual public void ValidateFirstAddedElementIsFirst() {
            Assert.AreEqual(a, sk.GetElements()[0]);
        }

        [Test]
        virtual public void ValidateMiddleIsMiddle() {
            Assert.AreEqual(b, sk.GetElements()[1]);
        }

        [Test]
        virtual public void ValidateLastIsLast() {
            Assert.AreEqual(c, sk.GetElements()[2]);
        }

        [Test]
        virtual public void ValidateCount() {
            Assert.AreEqual(3, sk.GetElements().Count);
        }

        [Test]
        virtual public void ValidateTag() {
            Assert.AreEqual(t, sk.GetTag());
        }
    }
}
