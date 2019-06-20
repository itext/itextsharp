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
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Org.BouncyCastle.Ocsp
{
	public class OcspResp
	{
		private OcspResponse resp;

		public OcspResp(
			OcspResponse resp)
		{
			this.resp = resp;
		}

		public OcspResp(
			byte[] resp)
			: this(new Asn1InputStream(resp))
		{
		}

		public OcspResp(
			Stream inStr)
			: this(new Asn1InputStream(inStr))
		{
		}

		private OcspResp(
			Asn1InputStream aIn)
		{
			try
			{
				this.resp = OcspResponse.GetInstance(aIn.ReadObject());
			}
			catch (Exception e)
			{
				throw new IOException("malformed response: " + e.Message, e);
			}
		}

		public int Status
		{
			get { return this.resp.ResponseStatus.Value.IntValue; }
		}

		public object GetResponseObject()
		{
			ResponseBytes rb = this.resp.ResponseBytes;

			if (rb == null)
				return null;

			if (rb.ResponseType.Equals(OcspObjectIdentifiers.PkixOcspBasic))
			{
				try
				{
					return new BasicOcspResp(
						BasicOcspResponse.GetInstance(
							Asn1Object.FromByteArray(rb.Response.GetOctets())));
				}
				catch (Exception e)
				{
					throw new OcspException("problem decoding object: " + e, e);
				}
			}

			return rb.Response;
		}

		/**
		* return the ASN.1 encoded representation of this object.
		*/
		public byte[] GetEncoded()
		{
			return resp.GetEncoded();
		}

		public override bool Equals(
			object obj)
		{
			if (obj == this)
				return true;

			OcspResp other = obj as OcspResp;

			if (other == null)
				return false;

			return resp.Equals(other.resp);
		}

		public override int GetHashCode()
		{
			return resp.GetHashCode();
		}
	}
}
