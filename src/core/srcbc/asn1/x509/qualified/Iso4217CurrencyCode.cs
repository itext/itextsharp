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

using Org.BouncyCastle.Asn1;

namespace Org.BouncyCastle.Asn1.X509.Qualified
{
    /**
    * The Iso4217CurrencyCode object.
    * <pre>
    * Iso4217CurrencyCode  ::=  CHOICE {
    *       alphabetic              PrintableString (SIZE 3), --Recommended
    *       numeric              INTEGER (1..999) }
    * -- Alphabetic or numeric currency code as defined in ISO 4217
    * -- It is recommended that the Alphabetic form is used
    * </pre>
    */
    public class Iso4217CurrencyCode
        : Asn1Encodable, IAsn1Choice
    {
        internal const int AlphabeticMaxSize = 3;
        internal const int NumericMinSize = 1;
        internal const int NumericMaxSize = 999;

		internal Asn1Encodable	obj;
//        internal int			numeric;

		public static Iso4217CurrencyCode GetInstance(
            object obj)
        {
            if (obj == null || obj is Iso4217CurrencyCode)
            {
                return (Iso4217CurrencyCode) obj;
            }

			if (obj is DerInteger)
            {
                DerInteger numericobj = DerInteger.GetInstance(obj);
                int numeric = numericobj.Value.IntValue;
                return new Iso4217CurrencyCode(numeric);
            }

			if (obj is DerPrintableString)
            {
                DerPrintableString alphabetic = DerPrintableString.GetInstance(obj);
                return new Iso4217CurrencyCode(alphabetic.GetString());
            }

			throw new ArgumentException("unknown object in GetInstance: " + obj.GetType().FullName, "obj");
        }

		public Iso4217CurrencyCode(
            int numeric)
        {
            if (numeric > NumericMaxSize || numeric < NumericMinSize)
            {
                throw new ArgumentException("wrong size in numeric code : not in (" +NumericMinSize +".."+ NumericMaxSize +")");
            }

			obj = new DerInteger(numeric);
        }

		public Iso4217CurrencyCode(
            string alphabetic)
        {
            if (alphabetic.Length > AlphabeticMaxSize)
            {
                throw new ArgumentException("wrong size in alphabetic code : max size is " + AlphabeticMaxSize);
            }

			obj = new DerPrintableString(alphabetic);
        }

		public bool IsAlphabetic { get { return obj is DerPrintableString; } }

		public string Alphabetic { get { return ((DerPrintableString) obj).GetString(); } }

		public int Numeric { get { return ((DerInteger)obj).Value.IntValue; } }

		public override Asn1Object ToAsn1Object()
        {
            return obj.ToAsn1Object();
        }
    }
}
