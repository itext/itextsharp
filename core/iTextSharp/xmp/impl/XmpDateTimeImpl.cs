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

namespace iTextSharp.xmp.impl {
    using XMPDateTime = IXmpDateTime;
    using XmpException = XmpException;
    using XMPCalendar = XmpCalendar;

    /// <summary>
    /// The implementation of <code>XMPDateTime</code>. Internally a <code>calendar</code> is used
    /// plus an additional nano seconds field, because <code>Calendar</code> supports only milli
    /// seconds. The <code>nanoSeconds</code> convers only the resolution beyond a milli second.
    /// 
    /// @since 16.02.2006
    /// </summary>
    public class XmpDateTimeImpl : XMPDateTime {
        private int _day;
        private bool _hasDate;
        private bool _hasTime;
        private bool _hasTimeZone;
        private int _hour;
        private int _minute;
        private int _month;

        /// <summary>
        /// The nano seconds take micro and nano seconds, while the milli seconds are in the calendar.
        /// </summary>
        private int _nanoSeconds;

        private int _second;

        /// <summary>
        /// Use NO time zone as default </summary>
        private TimeZone _timeZone;

        private int _year;


        /// <summary>
        /// Creates an <code>XMPDateTime</code>-instance with the current time in the default time
        /// zone.
        /// </summary>
        public XmpDateTimeImpl() {
            // EMPTY
        }


        /// <summary>
        /// Creates an <code>XMPDateTime</code>-instance from a calendar.
        /// </summary>
        /// <param name="calendar"> a <code>Calendar</code> </param>
        public XmpDateTimeImpl(XMPCalendar calendar) {
            // extract the date and timezone from the calendar provided
            DateTime date = calendar.DateTime;
            TimeZone zone = calendar.TimeZone;

            _year = date.Year;
            _month = date.Month + 1; // cal is from 0..12
            _day = date.Day;
            _hour = date.Hour;
            _minute = date.Minute;
            _second = date.Second;
            _nanoSeconds = date.Millisecond*1000000;
            _timeZone = zone;

            // object contains all date components
            _hasDate = _hasTime = _hasTimeZone = true;
        }


        /// <summary>
        /// Creates an <code>XMPDateTime</code>-instance from 
        /// a <code>Date</code> and a <code>TimeZone</code>.
        /// </summary>
        /// <param name="date"> a date describing an absolute point in time </param>
        /// <param name="timeZone"> a TimeZone how to interpret the date </param>
        public XmpDateTimeImpl(DateTime date, TimeZone timeZone) {
            _year = date.Year;
            _month = date.Month + 1; // cal is from 0..12
            _day = date.Day;
            _hour = date.Hour;
            _minute = date.Minute;
            _second = date.Second;
            _nanoSeconds = date.Millisecond*1000000;
            _timeZone = timeZone;

            // object contains all date components
            _hasDate = _hasTime = _hasTimeZone = true;
        }


        /// <summary>
        /// Creates an <code>XMPDateTime</code>-instance from an ISO 8601 string.
        /// </summary>
        /// <param name="strValue"> an ISO 8601 string </param>
        /// <exception cref="XmpException"> If the string is a non-conform ISO 8601 string, an exception is thrown </exception>
        public XmpDateTimeImpl(string strValue) {
            Iso8601Converter.Parse(strValue, this);
        }

        #region XmpDateTime Members

        /// <seealso cref= XMPDateTime#getYear() </seealso>
        public virtual int Year {
            get { return _year; }
            set {
                _year = Math.Min(Math.Abs(value), 9999);
                _hasDate = true;
            }
        }


        /// <seealso cref= XMPDateTime#getMonth() </seealso>
        public virtual int Month {
            get { return _month; }
            set {
                if (value < 1) {
                    _month = 1;
                }
                else if (value > 12) {
                    _month = 12;
                }
                else {
                    _month = value;
                }

                _hasDate = true;
            }
        }


        /// <seealso cref= XMPDateTime#getDay() </seealso>
        public virtual int Day {
            get { return _day; }
            set {
                if (value < 1) {
                    _day = 1;
                }
                else if (value > 31) {
                    _day = 31;
                }
                else {
                    _day = value;
                }

                _hasDate = true;
            }
        }


        /// <seealso cref= XMPDateTime#getHour() </seealso>
        public virtual int Hour {
            get { return _hour; }
            set {
                _hour = Math.Min(Math.Abs(value), 23);
                _hasTime = true;
            }
        }


        /// <seealso cref= XMPDateTime#getMinute() </seealso>
        public virtual int Minute {
            get { return _minute; }
            set {
                _minute = Math.Min(Math.Abs(value), 59);
                _hasTime = true;
            }
        }


        /// <seealso cref= XMPDateTime#getSecond() </seealso>
        public virtual int Second {
            get { return _second; }
            set {
                _second = Math.Min(Math.Abs(value), 59);
                _hasTime = true;
            }
        }


        /// <seealso cref= XMPDateTime#getNanoSecond() </seealso>
        public virtual int NanoSecond {
            get { return _nanoSeconds; }
            set {
                _nanoSeconds = value;
                _hasTime = true;
            }
        }


        /// <seealso cref= IComparable#CompareTo(Object) </seealso>
        public virtual int CompareTo(object dt) {
            long d = Calendar.TimeInMillis - ((XMPDateTime) dt).Calendar.TimeInMillis;
            if (d != 0) {
                return Math.Sign(d);
            }
            // if millis are equal, compare nanoseconds
            d = _nanoSeconds - ((XMPDateTime) dt).NanoSecond;
            return Math.Sign(d);
        }


        /// <seealso cref= XMPDateTime#getTimeZone() </seealso>
        public virtual TimeZone TimeZone {
            get { return _timeZone; }
            set {
                _timeZone = value;
                _hasTime = true;
                _hasTimeZone = true;
            }
        }


        /// <seealso cref= XMPDateTime#hasDate() </seealso>
        public virtual bool HasDate() {
            return _hasDate;
        }


        /// <seealso cref= XMPDateTime#hasTime() </seealso>
        public virtual bool HasTime() {
            return _hasTime;
        }


        /// <seealso cref= XMPDateTime#hasTimeZone() </seealso>
        public virtual bool HasTimeZone() {
            return _hasTimeZone;
        }


        /// <seealso cref= XMPDateTime#getCalendar() </seealso>
        public virtual XMPCalendar Calendar {
            get {
                XMPCalendar calendar = new XMPCalendar();
                if (_hasTimeZone) {
                    calendar.TimeZone = _timeZone;
                }
                calendar.DateTime = new DateTime(_year, _month - 1, _day, _hour, _minute, _second, _nanoSeconds/1000000);

                return calendar;
            }
        }


        /// <seealso cref= XMPDateTime#getISO8601String() </seealso>
        public virtual string Iso8601String {
            get { return Iso8601Converter.Render(this); }
        }

        #endregion

        /// <returns> Returns the ISO string representation. </returns>
        public override string ToString() {
            return Iso8601String;
        }
    }
}
