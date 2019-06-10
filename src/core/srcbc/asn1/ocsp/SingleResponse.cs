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
    public class SingleResponse
        : Asn1Encodable
    {
        private readonly CertID              certID;
        private readonly CertStatus          certStatus;
        private readonly DerGeneralizedTime  thisUpdate;
        private readonly DerGeneralizedTime  nextUpdate;
        private readonly X509Extensions      singleExtensions;

		public SingleResponse(
            CertID              certID,
            CertStatus          certStatus,
            DerGeneralizedTime  thisUpdate,
            DerGeneralizedTime  nextUpdate,
            X509Extensions      singleExtensions)
        {
            this.certID = certID;
            this.certStatus = certStatus;
            this.thisUpdate = thisUpdate;
            this.nextUpdate = nextUpdate;
            this.singleExtensions = singleExtensions;
        }

		public SingleResponse(
            Asn1Sequence seq)
        {
            this.certID = CertID.GetInstance(seq[0]);
            this.certStatus = CertStatus.GetInstance(seq[1]);
            this.thisUpdate = (DerGeneralizedTime)seq[2];

			if (seq.Count > 4)
            {
                this.nextUpdate = DerGeneralizedTime.GetInstance(
					(Asn1TaggedObject) seq[3], true);
                this.singleExtensions = X509Extensions.GetInstance(
					(Asn1TaggedObject) seq[4], true);
            }
            else if (seq.Count > 3)
            {
                Asn1TaggedObject o = (Asn1TaggedObject) seq[3];

				if (o.TagNo == 0)
                {
                    this.nextUpdate = DerGeneralizedTime.GetInstance(o, true);
                }
                else
                {
                    this.singleExtensions = X509Extensions.GetInstance(o, true);
                }
            }
        }

		public static SingleResponse GetInstance(
            Asn1TaggedObject	obj,
            bool				explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

		public static SingleResponse GetInstance(
            object obj)
        {
            if (obj == null || obj is SingleResponse)
            {
                return (SingleResponse)obj;
            }

			if (obj is Asn1Sequence)
            {
                return new SingleResponse((Asn1Sequence)obj);
            }

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
        }

		public CertID CertId
		{
			get { return certID; }
		}

		public CertStatus CertStatus
		{
			get { return certStatus; }
		}

		public DerGeneralizedTime ThisUpdate
		{
			get { return thisUpdate; }
		}

		public DerGeneralizedTime NextUpdate
		{
			get { return nextUpdate; }
		}

		public X509Extensions SingleExtensions
		{
			get { return singleExtensions; }
		}

		/**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         *  SingleResponse ::= Sequence {
         *          certID                       CertID,
         *          certStatus                   CertStatus,
         *          thisUpdate                   GeneralizedTime,
         *          nextUpdate         [0]       EXPLICIT GeneralizedTime OPTIONAL,
         *          singleExtensions   [1]       EXPLICIT Extensions OPTIONAL }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(
				certID, certStatus, thisUpdate);

			if (nextUpdate != null)
            {
                v.Add(new DerTaggedObject(true, 0, nextUpdate));
            }

			if (singleExtensions != null)
            {
                v.Add(new DerTaggedObject(true, 1, singleExtensions));
            }

			return new DerSequence(v);
        }
    }
}
