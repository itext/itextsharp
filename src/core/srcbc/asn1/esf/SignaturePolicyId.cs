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
using System.Collections;

using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Asn1.Esf
{
	/// <remarks>
	/// <code>
	/// SignaturePolicyId ::= SEQUENCE {
	/// 	sigPolicyIdentifier		SigPolicyId,
	/// 	sigPolicyHash			SigPolicyHash,
	/// 	sigPolicyQualifiers		SEQUENCE SIZE (1..MAX) OF SigPolicyQualifierInfo OPTIONAL
	/// }
	/// 
	/// SigPolicyId ::= OBJECT IDENTIFIER
	/// 
	/// SigPolicyHash ::= OtherHashAlgAndValue
	/// </code>
	/// </remarks>
	public class SignaturePolicyId
		: Asn1Encodable
	{
		private readonly DerObjectIdentifier	sigPolicyIdentifier;
		private readonly OtherHashAlgAndValue	sigPolicyHash;
		private readonly Asn1Sequence			sigPolicyQualifiers;

		public static SignaturePolicyId GetInstance(
			object obj)
		{
			if (obj == null || obj is SignaturePolicyId)
				return (SignaturePolicyId) obj;

			if (obj is Asn1Sequence)
				return new SignaturePolicyId((Asn1Sequence) obj);

			throw new ArgumentException(
				"Unknown object in 'SignaturePolicyId' factory: "
					+ obj.GetType().Name,
				"obj");
		}

		private SignaturePolicyId(
			Asn1Sequence seq)
		{
			if (seq == null)
				throw new ArgumentNullException("seq");
			if (seq.Count < 2 || seq.Count > 3)
				throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");

			this.sigPolicyIdentifier = (DerObjectIdentifier) seq[0].ToAsn1Object();
			this.sigPolicyHash = OtherHashAlgAndValue.GetInstance(seq[1].ToAsn1Object());

			if (seq.Count > 2)
			{
				this.sigPolicyQualifiers = (Asn1Sequence) seq[2].ToAsn1Object();
			}
		}

		public SignaturePolicyId(
			DerObjectIdentifier		sigPolicyIdentifier,
			OtherHashAlgAndValue	sigPolicyHash)
			: this(sigPolicyIdentifier, sigPolicyHash, null)
		{
		}

		public SignaturePolicyId(
			DerObjectIdentifier				sigPolicyIdentifier,
			OtherHashAlgAndValue			sigPolicyHash,
			params SigPolicyQualifierInfo[]	sigPolicyQualifiers)
		{
			if (sigPolicyIdentifier == null)
				throw new ArgumentNullException("sigPolicyIdentifier");
			if (sigPolicyHash == null)
				throw new ArgumentNullException("sigPolicyHash");

			this.sigPolicyIdentifier = sigPolicyIdentifier;
			this.sigPolicyHash = sigPolicyHash;

			if (sigPolicyQualifiers != null)
			{
				this.sigPolicyQualifiers = new DerSequence(sigPolicyQualifiers);
			}
		}

		public SignaturePolicyId(
			DerObjectIdentifier		sigPolicyIdentifier,
			OtherHashAlgAndValue	sigPolicyHash,
			IEnumerable				sigPolicyQualifiers)
		{
			if (sigPolicyIdentifier == null)
				throw new ArgumentNullException("sigPolicyIdentifier");
			if (sigPolicyHash == null)
				throw new ArgumentNullException("sigPolicyHash");

			this.sigPolicyIdentifier = sigPolicyIdentifier;
			this.sigPolicyHash = sigPolicyHash;

			if (sigPolicyQualifiers != null)
			{
				if (!CollectionUtilities.CheckElementsAreOfType(sigPolicyQualifiers, typeof(SigPolicyQualifierInfo)))
					throw new ArgumentException("Must contain only 'SigPolicyQualifierInfo' objects", "sigPolicyQualifiers");

				this.sigPolicyQualifiers = new DerSequence(
					Asn1EncodableVector.FromEnumerable(sigPolicyQualifiers));
			}
		}

		public DerObjectIdentifier SigPolicyIdentifier
		{
			get { return sigPolicyIdentifier; }
		}

		public OtherHashAlgAndValue SigPolicyHash
		{
			get { return sigPolicyHash; }
		}

		public SigPolicyQualifierInfo[] GetSigPolicyQualifiers()
		{
			if (sigPolicyQualifiers == null)
				return null;

			SigPolicyQualifierInfo[] infos = new SigPolicyQualifierInfo[sigPolicyQualifiers.Count];
			for (int i = 0; i < sigPolicyQualifiers.Count; ++i)
			{
				infos[i] = SigPolicyQualifierInfo.GetInstance(sigPolicyQualifiers[i]);
			}
			return infos;
		}

		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector(
				sigPolicyIdentifier, sigPolicyHash.ToAsn1Object());

			if (sigPolicyQualifiers != null)
			{
				v.Add(sigPolicyQualifiers.ToAsn1Object());
			}

			return new DerSequence(v);
		}
	}
}
