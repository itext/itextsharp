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
    public class EncryptedValue
        : Asn1Encodable
    {
        private readonly AlgorithmIdentifier intendedAlg;
        private readonly AlgorithmIdentifier symmAlg;
        private readonly DerBitString encSymmKey;
        private readonly AlgorithmIdentifier keyAlg;
        private readonly Asn1OctetString valueHint;
        private readonly DerBitString encValue;

        private EncryptedValue(Asn1Sequence seq)
        {
            int index = 0;
            while (seq[index] is Asn1TaggedObject)
            {
                Asn1TaggedObject tObj = (Asn1TaggedObject)seq[index];

                switch (tObj.TagNo)
                {
                    case 0:
                        intendedAlg = AlgorithmIdentifier.GetInstance(tObj, false);
                        break;
                    case 1:
                        symmAlg = AlgorithmIdentifier.GetInstance(tObj, false);
                        break;
                    case 2:
                        encSymmKey = DerBitString.GetInstance(tObj, false);
                        break;
                    case 3:
                        keyAlg = AlgorithmIdentifier.GetInstance(tObj, false);
                        break;
                    case 4:
                        valueHint = Asn1OctetString.GetInstance(tObj, false);
                        break;
                }
                ++index;
            }

            encValue = DerBitString.GetInstance(seq[index]);
        }

        public static EncryptedValue GetInstance(object obj)
        {
            if (obj is EncryptedValue)
                return (EncryptedValue)obj;

            if (obj != null)
                return new EncryptedValue(Asn1Sequence.GetInstance(obj));

            return null;
        }

        public EncryptedValue(
            AlgorithmIdentifier intendedAlg,
            AlgorithmIdentifier symmAlg,
            DerBitString encSymmKey,
            AlgorithmIdentifier keyAlg,
            Asn1OctetString valueHint,
            DerBitString encValue)
        {
            if (encValue == null)
            {
                throw new ArgumentNullException("encValue");
            }

            this.intendedAlg = intendedAlg;
            this.symmAlg = symmAlg;
            this.encSymmKey = encSymmKey;
            this.keyAlg = keyAlg;
            this.valueHint = valueHint;
            this.encValue = encValue;
        }

        public virtual AlgorithmIdentifier IntendedAlg
        {
            get { return intendedAlg; }
        }

        public virtual AlgorithmIdentifier SymmAlg
        {
            get { return symmAlg; }
        }

        public virtual DerBitString EncSymmKey
        {
            get { return encSymmKey; }
        }

        public virtual AlgorithmIdentifier KeyAlg
        {
            get { return keyAlg; }
        }

        public virtual Asn1OctetString ValueHint
        {
            get { return valueHint; }
        }

        public virtual DerBitString EncValue
        {
            get { return encValue; }
        }

        /**
         * <pre>
         * EncryptedValue ::= SEQUENCE {
         *                     intendedAlg   [0] AlgorithmIdentifier  OPTIONAL,
         *                     -- the intended algorithm for which the value will be used
         *                     symmAlg       [1] AlgorithmIdentifier  OPTIONAL,
         *                     -- the symmetric algorithm used to encrypt the value
         *                     encSymmKey    [2] BIT STRING           OPTIONAL,
         *                     -- the (encrypted) symmetric key used to encrypt the value
         *                     keyAlg        [3] AlgorithmIdentifier  OPTIONAL,
         *                     -- algorithm used to encrypt the symmetric key
         *                     valueHint     [4] OCTET STRING         OPTIONAL,
         *                     -- a brief description or identifier of the encValue content
         *                     -- (may be meaningful only to the sending entity, and used only
         *                     -- if EncryptedValue might be re-examined by the sending entity
         *                     -- in the future)
         *                     encValue       BIT STRING }
         *                     -- the encrypted value itself
         * </pre>
         * @return a basic ASN.1 object representation.
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector();

            AddOptional(v, 0, intendedAlg);
            AddOptional(v, 1, symmAlg);
            AddOptional(v, 2, encSymmKey);
            AddOptional(v, 3, keyAlg);
            AddOptional(v, 4, valueHint);

            v.Add(encValue);

            return new DerSequence(v);
        }

        private void AddOptional(Asn1EncodableVector v, int tagNo, Asn1Encodable obj)
        {
            if (obj != null)
            {
                v.Add(new DerTaggedObject(false, tagNo, obj));
            }
        }
    }
}
