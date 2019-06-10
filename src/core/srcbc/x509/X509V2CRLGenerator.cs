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
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.X509
{
	/**
	* class to produce an X.509 Version 2 CRL.
	*/
	public class X509V2CrlGenerator
	{
		private readonly X509ExtensionsGenerator extGenerator = new X509ExtensionsGenerator();

		private V2TbsCertListGenerator	tbsGen;
		private DerObjectIdentifier		sigOID;
		private AlgorithmIdentifier		sigAlgId;
		private string					signatureAlgorithm;

		public X509V2CrlGenerator()
		{
			tbsGen = new V2TbsCertListGenerator();
		}

		/**
		* reset the generator
		*/
		public void Reset()
		{
			tbsGen = new V2TbsCertListGenerator();
			extGenerator.Reset();
		}

		/**
		* Set the issuer distinguished name - the issuer is the entity whose private key is used to sign the
		* certificate.
		*/
		public void SetIssuerDN(
			X509Name issuer)
		{
			tbsGen.SetIssuer(issuer);
		}

		public void SetThisUpdate(
			DateTime date)
		{
			tbsGen.SetThisUpdate(new Time(date));
		}

		public void SetNextUpdate(
			DateTime date)
		{
			tbsGen.SetNextUpdate(new Time(date));
		}

		/**
		* Reason being as indicated by CrlReason, i.e. CrlReason.KeyCompromise
		* or 0 if CrlReason is not to be used
		**/
		public void AddCrlEntry(
			BigInteger	userCertificate,
			DateTime	revocationDate,
			int			reason)
		{
			tbsGen.AddCrlEntry(new DerInteger(userCertificate), new Time(revocationDate), reason);
		}

		/**
		* Add a CRL entry with an Invalidity Date extension as well as a CrlReason extension.
		* Reason being as indicated by CrlReason, i.e. CrlReason.KeyCompromise
		* or 0 if CrlReason is not to be used
		**/
		public void AddCrlEntry(
			BigInteger	userCertificate,
			DateTime	revocationDate,
			int			reason,
			DateTime	invalidityDate)
		{
			tbsGen.AddCrlEntry(new DerInteger(userCertificate), new Time(revocationDate), reason, new DerGeneralizedTime(invalidityDate));
		}

		/**
		* Add a CRL entry with extensions.
		**/
		public void AddCrlEntry(
			BigInteger		userCertificate,
			DateTime		revocationDate,
			X509Extensions	extensions)
		{
			tbsGen.AddCrlEntry(new DerInteger(userCertificate), new Time(revocationDate), extensions);
		}

		/**
		* Add the CRLEntry objects contained in a previous CRL.
		*
		* @param other the X509Crl to source the other entries from.
		*/
		public void AddCrl(
			X509Crl other)
		{
			if (other == null)
				throw new ArgumentNullException("other");

			ISet revocations = other.GetRevokedCertificates();

			if (revocations != null)
			{
				foreach (X509CrlEntry entry in revocations)
				{
					try
					{
						tbsGen.AddCrlEntry(
							Asn1Sequence.GetInstance(
							Asn1Object.FromByteArray(entry.GetEncoded())));
					}
					catch (IOException e)
					{
						throw new CrlException("exception processing encoding of CRL", e);
					}
				}
			}
		}

		/**
		* Set the signature algorithm. This can be either a name or an oid, names
		* are treated as case insensitive.
		*
		* @param signatureAlgorithm string representation of the algorithm name.
		*/
		public void SetSignatureAlgorithm(
			string signatureAlgorithm)
		{
			this.signatureAlgorithm = signatureAlgorithm;

			try
			{
				sigOID = X509Utilities.GetAlgorithmOid(signatureAlgorithm);
			}
			catch (Exception e)
			{
				throw new ArgumentException("Unknown signature type requested", e);
			}

			sigAlgId = X509Utilities.GetSigAlgID(sigOID, signatureAlgorithm);

			tbsGen.SetSignature(sigAlgId);
		}

		/**
		* add a given extension field for the standard extensions tag (tag 0)
		*/
		public void AddExtension(
			string			oid,
			bool			critical,
			Asn1Encodable	extensionValue)
		{
			extGenerator.AddExtension(new DerObjectIdentifier(oid), critical, extensionValue);
		}

		/**
		* add a given extension field for the standard extensions tag (tag 0)
		*/
		public void AddExtension(
			DerObjectIdentifier	oid,
			bool				critical,
			Asn1Encodable		extensionValue)
		{
			extGenerator.AddExtension(oid, critical, extensionValue);
		}

		/**
		* add a given extension field for the standard extensions tag (tag 0)
		*/
		public void AddExtension(
			string	oid,
			bool	critical,
			byte[]	extensionValue)
		{
			extGenerator.AddExtension(new DerObjectIdentifier(oid), critical, new DerOctetString(extensionValue));
		}

		/**
		* add a given extension field for the standard extensions tag (tag 0)
		*/
		public void AddExtension(
			DerObjectIdentifier	oid,
			bool				critical,
			byte[]				extensionValue)
		{
			extGenerator.AddExtension(oid, critical, new DerOctetString(extensionValue));
		}

		/// <summary>Generate an X509 CRL, based on the current issuer and subject.</summary>
		/// <param name="privateKey">The key used for signing.</param>
		public X509Crl Generate(
			AsymmetricKeyParameter privateKey)
		{
			return Generate(privateKey, null);
		}

		/// <summary>Generate an X509 CRL, based on the current issuer and subject.</summary>
		/// <param name="privateKey">The key used for signing.</param>
		/// <param name="random">A user-defined source of randomness.</param>
		public X509Crl Generate(
			AsymmetricKeyParameter	privateKey,
			SecureRandom			random)
		{
			TbsCertificateList tbsCrl = GenerateCertList();
			byte[] signature;

			try
			{
				signature = X509Utilities.GetSignatureForObject(
					sigOID, signatureAlgorithm, privateKey, random, tbsCrl);
			}
			catch (IOException e)
			{
				// TODO
//				throw new ExtCrlException("cannot generate CRL encoding", e);
				throw new CrlException("cannot generate CRL encoding", e);
			}

			return GenerateJcaObject(tbsCrl, signature);
		}

		private TbsCertificateList GenerateCertList()
		{
			if (!extGenerator.IsEmpty)
			{
				tbsGen.SetExtensions(extGenerator.Generate());
			}

			return tbsGen.GenerateTbsCertList();
		}

		private X509Crl GenerateJcaObject(
			TbsCertificateList	tbsCrl,
			byte[]				signature)
		{
			return new X509Crl(
				CertificateList.GetInstance(
					new DerSequence(tbsCrl, sigAlgId, new DerBitString(signature))));
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
