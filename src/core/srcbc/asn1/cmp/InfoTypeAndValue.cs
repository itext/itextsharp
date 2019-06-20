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

namespace Org.BouncyCastle.Asn1.Cmp
{
    /**
     * Example InfoTypeAndValue contents include, but are not limited
     * to, the following (un-comment in this ASN.1 module and use as
     * appropriate for a given environment):
     * <pre>
     *   id-it-caProtEncCert    OBJECT IDENTIFIER ::= {id-it 1}
     *      CAProtEncCertValue      ::= CMPCertificate
     *   id-it-signKeyPairTypes OBJECT IDENTIFIER ::= {id-it 2}
     *     SignKeyPairTypesValue   ::= SEQUENCE OF AlgorithmIdentifier
     *   id-it-encKeyPairTypes  OBJECT IDENTIFIER ::= {id-it 3}
     *     EncKeyPairTypesValue    ::= SEQUENCE OF AlgorithmIdentifier
     *   id-it-preferredSymmAlg OBJECT IDENTIFIER ::= {id-it 4}
     *      PreferredSymmAlgValue   ::= AlgorithmIdentifier
     *   id-it-caKeyUpdateInfo  OBJECT IDENTIFIER ::= {id-it 5}
     *      CAKeyUpdateInfoValue    ::= CAKeyUpdAnnContent
     *   id-it-currentCRL       OBJECT IDENTIFIER ::= {id-it 6}
     *      CurrentCRLValue         ::= CertificateList
     *   id-it-unsupportedOIDs  OBJECT IDENTIFIER ::= {id-it 7}
     *      UnsupportedOIDsValue    ::= SEQUENCE OF OBJECT IDENTIFIER
     *   id-it-keyPairParamReq  OBJECT IDENTIFIER ::= {id-it 10}
     *      KeyPairParamReqValue    ::= OBJECT IDENTIFIER
     *   id-it-keyPairParamRep  OBJECT IDENTIFIER ::= {id-it 11}
     *      KeyPairParamRepValue    ::= AlgorithmIdentifer
     *   id-it-revPassphrase    OBJECT IDENTIFIER ::= {id-it 12}
     *      RevPassphraseValue      ::= EncryptedValue
     *   id-it-implicitConfirm  OBJECT IDENTIFIER ::= {id-it 13}
     *      ImplicitConfirmValue    ::= NULL
     *   id-it-confirmWaitTime  OBJECT IDENTIFIER ::= {id-it 14}
     *      ConfirmWaitTimeValue    ::= GeneralizedTime
     *   id-it-origPKIMessage   OBJECT IDENTIFIER ::= {id-it 15}
     *      OrigPKIMessageValue     ::= PKIMessages
     *   id-it-suppLangTags     OBJECT IDENTIFIER ::= {id-it 16}
     *      SuppLangTagsValue       ::= SEQUENCE OF UTF8String
     *
     * where
     *
     *   id-pkix OBJECT IDENTIFIER ::= {
     *      iso(1) identified-organization(3)
     *      dod(6) internet(1) security(5) mechanisms(5) pkix(7)}
     * and
     *      id-it   OBJECT IDENTIFIER ::= {id-pkix 4}
     * </pre>
     */
    public class InfoTypeAndValue
        : Asn1Encodable
    {
        private readonly DerObjectIdentifier infoType;
        private readonly Asn1Encodable infoValue;

        private InfoTypeAndValue(Asn1Sequence seq)
        {
            infoType = DerObjectIdentifier.GetInstance(seq[0]);

            if (seq.Count > 1)
            {
                infoValue = (Asn1Encodable)seq[1];
            }
        }

        public static InfoTypeAndValue GetInstance(object obj)
        {
            if (obj is InfoTypeAndValue)
                return (InfoTypeAndValue)obj;

            if (obj is Asn1Sequence)
                return new InfoTypeAndValue((Asn1Sequence)obj);

            throw new ArgumentException("Invalid object: " + obj.GetType().Name, "obj");
        }

        public InfoTypeAndValue(
            DerObjectIdentifier infoType)
        {
            this.infoType = infoType;
            this.infoValue = null;
        }

        public InfoTypeAndValue(
            DerObjectIdentifier infoType,
            Asn1Encodable       optionalValue)
        {
            this.infoType = infoType;
            this.infoValue = optionalValue;
        }

        public virtual DerObjectIdentifier InfoType
        {
            get { return infoType; }
        }

        public virtual Asn1Encodable InfoValue
        {
            get { return infoValue; }
        }

        /**
         * <pre>
         * InfoTypeAndValue ::= SEQUENCE {
         *                         infoType               OBJECT IDENTIFIER,
         *                         infoValue              ANY DEFINED BY infoType  OPTIONAL
         * }
         * </pre>
         * @return a basic ASN.1 object representation.
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(infoType);

            if (infoValue != null)
            {
                v.Add(infoValue);
            }

            return new DerSequence(v);
        }
    }
}
