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

using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Asn1.Pkcs;

namespace Org.BouncyCastle.Asn1.Cmp
{
    public class PkiBody
        : Asn1Encodable, IAsn1Choice
    {
        public const int TYPE_INIT_REQ = 0;
        public const int TYPE_INIT_REP = 1;
        public const int TYPE_CERT_REQ = 2;
        public const int TYPE_CERT_REP = 3;
        public const int TYPE_P10_CERT_REQ = 4;
        public const int TYPE_POPO_CHALL = 5;
        public const int TYPE_POPO_REP = 6;
        public const int TYPE_KEY_UPDATE_REQ = 7;
        public const int TYPE_KEY_UPDATE_REP = 8;
        public const int TYPE_KEY_RECOVERY_REQ = 9;
        public const int TYPE_KEY_RECOVERY_REP = 10;
        public const int TYPE_REVOCATION_REQ = 11;
        public const int TYPE_REVOCATION_REP = 12;
        public const int TYPE_CROSS_CERT_REQ = 13;
        public const int TYPE_CROSS_CERT_REP = 14;
        public const int TYPE_CA_KEY_UPDATE_ANN = 15;
        public const int TYPE_CERT_ANN = 16;
        public const int TYPE_REVOCATION_ANN = 17;
        public const int TYPE_CRL_ANN = 18;
        public const int TYPE_CONFIRM = 19;
        public const int TYPE_NESTED = 20;
        public const int TYPE_GEN_MSG = 21;
        public const int TYPE_GEN_REP = 22;
        public const int TYPE_ERROR = 23;
        public const int TYPE_CERT_CONFIRM = 24;
        public const int TYPE_POLL_REQ = 25;
        public const int TYPE_POLL_REP = 26;

        private int tagNo;
        private Asn1Encodable body;

        public static PkiBody GetInstance(object obj)
        {
            if (obj is PkiBody)
                return (PkiBody)obj;

            if (obj is Asn1TaggedObject)
                return new PkiBody((Asn1TaggedObject)obj);

            throw new ArgumentException("Invalid object: " + obj.GetType().Name, "obj");
        }

        private PkiBody(Asn1TaggedObject tagged)
        {
            tagNo = tagged.TagNo;
            body = GetBodyForType(tagNo, tagged.GetObject());
        }

        /**
         * Creates a new PkiBody.
         * @param type one of the TYPE_* constants
         * @param content message content
         */
        public PkiBody(
            int type,
            Asn1Encodable content)
        {
            tagNo = type;
            body = GetBodyForType(type, content);
        }

        private static Asn1Encodable GetBodyForType(
            int type,
            Asn1Encodable o)
        {
            switch (type)
            {
                case TYPE_INIT_REQ:
                    return CertReqMessages.GetInstance(o);
	            case TYPE_INIT_REP:
	                return CertRepMessage.GetInstance(o);
                case TYPE_CERT_REQ:
                    return CertReqMessages.GetInstance(o);
	            case TYPE_CERT_REP:
	                return CertRepMessage.GetInstance(o);
	            case TYPE_P10_CERT_REQ:
	                return CertificationRequest.GetInstance(o);
	            case TYPE_POPO_CHALL:
	                return PopoDecKeyChallContent.GetInstance(o);
	            case TYPE_POPO_REP:
	                return PopoDecKeyRespContent.GetInstance(o);
                case TYPE_KEY_UPDATE_REQ:
                    return CertReqMessages.GetInstance(o);
	            case TYPE_KEY_UPDATE_REP:
	                return CertRepMessage.GetInstance(o);
                case TYPE_KEY_RECOVERY_REQ:
                    return CertReqMessages.GetInstance(o);
	            case TYPE_KEY_RECOVERY_REP:
	                return KeyRecRepContent.GetInstance(o);
	            case TYPE_REVOCATION_REQ:
	                return RevReqContent.GetInstance(o);
	            case TYPE_REVOCATION_REP:
	                return RevRepContent.GetInstance(o);
                case TYPE_CROSS_CERT_REQ:
                    return CertReqMessages.GetInstance(o);
	            case TYPE_CROSS_CERT_REP:
	                return CertRepMessage.GetInstance(o);
	            case TYPE_CA_KEY_UPDATE_ANN:
	                return CAKeyUpdAnnContent.GetInstance(o);
	            case TYPE_CERT_ANN:
	                return CmpCertificate.GetInstance(o);
	            case TYPE_REVOCATION_ANN:
	                return RevAnnContent.GetInstance(o);
	            case TYPE_CRL_ANN:
	                return CrlAnnContent.GetInstance(o);
	            case TYPE_CONFIRM:
	                return PkiConfirmContent.GetInstance(o);
                case TYPE_NESTED:
                    return PkiMessages.GetInstance(o);
	            case TYPE_GEN_MSG:
	                return GenMsgContent.GetInstance(o);
	            case TYPE_GEN_REP:
	                return GenRepContent.GetInstance(o);
	            case TYPE_ERROR:
	                return ErrorMsgContent.GetInstance(o);
	            case TYPE_CERT_CONFIRM:
	                return CertConfirmContent.GetInstance(o);
	            case TYPE_POLL_REQ:
	                return PollReqContent.GetInstance(o);
	            case TYPE_POLL_REP:
	                return PollRepContent.GetInstance(o);
	            default:
	                throw new ArgumentException("unknown tag number: " + type, "type");
            }
        }

        public virtual int Type
        {
            get { return tagNo; }
        }

        public virtual Asn1Encodable Content
        {
            get { return body; }
        }

        /**
         * <pre>
         * PkiBody ::= CHOICE {       -- message-specific body elements
         *        ir       [0]  CertReqMessages,        --Initialization Request
         *        ip       [1]  CertRepMessage,         --Initialization Response
         *        cr       [2]  CertReqMessages,        --Certification Request
         *        cp       [3]  CertRepMessage,         --Certification Response
         *        p10cr    [4]  CertificationRequest,   --imported from [PKCS10]
         *        popdecc  [5]  POPODecKeyChallContent, --pop Challenge
         *        popdecr  [6]  POPODecKeyRespContent,  --pop Response
         *        kur      [7]  CertReqMessages,        --Key Update Request
         *        kup      [8]  CertRepMessage,         --Key Update Response
         *        krr      [9]  CertReqMessages,        --Key Recovery Request
         *        krp      [10] KeyRecRepContent,       --Key Recovery Response
         *        rr       [11] RevReqContent,          --Revocation Request
         *        rp       [12] RevRepContent,          --Revocation Response
         *        ccr      [13] CertReqMessages,        --Cross-Cert. Request
         *        ccp      [14] CertRepMessage,         --Cross-Cert. Response
         *        ckuann   [15] CAKeyUpdAnnContent,     --CA Key Update Ann.
         *        cann     [16] CertAnnContent,         --Certificate Ann.
         *        rann     [17] RevAnnContent,          --Revocation Ann.
         *        crlann   [18] CRLAnnContent,          --CRL Announcement
         *        pkiconf  [19] PKIConfirmContent,      --Confirmation
         *        nested   [20] NestedMessageContent,   --Nested Message
         *        genm     [21] GenMsgContent,          --General Message
         *        genp     [22] GenRepContent,          --General Response
         *        error    [23] ErrorMsgContent,        --Error Message
         *        certConf [24] CertConfirmContent,     --Certificate confirm
         *        pollReq  [25] PollReqContent,         --Polling request
         *        pollRep  [26] PollRepContent          --Polling response
         * }
         * </pre>
         * @return a basic ASN.1 object representation.
         */
        public override Asn1Object ToAsn1Object()
        {
            return new DerTaggedObject(true, tagNo, body);
        }
    }
}
