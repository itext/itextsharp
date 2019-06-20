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

using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1
{
    public class DerEnumerated
        : Asn1Object
    {
        private readonly byte[] bytes;

		/**
         * return an integer from the passed in object
         *
         * @exception ArgumentException if the object cannot be converted.
         */
        public static DerEnumerated GetInstance(
            object obj)
        {
            if (obj == null || obj is DerEnumerated)
            {
                return (DerEnumerated)obj;
            }

            throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name);
        }

        /**
         * return an Enumerated from a tagged object.
         *
         * @param obj the tagged object holding the object we want
         * @param explicitly true if the object is meant to be explicitly
         *              tagged false otherwise.
         * @exception ArgumentException if the tagged object cannot
         *               be converted.
         */
        public static DerEnumerated GetInstance(
            Asn1TaggedObject	obj,
            bool				isExplicit)
        {
			Asn1Object o = obj.GetObject();

			if (isExplicit || o is DerEnumerated)
			{
				return GetInstance(o);
			}

			return new DerEnumerated(((Asn1OctetString)o).GetOctets());
        }

        public DerEnumerated(
            int val)
        {
            bytes = BigInteger.ValueOf(val).ToByteArray();
        }

        public DerEnumerated(
            BigInteger val)
        {
            bytes = val.ToByteArray();
        }

        public DerEnumerated(
            byte[]   bytes)
        {
            this.bytes = bytes;
        }

        public BigInteger Value
        {
            get
            {
                return new BigInteger(bytes);
            }
        }

		internal override void Encode(
            DerOutputStream derOut)
        {
            derOut.WriteEncoded(Asn1Tags.Enumerated, bytes);
        }

		protected override bool Asn1Equals(
			Asn1Object asn1Object)
        {
			DerEnumerated other = asn1Object as DerEnumerated;

			if (other == null)
				return false;

			return Arrays.AreEqual(this.bytes, other.bytes);
        }

		protected override int Asn1GetHashCode()
		{
			return Arrays.GetHashCode(bytes);
        }
    }
}
