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

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1
{
	/**
	 * BER TaggedObject - in ASN.1 notation this is any object preceded by
	 * a [n] where n is some number - these are assumed to follow the construction
	 * rules (as with sequences).
	 */
	public class BerTaggedObject
		: DerTaggedObject
	{
		/**
		 * @param tagNo the tag number for this object.
		 * @param obj the tagged object.
		 */
		public BerTaggedObject(
			int				tagNo,
			Asn1Encodable	obj)
			: base(tagNo, obj)
		{
		}

		/**
		 * @param explicitly true if an explicitly tagged object.
		 * @param tagNo the tag number for this object.
		 * @param obj the tagged object.
		 */
		public BerTaggedObject(
			bool			explicitly,
			int				tagNo,
			Asn1Encodable	obj)
			: base(explicitly, tagNo, obj)
		{
		}

		/**
		 * create an implicitly tagged object that contains a zero
		 * length sequence.
		 */
		public BerTaggedObject(
			int tagNo)
			: base(false, tagNo, BerSequence.Empty)
		{
		}

		internal override void Encode(
			DerOutputStream derOut)
		{
			if (derOut is Asn1OutputStream || derOut is BerOutputStream)
			{
				derOut.WriteTag((byte)(Asn1Tags.Constructed | Asn1Tags.Tagged), tagNo);
				derOut.WriteByte(0x80);

				if (!IsEmpty())
				{
					if (!explicitly)
					{
						IEnumerable eObj;
						if (obj is Asn1OctetString)
						{
							if (obj is BerOctetString)
							{
								eObj = (BerOctetString) obj;
							}
							else
							{
								Asn1OctetString octs = (Asn1OctetString)obj;
								eObj = new BerOctetString(octs.GetOctets());
							}
						}
						else if (obj is Asn1Sequence)
						{
							eObj = (Asn1Sequence) obj;
						}
						else if (obj is Asn1Set)
						{
							eObj = (Asn1Set) obj;
						}
						else
						{
							throw Platform.CreateNotImplementedException(obj.GetType().Name);
						}

						foreach (Asn1Encodable o in eObj)
						{
							derOut.WriteObject(o);
						}
					}
					else
					{
						derOut.WriteObject(obj);
					}
				}

				derOut.WriteByte(0x00);
				derOut.WriteByte(0x00);
			}
			else
			{
				base.Encode(derOut);
			}
		}
	}
}
