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
using Org.BouncyCastle.Math;

namespace Org.BouncyCastle.X509
{
	/// <remarks>Interface for an X.509 Attribute Certificate.</remarks>
	public interface IX509AttributeCertificate
		: IX509Extension
	{
		/// <summary>The version number for the certificate.</summary>
		int Version { get; }

		/// <summary>The serial number for the certificate.</summary>
		BigInteger SerialNumber { get; }

		/// <summary>The UTC DateTime before which the certificate is not valid.</summary>
		DateTime NotBefore { get; }

		/// <summary>The UTC DateTime after which the certificate is not valid.</summary>
		DateTime NotAfter { get; }

		/// <summary>The holder of the certificate.</summary>
		AttributeCertificateHolder Holder { get; }

		/// <summary>The issuer details for the certificate.</summary>
		AttributeCertificateIssuer Issuer { get; }

		/// <summary>Return the attributes contained in the attribute block in the certificate.</summary>
		/// <returns>An array of attributes.</returns>
		X509Attribute[] GetAttributes();

		/// <summary>Return the attributes with the same type as the passed in oid.</summary>
		/// <param name="oid">The object identifier we wish to match.</param>
		/// <returns>An array of matched attributes, null if there is no match.</returns>
		X509Attribute[] GetAttributes(string oid);

		bool[] GetIssuerUniqueID();

		bool IsValidNow { get; }
		bool IsValid(DateTime date);

		void CheckValidity();
		void CheckValidity(DateTime date);

		byte[] GetSignature();

		void Verify(AsymmetricKeyParameter publicKey);

		/// <summary>Return an ASN.1 encoded byte array representing the attribute certificate.</summary>
		/// <returns>An ASN.1 encoded byte array.</returns>
		/// <exception cref="IOException">If the certificate cannot be encoded.</exception>
		byte[] GetEncoded();
	}
}
