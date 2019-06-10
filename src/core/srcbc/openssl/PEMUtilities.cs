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

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.OpenSsl
{
	internal sealed class PemUtilities
	{
		private enum PemBaseAlg { AES_128, AES_192, AES_256, BF, DES, DES_EDE, DES_EDE3, RC2, RC2_40, RC2_64 };
		private enum PemMode { CBC, CFB, ECB, OFB };

		static PemUtilities()
		{
			// Signal to obfuscation tools not to change enum constants
			((PemBaseAlg)Enums.GetArbitraryValue(typeof(PemBaseAlg))).ToString();
			((PemMode)Enums.GetArbitraryValue(typeof(PemMode))).ToString();
		}

		private static void ParseDekAlgName(
			string			dekAlgName,
			out PemBaseAlg	baseAlg,
			out PemMode		mode)
		{
			try
			{
				mode = PemMode.ECB;

				if (dekAlgName == "DES-EDE" || dekAlgName == "DES-EDE3")
				{
					baseAlg = (PemBaseAlg)Enums.GetEnumValue(typeof(PemBaseAlg), dekAlgName);
					return;
				}

				int pos = dekAlgName.LastIndexOf('-');
				if (pos >= 0)
				{
					baseAlg = (PemBaseAlg)Enums.GetEnumValue(typeof(PemBaseAlg), dekAlgName.Substring(0, pos));
					mode = (PemMode)Enums.GetEnumValue(typeof(PemMode), dekAlgName.Substring(pos + 1));
					return;
				}
			}
			catch (ArgumentException)
			{
			}

			throw new EncryptionException("Unknown DEK algorithm: " + dekAlgName);
		}

		internal static byte[] Crypt(
			bool	encrypt,
			byte[]	bytes,
			char[]	password,
			string	dekAlgName,
			byte[]	iv)
		{
			PemBaseAlg baseAlg;
			PemMode mode;
			ParseDekAlgName(dekAlgName, out baseAlg, out mode);

			string padding;
			switch (mode)
			{
				case PemMode.CBC:
				case PemMode.ECB:
					padding = "PKCS5Padding";
					break;
				case PemMode.CFB:
				case PemMode.OFB:
					padding = "NoPadding";
					break;
				default:
					throw new EncryptionException("Unknown DEK algorithm: " + dekAlgName);
			}

			string algorithm;

			byte[] salt = iv;
			switch (baseAlg)
			{
				case PemBaseAlg.AES_128:
				case PemBaseAlg.AES_192:
				case PemBaseAlg.AES_256:
					algorithm = "AES";
					if (salt.Length > 8)
					{
						salt = new byte[8];
						Array.Copy(iv, 0, salt, 0, salt.Length);
					}
					break;
				case PemBaseAlg.BF:
					algorithm = "BLOWFISH";
					break;
				case PemBaseAlg.DES:
					algorithm = "DES";
					break;
				case PemBaseAlg.DES_EDE:
				case PemBaseAlg.DES_EDE3:
					algorithm = "DESede";
					break;
				case PemBaseAlg.RC2:
				case PemBaseAlg.RC2_40:
				case PemBaseAlg.RC2_64:
					algorithm = "RC2";
					break;
				default:
					throw new EncryptionException("Unknown DEK algorithm: " + dekAlgName);
			}

			string cipherName = algorithm + "/" + mode + "/" + padding;
			IBufferedCipher cipher = CipherUtilities.GetCipher(cipherName);

			ICipherParameters cParams = GetCipherParameters(password, baseAlg, salt);

			if (mode != PemMode.ECB)
			{
				cParams = new ParametersWithIV(cParams, iv);
			}

			cipher.Init(encrypt, cParams);

			return cipher.DoFinal(bytes);
		}

		private static ICipherParameters GetCipherParameters(
			char[]		password,
			PemBaseAlg	baseAlg,
			byte[]		salt)
		{
			string algorithm;
			int keyBits;
			switch (baseAlg)
			{
				case PemBaseAlg.AES_128:		keyBits = 128;	algorithm = "AES128";	break;
				case PemBaseAlg.AES_192:		keyBits = 192;	algorithm = "AES192";	break;
				case PemBaseAlg.AES_256:		keyBits = 256;	algorithm = "AES256";	break;
				case PemBaseAlg.BF:				keyBits = 128;	algorithm = "BLOWFISH";	break;
				case PemBaseAlg.DES:			keyBits = 64;	algorithm = "DES";		break;
				case PemBaseAlg.DES_EDE:		keyBits = 128;	algorithm = "DESEDE";	break;
				case PemBaseAlg.DES_EDE3:		keyBits = 192;	algorithm = "DESEDE3";	break;
				case PemBaseAlg.RC2:			keyBits = 128;	algorithm = "RC2";		break;
				case PemBaseAlg.RC2_40:			keyBits = 40;	algorithm = "RC2";		break;
				case PemBaseAlg.RC2_64:			keyBits = 64;	algorithm = "RC2";		break;
				default:
					return null;
			}

			OpenSslPbeParametersGenerator pGen = new OpenSslPbeParametersGenerator();

			pGen.Init(PbeParametersGenerator.Pkcs5PasswordToBytes(password), salt);

			return pGen.GenerateDerivedParameters(algorithm, keyBits);
		}
	}
}
