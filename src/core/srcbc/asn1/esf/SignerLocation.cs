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

using Org.BouncyCastle.Asn1;

namespace Org.BouncyCastle.Asn1.Esf
{
	/**
	* Signer-Location attribute (RFC3126).
	*
	* <pre>
	*   SignerLocation ::= SEQUENCE {
	*       countryName        [0] DirectoryString OPTIONAL,
	*       localityName       [1] DirectoryString OPTIONAL,
	*       postalAddress      [2] PostalAddress OPTIONAL }
	*
	*   PostalAddress ::= SEQUENCE SIZE(1..6) OF DirectoryString
	* </pre>
	*/
	public class SignerLocation
		: Asn1Encodable
	{
		// TODO Should these be using DirectoryString?
		private DerUtf8String	countryName;
		private DerUtf8String	localityName;
		private Asn1Sequence	postalAddress;

		public SignerLocation(
			Asn1Sequence seq)
		{
			foreach (Asn1TaggedObject obj in seq)
			{
				switch (obj.TagNo)
				{
					case 0:
						this.countryName = DerUtf8String.GetInstance(obj, true);
						break;
					case 1:
						this.localityName = DerUtf8String.GetInstance(obj, true);
						break;
					case 2:
						bool isExplicit = obj.IsExplicit();	// handle erroneous implicitly tagged sequences
						this.postalAddress = Asn1Sequence.GetInstance(obj, isExplicit);
						if (postalAddress != null && postalAddress.Count > 6)
							throw new ArgumentException("postal address must contain less than 6 strings");
						break;
					default:
						throw new ArgumentException("illegal tag");
				}
			}
		}

		public SignerLocation(
			DerUtf8String	countryName,
			DerUtf8String	localityName,
			Asn1Sequence	postalAddress)
		{
			if (postalAddress != null && postalAddress.Count > 6)
			{
				throw new ArgumentException("postal address must contain less than 6 strings");
			}

			if (countryName != null)
			{
				this.countryName = DerUtf8String.GetInstance(countryName.ToAsn1Object());
			}

			if (localityName != null)
			{
				this.localityName = DerUtf8String.GetInstance(localityName.ToAsn1Object());
			}

			if (postalAddress != null)
			{
				this.postalAddress = (Asn1Sequence) postalAddress.ToAsn1Object();
			}
		}

		public static SignerLocation GetInstance(
			object obj)
		{
			if (obj == null || obj is SignerLocation)
			{
				return (SignerLocation) obj;
			}

			return new SignerLocation(Asn1Sequence.GetInstance(obj));
		}

		public DerUtf8String CountryName
		{
			get { return countryName; }
		}

		public DerUtf8String LocalityName
		{
			get { return localityName; }
		}

		public Asn1Sequence PostalAddress
		{
			get { return postalAddress; }
		}

		/**
		* <pre>
		*   SignerLocation ::= SEQUENCE {
		*       countryName        [0] DirectoryString OPTIONAL,
		*       localityName       [1] DirectoryString OPTIONAL,
		*       postalAddress      [2] PostalAddress OPTIONAL }
		*
		*   PostalAddress ::= SEQUENCE SIZE(1..6) OF DirectoryString
		*
		*   DirectoryString ::= CHOICE {
		*         teletexString           TeletexString (SIZE (1..MAX)),
		*         printableString         PrintableString (SIZE (1..MAX)),
		*         universalString         UniversalString (SIZE (1..MAX)),
		*         utf8String              UTF8String (SIZE (1.. MAX)),
		*         bmpString               BMPString (SIZE (1..MAX)) }
		* </pre>
		*/
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector();

			if (countryName != null)
			{
				v.Add(new DerTaggedObject(true, 0, countryName));
			}

			if (localityName != null)
			{
				v.Add(new DerTaggedObject(true, 1, localityName));
			}

			if (postalAddress != null)
			{
				v.Add(new DerTaggedObject(true, 2, postalAddress));
			}

			return new DerSequence(v);
		}
	}
}
