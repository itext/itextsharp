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

using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Crmf
{
    /**
     * Password-based MAC value for use with POPOSigningKeyInput.
     */
    public class PKMacValue
        : Asn1Encodable
    {
        private readonly AlgorithmIdentifier  algID;
        private readonly DerBitString         macValue;

        private PKMacValue(Asn1Sequence seq)
        {
            this.algID = AlgorithmIdentifier.GetInstance(seq[0]);
            this.macValue = DerBitString.GetInstance(seq[1]);
        }

        public static PKMacValue GetInstance(object obj)
        {
            if (obj is PKMacValue)
                return (PKMacValue)obj;

            if (obj is Asn1Sequence)
                return new PKMacValue((Asn1Sequence)obj);

            throw new ArgumentException("Invalid object: " + obj.GetType().Name, "obj");
        }

        public static PKMacValue GetInstance(Asn1TaggedObject obj, bool isExplicit)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
        }

        /**
         * Creates a new PKMACValue.
         * @param params parameters for password-based MAC
         * @param value MAC of the DER-encoded SubjectPublicKeyInfo
         */
        public PKMacValue(
            PbmParameter pbmParams,
            DerBitString macValue)
            : this(new AlgorithmIdentifier(CmpObjectIdentifiers.passwordBasedMac, pbmParams), macValue)
        {
        }

        /**
         * Creates a new PKMACValue.
         * @param aid CMPObjectIdentifiers.passwordBasedMAC, with PBMParameter
         * @param value MAC of the DER-encoded SubjectPublicKeyInfo
         */
        public PKMacValue(
            AlgorithmIdentifier algID,
            DerBitString        macValue)
        {
            this.algID = algID;
            this.macValue = macValue;
        }

        public virtual AlgorithmIdentifier AlgID
        {
            get { return algID; }
        }

        public virtual DerBitString MacValue
        {
            get { return macValue; }
        }

        /**
         * <pre>
         * PKMACValue ::= SEQUENCE {
         *      algId  AlgorithmIdentifier,
         *      -- algorithm value shall be PasswordBasedMac 1.2.840.113533.7.66.13
         *      -- parameter value is PBMParameter
         *      value  BIT STRING }
         * </pre>
         * @return a basic ASN.1 object representation.
         */
        public override Asn1Object ToAsn1Object()
        {
            return new DerSequence(algID, macValue);
        }
    }
}
