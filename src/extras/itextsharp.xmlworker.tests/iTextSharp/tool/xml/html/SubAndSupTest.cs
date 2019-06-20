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
using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html {
    /**
 * @author Balder
 *
 */

    internal class SubAndSupTest {
        private static List<IElement> elementList;
        private const string RESOURCES = @"..\..\resources\";

        private class CustomElementHandler : IElementHandler {
            virtual public void Add(IWritable w) {
                elementList.AddRange(((WritableElement) w).Elements());
            }
        }

        [SetUp]
        virtual public void SetUp() {
            LoggerFactory.GetInstance().SetLogger(new SysoLogger(3));
            StreamReader bis = File.OpenText(RESOURCES + "/snippets/br-sub-sup_snippet.html");
            XMLWorkerHelper helper = XMLWorkerHelper.GetInstance();
            elementList = new List<IElement>();
            helper.ParseXHtml(new CustomElementHandler(), bis);
        }

        [TearDown]
        virtual public void TearDown() {
            elementList = null;
        }

        [Test]
        virtual public void ResolveNumberOfElements() {
            Assert.AreEqual(8, elementList.Count); // Br's count for one element(Chunk.NEWLINE).
        }

        [Test]
        virtual public void ResolveNewLines() {
            Assert.AreEqual(Chunk.NEWLINE.Content, elementList[1].Chunks[0].Content);
            Assert.AreEqual(8f, elementList[1].Chunks[0].Font.Size, 0);
            Assert.AreEqual(Chunk.NEWLINE.Content, elementList[4].Chunks[1].Content);
        }

        [Test]
        virtual public void ResolveFontSize() {
            Assert.AreEqual(12, elementList[5].Chunks[0].Font.Size, 0);
            Assert.AreEqual(9.75f, elementList[5].Chunks[2].Font.Size, 0);
            Assert.AreEqual(24, elementList[6].Chunks[0].Font.Size, 0);
            Assert.AreEqual(18f, elementList[6].Chunks[2].Font.Size, 0);
        }

        [Test]
        virtual public void ResolveTextRise() {
            Assert.AreEqual(-9.75f/2, elementList[5].Chunks[2].GetTextRise(), 0);
            Assert.AreEqual(-9.75f/2, elementList[5].Chunks[4].GetTextRise(), 0);
            Assert.AreEqual(18/2 + 0.5, elementList[6].Chunks[2].GetTextRise(), 0);
            Assert.AreEqual(-18/2, elementList[6].Chunks[6].GetTextRise(), 0);
            Assert.AreEqual(0, elementList[7].Chunks[0].GetTextRise(), 0);
            Assert.AreEqual(-3, elementList[7].Chunks[2].GetTextRise(), 0);
            Assert.AreEqual(4, elementList[7].Chunks[14].GetTextRise(), 0);
            Assert.AreEqual(3, elementList[7].Chunks[22].GetTextRise(), 0);
        }

        [Test]
        virtual public void ResolvePhraseLeading() {
            Assert.IsTrue(Math.Abs(1.2f - ((Paragraph) elementList[5]).MultipliedLeading) < 0.0001);
            Assert.IsTrue(Math.Abs(1.2f - ((Paragraph) elementList[6]).MultipliedLeading) < 0.0001);
        }
    }
}
