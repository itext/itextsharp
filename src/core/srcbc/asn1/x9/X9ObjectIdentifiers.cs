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

namespace Org.BouncyCastle.Asn1.X9
{
    public abstract class X9ObjectIdentifiers
    {
        //
        // X9.62
        //
        // ansi-X9-62 OBJECT IDENTIFIER ::= { iso(1) member-body(2)
        //            us(840) ansi-x962(10045) }
        //

        internal const string AnsiX962 = "1.2.840.10045";

        public static readonly DerObjectIdentifier ansi_X9_62 = new DerObjectIdentifier(AnsiX962);

        public static readonly DerObjectIdentifier IdFieldType = ansi_X9_62.Branch("1");

        public static readonly DerObjectIdentifier PrimeField = IdFieldType.Branch("1");
        public static readonly DerObjectIdentifier CharacteristicTwoField = IdFieldType.Branch("2");

        public static readonly DerObjectIdentifier GNBasis = CharacteristicTwoField.Branch("3.1");
        public static readonly DerObjectIdentifier TPBasis = CharacteristicTwoField.Branch("3.2");
        public static readonly DerObjectIdentifier PPBasis = CharacteristicTwoField.Branch("3.3");

        [Obsolete("Use 'id_ecSigType' instead")]
        public const string IdECSigType = AnsiX962 + ".4";
        public static readonly DerObjectIdentifier id_ecSigType = ansi_X9_62.Branch("4");

        public static readonly DerObjectIdentifier ECDsaWithSha1 = id_ecSigType.Branch("1");

        [Obsolete("Use 'id_publicKeyType' instead")]
        public const string IdPublicKeyType = AnsiX962 + ".2";
        public static readonly DerObjectIdentifier id_publicKeyType = ansi_X9_62.Branch("2");

        public static readonly DerObjectIdentifier IdECPublicKey = id_publicKeyType.Branch("1");

        public static readonly DerObjectIdentifier ECDsaWithSha2 = id_ecSigType.Branch("3");

        public static readonly DerObjectIdentifier ECDsaWithSha224 = ECDsaWithSha2.Branch("1");
        public static readonly DerObjectIdentifier ECDsaWithSha256 = ECDsaWithSha2.Branch("2");
        public static readonly DerObjectIdentifier ECDsaWithSha384 = ECDsaWithSha2.Branch("3");
        public static readonly DerObjectIdentifier ECDsaWithSha512 = ECDsaWithSha2.Branch("4");


        //
        // named curves
        //
        public static readonly DerObjectIdentifier EllipticCurve = ansi_X9_62.Branch("3");

        //
        // Two Curves
        //
        public static readonly DerObjectIdentifier CTwoCurve = EllipticCurve.Branch("0");

        public static readonly DerObjectIdentifier C2Pnb163v1 = CTwoCurve.Branch("1");
        public static readonly DerObjectIdentifier C2Pnb163v2 = CTwoCurve.Branch("2");
        public static readonly DerObjectIdentifier C2Pnb163v3 = CTwoCurve.Branch("3");
        public static readonly DerObjectIdentifier C2Pnb176w1 = CTwoCurve.Branch("4");
        public static readonly DerObjectIdentifier C2Tnb191v1 = CTwoCurve.Branch("5");
        public static readonly DerObjectIdentifier C2Tnb191v2 = CTwoCurve.Branch("6");
        public static readonly DerObjectIdentifier C2Tnb191v3 = CTwoCurve.Branch("7");
        public static readonly DerObjectIdentifier C2Onb191v4 = CTwoCurve.Branch("8");
        public static readonly DerObjectIdentifier C2Onb191v5 = CTwoCurve.Branch("9");
        public static readonly DerObjectIdentifier C2Pnb208w1 = CTwoCurve.Branch("10");
        public static readonly DerObjectIdentifier C2Tnb239v1 = CTwoCurve.Branch("11");
        public static readonly DerObjectIdentifier C2Tnb239v2 = CTwoCurve.Branch("12");
        public static readonly DerObjectIdentifier C2Tnb239v3 = CTwoCurve.Branch("13");
        public static readonly DerObjectIdentifier C2Onb239v4 = CTwoCurve.Branch("14");
        public static readonly DerObjectIdentifier C2Onb239v5 = CTwoCurve.Branch("15");
        public static readonly DerObjectIdentifier C2Pnb272w1 = CTwoCurve.Branch("16");
        public static readonly DerObjectIdentifier C2Pnb304w1 = CTwoCurve.Branch("17");
        public static readonly DerObjectIdentifier C2Tnb359v1 = CTwoCurve.Branch("18");
        public static readonly DerObjectIdentifier C2Pnb368w1 = CTwoCurve.Branch("19");
        public static readonly DerObjectIdentifier C2Tnb431r1 = CTwoCurve.Branch("20");

        //
        // Prime
        //
        public static readonly DerObjectIdentifier PrimeCurve = EllipticCurve.Branch("1");

        public static readonly DerObjectIdentifier Prime192v1 = PrimeCurve.Branch("1");
        public static readonly DerObjectIdentifier Prime192v2 = PrimeCurve.Branch("2");
        public static readonly DerObjectIdentifier Prime192v3 = PrimeCurve.Branch("3");
        public static readonly DerObjectIdentifier Prime239v1 = PrimeCurve.Branch("4");
        public static readonly DerObjectIdentifier Prime239v2 = PrimeCurve.Branch("5");
        public static readonly DerObjectIdentifier Prime239v3 = PrimeCurve.Branch("6");
        public static readonly DerObjectIdentifier Prime256v1 = PrimeCurve.Branch("7");

        //
        // DSA
        //
        // dsapublicnumber OBJECT IDENTIFIER ::= { iso(1) member-body(2)
        //            us(840) ansi-x957(10040) number-type(4) 1 }
        public static readonly DerObjectIdentifier IdDsa = new DerObjectIdentifier("1.2.840.10040.4.1");

        /**
         *   id-dsa-with-sha1 OBJECT IDENTIFIER ::=  { iso(1) member-body(2)
         *         us(840) x9-57 (10040) x9cm(4) 3 }
         */
        public static readonly DerObjectIdentifier IdDsaWithSha1 = new DerObjectIdentifier("1.2.840.10040.4.3");

        /**
         * X9.63
         */
        public static readonly DerObjectIdentifier X9x63Scheme = new DerObjectIdentifier("1.3.133.16.840.63.0");
        public static readonly DerObjectIdentifier DHSinglePassStdDHSha1KdfScheme = X9x63Scheme.Branch("2");
        public static readonly DerObjectIdentifier DHSinglePassCofactorDHSha1KdfScheme = X9x63Scheme.Branch("3");
        public static readonly DerObjectIdentifier MqvSinglePassSha1KdfScheme = X9x63Scheme.Branch("16");

        /**
         * X9.42
         */

        public static readonly DerObjectIdentifier ansi_x9_42 = new DerObjectIdentifier("1.2.840.10046");

        //
        // Diffie-Hellman
        //
        // dhpublicnumber OBJECT IDENTIFIER ::= { iso(1) member-body(2)
        //            us(840) ansi-x942(10046) number-type(2) 1 }
        //
        public static readonly DerObjectIdentifier DHPublicNumber = ansi_x9_42.Branch("2.1");

        public static readonly DerObjectIdentifier X9x42Schemes = ansi_x9_42.Branch("2.3");

        public static readonly DerObjectIdentifier DHStatic = X9x42Schemes.Branch("1");
        public static readonly DerObjectIdentifier DHEphem = X9x42Schemes.Branch("2");
        public static readonly DerObjectIdentifier DHOneFlow = X9x42Schemes.Branch("3");
        public static readonly DerObjectIdentifier DHHybrid1 = X9x42Schemes.Branch("4");
        public static readonly DerObjectIdentifier DHHybrid2 = X9x42Schemes.Branch("5");
        public static readonly DerObjectIdentifier DHHybridOneFlow = X9x42Schemes.Branch("6");
        public static readonly DerObjectIdentifier Mqv2 = X9x42Schemes.Branch("7");
        public static readonly DerObjectIdentifier Mqv1 = X9x42Schemes.Branch("8");
    }
}
