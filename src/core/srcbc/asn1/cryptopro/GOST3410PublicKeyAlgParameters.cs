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

namespace Org.BouncyCastle.Asn1.CryptoPro
{
    public class Gost3410PublicKeyAlgParameters
        : Asn1Encodable
    {
        private DerObjectIdentifier	publicKeyParamSet;
        private DerObjectIdentifier	digestParamSet;
        private DerObjectIdentifier	encryptionParamSet;

		public static Gost3410PublicKeyAlgParameters GetInstance(
            Asn1TaggedObject	obj,
            bool				explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

		public static Gost3410PublicKeyAlgParameters GetInstance(
            object obj)
        {
            if (obj == null || obj is Gost3410PublicKeyAlgParameters)
            {
                return (Gost3410PublicKeyAlgParameters) obj;
            }

			if (obj is Asn1Sequence)
            {
                return new Gost3410PublicKeyAlgParameters((Asn1Sequence) obj);
            }

			throw new ArgumentException("Invalid GOST3410Parameter: " + obj.GetType().Name);
        }

		public Gost3410PublicKeyAlgParameters(
            DerObjectIdentifier	publicKeyParamSet,
            DerObjectIdentifier	digestParamSet)
			: this (publicKeyParamSet, digestParamSet, null)
        {
        }

		public Gost3410PublicKeyAlgParameters(
            DerObjectIdentifier  publicKeyParamSet,
            DerObjectIdentifier  digestParamSet,
            DerObjectIdentifier  encryptionParamSet)
        {
			if (publicKeyParamSet == null)
				throw new ArgumentNullException("publicKeyParamSet");
			if (digestParamSet == null)
				throw new ArgumentNullException("digestParamSet");

			this.publicKeyParamSet = publicKeyParamSet;
            this.digestParamSet = digestParamSet;
            this.encryptionParamSet = encryptionParamSet;
        }

		public Gost3410PublicKeyAlgParameters(
            Asn1Sequence seq)
        {
            this.publicKeyParamSet = (DerObjectIdentifier) seq[0];
            this.digestParamSet = (DerObjectIdentifier) seq[1];

			if (seq.Count > 2)
            {
                this.encryptionParamSet = (DerObjectIdentifier) seq[2];
            }
        }

		public DerObjectIdentifier PublicKeyParamSet
		{
			get { return publicKeyParamSet; }
		}

		public DerObjectIdentifier DigestParamSet
		{
			get { return digestParamSet; }
		}

		public DerObjectIdentifier EncryptionParamSet
		{
			get { return encryptionParamSet; }
		}

		public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(
				publicKeyParamSet, digestParamSet);

			if (encryptionParamSet != null)
            {
                v.Add(encryptionParamSet);
            }

			return new DerSequence(v);
        }
    }
}
