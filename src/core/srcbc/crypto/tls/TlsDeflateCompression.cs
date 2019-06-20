/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General Public License version 3 as published by the Free Software Foundation with the addition of the following permission added to Section 15 as permitted in Section 7(a): FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY iText Group NV, iText Group NV DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License along with this program; if not, see http://www.gnu.org/licenses or write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA, 02110-1301 USA, or download the license from the following URL:

http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions of this program must display Appropriate Legal Notices, as required under Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License, a covered work must retain the producer line in every PDF that is created or manipulated using iText.

You can be released from the requirements of the license by purchasing a commercial license. Buying such a license is mandatory as soon as you develop commercial activities involving the iText software without disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP, serving PDFs on the fly in a web application, shipping iText with a closed source product.

For more information, please contact iText Software Corp. at this address: sales@itextpdf.com */
using System;
using System.IO;

using Org.BouncyCastle.Utilities.Zlib;

namespace Org.BouncyCastle.Crypto.Tls
{
    public class TlsDeflateCompression : TlsCompression
    {
        public const int LEVEL_NONE = JZlib.Z_NO_COMPRESSION;
        public const int LEVEL_FASTEST = JZlib.Z_BEST_SPEED;
        public const int LEVEL_SMALLEST = JZlib.Z_BEST_COMPRESSION;
        public const int LEVEL_DEFAULT = JZlib.Z_DEFAULT_COMPRESSION;

        protected readonly ZStream zIn, zOut;

        public TlsDeflateCompression()
            : this(LEVEL_DEFAULT)
        {
        }

        public TlsDeflateCompression(int level)
        {
            this.zIn = new ZStream();
            this.zIn.inflateInit();

            this.zOut = new ZStream();
            this.zOut.deflateInit(level);
        }

        public virtual Stream Compress(Stream output)
        {
            return new DeflateOutputStream(output, zOut, true);
        }

        public virtual Stream Decompress(Stream output)
        {
            return new DeflateOutputStream(output, zIn, false);
        }

        protected class DeflateOutputStream : ZOutputStream
        {
            public DeflateOutputStream(Stream output, ZStream z, bool compress)
                : base(output, z)
            {
                this.compress = compress;

                /*
                 * See discussion at http://www.bolet.org/~pornin/deflate-flush.html .
                 */
                this.FlushMode = JZlib.Z_SYNC_FLUSH;
            }

            public override void Flush()
            {
                /*
                 * TODO The inflateSyncPoint doesn't appear to work the way I hoped at the moment.
                 * In any case, we may like to accept PARTIAL_FLUSH input, not just SYNC_FLUSH.
                 * It's not clear how to check this in the Inflater.
                 */
                //if (!this.compress && (z == null || z.istate == null || z.istate.inflateSyncPoint(z) <= 0))
                //{
                //    throw new TlsFatalAlert(AlertDescription.decompression_failure);
                //}
            }
        }
    }
}
