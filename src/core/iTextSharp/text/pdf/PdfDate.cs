using System;
using System.Text;
using System.Globalization;

/*
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */

namespace iTextSharp.text.pdf {

    /**
     * <CODE>PdfDate</CODE> is the PDF date object.
     * <P>
     * PDF defines a standard date format. The PDF date format closely follows the format
     * defined by the international standard ASN.1 (Abstract Syntax Notation One, defined
     * in CCITT X.208 or ISO/IEC 8824). A date is a <CODE>PdfString</CODE> of the form:
     * <P><BLOCKQUOTE>
     * (D:YYYYMMDDHHmmSSOHH'mm')
     * </BLOCKQUOTE><P>
     * This object is described in the 'Portable Document Format Reference Manual version 1.3'
     * section 7.2 (page 183-184)
     *
     * @see     PdfString
     * @see     java.util.GregorianCalendar
     */

    public class PdfDate : PdfString {
    
        // constructors
    
        /**
         * Constructs a <CODE>PdfDate</CODE>-object.
         *
         * @param       d           the date that has to be turned into a <CODE>PdfDate</CODE>-object
         */
    
        public PdfDate(DateTime d) : base() {
            //d = d.ToUniversalTime();
            
            value = d.ToString("\\D\\:yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo);
            string timezone = d.ToString("zzz", DateTimeFormatInfo.InvariantInfo);
            timezone = timezone.Replace(":", "'");
            value += timezone + "'";
        }
    
        /**
         * Constructs a <CODE>PdfDate</CODE>-object, representing the current day and time.
         */
    
        public PdfDate() : this(DateTime.Now) {}
    
        /**
         * Adds a number of leading zeros to a given <CODE>string</CODE> in order to get a <CODE>string</CODE>
         * of a certain length.
         *
         * @param       i           a given number
         * @param       length      the length of the resulting <CODE>string</CODE>
         * @return      the resulting <CODE>string</CODE>
         */
    
        private static String SetLength(int i, int length) {
            return i.ToString().PadLeft(length, '0');
        }

        /**
        * Gives the W3C format of the PdfDate.
        * @return a formatted date
        */
        virtual public String GetW3CDate() {
            return GetW3CDate(value);
        }
        
        /**
        * Gives the W3C format of the PdfDate.
        * @param d the date in the format D:YYYYMMDDHHmmSSOHH'mm'
        * @return a formatted date
        */
        public static String GetW3CDate(String d) {
            if (d.StartsWith("D:"))
                d = d.Substring(2);
            StringBuilder sb = new StringBuilder();
            if (d.Length < 4)
                return "0000";
            sb.Append(d.Substring(0, 4)); //year
            d = d.Substring(4);
            if (d.Length < 2)
                return sb.ToString();
            sb.Append('-').Append(d.Substring(0, 2)); //month
            d = d.Substring(2);
            if (d.Length < 2)
                return sb.ToString();
            sb.Append('-').Append(d.Substring(0, 2)); //day
            d = d.Substring(2);
            if (d.Length < 2)
                return sb.ToString();
            sb.Append('T').Append(d.Substring(0, 2)); //hour
            d = d.Substring(2);
            if (d.Length < 2) {
                sb.Append(":00Z");
                return sb.ToString();
            }
            sb.Append(':').Append(d.Substring(0, 2)); //minute
            d = d.Substring(2);
            if (d.Length < 2) {
                sb.Append('Z');
                return sb.ToString();
            }
            sb.Append(':').Append(d.Substring(0, 2)); //second
            d = d.Substring(2);
            if (d.StartsWith("-") || d.StartsWith("+")) {
                String sign = d.Substring(0, 1);
                d = d.Substring(1);
                String h = "00";
                String m = "00";
                if (d.Length >= 2) {
                    h = d.Substring(0, 2);
                    if (d.Length > 2) {
                        d = d.Substring(3);
                        if (d.Length >= 2)
                            m = d.Substring(0, 2);
                    }
                    sb.Append(sign).Append(h).Append(':').Append(m);
                    return sb.ToString();
                }
            }
            sb.Append('Z');
            return sb.ToString();
        }

        public static DateTime Decode(string date) {
            if (date.StartsWith("D:"))
                date = date.Substring(2);
            int year, month = 1, day = 1, hour = 0, minute = 0, second = 0;
            int offsetHour = 0, offsetMinute = 0;
            char variation = '\0';
            year = int.Parse(date.Substring(0, 4));
            if (date.Length >= 6) {
                month = int.Parse(date.Substring(4, 2));
                if (date.Length >= 8) {
                    day = int.Parse(date.Substring(6, 2));
                    if (date.Length >= 10) {
                        hour = int.Parse(date.Substring(8, 2));
                        if (date.Length >= 12) {
                            minute = int.Parse(date.Substring(10, 2));
                            if (date.Length >= 14) {
                                second = int.Parse(date.Substring(12, 2));
                            }
                        }
                    }
                }
            }
            DateTime d = new DateTime(year, month, day, hour, minute, second);
            if (date.Length <= 14)
                return d;
            variation = date[14];
            if (variation == 'Z')
                return d.ToLocalTime();
            if (date.Length >= 17) {
                offsetHour = int.Parse(date.Substring(15, 2));
                if (date.Length >= 20) {
                    offsetMinute = int.Parse(date.Substring(18, 2));
                }
            }
            TimeSpan span = new TimeSpan(offsetHour, offsetMinute, 0);
            if (variation == '-')
                d += span;
            else
                d -= span;
            return d.ToLocalTime();
        }
    }
}
