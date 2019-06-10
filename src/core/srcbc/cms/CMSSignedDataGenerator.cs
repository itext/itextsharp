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
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cms
{
    /**
     * general class for generating a pkcs7-signature message.
     * <p>
     * A simple example of usage.
     *
     * <pre>
     *      IX509Store certs...
     *      IX509Store crls...
     *      CmsSignedDataGenerator gen = new CmsSignedDataGenerator();
     *
     *      gen.AddSigner(privKey, cert, CmsSignedGenerator.DigestSha1);
     *      gen.AddCertificates(certs);
     *      gen.AddCrls(crls);
     *
     *      CmsSignedData data = gen.Generate(content);
     * </pre>
	 * </p>
     */
    public class CmsSignedDataGenerator
        : CmsSignedGenerator
    {
		private static readonly CmsSignedHelper Helper = CmsSignedHelper.Instance;

		private readonly IList signerInfs = Platform.CreateArrayList();

		private class SignerInf
        {
            private readonly CmsSignedGenerator outer;

			private readonly AsymmetricKeyParameter		key;
			private readonly SignerIdentifier			signerIdentifier;
			private readonly string						digestOID;
			private readonly string						encOID;
			private readonly CmsAttributeTableGenerator	sAttr;
			private readonly CmsAttributeTableGenerator	unsAttr;
			private readonly Asn1.Cms.AttributeTable	baseSignedTable;

			internal SignerInf(
                CmsSignedGenerator			outer,
	            AsymmetricKeyParameter		key,
	            SignerIdentifier			signerIdentifier,
	            string						digestOID,
	            string						encOID,
	            CmsAttributeTableGenerator	sAttr,
	            CmsAttributeTableGenerator	unsAttr,
	            Asn1.Cms.AttributeTable		baseSignedTable)
	        {
                this.outer = outer;
                this.key = key;
                this.signerIdentifier = signerIdentifier;
                this.digestOID = digestOID;
                this.encOID = encOID;
	            this.sAttr = sAttr;
	            this.unsAttr = unsAttr;
	            this.baseSignedTable = baseSignedTable;
            }

			internal AlgorithmIdentifier DigestAlgorithmID
			{
				get { return new AlgorithmIdentifier(new DerObjectIdentifier(digestOID), DerNull.Instance); }
			}

			internal CmsAttributeTableGenerator SignedAttributes
            {
				get { return sAttr; }
            }

            internal CmsAttributeTableGenerator UnsignedAttributes
            {
				get { return unsAttr; }
            }

			internal SignerInfo ToSignerInfo(
                DerObjectIdentifier	contentType,
                CmsProcessable		content,
				SecureRandom		random)
            {
                AlgorithmIdentifier digAlgId = DigestAlgorithmID;
				string digestName = Helper.GetDigestAlgName(digestOID);
				IDigest dig = Helper.GetDigestInstance(digestName);

				string signatureName = digestName + "with" + Helper.GetEncryptionAlgName(encOID);
				ISigner sig = Helper.GetSignatureInstance(signatureName);

				// TODO Optimise the case where more than one signer with same digest
				if (content != null)
                {
                    content.Write(new DigOutputStream(dig));
				}

				byte[] hash = DigestUtilities.DoFinal(dig);
				outer._digests.Add(digestOID, hash.Clone());

				sig.Init(true, new ParametersWithRandom(key, random));
#if NETCF_1_0 || NETCF_2_0 || SILVERLIGHT
				Stream sigStr = new SigOutputStream(sig);
#else
				Stream sigStr = new BufferedStream(new SigOutputStream(sig));
#endif

				Asn1Set signedAttr = null;
				if (sAttr != null)
				{
					IDictionary parameters = outer.GetBaseParameters(contentType, digAlgId, hash);

//					Asn1.Cms.AttributeTable signed = sAttr.GetAttributes(Collections.unmodifiableMap(parameters));
					Asn1.Cms.AttributeTable signed = sAttr.GetAttributes(parameters);

                    if (contentType == null) //counter signature
                    {
                        if (signed != null && signed[CmsAttributes.ContentType] != null)
                        {
                            IDictionary tmpSigned = signed.ToDictionary();
                            tmpSigned.Remove(CmsAttributes.ContentType);
                            signed = new Asn1.Cms.AttributeTable(tmpSigned);
                        }
                    }

					// TODO Validate proposed signed attributes

					signedAttr = outer.GetAttributeSet(signed);

					// sig must be composed from the DER encoding.
					new DerOutputStream(sigStr).WriteObject(signedAttr);
				}
                else if (content != null)
                {
					// TODO Use raw signature of the hash value instead
					content.Write(sigStr);
                }

				sigStr.Close();
				byte[] sigBytes = sig.GenerateSignature();

				Asn1Set unsignedAttr = null;
				if (unsAttr != null)
				{
					IDictionary baseParameters = outer.GetBaseParameters(contentType, digAlgId, hash);
					baseParameters[CmsAttributeTableParameter.Signature] = sigBytes.Clone();

//					Asn1.Cms.AttributeTable unsigned = unsAttr.GetAttributes(Collections.unmodifiableMap(baseParameters));
					Asn1.Cms.AttributeTable unsigned = unsAttr.GetAttributes(baseParameters);

					// TODO Validate proposed unsigned attributes

					unsignedAttr = outer.GetAttributeSet(unsigned);
				}

				// TODO[RSAPSS] Need the ability to specify non-default parameters
				Asn1Encodable sigX509Parameters = SignerUtilities.GetDefaultX509Parameters(signatureName);
				AlgorithmIdentifier encAlgId = CmsSignedGenerator.GetEncAlgorithmIdentifier(
					new DerObjectIdentifier(encOID), sigX509Parameters);
				
                return new SignerInfo(signerIdentifier, digAlgId,
                    signedAttr, encAlgId, new DerOctetString(sigBytes), unsignedAttr);
            }
        }

		public CmsSignedDataGenerator()
        {
        }

		/// <summary>Constructor allowing specific source of randomness</summary>
		/// <param name="rand">Instance of <c>SecureRandom</c> to use.</param>
		public CmsSignedDataGenerator(
			SecureRandom rand)
			: base(rand)
		{
		}

		/**
        * add a signer - no attributes other than the default ones will be
        * provided here.
		*
		* @param key signing key to use
		* @param cert certificate containing corresponding public key
		* @param digestOID digest algorithm OID
        */
        public void AddSigner(
            AsymmetricKeyParameter	privateKey,
            X509Certificate			cert,
            string					digestOID)
        {
        	AddSigner(privateKey, cert, GetEncOid(privateKey, digestOID), digestOID);
		}

		/**
		 * add a signer, specifying the digest encryption algorithm to use - no attributes other than the default ones will be
		 * provided here.
		 *
		 * @param key signing key to use
		 * @param cert certificate containing corresponding public key
		 * @param encryptionOID digest encryption algorithm OID
		 * @param digestOID digest algorithm OID
		 */
		public void AddSigner(
			AsymmetricKeyParameter	privateKey,
			X509Certificate			cert,
			string					encryptionOID,
			string					digestOID)
		{
			doAddSigner(privateKey, GetSignerIdentifier(cert), encryptionOID, digestOID,
				new DefaultSignedAttributeTableGenerator(), null, null);
		}

	    /**
	     * add a signer - no attributes other than the default ones will be
	     * provided here.
	     */
	    public void AddSigner(
            AsymmetricKeyParameter	privateKey,
	        byte[]					subjectKeyID,
            string					digestOID)
	    {
			AddSigner(privateKey, subjectKeyID, GetEncOid(privateKey, digestOID), digestOID);
	    }

		/**
		 * add a signer, specifying the digest encryption algorithm to use - no attributes other than the default ones will be
		 * provided here.
		 */
		public void AddSigner(
			AsymmetricKeyParameter	privateKey,
			byte[]					subjectKeyID,
			string					encryptionOID,
			string					digestOID)
		{
			doAddSigner(privateKey, GetSignerIdentifier(subjectKeyID), encryptionOID, digestOID,
				new DefaultSignedAttributeTableGenerator(), null, null);
		}

        /**
        * add a signer with extra signed/unsigned attributes.
		*
		* @param key signing key to use
		* @param cert certificate containing corresponding public key
		* @param digestOID digest algorithm OID
		* @param signedAttr table of attributes to be included in signature
		* @param unsignedAttr table of attributes to be included as unsigned
        */
        public void AddSigner(
            AsymmetricKeyParameter	privateKey,
            X509Certificate			cert,
            string					digestOID,
            Asn1.Cms.AttributeTable	signedAttr,
            Asn1.Cms.AttributeTable	unsignedAttr)
        {
			AddSigner(privateKey, cert, GetEncOid(privateKey, digestOID), digestOID,
				signedAttr, unsignedAttr);
		}

		/**
		 * add a signer, specifying the digest encryption algorithm, with extra signed/unsigned attributes.
		 *
		 * @param key signing key to use
		 * @param cert certificate containing corresponding public key
		 * @param encryptionOID digest encryption algorithm OID
		 * @param digestOID digest algorithm OID
		 * @param signedAttr table of attributes to be included in signature
		 * @param unsignedAttr table of attributes to be included as unsigned
		 */
		public void AddSigner(
			AsymmetricKeyParameter	privateKey,
			X509Certificate			cert,
			string					encryptionOID,
			string					digestOID,
			Asn1.Cms.AttributeTable	signedAttr,
			Asn1.Cms.AttributeTable	unsignedAttr)
		{
			doAddSigner(privateKey, GetSignerIdentifier(cert), encryptionOID, digestOID,
				new DefaultSignedAttributeTableGenerator(signedAttr),
				new SimpleAttributeTableGenerator(unsignedAttr),
				signedAttr);
		}

	    /**
	     * add a signer with extra signed/unsigned attributes.
		 *
		 * @param key signing key to use
		 * @param subjectKeyID subjectKeyID of corresponding public key
		 * @param digestOID digest algorithm OID
		 * @param signedAttr table of attributes to be included in signature
		 * @param unsignedAttr table of attributes to be included as unsigned
	     */
		public void AddSigner(
			AsymmetricKeyParameter	privateKey,
			byte[]					subjectKeyID,
			string					digestOID,
			Asn1.Cms.AttributeTable	signedAttr,
			Asn1.Cms.AttributeTable	unsignedAttr)
		{
			AddSigner(privateKey, subjectKeyID, GetEncOid(privateKey, digestOID), digestOID,
				signedAttr, unsignedAttr); 
		}

		/**
		 * add a signer, specifying the digest encryption algorithm, with extra signed/unsigned attributes.
		 *
		 * @param key signing key to use
		 * @param subjectKeyID subjectKeyID of corresponding public key
		 * @param encryptionOID digest encryption algorithm OID
		 * @param digestOID digest algorithm OID
		 * @param signedAttr table of attributes to be included in signature
		 * @param unsignedAttr table of attributes to be included as unsigned
		 */
		public void AddSigner(
			AsymmetricKeyParameter	privateKey,
			byte[]					subjectKeyID,
			string					encryptionOID,
			string					digestOID,
			Asn1.Cms.AttributeTable	signedAttr,
			Asn1.Cms.AttributeTable	unsignedAttr)
		{
			doAddSigner(privateKey, GetSignerIdentifier(subjectKeyID), encryptionOID, digestOID,
				new DefaultSignedAttributeTableGenerator(signedAttr),
				new SimpleAttributeTableGenerator(unsignedAttr),
				signedAttr);
		}

		/**
		 * add a signer with extra signed/unsigned attributes based on generators.
		 */
		public void AddSigner(
			AsymmetricKeyParameter		privateKey,
			X509Certificate				cert,
			string						digestOID,
			CmsAttributeTableGenerator	signedAttrGen,
			CmsAttributeTableGenerator	unsignedAttrGen)
		{
			AddSigner(privateKey, cert, GetEncOid(privateKey, digestOID), digestOID,
				signedAttrGen, unsignedAttrGen);
		}

		/**
		 * add a signer, specifying the digest encryption algorithm, with extra signed/unsigned attributes based on generators.
		 */
		public void AddSigner(
			AsymmetricKeyParameter		privateKey,
			X509Certificate				cert,
			string						encryptionOID,
			string						digestOID,
			CmsAttributeTableGenerator	signedAttrGen,
			CmsAttributeTableGenerator	unsignedAttrGen)
		{
			doAddSigner(privateKey, GetSignerIdentifier(cert), encryptionOID, digestOID, signedAttrGen,
				unsignedAttrGen, null);
		}

	    /**
	     * add a signer with extra signed/unsigned attributes based on generators.
	     */
	    public void AddSigner(
			AsymmetricKeyParameter		privateKey,
	        byte[]						subjectKeyID,
	        string						digestOID,
	        CmsAttributeTableGenerator	signedAttrGen,
	        CmsAttributeTableGenerator	unsignedAttrGen)
	    {
			AddSigner(privateKey, subjectKeyID, GetEncOid(privateKey, digestOID), digestOID,
				signedAttrGen, unsignedAttrGen);
	    }

		/**
		 * add a signer, including digest encryption algorithm, with extra signed/unsigned attributes based on generators.
		 */
		public void AddSigner(
			AsymmetricKeyParameter		privateKey,
			byte[]						subjectKeyID,
			string						encryptionOID,
			string						digestOID,
			CmsAttributeTableGenerator	signedAttrGen,
			CmsAttributeTableGenerator	unsignedAttrGen)
		{
			doAddSigner(privateKey, GetSignerIdentifier(subjectKeyID), encryptionOID, digestOID,
				signedAttrGen, unsignedAttrGen, null);
		}

		private void doAddSigner(
			AsymmetricKeyParameter		privateKey,
			SignerIdentifier            signerIdentifier,
			string                      encryptionOID,
			string                      digestOID,
			CmsAttributeTableGenerator  signedAttrGen,
			CmsAttributeTableGenerator  unsignedAttrGen,
			Asn1.Cms.AttributeTable		baseSignedTable)
		{
			signerInfs.Add(new SignerInf(this, privateKey, signerIdentifier, digestOID, encryptionOID,
				signedAttrGen, unsignedAttrGen, baseSignedTable));
		}

		/**
        * generate a signed object that for a CMS Signed Data object
        */
        public CmsSignedData Generate(
            CmsProcessable content)
        {
            return Generate(content, false);
        }

        /**
        * generate a signed object that for a CMS Signed Data
        * object  - if encapsulate is true a copy
        * of the message will be included in the signature. The content type
        * is set according to the OID represented by the string signedContentType.
        */
        public CmsSignedData Generate(
            string			signedContentType,
			// FIXME Avoid accessing more than once to support CmsProcessableInputStream
            CmsProcessable	content,
            bool			encapsulate)
        {
            Asn1EncodableVector digestAlgs = new Asn1EncodableVector();
            Asn1EncodableVector signerInfos = new Asn1EncodableVector();

			_digests.Clear(); // clear the current preserved digest state

			//
            // add the precalculated SignerInfo objects.
            //
            foreach (SignerInformation signer in _signers)
            {
				digestAlgs.Add(Helper.FixAlgID(signer.DigestAlgorithmID));

				// TODO Verify the content type and calculated digest match the precalculated SignerInfo
				signerInfos.Add(signer.ToSignerInfo());
            }

			//
            // add the SignerInfo objects
            //
            bool isCounterSignature = (signedContentType == null);

            DerObjectIdentifier contentTypeOid = isCounterSignature
                ?   null
				:	new DerObjectIdentifier(signedContentType);

            foreach (SignerInf signer in signerInfs)
            {
				try
                {
					digestAlgs.Add(signer.DigestAlgorithmID);
                    signerInfos.Add(signer.ToSignerInfo(contentTypeOid, content, rand));
				}
                catch (IOException e)
                {
                    throw new CmsException("encoding error.", e);
                }
                catch (InvalidKeyException e)
                {
                    throw new CmsException("key inappropriate for signature.", e);
                }
                catch (SignatureException e)
                {
                    throw new CmsException("error creating signature.", e);
                }
                catch (CertificateEncodingException e)
                {
                    throw new CmsException("error creating sid.", e);
                }
            }

			Asn1Set certificates = null;

			if (_certs.Count != 0)
			{
				certificates = CmsUtilities.CreateBerSetFromList(_certs);
			}

			Asn1Set certrevlist = null;

			if (_crls.Count != 0)
			{
				certrevlist = CmsUtilities.CreateBerSetFromList(_crls);
			}

			Asn1OctetString octs = null;
			if (encapsulate)
            {
                MemoryStream bOut = new MemoryStream();
				if (content != null)
				{
	                try
	                {
	                    content.Write(bOut);
	                }
	                catch (IOException e)
	                {
	                    throw new CmsException("encapsulation error.", e);
	                }
				}
				octs = new BerOctetString(bOut.ToArray());
            }

            ContentInfo encInfo = new ContentInfo(contentTypeOid, octs);

            SignedData sd = new SignedData(
                new DerSet(digestAlgs),
                encInfo,
                certificates,
                certrevlist,
                new DerSet(signerInfos));

            ContentInfo contentInfo = new ContentInfo(CmsObjectIdentifiers.SignedData, sd);

            return new CmsSignedData(content, contentInfo);
        }

        /**
        * generate a signed object that for a CMS Signed Data
        * object - if encapsulate is true a copy
        * of the message will be included in the signature with the
        * default content type "data".
        */
        public CmsSignedData Generate(
            CmsProcessable	content,
            bool			encapsulate)
        {
            return this.Generate(Data, content, encapsulate);
        }

		/**
		* generate a set of one or more SignerInformation objects representing counter signatures on
		* the passed in SignerInformation object.
		*
		* @param signer the signer to be countersigned
		* @param sigProvider the provider to be used for counter signing.
		* @return a store containing the signers.
		*/
		public SignerInformationStore GenerateCounterSigners(
			SignerInformation signer)
		{
			return this.Generate(null, new CmsProcessableByteArray(signer.GetSignature()), false).GetSignerInfos();
		}
	}
}
