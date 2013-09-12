using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser
{
    class VectorTest
    {
        [Test]
        public void TestCrossVector()
        {
            Vector v = new Vector(2, 3, 4);
            Matrix m = new Matrix(5, 6, 7, 8, 9, 10);
            Vector shouldBe = new Vector(67, 76, 4);

            Vector rslt = v.Cross(m);
            Assert.AreEqual(shouldBe, rslt);
        }

    }
}
