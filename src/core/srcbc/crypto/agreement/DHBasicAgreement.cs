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

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Crypto.Agreement
{
    /**
     * a Diffie-Hellman key agreement class.
     * <p>
     * note: This is only the basic algorithm, it doesn't take advantage of
     * long term public keys if they are available. See the DHAgreement class
     * for a "better" implementation.</p>
     */
    public class DHBasicAgreement
        : IBasicAgreement
    {
        private DHPrivateKeyParameters	key;
        private DHParameters			dhParams;

        public virtual void Init(
            ICipherParameters parameters)
        {
            if (parameters is ParametersWithRandom)
            {
                parameters = ((ParametersWithRandom) parameters).Parameters;
            }

            if (!(parameters is DHPrivateKeyParameters))
            {
                throw new ArgumentException("DHEngine expects DHPrivateKeyParameters");
            }

            this.key = (DHPrivateKeyParameters) parameters;
            this.dhParams = key.Parameters;
        }

        public virtual int GetFieldSize()
        {
            return (key.Parameters.P.BitLength + 7) / 8;
        }

        /**
         * given a short term public key from a given party calculate the next
         * message in the agreement sequence.
         */
        public virtual BigInteger CalculateAgreement(
            ICipherParameters pubKey)
        {
            if (this.key == null)
                throw new InvalidOperationException("Agreement algorithm not initialised");

            DHPublicKeyParameters pub = (DHPublicKeyParameters)pubKey;

            if (!pub.Parameters.Equals(dhParams))
            {
                throw new ArgumentException("Diffie-Hellman public key has wrong parameters.");
            }

            return pub.Y.ModPow(key.X, dhParams.P);
        }
    }
}
