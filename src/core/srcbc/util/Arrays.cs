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

namespace Org.BouncyCastle.Utilities
{

    /// <summary> General array utilities.</summary>
    public sealed class Arrays
    {
        private Arrays()
        {
        }

		public static bool AreEqual(
			bool[]  a,
			bool[]  b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

            return HaveSameContents(a, b);
		}

        public static bool AreEqual(
            char[] a,
            char[] b)
        {
            if (a == b)
                return true;

            if (a == null || b == null)
                return false;

            return HaveSameContents(a, b);
        }

        /// <summary>
        /// Are two arrays equal.
        /// </summary>
        /// <param name="a">Left side.</param>
        /// <param name="b">Right side.</param>
        /// <returns>True if equal.</returns>
        public static bool AreEqual(
			byte[]	a,
			byte[]	b)
        {
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			return HaveSameContents(a, b);
		}

		[Obsolete("Use 'AreEqual' method instead")]
		public static bool AreSame(
			byte[]	a,
			byte[]	b)
		{
			return AreEqual(a, b);
		}

		/// <summary>
		/// A constant time equals comparison - does not terminate early if
		/// test will fail.
		/// </summary>
		/// <param name="a">first array</param>
		/// <param name="b">second array</param>
		/// <returns>true if arrays equal, false otherwise.</returns>
		public static bool ConstantTimeAreEqual(
			byte[]	a,
			byte[]	b)
		{
			int i = a.Length;
			if (i != b.Length)
				return false;
			int cmp = 0;
			while (i != 0)
			{
				--i;
				cmp |= (a[i] ^ b[i]);
			}
			return cmp == 0;
		}

		public static bool AreEqual(
			int[]	a,
			int[]	b)
		{
			if (a == b)
				return true;

			if (a == null || b == null)
				return false;

			return HaveSameContents(a, b);
		}

        private static bool HaveSameContents(
            bool[] a,
            bool[] b)
        {
            int i = a.Length;
            if (i != b.Length)
                return false;
            while (i != 0)
            {
                --i;
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }

        private static bool HaveSameContents(
            char[] a,
            char[] b)
        {
            int i = a.Length;
            if (i != b.Length)
                return false;
            while (i != 0)
            {
                --i;
                if (a[i] != b[i])
                    return false;
            }
            return true;
        }

        private static bool HaveSameContents(
			byte[]	a,
			byte[]	b)
		{
			int i = a.Length;
			if (i != b.Length)
				return false;
			while (i != 0)
			{
				--i;
				if (a[i] != b[i])
					return false;
			}
			return true;
		}

		private static bool HaveSameContents(
			int[]	a,
			int[]	b)
		{
			int i = a.Length;
			if (i != b.Length)
				return false;
			while (i != 0)
			{
				--i;
				if (a[i] != b[i])
					return false;
			}
			return true;
		}

        public static string ToString(
			object[] a)
		{
			StringBuilder sb = new StringBuilder('[');
			if (a.Length > 0)
			{
				sb.Append(a[0]);
				for (int index = 1; index < a.Length; ++index)
				{
					sb.Append(", ").Append(a[index]);
				}
			}
			sb.Append(']');
			return sb.ToString();
		}

		public static int GetHashCode(
			byte[] data)
		{
			if (data == null)
			{
				return 0;
			}

			int i = data.Length;
			int hc = i + 1;

			while (--i >= 0)
			{
				hc *= 257;
				hc ^= data[i];
			}

			return hc;
		}

		public static byte[] Clone(
			byte[] data)
		{
			return data == null ? null : (byte[]) data.Clone();
		}

		public static int[] Clone(
			int[] data)
		{
			return data == null ? null : (int[]) data.Clone();
		}

		public static void Fill(
			byte[]	buf,
			byte	b)
		{
			int i = buf.Length;
			while (i > 0)
			{
				buf[--i] = b;
			}
		}

        public static byte[] Copy(byte[] data, int off, int len)
        {
            byte[] result = new byte[len];
            Array.Copy(data, off, result, 0, len);
            return result;
        }
	}
}
