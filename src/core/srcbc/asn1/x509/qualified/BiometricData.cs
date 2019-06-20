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
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.X509.Qualified
{
    /**
    * The BiometricData object.
    * <pre>
    * BiometricData  ::=  SEQUENCE {
    *       typeOfBiometricData  TypeOfBiometricData,
    *       hashAlgorithm        AlgorithmIdentifier,
    *       biometricDataHash    OCTET STRING,
    *       sourceDataUri        IA5String OPTIONAL  }
    * </pre>
    */
    public class BiometricData
        : Asn1Encodable
    {
        private readonly TypeOfBiometricData typeOfBiometricData;
        private readonly AlgorithmIdentifier hashAlgorithm;
        private readonly Asn1OctetString     biometricDataHash;
        private readonly DerIA5String        sourceDataUri;

        public static BiometricData GetInstance(
            object obj)
        {
            if (obj == null || obj is BiometricData)
            {
                return (BiometricData)obj;
            }

            if (obj is Asn1Sequence)
            {
				return new BiometricData(Asn1Sequence.GetInstance(obj));
            }

			throw new ArgumentException("unknown object in GetInstance: " + obj.GetType().FullName, "obj");
		}

		private BiometricData(
			Asn1Sequence seq)
        {
			typeOfBiometricData = TypeOfBiometricData.GetInstance(seq[0]);
			hashAlgorithm = AlgorithmIdentifier.GetInstance(seq[1]);
			biometricDataHash = Asn1OctetString.GetInstance(seq[2]);

			if (seq.Count > 3)
			{
				sourceDataUri = DerIA5String.GetInstance(seq[3]);
			}
        }

		public BiometricData(
            TypeOfBiometricData	typeOfBiometricData,
            AlgorithmIdentifier	hashAlgorithm,
            Asn1OctetString		biometricDataHash,
            DerIA5String		sourceDataUri)
        {
            this.typeOfBiometricData = typeOfBiometricData;
            this.hashAlgorithm = hashAlgorithm;
            this.biometricDataHash = biometricDataHash;
            this.sourceDataUri = sourceDataUri;
        }

        public BiometricData(
            TypeOfBiometricData	typeOfBiometricData,
            AlgorithmIdentifier	hashAlgorithm,
            Asn1OctetString		biometricDataHash)
        {
            this.typeOfBiometricData = typeOfBiometricData;
            this.hashAlgorithm = hashAlgorithm;
            this.biometricDataHash = biometricDataHash;
            this.sourceDataUri = null;
        }

        public TypeOfBiometricData TypeOfBiometricData
        {
			get { return typeOfBiometricData; }
        }

		public AlgorithmIdentifier HashAlgorithm
		{
			get { return hashAlgorithm; }
		}

		public Asn1OctetString BiometricDataHash
		{
			get { return biometricDataHash; }
		}

		public DerIA5String SourceDataUri
		{
			get { return sourceDataUri; }
		}

		public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector seq = new Asn1EncodableVector(
				typeOfBiometricData, hashAlgorithm, biometricDataHash);

			if (sourceDataUri != null)
            {
                seq.Add(sourceDataUri);
            }

			return new DerSequence(seq);
        }
    }
}
