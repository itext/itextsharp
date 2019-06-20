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
using System.Collections;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Cms
{
    public class SignerInfo
        : Asn1Encodable
    {
        private DerInteger              version;
        private SignerIdentifier        sid;
        private AlgorithmIdentifier     digAlgorithm;
        private Asn1Set                 authenticatedAttributes;
        private AlgorithmIdentifier     digEncryptionAlgorithm;
        private Asn1OctetString         encryptedDigest;
        private Asn1Set                 unauthenticatedAttributes;

        public static SignerInfo GetInstance(
            object obj)
        {
            if (obj == null || obj is SignerInfo)
                return (SignerInfo) obj;

            if (obj is Asn1Sequence)
                return new SignerInfo((Asn1Sequence) obj);

            throw new ArgumentException("Unknown object in factory: " + obj.GetType().FullName, "obj");
        }

        public SignerInfo(
            SignerIdentifier        sid,
            AlgorithmIdentifier     digAlgorithm,
            Asn1Set                 authenticatedAttributes,
            AlgorithmIdentifier     digEncryptionAlgorithm,
            Asn1OctetString         encryptedDigest,
            Asn1Set                 unauthenticatedAttributes)
        {
            this.version = new DerInteger(sid.IsTagged ? 3 : 1);
            this.sid = sid;
            this.digAlgorithm = digAlgorithm;
            this.authenticatedAttributes = authenticatedAttributes;
            this.digEncryptionAlgorithm = digEncryptionAlgorithm;
            this.encryptedDigest = encryptedDigest;
            this.unauthenticatedAttributes = unauthenticatedAttributes;
        }

        public SignerInfo(
            SignerIdentifier        sid,
            AlgorithmIdentifier     digAlgorithm,
            Attributes              authenticatedAttributes,
            AlgorithmIdentifier     digEncryptionAlgorithm,
            Asn1OctetString         encryptedDigest,
            Attributes              unauthenticatedAttributes)
        {
            this.version = new DerInteger(sid.IsTagged ? 3 : 1);
            this.sid = sid;
            this.digAlgorithm = digAlgorithm;
            this.authenticatedAttributes = Asn1Set.GetInstance(authenticatedAttributes);
            this.digEncryptionAlgorithm = digEncryptionAlgorithm;
            this.encryptedDigest = encryptedDigest;
            this.unauthenticatedAttributes = Asn1Set.GetInstance(unauthenticatedAttributes);
        }

        [Obsolete("Use 'GetInstance' instead")]
        public SignerInfo(
            Asn1Sequence seq)
        {
            IEnumerator e = seq.GetEnumerator();

            e.MoveNext();
            version = (DerInteger) e.Current;

            e.MoveNext();
            sid = SignerIdentifier.GetInstance(e.Current);

            e.MoveNext();
            digAlgorithm = AlgorithmIdentifier.GetInstance(e.Current);

            e.MoveNext();
            object obj = e.Current;

            if (obj is Asn1TaggedObject)
            {
                authenticatedAttributes = Asn1Set.GetInstance((Asn1TaggedObject) obj, false);

                e.MoveNext();
                digEncryptionAlgorithm = AlgorithmIdentifier.GetInstance(e.Current);
            }
            else
            {
                authenticatedAttributes = null;
                digEncryptionAlgorithm = AlgorithmIdentifier.GetInstance(obj);
            }

            e.MoveNext();
            encryptedDigest = DerOctetString.GetInstance(e.Current);

            if (e.MoveNext())
            {
                unauthenticatedAttributes = Asn1Set.GetInstance((Asn1TaggedObject) e.Current, false);
            }
            else
            {
                unauthenticatedAttributes = null;
            }
        }

        public DerInteger Version
        {
            get { return version; }
        }

        public SignerIdentifier SignerID
        {
            get { return sid; }
        }

        public Asn1Set AuthenticatedAttributes
        {
            get { return authenticatedAttributes; }
        }

        public AlgorithmIdentifier DigestAlgorithm
        {
            get { return digAlgorithm; }
        }

        public Asn1OctetString EncryptedDigest
        {
            get { return encryptedDigest; }
        }

        public AlgorithmIdentifier DigestEncryptionAlgorithm
        {
            get { return digEncryptionAlgorithm; }
        }

        public Asn1Set UnauthenticatedAttributes
        {
            get { return unauthenticatedAttributes; }
        }

        /**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         *  SignerInfo ::= Sequence {
         *      version Version,
         *      SignerIdentifier sid,
         *      digestAlgorithm DigestAlgorithmIdentifier,
         *      authenticatedAttributes [0] IMPLICIT Attributes OPTIONAL,
         *      digestEncryptionAlgorithm DigestEncryptionAlgorithmIdentifier,
         *      encryptedDigest EncryptedDigest,
         *      unauthenticatedAttributes [1] IMPLICIT Attributes OPTIONAL
         *  }
         *
         *  EncryptedDigest ::= OCTET STRING
         *
         *  DigestAlgorithmIdentifier ::= AlgorithmIdentifier
         *
         *  DigestEncryptionAlgorithmIdentifier ::= AlgorithmIdentifier
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(
                version, sid, digAlgorithm);

            if (authenticatedAttributes != null)
            {
                v.Add(new DerTaggedObject(false, 0, authenticatedAttributes));
            }

            v.Add(digEncryptionAlgorithm, encryptedDigest);

            if (unauthenticatedAttributes != null)
            {
                v.Add(new DerTaggedObject(false, 1, unauthenticatedAttributes));
            }

            return new DerSequence(v);
        }
    }
}
