using System;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.parser.state;
using NUnit.Framework;

namespace itextsharp.xmlworker.tests.iTextSharp.tool.xml.parser {
    internal class SpecialCharactersTest {
        private String regHtml;
        private int reg;
        private SpecialCharState scState;
        private XMLParser parser;
        private String regStr;
        private InsideTagHTMLState itState;
        private int hex;
        private String e;
        private char accent;

        [SetUp]
        public void SetUp() {
            parser = new XMLParser();
            scState = new SpecialCharState(parser);
            itState = new InsideTagHTMLState(parser);
            reg = 174;
            regHtml = "&reg";
            regStr = "\u00ae";
            hex = 0x00ae;
            e =
                "Travailleur ou ch\u00f4meur, ouvrier, employ\u00e9 ou cadre, homme ou femme, jeune ou moins jeune,... au Syndicat lib\u00e9ral vous n'\u00eates pas un num\u00e9ro et vous pouvez compter sur l'aide de l'ensemble de nos collaborateurs.";
        }

        [Test]
        public void TestIntCode() {
            itState.Process((char) hex);
            String str = parser.Memory().Current().ToString();
            Console.WriteLine(str);
            Assert.AreEqual(hex, str[0]);
        }

        [Test]
        public void TestHtmlChar() {
            scState.Process('r');
            scState.Process('e');
            scState.Process('g');
            scState.Process(';');
            String str = parser.Memory().Current().ToString();
            Assert.AreEqual(hex, str[0]);
        }

        [Test]
        public void TestEacuteCcedilleEtc() {
            for (int i = 0; i < e.Length; i++) {
                itState.Process((char) e[i]);
            }
            String str = parser.Memory().Current().ToString();
            Assert.AreEqual(e, str);
        }
    }
}