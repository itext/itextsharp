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
using System.Text;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1
{
    /**
     * UTC time object.
     */
    public class DerUtcTime
        : Asn1Object
    {
        private readonly string time;

		/**
         * return an UTC Time from the passed in object.
         *
         * @exception ArgumentException if the object cannot be converted.
         */
        public static DerUtcTime GetInstance(
            object obj)
        {
            if (obj == null || obj is DerUtcTime)
            {
                return (DerUtcTime)obj;
            }

            throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name);
        }

        /**
         * return an UTC Time from a tagged object.
         *
         * @param obj the tagged object holding the object we want
         * @param explicitly true if the object is meant to be explicitly
         *              tagged false otherwise.
         * @exception ArgumentException if the tagged object cannot
         *               be converted.
         */
        public static DerUtcTime GetInstance(
            Asn1TaggedObject	obj,
            bool				isExplicit)
        {
			Asn1Object o = obj.GetObject();

			if (isExplicit || o is DerUtcTime)
			{
				return GetInstance(o);
			}

			return new DerUtcTime(((Asn1OctetString)o).GetOctets());
        }

        /**
         * The correct format for this is YYMMDDHHMMSSZ (it used to be that seconds were
         * never encoded. When you're creating one of these objects from scratch, that's
         * what you want to use, otherwise we'll try to deal with whatever Gets read from
         * the input stream... (this is why the input format is different from the GetTime()
         * method output).
         * <p>
         * @param time the time string.</p>
         */
        public DerUtcTime(
            string time)
        {
			if (time == null)
				throw new ArgumentNullException("time");

			this.time = time;

			try
			{
				ToDateTime();
			}
			catch (FormatException e)
			{
				throw new ArgumentException("invalid date string: " + e.Message);
			}
        }

		/**
         * base constructor from a DateTime object
         */
        public DerUtcTime(
            DateTime time)
        {
            this.time = time.ToString("yyMMddHHmmss") + "Z";
        }

		internal DerUtcTime(
            byte[] bytes)
        {
            //
            // explicitly convert to characters
            //
            this.time = Strings.FromAsciiByteArray(bytes);
        }

//		public DateTime ToDateTime()
//		{
//			string tm = this.AdjustedTimeString;
//
//			return new DateTime(
//				Int16.Parse(tm.Substring(0, 4)),
//				Int16.Parse(tm.Substring(4, 2)),
//				Int16.Parse(tm.Substring(6, 2)),
//				Int16.Parse(tm.Substring(8, 2)),
//				Int16.Parse(tm.Substring(10, 2)),
//				Int16.Parse(tm.Substring(12, 2)));
//		}

		/**
		 * return the time as a date based on whatever a 2 digit year will return. For
		 * standardised processing use ToAdjustedDateTime().
		 *
		 * @return the resulting date
		 * @exception ParseException if the date string cannot be parsed.
		 */
		public DateTime ToDateTime()
		{
			return ParseDateString(TimeString, @"yyMMddHHmmss'GMT'zzz");
		}

		/**
		* return the time as an adjusted date
		* in the range of 1950 - 2049.
		*
		* @return a date in the range of 1950 to 2049.
		* @exception ParseException if the date string cannot be parsed.
		*/
		public DateTime ToAdjustedDateTime()
		{
			return ParseDateString(AdjustedTimeString, @"yyyyMMddHHmmss'GMT'zzz");
		}

		private DateTime ParseDateString(
			string	dateStr,
			string	formatStr)
		{
			DateTime dt = DateTime.ParseExact(
				dateStr,
				formatStr,
				DateTimeFormatInfo.InvariantInfo);

			return dt.ToUniversalTime();
		}

		/**
         * return the time - always in the form of
         *  YYMMDDhhmmssGMT(+hh:mm|-hh:mm).
         * <p>
         * Normally in a certificate we would expect "Z" rather than "GMT",
         * however adding the "GMT" means we can just use:
         * <pre>
         *     dateF = new SimpleDateFormat("yyMMddHHmmssz");
         * </pre>
         * To read in the time and Get a date which is compatible with our local
         * time zone.</p>
         * <p>
         * <b>Note:</b> In some cases, due to the local date processing, this
         * may lead to unexpected results. If you want to stick the normal
         * convention of 1950 to 2049 use the GetAdjustedTime() method.</p>
         */
        public string TimeString
        {
			get
			{
				//
				// standardise the format.
				//
				if (time.IndexOf('-') < 0 && time.IndexOf('+') < 0)
				{
					if (time.Length == 11)
					{
						return time.Substring(0, 10) + "00GMT+00:00";
					}
					else
					{
						return time.Substring(0, 12) + "GMT+00:00";
					}
				}
				else
				{
					int index = time.IndexOf('-');
					if (index < 0)
					{
						index = time.IndexOf('+');
					}
					string d = time;

					if (index == time.Length - 3)
					{
						d += "00";
					}

					if (index == 10)
					{
						return d.Substring(0, 10) + "00GMT" + d.Substring(10, 3) + ":" + d.Substring(13, 2);
					}
					else
					{
						return d.Substring(0, 12) + "GMT" + d.Substring(12, 3) + ":" +  d.Substring(15, 2);
					}
				}
			}
        }

		[Obsolete("Use 'AdjustedTimeString' property instead")]
		public string AdjustedTime
		{
			get { return AdjustedTimeString; }
		}

		/// <summary>
		/// Return a time string as an adjusted date with a 4 digit year.
		/// This goes in the range of 1950 - 2049.
		/// </summary>
		public string AdjustedTimeString
		{
			get
			{
				string d = TimeString;
				string c = d[0] < '5' ? "20" : "19";

				return c + d;
			}
		}

        private byte[] GetOctets()
        {
            return Strings.ToAsciiByteArray(time);
        }

		internal override void Encode(
            DerOutputStream derOut)
        {
            derOut.WriteEncoded(Asn1Tags.UtcTime, GetOctets());
        }

		protected override bool Asn1Equals(
			Asn1Object asn1Object)
		{
			DerUtcTime other = asn1Object as DerUtcTime;

			if (other == null)
				return false;

			return this.time.Equals(other.time);
        }

		protected override int Asn1GetHashCode()
		{
            return time.GetHashCode();
        }

		public override string ToString()
		{
			return time;
		}
	}
}
