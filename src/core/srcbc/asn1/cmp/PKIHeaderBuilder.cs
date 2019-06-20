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
	public class PkiHeaderBuilder
	{
		private DerInteger pvno;
		private GeneralName sender;
		private GeneralName recipient;
		private DerGeneralizedTime messageTime;
		private AlgorithmIdentifier protectionAlg;
		private Asn1OctetString senderKID;       // KeyIdentifier
		private Asn1OctetString recipKID;        // KeyIdentifier
		private Asn1OctetString transactionID;
		private Asn1OctetString senderNonce;
		private Asn1OctetString recipNonce;
		private PkiFreeText     freeText;
		private Asn1Sequence    generalInfo;

		public PkiHeaderBuilder(
			int			pvno,
			GeneralName	sender,
			GeneralName	recipient)
			: this(new DerInteger(pvno), sender, recipient)
		{
		}

		private PkiHeaderBuilder(
			DerInteger	pvno,
			GeneralName	sender,
			GeneralName	recipient)
		{
			this.pvno = pvno;
			this.sender = sender;
			this.recipient = recipient;
		}

		public virtual PkiHeaderBuilder SetMessageTime(DerGeneralizedTime time)
		{
			messageTime = time;
			return this;
		}

		public virtual PkiHeaderBuilder SetProtectionAlg(AlgorithmIdentifier aid)
		{
			protectionAlg = aid;
			return this;
		}

		public virtual PkiHeaderBuilder SetSenderKID(byte[] kid)
		{
            return SetSenderKID(kid == null ? null : new DerOctetString(kid));
		}

		public virtual PkiHeaderBuilder SetSenderKID(Asn1OctetString kid)
		{
			senderKID = kid;
			return this;
		}

		public virtual PkiHeaderBuilder SetRecipKID(byte[] kid)
		{
            return SetRecipKID(kid == null ? null : new DerOctetString(kid));
		}
		
		public virtual PkiHeaderBuilder SetRecipKID(DerOctetString kid)
		{
			recipKID = kid;
			return this;
		}

		public virtual PkiHeaderBuilder SetTransactionID(byte[] tid)
		{
			return SetTransactionID(tid == null ? null : new DerOctetString(tid));
		}

		public virtual PkiHeaderBuilder SetTransactionID(Asn1OctetString tid)
		{
			transactionID = tid;
			return this;
		}
		
		public virtual PkiHeaderBuilder SetSenderNonce(byte[] nonce)
		{
            return SetSenderNonce(nonce == null ? null : new DerOctetString(nonce));
		}

		public virtual PkiHeaderBuilder SetSenderNonce(Asn1OctetString nonce)
		{
			senderNonce = nonce;
			return this;
		}

		public virtual PkiHeaderBuilder SetRecipNonce(byte[] nonce)
		{
            return SetRecipNonce(nonce == null ? null : new DerOctetString(nonce));
		}

		public virtual PkiHeaderBuilder SetRecipNonce(Asn1OctetString nonce)
		{
			recipNonce = nonce;
			return this;
		}

		public virtual PkiHeaderBuilder SetFreeText(PkiFreeText text)
		{
			freeText = text;
			return this;
		}
		
		public virtual PkiHeaderBuilder SetGeneralInfo(InfoTypeAndValue genInfo)
		{
			return SetGeneralInfo(MakeGeneralInfoSeq(genInfo));
		}
		
		public virtual PkiHeaderBuilder SetGeneralInfo(InfoTypeAndValue[] genInfos)
		{
			return SetGeneralInfo(MakeGeneralInfoSeq(genInfos));
		}
		
		public virtual PkiHeaderBuilder SetGeneralInfo(Asn1Sequence seqOfInfoTypeAndValue)
		{
			generalInfo = seqOfInfoTypeAndValue;
			return this;
		}

		private static Asn1Sequence MakeGeneralInfoSeq(
			InfoTypeAndValue generalInfo)
		{
			return new DerSequence(generalInfo);
		}
		
		private static Asn1Sequence MakeGeneralInfoSeq(
			InfoTypeAndValue[] generalInfos)
		{
			Asn1Sequence genInfoSeq = null;
			if (generalInfos != null)
			{
				Asn1EncodableVector v = new Asn1EncodableVector();
				for (int i = 0; i < generalInfos.Length; ++i)
				{
					v.Add(generalInfos[i]);
				}
				genInfoSeq = new DerSequence(v);
			}
			return genInfoSeq;
		}

		/**
		 * <pre>
		 *  PKIHeader ::= SEQUENCE {
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
		public virtual PkiHeader Build()
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

			messageTime = null;
			protectionAlg = null;
			senderKID = null;
			recipKID = null;
			transactionID = null;
			senderNonce = null;
			recipNonce = null;
			freeText = null;
			generalInfo = null;

			return PkiHeader.GetInstance(new DerSequence(v));
		}

		private void AddOptional(Asn1EncodableVector v, int tagNo, Asn1Encodable obj)
		{
			if (obj != null)
			{
				v.Add(new DerTaggedObject(true, tagNo, obj));
			}
		}
	}
}
