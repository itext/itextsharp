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
    public class PopoSigningKey
        : Asn1Encodable
    {
        private readonly PopoSigningKeyInput poposkInput;
        private readonly AlgorithmIdentifier algorithmIdentifier;
        private readonly DerBitString signature;

        private PopoSigningKey(Asn1Sequence seq)
        {
            int index = 0;

            if (seq[index] is Asn1TaggedObject)
            {
                Asn1TaggedObject tagObj
                    = (Asn1TaggedObject) seq[index++];
                if (tagObj.TagNo != 0)
                {
                    throw new ArgumentException( "Unknown PopoSigningKeyInput tag: " + tagObj.TagNo, "seq");
                }
                poposkInput = PopoSigningKeyInput.GetInstance(tagObj.GetObject());
            }
            algorithmIdentifier = AlgorithmIdentifier.GetInstance(seq[index++]);
            signature = DerBitString.GetInstance(seq[index]);
        }

        public static PopoSigningKey GetInstance(object obj)
        {
            if (obj is PopoSigningKey)
                return (PopoSigningKey)obj;

            if (obj is Asn1Sequence)
                return new PopoSigningKey((Asn1Sequence)obj);

            throw new ArgumentException("Invalid object: " + obj.GetType().Name, "obj");
        }

        public static PopoSigningKey GetInstance(Asn1TaggedObject obj, bool isExplicit)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
        }

        /**
         * Creates a new Proof of Possession object for a signing key.
         * @param poposkIn the PopoSigningKeyInput structure, or null if the
         *     CertTemplate includes both subject and publicKey values.
         * @param aid the AlgorithmIdentifier used to sign the proof of possession.
         * @param signature a signature over the DER-encoded value of poposkIn,
         *     or the DER-encoded value of certReq if poposkIn is null.
         */
        public PopoSigningKey(
            PopoSigningKeyInput poposkIn,
            AlgorithmIdentifier aid,
            DerBitString signature)
        {
            this.poposkInput = poposkIn;
            this.algorithmIdentifier = aid;
            this.signature = signature;
        }

        public virtual PopoSigningKeyInput PoposkInput
        {
            get { return poposkInput; }
        }

        public virtual AlgorithmIdentifier AlgorithmIdentifier
        {
            get { return algorithmIdentifier; }
        }

        public virtual DerBitString Signature
        {
            get { return signature; }
        }

        /**
         * <pre>
         * PopoSigningKey ::= SEQUENCE {
         *                      poposkInput           [0] PopoSigningKeyInput OPTIONAL,
         *                      algorithmIdentifier   AlgorithmIdentifier,
         *                      signature             BIT STRING }
         *  -- The signature (using "algorithmIdentifier") is on the
         *  -- DER-encoded value of poposkInput.  NOTE: If the CertReqMsg
         *  -- certReq CertTemplate contains the subject and publicKey values,
         *  -- then poposkInput MUST be omitted and the signature MUST be
         *  -- computed on the DER-encoded value of CertReqMsg certReq.  If
         *  -- the CertReqMsg certReq CertTemplate does not contain the public
         *  -- key and subject values, then poposkInput MUST be present and
         *  -- MUST be signed.  This strategy ensures that the public key is
         *  -- not present in both the poposkInput and CertReqMsg certReq
         *  -- CertTemplate fields.
         * </pre>
         * @return a basic ASN.1 object representation.
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector();

            if (poposkInput != null)
            {
                v.Add(new DerTaggedObject(false, 0, poposkInput));
            }

            v.Add(algorithmIdentifier);
            v.Add(signature);

            return new DerSequence(v);
        }
    }
}
