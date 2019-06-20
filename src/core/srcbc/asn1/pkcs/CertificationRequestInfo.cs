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

namespace Org.BouncyCastle.Asn1.Pkcs
{
    /**
     * Pkcs10 CertificationRequestInfo object.
     * <pre>
     *  CertificationRequestInfo ::= Sequence {
     *   version             Integer { v1(0) } (v1,...),
     *   subject             Name,
     *   subjectPKInfo   SubjectPublicKeyInfo{{ PKInfoAlgorithms }},
     *   attributes          [0] Attributes{{ CRIAttributes }}
     *  }
     *
     *  Attributes { ATTRIBUTE:IOSet } ::= Set OF Attr{{ IOSet }}
     *
     *  Attr { ATTRIBUTE:IOSet } ::= Sequence {
     *    type    ATTRIBUTE.&amp;id({IOSet}),
     *    values  Set SIZE(1..MAX) OF ATTRIBUTE.&amp;Type({IOSet}{\@type})
     *  }
     * </pre>
     */
    public class CertificationRequestInfo
        : Asn1Encodable
    {
        internal DerInteger				version = new DerInteger(0);
        internal X509Name				subject;
        internal SubjectPublicKeyInfo	subjectPKInfo;
        internal Asn1Set				attributes;

		public static CertificationRequestInfo GetInstance(
            object  obj)
        {
            if (obj is CertificationRequestInfo)
            {
                return (CertificationRequestInfo) obj;
            }

			if (obj is Asn1Sequence)
            {
                return new CertificationRequestInfo((Asn1Sequence) obj);
            }

			throw new ArgumentException("Unknown object in factory: " + obj.GetType().FullName, "obj");
		}

		public CertificationRequestInfo(
            X509Name				subject,
            SubjectPublicKeyInfo	pkInfo,
            Asn1Set					attributes)
        {
            this.subject = subject;
            this.subjectPKInfo = pkInfo;
            this.attributes = attributes;

			if (subject == null || version == null || subjectPKInfo == null)
            {
                throw new ArgumentException(
					"Not all mandatory fields set in CertificationRequestInfo generator.");
            }
        }

		private CertificationRequestInfo(
            Asn1Sequence seq)
        {
            version = (DerInteger) seq[0];

			subject = X509Name.GetInstance(seq[1]);
            subjectPKInfo = SubjectPublicKeyInfo.GetInstance(seq[2]);

			//
            // some CertificationRequestInfo objects seem to treat this field
            // as optional.
            //
            if (seq.Count > 3)
            {
                DerTaggedObject tagobj = (DerTaggedObject) seq[3];
                attributes = Asn1Set.GetInstance(tagobj, false);
            }

			if (subject == null || version == null || subjectPKInfo == null)
            {
                throw new ArgumentException(
					"Not all mandatory fields set in CertificationRequestInfo generator.");
            }
        }

		public DerInteger Version
		{
			get { return version; }
		}

		public X509Name Subject
		{
			get { return subject; }
		}

		public SubjectPublicKeyInfo SubjectPublicKeyInfo
		{
			get { return subjectPKInfo; }
		}

		public Asn1Set Attributes
		{
			get { return attributes; }
		}

		public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(
				version, subject, subjectPKInfo);

			if (attributes != null)
            {
                v.Add(new DerTaggedObject(false, 0, attributes));
            }

			return new DerSequence(v);
        }
    }
}
