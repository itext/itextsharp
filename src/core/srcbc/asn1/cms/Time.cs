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

using Org.BouncyCastle.Asn1;

namespace Org.BouncyCastle.Asn1.Cms
{
    public class Time
        : Asn1Encodable, IAsn1Choice
    {
        private readonly Asn1Object time;

		public static Time GetInstance(
            Asn1TaggedObject	obj,
            bool				explicitly)
        {
            return GetInstance(obj.GetObject());
        }

		public Time(
            Asn1Object time)
        {
            if (!(time is DerUtcTime)
                && !(time is DerGeneralizedTime))
            {
                throw new ArgumentException("unknown object passed to Time");
            }

			this.time = time;
        }

		/**
         * creates a time object from a given date - if the date is between 1950
         * and 2049 a UTCTime object is Generated, otherwise a GeneralizedTime
         * is used.
         */
        public Time(
            DateTime date)
        {
            string d = date.ToString("yyyyMMddHHmmss") + "Z";

			int year = int.Parse(d.Substring(0, 4));

			if (year < 1950 || year > 2049)
            {
                time = new DerGeneralizedTime(d);
            }
            else
            {
                time = new DerUtcTime(d.Substring(2));
            }
        }

		public static Time GetInstance(
            object obj)
        {
            if (obj == null || obj is Time)
                return (Time)obj;

			if (obj is DerUtcTime)
                return new Time((DerUtcTime)obj);

			if (obj is DerGeneralizedTime)
                return new Time((DerGeneralizedTime)obj);

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
        }

		public string TimeString
        {
			get
			{
				if (time is DerUtcTime)
				{
					return ((DerUtcTime)time).AdjustedTimeString;
				}
				else
				{
					return ((DerGeneralizedTime)time).GetTime();
				}
			}
        }

		public DateTime Date
        {
			get
			{
				try
				{
					if (time is DerUtcTime)
					{
						return ((DerUtcTime)time).ToAdjustedDateTime();
					}

					return ((DerGeneralizedTime)time).ToDateTime();
				}
				catch (FormatException e)
				{
					// this should never happen
					throw new InvalidOperationException("invalid date string: " + e.Message);
				}
			}
        }

		/**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         * Time ::= CHOICE {
         *             utcTime        UTCTime,
         *             generalTime    GeneralizedTime }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            return time;
        }
    }
}
