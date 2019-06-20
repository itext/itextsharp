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
using System.Collections;
using System.IO;

namespace Org.BouncyCastle.Asn1
{
	public class DerSequence
		: Asn1Sequence
	{
		public static readonly DerSequence Empty = new DerSequence();

		public static DerSequence FromVector(
			Asn1EncodableVector v)
		{
			return v.Count < 1 ? Empty : new DerSequence(v);
		}

		/**
		 * create an empty sequence
		 */
		public DerSequence()
			: base(0)
		{
		}

		/**
		 * create a sequence containing one object
		 */
		public DerSequence(
			Asn1Encodable obj)
			: base(1)
		{
			AddObject(obj);
		}

		public DerSequence(
			params Asn1Encodable[] v)
			: base(v.Length)
		{
			foreach (Asn1Encodable ae in v)
			{
				AddObject(ae);
			}
		}

		/**
		 * create a sequence containing a vector of objects.
		 */
		public DerSequence(
			Asn1EncodableVector v)
			: base(v.Count)
		{
			foreach (Asn1Encodable ae in v)
			{
				AddObject(ae);
			}
		}

		/*
		 * A note on the implementation:
		 * <p>
		 * As Der requires the constructed, definite-length model to
		 * be used for structured types, this varies slightly from the
		 * ASN.1 descriptions given. Rather than just outputing Sequence,
		 * we also have to specify Constructed, and the objects length.
		 */
		internal override void Encode(
			DerOutputStream derOut)
		{
			// TODO Intermediate buffer could be avoided if we could calculate expected length
			MemoryStream bOut = new MemoryStream();
			DerOutputStream dOut = new DerOutputStream(bOut);

			foreach (Asn1Encodable obj in this)
			{
				dOut.WriteObject(obj);
			}

			dOut.Close();

			byte[] bytes = bOut.ToArray();

			derOut.WriteEncoded(Asn1Tags.Sequence | Asn1Tags.Constructed, bytes);
		}
	}
}
