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
using Org.BouncyCastle.Crypto.Utilities;

namespace Org.BouncyCastle.Crypto.Engines
{
	/**
	* An TEA engine.
	*/
	public class TeaEngine
		: IBlockCipher
	{
		private const int
			rounds		= 32,
			block_size	= 8;
//			key_size	= 16,

		private const uint 
			delta		= 0x9E3779B9,
			d_sum		= 0xC6EF3720; // sum on decrypt

		/*
		* the expanded key array of 4 subkeys
		*/
		private uint _a, _b, _c, _d;
		private bool _initialised;
		private bool _forEncryption;

		/**
		* Create an instance of the TEA encryption algorithm
		* and set some defaults
		*/
		public TeaEngine()
		{
			_initialised = false;
		}

		public string AlgorithmName
		{
			get { return "TEA"; }
		}

		public bool IsPartialBlockOkay
		{
			get { return false; }
		}

		public int GetBlockSize()
		{
			return block_size;
		}

		/**
		* initialise
		*
		* @param forEncryption whether or not we are for encryption.
		* @param params the parameters required to set up the cipher.
		* @exception ArgumentException if the params argument is
		* inappropriate.
		*/
		public void Init(
			bool				forEncryption,
			ICipherParameters	parameters)
		{
			if (!(parameters is KeyParameter))
			{
				throw new ArgumentException("invalid parameter passed to TEA init - "
					+ parameters.GetType().FullName);
			}

			_forEncryption = forEncryption;
			_initialised = true;

			KeyParameter p = (KeyParameter) parameters;

			setKey(p.GetKey());
		}

		public int ProcessBlock(
			byte[]  inBytes,
			int     inOff,
			byte[]  outBytes,
			int     outOff)
		{
			if (!_initialised)
				throw new InvalidOperationException(AlgorithmName + " not initialised");

			if ((inOff + block_size) > inBytes.Length)
				throw new DataLengthException("input buffer too short");

			if ((outOff + block_size) > outBytes.Length)
				throw new DataLengthException("output buffer too short");

			return _forEncryption
				?	encryptBlock(inBytes, inOff, outBytes, outOff)
				:	decryptBlock(inBytes, inOff, outBytes, outOff);
		}

		public void Reset()
		{
		}

		/**
		* Re-key the cipher.
		*
		* @param  key  the key to be used
		*/
		private void setKey(
			byte[] key)
		{
			_a = Pack.BE_To_UInt32(key, 0);
			_b = Pack.BE_To_UInt32(key, 4);
			_c = Pack.BE_To_UInt32(key, 8);
			_d = Pack.BE_To_UInt32(key, 12);
		}

		private int encryptBlock(
			byte[]	inBytes,
			int		inOff,
			byte[]	outBytes,
			int		outOff)
		{
			// Pack bytes into integers
			uint v0 = Pack.BE_To_UInt32(inBytes, inOff);
			uint v1 = Pack.BE_To_UInt32(inBytes, inOff + 4);
	        
			uint sum = 0;
	        
			for (int i = 0; i != rounds; i++)
			{
				sum += delta;
				v0  += ((v1 << 4) + _a) ^ (v1 + sum) ^ ((v1 >> 5) + _b);
				v1  += ((v0 << 4) + _c) ^ (v0 + sum) ^ ((v0 >> 5) + _d);
			}

			Pack.UInt32_To_BE(v0, outBytes, outOff);
			Pack.UInt32_To_BE(v1, outBytes, outOff + 4);

			return block_size;
		}

		private int decryptBlock(
			byte[]	inBytes,
			int		inOff,
			byte[]	outBytes,
			int		outOff)
		{
			// Pack bytes into integers
			uint v0 = Pack.BE_To_UInt32(inBytes, inOff);
			uint v1 = Pack.BE_To_UInt32(inBytes, inOff + 4);

			uint sum = d_sum;

			for (int i = 0; i != rounds; i++)
			{
				v1  -= ((v0 << 4) + _c) ^ (v0 + sum) ^ ((v0 >> 5) + _d);
				v0  -= ((v1 << 4) + _a) ^ (v1 + sum) ^ ((v1 >> 5) + _b);
				sum -= delta;
			}

			Pack.UInt32_To_BE(v0, outBytes, outOff);
			Pack.UInt32_To_BE(v1, outBytes, outOff + 4);

			return block_size;
		}
	}
}
