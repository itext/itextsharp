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
using System.Globalization;
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.CryptoPro;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Oiw;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.TeleTrust;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Pkcs
{
	/// <remarks>
	/// A class for creating and verifying Pkcs10 Certification requests (this is an extension on <see cref="Pkcs10CertificationRequest"/>).
	/// The requests are made using delay signing. This is useful for situations where
	/// the private key is in another environment and not directly accessible (e.g. HSM)
	/// So the first step creates the request, then the signing is done outside this
	/// object and the signature is then used to complete the request.
	/// </remarks>
	/// <code>
	/// CertificationRequest ::= Sequence {
	///   certificationRequestInfo  CertificationRequestInfo,
	///   signatureAlgorithm        AlgorithmIdentifier{{ SignatureAlgorithms }},
	///   signature                 BIT STRING
	/// }
	///
	/// CertificationRequestInfo ::= Sequence {
	///   version             Integer { v1(0) } (v1,...),
	///   subject             Name,
	///   subjectPKInfo   SubjectPublicKeyInfo{{ PKInfoAlgorithms }},
	///   attributes          [0] Attributes{{ CRIAttributes }}
	///  }
	///
	///  Attributes { ATTRIBUTE:IOSet } ::= Set OF Attr{{ IOSet }}
	///
	///  Attr { ATTRIBUTE:IOSet } ::= Sequence {
	///    type    ATTRIBUTE.&amp;id({IOSet}),
	///    values  Set SIZE(1..MAX) OF ATTRIBUTE.&amp;Type({IOSet}{\@type})
	///  }
	/// </code>
	/// see <a href="http://www.rsasecurity.com/rsalabs/node.asp?id=2132"/>
	public class Pkcs10CertificationRequestDelaySigned : Pkcs10CertificationRequest
	{
		protected Pkcs10CertificationRequestDelaySigned()
			: base()
		{
		}
		public Pkcs10CertificationRequestDelaySigned(
			byte[] encoded)
			: base(encoded)
		{
		}
		public Pkcs10CertificationRequestDelaySigned(
			Asn1Sequence seq)
			: base(seq)
		{
		}
		public Pkcs10CertificationRequestDelaySigned(
			Stream input)
			: base(input)
		{
		}
		public Pkcs10CertificationRequestDelaySigned(
			string					signatureAlgorithm,
			X509Name				subject,
			AsymmetricKeyParameter	publicKey,
			Asn1Set					attributes,
			AsymmetricKeyParameter	signingKey)
			: base(signatureAlgorithm, subject, publicKey, attributes, signingKey)
		{
		}
		/// <summary>
		/// Instantiate a Pkcs10CertificationRequest object with the necessary credentials.
		/// </summary>
		/// <param name="signatureAlgorithm">Name of Sig Alg.</param>
		/// <param name="subject">X509Name of subject eg OU="My unit." O="My Organisatioin" C="au" </param>
		/// <param name="publicKey">Public Key to be included in cert reqest.</param>
		/// <param name="attributes">ASN1Set of Attributes.</param>
		/// <remarks>
        /// After the object is constructed use the <see cref="GetDataToSign"/> and finally the
        /// SignRequest methods to finalize the request.
		/// </remarks>
		public Pkcs10CertificationRequestDelaySigned(
			string					signatureAlgorithm,
			X509Name				subject,
			AsymmetricKeyParameter	publicKey,
			Asn1Set					attributes)
		{
			if (signatureAlgorithm == null)
				throw new ArgumentNullException("signatureAlgorithm");
			if (subject == null)
				throw new ArgumentNullException("subject");
			if (publicKey == null)
				throw new ArgumentNullException("publicKey");
			if (publicKey.IsPrivate)
				throw new ArgumentException("expected public key", "publicKey");
//			DerObjectIdentifier sigOid = SignerUtilities.GetObjectIdentifier(signatureAlgorithm);
			string algorithmName = Platform.ToUpperInvariant(signatureAlgorithm);
			DerObjectIdentifier sigOid = (DerObjectIdentifier) algorithms[algorithmName];
			if (sigOid == null)
			{
				try
				{
					sigOid = new DerObjectIdentifier(algorithmName);
				}
				catch (Exception e)
				{
					throw new ArgumentException("Unknown signature type requested", e);
				}
			}
			if (noParams.Contains(sigOid))
			{
				this.sigAlgId = new AlgorithmIdentifier(sigOid);
			}
			else if (exParams.Contains(algorithmName))
			{
				this.sigAlgId = new AlgorithmIdentifier(sigOid, (Asn1Encodable) exParams[algorithmName]);
			}
			else
			{
				this.sigAlgId = new AlgorithmIdentifier(sigOid, DerNull.Instance);
			}
			SubjectPublicKeyInfo pubInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
			this.reqInfo = new CertificationRequestInfo(subject, pubInfo, attributes);
		}
		public byte[] GetDataToSign()
		{
			return reqInfo.GetDerEncoded();
		}
		public void SignRequest(byte[] signedData)
		{
			//build the signature from the signed data
			sigBits = new DerBitString(signedData);
		}
		public void SignRequest(DerBitString signedData)
		{
			//build the signature from the signed data
			sigBits = signedData;
		}
	}
}
