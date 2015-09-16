using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text.error_messages;

using iTextSharp.text;

/*
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */

namespace iTextSharp.text.pdf {
    /**
    * Instance of PdfReader in each output document.
    *
    * @author Paulo Soares
    */
    public class PdfReaderInstance {
        internal static PdfLiteral IDENTITYMATRIX = new PdfLiteral("[1 0 0 1 0 0]");
        internal static PdfNumber ONE = new PdfNumber(1);
        internal int[] myXref;
        internal PdfReader reader;
        internal RandomAccessFileOrArray file;
        internal Dictionary<int, PdfImportedPage> importedPages = new Dictionary<int,PdfImportedPage>();
        internal PdfWriter writer;
        internal Dictionary<int,object> visited = new Dictionary<int,object>();
        internal List<int> nextRound = new List<int>();
        
        internal PdfReaderInstance(PdfReader reader, PdfWriter writer) {
            this.reader = reader;
            this.writer = writer;
            file = reader.SafeFile;
            myXref = new int[reader.XrefSize];
        }
        
        internal PdfReader Reader {
            get {
                return reader;
            }
        }
        
        internal PdfImportedPage GetImportedPage(int pageNumber) {
            if (!reader.IsOpenedWithFullPermissions)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("pdfreader.not.opened.with.owner.password"));
            if (pageNumber < 1 || pageNumber > reader.NumberOfPages)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("invalid.page.number.1", pageNumber));
            PdfImportedPage pageT;
            if (!importedPages.TryGetValue(pageNumber, out pageT)) {
                pageT = new PdfImportedPage(this, writer, pageNumber);
                importedPages[pageNumber] = pageT;
            }
            return pageT;
        }
        
        internal int GetNewObjectNumber(int number, int generation) {
            if (myXref[number] == 0) {
                myXref[number] = writer.IndirectReferenceNumber;
                nextRound.Add(number);
            }
            return myXref[number];
        }
        
        internal RandomAccessFileOrArray ReaderFile {
            get {
                return file;
            }
        }
        
        internal PdfObject GetResources(int pageNumber) {
            PdfObject obj = PdfReader.GetPdfObjectRelease(reader.GetPageNRelease(pageNumber).Get(PdfName.RESOURCES));
            return obj;
        }
        
        /**
        * Gets the content stream of a page as a PdfStream object.
        * @param   pageNumber          the page of which you want the stream
        * @param   compressionLevel    the compression level you want to apply to the stream
        * @return  a PdfStream object
        * @since   2.1.3 (the method already existed without param compressionLevel)
        */
        internal PdfStream GetFormXObject(int pageNumber, int compressionLevel) {
            PdfDictionary page = reader.GetPageNRelease(pageNumber);
            PdfObject contents = PdfReader.GetPdfObjectRelease(page.Get(PdfName.CONTENTS));
            PdfDictionary dic = new PdfDictionary();
            byte[] bout = null;
            if (contents != null) {
                if (contents.IsStream())
                    dic.Merge((PRStream)contents);
                else
                    bout = reader.GetPageContent(pageNumber, file);
            }
            else
                bout = new byte[0];
            dic.Put(PdfName.RESOURCES, PdfReader.GetPdfObjectRelease(page.Get(PdfName.RESOURCES)));
            dic.Put(PdfName.TYPE, PdfName.XOBJECT);
            dic.Put(PdfName.SUBTYPE, PdfName.FORM);
            PdfImportedPage impPage = importedPages[pageNumber];
            dic.Put(PdfName.BBOX, new PdfRectangle(impPage.BoundingBox));
            PdfArray matrix = impPage.Matrix;
            if (matrix == null)
                dic.Put(PdfName.MATRIX, IDENTITYMATRIX);
            else
                dic.Put(PdfName.MATRIX, matrix);
            dic.Put(PdfName.FORMTYPE, ONE);
            PRStream stream;
            if (bout == null) {
                stream = new PRStream((PRStream)contents, dic);
            }
            else {
                stream = new PRStream(reader, bout);
                stream.Merge(dic);
            }
            return stream;
        }
        
        internal void WriteAllVisited() {
            while (nextRound.Count > 0) {
                List<int> vec = nextRound;
                nextRound = new List<int>();
                foreach (int i in vec) {
                    if (!visited.ContainsKey(i)) {
                        visited[i] = null;
                        writer.AddToBody(reader.GetPdfObjectRelease(i), myXref[i]);
                    }
                }
            }
        }
        
        virtual public void WriteAllPages() {
            try {
                file.ReOpen();
                foreach (PdfImportedPage ip in importedPages.Values) {
                    if (ip.IsToCopy()) {
                        writer.AddToBody(ip.GetFormXObject(writer.CompressionLevel), ip.IndirectReference);
                        ip.SetCopied();
                    }
                }
                WriteAllVisited();
            }
            finally {
                try {
// TODO: Removed - the user should be responsible for closing all PdfReaders.  But, this could cause a lot of memory leaks in code out there that hasn't been properly closing things - maybe add a finalizer to PdfReader that calls PdfReader#close() ??            	
//                reader.Close();
                    file.Close();
                }
                catch  {
                    //Empty on purpose
                }
            }
        }
    }
}
