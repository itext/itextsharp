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
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.IO;
using Org.BouncyCastle.X509;

namespace Org.BouncyCastle.Cms
{
	/**
	* General class for generating a CMS enveloped-data message stream.
	* <p>
	* A simple example of usage.
	* <pre>
	*      CmsEnvelopedDataStreamGenerator edGen = new CmsEnvelopedDataStreamGenerator();
	*
	*      edGen.AddKeyTransRecipient(cert);
	*
	*      MemoryStream  bOut = new MemoryStream();
	*
	*      Stream out = edGen.Open(
	*                              bOut, CMSEnvelopedDataGenerator.AES128_CBC);*
	*      out.Write(data);
	*
	*      out.Close();
	* </pre>
	* </p>
	*/
	public class CmsEnvelopedDataStreamGenerator
		: CmsEnvelopedGenerator
	{
		private object	_originatorInfo = null;
		private object	_unprotectedAttributes = null;
		private int		_bufferSize;
		private bool	_berEncodeRecipientSet;

		public CmsEnvelopedDataStreamGenerator()
		{
		}

		/// <summary>Constructor allowing specific source of randomness</summary>
		/// <param name="rand">Instance of <c>SecureRandom</c> to use.</param>
		public CmsEnvelopedDataStreamGenerator(
			SecureRandom rand)
			: base(rand)
		{
		}

		/// <summary>Set the underlying string size for encapsulated data.</summary>
		/// <param name="bufferSize">Length of octet strings to buffer the data.</param>
		public void SetBufferSize(
			int bufferSize)
		{
			_bufferSize = bufferSize;
		}

		/// <summary>Use a BER Set to store the recipient information.</summary>
		public void SetBerEncodeRecipients(
			bool berEncodeRecipientSet)
		{
			_berEncodeRecipientSet = berEncodeRecipientSet;
		}

		private DerInteger Version
		{
			get
			{
				int version = (_originatorInfo != null || _unprotectedAttributes != null)
					?	2
					:	0;

				return new DerInteger(version);
			}
		}

		/// <summary>
		/// Generate an enveloped object that contains an CMS Enveloped Data
		/// object using the passed in key generator.
		/// </summary>
		private Stream Open(
			Stream				outStream,
			string				encryptionOid,
			CipherKeyGenerator	keyGen)
		{
			byte[] encKeyBytes = keyGen.GenerateKey();
			KeyParameter encKey = ParameterUtilities.CreateKeyParameter(encryptionOid, encKeyBytes);

			Asn1Encodable asn1Params = GenerateAsn1Parameters(encryptionOid, encKeyBytes);

			ICipherParameters cipherParameters;
			AlgorithmIdentifier encAlgID = GetAlgorithmIdentifier(
				encryptionOid, encKey, asn1Params, out cipherParameters);

			Asn1EncodableVector recipientInfos = new Asn1EncodableVector();

			foreach (RecipientInfoGenerator rig in recipientInfoGenerators)
			{
				try
				{
					recipientInfos.Add(rig.Generate(encKey, rand));
				}
				catch (InvalidKeyException e)
				{
					throw new CmsException("key inappropriate for algorithm.", e);
				}
				catch (GeneralSecurityException e)
				{
					throw new CmsException("error making encrypted content.", e);
				}
			}

			return Open(outStream, encAlgID, cipherParameters, recipientInfos);
		}

		private Stream Open(
			Stream				outStream,
			AlgorithmIdentifier	encAlgID,
			ICipherParameters	cipherParameters,
			Asn1EncodableVector	recipientInfos)
		{
			try
			{
				//
				// ContentInfo
				//
				BerSequenceGenerator cGen = new BerSequenceGenerator(outStream);

				cGen.AddObject(CmsObjectIdentifiers.EnvelopedData);

				//
				// Encrypted Data
				//
				BerSequenceGenerator envGen = new BerSequenceGenerator(
					cGen.GetRawOutputStream(), 0, true);

				envGen.AddObject(this.Version);

				Stream envRaw = envGen.GetRawOutputStream();
				Asn1Generator recipGen = _berEncodeRecipientSet
					?	(Asn1Generator) new BerSetGenerator(envRaw)
					:	new DerSetGenerator(envRaw);

				foreach (Asn1Encodable ae in recipientInfos)
				{
					recipGen.AddObject(ae);
				}

				recipGen.Close();

				BerSequenceGenerator eiGen = new BerSequenceGenerator(envRaw);
				eiGen.AddObject(CmsObjectIdentifiers.Data);
				eiGen.AddObject(encAlgID);

				Stream octetOutputStream = CmsUtilities.CreateBerOctetOutputStream(
					eiGen.GetRawOutputStream(), 0, false, _bufferSize);

				IBufferedCipher cipher = CipherUtilities.GetCipher(encAlgID.ObjectID);
				cipher.Init(true, new ParametersWithRandom(cipherParameters, rand));
				CipherStream cOut = new CipherStream(octetOutputStream, null, cipher);

				return new CmsEnvelopedDataOutputStream(this, cOut, cGen, envGen, eiGen);
			}
			catch (SecurityUtilityException e)
			{
				throw new CmsException("couldn't create cipher.", e);
			}
			catch (InvalidKeyException e)
			{
				throw new CmsException("key invalid in message.", e);
			}
			catch (IOException e)
			{
				throw new CmsException("exception decoding algorithm parameters.", e);
			}
		}

		/**
		* generate an enveloped object that contains an CMS Enveloped Data object
		* @throws IOException
		*/
		public Stream Open(
			Stream	outStream,
			string	encryptionOid)
		{
			CipherKeyGenerator keyGen = GeneratorUtilities.GetKeyGenerator(encryptionOid);

			keyGen.Init(new KeyGenerationParameters(rand, keyGen.DefaultStrength));

			return Open(outStream, encryptionOid, keyGen);
		}

		/**
		* generate an enveloped object that contains an CMS Enveloped Data object
		* @throws IOException
		*/
		public Stream Open(
			Stream	outStream,
			string	encryptionOid,
			int		keySize)
		{
			CipherKeyGenerator keyGen = GeneratorUtilities.GetKeyGenerator(encryptionOid);

			keyGen.Init(new KeyGenerationParameters(rand, keySize));

			return Open(outStream, encryptionOid, keyGen);
		}

		private class CmsEnvelopedDataOutputStream
			: BaseOutputStream
		{
            private readonly CmsEnvelopedGenerator _outer;

			private readonly CipherStream			_out;
			private readonly BerSequenceGenerator	_cGen;
			private readonly BerSequenceGenerator	_envGen;
			private readonly BerSequenceGenerator	_eiGen;

			public CmsEnvelopedDataOutputStream(
				CmsEnvelopedGenerator	outer,
				CipherStream			outStream,
				BerSequenceGenerator	cGen,
				BerSequenceGenerator	envGen,
				BerSequenceGenerator	eiGen)
			{
				_outer = outer;
				_out = outStream;
				_cGen = cGen;
				_envGen = envGen;
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

				// TODO Parent context(s) should really be be closed explicitly

				_eiGen.Close();

                if (_outer.unprotectedAttributeGenerator != null)
                {
                    Asn1.Cms.AttributeTable attrTable = _outer.unprotectedAttributeGenerator.GetAttributes(Platform.CreateHashtable());

                    Asn1Set unprotectedAttrs = new BerSet(attrTable.ToAsn1EncodableVector());

                    _envGen.AddObject(new DerTaggedObject(false, 1, unprotectedAttrs));
                }

				_envGen.Close();
				_cGen.Close();
				base.Close();
			}
		}
	}
}
