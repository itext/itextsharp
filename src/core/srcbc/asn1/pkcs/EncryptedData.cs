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
     * The EncryptedData object.
     * <pre>
     *      EncryptedData ::= Sequence {
     *           version Version,
     *           encryptedContentInfo EncryptedContentInfo
     *      }
     *
     *
     *      EncryptedContentInfo ::= Sequence {
     *          contentType ContentType,
     *          contentEncryptionAlgorithm  ContentEncryptionAlgorithmIdentifier,
     *          encryptedContent [0] IMPLICIT EncryptedContent OPTIONAL
     *    }
     *
     *    EncryptedContent ::= OCTET STRING
     * </pre>
     */
    public class EncryptedData
        : Asn1Encodable
    {
        private readonly Asn1Sequence data;
//        private readonly DerObjectIdentifier bagId;
//        private readonly Asn1Object bagValue;

		public static EncryptedData GetInstance(
             object obj)
        {
			if (obj is EncryptedData)
			{
				return (EncryptedData) obj;
			}

			if (obj is Asn1Sequence)
			{
				return new EncryptedData((Asn1Sequence) obj);
			}

			throw new ArgumentException("Unknown object in factory: " + obj.GetType().FullName, "obj");
		}

		private EncryptedData(
            Asn1Sequence seq)
        {
			if (seq.Count != 2)
				throw new ArgumentException("Wrong number of elements in sequence", "seq");

			int version = ((DerInteger) seq[0]).Value.IntValue;
			if (version != 0)
            {
                throw new ArgumentException("sequence not version 0");
            }

			this.data = (Asn1Sequence) seq[1];
        }

		public EncryptedData(
            DerObjectIdentifier	contentType,
            AlgorithmIdentifier	encryptionAlgorithm,
            Asn1Encodable		content)
        {
			data = new BerSequence(
				contentType,
				encryptionAlgorithm.ToAsn1Object(),
				new BerTaggedObject(false, 0, content));
        }

		public DerObjectIdentifier ContentType
        {
            get { return (DerObjectIdentifier) data[0]; }
        }

		public AlgorithmIdentifier EncryptionAlgorithm
        {
			get { return AlgorithmIdentifier.GetInstance(data[1]); }
        }

		public Asn1OctetString Content
        {
			get
			{
				if (data.Count == 3)
				{
					DerTaggedObject o = (DerTaggedObject) data[2];

					return Asn1OctetString.GetInstance(o, false);
				}

				return null;
			}
        }

		public override Asn1Object ToAsn1Object()
        {
			return new BerSequence(new DerInteger(0), data);
        }
    }
}
