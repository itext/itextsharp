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

using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Tls
{
	/// <summary>
	/// RFC 4492 5.1.1
	/// The named curves defined here are those specified in SEC 2 [13]. Note that many of
 	/// these curves are also recommended in ANSI X9.62 [7] and FIPS 186-2 [11]. Values 0xFE00
	/// through 0xFEFF are reserved for private use. Values 0xFF01 and 0xFF02 indicate that the
	/// client supports arbitrary prime and characteristic-2 curves, respectively (the curve
	/// parameters must be encoded explicitly in ECParameters).
	/// </summary>
	public enum NamedCurve : int
	{
		sect163k1 = 1,
		sect163r1 = 2,
		sect163r2 = 3,
		sect193r1 = 4,
		sect193r2 = 5,
		sect233k1 = 6,
		sect233r1 = 7,
		sect239k1 = 8,
		sect283k1 = 9,
		sect283r1 = 10,
		sect409k1 = 11,
		sect409r1 = 12,
		sect571k1 = 13,
		sect571r1 = 14,
		secp160k1 = 15,
		secp160r1 = 16,
		secp160r2 = 17,
		secp192k1 = 18,
		secp192r1 = 19,
		secp224k1 = 20,
		secp224r1 = 21,
		secp256k1 = 22,
		secp256r1 = 23,
		secp384r1 = 24,
		secp521r1 = 25,

		/*
		 * reserved (0xFE00..0xFEFF)
		 */

		arbitrary_explicit_prime_curves = 0xFF01,
		arbitrary_explicit_char2_curves = 0xFF02,
	}

	internal class NamedCurveHelper
	{
	    internal static ECDomainParameters GetECParameters(NamedCurve namedCurve)
	    {
            if (!Enum.IsDefined(typeof(NamedCurve), namedCurve))
                return null;

            string curveName = namedCurve.ToString();

            // Lazily created the first time a particular curve is accessed
	        X9ECParameters ecP = SecNamedCurves.GetByName(curveName);

            if (ecP == null)
                return null;

	        // It's a bit inefficient to do this conversion every time
	        return new ECDomainParameters(ecP.Curve, ecP.G, ecP.N, ecP.H, ecP.GetSeed());
	    }
	}
}
