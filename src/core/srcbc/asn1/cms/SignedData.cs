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

namespace Org.BouncyCastle.Asn1.Cms
{
    /**
     * a signed data object.
     */
    public class SignedData
        : Asn1Encodable
    {
        private static readonly DerInteger Version1 = new DerInteger(1);
        private static readonly DerInteger Version3 = new DerInteger(3);
        private static readonly DerInteger Version4 = new DerInteger(4);
        private static readonly DerInteger Version5 = new DerInteger(5);

        private readonly DerInteger		version;
        private readonly Asn1Set		digestAlgorithms;
        private readonly ContentInfo	contentInfo;
        private readonly Asn1Set		certificates;
        private readonly Asn1Set		crls;
        private readonly Asn1Set		signerInfos;
        private readonly bool			certsBer;
        private readonly bool		    crlsBer;

        public static SignedData GetInstance(
            object obj)
        {
            if (obj is SignedData)
                return (SignedData) obj;

            if (obj is Asn1Sequence)
                return new SignedData((Asn1Sequence) obj);

            throw new ArgumentException("Unknown object in factory: " + obj.GetType().FullName, "obj");
        }

        public SignedData(
            Asn1Set     digestAlgorithms,
            ContentInfo contentInfo,
            Asn1Set     certificates,
            Asn1Set     crls,
            Asn1Set     signerInfos)
        {
            this.version = CalculateVersion(contentInfo.ContentType, certificates, crls, signerInfos);
            this.digestAlgorithms = digestAlgorithms;
            this.contentInfo = contentInfo;
            this.certificates = certificates;
            this.crls = crls;
            this.signerInfos = signerInfos;
            this.crlsBer = crls is BerSet;
            this.certsBer = certificates is BerSet;
        }

        // RFC3852, section 5.1:
        // IF ((certificates is present) AND
        //    (any certificates with a type of other are present)) OR
        //    ((crls is present) AND
        //    (any crls with a type of other are present))
        // THEN version MUST be 5
        // ELSE
        //    IF (certificates is present) AND
        //       (any version 2 attribute certificates are present)
        //    THEN version MUST be 4
        //    ELSE
        //       IF ((certificates is present) AND
        //          (any version 1 attribute certificates are present)) OR
        //          (any SignerInfo structures are version 3) OR
        //          (encapContentInfo eContentType is other than id-data)
        //       THEN version MUST be 3
        //       ELSE version MUST be 1
        //
        private DerInteger CalculateVersion(
            DerObjectIdentifier	contentOid,
            Asn1Set				certs,
            Asn1Set				crls,
            Asn1Set				signerInfs)
        {
            bool otherCert = false;
            bool otherCrl = false;
            bool attrCertV1Found = false;
            bool attrCertV2Found = false;

            if (certs != null)
            {
                foreach (object obj in certs)
                {
                    if (obj is Asn1TaggedObject)
                    {
                        Asn1TaggedObject tagged = (Asn1TaggedObject)obj;

                        if (tagged.TagNo == 1)
                        {
                            attrCertV1Found = true;
                        }
                        else if (tagged.TagNo == 2)
                        {
                            attrCertV2Found = true;
                        }
                        else if (tagged.TagNo == 3)
                        {
                            otherCert = true;
                            break;
                        }
                    }
                }
            }

            if (otherCert)
            {
                return Version5;
            }

            if (crls != null)
            {
                foreach (object obj in crls)
                {
                    if (obj is Asn1TaggedObject)
                    {
                        otherCrl = true;
                        break;
                    }
                }
            }

            if (otherCrl)
            {
                return Version5;
            }

            if (attrCertV2Found)
            {
                return Version4;
            }

            if (attrCertV1Found || !CmsObjectIdentifiers.Data.Equals(contentOid) || CheckForVersion3(signerInfs))
            {
                return Version3;
            }

            return Version1;
        }

        private bool CheckForVersion3(
            Asn1Set signerInfs)
        {
            foreach (object obj in signerInfs)
            {
                SignerInfo s = SignerInfo.GetInstance(obj);

                if (s.Version.Value.IntValue == 3)
                {
                    return true;
                }
            }

            return false;
        }

        private SignedData(
            Asn1Sequence seq)
        {
            IEnumerator e = seq.GetEnumerator();

            e.MoveNext();
            version = (DerInteger)e.Current;

            e.MoveNext();
            digestAlgorithms = ((Asn1Set)e.Current);

            e.MoveNext();
            contentInfo = ContentInfo.GetInstance(e.Current);

            while (e.MoveNext())
            {
                Asn1Object o = (Asn1Object)e.Current;

                //
                // an interesting feature of SignedData is that there appear
                // to be varying implementations...
                // for the moment we ignore anything which doesn't fit.
                //
                if (o is Asn1TaggedObject)
                {
                    Asn1TaggedObject tagged = (Asn1TaggedObject)o;

                    switch (tagged.TagNo)
                    {
                        case 0:
                            certsBer = tagged is BerTaggedObject;
                            certificates = Asn1Set.GetInstance(tagged, false);
                            break;
                        case 1:
                            crlsBer = tagged is BerTaggedObject;
                            crls = Asn1Set.GetInstance(tagged, false);
                            break;
                        default:
                            throw new ArgumentException("unknown tag value " + tagged.TagNo);
                    }
                }
                else
                {
                    signerInfos = (Asn1Set) o;
                }
            }
        }

        public DerInteger Version
        {
            get { return version; }
        }

        public Asn1Set DigestAlgorithms
        {
            get { return digestAlgorithms; }
        }

        public ContentInfo EncapContentInfo
        {
            get { return contentInfo; }
        }

        public Asn1Set Certificates
        {
            get { return certificates; }
        }

        public Asn1Set CRLs
        {
            get { return crls; }
        }

        public Asn1Set SignerInfos
        {
            get { return signerInfos; }
        }

        /**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         * SignedData ::= Sequence {
         *     version CMSVersion,
         *     digestAlgorithms DigestAlgorithmIdentifiers,
         *     encapContentInfo EncapsulatedContentInfo,
         *     certificates [0] IMPLICIT CertificateSet OPTIONAL,
         *     crls [1] IMPLICIT CertificateRevocationLists OPTIONAL,
         *     signerInfos SignerInfos
         *   }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(
                version, digestAlgorithms, contentInfo);

            if (certificates != null)
            {
                if (certsBer)
                {
                    v.Add(new BerTaggedObject(false, 0, certificates));
                }
                else
                {
                    v.Add(new DerTaggedObject(false, 0, certificates));
                }
            }

            if (crls != null)
            {
                if (crlsBer)
                {
                    v.Add(new BerTaggedObject(false, 1, crls));
                }
                else
                {
                    v.Add(new DerTaggedObject(false, 1, crls));
                }
            }

            v.Add(signerInfos);

            return new BerSequence(v);
        }
    }
}
