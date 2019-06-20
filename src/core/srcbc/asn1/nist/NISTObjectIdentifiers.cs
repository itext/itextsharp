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

namespace Org.BouncyCastle.Asn1.Nist
{
    public sealed class NistObjectIdentifiers
    {
        private NistObjectIdentifiers()
        {
        }

        //
        // NIST
        //     iso/itu(2) joint-assign(16) us(840) organization(1) gov(101) csor(3)

        //
        // nistalgorithms(4)
        //
        public static readonly DerObjectIdentifier NistAlgorithm = new DerObjectIdentifier("2.16.840.1.101.3.4");

        public static readonly DerObjectIdentifier HashAlgs = NistAlgorithm.Branch("2");

        public static readonly DerObjectIdentifier IdSha256 = HashAlgs.Branch("1");
        public static readonly DerObjectIdentifier IdSha384 = HashAlgs.Branch("2");
        public static readonly DerObjectIdentifier IdSha512 = HashAlgs.Branch("3");
        public static readonly DerObjectIdentifier IdSha224 = HashAlgs.Branch("4");
        public static readonly DerObjectIdentifier IdSha512_224 = HashAlgs.Branch("5");
        public static readonly DerObjectIdentifier IdSha512_256 = HashAlgs.Branch("6");

        public static readonly DerObjectIdentifier Aes = new DerObjectIdentifier(NistAlgorithm + ".1");

        public static readonly DerObjectIdentifier IdAes128Ecb	= new DerObjectIdentifier(Aes + ".1");
        public static readonly DerObjectIdentifier IdAes128Cbc	= new DerObjectIdentifier(Aes + ".2");
        public static readonly DerObjectIdentifier IdAes128Ofb	= new DerObjectIdentifier(Aes + ".3");
        public static readonly DerObjectIdentifier IdAes128Cfb	= new DerObjectIdentifier(Aes + ".4");
        public static readonly DerObjectIdentifier IdAes128Wrap	= new DerObjectIdentifier(Aes + ".5");
        public static readonly DerObjectIdentifier IdAes128Gcm	= new DerObjectIdentifier(Aes + ".6");
        public static readonly DerObjectIdentifier IdAes128Ccm	= new DerObjectIdentifier(Aes + ".7");

        public static readonly DerObjectIdentifier IdAes192Ecb	= new DerObjectIdentifier(Aes + ".21");
        public static readonly DerObjectIdentifier IdAes192Cbc	= new DerObjectIdentifier(Aes + ".22");
        public static readonly DerObjectIdentifier IdAes192Ofb	= new DerObjectIdentifier(Aes + ".23");
        public static readonly DerObjectIdentifier IdAes192Cfb	= new DerObjectIdentifier(Aes + ".24");
        public static readonly DerObjectIdentifier IdAes192Wrap	= new DerObjectIdentifier(Aes + ".25");
        public static readonly DerObjectIdentifier IdAes192Gcm	= new DerObjectIdentifier(Aes + ".26");
        public static readonly DerObjectIdentifier IdAes192Ccm	= new DerObjectIdentifier(Aes + ".27");

        public static readonly DerObjectIdentifier IdAes256Ecb	= new DerObjectIdentifier(Aes + ".41");
        public static readonly DerObjectIdentifier IdAes256Cbc	= new DerObjectIdentifier(Aes + ".42");
        public static readonly DerObjectIdentifier IdAes256Ofb	= new DerObjectIdentifier(Aes + ".43");
        public static readonly DerObjectIdentifier IdAes256Cfb	= new DerObjectIdentifier(Aes + ".44");
        public static readonly DerObjectIdentifier IdAes256Wrap	= new DerObjectIdentifier(Aes + ".45");
        public static readonly DerObjectIdentifier IdAes256Gcm	= new DerObjectIdentifier(Aes + ".46");
        public static readonly DerObjectIdentifier IdAes256Ccm	= new DerObjectIdentifier(Aes + ".47");

        //
        // signatures
        //
        public static readonly DerObjectIdentifier IdDsaWithSha2 = new DerObjectIdentifier(NistAlgorithm + ".3");

        public static readonly DerObjectIdentifier DsaWithSha224 = new DerObjectIdentifier(IdDsaWithSha2 + ".1");
        public static readonly DerObjectIdentifier DsaWithSha256 = new DerObjectIdentifier(IdDsaWithSha2 + ".2");
        public static readonly DerObjectIdentifier DsaWithSha384 = new DerObjectIdentifier(IdDsaWithSha2 + ".3");
        public static readonly DerObjectIdentifier DsaWithSha512 = new DerObjectIdentifier(IdDsaWithSha2 + ".4"); 
    }
}
