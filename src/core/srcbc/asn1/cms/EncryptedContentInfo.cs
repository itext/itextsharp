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

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Cms
{
    public class EncryptedContentInfo
        : Asn1Encodable
    {
        private DerObjectIdentifier	contentType;
        private AlgorithmIdentifier	contentEncryptionAlgorithm;
        private Asn1OctetString		encryptedContent;

		public EncryptedContentInfo(
            DerObjectIdentifier	contentType,
            AlgorithmIdentifier	contentEncryptionAlgorithm,
            Asn1OctetString		encryptedContent)
        {
            this.contentType = contentType;
            this.contentEncryptionAlgorithm = contentEncryptionAlgorithm;
            this.encryptedContent = encryptedContent;
        }

		public EncryptedContentInfo(
            Asn1Sequence seq)
        {
            contentType = (DerObjectIdentifier) seq[0];
            contentEncryptionAlgorithm = AlgorithmIdentifier.GetInstance(seq[1]);

			if (seq.Count > 2)
            {
                encryptedContent = Asn1OctetString.GetInstance(
					(Asn1TaggedObject) seq[2], false);
            }
        }

		/**
         * return an EncryptedContentInfo object from the given object.
         *
         * @param obj the object we want converted.
         * @exception ArgumentException if the object cannot be converted.
         */
        public static EncryptedContentInfo GetInstance(
            object obj)
        {
            if (obj == null || obj is EncryptedContentInfo)
                return (EncryptedContentInfo)obj;

			if (obj is Asn1Sequence)
                return new EncryptedContentInfo((Asn1Sequence)obj);

			throw new ArgumentException("Invalid EncryptedContentInfo: " + obj.GetType().Name);
        }

        public DerObjectIdentifier ContentType
        {
            get { return contentType; }
        }

		public AlgorithmIdentifier ContentEncryptionAlgorithm
        {
			get { return contentEncryptionAlgorithm; }
        }

		public Asn1OctetString EncryptedContent
        {
			get { return encryptedContent; }
        }

		/**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         * EncryptedContentInfo ::= Sequence {
         *     contentType ContentType,
         *     contentEncryptionAlgorithm ContentEncryptionAlgorithmIdentifier,
         *     encryptedContent [0] IMPLICIT EncryptedContent OPTIONAL
         * }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(
				contentType, contentEncryptionAlgorithm);

			if (encryptedContent != null)
            {
                v.Add(new BerTaggedObject(false, 0, encryptedContent));
            }

			return new BerSequence(v);
        }
    }
}
