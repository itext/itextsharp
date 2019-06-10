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

namespace Org.BouncyCastle.Asn1.Crmf
{
    public class ProofOfPossession
        : Asn1Encodable, IAsn1Choice
    {
        public const int TYPE_RA_VERIFIED = 0;
        public const int TYPE_SIGNING_KEY = 1;
        public const int TYPE_KEY_ENCIPHERMENT = 2;
        public const int TYPE_KEY_AGREEMENT = 3;

        private readonly int tagNo;
        private readonly Asn1Encodable obj;

        private ProofOfPossession(Asn1TaggedObject tagged)
        {
            tagNo = tagged.TagNo;
            switch (tagNo)
            {
            case 0:
                obj = DerNull.Instance;
                break;
            case 1:
                obj = PopoSigningKey.GetInstance(tagged, false);
                break;
            case 2:
            case 3:
                obj = PopoPrivKey.GetInstance(tagged, false);
                break;
            default:
                throw new ArgumentException("unknown tag: " + tagNo, "tagged");
            }
        }

        public static ProofOfPossession GetInstance(object obj)
        {
            if (obj is ProofOfPossession)
                return (ProofOfPossession)obj;

            if (obj is Asn1TaggedObject)
                return new ProofOfPossession((Asn1TaggedObject)obj);

            throw new ArgumentException("Invalid object: " + obj.GetType().Name, "obj");
        }

        /** Creates a ProofOfPossession with type raVerified. */
        public ProofOfPossession()
        {
            tagNo = TYPE_RA_VERIFIED;
            obj = DerNull.Instance;
        }

        /** Creates a ProofOfPossession for a signing key. */
        public ProofOfPossession(PopoSigningKey Poposk)
        {
            tagNo = TYPE_SIGNING_KEY;
            obj = Poposk;
        }

        /**
         * Creates a ProofOfPossession for key encipherment or agreement.
         * @param type one of TYPE_KEY_ENCIPHERMENT or TYPE_KEY_AGREEMENT
         */
        public ProofOfPossession(int type, PopoPrivKey privkey)
        {
            tagNo = type;
            obj = privkey;
        }

        public virtual int Type
        {
            get { return tagNo; }
        }

        public virtual Asn1Encodable Object
        {
            get { return obj; }
        }

        /**
         * <pre>
         * ProofOfPossession ::= CHOICE {
         *                           raVerified        [0] NULL,
         *                           -- used if the RA has already verified that the requester is in
         *                           -- possession of the private key
         *                           signature         [1] PopoSigningKey,
         *                           keyEncipherment   [2] PopoPrivKey,
         *                           keyAgreement      [3] PopoPrivKey }
         * </pre>
         * @return a basic ASN.1 object representation.
         */
        public override Asn1Object ToAsn1Object()
        {
            return new DerTaggedObject(false, tagNo, obj);
        }
    }
}
