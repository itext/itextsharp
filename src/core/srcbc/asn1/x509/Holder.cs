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
	 * The Holder object.
	 * <p>
	 * For an v2 attribute certificate this is:
	 * 
	 * <pre>
	 *            Holder ::= SEQUENCE {
	 *                  baseCertificateID   [0] IssuerSerial OPTIONAL,
	 *                           -- the issuer and serial number of
	 *                           -- the holder's Public Key Certificate
	 *                  entityName          [1] GeneralNames OPTIONAL,
	 *                           -- the name of the claimant or role
	 *                  objectDigestInfo    [2] ObjectDigestInfo OPTIONAL
	 *                           -- used to directly authenticate the holder,
	 *                           -- for example, an executable
	 *            }
	 * </pre>
	 * </p>
	 * <p>
	 * For an v1 attribute certificate this is:
	 * 
	 * <pre>
	 *         subject CHOICE {
	 *          baseCertificateID [0] IssuerSerial,
	 *          -- associated with a Public Key Certificate
	 *          subjectName [1] GeneralNames },
	 *          -- associated with a name
	 * </pre>
	 * </p>
	 */
	public class Holder
        : Asn1Encodable
    {
		internal readonly IssuerSerial		baseCertificateID;
        internal readonly GeneralNames		entityName;
        internal readonly ObjectDigestInfo	objectDigestInfo;
		private readonly int version;

		public static Holder GetInstance(
            object obj)
        {
            if (obj is Holder)
            {
                return (Holder) obj;
            }

			if (obj is Asn1Sequence)
            {
                return new Holder((Asn1Sequence) obj);
            }

			if (obj is Asn1TaggedObject)
			{
				return new Holder((Asn1TaggedObject) obj);
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		/**
		 * Constructor for a holder for an v1 attribute certificate.
		 * 
		 * @param tagObj The ASN.1 tagged holder object.
		 */
		public Holder(
			Asn1TaggedObject tagObj)
		{
			switch (tagObj.TagNo)
			{
				case 0:
					baseCertificateID = IssuerSerial.GetInstance(tagObj, false);
					break;
				case 1:
					entityName = GeneralNames.GetInstance(tagObj, false);
					break;
				default:
					throw new ArgumentException("unknown tag in Holder");
			}

			this.version = 0;
		}

		/**
		 * Constructor for a holder for an v2 attribute certificate. *
		 * 
		 * @param seq The ASN.1 sequence.
		 */
		private Holder(
            Asn1Sequence seq)
        {
			if (seq.Count > 3)
				throw new ArgumentException("Bad sequence size: " + seq.Count);

			for (int i = 0; i != seq.Count; i++)
            {
				Asn1TaggedObject tObj = Asn1TaggedObject.GetInstance(seq[i]);

				switch (tObj.TagNo)
                {
                    case 0:
                        baseCertificateID = IssuerSerial.GetInstance(tObj, false);
                        break;
                    case 1:
                        entityName = GeneralNames.GetInstance(tObj, false);
                        break;
                    case 2:
                        objectDigestInfo = ObjectDigestInfo.GetInstance(tObj, false);
                        break;
                    default:
                        throw new ArgumentException("unknown tag in Holder");
                }
            }

			this.version = 1;
		}

		public Holder(
			IssuerSerial baseCertificateID)
			: this(baseCertificateID, 1)
		{
		}

		/**
		 * Constructs a holder from a IssuerSerial.
		 * @param baseCertificateID The IssuerSerial.
		 * @param version The version of the attribute certificate. 
		 */
		public Holder(
			IssuerSerial	baseCertificateID,
			int				version)
		{
			this.baseCertificateID = baseCertificateID;
			this.version = version;
		}

		/**
		 * Returns 1 for v2 attribute certificates or 0 for v1 attribute
		 * certificates. 
		 * @return The version of the attribute certificate.
		 */
		public int Version
		{
			get { return version; }
		}

		/**
		 * Constructs a holder with an entityName for v2 attribute certificates or
		 * with a subjectName for v1 attribute certificates.
		 * 
		 * @param entityName The entity or subject name.
		 */
		public Holder(
			GeneralNames entityName)
			: this(entityName, 1)
		{
		}

		/**
		 * Constructs a holder with an entityName for v2 attribute certificates or
		 * with a subjectName for v1 attribute certificates.
		 * 
		 * @param entityName The entity or subject name.
		 * @param version The version of the attribute certificate. 
		 */
		public Holder(
			GeneralNames	entityName,
			int				version)
		{
			this.entityName = entityName;
			this.version = version;
		}

		/**
		 * Constructs a holder from an object digest info.
		 * 
		 * @param objectDigestInfo The object digest info object.
		 */
		public Holder(
			ObjectDigestInfo objectDigestInfo)
		{
			this.objectDigestInfo = objectDigestInfo;
			this.version = 1;
		}

		public IssuerSerial BaseCertificateID
		{
			get { return baseCertificateID; }
		}

		/**
		 * Returns the entityName for an v2 attribute certificate or the subjectName
		 * for an v1 attribute certificate.
		 * 
		 * @return The entityname or subjectname.
		 */
		public GeneralNames EntityName
		{
			get { return entityName; }
		}

		public ObjectDigestInfo ObjectDigestInfo
		{
			get { return objectDigestInfo; }
		}

		/**
         * The Holder object.
         * <pre>
         *  Holder ::= Sequence {
         *        baseCertificateID   [0] IssuerSerial OPTIONAL,
         *                 -- the issuer and serial number of
         *                 -- the holder's Public Key Certificate
         *        entityName          [1] GeneralNames OPTIONAL,
         *                 -- the name of the claimant or role
         *        objectDigestInfo    [2] ObjectDigestInfo OPTIONAL
         *                 -- used to directly authenticate the holder,
         *                 -- for example, an executable
         *  }
         * </pre>
         */
		public override Asn1Object ToAsn1Object()
		{
			if (version == 1)
			{
				Asn1EncodableVector v = new Asn1EncodableVector();

				if (baseCertificateID != null)
				{
					v.Add(new DerTaggedObject(false, 0, baseCertificateID));
				}

				if (entityName != null)
				{
					v.Add(new DerTaggedObject(false, 1, entityName));
				}

				if (objectDigestInfo != null)
				{
					v.Add(new DerTaggedObject(false, 2, objectDigestInfo));
				}

				return new DerSequence(v);
			}

			if (entityName != null)
			{
				return new DerTaggedObject(false, 1, entityName);
			}

			return new DerTaggedObject(false, 0, baseCertificateID);
		}
	}
}
