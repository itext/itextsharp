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

namespace Org.BouncyCastle.Asn1.Cmp
{
	public class CertRepMessage
		: Asn1Encodable
	{
		private readonly Asn1Sequence caPubs;
		private readonly Asn1Sequence response;
		
		private CertRepMessage(Asn1Sequence seq)
		{
			int index = 0;

			if (seq.Count > 1)
			{
				caPubs = Asn1Sequence.GetInstance((Asn1TaggedObject)seq[index++], true);
			}

			response = Asn1Sequence.GetInstance(seq[index]);
		}

		public static CertRepMessage GetInstance(object obj)
		{
			if (obj is CertRepMessage)
				return (CertRepMessage)obj;

			if (obj is Asn1Sequence)
				return new CertRepMessage((Asn1Sequence)obj);

			throw new ArgumentException("Invalid object: " + obj.GetType().Name, "obj");
		}

		public CertRepMessage(CmpCertificate[] caPubs, CertResponse[] response)
		{
			if (response == null)
				throw new ArgumentNullException("response");

			if (caPubs != null)
			{
				this.caPubs = new DerSequence(caPubs);
			}

			this.response = new DerSequence(response);
		}

		public virtual CmpCertificate[] GetCAPubs()
		{
			if (caPubs == null)
				return null;

			CmpCertificate[] results = new CmpCertificate[caPubs.Count];
			for (int i = 0; i != results.Length; ++i)
			{
				results[i] = CmpCertificate.GetInstance(caPubs[i]);
			}
			return results;
		}

		public virtual CertResponse[] GetResponse()
		{
			CertResponse[] results = new CertResponse[response.Count];
			for (int i = 0; i != results.Length; ++i)
			{
				results[i] = CertResponse.GetInstance(response[i]);
			}
			return results;
		}

		/**
		 * <pre>
		 * CertRepMessage ::= SEQUENCE {
		 *                          caPubs       [1] SEQUENCE SIZE (1..MAX) OF CMPCertificate
		 *                                                                             OPTIONAL,
		 *                          response         SEQUENCE OF CertResponse
		 * }
		 * </pre>
		 * @return a basic ASN.1 object representation.
		 */
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector();

			if (caPubs != null)
			{
				v.Add(new DerTaggedObject(true, 1, caPubs));
			}

			v.Add(response);

			return new DerSequence(v);
		}
	}
}
