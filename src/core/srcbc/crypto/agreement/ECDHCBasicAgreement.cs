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
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace Org.BouncyCastle.Crypto.Agreement
{
    /**
     * P1363 7.2.2 ECSVDP-DHC
     *
     * ECSVDP-DHC is Elliptic Curve Secret Value Derivation Primitive,
     * Diffie-Hellman version with cofactor multiplication. It is based on
     * the work of [DH76], [Mil86], [Kob87], [LMQ98] and [Kal98a]. This
     * primitive derives a shared secret value from one party's private key
     * and another party's public key, where both have the same set of EC
     * domain parameters. If two parties correctly execute this primitive,
     * they will produce the same output. This primitive can be invoked by a
     * scheme to derive a shared secret key; specifically, it may be used
     * with the schemes ECKAS-DH1 and DL/ECKAS-DH2. It does not assume the
     * validity of the input public key (see also Section 7.2.1).
     * <p>
     * Note: As stated P1363 compatibility mode with ECDH can be preset, and
     * in this case the implementation doesn't have a ECDH compatibility mode
     * (if you want that just use ECDHBasicAgreement and note they both implement
     * BasicAgreement!).</p>
     */
    public class ECDHCBasicAgreement
        : IBasicAgreement
    {
        private ECPrivateKeyParameters key;

        public virtual void Init(
            ICipherParameters parameters)
        {
            if (parameters is ParametersWithRandom)
            {
                parameters = ((ParametersWithRandom) parameters).Parameters;
            }

            this.key = (ECPrivateKeyParameters)parameters;
        }

        public virtual int GetFieldSize()
        {
            return (key.Parameters.Curve.FieldSize + 7) / 8;
        }

        public virtual BigInteger CalculateAgreement(
            ICipherParameters pubKey)
        {
            ECPublicKeyParameters pub = (ECPublicKeyParameters) pubKey;
            ECDomainParameters parameters = pub.Parameters;
            ECPoint P = pub.Q.Multiply(parameters.H.Multiply(key.D));

            // if ( p.IsInfinity ) throw new Exception("Invalid public key");

            return P.X.ToBigInteger();
        }
    }
}
