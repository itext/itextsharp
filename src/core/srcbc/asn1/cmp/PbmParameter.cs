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

using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Cmp
{
    public class PbmParameter
        : Asn1Encodable
    {
        private Asn1OctetString salt;
        private AlgorithmIdentifier owf;
        private DerInteger iterationCount;
        private AlgorithmIdentifier mac;

        private PbmParameter(Asn1Sequence seq)
        {
            salt = Asn1OctetString.GetInstance(seq[0]);
            owf = AlgorithmIdentifier.GetInstance(seq[1]);
            iterationCount = DerInteger.GetInstance(seq[2]);
            mac = AlgorithmIdentifier.GetInstance(seq[3]);
        }

        public static PbmParameter GetInstance(object obj)
        {
            if (obj is PbmParameter)
                return (PbmParameter)obj;

            if (obj is Asn1Sequence)
                return new PbmParameter((Asn1Sequence)obj);

            throw new ArgumentException("Invalid object: " + obj.GetType().Name, "obj");
        }

        public PbmParameter(
            byte[] salt,
            AlgorithmIdentifier owf,
            int iterationCount,
            AlgorithmIdentifier mac)
            : this(new DerOctetString(salt), owf, new DerInteger(iterationCount), mac)
        {
        }

        public PbmParameter(
            Asn1OctetString salt,
            AlgorithmIdentifier owf,
            DerInteger iterationCount,
            AlgorithmIdentifier mac)
        {
            this.salt = salt;
            this.owf = owf;
            this.iterationCount = iterationCount;
            this.mac = mac;
        }

        public virtual Asn1OctetString Salt
        {
            get { return salt; }
        }

        public virtual AlgorithmIdentifier Owf
        {
            get { return owf; }
        }

        public virtual DerInteger IterationCount
        {
            get { return iterationCount; }
        }

        public virtual AlgorithmIdentifier Mac
        {
            get { return mac; }
        }

        /**
         * <pre>
         *  PbmParameter ::= SEQUENCE {
         *                        salt                OCTET STRING,
         *                        -- note:  implementations MAY wish to limit acceptable sizes
         *                        -- of this string to values appropriate for their environment
         *                        -- in order to reduce the risk of denial-of-service attacks
         *                        owf                 AlgorithmIdentifier,
         *                        -- AlgId for a One-Way Function (SHA-1 recommended)
         *                        iterationCount      INTEGER,
         *                        -- number of times the OWF is applied
         *                        -- note:  implementations MAY wish to limit acceptable sizes
         *                        -- of this integer to values appropriate for their environment
         *                        -- in order to reduce the risk of denial-of-service attacks
         *                        mac                 AlgorithmIdentifier
         *                        -- the MAC AlgId (e.g., DES-MAC, Triple-DES-MAC [PKCS11],
         *    }   -- or HMAC [RFC2104, RFC2202])
         * </pre>
         * @return a basic ASN.1 object representation.
         */
        public override Asn1Object ToAsn1Object()
        {
            return new DerSequence(salt, owf, iterationCount, mac);
        }
    }
}
