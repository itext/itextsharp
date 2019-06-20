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
namespace Org.BouncyCastle.Asn1.TeleTrust
{
	public sealed class TeleTrusTObjectIdentifiers
	{
		private TeleTrusTObjectIdentifiers()
		{
		}

		public static readonly DerObjectIdentifier TeleTrusTAlgorithm = new DerObjectIdentifier("1.3.36.3");

		public static readonly DerObjectIdentifier RipeMD160 = new DerObjectIdentifier(TeleTrusTAlgorithm + ".2.1");
		public static readonly DerObjectIdentifier RipeMD128 = new DerObjectIdentifier(TeleTrusTAlgorithm + ".2.2");
		public static readonly DerObjectIdentifier RipeMD256 = new DerObjectIdentifier(TeleTrusTAlgorithm + ".2.3");

		public static readonly DerObjectIdentifier TeleTrusTRsaSignatureAlgorithm = new DerObjectIdentifier(TeleTrusTAlgorithm + ".3.1");

		public static readonly DerObjectIdentifier RsaSignatureWithRipeMD160 = new DerObjectIdentifier(TeleTrusTRsaSignatureAlgorithm + ".2");
		public static readonly DerObjectIdentifier RsaSignatureWithRipeMD128 = new DerObjectIdentifier(TeleTrusTRsaSignatureAlgorithm + ".3");
		public static readonly DerObjectIdentifier RsaSignatureWithRipeMD256 = new DerObjectIdentifier(TeleTrusTRsaSignatureAlgorithm + ".4");

		public static readonly DerObjectIdentifier ECSign = new DerObjectIdentifier(TeleTrusTAlgorithm + ".3.2");

		public static readonly DerObjectIdentifier ECSignWithSha1 = new DerObjectIdentifier(ECSign + ".1");
		public static readonly DerObjectIdentifier ECSignWithRipeMD160 = new DerObjectIdentifier(ECSign + ".2");

		public static readonly DerObjectIdentifier EccBrainpool = new DerObjectIdentifier(TeleTrusTAlgorithm + ".3.2.8");
		public static readonly DerObjectIdentifier EllipticCurve = new DerObjectIdentifier(EccBrainpool + ".1");
		public static readonly DerObjectIdentifier VersionOne = new DerObjectIdentifier(EllipticCurve + ".1");    

		public static readonly DerObjectIdentifier BrainpoolP160R1 = new DerObjectIdentifier(VersionOne + ".1");
		public static readonly DerObjectIdentifier BrainpoolP160T1 = new DerObjectIdentifier(VersionOne + ".2");
		public static readonly DerObjectIdentifier BrainpoolP192R1 = new DerObjectIdentifier(VersionOne + ".3");
		public static readonly DerObjectIdentifier BrainpoolP192T1 = new DerObjectIdentifier(VersionOne + ".4");
		public static readonly DerObjectIdentifier BrainpoolP224R1 = new DerObjectIdentifier(VersionOne + ".5");
		public static readonly DerObjectIdentifier BrainpoolP224T1 = new DerObjectIdentifier(VersionOne + ".6");
		public static readonly DerObjectIdentifier BrainpoolP256R1 = new DerObjectIdentifier(VersionOne + ".7");
		public static readonly DerObjectIdentifier BrainpoolP256T1 = new DerObjectIdentifier(VersionOne + ".8");
		public static readonly DerObjectIdentifier BrainpoolP320R1 = new DerObjectIdentifier(VersionOne + ".9");
		public static readonly DerObjectIdentifier BrainpoolP320T1 = new DerObjectIdentifier(VersionOne + ".10");
		public static readonly DerObjectIdentifier BrainpoolP384R1 = new DerObjectIdentifier(VersionOne + ".11");
		public static readonly DerObjectIdentifier BrainpoolP384T1 = new DerObjectIdentifier(VersionOne + ".12");
		public static readonly DerObjectIdentifier BrainpoolP512R1 = new DerObjectIdentifier(VersionOne + ".13");
		public static readonly DerObjectIdentifier BrainpoolP512T1 = new DerObjectIdentifier(VersionOne + ".14");
	}
}
