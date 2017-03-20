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
using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.javaone.edition14.part4.helper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace iTextSharp.javaone.edition14
{
    /// <summary>
    /// In this first example that parses a PDF to discover its structure,
    /// we'll highlight all the different text items and images that are
    /// encountered during the parsing process.
    /// </summary>
    public class S07_FindItemsSimple
    {
        /// <summary>
        /// The source file that is going to be parsed.
        /// </summary>
        public static readonly string SRC = "../../resources/pdfs/page229.pdf";

        /// <summary>
        /// The resulting PDF after parsing for structure.
        /// </summary>
        public static readonly string DEST = "results/javaone/edition2014/07_page229_itemssimple.pdf";

        /// <summary>
        /// Reads the first page of a document and highlights text items and images.
        /// </summary>
        /// <param name="args">No arguments needed</param>
        public static void Main(String[] args)
        {
            DirectoryInfo dir = new FileInfo(DEST).Directory;
            if (dir != null)
                dir.Create();

            S07_FindItemsSimple app = new S07_FindItemsSimple();
            PdfReader reader = new PdfReader(SRC);
            List<MyItem> items = app.GetContentItems(reader, 1, 48);
            app.Highlight(items, reader, 1, DEST);
        }

        /// <summary>
        /// Parses a page of a PDF file resulting in a list of
        /// </summary>
        /// <param name="reader">a PdfReader</param>
        /// <param name="page">the page number of the page that needs to be parsed</param>
        /// <param name="header_height">the height of the top margin</param>
        /// <returns>a list of TextItem and ImageItem objects</returns>
        public List<MyItem> GetContentItems(PdfReader reader, int page, float header_height)
        {
            PdfReaderContentParser parser = new PdfReaderContentParser(reader);
            MyRenderListenerSimple myRenderListener = new MyRenderListenerSimple();
            parser.ProcessContent(page, myRenderListener);
            return myRenderListener.Items;
        }

        /// <summary>
        /// Accepts a list of MyItem objects and draws a colored rectangle for each
        /// </summary>
        /// <param name="items">The list of items</param>
        /// <param name="reader">The reader instance that has access to the PDF file</param>
        /// <param name="pageNum">The page number of the page that needs to be parsed</param>
        /// <param name="destination">The path for the altered PDF file</param>
        public void Highlight(List<MyItem> items, PdfReader reader, int pageNum, String destination)
        {
            PdfStamper stamper = new PdfStamper(reader, new FileStream(destination, FileMode.Create));
            PdfContentByte over = stamper.GetOverContent(pageNum);
            foreach (MyItem item in items)
            {
                if (item.Color == null)
                    continue;
                over.SaveState();
                over.SetColorStroke(item.Color);
                over.SetLineWidth(2);
                Rectangle r = item.Rectangle;
                over.Rectangle(r.Left, r.Bottom, r.Width, r.Height);
                over.Stroke();
                over.RestoreState();
            }
            stamper.Close();
        }


    }
}
