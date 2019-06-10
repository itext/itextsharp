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
     * Generalized time object.
     */
    public class DerGeneralizedTime
        : Asn1Object
    {
        private readonly string time;

		/**
         * return a generalized time from the passed in object
         *
         * @exception ArgumentException if the object cannot be converted.
         */
        public static DerGeneralizedTime GetInstance(
            object obj)
        {
			if (obj == null || obj is DerGeneralizedTime)
            {
                return (DerGeneralizedTime)obj;
            }

			throw new ArgumentException("illegal object in GetInstance: " + obj.GetType().Name, "obj");
        }

		/**
         * return a Generalized Time object from a tagged object.
         *
         * @param obj the tagged object holding the object we want
         * @param explicitly true if the object is meant to be explicitly
         *              tagged false otherwise.
         * @exception ArgumentException if the tagged object cannot
         *               be converted.
         */
        public static DerGeneralizedTime GetInstance(
            Asn1TaggedObject	obj,
            bool				isExplicit)
        {
			Asn1Object o = obj.GetObject();

			if (isExplicit || o is DerGeneralizedTime)
			{
				return GetInstance(o);
			}

			return new DerGeneralizedTime(((Asn1OctetString)o).GetOctets());
        }

		/**
		 * The correct format for this is YYYYMMDDHHMMSS[.f]Z, or without the Z
		 * for local time, or Z+-HHMM on the end, for difference between local
		 * time and UTC time. The fractional second amount f must consist of at
		 * least one number with trailing zeroes removed.
		 *
		 * @param time the time string.
		 * @exception ArgumentException if string is an illegal format.
		 */
		public DerGeneralizedTime(
			string time)
		{
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
         * base constructor from a local time object
         */
        public DerGeneralizedTime(
            DateTime time)
        {
            this.time = time.ToString(@"yyyyMMddHHmmss\Z");
        }

		internal DerGeneralizedTime(
            byte[] bytes)
        {
            //
            // explicitly convert to characters
            //
            this.time = Strings.FromAsciiByteArray(bytes);
        }

		/**
		 * Return the time.
		 * @return The time string as it appeared in the encoded object.
		 */
		public string TimeString
		{
			get { return time; }
		}

		/**
         * return the time - always in the form of
         *  YYYYMMDDhhmmssGMT(+hh:mm|-hh:mm).
         * <p>
         * Normally in a certificate we would expect "Z" rather than "GMT",
         * however adding the "GMT" means we can just use:
         * <pre>
         *     dateF = new SimpleDateFormat("yyyyMMddHHmmssz");
         * </pre>
         * To read in the time and Get a date which is compatible with our local
         * time zone.</p>
         */
        public string GetTime()
        {
            //
            // standardise the format.
            //
            if (time[time.Length - 1] == 'Z')
            {
                return time.Substring(0, time.Length - 1) + "GMT+00:00";
            }
            else
            {
                int signPos = time.Length - 5;
                char sign = time[signPos];
                if (sign == '-' || sign == '+')
                {
                    return time.Substring(0, signPos)
                        + "GMT"
                        + time.Substring(signPos, 3)
                        + ":"
                        + time.Substring(signPos + 3);
                }
                else
                {
                    signPos = time.Length - 3;
                    sign = time[signPos];
                    if (sign == '-' || sign == '+')
                    {
                        return time.Substring(0, signPos)
                            + "GMT"
                            + time.Substring(signPos)
                            + ":00";
                    }
                }
            }

            return time + CalculateGmtOffset();
        }

		private string CalculateGmtOffset()
		{
			char sign = '+';
            DateTime time = ToDateTime();

#if SILVERLIGHT
			long offset = time.Ticks - time.ToUniversalTime().Ticks;
			if (offset < 0)
			{
				sign = '-';
				offset = -offset;
			}
			int hours = (int)(offset / TimeSpan.TicksPerHour);
			int minutes = (int)(offset / TimeSpan.TicksPerMinute) % 60;
#else
            // Note: GetUtcOffset incorporates Daylight Savings offset
			TimeSpan offset =  TimeZone.CurrentTimeZone.GetUtcOffset(time);
			if (offset.CompareTo(TimeSpan.Zero) < 0)
			{
				sign = '-';
				offset = offset.Duration();
			}
			int hours = offset.Hours;
			int minutes = offset.Minutes;
#endif

			return "GMT" + sign + Convert(hours) + ":" + Convert(minutes);
		}

		private static string Convert(
			int time)
		{
			if (time < 10)
			{
				return "0" + time;
			}

			return time.ToString();
		}

		public DateTime ToDateTime()
		{
			string formatStr;
			string d = time;
			bool makeUniversal = false;

			if (d.EndsWith("Z"))
			{
				if (HasFractionalSeconds)
				{
					int fCount = d.Length - d.IndexOf('.') - 2;
					formatStr = @"yyyyMMddHHmmss." + FString(fCount) + @"\Z";
				}
				else
				{
					formatStr = @"yyyyMMddHHmmss\Z";
				}
			}
			else if (time.IndexOf('-') > 0 || time.IndexOf('+') > 0)
			{
				d = GetTime();
				makeUniversal = true;

				if (HasFractionalSeconds)
				{
					int fCount = d.IndexOf("GMT") - 1 - d.IndexOf('.');
					formatStr = @"yyyyMMddHHmmss." + FString(fCount) + @"'GMT'zzz";
				}
				else
				{
					formatStr = @"yyyyMMddHHmmss'GMT'zzz";
				}
			}
			else
			{
				if (HasFractionalSeconds)
				{
					int fCount = d.Length - 1 - d.IndexOf('.');
					formatStr = @"yyyyMMddHHmmss." + FString(fCount);
				}
				else
				{
					formatStr = @"yyyyMMddHHmmss";
				}

				// TODO?
//				dateF.setTimeZone(new SimpleTimeZone(0, TimeZone.getDefault().getID()));
			}

			return ParseDateString(d, formatStr, makeUniversal);
		}

		private string FString(
			int count)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < count; ++i)
			{
				sb.Append('f');
			}
			return sb.ToString();
		}

		private DateTime ParseDateString(
			string	dateStr,
			string	formatStr,
			bool	makeUniversal)
		{
			DateTime dt = DateTime.ParseExact(
				dateStr,
				formatStr,
				DateTimeFormatInfo.InvariantInfo);

			return makeUniversal ? dt.ToUniversalTime() : dt;
		}

		private bool HasFractionalSeconds
		{
			get { return time.IndexOf('.') == 14; }
		}

		private byte[] GetOctets()
        {
            return Strings.ToAsciiByteArray(time);
        }

		internal override void Encode(
            DerOutputStream derOut)
        {
            derOut.WriteEncoded(Asn1Tags.GeneralizedTime, GetOctets());
        }

		protected override bool Asn1Equals(
			Asn1Object asn1Object)
        {
			DerGeneralizedTime other = asn1Object as DerGeneralizedTime;

			if (other == null)
				return false;

			return this.time.Equals(other.time);
        }

		protected override int Asn1GetHashCode()
		{
            return time.GetHashCode();
        }
    }
}
