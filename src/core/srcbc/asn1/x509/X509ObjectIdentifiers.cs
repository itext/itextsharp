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
namespace Org.BouncyCastle.Asn1.X509
{
    public abstract class X509ObjectIdentifiers
    {
        //
        // base id
        //
        internal const string ID = "2.5.4";

		public static readonly DerObjectIdentifier CommonName              = new DerObjectIdentifier(ID + ".3");
        public static readonly DerObjectIdentifier CountryName             = new DerObjectIdentifier(ID + ".6");
        public static readonly DerObjectIdentifier LocalityName            = new DerObjectIdentifier(ID + ".7");
        public static readonly DerObjectIdentifier StateOrProvinceName     = new DerObjectIdentifier(ID + ".8");
        public static readonly DerObjectIdentifier Organization            = new DerObjectIdentifier(ID + ".10");
        public static readonly DerObjectIdentifier OrganizationalUnitName  = new DerObjectIdentifier(ID + ".11");

		public static readonly DerObjectIdentifier id_at_telephoneNumber   = new DerObjectIdentifier(ID + ".20");
		public static readonly DerObjectIdentifier id_at_name              = new DerObjectIdentifier(ID + ".41");

		// id-SHA1 OBJECT IDENTIFIER ::=
        //   {iso(1) identified-organization(3) oiw(14) secsig(3) algorithms(2) 26 }    //
        public static readonly DerObjectIdentifier IdSha1 = new DerObjectIdentifier("1.3.14.3.2.26");

		//
        // ripemd160 OBJECT IDENTIFIER ::=
        //      {iso(1) identified-organization(3) TeleTrust(36) algorithm(3) hashAlgorithm(2) RipeMD-160(1)}
        //
        public static readonly DerObjectIdentifier RipeMD160 = new DerObjectIdentifier("1.3.36.3.2.1");

		//
        // ripemd160WithRSAEncryption OBJECT IDENTIFIER ::=
        //      {iso(1) identified-organization(3) TeleTrust(36) algorithm(3) signatureAlgorithm(3) rsaSignature(1) rsaSignatureWithripemd160(2) }
        //
        public static readonly DerObjectIdentifier RipeMD160WithRsaEncryption = new DerObjectIdentifier("1.3.36.3.3.1.2");

		public static readonly DerObjectIdentifier IdEARsa = new DerObjectIdentifier("2.5.8.1.1");

		// id-pkix
		public static readonly DerObjectIdentifier IdPkix = new DerObjectIdentifier("1.3.6.1.5.5.7");

		//
		// private internet extensions
		//
		public static readonly DerObjectIdentifier IdPE = new DerObjectIdentifier(IdPkix + ".1");

		//
		// authority information access
		//
		public static readonly DerObjectIdentifier IdAD = new DerObjectIdentifier(IdPkix + ".48");
		public static readonly DerObjectIdentifier IdADCAIssuers = new DerObjectIdentifier(IdAD + ".2");
		public static readonly DerObjectIdentifier IdADOcsp = new DerObjectIdentifier(IdAD + ".1");

		//
		// OID for ocsp and crl uri in AuthorityInformationAccess extension
		//
		public static readonly DerObjectIdentifier OcspAccessMethod = IdADOcsp;
		public static readonly DerObjectIdentifier CrlAccessMethod = IdADCAIssuers;
    }
}
