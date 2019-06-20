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

using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Collections;

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	/// <remarks>
	/// Class to hold a single master public key and its subkeys.
	/// <p>
	/// Often PGP keyring files consist of multiple master keys, if you are trying to process
	/// or construct one of these you should use the <c>PgpPublicKeyRingBundle</c> class.
	/// </p>
	/// </remarks>
	public class PgpPublicKeyRing
		: PgpKeyRing
    {
        private readonly IList keys;

		public PgpPublicKeyRing(
            byte[] encoding)
            : this(new MemoryStream(encoding, false))
        {
        }

		internal PgpPublicKeyRing(
            IList pubKeys)
        {
            this.keys = pubKeys;
        }

		public PgpPublicKeyRing(
            Stream inputStream)
        {
			this.keys = Platform.CreateArrayList();

            BcpgInputStream bcpgInput = BcpgInputStream.Wrap(inputStream);

			PacketTag initialTag = bcpgInput.NextPacketTag();
            if (initialTag != PacketTag.PublicKey && initialTag != PacketTag.PublicSubkey)
            {
                throw new IOException("public key ring doesn't start with public key tag: "
					+ "tag 0x" + ((int)initialTag).ToString("X"));
            }

			PublicKeyPacket pubPk = (PublicKeyPacket) bcpgInput.ReadPacket();;
			TrustPacket trustPk = ReadOptionalTrustPacket(bcpgInput);

            // direct signatures and revocations
			IList keySigs = ReadSignaturesAndTrust(bcpgInput);

			IList ids, idTrusts, idSigs;
			ReadUserIDs(bcpgInput, out ids, out idTrusts, out idSigs);

			keys.Add(new PgpPublicKey(pubPk, trustPk, keySigs, ids, idTrusts, idSigs));


			// Read subkeys
			while (bcpgInput.NextPacketTag() == PacketTag.PublicSubkey)
            {
				keys.Add(ReadSubkey(bcpgInput));
            }
        }

		/// <summary>Return the first public key in the ring.</summary>
        public PgpPublicKey GetPublicKey()
        {
            return (PgpPublicKey) keys[0];
        }

		/// <summary>Return the public key referred to by the passed in key ID if it is present.</summary>
        public PgpPublicKey GetPublicKey(
            long keyId)
        {
			foreach (PgpPublicKey k in keys)
			{
				if (keyId == k.KeyId)
                {
                    return k;
                }
            }

			return null;
        }

		/// <summary>Allows enumeration of all the public keys.</summary>
		/// <returns>An <c>IEnumerable</c> of <c>PgpPublicKey</c> objects.</returns>
        public IEnumerable GetPublicKeys()
        {
            return new EnumerableProxy(keys);
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
			if (outStr == null)
				throw new ArgumentNullException("outStr");

			foreach (PgpPublicKey k in keys)
			{
				k.Encode(outStr);
            }
        }

		/// <summary>
		/// Returns a new key ring with the public key passed in either added or
		/// replacing an existing one.
		/// </summary>
		/// <param name="pubRing">The public key ring to be modified.</param>
		/// <param name="pubKey">The public key to be inserted.</param>
		/// <returns>A new <c>PgpPublicKeyRing</c></returns>
        public static PgpPublicKeyRing InsertPublicKey(
            PgpPublicKeyRing	pubRing,
            PgpPublicKey		pubKey)
        {
            IList keys = Platform.CreateArrayList(pubRing.keys);
            bool found = false;
			bool masterFound = false;

			for (int i = 0; i != keys.Count; i++)
            {
                PgpPublicKey key = (PgpPublicKey) keys[i];

				if (key.KeyId == pubKey.KeyId)
                {
                    found = true;
                    keys[i] = pubKey;
                }
				if (key.IsMasterKey)
				{
					masterFound = true;
				}
			}

			if (!found)
            {
				if (pubKey.IsMasterKey)
				{
					if (masterFound)
						throw new ArgumentException("cannot add a master key to a ring that already has one");

					keys.Insert(0, pubKey);
				}
				else
				{
					keys.Add(pubKey);
				}
			}

			return new PgpPublicKeyRing(keys);
        }

		/// <summary>Returns a new key ring with the public key passed in removed from the key ring.</summary>
		/// <param name="pubRing">The public key ring to be modified.</param>
		/// <param name="pubKey">The public key to be removed.</param>
		/// <returns>A new <c>PgpPublicKeyRing</c>, or null if pubKey is not found.</returns>
        public static PgpPublicKeyRing RemovePublicKey(
            PgpPublicKeyRing	pubRing,
            PgpPublicKey		pubKey)
        {
            IList keys = Platform.CreateArrayList(pubRing.keys);
            bool found = false;

			for (int i = 0; i < keys.Count; i++)
            {
                PgpPublicKey key = (PgpPublicKey) keys[i];

				if (key.KeyId == pubKey.KeyId)
                {
                    found = true;
                    keys.RemoveAt(i);
                }
            }

			return found ? new PgpPublicKeyRing(keys) : null;
        }

		internal static PgpPublicKey ReadSubkey(BcpgInputStream bcpgInput)
		{
            PublicKeyPacket	pk = (PublicKeyPacket) bcpgInput.ReadPacket();
			TrustPacket kTrust = ReadOptionalTrustPacket(bcpgInput);

			// PGP 8 actually leaves out the signature.
			IList sigList = ReadSignaturesAndTrust(bcpgInput);

			return new PgpPublicKey(pk, kTrust, sigList);
		}
    }
}
