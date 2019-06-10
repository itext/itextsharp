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
using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace iTextSharp.sandbox.images
{
    [WrapToTest]
    public class RepeatImage
    {
        public static readonly string SRC = "../../resources/pdfs/chinese.pdf";
        public static readonly string DEST = "results/images/chinese.pdf";

        public static void Main(string[] args)
        {
            DirectoryInfo dir = new FileInfo(DEST).Directory;
            if (dir != null)
                dir.Create();
            new RepeatImage().ManipulatePdf(SRC, DEST);
        }

        public void ManipulatePdf(string src, string dest)
        {
            PdfReader reader = new PdfReader(src);
            // We assume that there's a single large picture on the first page
            PdfDictionary page = reader.GetPageN(1);
            PdfDictionary resources = page.GetAsDict(PdfName.RESOURCES);
            PdfDictionary xobjects = resources.GetAsDict(PdfName.XOBJECT);
            Dictionary<PdfName, PdfObject>.KeyCollection.Enumerator enumerator = xobjects.Keys.GetEnumerator();
            enumerator.MoveNext();
            PdfName imgName = enumerator.Current;
            Image img = Image.GetInstance((PRIndirectReference) xobjects.GetAsIndirectObject(imgName));
            img.SetAbsolutePosition(0, 0);
            img.ScaleAbsolute(reader.GetPageSize(1));
            PdfStamper stamper = new PdfStamper(reader, new FileStream(dest,FileMode.Create));
            stamper.GetOverContent(1).AddImage(img);
            stamper.Close();
            reader.Close();
        }
    }
}
