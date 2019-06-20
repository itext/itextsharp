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
    public class RecipientIdentifier
        : Asn1Encodable, IAsn1Choice
    {
        private Asn1Encodable id;

		public RecipientIdentifier(
            IssuerAndSerialNumber id)
        {
            this.id = id;
        }

		public RecipientIdentifier(
            Asn1OctetString id)
        {
            this.id = new DerTaggedObject(false, 0, id);
        }

		public RecipientIdentifier(
            Asn1Object id)
        {
            this.id = id;
        }

		/**
         * return a RecipientIdentifier object from the given object.
         *
         * @param o the object we want converted.
         * @exception ArgumentException if the object cannot be converted.
         */
        public static RecipientIdentifier GetInstance(
            object o)
        {
            if (o == null || o is RecipientIdentifier)
                return (RecipientIdentifier)o;

			if (o is IssuerAndSerialNumber)
                return new RecipientIdentifier((IssuerAndSerialNumber) o);

			if (o is Asn1OctetString)
                return new RecipientIdentifier((Asn1OctetString) o);

			if (o is Asn1Object)
                return new RecipientIdentifier((Asn1Object) o);

			throw new ArgumentException(
              "Illegal object in RecipientIdentifier: " + o.GetType().Name);
        }

		public bool IsTagged
		{
			get { return (id is Asn1TaggedObject); }
		}

		public Asn1Encodable ID
        {
            get
            {
                if (id is Asn1TaggedObject)
                {
                    return Asn1OctetString.GetInstance((Asn1TaggedObject) id, false);
                }

				return IssuerAndSerialNumber.GetInstance(id);
            }
        }

		/**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         * RecipientIdentifier ::= CHOICE {
         *     issuerAndSerialNumber IssuerAndSerialNumber,
         *     subjectKeyIdentifier [0] SubjectKeyIdentifier
         * }
         *
         * SubjectKeyIdentifier ::= OCTET STRING
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            return id.ToAsn1Object();
        }
    }
}
