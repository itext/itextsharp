using System;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.ocg;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.ocg {
    public class OcgRemovalTest {
        private static readonly String INPUT = @"..\..\resources\text\pdf\ocg\peek-a-boo2.pdf";

        [Test]
        public virtual void RemoveOcgLayer() {
            PdfReader reader = new PdfReader(INPUT);
            OCGRemover ocgRemover = new OCGRemover();
            ocgRemover.RemoveLayers(reader, "Do you see me?");
            PdfDictionary catalog = reader.Catalog;
            Assert.IsNull(catalog.Get(PdfName.OCPROPERTIES));
            Assert.AreNotSame(PdfName.USEOC, catalog.Get(PdfName.PAGEMODE));
            reader.Close();
        }
    }
}
