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

using Org.BouncyCastle.Asn1.X500;
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.X509.SigI
{
	/**
	* Contains personal data for the otherName field in the subjectAltNames
	* extension.
	* <p/>
	* <pre>
	*     PersonalData ::= SEQUENCE {
	*       nameOrPseudonym NameOrPseudonym,
	*       nameDistinguisher [0] INTEGER OPTIONAL,
	*       dateOfBirth [1] GeneralizedTime OPTIONAL,
	*       placeOfBirth [2] DirectoryString OPTIONAL,
	*       gender [3] PrintableString OPTIONAL,
	*       postalAddress [4] DirectoryString OPTIONAL
	*       }
	* </pre>
	*
	* @see org.bouncycastle.asn1.x509.sigi.NameOrPseudonym
	* @see org.bouncycastle.asn1.x509.sigi.SigIObjectIdentifiers
	*/
	public class PersonalData
		: Asn1Encodable
	{
		private readonly NameOrPseudonym	nameOrPseudonym;
		private readonly BigInteger			nameDistinguisher;
		private readonly DerGeneralizedTime	dateOfBirth;
		private readonly DirectoryString	placeOfBirth;
		private readonly string				gender;
		private readonly DirectoryString	postalAddress;

		public static PersonalData GetInstance(
			object obj)
		{
			if (obj == null || obj is PersonalData)
			{
				return (PersonalData) obj;
			}

			if (obj is Asn1Sequence)
			{
				return new PersonalData((Asn1Sequence) obj);
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		/**
		* Constructor from Asn1Sequence.
		* <p/>
		* The sequence is of type NameOrPseudonym:
		* <p/>
		* <pre>
		*     PersonalData ::= SEQUENCE {
		*       nameOrPseudonym NameOrPseudonym,
		*       nameDistinguisher [0] INTEGER OPTIONAL,
		*       dateOfBirth [1] GeneralizedTime OPTIONAL,
		*       placeOfBirth [2] DirectoryString OPTIONAL,
		*       gender [3] PrintableString OPTIONAL,
		*       postalAddress [4] DirectoryString OPTIONAL
		*       }
		* </pre>
		*
		* @param seq The ASN.1 sequence.
		*/
		private PersonalData(
			Asn1Sequence seq)
		{
			if (seq.Count < 1)
				throw new ArgumentException("Bad sequence size: " + seq.Count);

			IEnumerator e = seq.GetEnumerator();
			e.MoveNext();

			nameOrPseudonym = NameOrPseudonym.GetInstance(e.Current);

			while (e.MoveNext())
			{
				Asn1TaggedObject o = Asn1TaggedObject.GetInstance(e.Current);
				int tag = o.TagNo;
				switch (tag)
				{
					case 0:
						nameDistinguisher = DerInteger.GetInstance(o, false).Value;
						break;
					case 1:
						dateOfBirth = DerGeneralizedTime.GetInstance(o, false);
						break;
					case 2:
						placeOfBirth = DirectoryString.GetInstance(o, true);
						break;
					case 3:
						gender = DerPrintableString.GetInstance(o, false).GetString();
						break;
					case 4:
						postalAddress = DirectoryString.GetInstance(o, true);
						break;
					default:
						throw new ArgumentException("Bad tag number: " + o.TagNo);
				}
			}
		}

		/**
		* Constructor from a given details.
		*
		* @param nameOrPseudonym  Name or pseudonym.
		* @param nameDistinguisher Name distinguisher.
		* @param dateOfBirth      Date of birth.
		* @param placeOfBirth     Place of birth.
		* @param gender           Gender.
		* @param postalAddress    Postal Address.
		*/
		public PersonalData(
			NameOrPseudonym		nameOrPseudonym,
			BigInteger			nameDistinguisher,
			DerGeneralizedTime	dateOfBirth,
			DirectoryString		placeOfBirth,
			string				gender,
			DirectoryString		postalAddress)
		{
			this.nameOrPseudonym = nameOrPseudonym;
			this.dateOfBirth = dateOfBirth;
			this.gender = gender;
			this.nameDistinguisher = nameDistinguisher;
			this.postalAddress = postalAddress;
			this.placeOfBirth = placeOfBirth;
		}

		public NameOrPseudonym NameOrPseudonym
		{
			get { return nameOrPseudonym; }
		}

		public BigInteger NameDistinguisher
		{
			get { return nameDistinguisher; }
		}

		public DerGeneralizedTime DateOfBirth
		{
			get { return dateOfBirth; }
		}

		public DirectoryString PlaceOfBirth
		{
			get { return placeOfBirth; }
		}

		public string Gender
		{
			get { return gender; }
		}

		public DirectoryString PostalAddress
		{
			get { return postalAddress; }
		}

		/**
		* Produce an object suitable for an Asn1OutputStream.
		* <p/>
		* Returns:
		* <p/>
		* <pre>
		*     PersonalData ::= SEQUENCE {
		*       nameOrPseudonym NameOrPseudonym,
		*       nameDistinguisher [0] INTEGER OPTIONAL,
		*       dateOfBirth [1] GeneralizedTime OPTIONAL,
		*       placeOfBirth [2] DirectoryString OPTIONAL,
		*       gender [3] PrintableString OPTIONAL,
		*       postalAddress [4] DirectoryString OPTIONAL
		*       }
		* </pre>
		*
		* @return an Asn1Object
		*/
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector vec = new Asn1EncodableVector();
			vec.Add(nameOrPseudonym);
			if (nameDistinguisher != null)
			{
				vec.Add(new DerTaggedObject(false, 0, new DerInteger(nameDistinguisher)));
			}
			if (dateOfBirth != null)
			{
				vec.Add(new DerTaggedObject(false, 1, dateOfBirth));
			}
			if (placeOfBirth != null)
			{
				vec.Add(new DerTaggedObject(true, 2, placeOfBirth));
			}
			if (gender != null)
			{
				vec.Add(new DerTaggedObject(false, 3, new DerPrintableString(gender, true)));
			}
			if (postalAddress != null)
			{
				vec.Add(new DerTaggedObject(true, 4, postalAddress));
			}
			return new DerSequence(vec);
		}
	}
}
