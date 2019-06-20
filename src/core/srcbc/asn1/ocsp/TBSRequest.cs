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
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;

using System;

namespace Org.BouncyCastle.Asn1.Ocsp
{
    public class TbsRequest
        : Asn1Encodable
    {
		private static readonly DerInteger V1 = new DerInteger(0);

		private readonly DerInteger      version;
        private readonly GeneralName     requestorName;
        private readonly Asn1Sequence    requestList;
        private readonly X509Extensions  requestExtensions;

		private bool versionSet;

		public static TbsRequest GetInstance(
			Asn1TaggedObject	obj,
			bool				explicitly)
		{
			return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
		}

		public static TbsRequest GetInstance(
			object obj)
		{
			if (obj == null || obj is TbsRequest)
			{
				return (TbsRequest)obj;
			}

			if (obj is Asn1Sequence)
			{
				return new TbsRequest((Asn1Sequence)obj);
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		public TbsRequest(
            GeneralName     requestorName,
            Asn1Sequence    requestList,
            X509Extensions  requestExtensions)
        {
            this.version = V1;
            this.requestorName = requestorName;
            this.requestList = requestList;
            this.requestExtensions = requestExtensions;
        }

		private TbsRequest(
            Asn1Sequence seq)
        {
            int index = 0;

			Asn1Encodable enc = seq[0];
			if (enc is Asn1TaggedObject)
            {
                Asn1TaggedObject o = (Asn1TaggedObject) enc;

				if (o.TagNo == 0)
                {
					versionSet = true;
					version = DerInteger.GetInstance(o, true);
                    index++;
                }
                else
                {
                    version = V1;
                }
            }
            else
            {
                version = V1;
            }

			if (seq[index] is Asn1TaggedObject)
            {
                requestorName = GeneralName.GetInstance((Asn1TaggedObject) seq[index++], true);
            }

			requestList = (Asn1Sequence) seq[index++];

			if (seq.Count == (index + 1))
            {
                requestExtensions = X509Extensions.GetInstance((Asn1TaggedObject) seq[index], true);
            }
        }

		public DerInteger Version
		{
			get { return version; }
		}

		public GeneralName RequestorName
		{
			get { return requestorName; }
		}

		public Asn1Sequence RequestList
		{
			get { return requestList; }
		}

		public X509Extensions RequestExtensions
		{
			get { return requestExtensions; }
		}

		/**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         * TBSRequest      ::=     Sequence {
         *     version             [0]     EXPLICIT Version DEFAULT v1,
         *     requestorName       [1]     EXPLICIT GeneralName OPTIONAL,
         *     requestList                 Sequence OF Request,
         *     requestExtensions   [2]     EXPLICIT Extensions OPTIONAL }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector();

			//
			// if default don't include - unless explicitly provided. Not strictly correct
			// but required for some requests
			//
			if (!version.Equals(V1) || versionSet)
			{
                v.Add(new DerTaggedObject(true, 0, version));
            }

			if (requestorName != null)
            {
                v.Add(new DerTaggedObject(true, 1, requestorName));
            }

			v.Add(requestList);

			if (requestExtensions != null)
            {
                v.Add(new DerTaggedObject(true, 2, requestExtensions));
            }

			return new DerSequence(v);
        }
    }
}
