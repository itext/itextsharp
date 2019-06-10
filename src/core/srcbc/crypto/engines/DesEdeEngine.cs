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
    /// <remarks>A class that provides a basic DESede (or Triple DES) engine.</remarks>
    public class DesEdeEngine
        : DesEngine
    {
        private int[] workingKey1, workingKey2, workingKey3;
        private bool forEncryption;

        /**
        * initialise a DESede cipher.
        *
        * @param forEncryption whether or not we are for encryption.
        * @param parameters the parameters required to set up the cipher.
        * @exception ArgumentException if the parameters argument is
        * inappropriate.
        */
        public override void Init(
            bool				forEncryption,
            ICipherParameters	parameters)
        {
            if (!(parameters is KeyParameter))
                throw new ArgumentException("invalid parameter passed to DESede init - " + parameters.GetType().ToString());

            byte[] keyMaster = ((KeyParameter)parameters).GetKey();
            if (keyMaster.Length != 24 && keyMaster.Length != 16)
                throw new ArgumentException("key size must be 16 or 24 bytes.");

            this.forEncryption = forEncryption;

            byte[] key1 = new byte[8];
            Array.Copy(keyMaster, 0, key1, 0, key1.Length);
            workingKey1 = GenerateWorkingKey(forEncryption, key1);

            byte[] key2 = new byte[8];
            Array.Copy(keyMaster, 8, key2, 0, key2.Length);
            workingKey2 = GenerateWorkingKey(!forEncryption, key2);

            if (keyMaster.Length == 24)
            {
                byte[] key3 = new byte[8];
                Array.Copy(keyMaster, 16, key3, 0, key3.Length);
                workingKey3 = GenerateWorkingKey(forEncryption, key3);
            }
            else        // 16 byte key
            {
                workingKey3 = workingKey1;
            }
        }

        public override string AlgorithmName
        {
            get { return "DESede"; }
        }

        public override int GetBlockSize()
        {
            return BLOCK_SIZE;
        }

        public override int ProcessBlock(
            byte[]	input,
            int		inOff,
            byte[]	output,
            int		outOff)
        {
            if (workingKey1 == null)
                throw new InvalidOperationException("DESede engine not initialised");
            if ((inOff + BLOCK_SIZE) > input.Length)
                throw new DataLengthException("input buffer too short");
            if ((outOff + BLOCK_SIZE) > output.Length)
                throw new DataLengthException("output buffer too short");

            byte[] temp = new byte[BLOCK_SIZE];

            if (forEncryption)
            {
                DesFunc(workingKey1, input, inOff, temp, 0);
                DesFunc(workingKey2, temp, 0, temp, 0);
                DesFunc(workingKey3, temp, 0, output, outOff);
            }
            else
            {
                DesFunc(workingKey3, input, inOff, temp, 0);
                DesFunc(workingKey2, temp, 0, temp, 0);
                DesFunc(workingKey1, temp, 0, output, outOff);
            }

            return BLOCK_SIZE;
        }

        public override void Reset()
        {
        }
    }
}
