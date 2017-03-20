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
using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.html {
    /**
 * @author Balder
 *
 */

    internal class LineHeightLetterSpacingTest {
        private ITagProcessorFactory factory;
        private XMLWorker worker;
        private MemoryStream baos;
        private XMLWorkerHelper workerFactory;
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
            StreamReader bis = File.OpenText(RESOURCES + "/snippets/line-height_letter-spacing_snippet.html");
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
            Assert.AreEqual(7, elementList.Count);
        }

        [Test]
        virtual public void ResolveFontSize() {
            Assert.AreEqual(16, elementList[2].Chunks[0].Font.Size, 0);
            Assert.AreEqual(15, elementList[4].Chunks[0].Font.Size, 0);
        }

        [Test]
        virtual public void ResolveLeading() {
            Assert.IsTrue(Math.Abs(1.2f - ((Paragraph) elementList[0]).MultipliedLeading) < 0.0001f);
            Assert.AreEqual(8, ((Paragraph) elementList[1]).Leading, 0);
            // leading laten bepalen door inner line-height setting?
            Assert.AreEqual(160, ((Paragraph) elementList[2]).Leading, 0);
            Assert.AreEqual(21, ((Paragraph) elementList[3]).Leading, 0); //1.75em
            Assert.AreEqual(45, ((Paragraph) elementList[4]).Leading, 0);
        }

        [Test]
        virtual public void ResolveCharSpacing() {
            Assert.AreEqual(CssUtils.GetInstance().ParsePxInCmMmPcToPt("1.6pc"),
                elementList[5].Chunks[0].GetCharacterSpacing(), 0);
            Assert.AreEqual(CssUtils.GetInstance().ParseRelativeValue("0.83em", elementList[6].Chunks[2].Font.Size),
                elementList[6].Chunks[2].GetCharacterSpacing(), 0);
        }
    }
}
