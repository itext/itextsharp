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
    public class EnvelopedData
        : Asn1Encodable
    {
        private DerInteger				version;
        private OriginatorInfo			originatorInfo;
        private Asn1Set					recipientInfos;
        private EncryptedContentInfo	encryptedContentInfo;
        private Asn1Set					unprotectedAttrs;

        public EnvelopedData(
            OriginatorInfo			originatorInfo,
            Asn1Set					recipientInfos,
            EncryptedContentInfo	encryptedContentInfo,
            Asn1Set					unprotectedAttrs)
        {
            this.version = new DerInteger(CalculateVersion(originatorInfo, recipientInfos, unprotectedAttrs));
            this.originatorInfo = originatorInfo;
            this.recipientInfos = recipientInfos;
            this.encryptedContentInfo = encryptedContentInfo;
            this.unprotectedAttrs = unprotectedAttrs;
        }

        public EnvelopedData(
            OriginatorInfo originatorInfo,
            Asn1Set recipientInfos,
            EncryptedContentInfo encryptedContentInfo,
            Attributes unprotectedAttrs)
        {
            this.version = new DerInteger(CalculateVersion(originatorInfo, recipientInfos, Asn1Set.GetInstance(unprotectedAttrs)));
            this.originatorInfo = originatorInfo;
            this.recipientInfos = recipientInfos;
            this.encryptedContentInfo = encryptedContentInfo;
            this.unprotectedAttrs = Asn1Set.GetInstance(unprotectedAttrs);
        }

        [Obsolete("Use 'GetInstance' instead")]
        public EnvelopedData(
            Asn1Sequence seq)
        {
            int index = 0;

            version = (DerInteger) seq[index++];

            object tmp = seq[index++];

            if (tmp is Asn1TaggedObject)
            {
                originatorInfo = OriginatorInfo.GetInstance((Asn1TaggedObject) tmp, false);
                tmp = seq[index++];
            }

            recipientInfos = Asn1Set.GetInstance(tmp);
            encryptedContentInfo = EncryptedContentInfo.GetInstance(seq[index++]);

            if (seq.Count > index)
            {
                unprotectedAttrs = Asn1Set.GetInstance((Asn1TaggedObject) seq[index], false);
            }
        }

        /**
         * return an EnvelopedData object from a tagged object.
         *
         * @param obj the tagged object holding the object we want.
         * @param explicitly true if the object is meant to be explicitly
         *              tagged false otherwise.
         * @exception ArgumentException if the object held by the
         *          tagged object cannot be converted.
         */
        public static EnvelopedData GetInstance(
            Asn1TaggedObject	obj,
            bool				explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

        /**
         * return an EnvelopedData object from the given object.
         *
         * @param obj the object we want converted.
         * @exception ArgumentException if the object cannot be converted.
         */
        public static EnvelopedData GetInstance(
            object obj)
        {
            if (obj is EnvelopedData)
                return (EnvelopedData)obj;
            if (obj == null)
                return null;
            return new EnvelopedData(Asn1Sequence.GetInstance(obj));
        }

        public DerInteger Version
        {
            get { return version; }
        }

        public OriginatorInfo OriginatorInfo
        {
            get { return originatorInfo; }
        }

        public Asn1Set RecipientInfos
        {
            get { return recipientInfos; }
        }

        public EncryptedContentInfo EncryptedContentInfo
        {
            get { return encryptedContentInfo; }
        }

        public Asn1Set UnprotectedAttrs
        {
            get { return unprotectedAttrs; }
        }

        /**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         * EnvelopedData ::= Sequence {
         *     version CMSVersion,
         *     originatorInfo [0] IMPLICIT OriginatorInfo OPTIONAL,
         *     recipientInfos RecipientInfos,
         *     encryptedContentInfo EncryptedContentInfo,
         *     unprotectedAttrs [1] IMPLICIT UnprotectedAttributes OPTIONAL
         * }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(version);

            if (originatorInfo != null)
            {
                v.Add(new DerTaggedObject(false, 0, originatorInfo));
            }

            v.Add(recipientInfos, encryptedContentInfo);

            if (unprotectedAttrs != null)
            {
                v.Add(new DerTaggedObject(false, 1, unprotectedAttrs));
            }

            return new BerSequence(v);
        }

        public static int CalculateVersion(OriginatorInfo originatorInfo, Asn1Set recipientInfos, Asn1Set unprotectedAttrs)
        {
            if (originatorInfo != null || unprotectedAttrs != null)
            {
                return 2;
            }

            foreach (object o in recipientInfos)
            {
                RecipientInfo ri = RecipientInfo.GetInstance(o);

                if (ri.Version.Value.IntValue != 0)
                {
                    return 2;
                }
            }

            return 0;
        }
    }
}
