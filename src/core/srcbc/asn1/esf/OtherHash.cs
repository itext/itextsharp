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

using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Esf
{
	/// <remarks>
	/// <code>
	/// OtherHash ::= CHOICE {
	///		sha1Hash	OtherHashValue, -- This contains a SHA-1 hash
	/// 	otherHash	OtherHashAlgAndValue
	///	}
	///	
	///	OtherHashValue ::= OCTET STRING
	/// </code>
	/// </remarks>
	public class OtherHash
		: Asn1Encodable, IAsn1Choice
	{
		private readonly Asn1OctetString		sha1Hash;
		private readonly OtherHashAlgAndValue	otherHash;

		public static OtherHash GetInstance(
			object obj)
		{
			if (obj == null || obj is OtherHash)
				return (OtherHash) obj;

			if (obj is Asn1OctetString)
				return new OtherHash((Asn1OctetString) obj);

			return new OtherHash(
				OtherHashAlgAndValue.GetInstance(obj));
		}

		public OtherHash(
			byte[] sha1Hash)
		{
			if (sha1Hash == null)
				throw new ArgumentNullException("sha1Hash");

			this.sha1Hash = new DerOctetString(sha1Hash);
		}

		public OtherHash(
			Asn1OctetString sha1Hash)
		{
			if (sha1Hash == null)
				throw new ArgumentNullException("sha1Hash");

			this.sha1Hash = sha1Hash;
		}

		public OtherHash(
			OtherHashAlgAndValue otherHash)
		{
			if (otherHash == null)
				throw new ArgumentNullException("otherHash");

			this.otherHash = otherHash;
		}

		public AlgorithmIdentifier HashAlgorithm
		{
			get
			{
				return otherHash == null
					?	new AlgorithmIdentifier(OiwObjectIdentifiers.IdSha1)
					:	otherHash.HashAlgorithm;
			}
		}

		public byte[] GetHashValue()
		{
			return otherHash == null
				?	sha1Hash.GetOctets()
				:	otherHash.GetHashValue();
		}

		public override Asn1Object ToAsn1Object()
		{
			return otherHash == null
				?	sha1Hash
				:	otherHash.ToAsn1Object();
		}
	}
}
