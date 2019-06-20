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

namespace Org.BouncyCastle.Asn1.X509
{
    /**
     * <code>UserNotice</code> class, used in
     * <code>CertificatePolicies</code> X509 extensions (in policy
     * qualifiers).
     * <pre>
     * UserNotice ::= Sequence {
     *      noticeRef        NoticeReference OPTIONAL,
     *      explicitText     DisplayText OPTIONAL}
     *
     * </pre>
     *
     * @see PolicyQualifierId
     * @see PolicyInformation
     */
    public class UserNotice
        : Asn1Encodable
    {
        internal NoticeReference	noticeRef;
        internal DisplayText		explicitText;

		/**
         * Creates a new <code>UserNotice</code> instance.
         *
         * @param noticeRef a <code>NoticeReference</code> value
         * @param explicitText a <code>DisplayText</code> value
         */
        public UserNotice(
            NoticeReference	noticeRef,
            DisplayText		explicitText)
        {
            this.noticeRef = noticeRef;
            this.explicitText = explicitText;
        }

		/**
         * Creates a new <code>UserNotice</code> instance.
         *
         * @param noticeRef a <code>NoticeReference</code> value
         * @param str the explicitText field as a string.
         */
        public UserNotice(
            NoticeReference	noticeRef,
            string			str)
        {
            this.noticeRef = noticeRef;
            this.explicitText = new DisplayText(str);
        }

		/**
		 * Creates a new <code>UserNotice</code> instance.
		 * <p>Useful from reconstructing a <code>UserNotice</code> instance
		 * from its encodable/encoded form.
		 *
		 * @param as an <code>ASN1Sequence</code> value obtained from either
		 * calling @{link toASN1Object()} for a <code>UserNotice</code>
		 * instance or from parsing it from a DER-encoded stream.</p>
		 */
		public UserNotice(
			Asn1Sequence seq)
		{
			if (seq.Count == 2)
			{
				noticeRef = NoticeReference.GetInstance(seq[0]);
				explicitText = DisplayText.GetInstance(seq[1]);
			}
			else if (seq.Count == 1)
			{
				if (seq[0].ToAsn1Object() is Asn1Sequence)
				{
					noticeRef = NoticeReference.GetInstance(seq[0]);
				}
				else
				{
					explicitText = DisplayText.GetInstance(seq[0]);
				}
			}
			else
			{
				throw new ArgumentException("Bad sequence size: " + seq.Count);
			}
        }

		public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector av = new Asn1EncodableVector();

			if (noticeRef != null)
            {
                av.Add(noticeRef);
            }

			if (explicitText != null)
            {
                av.Add(explicitText);
            }

			return new DerSequence(av);
        }
    }
}
