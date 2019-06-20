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
namespace Org.BouncyCastle.Asn1.Oiw
{
	public abstract class OiwObjectIdentifiers
	{
		public static readonly DerObjectIdentifier MD4WithRsa			= new DerObjectIdentifier("1.3.14.3.2.2");
		public static readonly DerObjectIdentifier MD5WithRsa			= new DerObjectIdentifier("1.3.14.3.2.3");
		public static readonly DerObjectIdentifier MD4WithRsaEncryption	= new DerObjectIdentifier("1.3.14.3.2.4");

		public static readonly DerObjectIdentifier DesEcb				= new DerObjectIdentifier("1.3.14.3.2.6");
		public static readonly DerObjectIdentifier DesCbc				= new DerObjectIdentifier("1.3.14.3.2.7");
		public static readonly DerObjectIdentifier DesOfb				= new DerObjectIdentifier("1.3.14.3.2.8");
		public static readonly DerObjectIdentifier DesCfb				= new DerObjectIdentifier("1.3.14.3.2.9");

		public static readonly DerObjectIdentifier DesEde				= new DerObjectIdentifier("1.3.14.3.2.17");

		// id-SHA1 OBJECT IDENTIFIER ::=
		//   {iso(1) identified-organization(3) oiw(14) secsig(3) algorithms(2) 26 }    //
		public static readonly DerObjectIdentifier IdSha1				= new DerObjectIdentifier("1.3.14.3.2.26");

		public static readonly DerObjectIdentifier DsaWithSha1			= new DerObjectIdentifier("1.3.14.3.2.27");

		public static readonly DerObjectIdentifier Sha1WithRsa			= new DerObjectIdentifier("1.3.14.3.2.29");

		// ElGamal Algorithm OBJECT IDENTIFIER ::=
		// {iso(1) identified-organization(3) oiw(14) dirservsig(7) algorithm(2) encryption(1) 1 }
		//
		public static readonly DerObjectIdentifier ElGamalAlgorithm		= new DerObjectIdentifier("1.3.14.7.2.1.1");
	}
}
