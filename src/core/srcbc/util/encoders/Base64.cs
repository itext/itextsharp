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
using System.Text;

namespace Org.BouncyCastle.Utilities.Encoders
{
    public sealed class Base64
    {
        private Base64()
        {
        }

        public static string ToBase64String(
            byte[] data)
        {
            return Convert.ToBase64String(data, 0, data.Length);
        }

        public static string ToBase64String(
            byte[] data,
            int off,
            int length)
        {
            return Convert.ToBase64String(data, off, length);
        }

        /**
         * encode the input data producing a base 64 encoded byte array.
         *
         * @return a byte array containing the base 64 encoded data.
         */
        public static byte[] Encode(
            byte[] data)
        {
            return Encode(data, 0, data.Length);
        }

        /**
         * encode the input data producing a base 64 encoded byte array.
         *
         * @return a byte array containing the base 64 encoded data.
         */
        public static byte[] Encode(
            byte[] data,
            int off,
            int length)
        {
            string s = Convert.ToBase64String(data, off, length);
            return Strings.ToAsciiByteArray(s);
        }

        /**
         * Encode the byte data to base 64 writing it to the given output stream.
         *
         * @return the number of bytes produced.
         */
        public static int Encode(
            byte[]	data,
            Stream	outStream)
        {
            byte[] encoded = Encode(data);
            outStream.Write(encoded, 0, encoded.Length);
            return encoded.Length;
        }

        /**
         * Encode the byte data to base 64 writing it to the given output stream.
         *
         * @return the number of bytes produced.
         */
        public static int Encode(
            byte[]	data,
            int		off,
            int		length,
            Stream	outStream)
        {
            byte[] encoded = Encode(data, off, length);
            outStream.Write(encoded, 0, encoded.Length);
            return encoded.Length;
        }

        /**
         * decode the base 64 encoded input data. It is assumed the input data is valid.
         *
         * @return a byte array representing the decoded data.
         */
        public static byte[] Decode(
            byte[] data)
        {
            string s = Strings.FromAsciiByteArray(data);
            return Convert.FromBase64String(s);
        }

        /**
         * decode the base 64 encoded string data - whitespace will be ignored.
         *
         * @return a byte array representing the decoded data.
         */
        public static byte[] Decode(
            string data)
        {
            return Convert.FromBase64String(data);
        }

        /**
         * decode the base 64 encoded string data writing it to the given output stream,
         * whitespace characters will be ignored.
         *
         * @return the number of bytes produced.
         */
        public static int Decode(
            string	data,
            Stream	outStream)
        {
            byte[] decoded = Decode(data);
            outStream.Write(decoded, 0, decoded.Length);
            return decoded.Length;
        }
    }
}
