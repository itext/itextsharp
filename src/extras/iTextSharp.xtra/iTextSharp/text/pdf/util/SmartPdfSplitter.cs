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
using iTextSharp.text.log;

namespace iTextSharp.text.pdf.util {
    /**
     * Splits a PDF based on a given file size.
     */
    public class SmartPdfSplitter {

        private readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof (SmartPdfSplitter));

        protected PdfReader reader;
        protected int numberOfPages = 0;
        protected int currentPage = 1;
        protected bool overSized = false;

        public SmartPdfSplitter(PdfReader reader) {
            this.reader = reader;
            reader.Appendable = true;
            numberOfPages = reader.NumberOfPages;
            if (LOGGER.IsLogging(Level.INFO)) {
                LOGGER.Info(String.Format("Creating a splitter for a document with {0} pages", numberOfPages));
            }
        }

        public bool HasMorePages() {
            return currentPage <= numberOfPages;
        }

        public bool IsOverSized() {
            return overSized;
        }

        public bool Split(Stream os, long sizeInBytes) {
            if (!HasMorePages()) {
                os.Close();
                return false;
            }
            overSized = false;
            Document document = new Document();
            PdfCopy copy = new PdfSmartCopy(document, os);
            document.Open();
            bool hasPage = false;
            PdfResourceCounter counter = new PdfResourceCounter(reader.Trailer);
            long trailer = counter.GetLength(null);
            IDictionary<int, PdfObject> resources = counter.Resources;
            long length = 0;
            long page;
            while (HasMorePages()) {
                counter = new PdfResourceCounter(reader.GetPageN(currentPage));
                page = counter.GetLength(resources);
                resources = counter.Resources;
                length += page + trailer + XrefLength(resources.Count);
                if (LOGGER.IsLogging(Level.INFO)) {
                    LOGGER.Info(String.Format("Page {0}: Comparing {1} with {2}", currentPage, length, sizeInBytes));
                    LOGGER.Info(String.Format("   page {0} trailer {1} xref {2}", page, trailer, XrefLength(resources.Count)));
                }
                if (!hasPage || length < sizeInBytes) {
                    hasPage = true;
                    copy.AddPage(copy.GetImportedPage(reader, currentPage));
                    length = copy.Os.Counter;
                    if (LOGGER.IsLogging(Level.INFO)) {
                        LOGGER.Info(String.Format("Size after adding page: {0}", length));
                    }
                    if (length > sizeInBytes) overSized = true;
                    currentPage++;
                } else {
                    LOGGER.Info("Page doesn't fit");
                    break;
                }
            }
            document.Close();
            return true;
        }

        private long XrefLength(int size) {
            return 20l*(size + 1);
        }
    }

}
