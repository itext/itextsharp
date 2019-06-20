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
using System.Collections;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Macs
{
    /**
    * HMAC implementation based on RFC2104
    *
    * H(K XOR opad, H(K XOR ipad, text))
    */
    public class HMac
		: IMac
    {
        private const byte IPAD = (byte)0x36;
        private const byte OPAD = (byte)0x5C;

        private readonly IDigest digest;
        private readonly int digestSize;
        private readonly int blockLength;

		private readonly byte[] inputPad;
        private readonly byte[] outputBuf;

        public HMac(IDigest digest)
        {
            this.digest = digest;
            this.digestSize = digest.GetDigestSize();
            this.blockLength = digest.GetByteLength();
            this.inputPad = new byte[blockLength];
            this.outputBuf = new byte[blockLength + digestSize];
        }

        public virtual string AlgorithmName
        {
            get { return digest.AlgorithmName + "/HMAC"; }
        }

		public virtual IDigest GetUnderlyingDigest()
        {
            return digest;
        }

        public virtual void Init(ICipherParameters parameters)
        {
            digest.Reset();

            byte[] key = ((KeyParameter)parameters).GetKey();
			int keyLength = key.Length;

            if (keyLength > blockLength)
            {
                digest.BlockUpdate(key, 0, keyLength);
                digest.DoFinal(inputPad, 0);

				keyLength = digestSize;
            }
            else
            {
				Array.Copy(key, 0, inputPad, 0, keyLength);
            }

			Array.Clear(inputPad, keyLength, blockLength - keyLength);
            Array.Copy(inputPad, 0, outputBuf, 0, blockLength);

			XorPad(inputPad, blockLength, IPAD);
            XorPad(outputBuf, blockLength, OPAD);

            // Initialise the digest
			digest.BlockUpdate(inputPad, 0, inputPad.Length);
        }

        public virtual int GetMacSize()
        {
            return digestSize;
        }

        public virtual void Update(byte input)
        {
            digest.Update(input);
        }

        public virtual void BlockUpdate(byte[] input, int inOff, int len)
        {
            digest.BlockUpdate(input, inOff, len);
        }

        public virtual int DoFinal(byte[] output, int outOff)
        {
            digest.DoFinal(outputBuf, blockLength);
            digest.BlockUpdate(outputBuf, 0, outputBuf.Length);
            int len = digest.DoFinal(output, outOff);

            Array.Clear(outputBuf, blockLength, digestSize);

			// Initialise the digest
            digest.BlockUpdate(inputPad, 0, inputPad.Length);

            return len;
        }

        /**
        * Reset the mac generator.
        */
        public virtual void Reset()
        {
			// Reset underlying digest
            digest.Reset();

			// Initialise the digest
            digest.BlockUpdate(inputPad, 0, inputPad.Length);
        }

        private static void XorPad(byte[] pad, int len, byte n)
		{
			for (int i = 0; i < len; ++i)
            {
                pad[i] ^= n;
            }
		}
    }
}
