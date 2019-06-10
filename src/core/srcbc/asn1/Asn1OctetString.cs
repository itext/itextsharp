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
using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Asn1
{
    public abstract class Asn1OctetString
        : Asn1Object, Asn1OctetStringParser
    {
        internal byte[] str;

		/**
         * return an Octet string from a tagged object.
         *
         * @param obj the tagged object holding the object we want.
         * @param explicitly true if the object is meant to be explicitly
         *              tagged false otherwise.
         * @exception ArgumentException if the tagged object cannot
         *              be converted.
         */
		public static Asn1OctetString GetInstance(
			Asn1TaggedObject	obj,
			bool				isExplicit)
		{
			Asn1Object o = obj.GetObject();

			if (isExplicit || o is Asn1OctetString)
			{
				return GetInstance(o);
			}

			return BerOctetString.FromSequence(Asn1Sequence.GetInstance(o));
		}

        /**
         * return an Octet string from the given object.
         *
         * @param obj the object we want converted.
         * @exception ArgumentException if the object cannot be converted.
         */
		public static Asn1OctetString GetInstance(object obj)
		{
			if (obj == null || obj is Asn1OctetString)
			{
				return (Asn1OctetString)obj;
			}

			// TODO: this needs to be deleted in V2
			if (obj is Asn1TaggedObject)
				return GetInstance(((Asn1TaggedObject)obj).GetObject());

			throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name);
		}

        /**
         * @param string the octets making up the octet string.
         */
        internal Asn1OctetString(
            byte[] str)
        {
			if (str == null)
				throw new ArgumentNullException("str");

			this.str = str;
        }

        internal Asn1OctetString(
            Asn1Encodable obj)
        {
            try
            {
				this.str = obj.GetEncoded(Asn1Encodable.Der);
            }
            catch (IOException e)
            {
                throw new ArgumentException("Error processing object : " + e.ToString());
            }
        }

		public Stream GetOctetStream()
		{
			return new MemoryStream(str, false);
		}

		public Asn1OctetStringParser Parser
		{
			get { return this; }
		}

		public virtual byte[] GetOctets()
        {
            return str;
        }

		protected override int Asn1GetHashCode()
		{
			return Arrays.GetHashCode(GetOctets());
        }

		protected override bool Asn1Equals(
			Asn1Object asn1Object)
		{
			DerOctetString other = asn1Object as DerOctetString;

			if (other == null)
				return false;

			return Arrays.AreEqual(GetOctets(), other.GetOctets());
		}

		public override string ToString()
		{
			return "#" + Hex.ToHexString(str);
		}
	}
}
