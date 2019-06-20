/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
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
using System;
using System.IO;
using iTextSharp.javaone.edition14.part2;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace iTextSharp.javaone.edition14
{
    /// <summary>
    /// Extracts snippets of text from different Hello World examples
    /// </summary>
    public class S05_ExtractSnippets
    {
        public static readonly string RESULT_HIGH = "results/javaone/edition2014/05_hello-highlevel.txt";
        public static readonly string RESULT_LOW = "results/javaone/edition2014/05_hello-lowlevel.txt";
        public static readonly string RESULT_CHUNKS = "results/javaone/edition2014/05_hello-chunks.txt";
        public static readonly string RESULT_ABSOLUTE = "results/javaone/edition2014/05_hello-absolute.txt";

        public static void Main(String[] args)
        {
            ContentStreams._main(args);
            S05_ExtractSnippets app = new S05_ExtractSnippets();
            app.extractSnippets(ContentStreams.RESULT_HIGH, RESULT_HIGH);
            app.extractSnippets(ContentStreams.RESULT_CHUNKS, RESULT_CHUNKS);
            app.extractSnippets(ContentStreams.RESULT_ABSOLUTE, RESULT_ABSOLUTE);
        }

        public void extractSnippets(String src, String dest)
        {
            TextWriter output = new StreamWriter(new FileStream(dest, FileMode.Create));
            PdfReader reader = new PdfReader(src);
            IRenderListener listener = new MyTextRenderListener(output);
            PdfContentStreamProcessor processor =
                new PdfContentStreamProcessor(listener);
            PdfDictionary pageDic = reader.GetPageN(1);
            PdfDictionary resourcesDic = pageDic.GetAsDict(PdfName.RESOURCES);
            processor.ProcessContent(ContentByteUtils.GetContentBytesForPage(reader, 1), resourcesDic);
            output.Flush();
            output.Close();
            reader.Close();
        }
    }
}
