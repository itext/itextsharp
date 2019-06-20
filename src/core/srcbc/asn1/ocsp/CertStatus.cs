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

namespace Org.BouncyCastle.Asn1.Ocsp
{
    public class CertStatus
        : Asn1Encodable, IAsn1Choice
    {
        private readonly int			tagNo;
        private readonly Asn1Encodable	value;

		/**
         * create a CertStatus object with a tag of zero.
         */
        public CertStatus()
        {
            tagNo = 0;
            value = DerNull.Instance;
        }

		public CertStatus(
            RevokedInfo info)
        {
            tagNo = 1;
            value = info;
        }

		public CertStatus(
            int				tagNo,
            Asn1Encodable	value)
        {
            this.tagNo = tagNo;
            this.value = value;
        }

		public CertStatus(
            Asn1TaggedObject choice)
        {
            this.tagNo = choice.TagNo;

			switch (choice.TagNo)
            {
				case 1:
					value = RevokedInfo.GetInstance(choice, false);
					break;
				case 0:
				case 2:
					value = DerNull.Instance;
					break;
            }
        }

		public static CertStatus GetInstance(
            object obj)
        {
            if (obj == null || obj is CertStatus)
            {
                return (CertStatus)obj;
            }

			if (obj is Asn1TaggedObject)
            {
                return new CertStatus((Asn1TaggedObject)obj);
            }

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
        }

		public int TagNo
		{
			get { return tagNo; }
		}

		public Asn1Encodable Status
		{
			get { return value; }
		}

		/**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         *  CertStatus ::= CHOICE {
         *                  good        [0]     IMPLICIT Null,
         *                  revoked     [1]     IMPLICIT RevokedInfo,
         *                  unknown     [2]     IMPLICIT UnknownInfo }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            return new DerTaggedObject(false, tagNo, value);
        }
    }
}
