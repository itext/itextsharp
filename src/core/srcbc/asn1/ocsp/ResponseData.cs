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

namespace Org.BouncyCastle.Asn1.Ocsp
{
	public class ResponseData
		: Asn1Encodable
	{
		private static readonly DerInteger V1 = new DerInteger(0);

		private readonly bool                versionPresent;
		private readonly DerInteger          version;
		private readonly ResponderID         responderID;
		private readonly DerGeneralizedTime  producedAt;
		private readonly Asn1Sequence        responses;
		private readonly X509Extensions      responseExtensions;

		public static ResponseData GetInstance(
			Asn1TaggedObject	obj,
			bool				explicitly)
		{
			return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
		}

		public static ResponseData GetInstance(
			object  obj)
		{
			if (obj == null || obj is ResponseData)
			{
				return (ResponseData)obj;
			}

			if (obj is Asn1Sequence)
			{
				return new ResponseData((Asn1Sequence)obj);
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		public ResponseData(
			DerInteger          version,
			ResponderID         responderID,
			DerGeneralizedTime  producedAt,
			Asn1Sequence        responses,
			X509Extensions      responseExtensions)
		{
			this.version = version;
			this.responderID = responderID;
			this.producedAt = producedAt;
			this.responses = responses;
			this.responseExtensions = responseExtensions;
		}

		public ResponseData(
			ResponderID         responderID,
			DerGeneralizedTime  producedAt,
			Asn1Sequence        responses,
			X509Extensions      responseExtensions)
			: this(V1, responderID, producedAt, responses, responseExtensions)
		{
		}

		private ResponseData(
			Asn1Sequence seq)
		{
			int index = 0;

			Asn1Encodable enc = seq[0];
			if (enc is Asn1TaggedObject)
			{
				Asn1TaggedObject o = (Asn1TaggedObject)enc;

				if (o.TagNo == 0)
				{
					this.versionPresent = true;
					this.version = DerInteger.GetInstance(o, true);
					index++;
				}
				else
				{
					this.version = V1;
				}
			}
			else
			{
				this.version = V1;
			}

			this.responderID = ResponderID.GetInstance(seq[index++]);
			this.producedAt = (DerGeneralizedTime)seq[index++];
			this.responses = (Asn1Sequence)seq[index++];

			if (seq.Count > index)
			{
				this.responseExtensions = X509Extensions.GetInstance(
					(Asn1TaggedObject)seq[index], true);
			}
		}

		public DerInteger Version
		{
			get { return version; }
		}

		public ResponderID ResponderID
		{
			get { return responderID; }
		}

		public DerGeneralizedTime ProducedAt
		{
			get { return producedAt; }
		}

		public Asn1Sequence Responses
		{
			get { return responses; }
		}

		public X509Extensions ResponseExtensions
		{
			get { return responseExtensions; }
		}

		/**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         * ResponseData ::= Sequence {
         *     version              [0] EXPLICIT Version DEFAULT v1,
         *     responderID              ResponderID,
         *     producedAt               GeneralizedTime,
         *     responses                Sequence OF SingleResponse,
         *     responseExtensions   [1] EXPLICIT Extensions OPTIONAL }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector();

			if (versionPresent || !version.Equals(V1))
			{
				v.Add(new DerTaggedObject(true, 0, version));
			}

			v.Add(responderID, producedAt, responses);

			if (responseExtensions != null)
            {
                v.Add(new DerTaggedObject(true, 1, responseExtensions));
            }

			return new DerSequence(v);
        }
    }
}
