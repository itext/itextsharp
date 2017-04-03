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
using Org.BouncyCastle.Asn1.X509;

namespace Org.BouncyCastle.Asn1.Ocsp
{
    [Obsolete("For internal use only. If you want to use iText, please use a dependency on iText 7. ")]
    public class BasicOcspResponse
        : Asn1Encodable
    {
        private readonly ResponseData			tbsResponseData;
        private readonly AlgorithmIdentifier	signatureAlgorithm;
        private readonly DerBitString			signature;
        private readonly Asn1Sequence			certs;

		public static BasicOcspResponse GetInstance(
			Asn1TaggedObject	obj,
			bool				explicitly)
		{
			return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
		}

		public static BasicOcspResponse GetInstance(
			object obj)
		{
			if (obj == null || obj is BasicOcspResponse)
			{
				return (BasicOcspResponse)obj;
			}

			if (obj is Asn1Sequence)
			{
				return new BasicOcspResponse((Asn1Sequence)obj);
			}

			throw new ArgumentException("unknown object in factory: " + obj.GetType().Name, "obj");
		}

		public BasicOcspResponse(
            ResponseData		tbsResponseData,
            AlgorithmIdentifier	signatureAlgorithm,
            DerBitString		signature,
            Asn1Sequence		certs)
        {
            this.tbsResponseData = tbsResponseData;
            this.signatureAlgorithm = signatureAlgorithm;
            this.signature = signature;
            this.certs = certs;
        }

		private BasicOcspResponse(
            Asn1Sequence seq)
        {
            this.tbsResponseData = ResponseData.GetInstance(seq[0]);
            this.signatureAlgorithm = AlgorithmIdentifier.GetInstance(seq[1]);
            this.signature = (DerBitString)seq[2];

			if (seq.Count > 3)
            {
                this.certs = Asn1Sequence.GetInstance((Asn1TaggedObject)seq[3], true);
            }
        }

		[Obsolete("Use TbsResponseData property instead")]
		public ResponseData GetTbsResponseData()
        {
            return tbsResponseData;
        }

		public ResponseData TbsResponseData
		{
			get { return tbsResponseData; }
		}

		[Obsolete("Use SignatureAlgorithm property instead")]
		public AlgorithmIdentifier GetSignatureAlgorithm()
        {
            return signatureAlgorithm;
        }

		public AlgorithmIdentifier SignatureAlgorithm
		{
			get { return signatureAlgorithm; }
		}

		[Obsolete("Use Signature property instead")]
		public DerBitString GetSignature()
        {
            return signature;
        }

		public DerBitString Signature
		{
			get { return signature; }
		}

		[Obsolete("Use Certs property instead")]
		public Asn1Sequence GetCerts()
        {
            return certs;
        }

		public Asn1Sequence Certs
		{
			get { return certs; }
		}

		/**
         * Produce an object suitable for an Asn1OutputStream.
         * <pre>
         * BasicOcspResponse       ::= Sequence {
         *      tbsResponseData      ResponseData,
         *      signatureAlgorithm   AlgorithmIdentifier,
         *      signature            BIT STRING,
         *      certs                [0] EXPLICIT Sequence OF Certificate OPTIONAL }
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
            Asn1EncodableVector v = new Asn1EncodableVector(
				tbsResponseData, signatureAlgorithm, signature);

			if (certs != null)
            {
                v.Add(new DerTaggedObject(true, 0, certs));
            }

			return new DerSequence(v);
        }
    }
}
