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
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;

namespace Org.BouncyCastle.X509
{
	/// <summary>
	/// Class to Generate X509V1 Certificates.
	/// </summary>
	public class X509V1CertificateGenerator
	{
		private V1TbsCertificateGenerator   tbsGen;
		private DerObjectIdentifier         sigOID;
		private AlgorithmIdentifier         sigAlgId;
		private string                      signatureAlgorithm;

		/// <summary>
		/// Default Constructor.
		/// </summary>
		public X509V1CertificateGenerator()
		{
			tbsGen = new V1TbsCertificateGenerator();
		}

		/// <summary>
		/// Reset the generator.
		/// </summary>
		public void Reset()
		{
			tbsGen = new V1TbsCertificateGenerator();
		}

		/// <summary>
		/// Set the certificate's serial number.
		/// </summary>
		/// <remarks>Make serial numbers long, if you have no serial number policy make sure the number is at least 16 bytes of secure random data.
		/// You will be surprised how ugly a serial number collision can get.</remarks>
		/// <param name="serialNumber">The serial number.</param>
		public void SetSerialNumber(
			BigInteger serialNumber)
		{
			if (serialNumber.SignValue <= 0)
			{
				throw new ArgumentException("serial number must be a positive integer", "serialNumber");
			}

			tbsGen.SetSerialNumber(new DerInteger(serialNumber));
		}

		/// <summary>
		/// Set the issuer distinguished name.
		/// The issuer is the entity whose private key is used to sign the certificate.
		/// </summary>
		/// <param name="issuer">The issuers DN.</param>
		public void SetIssuerDN(
			X509Name issuer)
		{
			tbsGen.SetIssuer(issuer);
		}

		/// <summary>
		/// Set the date that this certificate is to be valid from.
		/// </summary>
		/// <param name="date"/>
		public void SetNotBefore(
			DateTime date)
		{
			tbsGen.SetStartDate(new Time(date));
		}

		/// <summary>
		/// Set the date after which this certificate will no longer be valid.
		/// </summary>
		/// <param name="date"/>
		public void SetNotAfter(
			DateTime date)
		{
			tbsGen.SetEndDate(new Time(date));
		}

		/// <summary>
		/// Set the subject distinguished name.
		/// The subject describes the entity associated with the public key.
		/// </summary>
		/// <param name="subject"/>
		public void SetSubjectDN(
			X509Name subject)
		{
			tbsGen.SetSubject(subject);
		}

        /// <summary>
        /// Set the public key that this certificate identifies.
        /// </summary>
        /// <param name="publicKey"/>
		public void SetPublicKey(
			AsymmetricKeyParameter publicKey)
		{
			try
			{
				tbsGen.SetSubjectPublicKeyInfo(
					SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey));
			}
			catch (Exception e)
			{
				throw new ArgumentException("unable to process key - " + e.ToString());
			}
		}

		/// <summary>
		/// Set the signature algorithm that will be used to sign this certificate.
		/// This can be either a name or an OID, names are treated as case insensitive.
		/// </summary>
		/// <param name="signatureAlgorithm">string representation of the algorithm name</param>
		public void SetSignatureAlgorithm(
			string signatureAlgorithm)
		{
			this.signatureAlgorithm = signatureAlgorithm;

			try
			{
				sigOID = X509Utilities.GetAlgorithmOid(signatureAlgorithm);
			}
			catch (Exception)
			{
				throw new ArgumentException("Unknown signature type requested", "signatureAlgorithm");
			}

			sigAlgId = X509Utilities.GetSigAlgID(sigOID, signatureAlgorithm);

			tbsGen.SetSignature(sigAlgId);
		}

		/// <summary>
		/// Generate a new X509Certificate.
		/// </summary>
		/// <param name="privateKey">The private key of the issuer used to sign this certificate.</param>
		/// <returns>An X509Certificate.</returns>
		public X509Certificate Generate(
			AsymmetricKeyParameter privateKey)
		{
			return Generate(privateKey, null);
		}

        /// <summary>
        /// Generate a new X509Certificate specifying a SecureRandom instance that you would like to use.
        /// </summary>
        /// <param name="privateKey">The private key of the issuer used to sign this certificate.</param>
        /// <param name="random">The Secure Random you want to use.</param>
        /// <returns>An X509Certificate.</returns>
		public X509Certificate Generate(
			AsymmetricKeyParameter	privateKey,
			SecureRandom			random)
		{
			TbsCertificateStructure tbsCert = tbsGen.GenerateTbsCertificate();
			byte[] signature;

			try
			{
				signature = X509Utilities.GetSignatureForObject(
					sigOID, signatureAlgorithm, privateKey, random, tbsCert);
			}
			catch (Exception e)
			{
				// TODO
//				throw new ExtCertificateEncodingException("exception encoding TBS cert", e);
				throw new CertificateEncodingException("exception encoding TBS cert", e);
			}

			try
			{
				return GenerateJcaObject(tbsCert, signature);
			}
			catch (CertificateParsingException e)
			{
				// TODO
				// throw new ExtCertificateEncodingException("exception producing certificate object", e);
				throw new CertificateEncodingException("exception producing certificate object", e);
			}
		}

		private X509Certificate GenerateJcaObject(
			TbsCertificateStructure	tbsCert,
			byte[]					signature)
		{
			return new X509Certificate(
				new X509CertificateStructure(tbsCert, sigAlgId, new DerBitString(signature)));
		}

		/// <summary>
		/// Allows enumeration of the signature names supported by the generator.
		/// </summary>
		public IEnumerable SignatureAlgNames
		{
			get { return X509Utilities.GetAlgNames(); }
		}
	}
}
