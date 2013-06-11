using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using iTextSharp.text.pdf;

namespace iTextSharp.text.pdfa
{
    [TestFixture]
    public class PdfAFileStructureTest
    {
        /*
            The % character of the file header shall occur at byte offset 0 of the file.
            The file header line shall be immediately followed by a comment consisting of a % character followed by at least four characters, each of whose encoded byte values shall have a decimal value greater than 127.
        */
        [Test]
        public void FileHeader()
        {
            MemoryStream baos = new MemoryStream();
            Document document = new Document();
            PdfWriter.GetInstance(document, baos);
            document.Open();
            document.Add(new Chunk("Hello World"));
            document.Close();

            byte[] bytes = baos.ToArray();
            Assert.AreEqual(bytes[0], '%');
            Assert.IsTrue((sbyte)bytes[10] < 0);
            Assert.IsTrue((sbyte)bytes[11] < 0);
            Assert.IsTrue((sbyte)bytes[12] < 0);
            Assert.IsTrue((sbyte)bytes[13] < 0);
        }

        /*
            The file trailer dictionary shall contain the ID keyword.
            The keyword Encrypt shall not be used in the trailer dictionary.
            No data shall follow the last end-of-file marker except a single optional end-of-line marker.
         */
        [Test]
        public void FileTrailer()
        {
            MemoryStream baos = new MemoryStream();
            Document document = new Document();
            PdfWriter.GetInstance(document, baos);
            document.Open();
            document.Add(new Chunk("Hello World"));
            document.Close();

            byte[] bytes = baos.ToArray();
            String str = Encoding.UTF8.GetString(bytes, bytes.Length - 6, 6);
            Assert.AreEqual("%%EOF\n", str);
            PdfReader reader = new PdfReader(baos.ToArray());
            Assert.IsNotNull(reader.Trailer.Get(PdfName.ID));
            reader.Close();
        }

        /*
            Hexadecimal strings shall contain an even number of non-white-space characters, each in the range 0 to 9, A to F or a to f.
         */
        [Test]
        public void stringObjects()
        {
            byte[] bytes = new byte[256];
            for (int i = 0; i < 256; i++)
                bytes[i] = (byte) i;
            PdfString str = new PdfString(bytes);
            MemoryStream baos = new MemoryStream();
            str.SetHexWriting(true);
            str.ToPdf(null, baos);
            String s = Encoding.UTF8.GetString(baos.ToArray());
            Assert.AreEqual(514, s.Length);
        }

        /*
            The stream keyword shall be followed either by a CARRIAGE RETURN (0Dh) and LINE FEED (0Ah) character sequence or by a single LINE FEED character.
            The endstream keyword shall be preceded by an EOL marker.
            The value of the Length key specified in the stream dictionary shall match the number of bytes in the file following the LINE FEED character after the stream keyword and preceding the EOL marker before the endstream keyword.
            A stream object dictionary shall not contain the F, FFilter, or FDecodeParams keys.
        */
        [Test]
        public void streamObjects()
        {
            MemoryStream baos = new MemoryStream();
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, baos);
            document.Open();
            document.Add(new Chunk("Hello World"));
            PdfIndirectReference refa =
                writer.AddToBody(new PdfStream(Encoding.UTF8.GetBytes("Hello World"))).IndirectReference;
            writer.Info.Put(new PdfName("HelloWorld"), refa);
            document.Close();

            byte[] bytes = baos.ToArray();
            String str = Encoding.UTF8.GetString(bytes, 22, 44);
            Assert.AreEqual("\n<</Length 11>>stream\nHello World\nendstream\n", str);
        }

        /*
            The object number and generation number shall be separated by a single white-space character.
            The generation number and obj keyword shall be separated by a single white-space character.
            The object number and endobj keyword shall each be preceded by an EOL marker. The obj and endobj keywords shall each be followed by an EOL marker.
         */
        [Test]
        public void indirectObjects()
        {
            MemoryStream baos = new MemoryStream();
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, baos);
            document.Open();
            document.Add(new Chunk("Hello World"));
            PdfIndirectReference refa = writer.AddToBody(new PdfString("Hello World")).IndirectReference;
            writer.Info.Put(new PdfName("HelloWorld"), refa);
            document.Close();

            byte[] bytes = baos.ToArray();
            String str = Encoding.UTF8.GetString(bytes, 14, 30);
            Assert.AreEqual("\n1 0 obj\n(Hello World)\nendobj\n", str);
        }
    }
}