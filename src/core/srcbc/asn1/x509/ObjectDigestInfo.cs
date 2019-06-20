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

namespace Org.BouncyCastle.Asn1.X509
{
	/**
	 * ObjectDigestInfo ASN.1 structure used in v2 attribute certificates.
	 * 
	 * <pre>
	 *  
	 *    ObjectDigestInfo ::= SEQUENCE {
	 *         digestedObjectType  ENUMERATED {
	 *                 publicKey            (0),
	 *                 publicKeyCert        (1),
	 *                 otherObjectTypes     (2) },
	 *                         -- otherObjectTypes MUST NOT
	 *                         -- be used in this profile
	 *         otherObjectTypeID   OBJECT IDENTIFIER OPTIONAL,
	 *         digestAlgorithm     AlgorithmIdentifier,
	 *         objectDigest        BIT STRING
	 *    }
	 *   
	 * </pre>
	 * 
	 */
	public class ObjectDigestInfo
        : Asn1Encodable
    {
		/**
		 * The public key is hashed.
		 */
		public const int PublicKey = 0;

		/**
		 * The public key certificate is hashed.
		 */
		public const int PublicKeyCert = 1;

		/**
		 * An other object is hashed.
		 */
		public const int OtherObjectDigest = 2;

		internal readonly DerEnumerated			digestedObjectType;
        internal readonly DerObjectIdentifier	otherObjectTypeID;
        internal readonly AlgorithmIdentifier	digestAlgorithm;
        internal readonly DerBitString			objectDigest;

		public static ObjectDigestInfo GetInstance(
            object obj)
        {
            if (obj == null || obj is ObjectDigestInfo)
            {
                return (ObjectDigestInfo) obj;
            }

			if (obj is Asn1Sequence)
            {
                return new ObjectDigestInfo((Asn1Sequence) obj);
            }

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		public static ObjectDigestInfo GetInstance(
            Asn1TaggedObject	obj,
            bool				isExplicit)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, isExplicit));
        }

		/**
		 * Constructor from given details.
		 * <p>
		 * If <code>digestedObjectType</code> is not {@link #publicKeyCert} or
		 * {@link #publicKey} <code>otherObjectTypeID</code> must be given,
		 * otherwise it is ignored.</p>
		 * 
		 * @param digestedObjectType The digest object type.
		 * @param otherObjectTypeID The object type ID for
		 *            <code>otherObjectDigest</code>.
		 * @param digestAlgorithm The algorithm identifier for the hash.
		 * @param objectDigest The hash value.
		 */
		public ObjectDigestInfo(
			int					digestedObjectType,
			string				otherObjectTypeID,
			AlgorithmIdentifier	digestAlgorithm,
			byte[]				objectDigest)
		{
			this.digestedObjectType = new DerEnumerated(digestedObjectType);

			if (digestedObjectType == OtherObjectDigest)
			{
				this.otherObjectTypeID = new DerObjectIdentifier(otherObjectTypeID);
			}

			this.digestAlgorithm = digestAlgorithm; 

			this.objectDigest = new DerBitString(objectDigest);
		}

		private ObjectDigestInfo(
			Asn1Sequence seq)
        {
			if (seq.Count > 4 || seq.Count < 3)
			{
				throw new ArgumentException("Bad sequence size: " + seq.Count);
			}

			digestedObjectType = DerEnumerated.GetInstance(seq[0]);

			int offset = 0;

			if (seq.Count == 4)
            {
                otherObjectTypeID = DerObjectIdentifier.GetInstance(seq[1]);
                offset++;
            }

			digestAlgorithm = AlgorithmIdentifier.GetInstance(seq[1 + offset]);
			objectDigest = DerBitString.GetInstance(seq[2 + offset]);
		}

		public DerEnumerated DigestedObjectType
		{
			get { return digestedObjectType; }
		}

		public DerObjectIdentifier OtherObjectTypeID
		{
			get { return otherObjectTypeID; }
		}

		public AlgorithmIdentifier DigestAlgorithm
		{
			get { return digestAlgorithm; }
		}

		public DerBitString ObjectDigest
		{
			get { return objectDigest; }
		}

		/**
		 * Produce an object suitable for an Asn1OutputStream.
		 * 
		 * <pre>
		 *  
		 *    ObjectDigestInfo ::= SEQUENCE {
		 *         digestedObjectType  ENUMERATED {
		 *                 publicKey            (0),
		 *                 publicKeyCert        (1),
		 *                 otherObjectTypes     (2) },
		 *                         -- otherObjectTypes MUST NOT
		 *                         -- be used in this profile
		 *         otherObjectTypeID   OBJECT IDENTIFIER OPTIONAL,
		 *         digestAlgorithm     AlgorithmIdentifier,
		 *         objectDigest        BIT STRING
		 *    }
		 *   
		 * </pre>
		 */
		public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(digestedObjectType);

			if (otherObjectTypeID != null)
            {
                v.Add(otherObjectTypeID);
            }

			v.Add(digestAlgorithm, objectDigest);

			return new DerSequence(v);
        }
    }
}
