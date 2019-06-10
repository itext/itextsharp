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
    /**
    * Commitment type qualifiers, used in the Commitment-Type-Indication attribute (RFC3126).
    *
    * <pre>
    *   CommitmentTypeQualifier ::= SEQUENCE {
    *       commitmentTypeIdentifier  CommitmentTypeIdentifier,
    *       qualifier          ANY DEFINED BY commitmentTypeIdentifier OPTIONAL }
    * </pre>
    */
    public class CommitmentTypeQualifier
        : Asn1Encodable
    {
        private readonly DerObjectIdentifier	commitmentTypeIdentifier;
        private readonly Asn1Object				qualifier;

        /**
        * Creates a new <code>CommitmentTypeQualifier</code> instance.
        *
        * @param commitmentTypeIdentifier a <code>CommitmentTypeIdentifier</code> value
        */
        public CommitmentTypeQualifier(
            DerObjectIdentifier commitmentTypeIdentifier)
            : this(commitmentTypeIdentifier, null)
        {
        }

    /**
        * Creates a new <code>CommitmentTypeQualifier</code> instance.
        *
        * @param commitmentTypeIdentifier a <code>CommitmentTypeIdentifier</code> value
        * @param qualifier the qualifier, defined by the above field.
        */
        public CommitmentTypeQualifier(
            DerObjectIdentifier	commitmentTypeIdentifier,
            Asn1Encodable		qualifier)
        {
			if (commitmentTypeIdentifier == null)
				throw new ArgumentNullException("commitmentTypeIdentifier");

			this.commitmentTypeIdentifier = commitmentTypeIdentifier;

			if (qualifier != null)
			{
				this.qualifier = qualifier.ToAsn1Object();
			}
        }

        /**
        * Creates a new <code>CommitmentTypeQualifier</code> instance.
        *
        * @param as <code>CommitmentTypeQualifier</code> structure
        * encoded as an Asn1Sequence.
        */
        public CommitmentTypeQualifier(
            Asn1Sequence seq)
        {
			if (seq == null)
				throw new ArgumentNullException("seq");
			if (seq.Count < 1 || seq.Count > 2)
				throw new ArgumentException("Bad sequence size: " + seq.Count, "seq");

			commitmentTypeIdentifier = (DerObjectIdentifier) seq[0].ToAsn1Object();

			if (seq.Count > 1)
            {
                qualifier = seq[1].ToAsn1Object();
            }
        }

		public static CommitmentTypeQualifier GetInstance(
			object obj)
		{
			if (obj == null || obj is CommitmentTypeQualifier)
				return (CommitmentTypeQualifier) obj;

			if (obj is Asn1Sequence)
				return new CommitmentTypeQualifier((Asn1Sequence) obj);

			throw new ArgumentException(
				"Unknown object in 'CommitmentTypeQualifier' factory: "
					+ obj.GetType().Name,
				"obj");
		}

		public DerObjectIdentifier CommitmentTypeIdentifier
		{
			get { return commitmentTypeIdentifier; }
		}

		public Asn1Object Qualifier
		{
			get { return qualifier; }
		}

		/**
        * Returns a DER-encodable representation of this instance.
        *
        * @return a <code>Asn1Object</code> value
        */
		public override Asn1Object ToAsn1Object()
		{
			Asn1EncodableVector v = new Asn1EncodableVector(
				commitmentTypeIdentifier);

			if (qualifier != null)
			{
				v.Add(qualifier);
			}

			return new DerSequence(v);
		}
    }
}
