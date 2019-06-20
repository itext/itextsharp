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
using System.Text;

namespace Org.BouncyCastle.Asn1.X509
{
    public class CertificatePolicies
        : Asn1Encodable
    {
        private readonly PolicyInformation[] policyInformation;

        public static CertificatePolicies GetInstance(object obj)
        {
            if (obj == null || obj is CertificatePolicies)
                return (CertificatePolicies)obj;

            return new CertificatePolicies(Asn1Sequence.GetInstance(obj));
        }

        public static CertificatePolicies GetInstance(Asn1TaggedObject obj, bool isExplicit)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
        }

        /**
         * Construct a CertificatePolicies object containing one PolicyInformation.
         * 
         * @param name the name to be contained.
         */
        public CertificatePolicies(PolicyInformation name)
        {
            this.policyInformation = new PolicyInformation[] { name };
        }

        public CertificatePolicies(PolicyInformation[] policyInformation)
        {
            this.policyInformation = policyInformation;
        }

        private CertificatePolicies(Asn1Sequence seq)
        {
            this.policyInformation = new PolicyInformation[seq.Count];

            for (int i = 0; i < seq.Count; ++i)
            {
                policyInformation[i] = PolicyInformation.GetInstance(seq[i]);
            }
        }

        public virtual PolicyInformation[] GetPolicyInformation()
        {
            return (PolicyInformation[])policyInformation.Clone();
        }

        /**
         * Produce an object suitable for an ASN1OutputStream.
         * <pre>
         * CertificatePolicies ::= SEQUENCE SIZE {1..MAX} OF PolicyInformation
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            return new DerSequence(policyInformation);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("CertificatePolicies:");
            if (policyInformation != null && policyInformation.Length > 0)
            {
                sb.Append(' ');
                sb.Append(policyInformation[0]);
                for (int i = 1; i < policyInformation.Length; ++i)
                {
                    sb.Append(", ");
                    sb.Append(policyInformation[i]);
                }
            }
            return sb.ToString();
        }
    }
}
