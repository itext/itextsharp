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

namespace Org.BouncyCastle.Asn1.Eac
{
	public abstract class EacObjectIdentifiers
	{
		// bsi-de OBJECT IDENTIFIER ::= {
		//         itu-t(0) identified-organization(4) etsi(0)
		//         reserved(127) etsi-identified-organization(0) 7
		//     }
		public static readonly DerObjectIdentifier bsi_de = new DerObjectIdentifier("0.4.0.127.0.7");

		// id-PK OBJECT IDENTIFIER ::= {
		//         bsi-de protocols(2) smartcard(2) 1
		//     }
		public static readonly DerObjectIdentifier id_PK = new DerObjectIdentifier(bsi_de + ".2.2.1");

		public static readonly DerObjectIdentifier id_PK_DH = new DerObjectIdentifier(id_PK + ".1");
		public static readonly DerObjectIdentifier id_PK_ECDH = new DerObjectIdentifier(id_PK + ".2");

		// id-CA OBJECT IDENTIFIER ::= {
		//         bsi-de protocols(2) smartcard(2) 3
		//     }
		public static readonly DerObjectIdentifier id_CA = new DerObjectIdentifier(bsi_de + ".2.2.3");
		public static readonly DerObjectIdentifier id_CA_DH = new DerObjectIdentifier(id_CA + ".1");
		public static readonly DerObjectIdentifier id_CA_DH_3DES_CBC_CBC = new DerObjectIdentifier(id_CA_DH + ".1");
		public static readonly DerObjectIdentifier id_CA_ECDH = new DerObjectIdentifier(id_CA + ".2");
		public static readonly DerObjectIdentifier id_CA_ECDH_3DES_CBC_CBC = new DerObjectIdentifier(id_CA_ECDH + ".1");

		//
		// id-TA OBJECT IDENTIFIER ::= {
		//     bsi-de protocols(2) smartcard(2) 2
		// }
		public static readonly DerObjectIdentifier id_TA = new DerObjectIdentifier(bsi_de + ".2.2.2");

		public static readonly DerObjectIdentifier id_TA_RSA = new DerObjectIdentifier(id_TA + ".1");
		public static readonly DerObjectIdentifier id_TA_RSA_v1_5_SHA_1 = new DerObjectIdentifier(id_TA_RSA + ".1");
		public static readonly DerObjectIdentifier id_TA_RSA_v1_5_SHA_256 = new DerObjectIdentifier(id_TA_RSA + ".2");
		public static readonly DerObjectIdentifier id_TA_RSA_PSS_SHA_1 = new DerObjectIdentifier(id_TA_RSA + ".3");
		public static readonly DerObjectIdentifier id_TA_RSA_PSS_SHA_256 = new DerObjectIdentifier(id_TA_RSA + ".4");
		public static readonly DerObjectIdentifier id_TA_ECDSA = new DerObjectIdentifier(id_TA + ".2");
		public static readonly DerObjectIdentifier id_TA_ECDSA_SHA_1 = new DerObjectIdentifier(id_TA_ECDSA + ".1");
		public static readonly DerObjectIdentifier id_TA_ECDSA_SHA_224 = new DerObjectIdentifier(id_TA_ECDSA + ".2");
		public static readonly DerObjectIdentifier id_TA_ECDSA_SHA_256 = new DerObjectIdentifier(id_TA_ECDSA + ".3");
		public static readonly DerObjectIdentifier id_TA_ECDSA_SHA_384 = new DerObjectIdentifier(id_TA_ECDSA + ".4");
		public static readonly DerObjectIdentifier id_TA_ECDSA_SHA_512 = new DerObjectIdentifier(id_TA_ECDSA + ".5");
	}
}
