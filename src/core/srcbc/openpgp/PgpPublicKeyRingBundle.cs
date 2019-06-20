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
	/// Often a PGP key ring file is made up of a succession of master/sub-key key rings.
	/// If you want to read an entire public key file in one hit this is the class for you.
	/// </remarks>
    public class PgpPublicKeyRingBundle
    {
        private readonly IDictionary pubRings;
        private readonly IList order;

		private PgpPublicKeyRingBundle(
            IDictionary	pubRings,
            IList       order)
        {
            this.pubRings = pubRings;
            this.order = order;
        }

		public PgpPublicKeyRingBundle(
            byte[] encoding)
            : this(new MemoryStream(encoding, false))
        {
        }

		/// <summary>Build a PgpPublicKeyRingBundle from the passed in input stream.</summary>
		/// <param name="inputStream">Input stream containing data.</param>
		/// <exception cref="IOException">If a problem parsing the stream occurs.</exception>
		/// <exception cref="PgpException">If an object is encountered which isn't a PgpPublicKeyRing.</exception>
		public PgpPublicKeyRingBundle(
            Stream inputStream)
			: this(new PgpObjectFactory(inputStream).AllPgpObjects())
		{
        }

		public PgpPublicKeyRingBundle(
            IEnumerable e)
        {
			this.pubRings = Platform.CreateHashtable();
			this.order = Platform.CreateArrayList();

			foreach (object obj in e)
            {
				PgpPublicKeyRing pgpPub = obj as PgpPublicKeyRing;

				if (pgpPub == null)
				{
					throw new PgpException(obj.GetType().FullName + " found where PgpPublicKeyRing expected");
				}

				long key = pgpPub.GetPublicKey().KeyId;
                pubRings.Add(key, pgpPub);
				order.Add(key);
            }
        }

		[Obsolete("Use 'Count' property instead")]
		public int Size
		{
			get { return order.Count; }
		}

		/// <summary>Return the number of key rings in this collection.</summary>
        public int Count
        {
			get { return order.Count; }
        }

		/// <summary>Allow enumeration of the public key rings making up this collection.</summary>
        public IEnumerable GetKeyRings()
        {
			return new EnumerableProxy(pubRings.Values);
        }

		/// <summary>Allow enumeration of the key rings associated with the passed in userId.</summary>
		/// <param name="userId">The user ID to be matched.</param>
		/// <returns>An <c>IEnumerable</c> of key rings which matched (possibly none).</returns>
		public IEnumerable GetKeyRings(
			string userId)
		{
			return GetKeyRings(userId, false, false);
		}

		/// <summary>Allow enumeration of the key rings associated with the passed in userId.</summary>
		/// <param name="userId">The user ID to be matched.</param>
		/// <param name="matchPartial">If true, userId need only be a substring of an actual ID string to match.</param>
		/// <returns>An <c>IEnumerable</c> of key rings which matched (possibly none).</returns>
        public IEnumerable GetKeyRings(
            string	userId,
            bool	matchPartial)
        {
			return GetKeyRings(userId, matchPartial, false);
        }

		/// <summary>Allow enumeration of the key rings associated with the passed in userId.</summary>
		/// <param name="userId">The user ID to be matched.</param>
		/// <param name="matchPartial">If true, userId need only be a substring of an actual ID string to match.</param>
		/// <param name="ignoreCase">If true, case is ignored in user ID comparisons.</param>
		/// <returns>An <c>IEnumerable</c> of key rings which matched (possibly none).</returns>
		public IEnumerable GetKeyRings(
			string	userId,
			bool	matchPartial,
			bool	ignoreCase)
		{
			IList rings = Platform.CreateArrayList();

			if (ignoreCase)
			{
                userId = Platform.ToLowerInvariant(userId);
			}

			foreach (PgpPublicKeyRing pubRing in GetKeyRings())
			{
				foreach (string nextUserID in pubRing.GetPublicKey().GetUserIds())
				{
					string next = nextUserID;
					if (ignoreCase)
					{
                        next = Platform.ToLowerInvariant(next);
                    }

					if (matchPartial)
					{
						if (next.IndexOf(userId) > -1)
						{
							rings.Add(pubRing);
						}
					}
					else
					{
						if (next.Equals(userId))
						{
							rings.Add(pubRing);
						}
					}
				}
			}

			return new EnumerableProxy(rings);
		}

		/// <summary>Return the PGP public key associated with the given key id.</summary>
		/// <param name="keyId">The ID of the public key to return.</param>
        public PgpPublicKey GetPublicKey(
            long keyId)
        {
            foreach (PgpPublicKeyRing pubRing in GetKeyRings())
            {
                PgpPublicKey pub = pubRing.GetPublicKey(keyId);

				if (pub != null)
                {
                    return pub;
                }
            }

			return null;
        }

		/// <summary>Return the public key ring which contains the key referred to by keyId</summary>
		/// <param name="keyId">key ID to match against</param>
        public PgpPublicKeyRing GetPublicKeyRing(
            long keyId)
        {
            if (pubRings.Contains(keyId))
            {
                return (PgpPublicKeyRing)pubRings[keyId];
            }

			foreach (PgpPublicKeyRing pubRing in GetKeyRings())
            {
                PgpPublicKey pub = pubRing.GetPublicKey(keyId);

                if (pub != null)
                {
                    return pubRing;
                }
            }

			return null;
        }

		/// <summary>
		/// Return true if a key matching the passed in key ID is present, false otherwise.
		/// </summary>
		/// <param name="keyID">key ID to look for.</param>
		public bool Contains(
			long keyID)
		{
			return GetPublicKey(keyID) != null;
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
			BcpgOutputStream bcpgOut = BcpgOutputStream.Wrap(outStr);

			foreach (long key in order)
            {
                PgpPublicKeyRing sec = (PgpPublicKeyRing) pubRings[key];

				sec.Encode(bcpgOut);
            }
        }

		/// <summary>
		/// Return a new bundle containing the contents of the passed in bundle and
		/// the passed in public key ring.
		/// </summary>
		/// <param name="bundle">The <c>PgpPublicKeyRingBundle</c> the key ring is to be added to.</param>
		/// <param name="publicKeyRing">The key ring to be added.</param>
		/// <returns>A new <c>PgpPublicKeyRingBundle</c> merging the current one with the passed in key ring.</returns>
		/// <exception cref="ArgumentException">If the keyId for the passed in key ring is already present.</exception>
        public static PgpPublicKeyRingBundle AddPublicKeyRing(
            PgpPublicKeyRingBundle  bundle,
            PgpPublicKeyRing        publicKeyRing)
        {
            long key = publicKeyRing.GetPublicKey().KeyId;

			if (bundle.pubRings.Contains(key))
            {
                throw new ArgumentException("Bundle already contains a key with a keyId for the passed in ring.");
            }

			IDictionary newPubRings = Platform.CreateHashtable(bundle.pubRings);
            IList newOrder = Platform.CreateArrayList(bundle.order);

			newPubRings[key] = publicKeyRing;

			newOrder.Add(key);

			return new PgpPublicKeyRingBundle(newPubRings, newOrder);
        }

		/// <summary>
		/// Return a new bundle containing the contents of the passed in bundle with
		/// the passed in public key ring removed.
		/// </summary>
		/// <param name="bundle">The <c>PgpPublicKeyRingBundle</c> the key ring is to be removed from.</param>
		/// <param name="publicKeyRing">The key ring to be removed.</param>
		/// <returns>A new <c>PgpPublicKeyRingBundle</c> not containing the passed in key ring.</returns>
		/// <exception cref="ArgumentException">If the keyId for the passed in key ring is not present.</exception>
        public static PgpPublicKeyRingBundle RemovePublicKeyRing(
            PgpPublicKeyRingBundle	bundle,
            PgpPublicKeyRing		publicKeyRing)
        {
            long key = publicKeyRing.GetPublicKey().KeyId;

			if (!bundle.pubRings.Contains(key))
            {
                throw new ArgumentException("Bundle does not contain a key with a keyId for the passed in ring.");
            }

			IDictionary newPubRings = Platform.CreateHashtable(bundle.pubRings);
            IList newOrder = Platform.CreateArrayList(bundle.order);

			newPubRings.Remove(key);
			newOrder.Remove(key);

			return new PgpPublicKeyRingBundle(newPubRings, newOrder);
        }
    }
}
