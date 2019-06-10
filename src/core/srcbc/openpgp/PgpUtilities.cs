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
using System.Text;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	/// <remarks>Basic utility class.</remarks>
    public sealed class PgpUtilities
    {
        private PgpUtilities()
        {
        }

		public static MPInteger[] DsaSigToMpi(
			byte[] encoding)
		{
			DerInteger i1, i2;

			try
			{
				Asn1Sequence s = (Asn1Sequence) Asn1Object.FromByteArray(encoding);

				i1 = (DerInteger) s[0];
				i2 = (DerInteger) s[1];
			}
			catch (IOException e)
			{
				throw new PgpException("exception encoding signature", e);
			}

			return new MPInteger[]{ new MPInteger(i1.Value), new MPInteger(i2.Value) };
		}

		public static MPInteger[] RsaSigToMpi(
			byte[] encoding)
		{
			return new MPInteger[]{ new MPInteger(new BigInteger(1, encoding)) };
		}

		public static string GetDigestName(
            HashAlgorithmTag hashAlgorithm)
        {
            switch (hashAlgorithm)
            {
				case HashAlgorithmTag.Sha1:
					return "SHA1";
				case HashAlgorithmTag.MD2:
					return "MD2";
				case HashAlgorithmTag.MD5:
					return "MD5";
				case HashAlgorithmTag.RipeMD160:
					return "RIPEMD160";
				case HashAlgorithmTag.Sha224:
					return "SHA224";
				case HashAlgorithmTag.Sha256:
					return "SHA256";
				case HashAlgorithmTag.Sha384:
					return "SHA384";
				case HashAlgorithmTag.Sha512:
					return "SHA512";
				default:
					throw new PgpException("unknown hash algorithm tag in GetDigestName: " + hashAlgorithm);
			}
        }

		public static string GetSignatureName(
            PublicKeyAlgorithmTag	keyAlgorithm,
            HashAlgorithmTag		hashAlgorithm)
        {
            string encAlg;
			switch (keyAlgorithm)
            {
				case PublicKeyAlgorithmTag.RsaGeneral:
				case PublicKeyAlgorithmTag.RsaSign:
					encAlg = "RSA";
					break;
				case PublicKeyAlgorithmTag.Dsa:
					encAlg = "DSA";
					break;
				case PublicKeyAlgorithmTag.ElGamalEncrypt: // in some malformed cases.
				case PublicKeyAlgorithmTag.ElGamalGeneral:
					encAlg = "ElGamal";
					break;
				default:
					throw new PgpException("unknown algorithm tag in signature:" + keyAlgorithm);
            }

			return GetDigestName(hashAlgorithm) + "with" + encAlg;
        }

		public static string GetSymmetricCipherName(
            SymmetricKeyAlgorithmTag algorithm)
        {
            switch (algorithm)
            {
				case SymmetricKeyAlgorithmTag.Null:
					return null;
				case SymmetricKeyAlgorithmTag.TripleDes:
					return "DESEDE";
				case SymmetricKeyAlgorithmTag.Idea:
					return "IDEA";
				case SymmetricKeyAlgorithmTag.Cast5:
					return "CAST5";
				case SymmetricKeyAlgorithmTag.Blowfish:
					return "Blowfish";
				case SymmetricKeyAlgorithmTag.Safer:
					return "SAFER";
				case SymmetricKeyAlgorithmTag.Des:
					return "DES";
				case SymmetricKeyAlgorithmTag.Aes128:
					return "AES";
				case SymmetricKeyAlgorithmTag.Aes192:
					return "AES";
				case SymmetricKeyAlgorithmTag.Aes256:
					return "AES";
				case SymmetricKeyAlgorithmTag.Twofish:
					return "Twofish";
				default:
					throw new PgpException("unknown symmetric algorithm: " + algorithm);
            }
        }

		public static int GetKeySize(SymmetricKeyAlgorithmTag algorithm)
        {
            int keySize;
            switch (algorithm)
            {
                case SymmetricKeyAlgorithmTag.Des:
                    keySize = 64;
                    break;
                case SymmetricKeyAlgorithmTag.Idea:
                case SymmetricKeyAlgorithmTag.Cast5:
                case SymmetricKeyAlgorithmTag.Blowfish:
                case SymmetricKeyAlgorithmTag.Safer:
                case SymmetricKeyAlgorithmTag.Aes128:
                    keySize = 128;
                    break;
                case SymmetricKeyAlgorithmTag.TripleDes:
                case SymmetricKeyAlgorithmTag.Aes192:
                    keySize = 192;
                    break;
                case SymmetricKeyAlgorithmTag.Aes256:
                case SymmetricKeyAlgorithmTag.Twofish:
                    keySize = 256;
                    break;
                default:
                    throw new PgpException("unknown symmetric algorithm: " + algorithm);
            }

			return keySize;
        }

		public static KeyParameter MakeKey(
			SymmetricKeyAlgorithmTag	algorithm,
			byte[]						keyBytes)
		{
			string algName = GetSymmetricCipherName(algorithm);

			return ParameterUtilities.CreateKeyParameter(algName, keyBytes);
		}

		public static KeyParameter MakeRandomKey(
            SymmetricKeyAlgorithmTag	algorithm,
            SecureRandom				random)
        {
            int keySize = GetKeySize(algorithm);
            byte[] keyBytes = new byte[(keySize + 7) / 8];
            random.NextBytes(keyBytes);
			return MakeKey(algorithm, keyBytes);
        }

		public static KeyParameter MakeKeyFromPassPhrase(
            SymmetricKeyAlgorithmTag	algorithm,
            S2k							s2k,
            char[]						passPhrase)
        {
			int keySize = GetKeySize(algorithm);
			byte[] pBytes = Strings.ToByteArray(new string(passPhrase));
			byte[] keyBytes = new byte[(keySize + 7) / 8];

			int generatedBytes = 0;
            int loopCount = 0;

			while (generatedBytes < keyBytes.Length)
            {
				IDigest digest;
				if (s2k != null)
                {
					string digestName = GetDigestName(s2k.HashAlgorithm);

                    try
                    {
						digest = DigestUtilities.GetDigest(digestName);
                    }
                    catch (Exception e)
                    {
                        throw new PgpException("can't find S2k digest", e);
                    }

					for (int i = 0; i != loopCount; i++)
                    {
                        digest.Update(0);
                    }

					byte[] iv = s2k.GetIV();

					switch (s2k.Type)
                    {
						case S2k.Simple:
							digest.BlockUpdate(pBytes, 0, pBytes.Length);
							break;
						case S2k.Salted:
							digest.BlockUpdate(iv, 0, iv.Length);
							digest.BlockUpdate(pBytes, 0, pBytes.Length);
							break;
						case S2k.SaltedAndIterated:
							long count = s2k.IterationCount;
							digest.BlockUpdate(iv, 0, iv.Length);
							digest.BlockUpdate(pBytes, 0, pBytes.Length);

							count -= iv.Length + pBytes.Length;

							while (count > 0)
							{
								if (count < iv.Length)
								{
									digest.BlockUpdate(iv, 0, (int)count);
									break;
								}
								else
								{
									digest.BlockUpdate(iv, 0, iv.Length);
									count -= iv.Length;
								}

								if (count < pBytes.Length)
								{
									digest.BlockUpdate(pBytes, 0, (int)count);
									count = 0;
								}
								else
								{
									digest.BlockUpdate(pBytes, 0, pBytes.Length);
									count -= pBytes.Length;
								}
							}
							break;
						default:
							throw new PgpException("unknown S2k type: " + s2k.Type);
                    }
                }
                else
                {
                    try
                    {
                        digest = DigestUtilities.GetDigest("MD5");

						for (int i = 0; i != loopCount; i++)
                        {
                            digest.Update(0);
                        }

						digest.BlockUpdate(pBytes, 0, pBytes.Length);
                    }
                    catch (Exception e)
                    {
                        throw new PgpException("can't find MD5 digest", e);
                    }
                }

				byte[] dig = DigestUtilities.DoFinal(digest);

				if (dig.Length > (keyBytes.Length - generatedBytes))
                {
                    Array.Copy(dig, 0, keyBytes, generatedBytes, keyBytes.Length - generatedBytes);
                }
                else
                {
                    Array.Copy(dig, 0, keyBytes, generatedBytes, dig.Length);
                }

				generatedBytes += dig.Length;

				loopCount++;
            }

			Array.Clear(pBytes, 0, pBytes.Length);

			return MakeKey(algorithm, keyBytes);
        }

		/// <summary>Write out the passed in file as a literal data packet.</summary>
        public static void WriteFileToLiteralData(
            Stream		output,
            char		fileType,
            FileInfo	file)
        {
            PgpLiteralDataGenerator lData = new PgpLiteralDataGenerator();
			Stream pOut = lData.Open(output, fileType, file.Name, file.Length, file.LastWriteTime);
			PipeFileContents(file, pOut, 4096);
        }

		/// <summary>Write out the passed in file as a literal data packet in partial packet format.</summary>
        public static void WriteFileToLiteralData(
            Stream		output,
            char		fileType,
            FileInfo	file,
            byte[]		buffer)
        {
            PgpLiteralDataGenerator lData = new PgpLiteralDataGenerator();
            Stream pOut = lData.Open(output, fileType, file.Name, file.LastWriteTime, buffer);
			PipeFileContents(file, pOut, buffer.Length);
        }

		private static void PipeFileContents(FileInfo file, Stream pOut, int bufSize)
		{
			FileStream inputStream = file.OpenRead();
			byte[] buf = new byte[bufSize];

			int len;
            while ((len = inputStream.Read(buf, 0, buf.Length)) > 0)
            {
                pOut.Write(buf, 0, len);
            }

			pOut.Close();
			inputStream.Close();
		}

		private const int ReadAhead = 60;

		private static bool IsPossiblyBase64(
            int ch)
        {
            return (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z')
                    || (ch >= '0' && ch <= '9') || (ch == '+') || (ch == '/')
                    || (ch == '\r') || (ch == '\n');
        }

		/// <summary>
		/// Return either an ArmoredInputStream or a BcpgInputStream based on whether
		/// the initial characters of the stream are binary PGP encodings or not.
		/// </summary>
        public static Stream GetDecoderStream(
            Stream inputStream)
        {
			// TODO Remove this restriction?
			if (!inputStream.CanSeek)
				throw new ArgumentException("inputStream must be seek-able", "inputStream");

			long markedPos = inputStream.Position;

			int ch = inputStream.ReadByte();
            if ((ch & 0x80) != 0)
            {
                inputStream.Position = markedPos;

				return inputStream;
            }
            else
            {
                if (!IsPossiblyBase64(ch))
                {
                    inputStream.Position = markedPos;

					return new ArmoredInputStream(inputStream);
                }

				byte[]	buf = new byte[ReadAhead];
                int		count = 1;
                int		index = 1;

				buf[0] = (byte)ch;
                while (count != ReadAhead && (ch = inputStream.ReadByte()) >= 0)
                {
                    if (!IsPossiblyBase64(ch))
                    {
                        inputStream.Position = markedPos;

						return new ArmoredInputStream(inputStream);
                    }

					if (ch != '\n' && ch != '\r')
                    {
                        buf[index++] = (byte)ch;
                    }

					count++;
                }

				inputStream.Position = markedPos;

				//
                // nothing but new lines, little else, assume regular armoring
                //
                if (count < 4)
                {
                    return new ArmoredInputStream(inputStream);
                }

				//
                // test our non-blank data
                //
                byte[] firstBlock = new byte[8];
				Array.Copy(buf, 0, firstBlock, 0, firstBlock.Length);
				byte[] decoded = Base64.Decode(firstBlock);

				//
                // it's a base64 PGP block.
                //
				bool hasHeaders = (decoded[0] & 0x80) == 0;

				return new ArmoredInputStream(inputStream, hasHeaders);
            }
        }
    }
}
