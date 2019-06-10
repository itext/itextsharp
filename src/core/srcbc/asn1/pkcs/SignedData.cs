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

namespace Org.BouncyCastle.Asn1.Pkcs
{
    /**
     * a Pkcs#7 signed data object.
     */
    public class SignedData
        : Asn1Encodable
    {
        private readonly DerInteger		version;
        private readonly Asn1Set		digestAlgorithms;
        private readonly ContentInfo	contentInfo;
        private readonly Asn1Set		certificates;
        private readonly Asn1Set		crls;
        private readonly Asn1Set		signerInfos;

        public static SignedData GetInstance(object obj)
        {
            if (obj == null)
                return null;
            SignedData existing = obj as SignedData;
            if (existing != null)
                return existing;
            return new SignedData(Asn1Sequence.GetInstance(obj));
        }

        public SignedData(
            DerInteger        _version,
            Asn1Set           _digestAlgorithms,
            ContentInfo       _contentInfo,
            Asn1Set           _certificates,
            Asn1Set           _crls,
            Asn1Set           _signerInfos)
        {
            version          = _version;
            digestAlgorithms = _digestAlgorithms;
            contentInfo      = _contentInfo;
            certificates     = _certificates;
            crls             = _crls;
            signerInfos      = _signerInfos;
        }

        private SignedData(
            Asn1Sequence seq)
        {
            IEnumerator e = seq.GetEnumerator();

            e.MoveNext();
            version = (DerInteger) e.Current;

            e.MoveNext();
            digestAlgorithms = (Asn1Set) e.Current;

            e.MoveNext();
            contentInfo = ContentInfo.GetInstance(e.Current);

            while (e.MoveNext())
            {
                Asn1Object o = (Asn1Object) e.Current;

                //
                // an interesting feature of SignedData is that there appear to be varying implementations...
                // for the moment we ignore anything which doesn't fit.
                //
                if (o is DerTaggedObject)
                {
                    DerTaggedObject tagged = (DerTaggedObject) o;

                    switch (tagged.TagNo)
                    {
                        case 0:
                            certificates = Asn1Set.GetInstance(tagged, false);
                            break;
                        case 1:
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

        public ContentInfo ContentInfo
        {
            get { return contentInfo; }
        }

        public Asn1Set Certificates
        {
            get { return certificates; }
        }

        public Asn1Set Crls
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
         *  SignedData ::= Sequence {
         *      version Version,
         *      digestAlgorithms DigestAlgorithmIdentifiers,
         *      contentInfo ContentInfo,
         *      certificates
         *          [0] IMPLICIT ExtendedCertificatesAndCertificates
         *                   OPTIONAL,
         *      crls
         *          [1] IMPLICIT CertificateRevocationLists OPTIONAL,
         *      signerInfos SignerInfos }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(
                version, digestAlgorithms, contentInfo);

            if (certificates != null)
            {
                v.Add(new DerTaggedObject(false, 0, certificates));
            }

            if (crls != null)
            {
                v.Add(new DerTaggedObject(false, 1, crls));
            }

            v.Add(signerInfos);

            return new BerSequence(v);
        }
    }
}
