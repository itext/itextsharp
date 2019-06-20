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

namespace Org.BouncyCastle.Asn1.Ess
{
    public class SigningCertificateV2
        : Asn1Encodable
    {
        private readonly Asn1Sequence certs;
        private readonly Asn1Sequence policies;

        public static SigningCertificateV2 GetInstance(
            object o)
        {
            if (o == null || o is SigningCertificateV2)
                return (SigningCertificateV2) o;

            if (o is Asn1Sequence)
                return new SigningCertificateV2((Asn1Sequence) o);

            throw new ArgumentException(
                "unknown object in 'SigningCertificateV2' factory : "
                + o.GetType().Name + ".");
        }

        private SigningCertificateV2(
            Asn1Sequence seq)
        {
            if (seq.Count < 1 || seq.Count > 2)
                throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");

            this.certs = Asn1Sequence.GetInstance(seq[0].ToAsn1Object());

            if (seq.Count > 1)
            {
                this.policies = Asn1Sequence.GetInstance(seq[1].ToAsn1Object());
            }
        }

        public SigningCertificateV2(
            EssCertIDv2 cert)
        {
            this.certs = new DerSequence(cert);
        }

        public SigningCertificateV2(
            EssCertIDv2[] certs)
        {
            this.certs = new DerSequence(certs);
        }

        public SigningCertificateV2(
            EssCertIDv2[]		certs,
            PolicyInformation[]	policies)
        {
            this.certs = new DerSequence(certs);

            if (policies != null)
            {
                this.policies = new DerSequence(policies);
            }
        }

        public EssCertIDv2[] GetCerts()
        {
            EssCertIDv2[] certIds = new EssCertIDv2[certs.Count];
            for (int i = 0; i != certs.Count; i++)
            {
                certIds[i] = EssCertIDv2.GetInstance(certs[i]);
            }
            return certIds;
        }

        public PolicyInformation[] GetPolicies()
        {
            if (policies == null)
                return null;

            PolicyInformation[] policyInformations = new PolicyInformation[policies.Count];
            for (int i = 0; i != policies.Count; i++)
            {
                policyInformations[i] = PolicyInformation.GetInstance(policies[i]);
            }
            return policyInformations;
        }

        /**
         * The definition of SigningCertificateV2 is
         * <pre>
         * SigningCertificateV2 ::=  SEQUENCE {
         *      certs        SEQUENCE OF EssCertIDv2,
         *      policies     SEQUENCE OF PolicyInformation OPTIONAL
         * }
         * </pre>
         * id-aa-signingCertificateV2 OBJECT IDENTIFIER ::= { iso(1)
         *    member-body(2) us(840) rsadsi(113549) pkcs(1) pkcs9(9)
         *    smime(16) id-aa(2) 47 }
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(certs);

            if (policies != null)
            {
                v.Add(policies);
            }

            return new DerSequence(v);
        }
    }
}
