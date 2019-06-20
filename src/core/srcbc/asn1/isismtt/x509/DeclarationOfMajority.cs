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

namespace Org.BouncyCastle.Asn1.IsisMtt.X509
{
	/**
	* A declaration of majority.
	* <p/>
	* <pre>
	*           DeclarationOfMajoritySyntax ::= CHOICE
	*           {
	*             notYoungerThan [0] IMPLICIT INTEGER,
	*             fullAgeAtCountry [1] IMPLICIT SEQUENCE
	*             {
	*               fullAge BOOLEAN DEFAULT TRUE,
	*               country PrintableString (SIZE(2))
	*             }
	*             dateOfBirth [2] IMPLICIT GeneralizedTime
	*           }
	* </pre>
	* <p/>
	* fullAgeAtCountry indicates the majority of the owner with respect to the laws
	* of a specific country.
	*/
	public class DeclarationOfMajority
		: Asn1Encodable, IAsn1Choice
	{
		public enum Choice
		{
			NotYoungerThan = 0,
			FullAgeAtCountry = 1,
			DateOfBirth = 2
		};

		private readonly Asn1TaggedObject declaration;

		public DeclarationOfMajority(
			int notYoungerThan)
		{
			declaration = new DerTaggedObject(false, 0, new DerInteger(notYoungerThan));
		}

		public DeclarationOfMajority(
			bool	fullAge,
			string	country)
		{
			if (country.Length > 2)
				throw new ArgumentException("country can only be 2 characters");

			DerPrintableString countryString = new DerPrintableString(country, true);

			DerSequence seq;
			if (fullAge)
			{
				seq = new DerSequence(countryString);
			}
			else
			{
				seq = new DerSequence(DerBoolean.False, countryString);
			}

			this.declaration = new DerTaggedObject(false, 1, seq);
		}

		public DeclarationOfMajority(
			DerGeneralizedTime dateOfBirth)
		{
			this.declaration = new DerTaggedObject(false, 2, dateOfBirth);
		}

		public static DeclarationOfMajority GetInstance(
			object obj)
		{
			if (obj == null || obj is DeclarationOfMajority)
			{
				return (DeclarationOfMajority) obj;
			}

			if (obj is Asn1TaggedObject)
			{
				return new DeclarationOfMajority((Asn1TaggedObject) obj);
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		private DeclarationOfMajority(
			Asn1TaggedObject o)
		{
			if (o.TagNo > 2)
				throw new ArgumentException("Bad tag number: " + o.TagNo);

			this.declaration = o;
		}

		/**
		* Produce an object suitable for an Asn1OutputStream.
		* <p/>
		* Returns:
		* <p/>
		* <pre>
		*           DeclarationOfMajoritySyntax ::= CHOICE
		*           {
		*             notYoungerThan [0] IMPLICIT INTEGER,
		*             fullAgeAtCountry [1] IMPLICIT SEQUENCE
		*             {
		*               fullAge BOOLEAN DEFAULT TRUE,
		*               country PrintableString (SIZE(2))
		*             }
		*             dateOfBirth [2] IMPLICIT GeneralizedTime
		*           }
		* </pre>
		*
		* @return an Asn1Object
		*/
		public override Asn1Object ToAsn1Object()
		{
			return declaration;
		}

		public Choice Type
		{
			get { return (Choice) declaration.TagNo; }
		}

		/**
		* @return notYoungerThan if that's what we are, -1 otherwise
		*/
		public virtual int NotYoungerThan
		{
			get
			{
				switch ((Choice) declaration.TagNo)
				{
					case Choice.NotYoungerThan:
						return DerInteger.GetInstance(declaration, false).Value.IntValue;
					default:
						return -1;
				}
			}
		}

		public virtual Asn1Sequence FullAgeAtCountry
		{
			get
			{
				switch ((Choice) declaration.TagNo)
				{
					case Choice.FullAgeAtCountry:
						return Asn1Sequence.GetInstance(declaration, false);
					default:
						return null;
				}
			}
		}

		public virtual DerGeneralizedTime DateOfBirth
		{
			get
			{
				switch ((Choice) declaration.TagNo)
				{
					case Choice.DateOfBirth:
						return DerGeneralizedTime.GetInstance(declaration, false);
					default:
						return null;
				}
			}
		}
	}
}
