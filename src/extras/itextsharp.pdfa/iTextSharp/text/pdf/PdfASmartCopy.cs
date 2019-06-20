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
using System.Text;
using iTextSharp.text;
using iTextSharp.text.error_messages;
using iTextSharp.text.log;
using iTextSharp.text.pdf;

namespace itextsharp.pdfa.iTextSharp.text.pdf {

    /**
     * PdfASmartCopy has the same functionality as PdfACopy,
     * but when resources (such as fonts, images,...) are
     * encountered, a reference to these resources is saved
     * in a cache, so that they can be reused.
     * This requires more memory, but reduces the file size
     * of the resulting PDF document.
     */

    public class PdfASmartCopy : PdfACopy {

        private static readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof(PdfSmartCopy));

        /** the cache with the streams and references. */
        private Dictionary<PdfSmartCopy.ByteStore, PdfIndirectReference> streamMap = null;
        private Dictionary<RefKey, int> serialized = new Dictionary<RefKey, int>();

        /**
         * Constructor
         *
         * @param document         document
         * @param os               outputstream
         * @param conformanceLevel
         */
        public PdfASmartCopy(Document document, Stream os, PdfAConformanceLevel conformanceLevel) : base(document, os, conformanceLevel) {
            this.streamMap = new Dictionary<PdfSmartCopy.ByteStore, PdfIndirectReference>();
        }

        protected override PdfIndirectReference CopyIndirect(PRIndirectReference @in) {
            PdfObject srcObj = PdfReader.GetPdfObjectRelease(@in);
            PdfSmartCopy.ByteStore streamKey = null;
            bool validStream = false;
            if (srcObj.IsStream()) {
                streamKey = new PdfSmartCopy.ByteStore((PRStream) srcObj, serialized);
                validStream = true;
                PdfIndirectReference streamRef;
                if (streamMap.TryGetValue(streamKey, out streamRef)) {
                    return streamRef;
                }
            } else if (srcObj.IsDictionary()) {
                streamKey = new PdfSmartCopy.ByteStore((PdfDictionary) srcObj, serialized);
                validStream = true;
                PdfIndirectReference streamRef;
                if (streamMap.TryGetValue(streamKey, out streamRef)) {
                    return streamRef;
                }
            }

            PdfIndirectReference theRef;
            RefKey key = new RefKey(@in);
            IndirectReferences iRef;
            if (indirects.TryGetValue(key, out iRef)) {
                theRef = iRef.Ref;
                if (iRef.Copied) {
                    return theRef;
                }
            } else {
                theRef = body.PdfIndirectReference;
                iRef = new IndirectReferences(theRef);
                indirects[key] = iRef;
            }
            if (srcObj.IsDictionary()) {
                PdfObject type = PdfReader.GetPdfObjectRelease(((PdfDictionary)srcObj).Get(PdfName.TYPE));
                if (type != null) {
                    if ((PdfName.PAGE.Equals(type))) {
                        return theRef;
                    }
                    if ((PdfName.CATALOG.Equals(type))) {
                        LOGGER.Warn(MessageLocalization.GetComposedMessage("make.copy.of.catalog.dictionary.is.forbidden"));
                        return null;
                    }
                }
            }

            iRef.SetCopied();

            if (validStream) {
                streamMap[streamKey] = theRef;
            }

            PdfObject obj = CopyObject(srcObj);
            AddToBody(obj, theRef);
            return theRef;
        }

        public override void FreeReader(PdfReader reader) {
            serialized.Clear();
            base.FreeReader(reader);
        }

        public override void AddPage(PdfImportedPage iPage) {
            if (currentPdfReaderInstance.Reader != reader)
                serialized.Clear();
            base.AddPage(iPage);
        }
    }
}
