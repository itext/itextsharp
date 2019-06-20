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
    public class CertTemplateBuilder
    {
        private DerInteger version;
        private DerInteger serialNumber;
        private AlgorithmIdentifier signingAlg;
        private X509Name issuer;
        private OptionalValidity validity;
        private X509Name subject;
        private SubjectPublicKeyInfo publicKey;
        private DerBitString issuerUID;
        private DerBitString subjectUID;
        private X509Extensions extensions;

        /** Sets the X.509 version. Note: for X509v3, use 2 here. */
        public virtual CertTemplateBuilder SetVersion(int ver)
        {
            version = new DerInteger(ver);
            return this;
        }

        public virtual CertTemplateBuilder SetSerialNumber(DerInteger ser)
        {
            serialNumber = ser;
            return this;
        }

        public virtual CertTemplateBuilder SetSigningAlg(AlgorithmIdentifier aid)
        {
            signingAlg = aid;
            return this;
        }

        public virtual CertTemplateBuilder SetIssuer(X509Name name)
        {
            issuer = name;
            return this;
        }

        public virtual CertTemplateBuilder SetValidity(OptionalValidity v)
        {
            validity = v;
            return this;
        }

        public virtual CertTemplateBuilder SetSubject(X509Name name)
        {
            subject = name;
            return this;
        }

        public virtual CertTemplateBuilder SetPublicKey(SubjectPublicKeyInfo spki)
        {
            publicKey = spki;
            return this;
        }

        /** Sets the issuer unique ID (deprecated in X.509v3) */
        public virtual CertTemplateBuilder SetIssuerUID(DerBitString uid)
        {
            issuerUID = uid;
            return this;
        }

        /** Sets the subject unique ID (deprecated in X.509v3) */
        public virtual CertTemplateBuilder SetSubjectUID(DerBitString uid)
        {
            subjectUID = uid;
            return this;
        }

        public virtual CertTemplateBuilder SetExtensions(X509Extensions extens)
        {
            extensions = extens;
            return this;
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
        public virtual CertTemplate Build()
        {
            Asn1EncodableVector v = new Asn1EncodableVector();

            AddOptional(v, 0, false, version);
            AddOptional(v, 1, false, serialNumber);
            AddOptional(v, 2, false, signingAlg);
            AddOptional(v, 3, true, issuer); // CHOICE
            AddOptional(v, 4, false, validity);
            AddOptional(v, 5, true, subject); // CHOICE
            AddOptional(v, 6, false, publicKey);
            AddOptional(v, 7, false, issuerUID);
            AddOptional(v, 8, false, subjectUID);
            AddOptional(v, 9, false, extensions);

            return CertTemplate.GetInstance(new DerSequence(v));
        }

        private void AddOptional(Asn1EncodableVector v, int tagNo, bool isExplicit, Asn1Encodable obj)
        {
            if (obj != null)
            {
                v.Add(new DerTaggedObject(isExplicit, tagNo, obj));
            }
        }
    }
}
