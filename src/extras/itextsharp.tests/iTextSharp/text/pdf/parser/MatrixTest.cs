using iTextSharp.text.pdf.parser;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.parser
{
    class MatrixTest
    {
        [Test]
        virtual public void TestMultiply()
        {
            Matrix m1 = new Matrix(2, 3, 4, 5, 6, 7);
            Matrix m2 = new Matrix(8, 9, 10, 11, 12, 13);
            Matrix shouldBe = new Matrix(46, 51, 82, 91, 130, 144);

            Matrix rslt = m1.Multiply(m2);
            Assert.AreEqual(shouldBe, rslt);
        }

        [Test]
        virtual public void TestDeterminant()
        {
            Matrix m = new Matrix(2, 3, 4, 5, 6, 7);
            Assert.AreEqual(-2f, m.GetDeterminant(), .001f);
        }
    }
}
