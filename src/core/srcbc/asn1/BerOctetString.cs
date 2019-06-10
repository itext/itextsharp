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
using System.IO;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1
{
    public class BerOctetString
        : DerOctetString, IEnumerable
    {
		public static BerOctetString FromSequence(Asn1Sequence seq)
		{
			IList v = Platform.CreateArrayList();

			foreach (Asn1Encodable obj in seq)
			{
				v.Add(obj);
			}

			return new BerOctetString(v);
		}

		private const int MaxLength = 1000;

		/**
         * convert a vector of octet strings into a single byte string
         */
        private static byte[] ToBytes(
            IEnumerable octs)
        {
            MemoryStream bOut = new MemoryStream();
			foreach (DerOctetString o in octs)
			{
                byte[] octets = o.GetOctets();
                bOut.Write(octets, 0, octets.Length);
            }
			return bOut.ToArray();
        }

		private readonly IEnumerable octs;

		/// <param name="str">The octets making up the octet string.</param>
		public BerOctetString(
			byte[] str)
			: base(str)
		{
		}

		public BerOctetString(
			IEnumerable octets)
			: base(ToBytes(octets))
        {
            this.octs = octets;
        }

        public BerOctetString(
			Asn1Object obj)
			: base(obj)
        {
        }

        public BerOctetString(
			Asn1Encodable obj)
			: base(obj.ToAsn1Object())
        {
        }

        public override byte[] GetOctets()
        {
            return str;
        }

        /**
         * return the DER octets that make up this string.
         */
		public IEnumerator GetEnumerator()
		{
			if (octs == null)
			{
				return GenerateOcts().GetEnumerator();
			}

			return octs.GetEnumerator();
		}

		[Obsolete("Use GetEnumerator() instead")]
        public IEnumerator GetObjects()
        {
			return GetEnumerator();
		}

		private IList GenerateOcts()
        {
            IList vec = Platform.CreateArrayList();
			for (int i = 0; i < str.Length; i += MaxLength)
			{
				int end = System.Math.Min(str.Length, i + MaxLength);

				byte[] nStr = new byte[end - i];

				Array.Copy(str, i, nStr, 0, nStr.Length);

				vec.Add(new DerOctetString(nStr));
			}
			return vec;
        }

        internal override void Encode(
            DerOutputStream derOut)
        {
            if (derOut is Asn1OutputStream || derOut is BerOutputStream)
            {
                derOut.WriteByte(Asn1Tags.Constructed | Asn1Tags.OctetString);

                derOut.WriteByte(0x80);

                //
                // write out the octet array
                //
                foreach (DerOctetString oct in this)
                {
                    derOut.WriteObject(oct);
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
