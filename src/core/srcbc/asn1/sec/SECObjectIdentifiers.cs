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
using Org.BouncyCastle.Asn1.X9;

namespace Org.BouncyCastle.Asn1.Sec
{
	public abstract class SecObjectIdentifiers
	{
		/**
		 *  EllipticCurve OBJECT IDENTIFIER ::= {
		 *        iso(1) identified-organization(3) certicom(132) curve(0)
		 *  }
		 */
		public static readonly DerObjectIdentifier EllipticCurve = new DerObjectIdentifier("1.3.132.0");

		public static readonly DerObjectIdentifier SecT163k1 = new DerObjectIdentifier(EllipticCurve + ".1");
		public static readonly DerObjectIdentifier SecT163r1 = new DerObjectIdentifier(EllipticCurve + ".2");
		public static readonly DerObjectIdentifier SecT239k1 = new DerObjectIdentifier(EllipticCurve + ".3");
		public static readonly DerObjectIdentifier SecT113r1 = new DerObjectIdentifier(EllipticCurve + ".4");
		public static readonly DerObjectIdentifier SecT113r2 = new DerObjectIdentifier(EllipticCurve + ".5");
		public static readonly DerObjectIdentifier SecP112r1 = new DerObjectIdentifier(EllipticCurve + ".6");
		public static readonly DerObjectIdentifier SecP112r2 = new DerObjectIdentifier(EllipticCurve + ".7");
		public static readonly DerObjectIdentifier SecP160r1 = new DerObjectIdentifier(EllipticCurve + ".8");
		public static readonly DerObjectIdentifier SecP160k1 = new DerObjectIdentifier(EllipticCurve + ".9");
		public static readonly DerObjectIdentifier SecP256k1 = new DerObjectIdentifier(EllipticCurve + ".10");
		public static readonly DerObjectIdentifier SecT163r2 = new DerObjectIdentifier(EllipticCurve + ".15");
		public static readonly DerObjectIdentifier SecT283k1 = new DerObjectIdentifier(EllipticCurve + ".16");
		public static readonly DerObjectIdentifier SecT283r1 = new DerObjectIdentifier(EllipticCurve + ".17");
		public static readonly DerObjectIdentifier SecT131r1 = new DerObjectIdentifier(EllipticCurve + ".22");
		public static readonly DerObjectIdentifier SecT131r2 = new DerObjectIdentifier(EllipticCurve + ".23");
		public static readonly DerObjectIdentifier SecT193r1 = new DerObjectIdentifier(EllipticCurve + ".24");
		public static readonly DerObjectIdentifier SecT193r2 = new DerObjectIdentifier(EllipticCurve + ".25");
		public static readonly DerObjectIdentifier SecT233k1 = new DerObjectIdentifier(EllipticCurve + ".26");
		public static readonly DerObjectIdentifier SecT233r1 = new DerObjectIdentifier(EllipticCurve + ".27");
		public static readonly DerObjectIdentifier SecP128r1 = new DerObjectIdentifier(EllipticCurve + ".28");
		public static readonly DerObjectIdentifier SecP128r2 = new DerObjectIdentifier(EllipticCurve + ".29");
		public static readonly DerObjectIdentifier SecP160r2 = new DerObjectIdentifier(EllipticCurve + ".30");
		public static readonly DerObjectIdentifier SecP192k1 = new DerObjectIdentifier(EllipticCurve + ".31");
		public static readonly DerObjectIdentifier SecP224k1 = new DerObjectIdentifier(EllipticCurve + ".32");
		public static readonly DerObjectIdentifier SecP224r1 = new DerObjectIdentifier(EllipticCurve + ".33");
		public static readonly DerObjectIdentifier SecP384r1 = new DerObjectIdentifier(EllipticCurve + ".34");
		public static readonly DerObjectIdentifier SecP521r1 = new DerObjectIdentifier(EllipticCurve + ".35");
		public static readonly DerObjectIdentifier SecT409k1 = new DerObjectIdentifier(EllipticCurve + ".36");
		public static readonly DerObjectIdentifier SecT409r1 = new DerObjectIdentifier(EllipticCurve + ".37");
		public static readonly DerObjectIdentifier SecT571k1 = new DerObjectIdentifier(EllipticCurve + ".38");
		public static readonly DerObjectIdentifier SecT571r1 = new DerObjectIdentifier(EllipticCurve + ".39");

		public static readonly DerObjectIdentifier SecP192r1 = X9ObjectIdentifiers.Prime192v1;
		public static readonly DerObjectIdentifier SecP256r1 = X9ObjectIdentifiers.Prime256v1;
	}
}