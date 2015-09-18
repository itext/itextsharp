using System;
using iTextSharp.text.pdf;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.fonts {
    
    /**
    * @author Daniel Lichtenberger, CHEMDOX
    */
    public class FontFamilyTest {

        private static string srcFolder = @"..\..\resources\text\pdf\fonts\NotoFont\";

        [Test]
        public void TestNotoFontFamily() {
            string[] fonts = {"NotoSansCJKjp-Bold.otf", "NotoSansCJKjp-Regular.otf"};

            string fontFamily = "Noto Sans CJK JP";

            foreach (string file in fonts) {
                BaseFont font = BaseFont.CreateFont(srcFolder + file, BaseFont.CP1252, false);
                String[][] familyFontName = font.FamilyFontName;
                foreach (string[] values in familyFontName) {
                    Assert.AreEqual(fontFamily, values[3]);
                }
            }
        }
    }
}