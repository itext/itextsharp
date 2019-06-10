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
using System.Text;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1
{
    /**
     * Der IA5String object - this is an ascii string.
     */
    public class DerIA5String
        : DerStringBase
    {
        private readonly string str;

        /**
         * return a IA5 string from the passed in object
         *
         * @exception ArgumentException if the object cannot be converted.
         */
        public static DerIA5String GetInstance(
            object obj)
        {
            if (obj == null || obj is DerIA5String)
            {
                return (DerIA5String)obj;
            }

            throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name);
        }

        /**
         * return an IA5 string from a tagged object.
         *
         * @param obj the tagged object holding the object we want
         * @param explicitly true if the object is meant to be explicitly
         *              tagged false otherwise.
         * @exception ArgumentException if the tagged object cannot
         *               be converted.
         */
        public static DerIA5String GetInstance(
            Asn1TaggedObject	obj,
            bool				isExplicit)
        {
			Asn1Object o = obj.GetObject();

			if (isExplicit || o is DerIA5String)
			{
				return GetInstance(o);
			}

			return new DerIA5String(((Asn1OctetString)o).GetOctets());
        }

        /**
         * basic constructor - with bytes.
         */
        public DerIA5String(
            byte[] str)
            : this(Strings.FromAsciiByteArray(str), false)
        {
        }

		/**
		* basic constructor - without validation.
		*/
		public DerIA5String(
			string str)
			: this(str, false)
		{
		}

		/**
		* Constructor with optional validation.
		*
		* @param string the base string to wrap.
		* @param validate whether or not to check the string.
		* @throws ArgumentException if validate is true and the string
		* contains characters that should not be in an IA5String.
		*/
		public DerIA5String(
			string	str,
			bool	validate)
		{
			if (str == null)
				throw new ArgumentNullException("str");
			if (validate && !IsIA5String(str))
				throw new ArgumentException("string contains illegal characters", "str");

			this.str = str;
		}

		public override string GetString()
        {
            return str;
        }

		public byte[] GetOctets()
        {
            return Strings.ToAsciiByteArray(str);
        }

		internal override void Encode(
            DerOutputStream derOut)
        {
            derOut.WriteEncoded(Asn1Tags.IA5String, GetOctets());
        }

		protected override int Asn1GetHashCode()
		{
            return this.str.GetHashCode();
        }

		protected override bool Asn1Equals(
            Asn1Object asn1Object)
        {
			DerIA5String other = asn1Object as DerIA5String;

			if (other == null)
				return false;

			return this.str.Equals(other.str);
        }

		/**
		 * return true if the passed in String can be represented without
		 * loss as an IA5String, false otherwise.
		 *
		 * @return true if in printable set, false otherwise.
		 */
		public static bool IsIA5String(
			string str)
		{
			foreach (char ch in str)
			{
				if (ch > 0x007f)
				{
					return false;
				}
			}

			return true;
		}
	}
}
