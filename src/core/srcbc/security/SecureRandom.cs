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
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Security
{
    public class SecureRandom
		: Random
    {
		// Note: all objects of this class should be deriving their random data from
		// a single generator appropriate to the digest being used.
		private static readonly IRandomGenerator sha1Generator = new DigestRandomGenerator(new Sha1Digest());
		private static readonly IRandomGenerator sha256Generator = new DigestRandomGenerator(new Sha256Digest());

		private static readonly SecureRandom[] master = { null };
		private static SecureRandom Master
		{
			get
			{
				if (master[0] == null)
				{
					IRandomGenerator gen = sha256Generator;
					gen = new ReversedWindowGenerator(gen, 32);
					SecureRandom sr = master[0] = new SecureRandom(gen);

					sr.SetSeed(DateTime.Now.Ticks);
					sr.SetSeed(new ThreadedSeedGenerator().GenerateSeed(24, true));
					sr.GenerateSeed(1 + sr.Next(32));
				}

				return master[0];
			}
		}

		public static SecureRandom GetInstance(
			string algorithm)
		{
			// TODO Compared to JDK, we don't auto-seed if the client forgets - problem?

			// TODO Support all digests more generally, by stripping PRNG and calling DigestUtilities?
			string drgName = Platform.ToUpperInvariant(algorithm);

			IRandomGenerator drg = null;
			if (drgName == "SHA1PRNG")
			{
				drg = sha1Generator;
			}
			else if (drgName == "SHA256PRNG")
			{
				drg = sha256Generator;
			}

			if (drg != null)
			{
				return new SecureRandom(drg);
			}

			throw new ArgumentException("Unrecognised PRNG algorithm: " + algorithm, "algorithm");
		}

		public static byte[] GetSeed(
			int length)
		{
			return Master.GenerateSeed(length);
		}

		protected IRandomGenerator generator;

		public SecureRandom()
			: this(sha1Generator)
        {
			SetSeed(GetSeed(8));
		}

		public SecureRandom(
			byte[] inSeed)
			: this(sha1Generator)
        {
			SetSeed(inSeed);
        }

		/// <summary>Use the specified instance of IRandomGenerator as random source.</summary>
		/// <remarks>
		/// This constructor performs no seeding of either the <c>IRandomGenerator</c> or the
		/// constructed <c>SecureRandom</c>. It is the responsibility of the client to provide
		/// proper seed material as necessary/appropriate for the given <c>IRandomGenerator</c>
		/// implementation.
		/// </remarks>
		/// <param name="generator">The source to generate all random bytes from.</param>
		public SecureRandom(
			IRandomGenerator generator)
			: base(0)
		{
			this.generator = generator;
		}

		public virtual byte[] GenerateSeed(
			int length)
		{
			SetSeed(DateTime.Now.Ticks);

			byte[] rv = new byte[length];
			NextBytes(rv);
			return rv;
		}

		public virtual void SetSeed(
			byte[] inSeed)
        {
			generator.AddSeedMaterial(inSeed);
        }

        public virtual void SetSeed(
			long seed)
        {
			generator.AddSeedMaterial(seed);
		}

		public override int Next()
		{
			for (;;)
			{
				int i = NextInt() & int.MaxValue;

				if (i != int.MaxValue)
					return i;
			}
		}

		public override int Next(
			int maxValue)
		{
			if (maxValue < 2)
			{
				if (maxValue < 0)
					throw new ArgumentOutOfRangeException("maxValue", "cannot be negative");

				return 0;
			}

			// Test whether maxValue is a power of 2
			if ((maxValue & -maxValue) == maxValue)
			{
				int val = NextInt() & int.MaxValue;
				long lr = ((long) maxValue * (long) val) >> 31;
				return (int) lr;
			}

			int bits, result;
			do
			{
				bits = NextInt() & int.MaxValue;
				result = bits % maxValue;
			}
			while (bits - result + (maxValue - 1) < 0); // Ignore results near overflow

			return result;
		}

		public override int Next(
			int	minValue,
			int	maxValue)
		{
			if (maxValue <= minValue)
			{
				if (maxValue == minValue)
					return minValue;

				throw new ArgumentException("maxValue cannot be less than minValue");
			}

			int diff = maxValue - minValue;
			if (diff > 0)
				return minValue + Next(diff);

			for (;;)
			{
				int i = NextInt();

				if (i >= minValue && i < maxValue)
					return i;
			}
		}

		public override void NextBytes(
			byte[] buffer)
        {
			generator.NextBytes(buffer);
        }

		public virtual void NextBytes(
			byte[]	buffer,
			int		start,
			int		length)
		{
			generator.NextBytes(buffer, start, length);
		}

		private static readonly double DoubleScale = System.Math.Pow(2.0, 64.0);

		public override double NextDouble()
		{
			return Convert.ToDouble((ulong) NextLong()) / DoubleScale;
		}

		public virtual int NextInt()
        {
			byte[] intBytes = new byte[4];
            NextBytes(intBytes);

			int result = 0;
            for (int i = 0; i < 4; i++)
            {
                result = (result << 8) + (intBytes[i] & 0xff);
            }

			return result;
        }

		public virtual long NextLong()
		{
			return ((long)(uint) NextInt() << 32) | (long)(uint) NextInt();
		}
    }
}
