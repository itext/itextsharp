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
using System.Collections.Generic;
using System.IO;
using iTextSharp.javaone.edition14.part4.helper;
using iTextSharp.text.pdf;

namespace iTextSharp.javaone.edition14
{
    /// <summary>
    ///  In this second example that parses a PDF to discover its structure,
    /// we'll highlight all the structures that are encountered.
    /// </summary>
    public class S10_FindStructure : S09_FindLines
    {
        /// <summary>
        /// The resulting PDF after parsing for structure.
        /// </summary>
        public static readonly string DEST = "results/javaone/edition2014/10_structure.pdf";

        /// <summary>
        /// Reads the first page of a document of which the top margin is 48pt heigh
        /// and highlights structures.
        /// </summary>
        /// <param name="args">No arguments needed</param>
        public static void Main(string[] args)
        {
            DirectoryInfo dir = new FileInfo(DEST).Directory;
            if (dir != null)
                dir.Create();

            S10_FindStructure app = new S10_FindStructure();
            PdfReader reader = new PdfReader(SRC);
            List<MyItem> items = app.GetContentItems(reader, 1, 48);
            items.Sort();
            List<MyItem> lines = app.GetLines(items);
            List<MyItem> structures = app.GetStructures(lines);
            app.Highlight(structures, reader, 1, DEST);
        }

        /// <summary>
        /// Combines lines into structures
        /// </summary>
        /// <param name="lines">lines a list of lines</param>
        /// <returns>list of structures</returns>
        public List<MyItem> GetStructures(List<MyItem> lines)
        {
            List<MyItem> structures = new List<MyItem>();
            List<MyItem> structure = new List<MyItem>();
            foreach (Line line in lines)
            {
                if (structure.Count == 0)
                {
                    structure.Add(line);
                    continue;
                }
                if (AreInSameStructure((Line) structure[structure.Count - 1], line))
                {
                    structure.Add(line);
                }
                else
                {
                    structures.Add(new Structure(structure));
                    structure = new List<MyItem>();
                    structure.Add(line);
                }
            }
            if (structure.Count != 0)
                structures.Add(new Structure(structure));
            return structures;
        }

        static bool AreInSameStructure(Line i1, Line i2)
        {
            if (!i1.Color.Equals(i2.Color))
                return false;
            else if (i2.Rectangle.Left - i1.Rectangle.Left >= MyItem.ItemPositionTolerance)
                return false;
            return true;
        }
    }
}
