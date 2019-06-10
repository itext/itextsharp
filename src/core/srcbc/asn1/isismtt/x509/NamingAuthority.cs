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

namespace Org.BouncyCastle.Asn1.IsisMtt.X509
{
	/**
	* Names of authorities which are responsible for the administration of title
	* registers.
	* 
	* <pre>
	*             NamingAuthority ::= SEQUENCE 
	*             {
	*               namingAuthorityID OBJECT IDENTIFIER OPTIONAL,
	*               namingAuthorityUrl IA5String OPTIONAL,
	*               namingAuthorityText DirectoryString(SIZE(1..128)) OPTIONAL
	*             }
	* </pre>
	* @see Org.BouncyCastle.Asn1.IsisMtt.X509.AdmissionSyntax
	* 
	*/
	public class NamingAuthority
		: Asn1Encodable
	{
		/**
		* Profession OIDs should always be defined under the OID branch of the
		* responsible naming authority. At the time of this writing, the work group
		* �Recht, Wirtschaft, Steuern� (�Law, Economy, Taxes�) is registered as the
		* first naming authority under the OID id-isismtt-at-namingAuthorities.
		*/
		public static readonly DerObjectIdentifier IdIsisMttATNamingAuthoritiesRechtWirtschaftSteuern
			= new DerObjectIdentifier(IsisMttObjectIdentifiers.IdIsisMttATNamingAuthorities + ".1");

		private readonly DerObjectIdentifier	namingAuthorityID;
		private readonly string					namingAuthorityUrl;
		private readonly DirectoryString		namingAuthorityText;

		public static NamingAuthority GetInstance(
			object obj)
		{
			if (obj == null || obj is NamingAuthority)
			{
				return (NamingAuthority) obj;
			}

			if (obj is Asn1Sequence)
			{
				return new NamingAuthority((Asn1Sequence) obj);
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		public static NamingAuthority GetInstance(
			Asn1TaggedObject	obj,
			bool				isExplicit)
		{
			return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
		}

		/**
		* Constructor from Asn1Sequence.
		* <p/>
		* <p/>
		* <pre>
		*             NamingAuthority ::= SEQUENCE
		*             {
		*               namingAuthorityID OBJECT IDENTIFIER OPTIONAL,
		*               namingAuthorityUrl IA5String OPTIONAL,
		*               namingAuthorityText DirectoryString(SIZE(1..128)) OPTIONAL
		*             }
		* </pre>
		*
		* @param seq The ASN.1 sequence.
		*/
		private NamingAuthority(
			Asn1Sequence seq)
		{
			if (seq.Count > 3)
				throw new ArgumentException("Bad sequence size: " + seq.Count);

			IEnumerator e = seq.GetEnumerator();

			if (e.MoveNext())
			{
				Asn1Encodable o = (Asn1Encodable) e.Current;
				if (o is DerObjectIdentifier)
				{
					namingAuthorityID = (DerObjectIdentifier) o;
				}
				else if (o is DerIA5String)
				{
					namingAuthorityUrl = DerIA5String.GetInstance(o).GetString();
				}
				else if (o is IAsn1String)
				{
					namingAuthorityText = DirectoryString.GetInstance(o);
				}
				else
				{
					throw new ArgumentException("Bad object encountered: " + o.GetType().Name);
				}
			}

			if (e.MoveNext())
			{
				Asn1Encodable o = (Asn1Encodable) e.Current;
				if (o is DerIA5String)
				{
					namingAuthorityUrl = DerIA5String.GetInstance(o).GetString();
				}
				else if (o is IAsn1String)
				{
					namingAuthorityText = DirectoryString.GetInstance(o);
				}
				else
				{
					throw new ArgumentException("Bad object encountered: " + o.GetType().Name);
				}
			}

			if (e.MoveNext())
			{
				Asn1Encodable o = (Asn1Encodable) e.Current;
				if (o is IAsn1String)
				{
					namingAuthorityText = DirectoryString.GetInstance(o);
				}
				else
				{
					throw new ArgumentException("Bad object encountered: " + o.GetType().Name);
				}
			}
		}

		/**
		* @return Returns the namingAuthorityID.
		*/
		public virtual DerObjectIdentifier NamingAuthorityID
		{
			get { return namingAuthorityID; }
		}

		/**
		* @return Returns the namingAuthorityText.
		*/
		public virtual DirectoryString NamingAuthorityText
		{
			get { return namingAuthorityText; }
		}

		/**
		* @return Returns the namingAuthorityUrl.
		*/
		public virtual string NamingAuthorityUrl
		{
			get { return namingAuthorityUrl; }
		}

		/**
		* Constructor from given details.
		* <p/>
		* All parameters can be combined.
		*
		* @param namingAuthorityID   ObjectIdentifier for naming authority.
		* @param namingAuthorityUrl  URL for naming authority.
		* @param namingAuthorityText Textual representation of naming authority.
		*/
		public NamingAuthority(
			DerObjectIdentifier	namingAuthorityID,
			string				namingAuthorityUrl,
			DirectoryString		namingAuthorityText)
		{
			this.namingAuthorityID = namingAuthorityID;
			this.namingAuthorityUrl = namingAuthorityUrl;
			this.namingAuthorityText = namingAuthorityText;
		}

		/**
		* Produce an object suitable for an Asn1OutputStream.
		* <p/>
		* Returns:
		* <p/>
		* <pre>
		*             NamingAuthority ::= SEQUENCE
		*             {
		*               namingAuthorityID OBJECT IDENTIFIER OPTIONAL,
		*               namingAuthorityUrl IA5String OPTIONAL,
		*               namingAuthorityText DirectoryString(SIZE(1..128)) OPTIONAL
		*             }
		* </pre>
		*
		* @return an Asn1Object
		*/
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector vec = new Asn1EncodableVector();
			if (namingAuthorityID != null)
			{
				vec.Add(namingAuthorityID);
			}
			if (namingAuthorityUrl != null)
			{
				vec.Add(new DerIA5String(namingAuthorityUrl, true));
			}
			if (namingAuthorityText != null)
			{
				vec.Add(namingAuthorityText);
			}
			return new DerSequence(vec);
		}
	}
}
