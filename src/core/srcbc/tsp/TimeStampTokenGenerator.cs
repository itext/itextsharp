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
using Org.BouncyCastle.Asn1.Ess;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;

namespace Org.BouncyCastle.Tsp
{
	public class TimeStampTokenGenerator
	{
		private int accuracySeconds = -1;
		private int accuracyMillis = -1;
		private int accuracyMicros = -1;
		private bool ordering = false;
		private GeneralName tsa = null;
		private string tsaPolicyOID;

		private AsymmetricKeyParameter	key;
		private X509Certificate			cert;
		private string					digestOID;
		private Asn1.Cms.AttributeTable	signedAttr;
		private Asn1.Cms.AttributeTable	unsignedAttr;
		private IX509Store				x509Certs;
		private IX509Store				x509Crls;

		/**
		 * basic creation - only the default attributes will be included here.
		 */
		public TimeStampTokenGenerator(
			AsymmetricKeyParameter	key,
			X509Certificate			cert,
			string					digestOID,
			string					tsaPolicyOID)
			: this(key, cert, digestOID, tsaPolicyOID, null, null)
		{
		}

		/**
		 * create with a signer with extra signed/unsigned attributes.
		 */
		public TimeStampTokenGenerator(
			AsymmetricKeyParameter	key,
			X509Certificate			cert,
			string					digestOID,
			string					tsaPolicyOID,
			Asn1.Cms.AttributeTable	signedAttr,
			Asn1.Cms.AttributeTable	unsignedAttr)
		{
			this.key = key;
			this.cert = cert;
			this.digestOID = digestOID;
			this.tsaPolicyOID = tsaPolicyOID;
			this.unsignedAttr = unsignedAttr;

			TspUtil.ValidateCertificate(cert);

			//
			// Add the ESSCertID attribute
			//
			IDictionary signedAttrs;
			if (signedAttr != null)
			{
				signedAttrs = signedAttr.ToDictionary();
			}
			else
			{
				signedAttrs = Platform.CreateHashtable();
			}

			try
			{
				byte[] hash = DigestUtilities.CalculateDigest("SHA-1", cert.GetEncoded());

				EssCertID essCertid = new EssCertID(hash);

				Asn1.Cms.Attribute attr = new Asn1.Cms.Attribute(
					PkcsObjectIdentifiers.IdAASigningCertificate,
					new DerSet(new SigningCertificate(essCertid)));

				signedAttrs[attr.AttrType] = attr;
			}
			catch (CertificateEncodingException e)
			{
				throw new TspException("Exception processing certificate.", e);
			}
			catch (SecurityUtilityException e)
			{
				throw new TspException("Can't find a SHA-1 implementation.", e);
			}

			this.signedAttr = new Asn1.Cms.AttributeTable(signedAttrs);
		}

		public void SetCertificates(
			IX509Store certificates)
		{
			this.x509Certs = certificates;
		}

		public void SetCrls(
			IX509Store crls)
		{
			this.x509Crls = crls;
		}

		public void SetAccuracySeconds(
			int accuracySeconds)
		{
			this.accuracySeconds = accuracySeconds;
		}

		public void SetAccuracyMillis(
			int accuracyMillis)
		{
			this.accuracyMillis = accuracyMillis;
		}

		public void SetAccuracyMicros(
			int accuracyMicros)
		{
			this.accuracyMicros = accuracyMicros;
		}

		public void SetOrdering(
			bool ordering)
		{
			this.ordering = ordering;
		}

		public void SetTsa(
			GeneralName tsa)
		{
			this.tsa = tsa;
		}

		//------------------------------------------------------------------------------

		public TimeStampToken Generate(
			TimeStampRequest	request,
			BigInteger			serialNumber,
			DateTime			genTime)
		{
			DerObjectIdentifier digestAlgOID = new DerObjectIdentifier(request.MessageImprintAlgOid);

			AlgorithmIdentifier algID = new AlgorithmIdentifier(digestAlgOID, DerNull.Instance);
			MessageImprint messageImprint = new MessageImprint(algID, request.GetMessageImprintDigest());

			Accuracy accuracy = null;
			if (accuracySeconds > 0 || accuracyMillis > 0 || accuracyMicros > 0)
			{
				DerInteger seconds = null;
				if (accuracySeconds > 0)
				{
					seconds = new DerInteger(accuracySeconds);
				}

				DerInteger millis = null;
				if (accuracyMillis > 0)
				{
					millis = new DerInteger(accuracyMillis);
				}

				DerInteger micros = null;
				if (accuracyMicros > 0)
				{
					micros = new DerInteger(accuracyMicros);
				}

				accuracy = new Accuracy(seconds, millis, micros);
			}

			DerBoolean derOrdering = null;
			if (ordering)
			{
				derOrdering = DerBoolean.GetInstance(ordering);
			}

			DerInteger nonce = null;
			if (request.Nonce != null)
			{
				nonce = new DerInteger(request.Nonce);
			}

			DerObjectIdentifier tsaPolicy = new DerObjectIdentifier(tsaPolicyOID);
			if (request.ReqPolicy != null)
			{
				tsaPolicy = new DerObjectIdentifier(request.ReqPolicy);
			}

			TstInfo tstInfo = new TstInfo(tsaPolicy, messageImprint,
				new DerInteger(serialNumber), new DerGeneralizedTime(genTime), accuracy,
				derOrdering, nonce, tsa, request.Extensions);

			try
			{
				CmsSignedDataGenerator signedDataGenerator = new CmsSignedDataGenerator();

				byte[] derEncodedTstInfo = tstInfo.GetDerEncoded();

				if (request.CertReq)
				{
					signedDataGenerator.AddCertificates(x509Certs);
				}

				signedDataGenerator.AddCrls(x509Crls);
				signedDataGenerator.AddSigner(key, cert, digestOID, signedAttr, unsignedAttr);

				CmsSignedData signedData = signedDataGenerator.Generate(
					PkcsObjectIdentifiers.IdCTTstInfo.Id,
					new CmsProcessableByteArray(derEncodedTstInfo),
					true);

				return new TimeStampToken(signedData);
			}
			catch (CmsException cmsEx)
			{
				throw new TspException("Error generating time-stamp token", cmsEx);
			}
			catch (IOException e)
			{
				throw new TspException("Exception encoding info", e);
			}
			catch (X509StoreException e)
			{
				throw new TspException("Exception handling CertStore", e);
			}
//			catch (InvalidAlgorithmParameterException e)
//			{
//				throw new TspException("Exception handling CertStore CRLs", e);
//			}
		}
	}
}
