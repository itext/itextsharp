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
using System.IO;

namespace Org.BouncyCastle.Asn1
{
	public abstract class Asn1Encodable
		: IAsn1Convertible
    {
		public const string Der = "DER";
		public const string Ber = "BER";

		public byte[] GetEncoded()
        {
            MemoryStream bOut = new MemoryStream();
            Asn1OutputStream aOut = new Asn1OutputStream(bOut);

			aOut.WriteObject(this);

			return bOut.ToArray();
        }

		public byte[] GetEncoded(
			string encoding)
		{
			if (encoding.Equals(Der))
			{
				MemoryStream bOut = new MemoryStream();
				DerOutputStream dOut = new DerOutputStream(bOut);

				dOut.WriteObject(this);

				return bOut.ToArray();
			}

			return GetEncoded();
		}

		/**
		* Return the DER encoding of the object, null if the DER encoding can not be made.
		*
		* @return a DER byte array, null otherwise.
		*/
		public byte[] GetDerEncoded()
		{
			try
			{
				return GetEncoded(Der);
			}
			catch (IOException)
			{
				return null;
			}
		}

		public sealed override int GetHashCode()
		{
			return ToAsn1Object().CallAsn1GetHashCode();
		}

		public sealed override bool Equals(
			object obj)
		{
			if (obj == this)
				return true;

			IAsn1Convertible other = obj as IAsn1Convertible;

			if (other == null)
				return false;

			Asn1Object o1 = ToAsn1Object();
			Asn1Object o2 = other.ToAsn1Object();

			return o1 == o2 || o1.CallAsn1Equals(o2);
		}

		public abstract Asn1Object ToAsn1Object();
    }
}
