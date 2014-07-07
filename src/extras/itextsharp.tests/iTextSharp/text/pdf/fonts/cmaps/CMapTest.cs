using iTextSharp.text.pdf;
using iTextSharp.text.pdf.fonts.cmaps;
using NUnit.Framework;

namespace itextsharp.tests.iTextSharp.text.pdf.fonts.cmaps
{
    class CMapTest
    {

        private void CheckInsertAndRetrieval(byte[] bytes, string uni)
        {
            CMapToUnicode c = new CMapToUnicode();
            c.AddChar(new PdfString(bytes), new PdfString(uni, "UTF-16BE"));
            string lookupResult = c.Lookup(bytes, 0, bytes.Length);
            Assert.AreEqual(uni, lookupResult);
        }

        [Test, Ignore]
        virtual public void TestHighOrderBytes()
        {
            CheckInsertAndRetrieval(new byte[] { (byte)0x91 }, "\u2018");
            CheckInsertAndRetrieval(new byte[] { (byte)0x91, (byte)0x92 }, "\u2018");

            CheckInsertAndRetrieval(new byte[] { (byte)0x20 }, "\u2018");
            CheckInsertAndRetrieval(new byte[] { (byte)0x23, (byte)0x21 }, "\u2018");
            CheckInsertAndRetrieval(new byte[] { (byte)0x22, (byte)0xf0 }, "\u2018");
            CheckInsertAndRetrieval(new byte[] { (byte)0xf1, (byte)0x25 }, "\u2018");
        }
    }
}
