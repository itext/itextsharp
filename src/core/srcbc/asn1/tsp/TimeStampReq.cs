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

using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Tsp
{
	public class TimeStampReq
		: Asn1Encodable
	{
		private readonly DerInteger				version;
		private readonly MessageImprint			messageImprint;
		private readonly DerObjectIdentifier	tsaPolicy;
		private readonly DerInteger				nonce;
		private readonly DerBoolean				certReq;
		private readonly X509Extensions			extensions;

		public static TimeStampReq GetInstance(
			object o)
		{
			if (o == null || o is TimeStampReq)
			{
				return (TimeStampReq) o;
			}

			if (o is Asn1Sequence)
			{
				return new TimeStampReq((Asn1Sequence) o);
			}

			throw new ArgumentException(
				"Unknown object in 'TimeStampReq' factory: " + o.GetType().FullName);
		}

		private TimeStampReq(
			Asn1Sequence seq)
		{
			int nbObjects = seq.Count;
			int seqStart = 0;

			// version
			version = DerInteger.GetInstance(seq[seqStart++]);

			// messageImprint
			messageImprint = MessageImprint.GetInstance(seq[seqStart++]);

			for (int opt = seqStart; opt < nbObjects; opt++)
			{
				// tsaPolicy
				if (seq[opt] is DerObjectIdentifier)
				{
					tsaPolicy = DerObjectIdentifier.GetInstance(seq[opt]);
				}
				// nonce
				else if (seq[opt] is DerInteger)
				{
					nonce = DerInteger.GetInstance(seq[opt]);
				}
				// certReq
				else if (seq[opt] is DerBoolean)
				{
					certReq = DerBoolean.GetInstance(seq[opt]);
				}
				// extensions
				else if (seq[opt] is Asn1TaggedObject)
				{
					Asn1TaggedObject tagged = (Asn1TaggedObject) seq[opt];
					if (tagged.TagNo == 0)
					{
						extensions = X509Extensions.GetInstance(tagged, false);
					}
				}
			}
		}

		public TimeStampReq(
			MessageImprint		messageImprint,
			DerObjectIdentifier	tsaPolicy,
			DerInteger			nonce,
			DerBoolean			certReq,
			X509Extensions		extensions)
		{
			// default
			this.version = new DerInteger(1);

			this.messageImprint = messageImprint;
			this.tsaPolicy = tsaPolicy;
			this.nonce = nonce;
			this.certReq = certReq;
			this.extensions = extensions;
		}

		public DerInteger Version
		{
			get { return version; }
		}

		public MessageImprint MessageImprint
		{
			get { return messageImprint; }
		}

		public DerObjectIdentifier ReqPolicy
		{
			get { return tsaPolicy; }
		}

		public DerInteger Nonce
		{
			get { return nonce; }
		}

		public DerBoolean CertReq
		{
			get { return certReq; }
		}

		public X509Extensions Extensions
		{
			get { return extensions; }
		}

		/**
		 * <pre>
		 * TimeStampReq ::= SEQUENCE  {
		 *  version                      INTEGER  { v1(1) },
		 *  messageImprint               MessageImprint,
		 *    --a hash algorithm OID and the hash value of the data to be
		 *    --time-stamped
		 *  reqPolicy             TSAPolicyId              OPTIONAL,
		 *  nonce                 INTEGER                  OPTIONAL,
		 *  certReq               BOOLEAN                  DEFAULT FALSE,
		 *  extensions            [0] IMPLICIT Extensions  OPTIONAL
		 * }
		 * </pre>
		 */
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector(
				version, messageImprint);

			if (tsaPolicy != null)
			{
				v.Add(tsaPolicy);
			}

			if (nonce != null)
			{
				v.Add(nonce);
			}

			if (certReq != null && certReq.IsTrue)
			{
				v.Add(certReq);
			}

			if (extensions != null)
			{
				v.Add(new DerTaggedObject(false, 0, extensions));
			}

			return new DerSequence(v);
		}
	}
}
