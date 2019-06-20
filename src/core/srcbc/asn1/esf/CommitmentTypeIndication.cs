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

namespace Org.BouncyCastle.Asn1.Esf
{
    public class CommitmentTypeIndication
        : Asn1Encodable
    {
        private readonly DerObjectIdentifier	commitmentTypeId;
        private readonly Asn1Sequence			commitmentTypeQualifier;

		public static CommitmentTypeIndication GetInstance(
			object obj)
		{
			if (obj == null || obj is CommitmentTypeIndication)
				return (CommitmentTypeIndication) obj;

			if (obj is Asn1Sequence)
				return new CommitmentTypeIndication((Asn1Sequence) obj);

			throw new ArgumentException(
				"Unknown object in 'CommitmentTypeIndication' factory: "
					+ obj.GetType().Name,
				"obj");
		}

		public CommitmentTypeIndication(
            Asn1Sequence seq)
        {
			if (seq == null)
				throw new ArgumentNullException("seq");
			if (seq.Count < 1 || seq.Count > 2)
				throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");

			this.commitmentTypeId = (DerObjectIdentifier) seq[0].ToAsn1Object();

			if (seq.Count > 1)
            {
                this.commitmentTypeQualifier = (Asn1Sequence) seq[1].ToAsn1Object();
            }
        }

		public CommitmentTypeIndication(
            DerObjectIdentifier commitmentTypeId)
			: this(commitmentTypeId, null)
        {
        }

		public CommitmentTypeIndication(
            DerObjectIdentifier	commitmentTypeId,
            Asn1Sequence		commitmentTypeQualifier)
        {
			if (commitmentTypeId == null)
				throw new ArgumentNullException("commitmentTypeId");

			this.commitmentTypeId = commitmentTypeId;

			if (commitmentTypeQualifier != null)
			{
				this.commitmentTypeQualifier = commitmentTypeQualifier;
			}
        }

		public DerObjectIdentifier CommitmentTypeID
		{
			get { return commitmentTypeId; }
		}

		public Asn1Sequence CommitmentTypeQualifier
		{
			get { return commitmentTypeQualifier; }
		}

		/**
        * <pre>
        * CommitmentTypeIndication ::= SEQUENCE {
        *      commitmentTypeId   CommitmentTypeIdentifier,
        *      commitmentTypeQualifier   SEQUENCE SIZE (1..MAX) OF
        *              CommitmentTypeQualifier OPTIONAL }
        * </pre>
        */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(commitmentTypeId);

			if (commitmentTypeQualifier != null)
            {
                v.Add(commitmentTypeQualifier);
            }

			return new DerSequence(v);
        }
    }
}
