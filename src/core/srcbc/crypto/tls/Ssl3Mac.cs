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
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls
{
	/**
	 * HMAC implementation based on original internet draft for HMAC (RFC 2104)
	 * 
	 * The difference is that padding is concatentated versus XORed with the key
	 * 
	 * H(K + opad, H(K + ipad, text))
	 */
	public class Ssl3Mac
		: IMac
	{
		private const byte IPAD = 0x36;
		private const byte OPAD = 0x5C;

		internal static readonly byte[] MD5_IPAD = GenPad(IPAD, 48);
		internal static readonly byte[] MD5_OPAD = GenPad(OPAD, 48);
		internal static readonly byte[] SHA1_IPAD = GenPad(IPAD, 40);
		internal static readonly byte[] SHA1_OPAD = GenPad(OPAD, 40);

		private IDigest digest;

		private byte[] secret;
		private byte[] ipad, opad;

		/**
		 * Base constructor for one of the standard digest algorithms that the byteLength of
		 * the algorithm is know for. Behaviour is undefined for digests other than MD5 or SHA1.
		 * 
		 * @param digest the digest.
		 */
		public Ssl3Mac(IDigest digest)
		{
			this.digest = digest;

	        if (digest.GetDigestSize() == 20)
	        {
	            this.ipad = SHA1_IPAD;
	            this.opad = SHA1_OPAD;
	        }
	        else
	        {
	            this.ipad = MD5_IPAD;
	            this.opad = MD5_OPAD;
	        }
		}

		public virtual string AlgorithmName
		{
			get { return digest.AlgorithmName + "/SSL3MAC"; }
		}

		public virtual void Init(ICipherParameters parameters)
		{
			secret = Arrays.Clone(((KeyParameter)parameters).GetKey());

			Reset();
		}

		public virtual int GetMacSize()
		{
			return digest.GetDigestSize();
		}

		public virtual void Update(byte input)
		{
			digest.Update(input);
		}

		public virtual void BlockUpdate(byte[] input, int inOff, int len)
		{
			digest.BlockUpdate(input, inOff, len);
		}

		public virtual int DoFinal(byte[] output, int outOff)
		{
			byte[] tmp = new byte[digest.GetDigestSize()];
			digest.DoFinal(tmp, 0);

			digest.BlockUpdate(secret, 0, secret.Length);
			digest.BlockUpdate(opad, 0, opad.Length);
			digest.BlockUpdate(tmp, 0, tmp.Length);

			int len = digest.DoFinal(output, outOff);

			Reset();

			return len;
		}

		/**
		 * Reset the mac generator.
		 */
		public virtual void Reset()
		{
			digest.Reset();
			digest.BlockUpdate(secret, 0, secret.Length);
			digest.BlockUpdate(ipad, 0, ipad.Length);
		}

		private static byte[] GenPad(byte b, int count)
		{
			byte[] padding = new byte[count];
			Arrays.Fill(padding, b);
			return padding;
		}
	}
}
