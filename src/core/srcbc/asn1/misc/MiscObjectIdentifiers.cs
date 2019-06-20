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
using Org.BouncyCastle.Asn1;

namespace Org.BouncyCastle.Asn1.Misc
{
    public abstract class MiscObjectIdentifiers
    {
        //
        // Netscape
        //       iso/itu(2) joint-assign(16) us(840) uscompany(1) Netscape(113730) cert-extensions(1) }
        //
        public static readonly DerObjectIdentifier Netscape                = new DerObjectIdentifier("2.16.840.1.113730.1");
        public static readonly DerObjectIdentifier NetscapeCertType        = new DerObjectIdentifier(Netscape + ".1");
        public static readonly DerObjectIdentifier NetscapeBaseUrl         = new DerObjectIdentifier(Netscape + ".2");
        public static readonly DerObjectIdentifier NetscapeRevocationUrl   = new DerObjectIdentifier(Netscape + ".3");
        public static readonly DerObjectIdentifier NetscapeCARevocationUrl = new DerObjectIdentifier(Netscape + ".4");
        public static readonly DerObjectIdentifier NetscapeRenewalUrl      = new DerObjectIdentifier(Netscape + ".7");
        public static readonly DerObjectIdentifier NetscapeCAPolicyUrl     = new DerObjectIdentifier(Netscape + ".8");
        public static readonly DerObjectIdentifier NetscapeSslServerName   = new DerObjectIdentifier(Netscape + ".12");
        public static readonly DerObjectIdentifier NetscapeCertComment     = new DerObjectIdentifier(Netscape + ".13");
        //
        // Verisign
        //       iso/itu(2) joint-assign(16) us(840) uscompany(1) verisign(113733) cert-extensions(1) }
        //
        internal const string Verisign = "2.16.840.1.113733.1";

		//
        // CZAG - country, zip, age, and gender
        //
        public static readonly DerObjectIdentifier VerisignCzagExtension = new DerObjectIdentifier(Verisign + ".6.3");

		// D&B D-U-N-S number
		public static readonly DerObjectIdentifier VerisignDnbDunsNumber = new DerObjectIdentifier(Verisign + ".6.15");

		//
		// Novell
		//       iso/itu(2) country(16) us(840) organization(1) novell(113719)
		//
		public static readonly string				Novell					= "2.16.840.1.113719";
		public static readonly DerObjectIdentifier	NovellSecurityAttribs	= new DerObjectIdentifier(Novell + ".1.9.4.1");

		//
		// Entrust
		//       iso(1) member-body(16) us(840) nortelnetworks(113533) entrust(7)
		//
		public static readonly string				Entrust					= "1.2.840.113533.7";
		public static readonly DerObjectIdentifier	EntrustVersionExtension = new DerObjectIdentifier(Entrust + ".65.0");
	}
}
