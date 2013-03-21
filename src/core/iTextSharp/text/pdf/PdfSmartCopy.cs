using System;
using System.IO;
using System.Collections.Generic;
using System.util.collections;
using iTextSharp.text.pdf.security;
/*
 * $Id$
 * 
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2012 1T3XT BVBA
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
        private Dictionary<ByteStore, PdfIndirectReference> streamMap = null;
        private readonly HashSet<PdfObject> serialized = new HashSet<PdfObject>();

        /** Creates a PdfSmartCopy instance. */
        public PdfSmartCopy(Document document, Stream os) : base(document, os) {
            this.streamMap = new Dictionary<ByteStore,PdfIndirectReference>();
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
                streamKey = new ByteStore((PRStream)srcObj, serialized);
                validStream = true;
                PdfIndirectReference streamRef;
                if (streamMap.TryGetValue(streamKey, out streamRef)) {
                    return streamRef;
                }
            } else if (srcObj.IsDictionary()) {
                streamKey = new ByteStore((PdfDictionary)srcObj, serialized);
                validStream = true;
                PdfIndirectReference streamRef = null;
                if (streamMap.TryGetValue(streamKey, out streamRef)) {
                    return streamRef;
                }
            }

            PdfIndirectReference theRef;
            RefKey key = new RefKey(inp);
            IndirectReferences iRef;
            indirects.TryGetValue(key, out iRef);
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
            private readonly byte[] b;
            private readonly int hash;
            private void SerObject(PdfObject obj, int level, ByteBuffer bb, HashSet<PdfObject> serialized)
            {
                if (level <= 0)
                    return;
                if (obj == null) {
                    bb.Append("$Lnull");
                    return;
                }

                if (obj.IsIndirect()) {
                    if (serialized.Contains(obj))
                        return;
                    else
                        serialized.Add(obj);
                }
                obj = PdfReader.GetPdfObject(obj);
                if (obj.IsStream()) {
                    bb.Append("$B");
                    SerDic((PdfDictionary)obj, level - 1, bb, serialized);
                    if (level > 0) {
                        bb.Append(DigestAlgorithms.Digest("MD5", PdfReader.GetStreamBytesRaw((PRStream)obj)));
                    }
                }
                else if (obj.IsDictionary()) {
                    SerDic((PdfDictionary)obj, level - 1, bb,serialized);
                }
                else if (obj.IsArray()) {
                    SerArray((PdfArray)obj, level - 1, bb,serialized);
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

            private void SerDic(PdfDictionary dic, int level, ByteBuffer bb, HashSet<PdfObject> serialized)
            {
                bb.Append("$D");
                if (level <= 0)
                    return;
                PdfName[] keys = new PdfName[dic.Size];
                dic.Keys.CopyTo(keys, 0);
                Array.Sort<PdfName>(keys);
                for (int k = 0; k < keys.Length; ++k) {
                    SerObject(keys[k], level, bb, serialized);
                    SerObject(dic.Get(keys[k]), level, bb, serialized);
                }
            }

            private void SerArray(PdfArray array, int level, ByteBuffer bb, HashSet<PdfObject> serialized)
            {
                bb.Append("$A");
                if (level <= 0)
                    return;
                for (int k = 0; k < array.Size; ++k) {
                    SerObject(array[k], level, bb, serialized);
                }
            }

            internal ByteStore(PRStream str, HashSet<PdfObject> serialized)
            {
                ByteBuffer bb = new ByteBuffer();
                int level = 100;
                SerObject(str, level, bb, serialized);
                this.b = bb.ToByteArray();
                hash = CalculateHash(this.b);
            }

            internal ByteStore(PdfDictionary dict, HashSet<PdfObject> serialized)
            {
                ByteBuffer bb = new ByteBuffer();
                int level = 100;
                SerObject(dict, level, bb, serialized);
                this.b = bb.ToByteArray();
                hash = CalculateHash(this.b);
            }

            private static int CalculateHash(Byte[] b)
            {
                int hash = 0;
                int len = b.Length;
                for(int k = 0; k < len; ++k)
                    hash = hash * 31 + b[k];
                return hash;
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
                return hash;
            }
        }
    }
}
