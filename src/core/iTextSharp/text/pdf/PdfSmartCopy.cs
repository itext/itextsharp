using System;
using System.IO;
using System.Collections;
using iTextSharp.text;
/*
 * $Id: PdfSmartCopy.cs,v 1.7 2008/05/13 11:25:23 psoares33 Exp $
 * 
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2009 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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
    * PdfSmartCopy has the same functionality as PdfCopy,
    * but when resources (such as fonts, images,...) are
    * encountered, a reference to these resources is saved
    * in a cache, so that they can be reused.
    * This requires more memory, but reduces the file size
    * of the resulting PDF document.
    */

    public class PdfSmartCopy : PdfCopy {

        /** the cache with the streams and references. */
        private Hashtable streamMap = null;

        /** Creates a PdfSmartCopy instance. */
        public PdfSmartCopy(Document document, Stream os) : base(document, os) {
            this.streamMap = new Hashtable();
        }
        /**
        * Translate a PRIndirectReference to a PdfIndirectReference
        * In addition, translates the object numbers, and copies the
        * referenced object to the output file if it wasn't available
        * in the cache yet. If it's in the cache, the reference to
        * the already used stream is returned.
        * 
        * NB: PRIndirectReferences (and PRIndirectObjects) really need to know what
        * file they came from, because each file has its own namespace. The translation
        * we do from their namespace to ours is *at best* heuristic, and guaranteed to
        * fail under some circumstances.
        */
        protected override PdfIndirectReference CopyIndirect(PRIndirectReference inp) {
            PdfObject srcObj = PdfReader.GetPdfObjectRelease(inp);
            ByteStore streamKey = null;
            bool validStream = false;
            if (srcObj.IsStream()) {
                streamKey = new ByteStore((PRStream)srcObj);
                validStream = true;
                PdfIndirectReference streamRef = (PdfIndirectReference) streamMap[streamKey];
                if (streamRef != null) {
                    return streamRef;
                }
            }

            PdfIndirectReference theRef;
            RefKey key = new RefKey(inp);
            IndirectReferences iRef = (IndirectReferences) indirects[key];
            if (iRef != null) {
                theRef = iRef.Ref;
                if (iRef.Copied) {
                    return theRef;
                }
            } else {
                theRef = body.PdfIndirectReference;
                iRef = new IndirectReferences(theRef);
                indirects[key] = iRef;
            }
            if (srcObj != null && srcObj.IsDictionary()) {
                PdfObject type = PdfReader.GetPdfObjectRelease(((PdfDictionary)srcObj).Get(PdfName.TYPE));
                if (type != null && PdfName.PAGE.Equals(type)) {
                    return theRef;
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

        internal class ByteStore {
            private byte[] b;
            private int hash;
            
            private void SerObject(PdfObject obj, int level, ByteBuffer bb) {
                if (level <= 0)
                    return;
                if (obj == null) {
                    bb.Append("$Lnull");
                    return;
                }
                obj = PdfReader.GetPdfObject(obj);
                if (obj.IsStream()) {
                    bb.Append("$B");
                    SerDic((PdfDictionary)obj, level - 1, bb);
                    if (level > 0) {
                        bb.Append(PdfEncryption.DigestComputeHash("MD5", PdfReader.GetStreamBytesRaw((PRStream)obj)));
                    }
                }
                else if (obj.IsDictionary()) {
                    SerDic((PdfDictionary)obj, level - 1, bb);
                }
                else if (obj.IsArray()) {
                    SerArray((PdfArray)obj, level - 1, bb);
                }
                else if (obj.IsString()) {
                    bb.Append("$S").Append(obj.ToString());
                }
                else if (obj.IsName()) {
                    bb.Append("$N").Append(obj.ToString());
                }
                else
                    bb.Append("$L").Append(obj.ToString());
            }
            
            private void SerDic(PdfDictionary dic, int level, ByteBuffer bb) {
                bb.Append("$D");
                if (level <= 0)
                    return;
                Object[] keys = new Object[dic.Size];
                dic.Keys.CopyTo(keys, 0);
                Array.Sort(keys);
                for (int k = 0; k < keys.Length; ++k) {
                    SerObject((PdfObject)keys[k], level, bb);
                    SerObject(dic.Get((PdfName)keys[k]), level, bb);
                }
            }
            
            private void SerArray(PdfArray array, int level, ByteBuffer bb) {
                bb.Append("$A");
                if (level <= 0)
                    return;
                for (int k = 0; k < array.Size; ++k) {
                    SerObject(array[k], level, bb);
                }
            }
            
            internal ByteStore(PRStream str) {
                ByteBuffer bb = new ByteBuffer();
                int level = 100;
                SerObject(str, level, bb);
                this.b = bb.ToByteArray();
            }

            public override bool Equals(Object obj) {
                if (obj == null || !(obj is ByteStore))
                    return false;
                if (GetHashCode() != obj.GetHashCode())
                    return false;
                byte[] b2 = ((ByteStore)obj).b;
                if (b2.Length != b.Length)
                    return false;
                int len = b.Length;
                for (int k = 0; k < len; ++k) {
                    if (b[k] != b2[k])
                        return false;
                }
                return true;
            }

            public override int GetHashCode() {
                if (hash == 0) {
                    int len = b.Length;
                    for (int k = 0; k < len; ++k) {
                        hash = hash * 31 + b[k];
                    }
                }
                return hash;
            }
        }
    }
}
