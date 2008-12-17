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

			private AsymmetricKeyParameter		_key;
            private X509Certificate				_cert;
            private string						_digestOID;
            private string						_encOID;
			private CmsAttributeTableGenerator	_sAttr;
			private CmsAttributeTableGenerator	_unsAttr;
            private ISigner						_signature;
            private byte[]						_subjectKeyID;

            internal SignerInf(
				CmsSignedDataStreamGenerator	outer,
				AsymmetricKeyParameter			key,
				X509Certificate					cert,
				string							digestOID,
				string							encOID,
				CmsAttributeTableGenerator		sAttr,
				CmsAttributeTableGenerator		unsAttr,
				ISigner							signature)
			{
				this.outer = outer;

				_key = key;
				_cert = cert;
				_digestOID = digestOID;
				_encOID = encOID;
				_sAttr = sAttr;
				_unsAttr = unsAttr;
				_signature = signature;
			}

			internal SignerInf(
				CmsSignedDataStreamGenerator	outer,
				AsymmetricKeyParameter			key,
				byte[]							subjectKeyID,
				string							digestOID,
				string							encOID,
				CmsAttributeTableGenerator		sAttr,
				CmsAttributeTableGenerator		unsAttr,
				ISigner							signature)
	        {
				this.outer = outer;

	            _key = key;
	            _subjectKeyID = subjectKeyID;
	            _digestOID = digestOID;
	            _encOID = encOID;
	            _sAttr = sAttr;
	            _unsAttr = unsAttr;
	            _signature = signature;
	        }

            internal AsymmetricKeyParameter Key
            {
				get { return _key; }
            }

			internal X509Certificate Certificate
            {
				get { return _cert; }
            }

			internal string EncryptionAlgOid
            {
				get { return _encOID; }
            }

			internal SignerInfo ToSignerInfo(
                DerObjectIdentifier contentType)
            {
                AlgorithmIdentifier digAlgId = new AlgorithmIdentifier(
                    new DerObjectIdentifier(this._digestOID), DerNull.Instance);
				AlgorithmIdentifier encAlgId = CmsSignedGenerator.GetEncAlgorithmIdentifier(this.EncryptionAlgOid);

				byte[] hash = (byte[])outer._messageHashes[Helper.GetDigestAlgName(this._digestOID)];

				outer._digests[_digestOID] = hash.Clone();

				IDictionary parameters = outer.GetBaseParameters(contentType, digAlgId, hash);

				Asn1.Cms.AttributeTable signed = (_sAttr != null)
//					?	_sAttr.GetAttributes(Collections.unmodifiableMap(parameters))
					?	_sAttr.GetAttributes(parameters)
					:	null;

				Asn1Set signedAttr = outer.GetAttributeSet(signed);

                //
                // sig must be composed from the DER encoding.
                //
				byte[] tmp;
				if (signedAttr != null)
                {
					tmp = signedAttr.GetEncoded(Asn1Encodable.Der);
				}
                else
                {
					throw new Exception("signatures without signed attributes not implemented.");
				}

				_signature.BlockUpdate(tmp, 0, tmp.Length);

				Asn1OctetString	encDigest = new DerOctetString(_signature.GenerateSignature());

				parameters = outer.GetBaseParameters(contentType, digAlgId, hash);
				parameters[CmsAttributeTableParameter.Signature] = encDigest.GetOctets().Clone();

				Asn1.Cms.AttributeTable unsigned = (_unsAttr != null)
//					?	_unsAttr.getAttributes(Collections.unmodifiableMap(parameters))
					?	_unsAttr.GetAttributes(parameters)
					:	null;

				Asn1Set unsignedAttr = outer.GetAttributeSet(unsigned);

                X509Certificate cert = this.Certificate;
				SignerIdentifier signerIdentifier;
				if (cert != null)
				{
	                TbsCertificateStructure tbs = TbsCertificateStructure.GetInstance(
						Asn1Object.FromByteArray(cert.GetTbsCertificate()));
	                IssuerAndSerialNumber encSid = new IssuerAndSerialNumber(
						tbs.Issuer, tbs.SerialNumber.Value);

					signerIdentifier = new SignerIdentifier(encSid);
				}
				else
				{
					signerIdentifier = new SignerIdentifier(new DerOctetString(_subjectKeyID));
				}

				return new SignerInfo(signerIdentifier, digAlgId,
					signedAttr, encAlgId, encDigest, unsignedAttr);
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

		public void AddSigner(
			AsymmetricKeyParameter		privateKey,
			X509Certificate				cert,
			string						digestOid,
			CmsAttributeTableGenerator  signedAttrGenerator,
			CmsAttributeTableGenerator  unsignedAttrGenerator)
		{
			ConfigureDigest(digestOid);

            string digestName = Helper.GetDigestAlgName(digestOid);
            string encOID = GetEncOid(privateKey, digestOid);
            string signatureName = digestName + "with" + Helper.GetEncryptionAlgName(encOID);
            ISigner sig = Helper.GetSignatureInstance(signatureName);
            sig.Init(true, new ParametersWithRandom(privateKey, rand));

			_signerInfs.Add(new SignerInf(this, privateKey, cert, digestOid, encOID,
				signedAttrGenerator, unsignedAttrGenerator, sig));
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
			ConfigureDigest(digestOid);

            string digestName = Helper.GetDigestAlgName(digestOid);
            string encOID = GetEncOid(privateKey, digestOid);
            string signatureName = digestName + "with" + Helper.GetEncryptionAlgName(encOID);
            ISigner sig = Helper.GetSignatureInstance(signatureName);
            sig.Init(true, new ParametersWithRandom(privateKey, rand));

			_signerInfs.Add(new SignerInf(this, privateKey, subjectKeyID, digestOid, encOID,
				signedAttrGenerator, unsignedAttrGenerator, sig));
		}

		internal override void AddSignerCallback(
			SignerInformation si)
		{
			// For precalculated signers, just need to register the algorithm, not configure a digest
			RegisterDigestOid(si.DigestAlgOid);
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

			Stream digStream;
			if (encapsulate)
            {
                BerOctetStringGenerator octGen = new BerOctetStringGenerator(
					eiGen.GetRawOutputStream(), 0, true);

				digStream = octGen.GetOctetOutputStream(_bufferSize);

				if (dataOutputStream != null)
				{
					digStream = new TeeOutputStream(dataOutputStream, digStream);
				}
            }
            else
            {
				if (dataOutputStream != null)
				{
					digStream = dataOutputStream;
				}
				else
				{
					digStream = new NullOutputStream();
				}
			}

			foreach (IDigest d in _messageDigests.Values)
			{
                digStream = new DigestStream(digStream, null, d);
            }

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
    
		private class NullOutputStream
            : BaseOutputStream
        {
            public override void WriteByte(
				byte b)
            {
                // do nothing
            }

			public override void Write(
				byte[]	buffer,
				int		offset,
				int		count)
			{
				// do nothing
			}
		}

		private class TeeOutputStream
			: BaseOutputStream
		{
			private readonly Stream s1, s2;

			public TeeOutputStream(Stream dataOutputStream, Stream digStream)
			{
				Debug.Assert(dataOutputStream.CanWrite);
				Debug.Assert(digStream.CanWrite);

				this.s1 = dataOutputStream;
				this.s2 = digStream;
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				s1.Write(buffer, offset, count);
				s2.Write(buffer, offset, count);
			}

			public override void WriteByte(byte b)
			{
				s1.WriteByte(b);
				s2.WriteByte(b);
			}

			public override void Close()
			{
				s1.Close();
				s2.Close();
			}
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
                        throw new IOException("encoding error." + e);
                    }
                    catch (SignatureException e)
                    {
                        throw new IOException("error creating signature." + e);
                    }
                    catch (CertificateEncodingException e)
                    {
                        throw new IOException("error creating sid." + e);
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
