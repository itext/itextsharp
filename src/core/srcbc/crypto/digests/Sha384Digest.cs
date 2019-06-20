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
     * Draft FIPS 180-2 implementation of SHA-384. <b>Note:</b> As this is
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
    public class Sha384Digest
		: LongDigest
    {
        private const int DigestLength = 48;

		public Sha384Digest()
        {
        }

        /**
         * Copy constructor.  This will copy the state of the provided
         * message digest.
         */
        public Sha384Digest(
			Sha384Digest t)
			: base(t)
		{
		}

		public override string AlgorithmName
		{
			get { return "SHA-384"; }
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

            Reset();

            return DigestLength;
        }

        /**
        * reset the chaining variables
        */
        public override void Reset()
        {
            base.Reset();

            /* SHA-384 initial hash value
                * The first 64 bits of the fractional parts of the square roots
                * of the 9th through 16th prime numbers
                */
            H1 = 0xcbbb9d5dc1059ed8;
            H2 = 0x629a292a367cd507;
            H3 = 0x9159015a3070dd17;
            H4 = 0x152fecd8f70e5939;
            H5 = 0x67332667ffc00b31;
            H6 = 0x8eb44a8768581511;
            H7 = 0xdb0c2e0d64f98fa7;
            H8 = 0x47b5481dbefa4fa4;
        }
    }
}
