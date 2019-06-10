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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Generators
{
    /**
    * Generator for Pbe derived keys and ivs as defined by Pkcs 5 V2.0 Scheme 2.
    * This generator uses a SHA-1 HMac as the calculation function.
    * <p>
    * The document this implementation is based on can be found at
    * <a href="http://www.rsasecurity.com/rsalabs/pkcs/pkcs-5/index.html">
    * RSA's Pkcs5 Page</a></p>
    */
    public class Pkcs5S2ParametersGenerator
        : PbeParametersGenerator
    {
        private readonly IMac hMac;
        private readonly byte[] state;

        /**
        * construct a Pkcs5 Scheme 2 Parameters generator.
        */
        public Pkcs5S2ParametersGenerator()
            : this(new Sha1Digest())
        {
        }

        public Pkcs5S2ParametersGenerator(IDigest digest)
        {
            this.hMac = new HMac(digest);
            this.state = new byte[hMac.GetMacSize()];
        }

        private void F(
            byte[]  S,
            int     c,
            byte[]  iBuf,
            byte[]  outBytes,
            int     outOff)
        {
            if (c == 0)
                throw new ArgumentException("iteration count must be at least 1.");

            if (S != null)
            {
                hMac.BlockUpdate(S, 0, S.Length);
            }

            hMac.BlockUpdate(iBuf, 0, iBuf.Length);
            hMac.DoFinal(state, 0);

            Array.Copy(state, 0, outBytes, outOff, state.Length);

            for (int count = 1; count < c; ++count)
            {
                hMac.BlockUpdate(state, 0, state.Length);
                hMac.DoFinal(state, 0);

                for (int j = 0; j < state.Length; ++j)
                {
                    outBytes[outOff + j] ^= state[j];
                }
            }
        }

        private byte[] GenerateDerivedKey(
            int dkLen)
        {
            int     hLen = hMac.GetMacSize();
            int     l = (dkLen + hLen - 1) / hLen;
            byte[]  iBuf = new byte[4];
            byte[]  outBytes = new byte[l * hLen];
            int     outPos = 0;

            ICipherParameters param = new KeyParameter(mPassword);

            hMac.Init(param);

            for (int i = 1; i <= l; i++)
            {
                // Increment the value in 'iBuf'
                int pos = 3;
                while (++iBuf[pos] == 0)
                {
                    --pos;
                }

                F(mSalt, mIterationCount, iBuf, outBytes, outPos);
                outPos += hLen;
            }

            return outBytes;
        }

        /**
        * Generate a key parameter derived from the password, salt, and iteration
        * count we are currently initialised with.
        *
        * @param keySize the size of the key we want (in bits)
        * @return a KeyParameter object.
        */
        [Obsolete("Use version with 'algorithm' parameter")]
        public override ICipherParameters GenerateDerivedParameters(
            int keySize)
        {
            return GenerateDerivedMacParameters(keySize);
        }

        public override ICipherParameters GenerateDerivedParameters(
            string	algorithm,
            int		keySize)
        {
            keySize /= 8;

            byte[] dKey = GenerateDerivedKey(keySize);

            return ParameterUtilities.CreateKeyParameter(algorithm, dKey, 0, keySize);
        }

        /**
        * Generate a key with initialisation vector parameter derived from
        * the password, salt, and iteration count we are currently initialised
        * with.
        *
        * @param keySize the size of the key we want (in bits)
        * @param ivSize the size of the iv we want (in bits)
        * @return a ParametersWithIV object.
        */
        [Obsolete("Use version with 'algorithm' parameter")]
        public override ICipherParameters GenerateDerivedParameters(
            int	keySize,
            int	ivSize)
        {
            keySize /= 8;
            ivSize /= 8;

            byte[] dKey = GenerateDerivedKey(keySize + ivSize);

            return new ParametersWithIV(new KeyParameter(dKey, 0, keySize), dKey, keySize, ivSize);
        }

        public override ICipherParameters GenerateDerivedParameters(
            string	algorithm,
            int		keySize,
            int		ivSize)
        {
            keySize /= 8;
            ivSize /= 8;

            byte[] dKey = GenerateDerivedKey(keySize + ivSize);
            KeyParameter key = ParameterUtilities.CreateKeyParameter(algorithm, dKey, 0, keySize);

            return new ParametersWithIV(key, dKey, keySize, ivSize);
        }

        /**
        * Generate a key parameter for use with a MAC derived from the password,
        * salt, and iteration count we are currently initialised with.
        *
        * @param keySize the size of the key we want (in bits)
        * @return a KeyParameter object.
        */
        public override ICipherParameters GenerateDerivedMacParameters(
            int keySize)
        {
            keySize /= 8;

            byte[] dKey = GenerateDerivedKey(keySize);

            return new KeyParameter(dKey, 0, keySize);
        }
    }
}
