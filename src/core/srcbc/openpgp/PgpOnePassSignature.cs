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
using Org.BouncyCastle.Security;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	/// <remarks>A one pass signature object.</remarks>
    public class PgpOnePassSignature
    {
        private OnePassSignaturePacket sigPack;
        private int signatureType;
		private ISigner sig;
		private byte lastb;

		internal PgpOnePassSignature(
            BcpgInputStream bcpgInput)
            : this((OnePassSignaturePacket) bcpgInput.ReadPacket())
        {
        }

		internal PgpOnePassSignature(
            OnePassSignaturePacket sigPack)
        {
            this.sigPack = sigPack;
            this.signatureType = sigPack.SignatureType;
        }

		/// <summary>Initialise the signature object for verification.</summary>
        public void InitVerify(
            PgpPublicKey pubKey)
        {
			lastb = 0;

			try
			{
				sig = SignerUtilities.GetSigner(
					PgpUtilities.GetSignatureName(sigPack.KeyAlgorithm, sigPack.HashAlgorithm));
			}
			catch (Exception e)
			{
				throw new PgpException("can't set up signature object.",  e);
			}

			try
            {
                sig.Init(false, pubKey.GetKey());
            }
			catch (InvalidKeyException e)
            {
                throw new PgpException("invalid key.", e);
            }
        }

		public void Update(
            byte b)
        {
			if (signatureType == PgpSignature.CanonicalTextDocument)
			{
				doCanonicalUpdateByte(b);
			}
			else
			{
				sig.Update(b);
			}
        }

		private void doCanonicalUpdateByte(
			byte b)
		{
			if (b == '\r')
			{
				doUpdateCRLF();
			}
			else if (b == '\n')
			{
				if (lastb != '\r')
				{
					doUpdateCRLF();
				}
			}
			else
			{
				sig.Update(b);
			}

			lastb = b;
		}

		private void doUpdateCRLF()
		{
			sig.Update((byte)'\r');
			sig.Update((byte)'\n');
		}

		public void Update(
            byte[] bytes)
        {
            if (signatureType == PgpSignature.CanonicalTextDocument)
            {
                for (int i = 0; i != bytes.Length; i++)
                {
                    doCanonicalUpdateByte(bytes[i]);
                }
            }
            else
            {
                sig.BlockUpdate(bytes, 0, bytes.Length);
            }
        }

        public void Update(
            byte[]  bytes,
            int     off,
            int     length)
        {
            if (signatureType == PgpSignature.CanonicalTextDocument)
            {
                int finish = off + length;

                for (int i = off; i != finish; i++)
                {
                    doCanonicalUpdateByte(bytes[i]);
                }
            }
            else
            {
                sig.BlockUpdate(bytes, off, length);
            }
        }

		/// <summary>Verify the calculated signature against the passed in PgpSignature.</summary>
        public bool Verify(
            PgpSignature pgpSig)
        {
            byte[] trailer = pgpSig.GetSignatureTrailer();

			sig.BlockUpdate(trailer, 0, trailer.Length);

			return sig.VerifySignature(pgpSig.GetSignature());
        }

        public long KeyId
        {
			get { return sigPack.KeyId; }
        }

		public int SignatureType
        {
            get { return sigPack.SignatureType; }
        }

		public HashAlgorithmTag HashAlgorithm
		{
			get { return sigPack.HashAlgorithm; }
		}

		public PublicKeyAlgorithmTag KeyAlgorithm
		{
			get { return sigPack.KeyAlgorithm; }
		}

		public byte[] GetEncoded()
        {
            MemoryStream bOut = new MemoryStream();

            Encode(bOut);

            return bOut.ToArray();
        }

		public void Encode(
            Stream outStr)
        {
            BcpgOutputStream.Wrap(outStr).WritePacket(sigPack);
        }
    }
}
