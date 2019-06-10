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
    public class DesParameters
		: KeyParameter
    {
        public DesParameters(
            byte[] key)
			: base(key)
        {
            if (IsWeakKey(key))
				throw new ArgumentException("attempt to create weak DES key");
        }

		public DesParameters(
			byte[]	key,
			int		keyOff,
			int		keyLen)
			: base(key, keyOff, keyLen)
		{
			if (IsWeakKey(key, keyOff))
				throw new ArgumentException("attempt to create weak DES key");
		}

		/*
        * DES Key Length in bytes.
        */
        public const int DesKeyLength = 8;

        /*
        * Table of weak and semi-weak keys taken from Schneier pp281
        */
        private const int N_DES_WEAK_KEYS = 16;

        private static readonly byte[] DES_weak_keys =
        {
            /* weak keys */
            (byte)0x01,(byte)0x01,(byte)0x01,(byte)0x01, (byte)0x01,(byte)0x01,(byte)0x01,(byte)0x01,
            (byte)0x1f,(byte)0x1f,(byte)0x1f,(byte)0x1f, (byte)0x0e,(byte)0x0e,(byte)0x0e,(byte)0x0e,
            (byte)0xe0,(byte)0xe0,(byte)0xe0,(byte)0xe0, (byte)0xf1,(byte)0xf1,(byte)0xf1,(byte)0xf1,
            (byte)0xfe,(byte)0xfe,(byte)0xfe,(byte)0xfe, (byte)0xfe,(byte)0xfe,(byte)0xfe,(byte)0xfe,

            /* semi-weak keys */
            (byte)0x01,(byte)0xfe,(byte)0x01,(byte)0xfe, (byte)0x01,(byte)0xfe,(byte)0x01,(byte)0xfe,
            (byte)0x1f,(byte)0xe0,(byte)0x1f,(byte)0xe0, (byte)0x0e,(byte)0xf1,(byte)0x0e,(byte)0xf1,
            (byte)0x01,(byte)0xe0,(byte)0x01,(byte)0xe0, (byte)0x01,(byte)0xf1,(byte)0x01,(byte)0xf1,
            (byte)0x1f,(byte)0xfe,(byte)0x1f,(byte)0xfe, (byte)0x0e,(byte)0xfe,(byte)0x0e,(byte)0xfe,
            (byte)0x01,(byte)0x1f,(byte)0x01,(byte)0x1f, (byte)0x01,(byte)0x0e,(byte)0x01,(byte)0x0e,
            (byte)0xe0,(byte)0xfe,(byte)0xe0,(byte)0xfe, (byte)0xf1,(byte)0xfe,(byte)0xf1,(byte)0xfe,
            (byte)0xfe,(byte)0x01,(byte)0xfe,(byte)0x01, (byte)0xfe,(byte)0x01,(byte)0xfe,(byte)0x01,
            (byte)0xe0,(byte)0x1f,(byte)0xe0,(byte)0x1f, (byte)0xf1,(byte)0x0e,(byte)0xf1,(byte)0x0e,
            (byte)0xe0,(byte)0x01,(byte)0xe0,(byte)0x01, (byte)0xf1,(byte)0x01,(byte)0xf1,(byte)0x01,
            (byte)0xfe,(byte)0x1f,(byte)0xfe,(byte)0x1f, (byte)0xfe,(byte)0x0e,(byte)0xfe,(byte)0x0e,
            (byte)0x1f,(byte)0x01,(byte)0x1f,(byte)0x01, (byte)0x0e,(byte)0x01,(byte)0x0e,(byte)0x01,
            (byte)0xfe,(byte)0xe0,(byte)0xfe,(byte)0xe0, (byte)0xfe,(byte)0xf1,(byte)0xfe,(byte)0xf1
        };

        /**
        * DES has 16 weak keys.  This method will check
        * if the given DES key material is weak or semi-weak.
        * Key material that is too short is regarded as weak.
        * <p>
        * See <a href="http://www.counterpane.com/applied.html">"Applied
        * Cryptography"</a> by Bruce Schneier for more information.
        * </p>
        * @return true if the given DES key material is weak or semi-weak,
        *     false otherwise.
        */
        public static bool IsWeakKey(
            byte[]	key,
            int		offset)
        {
            if (key.Length - offset < DesKeyLength)
                throw new ArgumentException("key material too short.");

			//nextkey:
            for (int i = 0; i < N_DES_WEAK_KEYS; i++)
            {
                bool unmatch = false;
                for (int j = 0; j < DesKeyLength; j++)
                {
                    if (key[j + offset] != DES_weak_keys[i * DesKeyLength + j])
                    {
                        //continue nextkey;
                        unmatch = true;
						break;
                    }
                }

				if (!unmatch)
				{
					return true;
				}
            }

			return false;
        }

		public static bool IsWeakKey(
			byte[] key)
		{
			return IsWeakKey(key, 0);
		}

		/**
        * DES Keys use the LSB as the odd parity bit.  This can
        * be used to check for corrupt keys.
        *
        * @param bytes the byte array to set the parity on.
        */
        public static void SetOddParity(
            byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                bytes[i] = (byte)((b & 0xfe) |
                                ((((b >> 1) ^
                                (b >> 2) ^
                                (b >> 3) ^
                                (b >> 4) ^
                                (b >> 5) ^
                                (b >> 6) ^
                                (b >> 7)) ^ 0x01) & 0x01));
            }
        }
    }

}
