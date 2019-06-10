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
    public class RC4Engine
		: IStreamCipher
    {
        private readonly static int STATE_LENGTH = 256;

        /*
        * variables to hold the state of the RC4 engine
        * during encryption and decryption
        */

        private byte[]	engineState;
        private int		x;
        private int		y;
        private byte[]	workingKey;

        /**
        * initialise a RC4 cipher.
        *
        * @param forEncryption whether or not we are for encryption.
        * @param parameters the parameters required to set up the cipher.
        * @exception ArgumentException if the parameters argument is
        * inappropriate.
        */
        public void Init(
            bool				forEncryption,
            ICipherParameters	parameters)
        {
            if (parameters is KeyParameter)
            {
                /*
                * RC4 encryption and decryption is completely
                * symmetrical, so the 'forEncryption' is
                * irrelevant.
                */
                workingKey = ((KeyParameter)parameters).GetKey();
                SetKey(workingKey);

                return;
            }

            throw new ArgumentException("invalid parameter passed to RC4 init - " + parameters.GetType().ToString());
        }

		public string AlgorithmName
        {
            get { return "RC4"; }
        }

		public byte ReturnByte(
			byte input)
        {
            x = (x + 1) & 0xff;
            y = (engineState[x] + y) & 0xff;

            // swap
            byte tmp = engineState[x];
            engineState[x] = engineState[y];
            engineState[y] = tmp;

            // xor
            return (byte)(input ^ engineState[(engineState[x] + engineState[y]) & 0xff]);
        }

        public void ProcessBytes(
            byte[]	input,
            int		inOff,
            int		length,
            byte[]	output,
            int		outOff
        )
        {
            if ((inOff + length) > input.Length)
            {
                throw new DataLengthException("input buffer too short");
            }

            if ((outOff + length) > output.Length)
            {
                throw new DataLengthException("output buffer too short");
            }

            for (int i = 0; i < length ; i++)
            {
                x = (x + 1) & 0xff;
                y = (engineState[x] + y) & 0xff;

                // swap
                byte tmp = engineState[x];
                engineState[x] = engineState[y];
                engineState[y] = tmp;

                // xor
                output[i+outOff] = (byte)(input[i + inOff]
                        ^ engineState[(engineState[x] + engineState[y]) & 0xff]);
            }
        }

        public void Reset()
        {
            SetKey(workingKey);
        }

        // Private implementation

        private void SetKey(
			byte[] keyBytes)
        {
            workingKey = keyBytes;

            // System.out.println("the key length is ; "+ workingKey.Length);

            x = 0;
            y = 0;

            if (engineState == null)
            {
                engineState = new byte[STATE_LENGTH];
            }

            // reset the state of the engine
            for (int i=0; i < STATE_LENGTH; i++)
            {
                engineState[i] = (byte)i;
            }

            int i1 = 0;
            int i2 = 0;

            for (int i=0; i < STATE_LENGTH; i++)
            {
                i2 = ((keyBytes[i1] & 0xff) + engineState[i] + i2) & 0xff;
                // do the byte-swap inline
                byte tmp = engineState[i];
                engineState[i] = engineState[i2];
                engineState[i2] = tmp;
                i1 = (i1+1) % keyBytes.Length;
            }
        }
    }

}
