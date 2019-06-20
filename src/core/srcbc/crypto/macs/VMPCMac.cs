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

namespace Org.BouncyCastle.Crypto.Macs
{
	public class VmpcMac
		: IMac
	{
		private byte g;

		private byte n = 0;
		private byte[] P = null;
		private byte s = 0;

		private byte[] T;
		private byte[] workingIV;

		private byte[] workingKey;

		private byte x1, x2, x3, x4;

		public virtual int DoFinal(byte[] output, int outOff)
		{
			// Execute the Post-Processing Phase
			for (int r = 1; r < 25; r++)
			{
				s = P[(s + P[n & 0xff]) & 0xff];

				x4 = P[(x4 + x3 + r) & 0xff];
				x3 = P[(x3 + x2 + r) & 0xff];
				x2 = P[(x2 + x1 + r) & 0xff];
				x1 = P[(x1 + s + r) & 0xff];
				T[g & 0x1f] = (byte) (T[g & 0x1f] ^ x1);
				T[(g + 1) & 0x1f] = (byte) (T[(g + 1) & 0x1f] ^ x2);
				T[(g + 2) & 0x1f] = (byte) (T[(g + 2) & 0x1f] ^ x3);
				T[(g + 3) & 0x1f] = (byte) (T[(g + 3) & 0x1f] ^ x4);
				g = (byte) ((g + 4) & 0x1f);

				byte temp = P[n & 0xff];
				P[n & 0xff] = P[s & 0xff];
				P[s & 0xff] = temp;
				n = (byte) ((n + 1) & 0xff);
			}

			// Input T to the IV-phase of the VMPC KSA
			for (int m = 0; m < 768; m++)
			{
				s = P[(s + P[m & 0xff] + T[m & 0x1f]) & 0xff];
				byte temp = P[m & 0xff];
				P[m & 0xff] = P[s & 0xff];
				P[s & 0xff] = temp;
			}

			// Store 20 new outputs of the VMPC Stream Cipher input table M
			byte[] M = new byte[20];
			for (int i = 0; i < 20; i++)
			{
				s = P[(s + P[i & 0xff]) & 0xff];
				M[i] = P[(P[(P[s & 0xff]) & 0xff] + 1) & 0xff];

				byte temp = P[i & 0xff];
				P[i & 0xff] = P[s & 0xff];
				P[s & 0xff] = temp;
			}

			Array.Copy(M, 0, output, outOff, M.Length);
			Reset();

			return M.Length;
		}

		public virtual string AlgorithmName
		{
			get { return "VMPC-MAC"; }
		}

		public virtual int GetMacSize()
		{
			return 20;
		}

		public virtual void Init(ICipherParameters parameters)
		{
			if (!(parameters is ParametersWithIV))
				throw new ArgumentException("VMPC-MAC Init parameters must include an IV", "parameters");

			ParametersWithIV ivParams = (ParametersWithIV) parameters;
			KeyParameter key = (KeyParameter) ivParams.Parameters;

			if (!(ivParams.Parameters is KeyParameter))
				throw new ArgumentException("VMPC-MAC Init parameters must include a key", "parameters");

			this.workingIV = ivParams.GetIV();

			if (workingIV == null || workingIV.Length < 1 || workingIV.Length > 768)
				throw new ArgumentException("VMPC-MAC requires 1 to 768 bytes of IV", "parameters");

			this.workingKey = key.GetKey();

			Reset();

		}

		private void initKey(byte[] keyBytes, byte[] ivBytes)
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

		public virtual void Reset()
		{
			initKey(this.workingKey, this.workingIV);
			g = x1 = x2 = x3 = x4 = n = 0;
			T = new byte[32];
			for (int i = 0; i < 32; i++)
			{
				T[i] = 0;
			}
		}

		public virtual void Update(byte input)
		{
			s = P[(s + P[n & 0xff]) & 0xff];
			byte c = (byte) (input ^ P[(P[(P[s & 0xff]) & 0xff] + 1) & 0xff]);

			x4 = P[(x4 + x3) & 0xff];
			x3 = P[(x3 + x2) & 0xff];
			x2 = P[(x2 + x1) & 0xff];
			x1 = P[(x1 + s + c) & 0xff];
			T[g & 0x1f] = (byte) (T[g & 0x1f] ^ x1);
			T[(g + 1) & 0x1f] = (byte) (T[(g + 1) & 0x1f] ^ x2);
			T[(g + 2) & 0x1f] = (byte) (T[(g + 2) & 0x1f] ^ x3);
			T[(g + 3) & 0x1f] = (byte) (T[(g + 3) & 0x1f] ^ x4);
			g = (byte) ((g + 4) & 0x1f);

			byte temp = P[n & 0xff];
			P[n & 0xff] = P[s & 0xff];
			P[s & 0xff] = temp;
			n = (byte) ((n + 1) & 0xff);
		}

		public virtual void BlockUpdate(byte[] input, int inOff, int len)
		{
			if ((inOff + len) > input.Length)
				throw new DataLengthException("input buffer too short");

			for (int i = 0; i < len; i++)
			{
				Update(input[i]);
			}
		}
	}
}
