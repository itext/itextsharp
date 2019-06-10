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
using System.IO;

namespace Org.BouncyCastle.Asn1.X509
{
    /**
     * The object that contains the public key stored in a certficate.
     * <p>
     * The GetEncoded() method in the public keys in the JCE produces a DER
     * encoded one of these.</p>
     */
    public class SubjectPublicKeyInfo
        : Asn1Encodable
    {
        private readonly AlgorithmIdentifier	algID;
        private readonly DerBitString			keyData;

		public static SubjectPublicKeyInfo GetInstance(
            Asn1TaggedObject	obj,
            bool				explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

		public static SubjectPublicKeyInfo GetInstance(
            object obj)
        {
            if (obj is SubjectPublicKeyInfo)
                return (SubjectPublicKeyInfo) obj;

			if (obj != null)
				return new SubjectPublicKeyInfo(Asn1Sequence.GetInstance(obj));

			return null;
        }

		public SubjectPublicKeyInfo(
            AlgorithmIdentifier	algID,
            Asn1Encodable		publicKey)
        {
            this.keyData = new DerBitString(publicKey);
            this.algID = algID;
        }

		public SubjectPublicKeyInfo(
            AlgorithmIdentifier	algID,
            byte[]				publicKey)
        {
            this.keyData = new DerBitString(publicKey);
            this.algID = algID;
        }

		private SubjectPublicKeyInfo(
            Asn1Sequence seq)
        {
			if (seq.Count != 2)
				throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");

            this.algID = AlgorithmIdentifier.GetInstance(seq[0]);
			this.keyData = DerBitString.GetInstance(seq[1]);
		}

		public AlgorithmIdentifier AlgorithmID
        {
			get { return algID; }
        }

		/**
         * for when the public key is an encoded object - if the bitstring
         * can't be decoded this routine raises an IOException.
         *
         * @exception IOException - if the bit string doesn't represent a Der
         * encoded object.
         */
        public Asn1Object GetPublicKey()
        {
			return Asn1Object.FromByteArray(keyData.GetBytes());
        }

		/**
         * for when the public key is raw bits...
         */
        public DerBitString PublicKeyData
        {
			get { return keyData; }
        }

		/**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         * SubjectPublicKeyInfo ::= Sequence {
         *                          algorithm AlgorithmIdentifier,
         *                          publicKey BIT STRING }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
			return new DerSequence(algID, keyData);
        }
    }
}
