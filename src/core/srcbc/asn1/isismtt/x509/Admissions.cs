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

using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.IsisMtt.X509
{
	/**
	* An Admissions structure.
	* <p/>
	* <pre>
	*            Admissions ::= SEQUENCE
	*            {
	*              admissionAuthority [0] EXPLICIT GeneralName OPTIONAL
	*              namingAuthority [1] EXPLICIT NamingAuthority OPTIONAL
	*              professionInfos SEQUENCE OF ProfessionInfo
	*            }
	* <p/>
	* </pre>
	*
	* @see Org.BouncyCastle.Asn1.IsisMtt.X509.AdmissionSyntax
	* @see Org.BouncyCastle.Asn1.IsisMtt.X509.ProfessionInfo
	* @see Org.BouncyCastle.Asn1.IsisMtt.X509.NamingAuthority
	*/
	public class Admissions
		: Asn1Encodable
	{
		private readonly GeneralName		admissionAuthority;
		private readonly NamingAuthority	namingAuthority;
		private readonly Asn1Sequence		professionInfos;

		public static Admissions GetInstance(
			object obj)
		{
			if (obj == null || obj is Admissions)
			{
				return (Admissions) obj;
			}

			if (obj is Asn1Sequence)
			{
				return new Admissions((Asn1Sequence) obj);
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		/**
		* Constructor from Asn1Sequence.
		* <p/>
		* The sequence is of type ProcurationSyntax:
		* <p/>
		* <pre>
		*            Admissions ::= SEQUENCE
		*            {
		*              admissionAuthority [0] EXPLICIT GeneralName OPTIONAL
		*              namingAuthority [1] EXPLICIT NamingAuthority OPTIONAL
		*              professionInfos SEQUENCE OF ProfessionInfo
		*            }
		* </pre>
		*
		* @param seq The ASN.1 sequence.
		*/
		private Admissions(
			Asn1Sequence seq)
		{
			if (seq.Count > 3)
				throw new ArgumentException("Bad sequence size: " + seq.Count);

			IEnumerator e = seq.GetEnumerator();

			e.MoveNext();
			Asn1Encodable o = (Asn1Encodable) e.Current;
			if (o is Asn1TaggedObject)
			{
				switch (((Asn1TaggedObject)o).TagNo)
				{
					case 0:
						admissionAuthority = GeneralName.GetInstance((Asn1TaggedObject)o, true);
						break;
					case 1:
						namingAuthority = NamingAuthority.GetInstance((Asn1TaggedObject)o, true);
						break;
					default:
						throw new ArgumentException("Bad tag number: " + ((Asn1TaggedObject)o).TagNo);
				}
				e.MoveNext();
				o = (Asn1Encodable) e.Current;
			}
			if (o is Asn1TaggedObject)
			{
				switch (((Asn1TaggedObject)o).TagNo)
				{
					case 1:
						namingAuthority = NamingAuthority.GetInstance((Asn1TaggedObject)o, true);
						break;
					default:
						throw new ArgumentException("Bad tag number: " + ((Asn1TaggedObject)o).TagNo);
				}
				e.MoveNext();
				o = (Asn1Encodable) e.Current;
			}
			professionInfos = Asn1Sequence.GetInstance(o);
			if (e.MoveNext())
			{
				throw new ArgumentException("Bad object encountered: " + e.Current.GetType().Name);
			}
		}

		/**
		* Constructor from a given details.
		* <p/>
		* Parameter <code>professionInfos</code> is mandatory.
		*
		* @param admissionAuthority The admission authority.
		* @param namingAuthority    The naming authority.
		* @param professionInfos    The profession infos.
		*/
		public Admissions(
			GeneralName			admissionAuthority,
			NamingAuthority		namingAuthority,
			ProfessionInfo[]	professionInfos)
		{
			this.admissionAuthority = admissionAuthority;
			this.namingAuthority = namingAuthority;
			this.professionInfos = new DerSequence(professionInfos);
		}

		public virtual GeneralName AdmissionAuthority
		{
			get { return admissionAuthority; }
		}

		public virtual NamingAuthority NamingAuthority
		{
			get { return namingAuthority; }
		}

		public ProfessionInfo[] GetProfessionInfos()
		{
			ProfessionInfo[] infos = new ProfessionInfo[professionInfos.Count];
			int count = 0;
			foreach (Asn1Encodable ae in professionInfos)
			{
				infos[count++] = ProfessionInfo.GetInstance(ae);
			}
			return infos;
		}

		/**
		* Produce an object suitable for an Asn1OutputStream.
		* <p/>
		* Returns:
		* <p/>
		* <pre>
		*       Admissions ::= SEQUENCE
		*       {
		*         admissionAuthority [0] EXPLICIT GeneralName OPTIONAL
		*         namingAuthority [1] EXPLICIT NamingAuthority OPTIONAL
		*         professionInfos SEQUENCE OF ProfessionInfo
		*       }
		* <p/>
		* </pre>
		*
		* @return an Asn1Object
		*/
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector vec = new Asn1EncodableVector();

			if (admissionAuthority != null)
			{
				vec.Add(new DerTaggedObject(true, 0, admissionAuthority));
			}

			if (namingAuthority != null)
			{
				vec.Add(new DerTaggedObject(true, 1, namingAuthority));
			}

			vec.Add(professionInfos);

			return new DerSequence(vec);
		}
	}
}
