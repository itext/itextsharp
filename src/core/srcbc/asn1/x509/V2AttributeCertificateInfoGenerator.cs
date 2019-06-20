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
     * Generator for Version 2 AttributeCertificateInfo
     * <pre>
     * AttributeCertificateInfo ::= Sequence {
     *       version              AttCertVersion -- version is v2,
     *       holder               Holder,
     *       issuer               AttCertIssuer,
     *       signature            AlgorithmIdentifier,
     *       serialNumber         CertificateSerialNumber,
     *       attrCertValidityPeriod   AttCertValidityPeriod,
     *       attributes           Sequence OF Attr,
     *       issuerUniqueID       UniqueIdentifier OPTIONAL,
     *       extensions           Extensions OPTIONAL
     * }
     * </pre>
     *
     */
    public class V2AttributeCertificateInfoGenerator
    {
        internal DerInteger				version;
        internal Holder					holder;
        internal AttCertIssuer			issuer;
        internal AlgorithmIdentifier	signature;
        internal DerInteger				serialNumber;
//        internal AttCertValidityPeriod	attrCertValidityPeriod;
        internal Asn1EncodableVector	attributes;
        internal DerBitString			issuerUniqueID;
        internal X509Extensions			extensions;
        internal DerGeneralizedTime		startDate, endDate;

		public V2AttributeCertificateInfoGenerator()
        {
            this.version = new DerInteger(1);
            attributes = new Asn1EncodableVector();
        }

		public void SetHolder(
			Holder holder)
        {
            this.holder = holder;
        }

		public void AddAttribute(
			string			oid,
			Asn1Encodable	value)
        {
            attributes.Add(new AttributeX509(new DerObjectIdentifier(oid), new DerSet(value)));
        }

		/**
         * @param attribute
         */
        public void AddAttribute(AttributeX509 attribute)
        {
            attributes.Add(attribute);
        }

		public void SetSerialNumber(
            DerInteger serialNumber)
        {
            this.serialNumber = serialNumber;
        }

		public void SetSignature(
            AlgorithmIdentifier signature)
        {
            this.signature = signature;
        }

		public void SetIssuer(
            AttCertIssuer issuer)
        {
            this.issuer = issuer;
        }

		public void SetStartDate(
            DerGeneralizedTime startDate)
        {
            this.startDate = startDate;
        }

		public void SetEndDate(
            DerGeneralizedTime endDate)
        {
            this.endDate = endDate;
        }

		public void SetIssuerUniqueID(
            DerBitString issuerUniqueID)
        {
            this.issuerUniqueID = issuerUniqueID;
        }

		public void SetExtensions(
            X509Extensions extensions)
        {
            this.extensions = extensions;
        }

		public AttributeCertificateInfo GenerateAttributeCertificateInfo()
        {
            if ((serialNumber == null) || (signature == null)
                || (issuer == null) || (startDate == null) || (endDate == null)
                || (holder == null) || (attributes == null))
            {
                throw new InvalidOperationException("not all mandatory fields set in V2 AttributeCertificateInfo generator");
            }

			Asn1EncodableVector v = new Asn1EncodableVector(
				version, holder, issuer, signature, serialNumber);

			//
            // before and after dates => AttCertValidityPeriod
            //
            v.Add(new AttCertValidityPeriod(startDate, endDate));

			// Attributes
            v.Add(new DerSequence(attributes));

			if (issuerUniqueID != null)
            {
                v.Add(issuerUniqueID);
            }

			if (extensions != null)
            {
                v.Add(extensions);
            }

			return AttributeCertificateInfo.GetInstance(new DerSequence(v));
        }
    }
}
