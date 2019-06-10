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

namespace Org.BouncyCastle.Bcpg
{
	/// <remarks>Base class for an RSA public key.</remarks>
	public class RsaPublicBcpgKey
		: BcpgObject, IBcpgKey
	{
		private readonly MPInteger n, e;

		/// <summary>Construct an RSA public key from the passed in stream.</summary>
		public RsaPublicBcpgKey(
			BcpgInputStream bcpgIn)
		{
			this.n = new MPInteger(bcpgIn);
			this.e = new MPInteger(bcpgIn);
		}

		/// <param name="n">The modulus.</param>
		/// <param name="e">The public exponent.</param>
		public RsaPublicBcpgKey(
			BigInteger	n,
			BigInteger	e)
		{
			this.n = new MPInteger(n);
			this.e = new MPInteger(e);
		}

		public BigInteger PublicExponent
		{
			get { return e.Value; }
		}

		public BigInteger Modulus
		{
			get { return n.Value; }
		}

		/// <summary>The format, as a string, always "PGP".</summary>
		public string Format
		{
			get { return "PGP"; }
		}

		/// <summary>Return the standard PGP encoding of the key.</summary>
		public override byte[] GetEncoded()
		{
			try
			{
				return base.GetEncoded();
			}
			catch (Exception)
			{
				return null;
			}
		}

		public override void Encode(
			BcpgOutputStream bcpgOut)
		{
			bcpgOut.WriteObjects(n, e);
		}
	}
}
