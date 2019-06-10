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
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Tls
{
    public class TlsStreamCipher : TlsCipher
    {
        protected TlsClientContext context;

        protected IStreamCipher encryptCipher;
        protected IStreamCipher decryptCipher;

        protected TlsMac writeMac;
        protected TlsMac readMac;

        public TlsStreamCipher(TlsClientContext context, IStreamCipher encryptCipher,
			IStreamCipher decryptCipher, IDigest writeDigest, IDigest readDigest, int cipherKeySize)
		{
			this.context = context;
			this.encryptCipher = encryptCipher;
			this.decryptCipher = decryptCipher;

            int prfSize = (2 * cipherKeySize) + writeDigest.GetDigestSize()
                + readDigest.GetDigestSize();

			SecurityParameters securityParameters = context.SecurityParameters;

			byte[] keyBlock = TlsUtilities.PRF(securityParameters.masterSecret, "key expansion",
				TlsUtilities.Concat(securityParameters.serverRandom, securityParameters.clientRandom),
				prfSize);

			int offset = 0;

			// Init MACs
			writeMac = CreateTlsMac(writeDigest, keyBlock, ref offset);
			readMac = CreateTlsMac(readDigest, keyBlock, ref offset);

			// Build keys
			KeyParameter encryptKey = CreateKeyParameter(keyBlock, ref offset, cipherKeySize);
			KeyParameter decryptKey = CreateKeyParameter(keyBlock, ref offset, cipherKeySize);

			if (offset != prfSize)
                throw new TlsFatalAlert(AlertDescription.internal_error);

            // Init Ciphers
            encryptCipher.Init(true, encryptKey);
            decryptCipher.Init(false, decryptKey);
		}

        public byte[] EncodePlaintext(ContentType type, byte[] plaintext, int offset, int len)
        {
            byte[] mac = writeMac.CalculateMac(type, plaintext, offset, len);
            int size = len + mac.Length;

            byte[] outbuf = new byte[size];

            encryptCipher.ProcessBytes(plaintext, offset, len, outbuf, 0);
            encryptCipher.ProcessBytes(mac, 0, mac.Length, outbuf, len);

            return outbuf;
        }

        public byte[] DecodeCiphertext(ContentType type, byte[] ciphertext, int offset, int len)
        {
            byte[] deciphered = new byte[len];
            decryptCipher.ProcessBytes(ciphertext, offset, len, deciphered, 0);

            int plaintextSize = deciphered.Length - readMac.Size;
            byte[] plainText = CopyData(deciphered, 0, plaintextSize);

            byte[] receivedMac = CopyData(deciphered, plaintextSize, readMac.Size);
            byte[] computedMac = readMac.CalculateMac(type, plainText, 0, plainText.Length);

            if (!Arrays.ConstantTimeAreEqual(receivedMac, computedMac))
            {
                throw new TlsFatalAlert(AlertDescription.bad_record_mac);
            }

            return plainText;
        }

        protected virtual TlsMac CreateTlsMac(IDigest digest, byte[] buf, ref int off)
        {
            int len = digest.GetDigestSize();
            TlsMac mac = new TlsMac(digest, buf, off, len);
            off += len;
            return mac;
        }

        protected virtual KeyParameter CreateKeyParameter(byte[] buf, ref int off, int len)
        {
            KeyParameter key = new KeyParameter(buf, off, len);
            off += len;
            return key;
        }

        protected virtual byte[] CopyData(byte[] text, int offset, int len)
        {
            byte[] result = new byte[len];
            Array.Copy(text, offset, result, 0, len);
            return result;
        }
    }
}
