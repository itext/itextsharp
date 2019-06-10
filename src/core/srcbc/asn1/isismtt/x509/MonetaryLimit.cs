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

using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.Asn1.IsisMtt.X509
{
	/**
	* Monetary limit for transactions. The QcEuMonetaryLimit QC statement MUST be
	* used in new certificates in place of the extension/attribute MonetaryLimit
	* since January 1, 2004. For the sake of backward compatibility with
	* certificates already in use, components SHOULD support MonetaryLimit (as well
	* as QcEuLimitValue).
	* <p/>
	* Indicates a monetary limit within which the certificate holder is authorized
	* to act. (This value DOES NOT express a limit on the liability of the
	* certification authority).
	* <p/>
	* <pre>
	*    MonetaryLimitSyntax ::= SEQUENCE
	*    {
	*      currency PrintableString (SIZE(3)),
	*      amount INTEGER,
	*      exponent INTEGER
	*    }
	* </pre>
	* <p/>
	* currency must be the ISO code.
	* <p/>
	* value = amount�10*exponent
	*/
	public class MonetaryLimit
		: Asn1Encodable
	{
		private readonly DerPrintableString	currency;
		private readonly DerInteger			amount;
		private readonly DerInteger			exponent;

		public static MonetaryLimit GetInstance(
			object obj)
		{
			if (obj == null || obj is MonetaryLimit)
			{
				return (MonetaryLimit) obj;
			}

			if (obj is Asn1Sequence)
			{
				return new MonetaryLimit(Asn1Sequence.GetInstance(obj));
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		private MonetaryLimit(
			Asn1Sequence seq)
		{
			if (seq.Count != 3)
				throw new ArgumentException("Bad sequence size: " + seq.Count);

			currency = DerPrintableString.GetInstance(seq[0]);
			amount = DerInteger.GetInstance(seq[1]);
			exponent = DerInteger.GetInstance(seq[2]);
		}

		/**
		* Constructor from a given details.
		* <p/>
		* <p/>
		* value = amount�10^exponent
		*
		* @param currency The currency. Must be the ISO code.
		* @param amount   The amount
		* @param exponent The exponent
		*/
		public MonetaryLimit(
			string	currency,
			int		amount,
			int		exponent)
		{
			this.currency = new DerPrintableString(currency, true);
			this.amount = new DerInteger(amount);
			this.exponent = new DerInteger(exponent);
		}

		public virtual string Currency
		{
			get { return currency.GetString(); }
		}

		public virtual BigInteger Amount
		{
			get { return amount.Value; }
		}

		public virtual BigInteger Exponent
		{
			get { return exponent.Value; }
		}

		/**
		* Produce an object suitable for an Asn1OutputStream.
		* <p/>
		* Returns:
		* <p/>
		* <pre>
		*    MonetaryLimitSyntax ::= SEQUENCE
		*    {
		*      currency PrintableString (SIZE(3)),
		*      amount INTEGER,
		*      exponent INTEGER
		*    }
		* </pre>
		*
		* @return an Asn1Object
		*/
		public override Asn1Object ToAsn1Object()
		{
			return new DerSequence(currency, amount, exponent);
		}

	}
}
