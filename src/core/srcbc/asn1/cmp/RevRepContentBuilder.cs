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

using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Cmp
{
	public class RevRepContentBuilder
	{
		private readonly Asn1EncodableVector status = new Asn1EncodableVector();
		private readonly Asn1EncodableVector revCerts = new Asn1EncodableVector();
		private readonly Asn1EncodableVector crls = new Asn1EncodableVector();

		public virtual RevRepContentBuilder Add(PkiStatusInfo status)
		{
			this.status.Add(status);
			return this;
		}

		public virtual RevRepContentBuilder Add(PkiStatusInfo status, CertId certId)
		{
			if (this.status.Count != this.revCerts.Count)
				throw new InvalidOperationException("status and revCerts sequence must be in common order");

			this.status.Add(status);
			this.revCerts.Add(certId);
			return this;
		}

		public virtual RevRepContentBuilder AddCrl(CertificateList crl)
		{
			this.crls.Add(crl);
			return this;
		}

		public virtual RevRepContent Build()
		{
			Asn1EncodableVector v = new Asn1EncodableVector();

			v.Add(new DerSequence(status));

			if (revCerts.Count != 0)
			{
				v.Add(new DerTaggedObject(true, 0, new DerSequence(revCerts)));
			}

			if (crls.Count != 0)
			{
				v.Add(new DerTaggedObject(true, 1, new DerSequence(crls)));
			}

			return RevRepContent.GetInstance(new DerSequence(v));
		}
	}
}
