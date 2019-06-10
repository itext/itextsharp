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
using System.Collections.Generic;
using System.IO;
using iTextSharp.javaone.edition14.part4.helper;
using iTextSharp.text.pdf;

namespace iTextSharp.javaone.edition14
{
    /// <summary>
    /// In this second example that parses a PDF to discover its structure,
    /// we'll highlight all the text lines and images that are encountered.
    /// </summary>
    public class S09_FindLines : S08_FindItems
    {
        /// <summary>
        /// The resulting PDF after parsing for structure.
        /// </summary>
        public static readonly String DEST = "results/javaone/edition2014/09_page229_lines.pdf";

        /// <summary>
        /// Reads the first page of a document of which the top margin is 48pt heigh
        /// and highlights text lines and images.
        /// </summary>
        /// <param name="args">No arguments needed</param>
        public static void Main(string[] args)
        {
            DirectoryInfo dir = new FileInfo(DEST).Directory;
            if (dir != null)
                dir.Create();

            S09_FindLines app = new S09_FindLines();
            PdfReader reader = new PdfReader(SRC);
            List<MyItem> items = app.GetContentItems(reader, 1, 48);
            items.Sort();
            List<MyItem> lines = app.GetLines(items);
            app.Highlight(lines, reader, 1, DEST);
        }

        /// <summary>
        /// Converts a list of items into a list of lines. This method assumes
        /// that all the items are sorted in the logical reading order.
        /// </summary>
        /// <param name="items">items a list of text and image items</param>
        /// <returns>a list of line items</returns>
        public List<MyItem> GetLines(List<MyItem> items)
        {
            List<MyItem> lines = new List<MyItem>();
            List<MyItem> line = new List<MyItem>();
            foreach (MyItem item in items)
            {
                if (line.Count == 0)
                {
                    line.Add(item);
                    continue;
                }
                if (AreOnSameLine(line[line.Count - 1], item))
                {
                    line.Add(item);
                }
                else
                {
                    lines.Add(new Line(line));
                    line = new List<MyItem>();
                    line.Add(item);
                }
            }
            if (line.Count != 0)
                lines.Add(new Line(line));
            return lines;
        }


        /// <summary>
        /// * Checks if 2 items are on the same line.
        /// </summary>
        /// <param name="i1">first item</param>
        /// <param name="i2">second item</param>
        /// <returns>true if items are on the same line, otherwise false</returns>
        static bool AreOnSameLine(MyItem i1, MyItem i2)
        {
            return Math.Abs(i1.GetLL().y - i2.GetLL().y) <= MyItem.ItemPositionTolerance;
        }
    }
}
