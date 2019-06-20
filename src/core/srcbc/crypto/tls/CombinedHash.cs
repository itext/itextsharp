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

namespace Org.BouncyCastle.Crypto.Tls
{
	/// <remarks>A combined hash, which implements md5(m) || sha1(m).</remarks>
	internal class CombinedHash
		: IDigest
	{
		private readonly MD5Digest md5;
		private readonly Sha1Digest sha1;

		internal CombinedHash()
		{
			this.md5 = new MD5Digest();
			this.sha1 = new Sha1Digest();
		}

		internal CombinedHash(CombinedHash t)
		{
			this.md5 = new MD5Digest(t.md5);
			this.sha1 = new Sha1Digest(t.sha1);
		}

		/// <seealso cref="IDigest.AlgorithmName"/>
		public string AlgorithmName
		{
			get
			{
				return md5.AlgorithmName + " and " + sha1.AlgorithmName + " for TLS 1.0";
			}
		}

		/// <seealso cref="IDigest.GetByteLength"/>
		public int GetByteLength()
		{
			return System.Math.Max(md5.GetByteLength(), sha1.GetByteLength());
		}

		/// <seealso cref="IDigest.GetDigestSize"/>
		public int GetDigestSize()
		{
			return md5.GetDigestSize() + sha1.GetDigestSize();
		}

		/// <seealso cref="IDigest.Update"/>
		public void Update(
			byte input)
		{
			md5.Update(input);
			sha1.Update(input);
		}

		/// <seealso cref="IDigest.BlockUpdate"/>
		public void BlockUpdate(
			byte[]	input,
			int		inOff,
			int		len)
		{
			md5.BlockUpdate(input, inOff, len);
			sha1.BlockUpdate(input, inOff, len);
		}

		/// <seealso cref="IDigest.DoFinal"/>
		public int DoFinal(
			byte[]	output,
			int		outOff)
		{
			int i1 = md5.DoFinal(output, outOff);
			int i2 = sha1.DoFinal(output, outOff + i1);
			return i1 + i2;
		}

		/// <seealso cref="IDigest.Reset"/>
		public void Reset()
		{
			md5.Reset();
			sha1.Reset();
		}
	}
}
