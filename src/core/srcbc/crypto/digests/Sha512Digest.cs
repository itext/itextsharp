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

using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Digests
{
    /**
     * Draft FIPS 180-2 implementation of SHA-512. <b>Note:</b> As this is
     * based on a draft this implementation is subject to change.
     *
     * <pre>
     *         block  word  digest
     * SHA-1   512    32    160
     * SHA-256 512    32    256
     * SHA-384 1024   64    384
     * SHA-512 1024   64    512
     * </pre>
     */
    public class Sha512Digest
		: LongDigest
    {
        private const int DigestLength = 64;

		public Sha512Digest()
        {
        }

		/**
         * Copy constructor.  This will copy the state of the provided
         * message digest.
         */
        public Sha512Digest(
			Sha512Digest t)
			: base(t)
		{
		}

		public override string AlgorithmName
		{
			get { return "SHA-512"; }
		}

		public override int GetDigestSize()
		{
			return DigestLength;
		}

		public override int DoFinal(
            byte[]  output,
            int     outOff)
        {
            Finish();

            Pack.UInt64_To_BE(H1, output, outOff);
            Pack.UInt64_To_BE(H2, output, outOff + 8);
            Pack.UInt64_To_BE(H3, output, outOff + 16);
            Pack.UInt64_To_BE(H4, output, outOff + 24);
            Pack.UInt64_To_BE(H5, output, outOff + 32);
            Pack.UInt64_To_BE(H6, output, outOff + 40);
            Pack.UInt64_To_BE(H7, output, outOff + 48);
            Pack.UInt64_To_BE(H8, output, outOff + 56);

            Reset();

            return DigestLength;

        }

        /**
        * reset the chaining variables
        */
        public override void Reset()
        {
            base.Reset();

            /* SHA-512 initial hash value
             * The first 64 bits of the fractional parts of the square roots
             * of the first eight prime numbers
             */
            H1 = 0x6a09e667f3bcc908;
            H2 = 0xbb67ae8584caa73b;
            H3 = 0x3c6ef372fe94f82b;
            H4 = 0xa54ff53a5f1d36f1;
            H5 = 0x510e527fade682d1;
            H6 = 0x9b05688c2b3e6c1f;
            H7 = 0x1f83d9abfb41bd6b;
            H8 = 0x5be0cd19137e2179;
        }
    }
}
