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
using System.Globalization;
using System.IO;
using System.Text;

using Org.BouncyCastle.Utilities.Encoders;

namespace Org.BouncyCastle.Asn1.X509
{
    /**
     * It turns out that the number of standard ways the fields in a DN should be
     * encoded into their ASN.1 counterparts is rapidly approaching the
     * number of machines on the internet. By default the X509Name class
     * will produce UTF8Strings in line with the current recommendations (RFC 3280).
     * <p>
     * An example of an encoder look like below:
     * <pre>
     * public class X509DirEntryConverter
     *     : X509NameEntryConverter
     * {
     *     public Asn1Object GetConvertedValue(
     *         DerObjectIdentifier  oid,
     *         string               value)
     *     {
     *         if (str.Length() != 0 &amp;&amp; str.charAt(0) == '#')
     *         {
     *             return ConvertHexEncoded(str, 1);
     *         }
     *         if (oid.Equals(EmailAddress))
     *         {
     *             return new DerIA5String(str);
     *         }
     *         else if (CanBePrintable(str))
     *         {
     *             return new DerPrintableString(str);
     *         }
     *         else if (CanBeUTF8(str))
     *         {
     *             return new DerUtf8String(str);
     *         }
     *         else
     *         {
     *             return new DerBmpString(str);
     *         }
     *     }
     * }
	 * </pre>
	 * </p>
     */
    public abstract class X509NameEntryConverter
    {
        /**
         * Convert an inline encoded hex string rendition of an ASN.1
         * object back into its corresponding ASN.1 object.
         *
         * @param str the hex encoded object
         * @param off the index at which the encoding starts
         * @return the decoded object
         */
        protected Asn1Object ConvertHexEncoded(
            string	hexString,
            int		offset)
        {
			string str = hexString.Substring(offset);

			return Asn1Object.FromByteArray(Hex.Decode(str));
        }

		/**
         * return true if the passed in string can be represented without
         * loss as a PrintableString, false otherwise.
         */
        protected bool CanBePrintable(
            string str)
        {
			return DerPrintableString.IsPrintableString(str);
        }

		/**
         * Convert the passed in string value into the appropriate ASN.1
         * encoded object.
         *
         * @param oid the oid associated with the value in the DN.
         * @param value the value of the particular DN component.
         * @return the ASN.1 equivalent for the value.
         */
        public abstract Asn1Object GetConvertedValue(DerObjectIdentifier oid, string value);
    }
}
