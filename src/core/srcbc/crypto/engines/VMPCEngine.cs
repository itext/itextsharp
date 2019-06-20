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

using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Engines
{
    public class VmpcEngine
        : IStreamCipher
    {
        /*
        * variables to hold the state of the VMPC engine during encryption and
        * decryption
        */
        protected byte n = 0;
        protected byte[] P = null;
        protected byte s = 0;

        protected byte[] workingIV;
        protected byte[] workingKey;

        public virtual string AlgorithmName
        {
            get { return "VMPC"; }
        }

        /**
        * initialise a VMPC cipher.
        * 
        * @param forEncryption
        *    whether or not we are for encryption.
        * @param params
        *    the parameters required to set up the cipher.
        * @exception ArgumentException
        *    if the params argument is inappropriate.
        */
        public virtual void Init(
            bool				forEncryption,
            ICipherParameters	parameters)
        {
            if (!(parameters is ParametersWithIV))
                throw new ArgumentException("VMPC Init parameters must include an IV");

            ParametersWithIV ivParams = (ParametersWithIV) parameters;

            if (!(ivParams.Parameters is KeyParameter))
                throw new ArgumentException("VMPC Init parameters must include a key");

            KeyParameter key = (KeyParameter)ivParams.Parameters;

            this.workingIV = ivParams.GetIV();

            if (workingIV == null || workingIV.Length < 1 || workingIV.Length > 768)
                throw new ArgumentException("VMPC requires 1 to 768 bytes of IV");

            this.workingKey = key.GetKey();

            InitKey(this.workingKey, this.workingIV);
        }

        protected virtual void InitKey(
            byte[]	keyBytes,
            byte[]	ivBytes)
        {
            s = 0;
            P = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                P[i] = (byte) i;
            }

            for (int m = 0; m < 768; m++)
            {
                s = P[(s + P[m & 0xff] + keyBytes[m % keyBytes.Length]) & 0xff];
                byte temp = P[m & 0xff];
                P[m & 0xff] = P[s & 0xff];
                P[s & 0xff] = temp;
            }
            for (int m = 0; m < 768; m++)
            {
                s = P[(s + P[m & 0xff] + ivBytes[m % ivBytes.Length]) & 0xff];
                byte temp = P[m & 0xff];
                P[m & 0xff] = P[s & 0xff];
                P[s & 0xff] = temp;
            }
            n = 0;
        }

        public virtual void ProcessBytes(
            byte[]	input,
            int		inOff,
            int		len,
            byte[]	output,
            int		outOff)
        {
            if ((inOff + len) > input.Length)
            {
                throw new DataLengthException("input buffer too short");
            }

            if ((outOff + len) > output.Length)
            {
                throw new DataLengthException("output buffer too short");
            }

            for (int i = 0; i < len; i++)
            {
                s = P[(s + P[n & 0xff]) & 0xff];
                byte z = P[(P[(P[s & 0xff]) & 0xff] + 1) & 0xff];
                // encryption
                byte temp = P[n & 0xff];
                P[n & 0xff] = P[s & 0xff];
                P[s & 0xff] = temp;
                n = (byte) ((n + 1) & 0xff);

                // xor
                output[i + outOff] = (byte) (input[i + inOff] ^ z);
            }
        }

        public virtual void Reset()
        {
            InitKey(this.workingKey, this.workingIV);
        }

        public virtual byte ReturnByte(
            byte input)
        {
            s = P[(s + P[n & 0xff]) & 0xff];
            byte z = P[(P[(P[s & 0xff]) & 0xff] + 1) & 0xff];
            // encryption
            byte temp = P[n & 0xff];
            P[n & 0xff] = P[s & 0xff];
            P[s & 0xff] = temp;
            n = (byte) ((n + 1) & 0xff);

            // xor
            return (byte) (input ^ z);
        }
    }
}
