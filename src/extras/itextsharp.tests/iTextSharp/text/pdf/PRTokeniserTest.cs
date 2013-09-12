using System;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class PRTokeniserTest
    {
        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
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

        [Test]
        public void TestOneNumber()
        {
            CheckTokenTypes(
                    "/Name1 70",
                    PRTokeniser.TokType.NAME,
                    PRTokeniser.TokType.NUMBER,
                    PRTokeniser.TokType.ENDOFFILE
            );
        }

        [Test]
        public void TestTwoNumbers()
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
        public void Test()
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
