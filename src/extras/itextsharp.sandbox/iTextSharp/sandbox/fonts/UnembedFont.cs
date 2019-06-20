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
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace iTextSharp.sandbox.fonts
{

    [WrapToTest]
    public class UnembedFont
    {

        public static readonly String SRC = "results/fonts/withSerifFont.pdf";
        public static readonly String DEST = "results/fonts/withoutSerifFont.pdf";

        public static void Main(String[] args)
        {
            UnembedFont app = new UnembedFont();
            app.ManipulatePdf(SRC, DEST);
        }

        /**
         * Creates a PDF with an embedded font.
         */
        public void CreatePdf(String file)
        {
            // step 1
            Document document = new Document();
            // step 2
            Stream os = new FileStream(file, FileMode.Create);
            PdfWriter.GetInstance(document, os);
            // step 3
            document.Open();
            // step 4
            BaseFont bf = BaseFont.CreateFont("../../resources/fonts/PT_Serif-Web-Regular.ttf", BaseFont.WINANSI,
                BaseFont.EMBEDDED);
            Font f = new Font(bf, 12);
            document.Add(new Paragraph("This is a test with Times New Roman.", f));
            // step 5
            document.Close();
        }

        /**
         * Removes the embedded font
         */
        public void ManipulatePdf(String src, String dest)
        {
            CreatePdf(src);
            // we create a reader instance
            PdfReader reader = new PdfReader(src);
            // we loop over all objects
            PdfObject obj;
            for (int i = 1; i < reader.XrefSize; i++)
            {
                obj = reader.GetPdfObject(i);
                // we skip all objects that aren't a dictionary
                if (obj == null || !obj.IsDictionary())
                    continue;
                // we process all dictionaries
                UnembedTTF((PdfDictionary) obj);
            }
            // removing unused objects will remove unused font file streams
            reader.RemoveUnusedObjects();
            // we persist the altered document
            Stream os = new FileStream(dest, FileMode.Create);
            PdfStamper stamper = new PdfStamper(reader, os);
            stamper.Close();
        }

        /**
        * Processes a dictionary.
        * In case of font dictionaries, the dictionary is processed.
        */
        public void UnembedTTF(PdfDictionary dict)
        {
            // we ignore all dictionaries that aren't font dictionaries
            if (!dict.IsFont())
                return;
            // we only remove TTF fonts
            if (dict.GetAsDict(PdfName.FONTFILE2) != null)
            {
                return;
            }
            // check if a subset was used (in which case we remove the prefix)
            PdfName baseFont = dict.GetAsName(PdfName.BASEFONT);
            if (baseFont.GetBytes()[7] == '+')
            {
                baseFont = new PdfName(baseFont.ToString().Substring(8));
                dict.Put(PdfName.BASEFONT, baseFont);
            }
            // we check if there's a font descriptor
            PdfDictionary fontDescriptor = dict.GetAsDict(PdfName.FONTDESCRIPTOR);
            if (fontDescriptor == null)
                return;
            // is there is, we replace the fontname and remove the font file
            fontDescriptor.Put(PdfName.FONTNAME, baseFont);
            fontDescriptor.Remove(PdfName.FONTFILE2);
        }
    }
}
