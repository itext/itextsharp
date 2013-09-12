using System;
using System.Collections.Generic;
using System.Text;
using com.itextpdf.text.pdf.languages;
using iTextSharp.text.pdf.languages;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf
{
    class IndicLigatures
    {
        [Test]
        public void TestDevanagari()
        {
            IndicLigaturizer d = new DevanagariLigaturizer();
            String processed = d.Process("\u0936\u093e\u0902\u0924\u094d\u093f");
            Assert.AreEqual("\u0936\u093e\u0902\u093f\u0924", processed);
        }

        [Test]
        public void TestDevanagari2()
        {
            IndicLigaturizer d = new DevanagariLigaturizer();
            String processed = d.Process("\u0936\u093e\u0902\u0924\u093f");
            Assert.AreEqual("\u0936\u093e\u0902\u093f\u0924", processed);
        }

        [Test]
        public void TestGujarati()
        {
            IndicLigaturizer g = new GujaratiLigaturizer();
            String processed = g.Process("\u0ab6\u0abe\u0a82\u0aa4\u0acd\u0abf");
            Assert.AreEqual("\u0ab6\u0abe\u0a82\u0abf\u0aa4", processed);
        }

        [Test]
        public void TestGujarati2()
        {
            IndicLigaturizer g = new GujaratiLigaturizer();
            String processed = g.Process("\u0ab6\u0abe\u0a82\u0aa4\u0abf");
            Assert.AreEqual("\u0ab6\u0abe\u0a82\u0abf\u0aa4", processed);
        }
    }
}
