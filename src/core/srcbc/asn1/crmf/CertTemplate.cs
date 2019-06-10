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

namespace Org.BouncyCastle.Asn1.Crmf
{
    public class CertTemplate
        : Asn1Encodable
    {
        private readonly Asn1Sequence seq;

        private readonly DerInteger version;
        private readonly DerInteger serialNumber;
        private readonly AlgorithmIdentifier signingAlg;
        private readonly X509Name issuer;
        private readonly OptionalValidity validity;
        private readonly X509Name subject;
        private readonly SubjectPublicKeyInfo publicKey;
        private readonly DerBitString issuerUID;
        private readonly DerBitString subjectUID;
        private readonly X509Extensions extensions;

        private CertTemplate(Asn1Sequence seq)
        {
            this.seq = seq;

            foreach (Asn1TaggedObject tObj in seq)
            {
                switch (tObj.TagNo)
                {
                case 0:
                    version = DerInteger.GetInstance(tObj, false);
                    break;
                case 1:
                    serialNumber = DerInteger.GetInstance(tObj, false);
                    break;
                case 2:
                    signingAlg = AlgorithmIdentifier.GetInstance(tObj, false);
                    break;
                case 3:
                    issuer = X509Name.GetInstance(tObj, true); // CHOICE
                    break;
                case 4:
                    validity = OptionalValidity.GetInstance(Asn1Sequence.GetInstance(tObj, false));
                    break;
                case 5:
                    subject = X509Name.GetInstance(tObj, true); // CHOICE
                    break;
                case 6:
                    publicKey = SubjectPublicKeyInfo.GetInstance(tObj, false);
                    break;
                case 7:
                    issuerUID = DerBitString.GetInstance(tObj, false);
                    break;
                case 8:
                    subjectUID = DerBitString.GetInstance(tObj, false);
                    break;
                case 9:
                    extensions = X509Extensions.GetInstance(tObj, false);
                    break;
                default:
                    throw new ArgumentException("unknown tag: " + tObj.TagNo, "seq");
                }
            }
        }

        public static CertTemplate GetInstance(object obj)
        {
            if (obj is CertTemplate)
                return (CertTemplate)obj;

            if (obj != null)
                return new CertTemplate(Asn1Sequence.GetInstance(obj));

            return null;
        }

        public virtual int Version
        {
            get { return version.Value.IntValue; }
        }

        public virtual DerInteger SerialNumber
        {
            get { return serialNumber; }
        }

        public virtual AlgorithmIdentifier SigningAlg
        {
            get { return signingAlg; }
        }

        public virtual X509Name Issuer
        {
            get { return issuer; }
        }

        public virtual OptionalValidity Validity
        {
            get { return validity; }
        }

        public virtual X509Name Subject
        {
            get { return subject; }
        }

        public virtual SubjectPublicKeyInfo PublicKey
        {
            get { return publicKey; }
        }

        public virtual DerBitString IssuerUID
        {
            get { return issuerUID; }
        }

        public virtual DerBitString SubjectUID
        {
            get { return subjectUID; }
        }

        public virtual X509Extensions Extensions
        {
            get { return extensions; }
        }

        /**
         * <pre>
         *  CertTemplate ::= SEQUENCE {
         *      version      [0] Version               OPTIONAL,
         *      serialNumber [1] INTEGER               OPTIONAL,
         *      signingAlg   [2] AlgorithmIdentifier   OPTIONAL,
         *      issuer       [3] Name                  OPTIONAL,
         *      validity     [4] OptionalValidity      OPTIONAL,
         *      subject      [5] Name                  OPTIONAL,
         *      publicKey    [6] SubjectPublicKeyInfo  OPTIONAL,
         *      issuerUID    [7] UniqueIdentifier      OPTIONAL,
         *      subjectUID   [8] UniqueIdentifier      OPTIONAL,
         *      extensions   [9] Extensions            OPTIONAL }
         * </pre>
         * @return a basic ASN.1 object representation.
         */
        public override Asn1Object ToAsn1Object()
        {
            return seq;
        }
    }
}
