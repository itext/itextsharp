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

namespace Org.BouncyCastle.Bcpg.OpenPgp
{
	/// <remarks>
	/// General class to handle JCA key pairs and convert them into OpenPGP ones.
	/// <p>
	/// A word for the unwary, the KeyId for an OpenPGP public key is calculated from
	/// a hash that includes the time of creation, if you pass a different date to the
	/// constructor below with the same public private key pair the KeyIs will not be the
	/// same as for previous generations of the key, so ideally you only want to do
	/// this once.
	/// </p>
	/// </remarks>
    public class PgpKeyPair
    {
        private readonly PgpPublicKey	pub;
        private readonly PgpPrivateKey	priv;

		public PgpKeyPair(
            PublicKeyAlgorithmTag	algorithm,
            AsymmetricCipherKeyPair	keyPair,
            DateTime				time)
			: this(algorithm, keyPair.Public, keyPair.Private, time)
        {
        }

		public PgpKeyPair(
            PublicKeyAlgorithmTag	algorithm,
            AsymmetricKeyParameter	pubKey,
            AsymmetricKeyParameter	privKey,
            DateTime				time)
        {
            this.pub = new PgpPublicKey(algorithm, pubKey, time);
			this.priv = new PgpPrivateKey(privKey, pub.KeyId);
        }

		/// <summary>Create a key pair from a PgpPrivateKey and a PgpPublicKey.</summary>
		/// <param name="pub">The public key.</param>
		/// <param name="priv">The private key.</param>
        public PgpKeyPair(
            PgpPublicKey	pub,
            PgpPrivateKey	priv)
        {
            this.pub = pub;
            this.priv = priv;
        }

		/// <summary>The keyId associated with this key pair.</summary>
        public long KeyId
        {
            get { return pub.KeyId; }
        }

		public PgpPublicKey PublicKey
        {
			get { return pub; }
        }

		public PgpPrivateKey PrivateKey
        {
			get { return priv; }
        }
    }
}
