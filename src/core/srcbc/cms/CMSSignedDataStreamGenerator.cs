using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.Utilities.IO;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cms
{
    /**
    * General class for generating a pkcs7-signature message stream.
    * <p>
    * A simple example of usage.
    * </p>
    * <pre>
    *      IX509Store                   certs...
    *      CmsSignedDataStreamGenerator gen = new CmsSignedDataStreamGenerator();
    *
    *      gen.AddSigner(privateKey, cert, CmsSignedDataStreamGenerator.DIGEST_SHA1);
    *
    *      gen.AddCertificates(certs);
    *
    *      Stream sigOut = gen.Open(bOut);
    *
    *      sigOut.Write(Encoding.UTF8.GetBytes("Hello World!"));
    *
    *      sigOut.Close();
    * </pre>
    */
    public class CmsSignedDataStreamGenerator
        : CmsSignedGenerator
    {
		private static readonly CmsSignedHelper Helper = CmsSignedHelper.Instance;

		private readonly ArrayList _signerInfs = new ArrayList();
		private readonly ISet       _messageDigestOids = new HashSet();
		private readonly Hashtable _messageDigests = new Hashtable();
		private readonly Hashtable _messageHashes = new Hashtable();
		private bool _messageDigestsLocked;
        private int _bufferSize;

		private class SignerInf
        {
			private readonly CmsSignedDataStreamGenerator outer;

			private readonly AsymmetricKeyParameter		_key;
			private readonly SignerIdentifier			_signerIdentifier;
			private readonly string						_digestOID;
			private readonly string						_encOID;
			private readonly CmsAttributeTableGenerator	_sAttr;
			private readonly CmsAttributeTableGenerator	_unsAttr;

			internal SignerInf(
				CmsSignedDataStreamGenerator	outer,
				AsymmetricKeyParameter			key,
				SignerIdentifier				signerIdentifier,
				string							digestOID,
				string							encOID,
				CmsAttributeTableGenerator		sAttr,
				CmsAttributeTableGenerator		unsAttr)
			{
				this.outer = outer;

				_key = key;
				_signerIdentifier = signerIdentifier;
				_digestOID = digestOID;
				_encOID = encOID;
				_sAttr = sAttr;
				_unsAttr = unsAttr;
			}

			internal AlgorithmIdentifier DigestAlgorithmID
			{
				get { return new AlgorithmIdentifier(new DerObjectIdentifier(this._digestOID), DerNull.Instance); }
			}

			internal SignerInfo ToSignerInfo(
                DerObjectIdentifier contentType)
            {
				string digestName = Helper.GetDigestAlgName(_digestOID);
				string encName = Helper.GetEncryptionAlgName(_encOID);
				string signatureName = digestName + "with" + encName;

				AlgorithmIdentifier digAlgId = DigestAlgorithmID;

				byte[] hash = (byte[])outer._messageHashes[Helper.GetDigestAlgName(this._digestOID)];
				outer._digests[_digestOID] = hash.Clone();

				byte[] bytesToSign = hash;
				ISigner sig;

				/* RFC 3852 5.4
				 * The result of the message digest calculation process depends on
				 * whether the signedAttrs field is present.  When the field is absent,
				 * the result is just the message digest of the content as described
				 * 
				 * above.  When the field is present, however, the result is the message
				 * digest of the complete DER encoding of the SignedAttrs value
				 * contained in the signedAttrs field.
				 */
				Asn1Set signedAttr = null;
				if (_sAttr != null)
				{
					IDictionary parameters = outer.GetBaseParameters(contentType, digAlgId, hash);
//					Asn1.Cms.AttributeTable signed = _sAttr.GetAttributes(Collections.unmodifiableMap(parameters));
					Asn1.Cms.AttributeTable signed = _sAttr.GetAttributes(parameters);

					// TODO Handle countersignatures (see CMSSignedDataGenerator)

					signedAttr = outer.GetAttributeSet(signed);

                	// sig must be composed from the DER encoding.
					bytesToSign = signedAttr.GetEncoded(Asn1Encodable.Der);
            		sig = Helper.GetSignatureInstance(signatureName);
				}
				else
				{
					// Note: Need to use raw signatures here since we have already calculated the digest
					if (encName.Equals("RSA"))
					{
						DigestInfo dInfo = new DigestInfo(digAlgId, hash);
						bytesToSign = dInfo.GetEncoded(Asn1Encodable.Der);
						sig = Helper.GetSignatureInstance("RSA");
					}
					else if (encName.Equals("DSA"))
					{
						sig = Helper.GetSignatureInstance("NONEwithDSA");
					}
					// TODO Add support for raw PSS
//					else if (encName.equals("RSAandMGF1"))
//					{
//						sig = CMSSignedHelper.INSTANCE.getSignatureInstance("NONEWITHRSAPSS", _sigProvider);
//						try
//						{
//							// Init the params this way to avoid having a 'raw' version of each PSS algorithm
//							Signature sig2 = CMSSignedHelper.INSTANCE.getSignatureInstance(signatureName, _sigProvider);
//							PSSParameterSpec spec = (PSSParameterSpec)sig2.getParameters().getParameterSpec(PSSParameterSpec.class);
//							sig.setParameter(spec);
//						}
//						catch (Exception e)
//						{
//							throw new SignatureException("algorithm: " + encName + " could not be configured.");
//						}
//					}
					else
					{
						throw new SignatureException("algorithm: " + encName + " not supported in base signatures.");
					}
				}

				sig.Init(true, new ParametersWithRandom(_key, outer.rand));
				sig.BlockUpdate(bytesToSign, 0, bytesToSign.Length);
				byte[] sigBytes = sig.GenerateSignature();

				Asn1Set unsignedAttr = null;
				if (_unsAttr != null)
				{
					IDictionary parameters = outer.GetBaseParameters(contentType, digAlgId, hash);
					parameters[CmsAttributeTableParameter.Signature] = sigBytes.Clone();

//					Asn1.Cms.AttributeTable unsigned = _unsAttr.getAttributes(Collections.unmodifiableMap(parameters));
					Asn1.Cms.AttributeTable unsigned = _unsAttr.GetAttributes(parameters);

					unsignedAttr = outer.GetAttributeSet(unsigned);
				}

				// TODO[RSAPSS] Need the ability to specify non-default parameters
				Asn1Encodable sigX509Parameters = SignerUtilities.GetDefaultX509Parameters(signatureName);
				AlgorithmIdentifier encAlgId = CmsSignedGenerator.GetEncAlgorithmIdentifier(
					new DerObjectIdentifier(_encOID), sigX509Parameters);

				return new SignerInfo(_signerIdentifier, digAlgId,
					signedAttr, encAlgId, new DerOctetString(sigBytes), unsignedAttr);
            }
        }

		public CmsSignedDataStreamGenerator()
        {
        }

		/// <summary>Constructor allowing specific source of randomness</summary>
		/// <param name="rand">Instance of <c>SecureRandom</c> to use.</param>
		public CmsSignedDataStreamGenerator(
			SecureRandom rand)
			: base(rand)
		{
		}

		/**
        * Set the underlying string size for encapsulated data
        *
        * @param bufferSize length of octet strings to buffer the data.
        */
        public void SetBufferSize(
            int bufferSize)
        {
            _bufferSize = bufferSize;
        }

		public void AddDigests(
       		params string[] digestOids)
		{
       		AddDigests((IEnumerable) digestOids);
		}

		public void AddDigests(
			IEnumerable digestOids)
		{
			foreach (string digestOid in digestOids)
			{
				ConfigureDigest(digestOid);
			}
		}

		/**
        * add a signer - no attributes other than the default ones will be
        * provided here.
        * @throws NoSuchAlgorithmException
        * @throws InvalidKeyException
        */
        public void AddSigner(
            AsymmetricKeyParameter	privateKey,
            X509Certificate			cert,
            string					digestOid)
        {
			AddSigner(privateKey, cert, digestOid,
				new DefaultSignedAttributeTableGenerator(), null);
		}

		/**
		 * add a signer, specifying the digest encryption algorithm - no attributes other than the default ones will be
		 * provided here.
		 * @throws NoSuchProviderException
		 * @throws NoSuchAlgorithmException
		 * @throws InvalidKeyException
		 */
		public void AddSigner(
			AsymmetricKeyParameter	privateKey,
			X509Certificate			cert,
			string					encryptionOid,
			string					digestOid)
		{
			AddSigner(privateKey, cert, encryptionOid, digestOid,
				new DefaultSignedAttributeTableGenerator(),
				(CmsAttributeTableGenerator)null);
		}

        /**
        * add a signer with extra signed/unsigned attributes.
        * @throws NoSuchAlgorithmException
        * @throws InvalidKeyException
        */
        public void AddSigner(
            AsymmetricKeyParameter	privateKey,
            X509Certificate			cert,
            string					digestOid,
            Asn1.Cms.AttributeTable	signedAttr,
            Asn1.Cms.AttributeTable	unsignedAttr)
        {
			AddSigner(privateKey, cert, digestOid,
				new DefaultSignedAttributeTableGenerator(signedAttr),
				new SimpleAttributeTableGenerator(unsignedAttr));
		}

		/**
		 * add a signer with extra signed/unsigned attributes - specifying digest
		 * encryption algorithm.
		 * @throws NoSuchProviderException
		 * @throws NoSuchAlgorithmException
		 * @throws InvalidKeyException
		 */
		public void AddSigner(
			AsymmetricKeyParameter	privateKey,
			X509Certificate			cert,
			string					encryptionOid,
			string					digestOid,
			Asn1.Cms.AttributeTable	signedAttr,
			Asn1.Cms.AttributeTable	unsignedAttr)
		{
			AddSigner(privateKey, cert, encryptionOid, digestOid,
				new DefaultSignedAttributeTableGenerator(signedAttr),
				new SimpleAttributeTableGenerator(unsignedAttr));
		}

		public void AddSigner(
			AsymmetricKeyParameter		privateKey,
			X509Certificate				cert,
			string						digestOid,
			CmsAttributeTableGenerator  signedAttrGenerator,
			CmsAttributeTableGenerator  unsignedAttrGenerator)
		{
			AddSigner(privateKey, cert, GetEncOid(privateKey, digestOid), digestOid,
				signedAttrGenerator, unsignedAttrGenerator);
        }

		public void AddSigner(
			AsymmetricKeyParameter		privateKey,
			X509Certificate				cert,
			string						encryptionOid,
			string						digestOid,
			CmsAttributeTableGenerator  signedAttrGenerator,
			CmsAttributeTableGenerator  unsignedAttrGenerator)
		{
			ConfigureDigest(digestOid);

			_signerInfs.Add(new SignerInf(this, privateKey, GetSignerIdentifier(cert), digestOid, encryptionOid,
				signedAttrGenerator, unsignedAttrGenerator));
		}

		/**
		* add a signer - no attributes other than the default ones will be
		* provided here.
		* @throws NoSuchAlgorithmException
		* @throws InvalidKeyException
		*/
		public void AddSigner(
			AsymmetricKeyParameter	privateKey,
			byte[]					subjectKeyID,
			string					digestOid)
		{
			AddSigner(privateKey, subjectKeyID, digestOid, new DefaultSignedAttributeTableGenerator(),
				(CmsAttributeTableGenerator)null);
		}

		/**
		 * add a signer - no attributes other than the default ones will be
		 * provided here.
		 * @throws NoSuchProviderException
		 * @throws NoSuchAlgorithmException
		 * @throws InvalidKeyException
		 */
		public void AddSigner(
			AsymmetricKeyParameter	privateKey,
			byte[]					subjectKeyID,
			string					encryptionOid,
			string					digestOid)
		{
			AddSigner(privateKey, subjectKeyID, encryptionOid, digestOid,
				new DefaultSignedAttributeTableGenerator(),
				(CmsAttributeTableGenerator)null);
		}

		/**
		* add a signer with extra signed/unsigned attributes.
		* @throws NoSuchAlgorithmException
		* @throws InvalidKeyException
		*/
		public void AddSigner(
			AsymmetricKeyParameter	privateKey,
			byte[]					subjectKeyID,
			string					digestOid,
			Asn1.Cms.AttributeTable	signedAttr,
			Asn1.Cms.AttributeTable	unsignedAttr)
	    {
	        AddSigner(privateKey, subjectKeyID, digestOid,
				new DefaultSignedAttributeTableGenerator(signedAttr),
				new SimpleAttributeTableGenerator(unsignedAttr));
		}

		public void AddSigner(
			AsymmetricKeyParameter		privateKey,
			byte[]						subjectKeyID,
			string						digestOid,
			CmsAttributeTableGenerator	signedAttrGenerator,
			CmsAttributeTableGenerator	unsignedAttrGenerator)
		{
			AddSigner(privateKey, subjectKeyID, GetEncOid(privateKey, digestOid),
				digestOid, signedAttrGenerator, unsignedAttrGenerator);
		}

		public void AddSigner(
			AsymmetricKeyParameter		privateKey,
			byte[]						subjectKeyID,
			string						encryptionOid,
			string						digestOid,
			CmsAttributeTableGenerator	signedAttrGenerator,
			CmsAttributeTableGenerator	unsignedAttrGenerator)
		{
			ConfigureDigest(digestOid);

			_signerInfs.Add(new SignerInf(this, privateKey, GetSignerIdentifier(subjectKeyID), digestOid, encryptionOid,
				signedAttrGenerator, unsignedAttrGenerator));
		}

		internal override void AddSignerCallback(
			SignerInformation si)
		{
			// FIXME If there were parameters in si.DigestAlgorithmID.Parameters, they are lost
			// NB: Would need to call FixAlgID on the DigestAlgorithmID

			// For precalculated signers, just need to register the algorithm, not configure a digest
			RegisterDigestOid(si.DigestAlgorithmID.ObjectID.Id);
		}

		/**
        * generate a signed object that for a CMS Signed Data object
        */
        public Stream Open(
            Stream outStream)
        {
            return Open(outStream, false);
        }

        /**
        * generate a signed object that for a CMS Signed Data
        * object - if encapsulate is true a copy
        * of the message will be included in the signature with the
        * default content type "data".
        */
        public Stream Open(
            Stream	outStream,
            bool	encapsulate)
        {
            return Open(outStream, Data, encapsulate);
        }

		/**
		 * generate a signed object that for a CMS Signed Data
		 * object using the given provider - if encapsulate is true a copy
		 * of the message will be included in the signature with the
		 * default content type "data". If dataOutputStream is non null the data
		 * being signed will be written to the stream as it is processed.
		 * @param out stream the CMS object is to be written to.
		 * @param encapsulate true if data should be encapsulated.
		 * @param dataOutputStream output stream to copy the data being signed to.
		 */
		public Stream Open(
			Stream	outStream,
			bool	encapsulate,
			Stream	dataOutputStream)
		{
			return Open(outStream, Data, encapsulate, dataOutputStream);
		}

		/**
        * generate a signed object that for a CMS Signed Data
        * object - if encapsulate is true a copy
        * of the message will be included in the signature. The content type
        * is set according to the OID represented by the string signedContentType.
        */
        public Stream Open(
            Stream	outStream,
            string	signedContentType,
            bool	encapsulate)
        {
			return Open(outStream, signedContentType, encapsulate, null);
		}

		/**
		* generate a signed object that for a CMS Signed Data
		* object using the given provider - if encapsulate is true a copy
		* of the message will be included in the signature. The content type
		* is set according to the OID represented by the string signedContentType.
		* @param out stream the CMS object is to be written to.
		* @param signedContentType OID for data to be signed.
		* @param encapsulate true if data should be encapsulated.
		* @param dataOutputStream output stream to copy the data being signed to.
		*/
		public Stream Open(
			Stream	outStream,
			string	signedContentType,
			bool	encapsulate,
			Stream	dataOutputStream)
		{
			if (outStream == null)
				throw new ArgumentNullException("outStream");
			if (!outStream.CanWrite)
				throw new ArgumentException("Expected writeable stream", "outStream");
			if (dataOutputStream != null && !dataOutputStream.CanWrite)
				throw new ArgumentException("Expected writeable stream", "dataOutputStream");

			_messageDigestsLocked = true;
			
			//
            // ContentInfo
            //
            BerSequenceGenerator sGen = new BerSequenceGenerator(outStream);

			sGen.AddObject(CmsObjectIdentifiers.SignedData);

			//
            // Signed Data
            //
            BerSequenceGenerator sigGen = new BerSequenceGenerator(
				sGen.GetRawOutputStream(), 0, true);

			sigGen.AddObject(CalculateVersion(signedContentType));

			Asn1EncodableVector digestAlgs = new Asn1EncodableVector();

			foreach (string digestOid in _messageDigestOids)
            {
				digestAlgs.Add(
            		new AlgorithmIdentifier(new DerObjectIdentifier(digestOid), DerNull.Instance));
            }

            {
				byte[] tmp = new DerSet(digestAlgs).GetEncoded();
				sigGen.GetRawOutputStream().Write(tmp, 0, tmp.Length);
			}

			BerSequenceGenerator eiGen = new BerSequenceGenerator(sigGen.GetRawOutputStream());
			eiGen.AddObject(new DerObjectIdentifier(signedContentType));

        	// If encapsulating, add the data as an octet string in the sequence
			Stream encapStream = encapsulate
				?	CmsUtilities.CreateBerOctetOutputStream(eiGen.GetRawOutputStream(), 0, true, _bufferSize)
				:	null;

        	// Also send the data to 'dataOutputStream' if necessary
			Stream teeStream = GetSafeTeeOutputStream(dataOutputStream, encapStream);

        	// Let all the digests see the data as it is written
			Stream digStream = AttachDigestsToOutputStream(_messageDigests.Values, teeStream);

			return new CmsSignedDataOutputStream(this, digStream, signedContentType, sGen, sigGen, eiGen);
        }

		private void RegisterDigestOid(
			string digestOid)
		{
       		if (_messageDigestsLocked)
       		{
       			if (!_messageDigestOids.Contains(digestOid))
					throw new InvalidOperationException("Cannot register new digest OIDs after the data stream is opened");
       		}
       		else
       		{
				_messageDigestOids.Add(digestOid);
       		}
		}

		private void ConfigureDigest(
			string digestOid)
		{
       		RegisterDigestOid(digestOid);

       		string digestName = Helper.GetDigestAlgName(digestOid);
			IDigest dig = (IDigest)_messageDigests[digestName];
			if (dig == null)
			{
				if (_messageDigestsLocked)
					throw new InvalidOperationException("Cannot configure new digests after the data stream is opened");

            	dig = Helper.GetDigestInstance(digestName);
            	_messageDigests[digestName] = dig;
            }
		}

		// RFC3852, section 5.1:
		// IF ((certificates is present) AND
		//    (any certificates with a type of other are present)) OR
		//    ((crls is present) AND
		//    (any crls with a type of other are present))
		// THEN version MUST be 5
		// ELSE
		//    IF (certificates is present) AND
		//       (any version 2 attribute certificates are present)
		//    THEN version MUST be 4
		//    ELSE
		//       IF ((certificates is present) AND
		//          (any version 1 attribute certificates are present)) OR
		//          (any SignerInfo structures are version 3) OR
		//          (encapContentInfo eContentType is other than id-data)
		//       THEN version MUST be 3
		//       ELSE version MUST be 1
		//
		private DerInteger CalculateVersion(
			string contentOid)
		{
			bool otherCert = false;
			bool otherCrl = false;
			bool attrCertV1Found = false;
			bool attrCertV2Found = false;

			if (_certs != null)
			{
				foreach (object obj in _certs)
				{
					if (obj is Asn1TaggedObject)
					{
						Asn1TaggedObject tagged = (Asn1TaggedObject) obj;

						if (tagged.TagNo == 1)
						{
							attrCertV1Found = true;
						}
						else if (tagged.TagNo == 2)
						{
							attrCertV2Found = true;
						}
						else if (tagged.TagNo == 3)
						{
							otherCert = true;
							break;
						}
					}
				}
			}

			if (otherCert)
			{
				return new DerInteger(5);
			}

			if (_crls != null)
			{
				foreach (object obj in _crls)
				{
					if (obj is Asn1TaggedObject)
					{
						otherCrl = true;
						break;
					}
				}
			}

			if (otherCrl)
			{
				return new DerInteger(5);
			}

			if (attrCertV2Found)
			{
				return new DerInteger(4);
			}

			if (attrCertV1Found)
			{
				return new DerInteger(3);
			}

			if (contentOid.Equals(Data)
				&& !CheckForVersion3(_signers))
			{
				return new DerInteger(1);
			}

			return new DerInteger(3);
        }

		private bool CheckForVersion3(
			IList signerInfos)
		{
			foreach (SignerInformation si in signerInfos)
			{
				SignerInfo s = SignerInfo.GetInstance(si.ToSignerInfo());

				if (s.Version.Value.IntValue == 3)
				{
					return true;
				}
			}

			return false;
		}

		private static Stream AttachDigestsToOutputStream(ICollection digests, Stream s)
		{
			Stream result = s;
			foreach (IDigest digest in digests)
			{
				result = GetSafeTeeOutputStream(result, new DigOutputStream(digest));
			}
			return result;
		}

		private static Stream GetSafeOutputStream(Stream s)
		{
			if (s == null)
				return new NullOutputStream();
			return s;
		}

		private static Stream GetSafeTeeOutputStream(Stream s1, Stream s2)
		{
			if (s1 == null)
				return GetSafeOutputStream(s2);
			if (s2 == null)
				return GetSafeOutputStream(s1);
			return new TeeOutputStream(s1, s2);
		}

		private class CmsSignedDataOutputStream
            : BaseOutputStream
        {
			private readonly CmsSignedDataStreamGenerator outer;

			private Stream					_out;
            private DerObjectIdentifier		_contentOID;
            private BerSequenceGenerator	_sGen;
            private BerSequenceGenerator	_sigGen;
            private BerSequenceGenerator	_eiGen;

			public CmsSignedDataOutputStream(
				CmsSignedDataStreamGenerator	outer,
				Stream							outStream,
                string							contentOID,
                BerSequenceGenerator			sGen,
                BerSequenceGenerator			sigGen,
                BerSequenceGenerator			eiGen)
            {
				this.outer = outer;

				_out = outStream;
                _contentOID = new DerObjectIdentifier(contentOID);
                _sGen = sGen;
                _sigGen = sigGen;
                _eiGen = eiGen;
            }

			public override void WriteByte(
                byte b)
            {
                _out.WriteByte(b);
            }

			public override void Write(
                byte[]	bytes,
                int		off,
                int		len)
            {
                _out.Write(bytes, off, len);
            }

			public override void Close()
            {
                _out.Close();
                _eiGen.Close();

				outer._digests.Clear();    // clear the current preserved digest state

				if (outer._certs.Count > 0)
				{
					Asn1Set certs = CmsUtilities.CreateBerSetFromList(outer._certs);

					WriteToGenerator(_sigGen, new BerTaggedObject(false, 0, certs));
				}

				if (outer._crls.Count > 0)
				{
					Asn1Set crls = CmsUtilities.CreateBerSetFromList(outer._crls);

					WriteToGenerator(_sigGen, new BerTaggedObject(false, 1, crls));
				}

				//
				// Calculate the digest hashes
				//
				foreach (DictionaryEntry de in outer._messageDigests)
				{
					outer._messageHashes.Add(de.Key, DigestUtilities.DoFinal((IDigest)de.Value));
				}

				// TODO If the digest OIDs for precalculated signers weren't mixed in with
				// the others, we could fill in outer._digests here, instead of SignerInf.ToSignerInfo

				//
                // add the precalculated SignerInfo objects.
                //
                Asn1EncodableVector signerInfos = new Asn1EncodableVector();

				foreach (SignerInformation signer in outer._signers)
				{
                    signerInfos.Add(signer.ToSignerInfo());
                }
				
				//
                // add the SignerInfo objects
                //
				foreach (SignerInf signer in outer._signerInfs)
				{
                    try
                    {
                        signerInfos.Add(signer.ToSignerInfo(_contentOID));
                    }
                    catch (IOException e)
                    {
                        throw new CmsStreamException("encoding error." + e);
                    }
					catch (InvalidKeyException e)
					{
						throw new CmsStreamException("key inappropriate for signature.", e);
					}
                    catch (SignatureException e)
                    {
                        throw new CmsStreamException("error creating signature." + e);
                    }
                    catch (CertificateEncodingException e)
                    {
                        throw new CmsStreamException("error creating sid." + e);
                    }
					catch (SecurityUtilityException e)
					{
						throw new CmsStreamException("unknown signature algorithm." + e);
					}
                }

				WriteToGenerator(_sigGen, new DerSet(signerInfos));

				_sigGen.Close();
                _sGen.Close();
				base.Close();
			}

			private static void WriteToGenerator(
				Asn1Generator	ag,
				Asn1Encodable	ae)
			{
				byte[] encoded = ae.GetEncoded();
				ag.GetRawOutputStream().Write(encoded, 0, encoded.Length);
			}
		}
    }
}
