using System;

//Copyright (c) 2006, Adobe Systems Incorporated
//All rights reserved.
//
//        Redistribution and use in source and binary forms, with or without
//        modification, are permitted provided that the following conditions are met:
//        1. Redistributions of source code must retain the above copyright
//        notice, this list of conditions and the following disclaimer.
//        2. Redistributions in binary form must reproduce the above copyright
//        notice, this list of conditions and the following disclaimer in the
//        documentation and/or other materials provided with the distribution.
//        3. All advertising materials mentioning features or use of this software
//        must display the following acknowledgement:
//        This product includes software developed by the Adobe Systems Incorporated.
//        4. Neither the name of the Adobe Systems Incorporated nor the
//        names of its contributors may be used to endorse or promote products
//        derived from this software without specific prior written permission.
//
//        THIS SOFTWARE IS PROVIDED BY ADOBE SYSTEMS INCORPORATED ''AS IS'' AND ANY
//        EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//        WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//        DISCLAIMED. IN NO EVENT SHALL ADOBE SYSTEMS INCORPORATED BE LIABLE FOR ANY
//        DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//        (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//        LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//        ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//        (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//        SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//        http://www.adobe.com/devnet/xmp/library/eula-xmp-library-java.html

namespace iTextSharp.xmp {
    /// <summary>
    /// The <code>XMPDateTime</code>-class represents a point in time up to a resolution of nano
    /// seconds. Dates and time in the serialized XMP are ISO 8601 strings. There are utility functions
    /// to convert to the ISO format, a <code>Calendar</code> or get the Timezone. The fields of
    /// <code>XMPDateTime</code> are:
    /// <ul>
    /// <li> month - The month in the range 1..12.
    /// <li> day - The day of the month in the range 1..31.
    /// <li> minute - The minute in the range 0..59.
    /// <li> hour - The time zone hour in the range 0..23.
    /// <li> minute - The time zone minute in the range 0..59.
    /// <li> nanoSecond - The nano seconds within a second. <em>Note:</em> if the XMPDateTime is
    /// converted into a calendar, the resolution is reduced to milli seconds.
    /// <li> timeZone - a <code>TimeZone</code>-object.
    /// </ul>
    /// DateTime values are occasionally used in cases with only a date or only a time component. A date
    /// without a time has zeros for all the time fields. A time without a date has zeros for all date
    /// fields (year, month, and day).
    /// </summary>
    public interface IXmpDateTime : IComparable {
        /// <returns> Returns the year, can be negative. </returns>
        int Year { get; set; }


        /// <returns> Returns The month in the range 1..12. </returns>
        int Month { get; set; }


        /// <returns> Returns the day of the month in the range 1..31. </returns>
        int Day { get; set; }


        /// <returns> Returns hour - The hour in the range 0..23. </returns>
        int Hour { get; set; }


        /// <returns> Returns the minute in the range 0..59. </returns>
        int Minute { get; set; }


        /// <returns> Returns the second in the range 0..59. </returns>
        int Second { get; set; }


        /// <returns> Returns milli-, micro- and nano seconds.
        /// 		   Nanoseconds within a second, often left as zero? </returns>
        int NanoSecond { get; set; }


        /// <returns> Returns the time zone. </returns>
        TimeZone TimeZone { get; set; }

        /// <returns> Returns a <code>Calendar</code> (only with milli second precision). <br>
        ///  		<em>Note:</em> the dates before Oct 15th 1585 (which normally fall into validity of 
        ///  		the Julian calendar) are also rendered internally as Gregorian dates.  </returns>
        XmpCalendar Calendar { get; }

        /// <returns> Returns the ISO 8601 string representation of the date and time. </returns>
        string Iso8601String { get; }


        /// <summary>
        /// This flag is set either by parsing or by setting year, month or day. </summary>
        /// <returns> Returns true if the XMPDateTime object has a date portion. </returns>
        bool HasDate();

        /// <summary>
        /// This flag is set either by parsing or by setting hours, minutes, seconds or milliseconds. </summary>
        /// <returns> Returns true if the XMPDateTime object has a time portion. </returns>
        bool HasTime();

        /// <summary>
        /// This flag is set either by parsing or by setting hours, minutes, seconds or milliseconds. </summary>
        /// <returns> Returns true if the XMPDateTime object has a defined timezone. </returns>
        bool HasTimeZone();
    }
}
