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

using Org.BouncyCastle.Asn1;

namespace Org.BouncyCastle.Asn1.X509
{
    /**
     * Implementation of <code>IetfAttrSyntax</code> as specified by RFC3281.
     */
    public class IetfAttrSyntax
        : Asn1Encodable
    {
        public const int ValueOctets	= 1;
        public const int ValueOid		= 2;
        public const int ValueUtf8		= 3;

		internal readonly GeneralNames	policyAuthority;
        internal readonly Asn1EncodableVector values = new Asn1EncodableVector();

		internal int valueChoice = -1;

		/**
         *
         */
        public IetfAttrSyntax(
			Asn1Sequence seq)
        {
            int i = 0;

            if (seq[0] is Asn1TaggedObject)
            {
                policyAuthority = GeneralNames.GetInstance(((Asn1TaggedObject)seq[0]), false);
                i++;
            }
            else if (seq.Count == 2)
            { // VOMS fix
                policyAuthority = GeneralNames.GetInstance(seq[0]);
                i++;
            }

			if (!(seq[i] is Asn1Sequence))
            {
                throw new ArgumentException("Non-IetfAttrSyntax encoding");
            }

			seq = (Asn1Sequence) seq[i];

			foreach (Asn1Object obj in seq)
			{
                int type;

                if (obj is DerObjectIdentifier)
                {
                    type = ValueOid;
                }
                else if (obj is DerUtf8String)
                {
                    type = ValueUtf8;
                }
                else if (obj is DerOctetString)
                {
                    type = ValueOctets;
                }
                else
                {
                    throw new ArgumentException("Bad value type encoding IetfAttrSyntax");
                }

				if (valueChoice < 0)
                {
                    valueChoice = type;
                }

				if (type != valueChoice)
                {
                    throw new ArgumentException("Mix of value types in IetfAttrSyntax");
                }

				values.Add(obj);
            }
        }

		public GeneralNames PolicyAuthority
		{
			get { return policyAuthority; }
		}

		public int ValueType
		{
			get { return valueChoice; }
		}

		public object[] GetValues()
        {
            if (this.ValueType == ValueOctets)
            {
                Asn1OctetString[] tmp = new Asn1OctetString[values.Count];

				for (int i = 0; i != tmp.Length; i++)
                {
                    tmp[i] = (Asn1OctetString) values[i];
                }

				return tmp;
            }

			if (this.ValueType == ValueOid)
            {
                DerObjectIdentifier[] tmp = new DerObjectIdentifier[values.Count];

                for (int i = 0; i != tmp.Length; i++)
                {
                    tmp[i] = (DerObjectIdentifier) values[i];
                }

				return tmp;
            }

			{
				DerUtf8String[] tmp = new DerUtf8String[values.Count];

				for (int i = 0; i != tmp.Length; i++)
				{
					tmp[i] = (DerUtf8String) values[i];
				}

				return tmp;
			}
        }

		/**
         *
         * <pre>
         *
         *  IetfAttrSyntax ::= Sequence {
         *    policyAuthority [0] GeneralNames OPTIONAL,
         *    values Sequence OF CHOICE {
         *      octets OCTET STRING,
         *      oid OBJECT IDENTIFIER,
         *      string UTF8String
         *    }
         *  }
         *
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector();

			if (policyAuthority != null)
            {
                v.Add(new DerTaggedObject(0, policyAuthority));
            }

			v.Add(new DerSequence(values));

			return new DerSequence(v);
        }
    }
}
