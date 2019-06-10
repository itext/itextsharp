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

using Org.BouncyCastle.Crypto.Digests;

namespace Org.BouncyCastle.Crypto.Prng
{
	/**
	 * Random generation based on the digest with counter. Calling AddSeedMaterial will
	 * always increase the entropy of the hash.
	 * <p>
	 * Internal access to the digest is synchronized so a single one of these can be shared.
	 * </p>
	 */
	public class DigestRandomGenerator
		: IRandomGenerator
	{
		private const long CYCLE_COUNT = 10;

		private long	stateCounter;
		private long	seedCounter;
		private IDigest	digest;
		private byte[]	state;
		private byte[]	seed;

		public DigestRandomGenerator(
			IDigest digest)
		{
			this.digest = digest;

			this.seed = new byte[digest.GetDigestSize()];
			this.seedCounter = 1;

			this.state = new byte[digest.GetDigestSize()];
			this.stateCounter = 1;
		}

		public void AddSeedMaterial(
			byte[] inSeed)
		{
			lock (this)
			{
				DigestUpdate(inSeed);
				DigestUpdate(seed);
				DigestDoFinal(seed);
			}
		}

		public void AddSeedMaterial(
			long rSeed)
		{
			lock (this)
			{
				DigestAddCounter(rSeed);
				DigestUpdate(seed);
				DigestDoFinal(seed);
			}
		}

		public void NextBytes(
			byte[] bytes)
		{
			NextBytes(bytes, 0, bytes.Length);
		}

		public void NextBytes(
			byte[]	bytes,
			int		start,
			int		len)
		{
			lock (this)
			{
				int stateOff = 0;

				GenerateState();

				int end = start + len;
				for (int i = start; i < end; ++i)
				{
					if (stateOff == state.Length)
					{
						GenerateState();
						stateOff = 0;
					}
					bytes[i] = state[stateOff++];
				}
			}
		}

		private void CycleSeed()
		{
			DigestUpdate(seed);
			DigestAddCounter(seedCounter++);
			DigestDoFinal(seed);
		}

		private void GenerateState()
		{
			DigestAddCounter(stateCounter++);
			DigestUpdate(state);
			DigestUpdate(seed);
			DigestDoFinal(state);

			if ((stateCounter % CYCLE_COUNT) == 0)
			{
				CycleSeed();
			}
		}

		private void DigestAddCounter(long seedVal)
		{
			ulong seed = (ulong)seedVal;
			for (int i = 0; i != 8; i++)
			{
				digest.Update((byte)seed);
				seed >>= 8;
			}
		}

		private void DigestUpdate(byte[] inSeed)
		{
			digest.BlockUpdate(inSeed, 0, inSeed.Length);
		}

		private void DigestDoFinal(byte[] result)
		{
			digest.DoFinal(result, 0);
		}
	}
}
