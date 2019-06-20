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
using System.Text;
using itextsharp.tests.iTextSharp.testutils;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser {
    public class TextRenderInfoTest {
        
        public const int FIRST_PAGE = 1;
        public const int FIRST_ELEMENT_INDEX = 0;
        private const string TEST_RESOURCES_PATH = @"..\..\resources\text\pdf\parser\TextRenderInfoTest\";

        [Test]
        public virtual void TestCharacterRenderInfos() {
            byte[] bytes = CreateSimplePdf(PageSize.LETTER.Rotate().Rotate(), "ABCD");
            //TestResourceUtils.saveBytesToFile(bytes, new File("C:/temp/out.pdf"));

            PdfReader r = new PdfReader(bytes);

            PdfReaderContentParser parser = new PdfReaderContentParser(r);
            parser.ProcessContent(FIRST_PAGE, new CharacterPositionRenderListener());
        }
        
        /**
         * Test introduced to exclude a bug related to a Unicode quirk for 
         * Japanese. TextRenderInfo threw an AIOOBE for some characters.
         * @throws java.lang.Exception
         * @since 5.5.5-SNAPSHOT
         */
        [Test]
        public void TestUnicodeEmptyString()  {
            StringBuilder sb = new StringBuilder();
            String inFile = "japanese_text.pdf";

    
            PdfReader p = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, inFile);
            ITextExtractionStrategy strat = new SimpleTextExtractionStrategy();

            sb.Append(PdfTextExtractor.GetTextFromPage(p, FIRST_PAGE, strat));

            String result = sb.ToString(0, sb.ToString().IndexOf('\n'));
            String origText =
                    "\u76f4\u8fd1\u306e\u0053\uff06\u0050\u0035\u0030\u0030"
                    + "\u914d\u5f53\u8cb4\u65cf\u6307\u6570\u306e\u30d1\u30d5"
                    + "\u30a9\u30fc\u30de\u30f3\u30b9\u306f\u0053\uff06\u0050"
                    + "\u0035\u0030\u0030\u6307\u6570\u3092\u4e0a\u56de\u308b";
            Assert.AreEqual(result, origText);
        }

        [Test]
        public void TestType3FontWidth() {
            String inFile = "type3font_text.pdf";
            LineSegment origLineSegment = new LineSegment(new Vector(20.3246f, 769.4974f, 1.0f), new Vector(151.22923f, 769.4974f, 1.0f));

            PdfReader reader = TestResourceUtils.GetResourceAsPdfReader(TEST_RESOURCES_PATH, inFile);
            TextPositionRenderListener renderListener = new TextPositionRenderListener();
            PdfContentStreamProcessor processor = new PdfContentStreamProcessor(renderListener);

            PdfDictionary pageDic = reader.GetPageN(FIRST_PAGE);
            PdfDictionary resourcesDic = pageDic.GetAsDict(PdfName.RESOURCES);
            processor.ProcessContent(ContentByteUtils.GetContentBytesForPage(reader, FIRST_PAGE), resourcesDic);


            Assert.AreEqual(renderListener.LineSegments[FIRST_ELEMENT_INDEX].GetStartPoint()[FIRST_ELEMENT_INDEX],
                origLineSegment.GetStartPoint()[FIRST_ELEMENT_INDEX], 1/2f);

            Assert.AreEqual(renderListener.LineSegments[FIRST_ELEMENT_INDEX].GetEndPoint()[FIRST_ELEMENT_INDEX],
                origLineSegment.GetEndPoint()[FIRST_ELEMENT_INDEX], 1/2f);

        }


        private class TextPositionRenderListener : IRenderListener {

            private List<LineSegment> lineSegments = new List<LineSegment>();

            public List<LineSegment> LineSegments {
                get { return lineSegments; }
            }

            public void RenderText(TextRenderInfo renderInfo) {
                lineSegments.Add(renderInfo.GetBaseline());
            }

            public void BeginTextBlock() {
            }

            public void EndTextBlock() {

            }

            public void RenderImage(ImageRenderInfo renderInfo) {

            }


        }

        private class CharacterPositionRenderListener : ITextExtractionStrategy {
            public virtual void BeginTextBlock() {
            }

            public virtual void RenderText(TextRenderInfo renderInfo) {
                IList<TextRenderInfo> subs = renderInfo.GetCharacterRenderInfos();
                TextRenderInfo previousCharInfo = subs[0];

                for (int i = 1; i < subs.Count; i++) {
                    TextRenderInfo charInfo = subs[i];
                    Vector previousEndPoint = previousCharInfo.GetBaseline().GetEndPoint();
                    Vector currentStartPoint = charInfo.GetBaseline().GetStartPoint();
                    AssertVectorsEqual(previousEndPoint, currentStartPoint, charInfo.GetText());
                    previousCharInfo = charInfo;
                }
            }

            private void AssertVectorsEqual(Vector v1, Vector v2, String message) {
                Assert.AreEqual(v1[0], v2[0], 1/72f, message);
                Assert.AreEqual(v1[1], v2[1], 1/72f, message);
            }

            public virtual void EndTextBlock() {
            }

            public virtual void RenderImage(ImageRenderInfo renderInfo) {
            }

            public virtual String GetResultantText() {
                return null;
            }
        }

        private byte[] CreateSimplePdf(Rectangle pageSize, params string[] text) {
            MemoryStream byteStream = new MemoryStream();

            Document document = new Document(pageSize);
            PdfWriter.GetInstance(document, byteStream);
            document.Open();
            foreach (string str in text) {
                document.Add(new Paragraph(str));
                document.NewPage();
            }

            document.Close();

            byte[] pdfBytes = byteStream.ToArray();

            return pdfBytes;
        }
    }
}
