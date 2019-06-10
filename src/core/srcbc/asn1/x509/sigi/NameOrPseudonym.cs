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

namespace Org.BouncyCastle.Asn1.X509.SigI
{
	/**
	* Structure for a name or pseudonym.
	* 
	* <pre>
	*       NameOrPseudonym ::= CHOICE {
	*     	   surAndGivenName SEQUENCE {
	*     	     surName DirectoryString,
	*     	     givenName SEQUENCE OF DirectoryString 
	*         },
	*     	   pseudonym DirectoryString 
	*       }
	* </pre>
	* 
	* @see org.bouncycastle.asn1.x509.sigi.PersonalData
	* 
	*/
	public class NameOrPseudonym
		: Asn1Encodable, IAsn1Choice
	{
		private readonly DirectoryString	pseudonym;
		private readonly DirectoryString	surname;
		private readonly Asn1Sequence		givenName;

		public static NameOrPseudonym GetInstance(
			object obj)
		{
			if (obj == null || obj is NameOrPseudonym)
			{
				return (NameOrPseudonym)obj;
			}

			if (obj is IAsn1String)
			{
				return new NameOrPseudonym(DirectoryString.GetInstance(obj));
			}

			if (obj is Asn1Sequence)
			{
				return new NameOrPseudonym((Asn1Sequence) obj);
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		/**
		* Constructor from DERString.
		* <p/>
		* The sequence is of type NameOrPseudonym:
		* <p/>
		* <pre>
		*       NameOrPseudonym ::= CHOICE {
		*     	   surAndGivenName SEQUENCE {
		*     	     surName DirectoryString,
		*     	     givenName SEQUENCE OF DirectoryString
		*         },
		*     	   pseudonym DirectoryString
		*       }
		* </pre>
		* @param pseudonym pseudonym value to use.
		*/
		public NameOrPseudonym(
			DirectoryString pseudonym)
		{
			this.pseudonym = pseudonym;
		}

		/**
		* Constructor from Asn1Sequence.
		* <p/>
		* The sequence is of type NameOrPseudonym:
		* <p/>
		* <pre>
		*       NameOrPseudonym ::= CHOICE {
		*     	   surAndGivenName SEQUENCE {
		*     	     surName DirectoryString,
		*     	     givenName SEQUENCE OF DirectoryString
		*         },
		*     	   pseudonym DirectoryString
		*       }
		* </pre>
		*
		* @param seq The ASN.1 sequence.
		*/
		private NameOrPseudonym(
			Asn1Sequence seq)
		{
			if (seq.Count != 2)
				throw new ArgumentException("Bad sequence size: " + seq.Count);

			if (!(seq[0] is IAsn1String))
				throw new ArgumentException("Bad object encountered: " + seq[0].GetType().Name);

			surname = DirectoryString.GetInstance(seq[0]);
			givenName = Asn1Sequence.GetInstance(seq[1]);
		}

		/**
		* Constructor from a given details.
		*
		* @param pseudonym The pseudonym.
		*/
		public NameOrPseudonym(
			string pseudonym)
			: this(new DirectoryString(pseudonym))
		{
		}

		/**
		 * Constructor from a given details.
		 *
		 * @param surname   The surname.
		 * @param givenName A sequence of directory strings making up the givenName
		 */
		public NameOrPseudonym(
			DirectoryString	surname,
			Asn1Sequence	givenName)
		{
			this.surname = surname;
			this.givenName = givenName;
		}

		public DirectoryString Pseudonym
		{
			get { return pseudonym; }
		}

		public DirectoryString Surname
		{
			get { return surname; }
		}

		public DirectoryString[] GetGivenName()
		{
			DirectoryString[] items = new DirectoryString[givenName.Count];
			int count = 0;
			foreach (object o in givenName)
			{
				items[count++] = DirectoryString.GetInstance(o);
			}
			return items;
		}

		/**
		* Produce an object suitable for an Asn1OutputStream.
		* <p/>
		* Returns:
		* <p/>
		* <pre>
		*       NameOrPseudonym ::= CHOICE {
		*     	   surAndGivenName SEQUENCE {
		*     	     surName DirectoryString,
		*     	     givenName SEQUENCE OF DirectoryString
		*         },
		*     	   pseudonym DirectoryString
		*       }
		* </pre>
		*
		* @return an Asn1Object
		*/
		public override Asn1Object ToAsn1Object()
		{
			if (pseudonym != null)
			{
				return pseudonym.ToAsn1Object();
			}

			return new DerSequence(surname, givenName);
		}
	}
}
