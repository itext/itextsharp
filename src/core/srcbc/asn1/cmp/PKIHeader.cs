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
    public class PkiHeader
        : Asn1Encodable
    {
        /**
         * Value for a "null" recipient or sender.
         */
        public static readonly GeneralName NULL_NAME = new GeneralName(X509Name.GetInstance(new DerSequence()));

        public static readonly int CMP_1999 = 1;
        public static readonly int CMP_2000 = 2;

        private readonly DerInteger pvno;
        private readonly GeneralName sender;
        private readonly GeneralName recipient;
        private readonly DerGeneralizedTime messageTime;
        private readonly AlgorithmIdentifier protectionAlg;
        private readonly Asn1OctetString senderKID;       // KeyIdentifier
        private readonly Asn1OctetString recipKID;        // KeyIdentifier
        private readonly Asn1OctetString transactionID;
        private readonly Asn1OctetString senderNonce;
        private readonly Asn1OctetString recipNonce;
        private readonly PkiFreeText freeText;
        private readonly Asn1Sequence generalInfo;

        private PkiHeader(Asn1Sequence seq)
        {
            pvno = DerInteger.GetInstance(seq[0]);
            sender = GeneralName.GetInstance(seq[1]);
            recipient = GeneralName.GetInstance(seq[2]);

            for (int pos = 3; pos < seq.Count; ++pos)
            {
                Asn1TaggedObject tObj = (Asn1TaggedObject)seq[pos];

                switch (tObj.TagNo)
                {
                    case 0:
                        messageTime = DerGeneralizedTime.GetInstance(tObj, true);
                        break;
                    case 1:
                        protectionAlg = AlgorithmIdentifier.GetInstance(tObj, true);
                        break;
                    case 2:
                        senderKID = Asn1OctetString.GetInstance(tObj, true);
                        break;
                    case 3:
                        recipKID = Asn1OctetString.GetInstance(tObj, true);
                        break;
                    case 4:
                        transactionID = Asn1OctetString.GetInstance(tObj, true);
                        break;
                    case 5:
                        senderNonce = Asn1OctetString.GetInstance(tObj, true);
                        break;
                    case 6:
                        recipNonce = Asn1OctetString.GetInstance(tObj, true);
                        break;
                    case 7:
                        freeText = PkiFreeText.GetInstance(tObj, true);
                        break;
                    case 8:
                        generalInfo = Asn1Sequence.GetInstance(tObj, true);
                        break;
                    default:
                        throw new ArgumentException("unknown tag number: " + tObj.TagNo, "seq");
                }
            }
        }

        public static PkiHeader GetInstance(object obj)
        {
            if (obj is PkiHeader)
                return (PkiHeader)obj;

            if (obj is Asn1Sequence)
                return new PkiHeader((Asn1Sequence)obj);

            throw new ArgumentException("Invalid object: " + obj.GetType().Name, "obj");
        }

        public PkiHeader(
            int pvno,
            GeneralName sender,
            GeneralName recipient)
            : this(new DerInteger(pvno), sender, recipient)
        {
        }

        private PkiHeader(
            DerInteger pvno,
            GeneralName sender,
            GeneralName recipient)
        {
            this.pvno = pvno;
            this.sender = sender;
            this.recipient = recipient;
        }

        public virtual DerInteger Pvno
        {
            get { return pvno; }
        }

        public virtual GeneralName Sender
        {
            get { return sender; }
        }

        public virtual GeneralName Recipient
        {
            get { return recipient; }
        }

        public virtual DerGeneralizedTime MessageTime
        {
            get { return messageTime; }
        }

        public virtual AlgorithmIdentifier ProtectionAlg
        {
            get { return protectionAlg; }
        }

        public virtual Asn1OctetString SenderKID
        {   
            get { return senderKID; }
        }

        public virtual Asn1OctetString RecipKID
        {   
            get { return recipKID; }
        }

        public virtual Asn1OctetString TransactionID
        {   
            get { return transactionID; }
        }

        public virtual Asn1OctetString SenderNonce
        {   
            get { return senderNonce; }
        }

        public virtual Asn1OctetString RecipNonce
        {   
            get { return recipNonce; }
        }

        public virtual PkiFreeText FreeText
        {
            get { return freeText; }
        }

        public virtual InfoTypeAndValue[] GetGeneralInfo()
        {
            if (generalInfo == null)
            {
                return null;
            }
            InfoTypeAndValue[] results = new InfoTypeAndValue[generalInfo.Count];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = InfoTypeAndValue.GetInstance(generalInfo[i]);
            }
            return results;
        }

        /**
         * <pre>
         *  PkiHeader ::= SEQUENCE {
         *            pvno                INTEGER     { cmp1999(1), cmp2000(2) },
         *            sender              GeneralName,
         *            -- identifies the sender
         *            recipient           GeneralName,
         *            -- identifies the intended recipient
         *            messageTime     [0] GeneralizedTime         OPTIONAL,
         *            -- time of production of this message (used when sender
         *            -- believes that the transport will be "suitable"; i.e.,
         *            -- that the time will still be meaningful upon receipt)
         *            protectionAlg   [1] AlgorithmIdentifier     OPTIONAL,
         *            -- algorithm used for calculation of protection bits
         *            senderKID       [2] KeyIdentifier           OPTIONAL,
         *            recipKID        [3] KeyIdentifier           OPTIONAL,
         *            -- to identify specific keys used for protection
         *            transactionID   [4] OCTET STRING            OPTIONAL,
         *            -- identifies the transaction; i.e., this will be the same in
         *            -- corresponding request, response, certConf, and PKIConf
         *            -- messages
         *            senderNonce     [5] OCTET STRING            OPTIONAL,
         *            recipNonce      [6] OCTET STRING            OPTIONAL,
         *            -- nonces used to provide replay protection, senderNonce
         *            -- is inserted by the creator of this message; recipNonce
         *            -- is a nonce previously inserted in a related message by
         *            -- the intended recipient of this message
         *            freeText        [7] PKIFreeText             OPTIONAL,
         *            -- this may be used to indicate context-specific instructions
         *            -- (this field is intended for human consumption)
         *            generalInfo     [8] SEQUENCE SIZE (1..MAX) OF
         *                                 InfoTypeAndValue     OPTIONAL
         *            -- this may be used to convey context-specific information
         *            -- (this field not primarily intended for human consumption)
         * }
         * </pre>
         * @return a basic ASN.1 object representation.
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(pvno, sender, recipient);

            AddOptional(v, 0, messageTime);
            AddOptional(v, 1, protectionAlg);
            AddOptional(v, 2, senderKID);
            AddOptional(v, 3, recipKID);
            AddOptional(v, 4, transactionID);
            AddOptional(v, 5, senderNonce);
            AddOptional(v, 6, recipNonce);
            AddOptional(v, 7, freeText);
            AddOptional(v, 8, generalInfo);

            return new DerSequence(v);
        }

        private static void AddOptional(Asn1EncodableVector v, int tagNo, Asn1Encodable obj)
        {
            if (obj != null)
            {
                v.Add(new DerTaggedObject(true, tagNo, obj));
            }
        }
    }
}
