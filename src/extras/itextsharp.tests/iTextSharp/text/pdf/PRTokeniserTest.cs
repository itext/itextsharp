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
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class PRTokeniserTest
    {
        [SetUp]
        virtual public void SetUp()
        {
        }

        [TearDown]
        virtual public void TearDown()
        {
        }

        static private byte[] GetBytes(string str)
        {
            return System.Text.Encoding.UTF8.GetBytes(str);
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private void CheckTokenTypes(String data, params PRTokeniser.TokType[] expectedTypes)
        {
            PRTokeniser tok = new PRTokeniser(new RandomAccessFileOrArray(GetBytes(data)));

            for (int i = 0; i < expectedTypes.Length; i++)
            {
                tok.NextValidToken();
                //System.out.println(tok.getTokenType() + " -> " + tok.getStringValue());
                Assert.AreEqual(expectedTypes[i], tok.TokenType, "Position " + i);
            }
        }

        private void CheckNumberValue(String data, String expectedValue) {
            PRTokeniser tok = new PRTokeniser(new RandomAccessFileOrArray(GetBytes(data)));

            tok.NextValidToken();
            Assert.AreEqual(PRTokeniser.TokType.NUMBER, tok.TokenType, "Wrong type");
            Assert.AreEqual(expectedValue, tok.StringValue, "Wrong multiple minus signs number handling");
        }

        [Test]
        virtual public void TestOneNumber()
        {
            CheckTokenTypes(
                    "/Name1 70",
                    PRTokeniser.TokType.NAME,
                    PRTokeniser.TokType.NUMBER,
                    PRTokeniser.TokType.ENDOFFILE
            );
        }

        [Test]
        virtual public void TestTwoNumbers()
        {
            CheckTokenTypes(
                    "/Name1 70/Name 2",
                    PRTokeniser.TokType.NAME,
                    PRTokeniser.TokType.NUMBER,
                    PRTokeniser.TokType.NAME,
                    PRTokeniser.TokType.NUMBER,
                    PRTokeniser.TokType.ENDOFFILE
            );
        }


        [Test]
        public void TestMultipleMinusSignsRealNumber() {
            CheckNumberValue("----40.25", "-40.25");
        }

        [Test]
        public void TestMultipleMinusSignsIntegerNumber() {
            CheckNumberValue("--9", "0");
        }


        [Test]
        virtual public void Test()
        {
            CheckTokenTypes(
                    "<</Size 70/Root 46 0 R/Info 44 0 R/ID[<8C2547D58D4BD2C6F3D32B830BE3259D><8F69587888569A458EB681A4285D5879>]/Prev 116 >>",
                    PRTokeniser.TokType.START_DIC,
                    PRTokeniser.TokType.NAME,
                    PRTokeniser.TokType.NUMBER,
                    PRTokeniser.TokType.NAME,
                    PRTokeniser.TokType.REF,
                    PRTokeniser.TokType.NAME,
                    PRTokeniser.TokType.REF,
                    PRTokeniser.TokType.NAME,
                    PRTokeniser.TokType.START_ARRAY,
                    PRTokeniser.TokType.STRING,
                    PRTokeniser.TokType.STRING,
                    PRTokeniser.TokType.END_ARRAY,
                    PRTokeniser.TokType.NAME,
                    PRTokeniser.TokType.NUMBER,
                    PRTokeniser.TokType.END_DIC,
                    PRTokeniser.TokType.ENDOFFILE

            );

        }
    }
}
