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
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace iTextSharp.javaone.edition14.part2
{
    public class ContentStreams
    {
        public static readonly string RESULT_HIGH = "results/javaone/edition2014/part2/hello-highlevel.pdf";
        public static readonly string RESULT_LOW = "results/javaone/edition2014/part2/hello-lowlevel.pdf";
        public static readonly string RESULT_UNCOMPRESSED = "results/javaone/edition2014/part2/hello-uncompressed.pdf";
        public static readonly string RESULT_CHUNKS = "results/javaone/edition2014/part2/hello-chunks.pdf";
        public static readonly string RESULT_ABSOLUTE = "results/javaone/edition2014/part2/hello-absolute.pdf";
        public static readonly string RESULT_REFLOW = "results/javaone/edition2014/part2/hello-reflow.pdf";
        public static readonly string RESULT_REFLOW_LOW = "results/javaone/edition2014/part2/hello-reflow-low.pdf";

        public static void _main(string[] args)
        {
            DirectoryInfo dir = new FileInfo(RESULT_HIGH).Directory;
            if (dir != null)
                dir.Create();

            ContentStreams cs = new ContentStreams();
            cs.CreatePdfHigh();
            cs.CreatePdfLow();
            cs.CreatePdfUncompressed();
            cs.CreatePdfChunks();
            cs.CreatePdfAbsolute();
            cs.CreatePdfReflow();
            cs.CreatePdfReflowLow();
        }

        public void CreatePdfHigh()
        {
            // step 1
            Document document = new Document(PageSize.LETTER);
            // step 2
            PdfWriter.GetInstance(document, new FileStream(RESULT_HIGH, FileMode.Create));
            // step 3
            document.Open();
            // step 4
            document.Add(new Paragraph("Hello World!"));
            // step 5
            document.Close();
        }

        public void CreatePdfLow()
        {
            // step 1
            Document document = new Document();
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(RESULT_LOW, FileMode.Create));
            writer.CompressionLevel = PdfStream.NO_COMPRESSION;
            // step 3
            document.Open();
            // step 4
            PdfContentByte canvas = writer.DirectContentUnder;
            canvas.SaveState(); // q
            canvas.BeginText(); // BT
            canvas.MoveText(36, 788); // 36 788 Td
            canvas.SetFontAndSize(BaseFont.CreateFont(), 12); // /F1 12 Tf
            canvas.ShowText("Hello World!"); // (Hello World!)Tj
            canvas.EndText(); // ET
            canvas.RestoreState(); // Q
            // step 5
            document.Close();
        }

        public void CreatePdfUncompressed()
        {
            // step 1
            Document document = new Document();
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(RESULT_UNCOMPRESSED, FileMode.Create));
            writer.CompressionLevel = PdfStream.NO_COMPRESSION;
            // step 3
            document.Open();
            // step 4
            document.Add(new Paragraph("Hello World!"));
            // step 5
            document.Close();
        }

        public void CreatePdfChunks()
        {
            // step 1
            Document document = new Document();
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(RESULT_CHUNKS, FileMode.Create));
            writer.CompressionLevel = PdfStream.NO_COMPRESSION;
            // step 3
            document.Open();
            // step 4
            Paragraph p = new Paragraph();
            p.Leading = 18;
            document.Add(p);
            document.Add(new Chunk("H"));
            document.Add(new Chunk("e"));
            document.Add(new Chunk("l"));
            document.Add(new Chunk("l"));
            document.Add(new Chunk("o"));
            document.Add(new Chunk(" "));
            document.Add(new Chunk("W"));
            document.Add(new Chunk("o"));
            document.Add(new Chunk("r"));
            document.Add(new Chunk("l"));
            document.Add(new Chunk("d"));
            document.Add(new Chunk("!"));
            // step 5
            document.Close();
        }

        public void CreatePdfAbsolute()
        {
            // step 1
            Document document = new Document();
            Document.Compress = false;
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(RESULT_ABSOLUTE, FileMode.Create));
            //writer.setCompressionLevel(PdfStream.NO_COMPRESSION);
            // step 3
            document.Open();
            // step 4
            PdfContentByte canvas = writer.DirectContentUnder;
            writer.CompressionLevel = 0;
            canvas.SaveState(); // q
            canvas.BeginText(); // BT
            canvas.MoveText(36, 788); // 36 788 Td
            canvas.SetFontAndSize(BaseFont.CreateFont(), 12); // /F1 12 Tf
            canvas.ShowText("Hel"); // (Hel)Tj
            canvas.MoveText(30.65f, 0);
            canvas.ShowText("World!"); // (World!)Tj
            canvas.MoveText(-12.7f, 0);
            canvas.ShowText("lo"); // (lo)Tj
            canvas.EndText(); // ET
            canvas.RestoreState(); // Q
            // step 5
            document.Close();
            Document.Compress = true;
        }

        public void CreatePdfReflow()
        {
            // step 1
            Document document = new Document();
            Document.Compress = false;
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(RESULT_REFLOW, FileMode.Create));
            // step 3
            document.Open();
            // step 4
            document.Add(
                new Paragraph(
                    "0 Hello World 1 Hello World 2 Hello World 3 Hello World 4 Hello World 5 Hello World 6 Hello World 7 Hello World 8 Hello World 9 Hello World A Hello World B Hello World"));
            // step 5
            document.Close();
            Document.Compress = true;
        }

        public void CreatePdfReflowLow()
        {
            // step 1
            Document document = new Document();
            // step 2
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(RESULT_REFLOW_LOW, FileMode.Create));
            writer.CompressionLevel = PdfStream.NO_COMPRESSION;
            // step 3
            document.Open();
            // step 4
            PdfContentByte canvas = writer.DirectContentUnder;
            canvas.SaveState(); // q
            canvas.BeginText(); // BT
            canvas.MoveText(36, 788); // 36 788 Td
            canvas.SetFontAndSize(BaseFont.CreateFont(), 12); // /F1 12 Tf
            canvas.ShowText(
                "0 Hello World 1 Hello World 2 Hello World 3 Hello World 4 Hello World 5 Hello World 6 Hello World 7 Hello World 8 Hello World 9 Hello World A Hello World B Hello World");
            canvas.EndText(); // ET
            canvas.RestoreState(); // Q
            // step 5
            document.Close();
        }
    }
}
