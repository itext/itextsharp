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
using System.IO;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1
{
	/**
	* Class representing the DER-type External
	*/
	public class DerExternal
		: Asn1Object
	{
		private DerObjectIdentifier	directReference;
		private DerInteger			indirectReference;
		private Asn1Object			dataValueDescriptor;
		private int					encoding;
		private Asn1Object			externalContent;

		public DerExternal(
			Asn1EncodableVector vector)
		{
			int offset = 0;
			Asn1Object enc = GetObjFromVector(vector, offset);
			if (enc is DerObjectIdentifier)
			{
				directReference = (DerObjectIdentifier)enc;
				offset++;
				enc = GetObjFromVector(vector, offset);
			}
			if (enc is DerInteger)
			{
				indirectReference = (DerInteger) enc;
				offset++;
				enc = GetObjFromVector(vector, offset);
			}
			if (!(enc is DerTaggedObject))
			{
				dataValueDescriptor = (Asn1Object) enc;
				offset++;
				enc = GetObjFromVector(vector, offset);
			}
			if (!(enc is DerTaggedObject))
			{
				throw new InvalidOperationException(
					"No tagged object found in vector. Structure doesn't seem to be of type External");
			}

			if (vector.Count != offset + 1)
				throw new ArgumentException("input vector too large", "vector");

			if (!(enc is DerTaggedObject))
				throw new ArgumentException("No tagged object found in vector. Structure doesn't seem to be of type External", "vector");

			DerTaggedObject obj = (DerTaggedObject)enc;

			// Use property accessor to include check on value
			Encoding = obj.TagNo;

			if (encoding < 0 || encoding > 2)
				throw new InvalidOperationException("invalid encoding value");

			externalContent = obj.GetObject();
		}

		/**
		* Creates a new instance of DerExternal
		* See X.690 for more informations about the meaning of these parameters
		* @param directReference The direct reference or <code>null</code> if not set.
		* @param indirectReference The indirect reference or <code>null</code> if not set.
		* @param dataValueDescriptor The data value descriptor or <code>null</code> if not set.
		* @param externalData The external data in its encoded form.
		*/
		public DerExternal(DerObjectIdentifier directReference, DerInteger indirectReference, Asn1Object dataValueDescriptor, DerTaggedObject externalData)
			: this(directReference, indirectReference, dataValueDescriptor, externalData.TagNo, externalData.ToAsn1Object())
		{
		}

		/**
		* Creates a new instance of DerExternal.
		* See X.690 for more informations about the meaning of these parameters
		* @param directReference The direct reference or <code>null</code> if not set.
		* @param indirectReference The indirect reference or <code>null</code> if not set.
		* @param dataValueDescriptor The data value descriptor or <code>null</code> if not set.
		* @param encoding The encoding to be used for the external data
		* @param externalData The external data
		*/
		public DerExternal(DerObjectIdentifier directReference, DerInteger indirectReference, Asn1Object dataValueDescriptor, int encoding, Asn1Object externalData)
		{
			DirectReference = directReference;
			IndirectReference = indirectReference;
			DataValueDescriptor = dataValueDescriptor;
			Encoding = encoding;
			ExternalContent = externalData.ToAsn1Object();
		}

		internal override void Encode(DerOutputStream derOut)
		{
			MemoryStream ms = new MemoryStream();
			WriteEncodable(ms, directReference);
			WriteEncodable(ms, indirectReference);
			WriteEncodable(ms, dataValueDescriptor);
			WriteEncodable(ms, new DerTaggedObject(Asn1Tags.External, externalContent));

			derOut.WriteEncoded(Asn1Tags.Constructed, Asn1Tags.External, ms.ToArray());
		}

		protected override int Asn1GetHashCode()
		{
			int ret = externalContent.GetHashCode();
			if (directReference != null)
			{
				ret ^= directReference.GetHashCode();
			}
			if (indirectReference != null)
			{
				ret ^= indirectReference.GetHashCode();
			}
			if (dataValueDescriptor != null)
			{
				ret ^= dataValueDescriptor.GetHashCode();
			}
			return ret;
		}

		protected override bool Asn1Equals(
			Asn1Object asn1Object)
		{
			if (this == asn1Object)
				return true;

			DerExternal other = asn1Object as DerExternal;

			if (other == null)
				return false;

			return Platform.Equals(directReference, other.directReference)
				&& Platform.Equals(indirectReference, other.indirectReference)
				&& Platform.Equals(dataValueDescriptor, other.dataValueDescriptor)
				&& externalContent.Equals(other.externalContent);
		}

		public Asn1Object DataValueDescriptor
		{
			get { return dataValueDescriptor; }
			set { this.dataValueDescriptor = value; }
		}

		public DerObjectIdentifier DirectReference
		{
			get { return directReference; }
			set { this.directReference = value; }
		}

		/**
		* The encoding of the content. Valid values are
		* <ul>
		* <li><code>0</code> single-ASN1-type</li>
		* <li><code>1</code> OCTET STRING</li>
		* <li><code>2</code> BIT STRING</li>
		* </ul>
		*/
		public int Encoding
		{
			get
			{
				return encoding;
			}
			set
			{
				if (encoding < 0 || encoding > 2)
					throw new InvalidOperationException("invalid encoding value: " + encoding);

				this.encoding = value;
			}
		}

		public Asn1Object ExternalContent
		{
			get { return externalContent; }
			set { this.externalContent = value; }
		}

		public DerInteger IndirectReference
		{
			get { return indirectReference; }
			set { this.indirectReference = value; }
		}

		private static Asn1Object GetObjFromVector(Asn1EncodableVector v, int index)
		{
			if (v.Count <= index)
				throw new ArgumentException("too few objects in input vector", "v");

			return v[index].ToAsn1Object();
		}

		private static void WriteEncodable(MemoryStream ms, Asn1Encodable e)
		{
			if (e != null)
			{
				byte[] bs = e.GetDerEncoded();
				ms.Write(bs, 0, bs.Length);
			}
		}
	}
}
