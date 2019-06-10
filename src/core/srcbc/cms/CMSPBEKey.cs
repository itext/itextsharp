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

using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;

//import javax.crypto.interfaces.PBEKey;

namespace Org.BouncyCastle.Cms
{
	public abstract class CmsPbeKey
		// TODO Create an equivalent interface somewhere?
		//	: PBEKey
		: ICipherParameters
	{
		internal readonly char[]	password;
		internal readonly byte[]	salt;
		internal readonly int		iterationCount;

		[Obsolete("Use version taking 'char[]' instead")]
		public CmsPbeKey(
			string	password,
			byte[]	salt,
			int		iterationCount)
			: this(password.ToCharArray(), salt, iterationCount)
		{
		}

		[Obsolete("Use version taking 'char[]' instead")]
		public CmsPbeKey(
			string				password,
			AlgorithmIdentifier keyDerivationAlgorithm)
			: this(password.ToCharArray(), keyDerivationAlgorithm)
		{
		}
		
		public CmsPbeKey(
			char[]	password,
			byte[]	salt,
			int		iterationCount)
		{
			this.password = (char[])password.Clone();
			this.salt = Arrays.Clone(salt);
			this.iterationCount = iterationCount;
		}

		public CmsPbeKey(
			char[]				password,
			AlgorithmIdentifier keyDerivationAlgorithm)
		{
			if (!keyDerivationAlgorithm.ObjectID.Equals(PkcsObjectIdentifiers.IdPbkdf2))
				throw new ArgumentException("Unsupported key derivation algorithm: "
					+ keyDerivationAlgorithm.ObjectID);

			Pbkdf2Params kdfParams = Pbkdf2Params.GetInstance(
				keyDerivationAlgorithm.Parameters.ToAsn1Object());

			this.password = (char[])password.Clone();
			this.salt = kdfParams.GetSalt();
			this.iterationCount = kdfParams.IterationCount.IntValue;
		}

		~CmsPbeKey()
		{
			Array.Clear(this.password, 0, this.password.Length);
		}

		[Obsolete("Will be removed")]
		public string Password
		{
			get { return new string(password); }
		}

		public byte[] Salt
		{
			get { return Arrays.Clone(salt); }
		}

		[Obsolete("Use 'Salt' property instead")]
		public byte[] GetSalt()
		{
			return Salt;
		}

		public int IterationCount
		{
			get { return iterationCount; }
		}

		public string Algorithm
		{
			get { return "PKCS5S2"; }
		}

		public string Format
		{
			get { return "RAW"; }
		}

		public byte[] GetEncoded()
		{
			return null;
		}

		internal abstract KeyParameter GetEncoded(string algorithmOid);
	}
}
