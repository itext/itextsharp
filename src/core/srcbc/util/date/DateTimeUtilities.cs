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

namespace Org.BouncyCastle.Utilities.Date
{
	public class DateTimeUtilities
	{
		public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

		private DateTimeUtilities()
		{
		}

		/// <summary>
		/// Return the number of milliseconds since the Unix epoch (1 Jan., 1970 UTC) for a given DateTime value.
		/// </summary>
		/// <param name="dateTime">A UTC DateTime value not before epoch.</param>
		/// <returns>Number of whole milliseconds after epoch.</returns>
		/// <exception cref="ArgumentException">'dateTime' is before epoch.</exception>
		public static long DateTimeToUnixMs(
			DateTime dateTime)
		{
			if (dateTime.CompareTo(UnixEpoch) < 0)
				throw new ArgumentException("DateTime value may not be before the epoch", "dateTime");

			return (dateTime.Ticks - UnixEpoch.Ticks) / TimeSpan.TicksPerMillisecond;
		}

		/// <summary>
		/// Create a DateTime value from the number of milliseconds since the Unix epoch (1 Jan., 1970 UTC).
		/// </summary>
		/// <param name="unixMs">Number of milliseconds since the epoch.</param>
		/// <returns>A UTC DateTime value</returns>
		public static DateTime UnixMsToDateTime(
			long unixMs)
		{
			return new DateTime(unixMs * TimeSpan.TicksPerMillisecond + UnixEpoch.Ticks);
		}

		/// <summary>
		/// Return the current number of milliseconds since the Unix epoch (1 Jan., 1970 UTC).
		/// </summary>
		public static long CurrentUnixMs()
		{
			return DateTimeToUnixMs(DateTime.UtcNow);
		}
	}
}
