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
using System.IO;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	/// <remarks>A password based encryption object.</remarks>
    public class PgpPbeEncryptedData
        : PgpEncryptedData
    {
        private readonly SymmetricKeyEncSessionPacket keyData;

		internal PgpPbeEncryptedData(
			SymmetricKeyEncSessionPacket	keyData,
			InputStreamPacket				encData)
			: base(encData)
		{
			this.keyData = keyData;
		}

		/// <summary>Return the raw input stream for the data stream.</summary>
		public override Stream GetInputStream()
		{
			return encData.GetInputStream();
		}

		/// <summary>Return the decrypted input stream, using the passed in passphrase.</summary>
        public Stream GetDataStream(
            char[] passPhrase)
        {
			try
			{
				SymmetricKeyAlgorithmTag keyAlgorithm = keyData.EncAlgorithm;

				KeyParameter key = PgpUtilities.MakeKeyFromPassPhrase(
					keyAlgorithm, keyData.S2k, passPhrase);


				byte[] secKeyData = keyData.GetSecKeyData();
				if (secKeyData != null && secKeyData.Length > 0)
				{
					IBufferedCipher keyCipher = CipherUtilities.GetCipher(
						PgpUtilities.GetSymmetricCipherName(keyAlgorithm) + "/CFB/NoPadding");

					keyCipher.Init(false,
						new ParametersWithIV(key, new byte[keyCipher.GetBlockSize()]));

					byte[] keyBytes = keyCipher.DoFinal(secKeyData);

					keyAlgorithm = (SymmetricKeyAlgorithmTag) keyBytes[0];

					key = ParameterUtilities.CreateKeyParameter(
						PgpUtilities.GetSymmetricCipherName(keyAlgorithm),
						keyBytes, 1, keyBytes.Length - 1);
				}


				IBufferedCipher c = CreateStreamCipher(keyAlgorithm);

				byte[] iv = new byte[c.GetBlockSize()];

				c.Init(false, new ParametersWithIV(key, iv));

				encStream = BcpgInputStream.Wrap(new CipherStream(encData.GetInputStream(), c, null));

				if (encData is SymmetricEncIntegrityPacket)
				{
					truncStream = new TruncatedStream(encStream);

					string digestName = PgpUtilities.GetDigestName(HashAlgorithmTag.Sha1);
					IDigest digest = DigestUtilities.GetDigest(digestName);

					encStream = new DigestStream(truncStream, digest, null);
				}

				if (Streams.ReadFully(encStream, iv, 0, iv.Length) < iv.Length)
					throw new EndOfStreamException("unexpected end of stream.");

				int v1 = encStream.ReadByte();
				int v2 = encStream.ReadByte();

				if (v1 < 0 || v2 < 0)
					throw new EndOfStreamException("unexpected end of stream.");


				// Note: the oracle attack on the "quick check" bytes is not deemed
				// a security risk for PBE (see PgpPublicKeyEncryptedData)

				bool repeatCheckPassed =
						iv[iv.Length - 2] == (byte)v1
					&&	iv[iv.Length - 1] == (byte)v2;

				// Note: some versions of PGP appear to produce 0 for the extra
				// bytes rather than repeating the two previous bytes
				bool zeroesCheckPassed =
						v1 == 0
					&&	v2 == 0;

				if (!repeatCheckPassed && !zeroesCheckPassed)
				{
					throw new PgpDataValidationException("quick check failed.");
				}


				return encStream;
			}
			catch (PgpException e)
			{
				throw e;
			}
			catch (Exception e)
			{
				throw new PgpException("Exception creating cipher", e);
			}
		}

		private IBufferedCipher CreateStreamCipher(
			SymmetricKeyAlgorithmTag keyAlgorithm)
		{
			string mode = (encData is SymmetricEncIntegrityPacket)
				? "CFB"
				: "OpenPGPCFB";

			string cName = PgpUtilities.GetSymmetricCipherName(keyAlgorithm)
				+ "/" + mode + "/NoPadding";

			return CipherUtilities.GetCipher(cName);
		}
	}
}
