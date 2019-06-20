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

namespace Org.BouncyCastle.Crypto.Parameters
{
    public class DesEdeParameters
		: DesParameters
    {
        /*
        * DES-EDE Key length in bytes.
        */
		public const int DesEdeKeyLength = 24;

		private static byte[] FixKey(
			byte[]	key,
			int		keyOff,
			int		keyLen)
		{
			byte[] tmp = new byte[24];

			switch (keyLen)
			{
				case 16:
					Array.Copy(key, keyOff, tmp, 0, 16);
					Array.Copy(key, keyOff, tmp, 16, 8);
					break;
				case 24:
					Array.Copy(key, keyOff, tmp, 0, 24);
					break;
				default:
					throw new ArgumentException("Bad length for DESede key: " + keyLen, "keyLen");
			}

			if (IsWeakKey(tmp))
				throw new ArgumentException("attempt to create weak DESede key");

			return tmp;
		}

		public DesEdeParameters(
            byte[] key)
			: base(FixKey(key, 0, key.Length))
        {
        }

		public DesEdeParameters(
			byte[]	key,
			int		keyOff,
			int		keyLen)
			: base(FixKey(key, keyOff, keyLen))
		{
		}

		/**
         * return true if the passed in key is a DES-EDE weak key.
         *
         * @param key bytes making up the key
         * @param offset offset into the byte array the key starts at
         * @param length number of bytes making up the key
         */
        public static bool IsWeakKey(
            byte[]  key,
            int     offset,
            int     length)
        {
            for (int i = offset; i < length; i += DesKeyLength)
            {
                if (DesParameters.IsWeakKey(key, i))
                {
                    return true;
                }
            }

            return false;
        }

        /**
         * return true if the passed in key is a DES-EDE weak key.
         *
         * @param key bytes making up the key
         * @param offset offset into the byte array the key starts at
         */
        public static new bool IsWeakKey(
            byte[]	key,
            int		offset)
        {
            return IsWeakKey(key, offset, key.Length - offset);
        }

		public static new bool IsWeakKey(
			byte[] key)
		{
			return IsWeakKey(key, 0, key.Length);
		}
    }
}
