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

namespace Org.BouncyCastle.Asn1.Esf
{
	/// <remarks>
	/// RFC 3126: 4.2.2 Complete Revocation Refs Attribute Definition
	/// <code>
	/// OtherRevRefs ::= SEQUENCE 
	/// {
	///		otherRevRefType      OtherRevRefType,
	///		otherRevRefs         ANY DEFINED BY otherRevRefType
	/// }
	///
	/// OtherRevRefType ::= OBJECT IDENTIFIER
	/// </code>
	/// </remarks>
	public class OtherRevRefs
		: Asn1Encodable
	{
		private readonly DerObjectIdentifier	otherRevRefType;
		private readonly Asn1Object				otherRevRefs;

		public static OtherRevRefs GetInstance(
			object obj)
		{
			if (obj == null || obj is OtherRevRefs)
				return (OtherRevRefs) obj;

			if (obj is Asn1Sequence)
				return new OtherRevRefs((Asn1Sequence) obj);

			throw new ArgumentException(
				"Unknown object in 'OtherRevRefs' factory: "
					+ obj.GetType().Name,
				"obj");
		}

		private OtherRevRefs(
			Asn1Sequence seq)
		{
			if (seq == null)
				throw new ArgumentNullException("seq");
			if (seq.Count != 2)
				throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");

			this.otherRevRefType = (DerObjectIdentifier) seq[0].ToAsn1Object();
			this.otherRevRefs = seq[1].ToAsn1Object();
		}

		public OtherRevRefs(
			DerObjectIdentifier	otherRevRefType,
			Asn1Encodable		otherRevRefs)
		{
			if (otherRevRefType == null)
				throw new ArgumentNullException("otherRevRefType");
			if (otherRevRefs == null)
				throw new ArgumentNullException("otherRevRefs");

			this.otherRevRefType = otherRevRefType;
			this.otherRevRefs = otherRevRefs.ToAsn1Object();
		}

		public DerObjectIdentifier OtherRevRefType
		{
			get { return otherRevRefType; }
		}

		public Asn1Object OtherRevRefsObject
		{
			get { return otherRevRefs; }
		}

		public override Asn1Object ToAsn1Object()
		{
			return new DerSequence(otherRevRefType, otherRevRefs);
		}
	}
}
