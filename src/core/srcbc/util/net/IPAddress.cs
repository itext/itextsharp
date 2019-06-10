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

using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Utilities.Net
{
	public class IPAddress
	{
		/**
		 * Validate the given IPv4 or IPv6 address.
		 *
		 * @param address the IP address as a string.
		 *
		 * @return true if a valid address, false otherwise
		 */
		public static bool IsValid(
			string address)
		{
			return IsValidIPv4(address) || IsValidIPv6(address);
		}

		/**
		 * Validate the given IPv4 or IPv6 address and netmask.
		 *
		 * @param address the IP address as a string.
		 *
		 * @return true if a valid address with netmask, false otherwise
		 */
		public static bool IsValidWithNetMask(
			string address)
		{
			return IsValidIPv4WithNetmask(address) || IsValidIPv6WithNetmask(address);
		}

		/**
		 * Validate the given IPv4 address.
		 * 
		 * @param address the IP address as a string.
		 *
		 * @return true if a valid IPv4 address, false otherwise
		 */
		public static bool IsValidIPv4(
			string address)
		{
			try
			{
				return unsafeIsValidIPv4(address);
			}
			catch (FormatException) {}
			catch (OverflowException) {}
			return false;
		}

		private static bool unsafeIsValidIPv4(
			string address)
		{
			if (address.Length == 0)
				return false;

			int octets = 0;
			string temp = address + ".";

			int pos;
			int start = 0;
			while (start < temp.Length
				&& (pos = temp.IndexOf('.', start)) > start)
			{
				if (octets == 4)
					return false;

				string octetStr = temp.Substring(start, pos - start);
				int octet = Int32.Parse(octetStr);

				if (octet < 0 || octet > 255)
					return false;

				start = pos + 1;
				octets++;
			}

			return octets == 4;
		}

		public static bool IsValidIPv4WithNetmask(
			string address)
		{
			int index = address.IndexOf("/");
			string mask = address.Substring(index + 1);

			return (index > 0) && IsValidIPv4(address.Substring(0, index))
				&& (IsValidIPv4(mask) || IsMaskValue(mask, 32));
		}

		public static bool IsValidIPv6WithNetmask(
			string address)
		{
			int index = address.IndexOf("/");
			string mask = address.Substring(index + 1);

			return (index > 0) && (IsValidIPv6(address.Substring(0, index))
				&& (IsValidIPv6(mask) || IsMaskValue(mask, 128)));
		}

		private static bool IsMaskValue(
			string	component,
			int		size)
		{
			int val = Int32.Parse(component);
			try
			{
				return val >= 0 && val <= size;
			}
			catch (FormatException) {}
			catch (OverflowException) {}
			return false;
		}

		/**
		 * Validate the given IPv6 address.
		 *
		 * @param address the IP address as a string.
		 *
		 * @return true if a valid IPv4 address, false otherwise
		 */
		public static bool IsValidIPv6(
			string address)
		{
			try
			{
				return unsafeIsValidIPv6(address);
			}
			catch (FormatException) {}
			catch (OverflowException) {}
			return false;
		}

		private static bool unsafeIsValidIPv6(
			string address)
		{
			if (address.Length == 0)
			{
				return false;
			}

			int octets = 0;

			string temp = address + ":";
			bool doubleColonFound = false;
			int pos;
			int start = 0;
			while (start < temp.Length
				&& (pos = temp.IndexOf(':', start)) >= start)
			{
				if (octets == 8)
				{
					return false;
				}

				if (start != pos)
				{
					string value = temp.Substring(start, pos - start);

					if (pos == (temp.Length - 1) && value.IndexOf('.') > 0)
					{
						if (!IsValidIPv4(value))
						{
							return false;
						}

						octets++; // add an extra one as address covers 2 words.
					}
					else
					{
						string octetStr = temp.Substring(start, pos - start);
						int octet = Int32.Parse(octetStr, NumberStyles.AllowHexSpecifier);

						if (octet < 0 || octet > 0xffff)
							return false;
					}
				}
				else
				{
					if (pos != 1 && pos != temp.Length - 1 && doubleColonFound)
					{
						return false;
					}
					doubleColonFound = true;
				}
				start = pos + 1;
				octets++;
			}

			return octets == 8 || doubleColonFound;
		}
	}
}
