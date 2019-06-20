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

using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Asn1.Esf
{
	/// <remarks>
	/// RFC 3126: 4.3.1 Certificate Values Attribute Definition
	/// <code>
	/// CertificateValues ::= SEQUENCE OF Certificate
	/// </code>
	/// </remarks>
	public class CertificateValues
		: Asn1Encodable
	{
		private readonly Asn1Sequence certificates;

		public static CertificateValues GetInstance(
			object obj)
		{
			if (obj == null || obj is CertificateValues)
				return (CertificateValues) obj;

			if (obj is Asn1Sequence)
				return new CertificateValues((Asn1Sequence) obj);

			throw new ArgumentException(
				"Unknown object in 'CertificateValues' factory: "
					+ obj.GetType().Name,
				"obj");
		}

		private CertificateValues(
			Asn1Sequence seq)
		{
			if (seq == null)
				throw new ArgumentNullException("seq");

			foreach (Asn1Encodable ae in seq)
			{
				X509CertificateStructure.GetInstance(ae.ToAsn1Object());
			}

			this.certificates = seq;
		}

		public CertificateValues(
			params X509CertificateStructure[] certificates)
		{
			if (certificates == null)
				throw new ArgumentNullException("certificates");

			this.certificates = new DerSequence(certificates);
		}

		public CertificateValues(
			IEnumerable certificates)
		{
			if (certificates == null)
				throw new ArgumentNullException("certificates");
			if (!CollectionUtilities.CheckElementsAreOfType(certificates, typeof(X509CertificateStructure)))
				throw new ArgumentException("Must contain only 'X509CertificateStructure' objects", "certificates");

			this.certificates = new DerSequence(
				Asn1EncodableVector.FromEnumerable(certificates));
		}

		public X509CertificateStructure[] GetCertificates()
		{
			X509CertificateStructure[] result = new X509CertificateStructure[certificates.Count];
			for (int i = 0; i < certificates.Count; ++i)
			{
				result[i] = X509CertificateStructure.GetInstance(certificates[i]);
			}
			return result;
		}

		public override Asn1Object ToAsn1Object()
		{
			return certificates;
		}
 	}
}
