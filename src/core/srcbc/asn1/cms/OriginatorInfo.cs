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

namespace Org.BouncyCastle.Asn1.Cms
{
    public class OriginatorInfo
        : Asn1Encodable
    {
        private Asn1Set certs;
        private Asn1Set crls;

        public OriginatorInfo(
            Asn1Set certs,
            Asn1Set crls)
        {
            this.certs = certs;
            this.crls = crls;
        }

		public OriginatorInfo(
            Asn1Sequence seq)
        {
            switch (seq.Count)
            {
            case 0:     // empty
                break;
            case 1:
                Asn1TaggedObject o = (Asn1TaggedObject) seq[0];
                switch (o.TagNo)
                {
                case 0 :
                    certs = Asn1Set.GetInstance(o, false);
                    break;
                case 1 :
                    crls = Asn1Set.GetInstance(o, false);
                    break;
                default:
                    throw new ArgumentException("Bad tag in OriginatorInfo: " + o.TagNo);
                }
                break;
            case 2:
                certs = Asn1Set.GetInstance((Asn1TaggedObject) seq[0], false);
                crls  = Asn1Set.GetInstance((Asn1TaggedObject) seq[1], false);
                break;
            default:
                throw new ArgumentException("OriginatorInfo too big");
            }
        }

		/**
         * return an OriginatorInfo object from a tagged object.
         *
         * @param obj the tagged object holding the object we want.
         * @param explicitly true if the object is meant to be explicitly
         *              tagged false otherwise.
         * @exception ArgumentException if the object held by the
         *          tagged object cannot be converted.
         */
        public static OriginatorInfo GetInstance(
            Asn1TaggedObject	obj,
            bool				explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

		/**
         * return an OriginatorInfo object from the given object.
         *
         * @param obj the object we want converted.
         * @exception ArgumentException if the object cannot be converted.
         */
        public static OriginatorInfo GetInstance(
            object obj)
        {
            if (obj == null || obj is OriginatorInfo)
                return (OriginatorInfo)obj;

			if (obj is Asn1Sequence)
                return new OriginatorInfo((Asn1Sequence)obj);

			throw new ArgumentException("Invalid OriginatorInfo: " + obj.GetType().Name);
        }

		public Asn1Set Certificates
		{
			get { return certs; }
		}

		public Asn1Set Crls
		{
			get { return crls; }
		}

		/**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         * OriginatorInfo ::= Sequence {
         *     certs [0] IMPLICIT CertificateSet OPTIONAL,
         *     crls [1] IMPLICIT CertificateRevocationLists OPTIONAL
         * }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector();

			if (certs != null)
            {
                v.Add(new DerTaggedObject(false, 0, certs));
            }

			if (crls != null)
            {
                v.Add(new DerTaggedObject(false, 1, crls));
            }

			return new DerSequence(v);
        }
    }
}
