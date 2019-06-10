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

namespace Org.BouncyCastle.Asn1.CryptoPro
{
    public abstract class CryptoProObjectIdentifiers
    {
        // GOST Algorithms OBJECT IDENTIFIERS :
        // { iso(1) member-body(2) ru(643) rans(2) cryptopro(2)}
        public const string GostID = "1.2.643.2.2";

		public static readonly DerObjectIdentifier GostR3411 = new DerObjectIdentifier(GostID + ".9");
        public static readonly DerObjectIdentifier GostR3411Hmac = new DerObjectIdentifier(GostID + ".10");

        public static readonly DerObjectIdentifier GostR28147Cbc = new DerObjectIdentifier(GostID + ".21");

        public static readonly DerObjectIdentifier ID_Gost28147_89_CryptoPro_A_ParamSet = new DerObjectIdentifier(GostID + ".31.1");

        public static readonly DerObjectIdentifier GostR3410x94 = new DerObjectIdentifier(GostID + ".20");
        public static readonly DerObjectIdentifier GostR3410x2001 = new DerObjectIdentifier(GostID + ".19");
        public static readonly DerObjectIdentifier GostR3411x94WithGostR3410x94 = new DerObjectIdentifier(GostID + ".4");
        public static readonly DerObjectIdentifier GostR3411x94WithGostR3410x2001 = new DerObjectIdentifier(GostID + ".3");

		// { iso(1) member-body(2) ru(643) rans(2) cryptopro(2) hashes(30) }
        public static readonly DerObjectIdentifier GostR3411x94CryptoProParamSet = new DerObjectIdentifier(GostID + ".30.1");

		// { iso(1) member-body(2) ru(643) rans(2) cryptopro(2) signs(32) }
        public static readonly DerObjectIdentifier GostR3410x94CryptoProA = new DerObjectIdentifier(GostID + ".32.2");
        public static readonly DerObjectIdentifier GostR3410x94CryptoProB = new DerObjectIdentifier(GostID + ".32.3");
        public static readonly DerObjectIdentifier GostR3410x94CryptoProC = new DerObjectIdentifier(GostID + ".32.4");
        public static readonly DerObjectIdentifier GostR3410x94CryptoProD = new DerObjectIdentifier(GostID + ".32.5");

		// { iso(1) member-body(2) ru(643) rans(2) cryptopro(2) exchanges(33) }
        public static readonly DerObjectIdentifier GostR3410x94CryptoProXchA = new DerObjectIdentifier(GostID + ".33.1");
        public static readonly DerObjectIdentifier GostR3410x94CryptoProXchB = new DerObjectIdentifier(GostID + ".33.2");
        public static readonly DerObjectIdentifier GostR3410x94CryptoProXchC = new DerObjectIdentifier(GostID + ".33.3");

		//{ iso(1) member-body(2)ru(643) rans(2) cryptopro(2) ecc-signs(35) }
        public static readonly DerObjectIdentifier GostR3410x2001CryptoProA = new DerObjectIdentifier(GostID + ".35.1");
        public static readonly DerObjectIdentifier GostR3410x2001CryptoProB = new DerObjectIdentifier(GostID + ".35.2");
        public static readonly DerObjectIdentifier GostR3410x2001CryptoProC = new DerObjectIdentifier(GostID + ".35.3");

		// { iso(1) member-body(2) ru(643) rans(2) cryptopro(2) ecc-exchanges(36) }
        public static readonly DerObjectIdentifier GostR3410x2001CryptoProXchA  = new DerObjectIdentifier(GostID + ".36.0");
        public static readonly DerObjectIdentifier GostR3410x2001CryptoProXchB  = new DerObjectIdentifier(GostID + ".36.1");

		public static readonly DerObjectIdentifier GostElSgDH3410Default = new DerObjectIdentifier(GostID + ".36.0");
        public static readonly DerObjectIdentifier GostElSgDH3410x1 = new DerObjectIdentifier(GostID + ".36.1");
    }
}
