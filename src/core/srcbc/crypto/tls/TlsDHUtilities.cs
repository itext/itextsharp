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
using System.IO;

using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls
{
    public abstract class TlsDHUtilities
    {
        public static byte[] CalculateDHBasicAgreement(DHPublicKeyParameters publicKey,
            DHPrivateKeyParameters privateKey)
        {
            DHBasicAgreement basicAgreement = new DHBasicAgreement();
            basicAgreement.Init(privateKey);
            BigInteger agreementValue = basicAgreement.CalculateAgreement(publicKey);

            /*
             * RFC 5246 8.1.2. Leading bytes of Z that contain all zero bits are stripped before it is
             * used as the pre_master_secret.
             */
            return BigIntegers.AsUnsignedByteArray(agreementValue);
        }

        public static AsymmetricCipherKeyPair GenerateDHKeyPair(SecureRandom random, DHParameters dhParams)
        {
            DHBasicKeyPairGenerator dhGen = new DHBasicKeyPairGenerator();
            dhGen.Init(new DHKeyGenerationParameters(random, dhParams));
            return dhGen.GenerateKeyPair();
        }

        public static DHPrivateKeyParameters GenerateEphemeralClientKeyExchange(SecureRandom random,
            DHParameters dhParams, Stream output)
        {
            AsymmetricCipherKeyPair dhAgreeClientKeyPair = GenerateDHKeyPair(random, dhParams);
            DHPrivateKeyParameters dhAgreeClientPrivateKey =
                (DHPrivateKeyParameters)dhAgreeClientKeyPair.Private;

            BigInteger Yc = ((DHPublicKeyParameters)dhAgreeClientKeyPair.Public).Y;
            byte[] keData = BigIntegers.AsUnsignedByteArray(Yc);
            TlsUtilities.WriteOpaque16(keData, output);

            return dhAgreeClientPrivateKey;
        }
        
        public static DHPublicKeyParameters ValidateDHPublicKey(DHPublicKeyParameters key)
        {
            BigInteger Y = key.Y;
            DHParameters parameters = key.Parameters;
            BigInteger p = parameters.P;
            BigInteger g = parameters.G;

            if (!p.IsProbablePrime(2))
            {
                throw new TlsFatalAlert(AlertDescription.illegal_parameter);
            }
            if (g.CompareTo(BigInteger.Two) < 0 || g.CompareTo(p.Subtract(BigInteger.Two)) > 0)
            {
                throw new TlsFatalAlert(AlertDescription.illegal_parameter);
            }
            if (Y.CompareTo(BigInteger.Two) < 0 || Y.CompareTo(p.Subtract(BigInteger.One)) > 0)
            {
                throw new TlsFatalAlert(AlertDescription.illegal_parameter);
            }

            // TODO See RFC 2631 for more discussion of Diffie-Hellman validation

            return key;
        }
    }
}