using System;
using iTextSharp.xmp.impl;

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
    /// A factory to create <code>XMPDateTime</code>-instances from a <code>Calendar</code> or an
    /// ISO 8601 string or for the current time.
    /// 
    /// @since 16.02.2006
    /// </summary>
    public static class XmpDateTimeFactory {

        /// <summary>
        /// Obtain the current date and time.
        /// </summary>
        /// <returns> Returns The returned time is UTC, properly adjusted for the local time zone. The
        ///         resolution of the time is not guaranteed to be finer than seconds. </returns>
        public static IXmpDateTime CurrentDateTime {
            get { return new XmpDateTimeImpl(new XmpCalendar()); }
        }


        /// <summary>
        /// Creates an <code>XMPDateTime</code> from a <code>Calendar</code>-object.
        /// </summary>
        /// <param name="calendar"> a <code>Calendar</code>-object. </param>
        /// <returns> An <code>XMPDateTime</code>-object. </returns>
        public static IXmpDateTime CreateFromCalendar(XmpCalendar calendar) {
            return new XmpDateTimeImpl(calendar);
        }


        /// <summary>
        /// Creates an empty <code>XMPDateTime</code>-object. </summary>
        /// <returns> Returns an <code>XMPDateTime</code>-object. </returns>
        public static IXmpDateTime Create() {
            return new XmpDateTimeImpl();
        }


        /// <summary>
        /// Creates an <code>XMPDateTime</code>-object from initial values. </summary>
        /// <param name="year"> years </param>
        /// <param name="month"> months from 1 to 12<br>
        /// <em>Note:</em> Remember that the month in <seealso cref="Calendar"/> is defined from 0 to 11. </param>
        /// <param name="day"> days </param>
        /// <returns> Returns an <code>XMPDateTime</code>-object. </returns>
        public static IXmpDateTime Create(int year, int month, int day) {
            IXmpDateTime dt = new XmpDateTimeImpl();
            dt.Year = year;
            dt.Month = month;
            dt.Day = day;
            return dt;
        }


        /// <summary>
        /// Creates an <code>XMPDateTime</code>-object from initial values. </summary>
        /// <param name="year"> years </param>
        /// <param name="month"> months from 1 to 12<br>
        /// <em>Note:</em> Remember that the month in <seealso cref="Calendar"/> is defined from 0 to 11. </param>
        /// <param name="day"> days </param>
        /// <param name="hour"> hours </param>
        /// <param name="minute"> minutes </param>
        /// <param name="second"> seconds </param>
        /// <param name="nanoSecond"> nanoseconds </param>
        /// <returns> Returns an <code>XMPDateTime</code>-object. </returns>
        public static IXmpDateTime Create(int year, int month, int day, int hour, int minute, int second, int nanoSecond) {
            IXmpDateTime dt = new XmpDateTimeImpl();
            dt.Year = year;
            dt.Month = month;
            dt.Day = day;
            dt.Hour = hour;
            dt.Minute = minute;
            dt.Second = second;
            dt.NanoSecond = nanoSecond;
            return dt;
        }


        /// <summary>
        /// Creates an <code>XMPDateTime</code> from an ISO 8601 string.
        /// </summary>
        /// <param name="strValue"> The ISO 8601 string representation of the date/time. </param>
        /// <returns> An <code>XMPDateTime</code>-object. </returns>
        /// <exception cref="XmpException"> When the ISO 8601 string is non-conform </exception>
        public static IXmpDateTime CreateFromIso8601(string strValue) {
            return new XmpDateTimeImpl(strValue);
        }


        /// <summary>
        /// Sets the local time zone without touching any other Any existing time zone value is replaced,
        /// the other date/time fields are not adjusted in any way.
        /// </summary>
        /// <param name="dateTime"> the <code>XMPDateTime</code> variable containing the value to be modified. </param>
        /// <returns> Returns an updated <code>XMPDateTime</code>-object. </returns>
        public static IXmpDateTime SetLocalTimeZone(IXmpDateTime dateTime) {
            XmpCalendar cal = dateTime.Calendar;
            cal.TimeZone = TimeZone.CurrentTimeZone;
            return new XmpDateTimeImpl(cal);
        }


        /// <summary>
        /// Make sure a time is UTC. If the time zone is not UTC, the time is
        /// adjusted and the time zone set to be UTC.
        /// </summary>
        /// <param name="dateTime">
        ///            the <code>XMPDateTime</code> variable containing the time to
        ///            be modified. </param>
        /// <returns> Returns an updated <code>XMPDateTime</code>-object. </returns>
        public static IXmpDateTime ConvertToUtcTime(IXmpDateTime dateTime) {
            long timeInMillis = dateTime.Calendar.TimeInMillis;
            XmpCalendar cal = new XmpCalendar();
            cal.TimeInMillis = timeInMillis;
            return new XmpDateTimeImpl(cal);
        }


        /// <summary>
        /// Make sure a time is local. If the time zone is not the local zone, the time is adjusted and
        /// the time zone set to be local.
        /// </summary>
        /// <param name="dateTime"> the <code>XMPDateTime</code> variable containing the time to be modified. </param>
        /// <returns> Returns an updated <code>XMPDateTime</code>-object. </returns>
        public static IXmpDateTime ConvertToLocalTime(IXmpDateTime dateTime) {
            long timeInMillis = dateTime.Calendar.TimeInMillis;
            // has automatically local timezone
            XmpCalendar cal = new XmpCalendar();
            cal.TimeInMillis = timeInMillis;
            return new XmpDateTimeImpl(cal);
        }
    }
}
