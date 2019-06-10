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

namespace Org.BouncyCastle.Crypto
{
    /**
     * super class for all Password Based Encyrption (Pbe) parameter generator classes.
     */
    public abstract class PbeParametersGenerator
    {
        protected byte[]	mPassword;
        protected byte[]	mSalt;
        protected int		mIterationCount;

        /**
         * base constructor.
         */
        protected PbeParametersGenerator()
        {
        }

        /**
         * initialise the Pbe generator.
         *
         * @param password the password converted into bytes (see below).
         * @param salt the salt to be mixed with the password.
         * @param iterationCount the number of iterations the "mixing" function
         * is to be applied for.
         */
        public virtual void Init(
            byte[]  password,
            byte[]  salt,
            int     iterationCount)
        {
            if (password == null)
                throw new ArgumentNullException("password");
            if (salt == null)
                throw new ArgumentNullException("salt");

            this.mPassword = Arrays.Clone(password);
            this.mSalt = Arrays.Clone(salt);
            this.mIterationCount = iterationCount;
        }

        public virtual byte[] Password
        {
            get { return Arrays.Clone(mPassword); }
        }

        /**
         * return the password byte array.
         *
         * @return the password byte array.
         */
        [Obsolete("Use 'Password' property")]
        public byte[] GetPassword()
        {
            return Password;
        }

        public virtual byte[] Salt
        {
            get { return Arrays.Clone(mSalt); }
        }

        /**
         * return the salt byte array.
         *
         * @return the salt byte array.
         */
        [Obsolete("Use 'Salt' property")]
        public byte[] GetSalt()
        {
            return Salt;
        }

        /**
         * return the iteration count.
         *
         * @return the iteration count.
         */
        public virtual int IterationCount
        {
            get { return mIterationCount; }
        }

        /**
         * Generate derived parameters for a key of length keySize.
         *
         * @param keySize the length, in bits, of the key required.
         * @return a parameters object representing a key.
         */
        [Obsolete("Use version with 'algorithm' parameter")]
        public abstract ICipherParameters GenerateDerivedParameters(int keySize);
        public abstract ICipherParameters GenerateDerivedParameters(string algorithm, int keySize);

        /**
         * Generate derived parameters for a key of length keySize, and
         * an initialisation vector (IV) of length ivSize.
         *
         * @param keySize the length, in bits, of the key required.
         * @param ivSize the length, in bits, of the iv required.
         * @return a parameters object representing a key and an IV.
         */
        [Obsolete("Use version with 'algorithm' parameter")]
        public abstract ICipherParameters GenerateDerivedParameters(int keySize, int ivSize);
        public abstract ICipherParameters GenerateDerivedParameters(string algorithm, int keySize, int ivSize);

        /**
         * Generate derived parameters for a key of length keySize, specifically
         * for use with a MAC.
         *
         * @param keySize the length, in bits, of the key required.
         * @return a parameters object representing a key.
         */
        public abstract ICipherParameters GenerateDerivedMacParameters(int keySize);

        /**
         * converts a password to a byte array according to the scheme in
         * Pkcs5 (ascii, no padding)
         *
         * @param password a character array representing the password.
         * @return a byte array representing the password.
         */
        public static byte[] Pkcs5PasswordToBytes(
            char[] password)
        {
            if (password == null)
                return new byte[0];

            return Strings.ToAsciiByteArray(password);
        }

        [Obsolete("Use version taking 'char[]' instead")]
        public static byte[] Pkcs5PasswordToBytes(
            string password)
        {
            if (password == null)
                return new byte[0];

            return Strings.ToAsciiByteArray(password);
        }

        /**
         * converts a password to a byte array according to the scheme in
         * PKCS5 (UTF-8, no padding)
         *
         * @param password a character array representing the password.
         * @return a byte array representing the password.
         */
        public static byte[] Pkcs5PasswordToUtf8Bytes(
            char[] password)
        {
            if (password == null)
                return new byte[0];

            return Encoding.UTF8.GetBytes(password);
        }

        [Obsolete("Use version taking 'char[]' instead")]
        public static byte[] Pkcs5PasswordToUtf8Bytes(
            string password)
        {
            if (password == null)
                return new byte[0];

            return Encoding.UTF8.GetBytes(password);
        }

        /**
         * converts a password to a byte array according to the scheme in
         * Pkcs12 (unicode, big endian, 2 zero pad bytes at the end).
         *
         * @param password a character array representing the password.
         * @return a byte array representing the password.
         */
        public static byte[] Pkcs12PasswordToBytes(
            char[] password)
        {
            return Pkcs12PasswordToBytes(password, false);
        }

        public static byte[] Pkcs12PasswordToBytes(
            char[]	password,
            bool	wrongPkcs12Zero)
        {
            if (password == null || password.Length < 1)
            {
                return new byte[wrongPkcs12Zero ? 2 : 0];
            }

            // +1 for extra 2 pad bytes.
            byte[] bytes = new byte[(password.Length + 1) * 2];

            Encoding.BigEndianUnicode.GetBytes(password, 0, password.Length, bytes, 0);

            return bytes;
        }
    }
}
