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
     * Generator for Version 1 TbsCertificateStructures.
     * <pre>
     * TbsCertificate ::= Sequence {
     *      version          [ 0 ]  Version DEFAULT v1(0),
     *      serialNumber            CertificateSerialNumber,
     *      signature               AlgorithmIdentifier,
     *      issuer                  Name,
     *      validity                Validity,
     *      subject                 Name,
     *      subjectPublicKeyInfo    SubjectPublicKeyInfo,
     *      }
     * </pre>
     *
     */
    public class V1TbsCertificateGenerator
    {
        internal DerTaggedObject		version = new DerTaggedObject(0, new DerInteger(0));
        internal DerInteger				serialNumber;
        internal AlgorithmIdentifier	signature;
        internal X509Name				issuer;
        internal Time					startDate, endDate;
        internal X509Name				subject;
        internal SubjectPublicKeyInfo	subjectPublicKeyInfo;

		public V1TbsCertificateGenerator()
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
            Time startDate)
        {
            this.startDate = startDate;
        }

		public void SetStartDate(
            DerUtcTime startDate)
        {
            this.startDate = new Time(startDate);
        }

		public void SetEndDate(
            Time endDate)
        {
            this.endDate = endDate;
        }

		public void SetEndDate(
            DerUtcTime endDate)
        {
            this.endDate = new Time(endDate);
        }

		public void SetSubject(
            X509Name subject)
        {
            this.subject = subject;
        }

		public void SetSubjectPublicKeyInfo(
            SubjectPublicKeyInfo pubKeyInfo)
        {
            this.subjectPublicKeyInfo = pubKeyInfo;
        }

		public TbsCertificateStructure GenerateTbsCertificate()
        {
            if ((serialNumber == null) || (signature == null)
                || (issuer == null) || (startDate == null) || (endDate == null)
                || (subject == null) || (subjectPublicKeyInfo == null))
            {
                throw new InvalidOperationException("not all mandatory fields set in V1 TBScertificate generator");
            }

			return new TbsCertificateStructure(
				new DerSequence(
					//version, - not required as default value
					serialNumber,
					signature,
					issuer,
					new DerSequence(startDate, endDate), // before and after dates
					subject,
					subjectPublicKeyInfo));
        }
    }
}
