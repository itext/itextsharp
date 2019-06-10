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

namespace Org.BouncyCastle.Asn1.Ocsp
{
    public class CrlID
        : Asn1Encodable
    {
        private readonly DerIA5String		crlUrl;
        private readonly DerInteger			crlNum;
        private readonly DerGeneralizedTime	crlTime;

		// TODO Add GetInstance method(s) and amke this private?
		public CrlID(
            Asn1Sequence seq)
        {
			foreach (Asn1TaggedObject o in seq)
			{
				switch (o.TagNo)
                {
                case 0:
                    crlUrl = DerIA5String.GetInstance(o, true);
                    break;
                case 1:
                    crlNum = DerInteger.GetInstance(o, true);
                    break;
                case 2:
                    crlTime = DerGeneralizedTime.GetInstance(o, true);
                    break;
                default:
                    throw new ArgumentException("unknown tag number: " + o.TagNo);
                }
            }
        }

		public DerIA5String CrlUrl
		{
			get { return crlUrl; }
		}

		public DerInteger CrlNum
		{
			get { return crlNum; }
		}

		public DerGeneralizedTime CrlTime
		{
			get { return crlTime; }
		}

		/**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         * CrlID ::= Sequence {
         *     crlUrl               [0]     EXPLICIT IA5String OPTIONAL,
         *     crlNum               [1]     EXPLICIT Integer OPTIONAL,
         *     crlTime              [2]     EXPLICIT GeneralizedTime OPTIONAL }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector();

			if (crlUrl != null)
            {
                v.Add(new DerTaggedObject(true, 0, crlUrl));
            }

			if (crlNum != null)
            {
                v.Add(new DerTaggedObject(true, 1, crlNum));
            }

			if (crlTime != null)
            {
                v.Add(new DerTaggedObject(true, 2, crlTime));
            }

			return new DerSequence(v);
        }
    }
}
