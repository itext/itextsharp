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
	* A Noekeon engine, using direct-key mode.
	*/
	public class NoekeonEngine
		: IBlockCipher
	{
		private const int GenericSize = 16; // Block and key size, as well as the amount of rounds.

		private static readonly uint[] nullVector = 
		{
			0x00, 0x00, 0x00, 0x00 // Used in decryption
		};

		private static readonly uint[] roundConstants = 
		{
			0x80, 0x1b, 0x36, 0x6c,
			0xd8, 0xab, 0x4d, 0x9a,
			0x2f, 0x5e, 0xbc, 0x63,
			0xc6, 0x97, 0x35, 0x6a,
			0xd4
		};

		private uint[]	state = new uint[4], // a
						subKeys = new uint[4], // k
						decryptKeys = new uint[4];

		private bool _initialised, _forEncryption;

		/**
		* Create an instance of the Noekeon encryption algorithm
		* and set some defaults
		*/
		public NoekeonEngine()
		{
			_initialised = false;
		}

		public string AlgorithmName
		{
			get { return "Noekeon"; }
		}

		public bool IsPartialBlockOkay
		{
			get { return false; }
		}

		public int GetBlockSize()
		{
			return GenericSize;
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
				throw new ArgumentException("Invalid parameters passed to Noekeon init - " + parameters.GetType().Name, "parameters");

			_forEncryption = forEncryption;
			_initialised = true;

			KeyParameter p = (KeyParameter) parameters;

			setKey(p.GetKey());
		}

		public int ProcessBlock(
			byte[]	input,
			int		inOff,
			byte[]	output,
			int		outOff)
		{
			if (!_initialised)
				throw new InvalidOperationException(AlgorithmName + " not initialised");
			if ((inOff + GenericSize) > input.Length)
				throw new DataLengthException("input buffer too short");
			if ((outOff + GenericSize) > output.Length)
				throw new DataLengthException("output buffer too short");

			return _forEncryption
				?	encryptBlock(input, inOff, output, outOff)
				:	decryptBlock(input, inOff, output, outOff);
		}

		public void Reset()
		{
			// TODO This should do something in case the encryption is aborted
		}

		/**
		* Re-key the cipher.
		*
		* @param  key  the key to be used
		*/
		private void setKey(byte[] key)
		{
			subKeys[0] = Pack.BE_To_UInt32(key, 0);
			subKeys[1] = Pack.BE_To_UInt32(key, 4);
			subKeys[2] = Pack.BE_To_UInt32(key, 8);
			subKeys[3] = Pack.BE_To_UInt32(key, 12);
		}

		private int encryptBlock(
			byte[]	input,
			int		inOff,
			byte[]	output,
			int		outOff)
		{
			state[0] = Pack.BE_To_UInt32(input, inOff);
			state[1] = Pack.BE_To_UInt32(input, inOff+4);
			state[2] = Pack.BE_To_UInt32(input, inOff+8);
			state[3] = Pack.BE_To_UInt32(input, inOff+12);

			int i;
			for (i = 0; i < GenericSize; i++)
			{
				state[0] ^= roundConstants[i];
				theta(state, subKeys);
				pi1(state);
				gamma(state);
				pi2(state);            
			}

			state[0] ^= roundConstants[i];
			theta(state, subKeys);

			Pack.UInt32_To_BE(state[0], output, outOff);
			Pack.UInt32_To_BE(state[1], output, outOff+4);
			Pack.UInt32_To_BE(state[2], output, outOff+8);
			Pack.UInt32_To_BE(state[3], output, outOff+12);

			return GenericSize;
		}

		private int decryptBlock(
			byte[]	input,
			int		inOff,
			byte[]	output,
			int		outOff)
		{
			state[0] = Pack.BE_To_UInt32(input, inOff);
			state[1] = Pack.BE_To_UInt32(input, inOff+4);
			state[2] = Pack.BE_To_UInt32(input, inOff+8);
			state[3] = Pack.BE_To_UInt32(input, inOff+12);

			Array.Copy(subKeys, 0, decryptKeys, 0, subKeys.Length);
			theta(decryptKeys, nullVector);

			int i;
			for (i = GenericSize; i > 0; i--)
			{
				theta(state, decryptKeys);
				state[0] ^= roundConstants[i];
				pi1(state);
				gamma(state);
				pi2(state);
			}

			theta(state, decryptKeys);
			state[0] ^= roundConstants[i];

			Pack.UInt32_To_BE(state[0], output, outOff);
			Pack.UInt32_To_BE(state[1], output, outOff+4);
			Pack.UInt32_To_BE(state[2], output, outOff+8);
			Pack.UInt32_To_BE(state[3], output, outOff+12);

			return GenericSize;
		}

		private void gamma(uint[] a)
		{
			a[1] ^= ~a[3] & ~a[2];
			a[0] ^= a[2] & a[1];

			uint tmp = a[3];
			a[3]  = a[0];
			a[0]  = tmp;
			a[2] ^= a[0]^a[1]^a[3];

			a[1] ^= ~a[3] & ~a[2];
			a[0] ^= a[2] & a[1];
		}

		private void theta(uint[] a, uint[] k)
		{
			uint tmp;
			tmp   = a[0]^a[2]; 
			tmp  ^= rotl(tmp,8)^rotl(tmp,24); 
			a[1] ^= tmp; 
			a[3] ^= tmp; 

			for (int i = 0; i < 4; i++)
			{
				a[i] ^= k[i];
			}

			tmp   = a[1]^a[3]; 
			tmp  ^= rotl(tmp,8)^rotl(tmp,24); 
			a[0] ^= tmp; 
			a[2] ^= tmp;
		}

		private void pi1(uint[] a)
		{
			a[1] = rotl(a[1], 1);
			a[2] = rotl(a[2], 5);
			a[3] = rotl(a[3], 2);
		}

		private void pi2(uint[] a)
		{
			a[1] = rotl(a[1], 31);
			a[2] = rotl(a[2], 27);
			a[3] = rotl(a[3], 30);
		}

		// Helpers

		private uint rotl(uint x, int y)
		{
			return (x << y) | (x >> (32-y));
		}
	}
}
