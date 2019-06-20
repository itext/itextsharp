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
     * Generator for Version 3 TbsCertificateStructures.
     * <pre>
     * TbsCertificate ::= Sequence {
     *      version          [ 0 ]  Version DEFAULT v1(0),
     *      serialNumber            CertificateSerialNumber,
     *      signature               AlgorithmIdentifier,
     *      issuer                  Name,
     *      validity                Validity,
     *      subject                 Name,
     *      subjectPublicKeyInfo    SubjectPublicKeyInfo,
     *      issuerUniqueID    [ 1 ] IMPLICIT UniqueIdentifier OPTIONAL,
     *      subjectUniqueID   [ 2 ] IMPLICIT UniqueIdentifier OPTIONAL,
     *      extensions        [ 3 ] Extensions OPTIONAL
     *      }
     * </pre>
     *
     */
    public class V3TbsCertificateGenerator
    {
        internal DerTaggedObject         version = new DerTaggedObject(0, new DerInteger(2));
        internal DerInteger              serialNumber;
        internal AlgorithmIdentifier     signature;
        internal X509Name                issuer;
        internal Time                    startDate, endDate;
        internal X509Name                subject;
        internal SubjectPublicKeyInfo    subjectPublicKeyInfo;
        internal X509Extensions          extensions;

		private bool altNamePresentAndCritical;
		private DerBitString issuerUniqueID;
		private DerBitString subjectUniqueID;

		public V3TbsCertificateGenerator()
        {
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
            X509Name issuer)
        {
            this.issuer = issuer;
        }

		public void SetStartDate(
            DerUtcTime startDate)
        {
            this.startDate = new Time(startDate);
        }

		public void SetStartDate(
            Time startDate)
        {
            this.startDate = startDate;
        }

		public void SetEndDate(
            DerUtcTime endDate)
        {
            this.endDate = new Time(endDate);
        }

		public void SetEndDate(
            Time endDate)
        {
            this.endDate = endDate;
        }

		public void SetSubject(
            X509Name subject)
        {
            this.subject = subject;
        }

		public void SetIssuerUniqueID(
			DerBitString uniqueID)
		{
			this.issuerUniqueID = uniqueID;
		}

		public void SetSubjectUniqueID(
			DerBitString uniqueID)
		{
			this.subjectUniqueID = uniqueID;
		}

		public void SetSubjectPublicKeyInfo(
            SubjectPublicKeyInfo pubKeyInfo)
        {
            this.subjectPublicKeyInfo = pubKeyInfo;
        }

		public void SetExtensions(
            X509Extensions extensions)
        {
            this.extensions = extensions;

			if (extensions != null)
			{
				X509Extension altName = extensions.GetExtension(X509Extensions.SubjectAlternativeName);

				if (altName != null && altName.IsCritical)
				{
					altNamePresentAndCritical = true;
				}
			}
		}

		public TbsCertificateStructure GenerateTbsCertificate()
        {
            if ((serialNumber == null) || (signature == null)
                || (issuer == null) || (startDate == null) || (endDate == null)
				|| (subject == null && !altNamePresentAndCritical)
				|| (subjectPublicKeyInfo == null))
            {
                throw new InvalidOperationException("not all mandatory fields set in V3 TBScertificate generator");
            }

			DerSequence validity = new DerSequence(startDate, endDate); // before and after dates

			Asn1EncodableVector v = new Asn1EncodableVector(
				version, serialNumber, signature, issuer, validity);

			if (subject != null)
			{
				v.Add(subject);
			}
			else
			{
				v.Add(DerSequence.Empty);
			}

			v.Add(subjectPublicKeyInfo);

			if (issuerUniqueID != null)
			{
				v.Add(new DerTaggedObject(false, 1, issuerUniqueID));
			}

			if (subjectUniqueID != null)
			{
				v.Add(new DerTaggedObject(false, 2, subjectUniqueID));
			}

			if (extensions != null)
            {
                v.Add(new DerTaggedObject(3, extensions));
            }

			return new TbsCertificateStructure(new DerSequence(v));
        }
    }
}
