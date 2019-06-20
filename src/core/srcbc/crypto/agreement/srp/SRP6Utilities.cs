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

using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Agreement.Srp
{
	public class Srp6Utilities
	{
		public static BigInteger CalculateK(IDigest digest, BigInteger N, BigInteger g)
		{
			return HashPaddedPair(digest, N, N, g);
		}

	    public static BigInteger CalculateU(IDigest digest, BigInteger N, BigInteger A, BigInteger B)
	    {
	    	return HashPaddedPair(digest, N, A, B);
	    }

		public static BigInteger CalculateX(IDigest digest, BigInteger N, byte[] salt, byte[] identity, byte[] password)
	    {
	        byte[] output = new byte[digest.GetDigestSize()];

	        digest.BlockUpdate(identity, 0, identity.Length);
	        digest.Update((byte)':');
	        digest.BlockUpdate(password, 0, password.Length);
	        digest.DoFinal(output, 0);

	        digest.BlockUpdate(salt, 0, salt.Length);
	        digest.BlockUpdate(output, 0, output.Length);
	        digest.DoFinal(output, 0);

	        return new BigInteger(1, output);
	    }

		public static BigInteger GeneratePrivateValue(IDigest digest, BigInteger N, BigInteger g, SecureRandom random)
	    {
			int minBits = System.Math.Min(256, N.BitLength / 2);
	        BigInteger min = BigInteger.One.ShiftLeft(minBits - 1);
	        BigInteger max = N.Subtract(BigInteger.One);

	        return BigIntegers.CreateRandomInRange(min, max, random);
	    }

		public static BigInteger ValidatePublicValue(BigInteger N, BigInteger val)
		{
		    val = val.Mod(N);

	        // Check that val % N != 0
	        if (val.Equals(BigInteger.Zero))
	            throw new CryptoException("Invalid public value: 0");

		    return val;
		}

		private static BigInteger HashPaddedPair(IDigest digest, BigInteger N, BigInteger n1, BigInteger n2)
		{
	    	int padLength = (N.BitLength + 7) / 8;

	    	byte[] n1_bytes = GetPadded(n1, padLength);
	    	byte[] n2_bytes = GetPadded(n2, padLength);

	        digest.BlockUpdate(n1_bytes, 0, n1_bytes.Length);
	        digest.BlockUpdate(n2_bytes, 0, n2_bytes.Length);

	        byte[] output = new byte[digest.GetDigestSize()];
	        digest.DoFinal(output, 0);

	        return new BigInteger(1, output);
		}

		private static byte[] GetPadded(BigInteger n, int length)
		{
			byte[] bs = BigIntegers.AsUnsignedByteArray(n);
			if (bs.Length < length)
			{
				byte[] tmp = new byte[length];
				Array.Copy(bs, 0, tmp, length - bs.Length, bs.Length);
				bs = tmp;
			}
			return bs;
		}
	}
}
