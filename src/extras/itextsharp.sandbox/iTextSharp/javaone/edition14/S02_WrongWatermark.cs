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
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace iTextSharp.javaone.edition14
{
    /// <summary>
    /// Example that shows how to NOT watermark a less simple PDF.
    /// </summary>
    public class S02_WrongWatermark
    {
        /// <summary>
        /// The original PDF file. 
        /// </summary>
        public static readonly string SRC = "../../resources/pdfs/superman.pdf";

        /// <summary>
        /// The resulting PDF file.
        /// </summary>
        public static readonly string DEST = "results/javaone/edition2014/02_wrong_watermark.pdf";

        /// <summary>
        /// Fills out an interactive form.
        /// </summary>
        /// <param name="args">no arguments needed</param>
        public static void Main(string[] args)
        {
            DirectoryInfo dir = new FileInfo(DEST).Directory;
            if (dir != null)
                dir.Create();

            new S02_WrongWatermark().ManipulatePdf(SRC, DEST);
        }

        /// <summary>
        /// Fills out and flattens a form with the name, company and country.
        /// </summary>
        /// <param name="src"> the path to the original form </param>
        /// <param name="dest"> the path to the filled out form </param>
        public void ManipulatePdf(String src, String dest)
        {
            PdfReader reader = new PdfReader(src);
            PdfStamper stamper = new PdfStamper(reader, new FileStream(dest, FileMode.Create));
            int n = reader.NumberOfPages;
            for (int i = 1; i <= n; i++)
            {
                PdfContentByte under = stamper.GetUnderContent(i);
                PdfGState gs = new PdfGState();
                gs.FillOpacity = 0.3f;
                under.SaveState();
                under.SetGState(gs);
                under.SetRGBColorFill(200, 200, 0);
                ColumnText.ShowTextAligned(under, Element.ALIGN_CENTER,
                        new Phrase("Watermark", new Font(Font.FontFamily.HELVETICA, 120)),
                        297, 421, 45);
                under.RestoreState();
            }
            stamper.Close();
            reader.Close();
        }
    }
}
