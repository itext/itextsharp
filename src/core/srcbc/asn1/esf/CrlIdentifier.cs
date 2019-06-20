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

using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.Esf
{
	/// <remarks>
	/// RFC 3126: 4.2.2 Complete Revocation Refs Attribute Definition
	/// <code>
	/// CrlIdentifier ::= SEQUENCE 
	/// {
	/// 	crlissuer		Name,
	/// 	crlIssuedTime	UTCTime,
	/// 	crlNumber		INTEGER OPTIONAL
	/// }
	/// </code>
	/// </remarks>
	public class CrlIdentifier
		: Asn1Encodable
	{
		private readonly X509Name	crlIssuer;
		private readonly DerUtcTime	crlIssuedTime;
		private readonly DerInteger	crlNumber;

		public static CrlIdentifier GetInstance(
			object obj)
		{
			if (obj == null || obj is CrlIdentifier)
				return (CrlIdentifier) obj;

			if (obj is Asn1Sequence)
				return new CrlIdentifier((Asn1Sequence) obj);

			throw new ArgumentException(
				"Unknown object in 'CrlIdentifier' factory: "
					+ obj.GetType().Name,
				"obj");
		}

		private CrlIdentifier(
			Asn1Sequence seq)
		{
			if (seq == null)
				throw new ArgumentNullException("seq");
			if (seq.Count < 2 || seq.Count > 3)
				throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");

			this.crlIssuer = X509Name.GetInstance(seq[0]);
			this.crlIssuedTime = DerUtcTime.GetInstance(seq[1]);

			if (seq.Count > 2)
			{
				this.crlNumber = DerInteger.GetInstance(seq[2]);
			}
		}

		public CrlIdentifier(
			X509Name	crlIssuer,
			DateTime	crlIssuedTime)
			: this(crlIssuer, crlIssuedTime, null)
		{
		}

		public CrlIdentifier(
			X509Name	crlIssuer,
			DateTime	crlIssuedTime,
			BigInteger	crlNumber)
		{
			if (crlIssuer == null)
				throw new ArgumentNullException("crlIssuer");

			this.crlIssuer = crlIssuer;
			this.crlIssuedTime = new DerUtcTime(crlIssuedTime);

			if (crlNumber != null)
			{
				this.crlNumber = new DerInteger(crlNumber);
			}
		}

		public X509Name CrlIssuer
		{
			get { return crlIssuer; }
		}

		public DateTime CrlIssuedTime
		{
			get { return crlIssuedTime.ToAdjustedDateTime(); }
		}

		public BigInteger CrlNumber
		{
			get { return crlNumber == null ? null : crlNumber.Value; }
		}

		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector(
				crlIssuer.ToAsn1Object(), crlIssuedTime);

			if (crlNumber != null)
			{
				v.Add(crlNumber);
			}

			return new DerSequence(v);
		}
	}
}
